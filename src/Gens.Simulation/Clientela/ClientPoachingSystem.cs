using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Clientela;

/// <summary>Emitted whenever <see cref="ClientPoachingSystem"/> actually flips a client's bond away
/// from their patron. Private to the old patron's head, the client, and the rival's head — the same
/// three-party shape a poaching event naturally has, extending <see
/// cref="Reputation.FavorGrantedEvent"/>'s "private, named-parties" convention to a third participant.</summary>
public sealed record ClientPoachedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> OldPatronHouseholdId,
    RuntimeId<Character> OldPatronHeadId,
    RuntimeId<Character> ClientId,
    RuntimeId<Actor> RivalActorId,
    RuntimeId<Character> RivalHeadId,
    string? CausationId) : IDomainEvent
{
    public string Type => "clientela.clientPoached";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { OldPatronHeadId.ToTaggedString(), ClientId.ToTaggedString(), RivalHeadId.ToTaggedString() };
    public Visibility Visibility => Visibility.Private(OldPatronHeadId.ToTaggedString(), ClientId.ToTaggedString(), RivalHeadId.ToTaggedString());
}

/// <summary>
/// The monthly poaching-risk tick (Phase 12 item 2; §4.5): "a high-Ambition client whose favors keep
/// going unrewarded... is a real poaching risk — the client relationship-web bond can flip from the
/// player's Patron/Client tag to a rival's." §4.5 itself only asks for "whatever Character is available"
/// as the poacher "for now," with the mechanic "already built to point at a real Rival House record the
/// moment that system is designed" — but Rival Houses (<see cref="LivingWorldActor"/>) already shipped
/// in Phase 10, so this system targets a real <see cref="LivingWorldActor"/> directly rather than
/// inventing a placeholder stranger, closing that forward reference immediately instead of waiting on
/// it a second time.
///
/// <b>Scope note:</b> only an Actor whose head Character has already been lazily generated (<see
/// cref="LivingWorldActorHeadGenerator"/>) is eligible to poach — this system does not itself trigger
/// that generation, which needs a <see cref="Characters.NamePool"/>/culture/settlement context this
/// generic Clientela system has no principled way to supply on a rival's behalf. If no Actor with a
/// generated head exists yet in a given campaign, poaching risk accrues (the underlying Ambition/Loyalty/
/// overdrawn check still applies) but has no eligible target to resolve against that tick — an honest
/// "nothing to poach the client into yet" rather than a fabricated one.
/// </summary>
public sealed class ClientPoachingSystem : IMonthlySystem<WorldState>
{
    /// <summary>The named random stream this system reserves for its monthly poaching-chance roll
    /// (rule 8), kept distinct from every other stream in <see cref="Campaign.CampaignBootstrapper"/>.</summary>
    public const string PoachingRiskStreamName = "clientela.poachingRisk";

    public string Id => "clientela.clientPoaching";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "clientelaEntries", "characters", "actors", "householdHeadships" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "clientelaEntries", "relationships", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // Materialize first: a successful poach removes the entry being iterated, matching
        // RelationshipDecaySystem's identical "snapshot before mutating" guard.
        foreach (var entry in state.ClientelaEntries.InAscendingOrder().Select(kv => kv.Value).ToArray())
        {
            if (!state.Characters.TryGet(entry.ClientId, out var client) || !client!.IsAlive)
                continue;

            if (client.Condition.Ambition < ClientelaCatalog.PoachingHighAmbitionThreshold)
                continue;
            if (client.Condition.Loyalty > ClientelaCatalog.PoachingLowLoyaltyThreshold)
                continue;

            var sinceMonth = (entry.LastFavorCalledDate ?? entry.RecruitedDate).TotalMonths;
            if (context.Date.TotalMonths - sinceMonth < ClientelaCatalog.PoachingOverdrawnAfterMonths)
                continue;

            if (!state.HouseholdHeadships.TryGet(entry.PatronHouseholdId, out var headship))
                continue;
            var oldPatronHeadId = headship!.HeadCharacterId;

            if (!TryFindEligibleRival(state, oldPatronHeadId, out var rivalActorId, out var rivalHeadId))
                continue;

            if (context.RandomStreams.NextUInt(PoachingRiskStreamName, 100) >= (uint)ClientelaCatalog.PoachingChancePercent)
                continue;

            state.ClientelaEntries.Remove(entry.ClientId);
            ClientelaBondHelper.BreakBond(state, oldPatronHeadId, entry.ClientId, context.Date);
            ClientelaBondHelper.EstablishBond(state, rivalHeadId, entry.ClientId, context.Date);

            events.Add(new ClientPoachedEvent(
                state.EventIds.Issue(), context.Date, entry.PatronHouseholdId, oldPatronHeadId, entry.ClientId,
                rivalActorId, rivalHeadId, CausationId: null));
        }

        return events;
    }

    private static bool TryFindEligibleRival(
        WorldState state, RuntimeId<Character> oldPatronHeadId, out RuntimeId<Actor> rivalActorId, out RuntimeId<Character> rivalHeadId)
    {
        foreach (var entry in state.Actors.InAscendingOrder())
        {
            if (entry.Value.HeadCharacterId is not { } headId || headId == oldPatronHeadId)
                continue;
            if (!state.Characters.TryGet(headId, out var head) || !head!.IsAlive)
                continue;

            rivalActorId = entry.Key;
            rivalHeadId = headId;
            return true;
        }

        rivalActorId = default;
        rivalHeadId = default;
        return false;
    }
}
