using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Stewardship;
using Gens.Simulation.Time;

namespace Gens.Simulation.Succession;

/// <summary>Emitted whenever a minor head with no surviving spouse gets a non-family Regent appointed
/// (Phase 11 item 2; §6.2's "failing that, the household's own highest-ranking appointee"). The
/// Succession-side record of *who* is regent for *which* minor, distinct from the generic <see
/// cref="StewardshipAssignedEvent"/> the appointment pipeline already emits alongside it.</summary>
public sealed record NonFamilyRegencyEstablishedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> MinorHeadCharacterId,
    RuntimeId<Character> RegentCharacterId,
    string? CausationId) : IDomainEvent
{
    public string Type => "succession.regencyEstablished";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), MinorHeadCharacterId.ToTaggedString(), RegentCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>Emitted whenever a Regency ends because its heir has come of age (Phase 11 item 2; §6.2 —
/// this is the "future Regency-ends-when-the-heir-comes-of-age system" <see
/// cref="SuccessionHandoffSystem"/>'s own doc comment named as out of that item's scope).</summary>
public sealed record RegencyEndedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> FormerHeirNowHeadCharacterId,
    RuntimeId<Character> FormerRegentCharacterId,
    string? CausationId) : IDomainEvent
{
    public string Type => "succession.regencyEnded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), FormerHeirNowHeadCharacterId.ToTaggedString(), FormerRegentCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// The monthly system that closes the gap <see cref="SuccessionHandoffSystem"/>'s minor-heir branch
/// leaves open (Phase 11 item 2; §6.2): a minor head with no surviving spouse ends up with no Regent
/// at all today, un-governed. Two responsibilities, both per month, run after <see
/// cref="SuccessionHandoffSystem"/> and <see cref="SuccessionDisputeResolutionSystem"/> so headship is
/// fully settled first:
///
/// <list type="number">
/// <item>Establish a non-family Regency (a Rationalis/Procurator appointee, per §6.2's fallback) for
/// any minor head with no <see cref="HouseholdHeadship.RegentCharacterId"/> yet and no already-active
/// Regency <see cref="StewardshipAssignment"/> — the Regent candidate is the living adult (§6.2's
/// Adult floor) household member with the highest effective Stewardship attribute, tie-broken by
/// lowest <see cref="RuntimeId{T}"/> value (this codebase's everywhere-else deterministic tie-break).
/// If no eligible candidate exists in the household at all, the headship is left as-is this month — an
/// honestly-reached edge case this implementation names rather than hides (matching §10's "untuned
/// numbers" convention of naming open gaps), not a crash.</item>
/// <item>End a Regency once its heir is no longer a minor (§6.2), ending the backing <see
/// cref="StewardshipAssignment"/> if one exists (the spouse-in-trust path never created one) and
/// clearing <see cref="HouseholdHeadship.RegentCharacterId"/> either way.</item>
/// </list>
///
/// Draws no random numbers: candidate selection is a deterministic attribute comparison and Regency
/// end is a deterministic age comparison, matching <see cref="HeirEligibilityService"/>'s own "pure
/// lookup, no RNG" note.
/// </summary>
public sealed class RegencySystem : IMonthlySystem<WorldState>
{
    public string Id => "succession.regency";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "householdHeadships", "characters", "stewardshipAssignments" };

    // Includes every partition AppointStewardshipCommand's and EndStewardshipAssignmentCommand's own
    // mutate handlers touch, alongside this system's own direct householdHeadships writes — ADR 0005's
    // declared write-set must name every partition CapturePartitionVersions tracks that this system
    // can actually change, not just the obvious ones (see StewardAutonomousDecisionSystem.Writes's
    // own doc comment for why).
    public IReadOnlyCollection<string> Writes { get; } = new[]
    {
        "householdHeadships", "stewardshipAssignments", "stewardshipAssignmentIds", "returnReports",
        "returnReportIds", "eventIds", "commandIds", "commandSequence",
    };

    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "succession.handoff", "succession.disputeResolution" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // Materialize first: the loop body mutates state.HouseholdHeadships (Remove+Add) mid-iteration.
        var headships = state.HouseholdHeadships.InAscendingOrder().ToArray();

        foreach (var (householdId, headship) in headships)
        {
            var isMinor = HeirEligibilityService.IsMinor(state, headship.HeadCharacterId, context.Date);

            if (headship.RegentCharacterId is null)
            {
                if (isMinor)
                    TryEstablishNonFamilyRegency(state, householdId, headship, context, events);
                continue;
            }

            if (!isMinor)
                EndRegency(state, householdId, headship, context, events);
        }

        return events;
    }

    private static void TryEstablishNonFamilyRegency(
        WorldState state, RuntimeId<Household> householdId, HouseholdHeadship headship, MonthlyTickContext context, List<IDomainEvent> events)
    {
        if (HasActiveRegency(state, householdId))
            return;

        var candidateId = FindRegentCandidate(state, householdId, headship.HeadCharacterId, context.Date);
        if (candidateId is null)
            return;

        var appointCommand = new AppointStewardshipCommand(
            state.CommandIds.Issue(), "system", context.Date, null, householdId, StewardshipContext.Regency,
            StewardshipMode.SingleSteward, candidateId, CouncilMembers: null, CouncilHeadCharacterId: null,
            StewardshipAssignment.DefaultAutonomyLevel);
        var appointResult = StewardshipCommands.AppointPipeline.Execute(state, appointCommand);
        if (!appointResult.Accepted)
            return;

        events.AddRange(appointResult.Events);

        state.HouseholdHeadships.Remove(householdId);
        state.HouseholdHeadships.Add(householdId, headship with { RegentCharacterId = candidateId });

        events.Add(new NonFamilyRegencyEstablishedEvent(
            state.EventIds.Issue(), context.Date, householdId, headship.HeadCharacterId, candidateId.Value, CausationId: null));
    }

    private static void EndRegency(
        WorldState state, RuntimeId<Household> householdId, HouseholdHeadship headship, MonthlyTickContext context, List<IDomainEvent> events)
    {
        var activeRegency = state.StewardshipAssignments.InAscendingOrder()
            .Where(entry => entry.Value.HouseholdId == householdId && entry.Value.IsActive && entry.Value.Context == StewardshipContext.Regency)
            .Select(entry => entry.Value)
            .FirstOrDefault();

        if (activeRegency is not null)
        {
            var endCommand = new EndStewardshipAssignmentCommand(state.CommandIds.Issue(), "system", context.Date, null, activeRegency.AssignmentId);
            var endResult = StewardshipCommands.EndPipeline.Execute(state, endCommand);
            if (endResult.Accepted)
                events.AddRange(endResult.Events);
        }

        var formerRegentId = headship.RegentCharacterId!.Value;
        state.HouseholdHeadships.Remove(householdId);
        state.HouseholdHeadships.Add(householdId, headship with { RegentCharacterId = null });

        events.Add(new RegencyEndedEvent(
            state.EventIds.Issue(), context.Date, householdId, headship.HeadCharacterId, formerRegentId, CausationId: null));
    }

    private static bool HasActiveRegency(WorldState state, RuntimeId<Household> householdId) =>
        state.StewardshipAssignments.InAscendingOrder()
            .Any(entry => entry.Value.HouseholdId == householdId && entry.Value.IsActive && entry.Value.Context == StewardshipContext.Regency);

    /// <summary>The living adult (§6.2's Adult floor, <see cref="SuccessionCatalog.MinimumAdultAgeYears"/>)
    /// household member — other than the minor head themself — with the highest effective Stewardship
    /// attribute, tie-broken by lowest <see cref="RuntimeId{T}"/> value. <c>null</c> if the household
    /// has no such member (the honestly-reached edge case this system's own doc comment names).</summary>
    private static RuntimeId<Character>? FindRegentCandidate(
        WorldState state, RuntimeId<Household> householdId, RuntimeId<Character> minorHeadId, GameDate asOf)
    {
        RuntimeId<Character>? bestId = null;
        var bestStewardship = int.MinValue;

        foreach (var (id, character) in state.Characters.InAscendingOrder())
        {
            if (id == minorHeadId || !character.IsAlive || character.Household != householdId)
                continue;
            if (character.AgeInYears(asOf) < SuccessionCatalog.MinimumAdultAgeYears)
                continue;

            var stewardship = character.GetEffectiveAttributes().Stewardship;
            if (bestId is null || stewardship > bestStewardship || (stewardship == bestStewardship && id.Value < bestId.Value.Value))
            {
                bestId = id;
                bestStewardship = stewardship;
            }
        }

        return bestId;
    }
}
