using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Actors;
using Gens.Simulation.Activities;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Reputation;
using Gens.Simulation.Scandal;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Feasts;

/// <summary>
/// Judges every Feast's seating chart the month its Arrival &amp; Seating Phase actually occurs (Phase
/// 17 item 5; §4), and applies §4's own Insult/Honor consequences. Piggybacks on <see
/// cref="ActivityProgressSystem"/>'s own monthly tick via a <see cref="Prerequisites"/> dependency —
/// exactly the pattern <see cref="NpcActivityHostingSystem"/> already established — rather than editing
/// <see cref="ActivityProgressSystem"/> or <see cref="ActivityPhaseRunner"/> to know about Feasts at all.
/// A Phase's <c>OccurredDate</c> is set exactly once by <see cref="ActivityPhaseRunner.RunPhase"/>, so
/// a Feast is only ever a resolution candidate for one real month — but resolving a guest's own seating
/// only once, even if this system were somehow ticked twice for that month, is guarded directly on each
/// <see cref="FeastSeatingAssignment"/>: <see cref="ResolveSeating"/> skips any assignment whose <see
/// cref="FeastSeatingAssignment.Judgment"/> is already set, the same "resolve once, keep forever" shape
/// <see cref="HostedActivity.Outcome"/> itself already uses.
///
/// §11's own open question ("independently or as one composite impression?") is resolved here as
/// "independently": each seated guest's actual seat rank is compared to their own expected rank among
/// every attending guest — seated or not, since a large Feast can't seat everyone in the 9 represented
/// seats and an unseated high-standing guest still has to occupy their own expected rank — with a slack
/// (<see cref="FeastCatalog.SeatingJudgmentSlack"/>) before a placement reads as a felt Insult or a
/// deliberate Honor rather than an ordinary, unremarkable seat.
/// </summary>
public sealed class FeastSeatingResolutionSystem : IMonthlySystem<WorldState>
{
    public string Id => "feasts.seatingResolution";
    public TickPhase Phase => TickPhase.RelationshipsActors;

    public IReadOnlyCollection<string> Reads { get; } =
        new[] { "activities", "activityInvitations", "feastSeatingAssignments", "feastRecords", "characters", "householdReputations", "actors" };

    public IReadOnlyCollection<string> Writes { get; } = new[]
    {
        "feastSeatingAssignments", "relationships", "householdReputations", "scandalRecords", "scandalRecordIds",
        "eventIds", "commandIds", "commandSequence",
    };

    /// <summary>Must run after the Activity Engine's own monthly tick, which is what actually sets a
    /// Feast's Arrival &amp; Seating Phase's <c>OccurredDate</c> this system reads.</summary>
    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "activities.progress" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var entry in state.Activities.InAscendingOrder())
        {
            var activity = entry.Value;
            if (!string.Equals(activity.TypeKey, FeastCatalog.FeastType.Key, StringComparison.Ordinal) ||
                FeastRecordResolver.Get(state, activity.Id) is null)
                continue;

            var seatingPhase = activity.Phases.FirstOrDefault(phase => phase.PhaseKey == FeastCatalog.FeastPhaseKeys.ArrivalAndSeating);
            if (seatingPhase is not { OccurredDate: { } occurred } || occurred != context.Date)
                continue;

            ResolveSeating(state, activity, context.Date, events);
        }

        return events;
    }

    private static void ResolveSeating(WorldState state, HostedActivity activity, GameDate date, List<IDomainEvent> events)
    {
        // Expected rank is read from the FULL attending Guest List, not just whoever happens to have a
        // seat assignment — a large Feast can't seat every guest in the 9 represented seats, and a
        // high-standing attendee the host simply never got around to seating still has to occupy their
        // own expected rank, or a lower-standing seated guest would be misjudged against a thinner field.
        var attendees = ActivityInvitationResolver.GuestList(state, activity.Id)
            .Where(invitation => invitation.IsAttending)
            .Select(invitation => invitation.InviteeId)
            .Where(guestId => state.Characters.TryGet(guestId, out var guest) && guest!.IsAlive)
            .OrderByDescending(guestId => ExpectedStanding(state, guestId))
            .ThenBy(guestId => guestId)
            .ToArray();

        for (var i = 0; i < attendees.Length; i++)
        {
            var guestId = attendees[i];
            var assignment = FeastSeatingResolver.Get(state, activity.Id, guestId);
            if (assignment is null || assignment.Judgment is not null)
                continue;

            var expectedPosition = i + 1;
            var actualRank = FeastCatalog.SeatRank(assignment.Couch, assignment.Position);
            var gap = actualRank - expectedPosition;

            var judgment = gap > FeastCatalog.SeatingJudgmentSlack ? FeastSeatingJudgment.UnderSeated
                : gap < -FeastCatalog.SeatingJudgmentSlack ? FeastSeatingJudgment.OverSeated
                : FeastSeatingJudgment.AppropriatelySeated;

            FeastSeatingResolver.Replace(state, assignment with { Judgment = judgment });

            if (judgment == FeastSeatingJudgment.UnderSeated)
                ApplyUnderSeated(state, activity, guestId, expectedPosition, gap - FeastCatalog.SeatingJudgmentSlack, date, events);
            else if (judgment == FeastSeatingJudgment.OverSeated)
                ApplyOverSeated(state, activity, guestId, -gap - FeastCatalog.SeatingJudgmentSlack, actualRank, attendees, date, events);
        }
    }

    private static void ApplyUnderSeated(
        WorldState state, HostedActivity activity, RuntimeId<Character> guestId, int expectedPosition, int effectiveGap,
        GameDate date, List<IDomainEvent> events)
    {
        var opinionPenalty = Math.Min(FeastCatalog.UnderSeatingOpinionPenaltyPerRank * effectiveGap, FeastCatalog.MaxSeatingOpinionMagnitude);
        events.AddRange(RecordInteractionCommands.Pipeline.Execute(
            state,
            new RecordInteractionCommand(
                state.CommandIds.Issue(), "system", date, activity.Id.ToTaggedString(), guestId, activity.HostCharacterId,
                -opinionPenalty, BondTag.None, BondTag.None, RelationshipOrigin.Encounter)).Events);

        if (activity.HostHouseholdId is { } hostHouseholdId)
        {
            var dignitasPenalty = Math.Min(FeastCatalog.UnderSeatingDignitasPenaltyPerRank * effectiveGap, FeastCatalog.MaxSeatingDignitasMagnitude);
            events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
                state,
                new AdjustDignitasCommand(
                    state.CommandIds.Issue(), "system", date, activity.Id.ToTaggedString(), hostHouseholdId, -dignitasPenalty,
                    "a guest was seated below their own reasonably expected standing")).Events);

            if (expectedPosition <= FeastCatalog.SevereInsultExpectedPositionThreshold && effectiveGap >= FeastCatalog.SevereInsultGapThreshold)
            {
                events.AddRange(RecordScandalCommands.Pipeline.Execute(
                    state,
                    new RecordScandalCommand(
                        state.CommandIds.Issue(), "system", date, activity.Id.ToTaggedString(), hostHouseholdId,
                        ScandalSourceType.FeastSeatingInsult, ScandalSeverity.MinorEmbarrassment)).Events);
            }
        }
    }

    private static void ApplyOverSeated(
        WorldState state, HostedActivity activity, RuntimeId<Character> guestId, int effectiveGap, int actualRank,
        RuntimeId<Character>[] rankedGuestIds, GameDate date, List<IDomainEvent> events)
    {
        var opinionBonus = Math.Min(FeastCatalog.OverSeatingOpinionBonusPerRank * effectiveGap, FeastCatalog.MaxSeatingOpinionMagnitude);
        events.AddRange(RecordInteractionCommands.Pipeline.Execute(
            state,
            new RecordInteractionCommand(
                state.CommandIds.Issue(), "system", date, activity.Id.ToTaggedString(), guestId, activity.HostCharacterId,
                opinionBonus, BondTag.None, BondTag.None, RelationshipOrigin.Encounter)).Events);

        if (activity.HostHouseholdId is { } hostHouseholdId)
        {
            var dignitasBonus = Math.Min(FeastCatalog.OverSeatingDignitasBonusPerRank * effectiveGap, FeastCatalog.MaxSeatingDignitasMagnitude);
            events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
                state,
                new AdjustDignitasCommand(
                    state.CommandIds.Issue(), "system", date, activity.Id.ToTaggedString(), hostHouseholdId, dignitasBonus,
                    "a guest was seated above their own reasonably expected standing")).Events);
        }

        // §4's "at the real risk of a corresponding envy... from whoever was thereby displaced": the
        // guest whose own expected rank matches the seat this honored guest actually took.
        var displacedIndex = actualRank - 1;
        if (displacedIndex >= 0 && displacedIndex < rankedGuestIds.Length && rankedGuestIds[displacedIndex] != guestId)
        {
            var displacedGuestId = rankedGuestIds[displacedIndex];
            var envyPenalty = Math.Min(FeastCatalog.OverSeatingEnvyOpinionPenaltyPerRank * effectiveGap, FeastCatalog.MaxSeatingOpinionMagnitude);
            events.AddRange(RecordInteractionCommands.Pipeline.Execute(
                state,
                new RecordInteractionCommand(
                    state.CommandIds.Issue(), "system", date, activity.Id.ToTaggedString(), displacedGuestId, guestId,
                    -envyPenalty, BondTag.None, BondTag.None, RelationshipOrigin.Encounter)).Events);
        }
    }

    /// <summary>§4's "reasonably expected standing": a guest's own household Dignitas when tracked, or a
    /// rival house's own band-tracked Dignitas when the guest is that house's head, matching <see
    /// cref="DignitasResolver.Current"/>'s identical "no entry means zero" neutral default for anyone
    /// else — no bespoke fallback branch needed.</summary>
    private static int ExpectedStanding(WorldState state, RuntimeId<Character> guestId)
    {
        state.Characters.TryGet(guestId, out var guest);
        if (guest!.Household is { } householdId)
            return DignitasResolver.Current(state, householdId);

        foreach (var entry in state.Actors.InAscendingOrder())
            if (entry.Value.HeadCharacterId == guestId)
                return entry.Value.Dignitas;

        return 0;
    }
}
