using Gens.Simulation.Actors;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Interactions;

/// <summary>Emitted when a <see cref="Scheme"/> leaves <see cref="SchemeStatus.InProgress"/>
/// (<c>gens-characters-design.md</c> §10.5). Private to the two participants, like <see
/// cref="SchemeInitiatedEvent"/> — surfacing a discovered Scheme to anyone beyond them (a rumor, a
/// Chronicle entry) is a future Events/Chronicle consumer's job (§10's own closing paragraph names
/// those as forward hooks), not this engine's.</summary>
public sealed record SchemeResolvedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Scheme> SchemeId,
    RuntimeId<Character> InitiatorCharacterId,
    RuntimeId<Character> TargetCharacterId,
    SchemeStatus Status) : IDomainEvent
{
    public string Type => "interactions.schemeResolved";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { InitiatorCharacterId.ToTaggedString(), TargetCharacterId.ToTaggedString() };
    public string? CausationId => null;
    public Visibility Visibility => Visibility.Private(InitiatorCharacterId.ToTaggedString(), TargetCharacterId.ToTaggedString());
}

/// <summary>
/// The monthly Progress/Discovery/Counter-play/Resolution tick for every <see
/// cref="SchemeStatus.InProgress"/> <see cref="Scheme"/> (Phase 10 item 6; <c>gens-characters-design.md</c>
/// §10.2-§10.5). Each processed month:
///
/// <list type="number">
/// <item><b>Progress</b> (§10.2) advances by <see cref="SchemeProgressCatalog.BaseProgressPerMonthPercent"/>
/// plus a bonus scaled by the initiator's <see cref="CoreAttributes.Intrigue"/>.</item>
/// <item><b>Discovery risk</b> (§10.3) rises by <see
/// cref="SchemeProgressCatalog.BaseDiscoveryRiskPerMonthPercent"/> plus a bonus scaled by the target's
/// own Intrigue (standing in for that section's Perceptive/Oblivious trait check, which has no numeric
/// score wired up yet).</item>
/// <item>If risk reaches <see cref="SchemeProgressCatalog.DiscoveryRiskThresholdPercent"/> first, <b>
/// counter-play</b> (§10.4) resolves immediately: an Intrigue-weighted roll decides <see
/// cref="SchemeStatus.DiscoveredAndFoiled"/> versus <see cref="SchemeStatus.DiscoveredAndEscalated"/>,
/// regardless of how much Progress had accumulated.</item>
/// <item>Otherwise, once Progress reaches 100 cleanly, an Intrigue-weighted roll decides <see
/// cref="SchemeStatus.Succeeded"/> versus <see cref="SchemeStatus.FailedQuietly"/> (§10.5: "completing
/// the plan is necessary but not sufficient").</item>
/// </list>
///
/// A Scheme whose initiator has died, or whose target has died or no longer exists at all, resolves
/// immediately as <see cref="SchemeStatus.FailedQuietly"/> rather than continuing to progress against
/// nobody — mirroring <see cref="InitiateSchemeCommands"/>' own initiation-time liveness checks. Every
/// numeric constant here is this codebase's own untuned first pass — see <see
/// cref="SchemeProgressCatalog"/>'s own doc comments.
/// </summary>
public sealed class SchemeProgressSystem : IMonthlySystem<WorldState>
{
    public string Id => "interactions.schemeProgress";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "schemes", "characters", "actors" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "schemes", "eventIds", "rivalDossiers" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // Materialize first: resolving a Scheme replaces entries in state.Schemes mid-iteration,
        // matching BackgroundHouseDriftSystem's identical "snapshot before mutating" guard.
        var inProgress = state.Schemes.InAscendingOrder()
            .Where(entry => entry.Value.Status == SchemeStatus.InProgress)
            .ToArray();

        foreach (var (schemeId, scheme) in inProgress)
        {
            if (!state.Characters.TryGet(scheme.InitiatorCharacterId, out var initiator) || !initiator.IsAlive ||
                !state.Characters.TryGet(scheme.TargetCharacterId, out var target) || !target.IsAlive)
            {
                Resolve(state, schemeId, scheme, SchemeStatus.FailedQuietly, context.Date, events);
                continue;
            }

            var progressDelta = SchemeProgressCatalog.BaseProgressPerMonthPercent
                + initiator.Attributes.Intrigue * SchemeProgressCatalog.MaxIntrigueProgressBonusPercent / 100;
            var riskDelta = SchemeProgressCatalog.BaseDiscoveryRiskPerMonthPercent
                + target.Attributes.Intrigue * SchemeProgressCatalog.MaxTargetIntrigueRiskBonusPercent / 100;

            var newProgress = Math.Min(Scheme.MaxValue, scheme.Progress + progressDelta);
            var newRisk = Math.Min(Scheme.MaxValue, scheme.DiscoveryRisk + riskDelta);
            var advanced = scheme with { Progress = newProgress, DiscoveryRisk = newRisk };

            if (newRisk >= SchemeProgressCatalog.DiscoveryRiskThresholdPercent)
            {
                var foilChance = Math.Clamp(
                    SchemeProgressCatalog.BaseCounterPlayFoilChancePercent
                        + (target.Attributes.Intrigue - initiator.Attributes.Intrigue)
                            * SchemeProgressCatalog.CounterPlayIntrigueDifferenceWeightPercent / 100,
                    0, 100);
                var foiled = context.RandomStreams.NextUInt(StreamName, 100) < (uint)foilChance;
                Resolve(state, schemeId, advanced, foiled ? SchemeStatus.DiscoveredAndFoiled : SchemeStatus.DiscoveredAndEscalated, context.Date, events);
            }
            else if (newProgress >= Scheme.MaxValue)
            {
                var successChance = Math.Clamp(
                    SchemeProgressCatalog.BaseSuccessChancePercent
                        + initiator.Attributes.Intrigue * SchemeProgressCatalog.SuccessChanceIntrigueWeightPercent / 100,
                    0, 100);
                var succeeded = context.RandomStreams.NextUInt(StreamName, 100) < (uint)successChance;
                Resolve(state, schemeId, advanced, succeeded ? SchemeStatus.Succeeded : SchemeStatus.FailedQuietly, context.Date, events);
            }
            else
            {
                state.Schemes.Remove(schemeId);
                state.Schemes.Add(schemeId, advanced with { LastProgressedDate = context.Date });
            }
        }

        return events;
    }

    private static void Resolve(
        WorldState state, RuntimeId<Scheme> schemeId, Scheme scheme, SchemeStatus status, GameDate date, List<IDomainEvent> events)
    {
        state.Schemes.Remove(schemeId);
        state.Schemes.Add(schemeId, scheme with { Status = status, LastProgressedDate = date });
        events.Add(new SchemeResolvedEvent(state.EventIds.Issue(), date, schemeId, scheme.InitiatorCharacterId, scheme.TargetCharacterId, status));

        // Genuine contact for both participants (Phase 10 package 14) — a Scheme resolving against or
        // by a tracked rival's own head Character is exactly the "shared event" §7 names; a no-op for
        // whichever side is not currently a LivingWorldActor head (e.g. the player's own character).
        var summary = $"A Scheme between the two houses resolved: {status}.";
        RivalDossierRefresh.RefreshForCharacter(state, scheme.InitiatorCharacterId, date, summary);
        RivalDossierRefresh.RefreshForCharacter(state, scheme.TargetCharacterId, date, summary);
    }

    /// <summary>The named random stream this system draws from for its resolution rolls (Phase 10 item
    /// 6), kept distinct from every other stream for rule 8's "adding a draw in one system must not
    /// perturb another".</summary>
    private const string StreamName = CampaignBootstrapper.SchemeProgressStreamName;
}
