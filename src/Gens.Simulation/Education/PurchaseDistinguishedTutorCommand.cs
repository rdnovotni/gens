using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Education;

/// <summary>
/// One household's paid-for Distinguished tier upgrade on a Character's active <see
/// cref="EducationalTrackEnrollment"/> (Phase 17 item 2; §3). No generic runtime building-tier-upgrade
/// concept exists in this codebase to reuse (per the ticket's own scope note), so this is a bespoke,
/// one-time investment record — kept forever once purchased, matching this codebase's "kept for the
/// campaign's lifetime" convention for a real historical fact, keyed by the Character it upgrades (one
/// Distinguished purchase in effect per Character at a time, matching <see
/// cref="EducationalTrackEnrollment"/>'s identical single-entry-per-Character shape).
/// </summary>
public sealed record DistinguishedEducationInvestment(
    RuntimeId<Character> CharacterId,
    DefinitionId<EducationTrack> TrackId,
    GameDate PurchasedDate,
    Money AmountPaid);

/// <summary>Emitted whenever a <see cref="PurchaseDistinguishedTutorCommand"/> is accepted.</summary>
public sealed record DistinguishedTutorPurchasedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    DefinitionId<EducationTrack> TrackId,
    string? CausationId) : IDomainEvent
{
    public string Type => "education.distinguishedTutorPurchased";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Private(CharacterId.ToTaggedString());
}

/// <summary>
/// Upgrades a Character's active Educational Track enrollment to the Distinguished tier (§3's "a paid
/// upgrade tier"): a one-time Ledger draw of the Track's <see
/// cref="EducationTrackDefinition.DistinguishedTierCostPerMonth"/> (this implementation's own reading of
/// that figure as a single upfront investment rather than a second ongoing monthly draw layered on top of
/// <see cref="EducationalTrackProgressSystem"/>'s own attribute-gain multiplier — the "per month" naming
/// describes the value tier being bought, not a second recurring charge), which then flips <see
/// cref="EducationalTrackEnrollment.DistinguishedTierActive"/> for that system's own multiplier to read.
/// </summary>
public sealed record PurchaseDistinguishedTutorCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> CharacterId) : ICommand;

/// <summary>The validate/mutate pipeline for <see cref="PurchaseDistinguishedTutorCommand"/> (ADR 0006).</summary>
public static class PurchaseDistinguishedTutorCommands
{
    public static readonly ValidationErrorCode NoActiveEnrollment = new("education.purchaseDistinguished.noActiveEnrollment");
    public static readonly ValidationErrorCode AlreadyDistinguished = new("education.purchaseDistinguished.alreadyDistinguished");
    public static readonly ValidationErrorCode InsufficientTreasury = new("education.purchaseDistinguished.insufficientTreasury");

    private static readonly LedgerAccountKey DistinguishedTutorSink = new(LedgerAccountKind.System, "education:distinguishedTutor");

    public static readonly CommandPipeline<WorldState, PurchaseDistinguishedTutorCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, PurchaseDistinguishedTutorCommand command)
    {
        if (!EducationalTrackEnrollmentResolver.TryGet(state, command.CharacterId, out var enrollment) ||
            !EducationalTrackEnrollmentResolver.IsActive(enrollment))
        {
            return NoActiveEnrollment;
        }
        if (enrollment.DistinguishedTierActive)
            return AlreadyDistinguished;

        var track = KnownEducationTracks.Catalog.Get(enrollment.TrackId);
        var balance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(command.HouseholdId), out var account)
            ? account!.Balance
            : Money.Zero;
        if (balance < track.DistinguishedTierCostPerMonth)
            return InsufficientTreasury;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, PurchaseDistinguishedTutorCommand command)
    {
        state.EducationalTrackEnrollments.TryGet(command.CharacterId, out var enrollment);
        var track = KnownEducationTracks.Catalog.Get(enrollment.TrackId);

        state.EducationalTrackEnrollments.Remove(command.CharacterId);
        state.EducationalTrackEnrollments.Add(command.CharacterId, enrollment with { DistinguishedTierActive = true });

        if (state.DistinguishedEducationInvestments.TryGet(command.CharacterId, out _))
            state.DistinguishedEducationInvestments.Remove(command.CharacterId);
        state.DistinguishedEducationInvestments.Add(
            command.CharacterId,
            new DistinguishedEducationInvestment(command.CharacterId, enrollment.TrackId, command.SubmittedDate, track.DistinguishedTierCostPerMonth));

        var posted = LedgerService.Post(
            state, command.SubmittedDate, LedgerTransactionCategory.Gifts,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(command.HouseholdId), -track.DistinguishedTierCostPerMonth),
                new LedgerPosting(DistinguishedTutorSink, track.DistinguishedTierCostPerMonth),
            },
            reference: $"education:distinguishedTutor:{command.CharacterId.ToTaggedString()}");

        return new IDomainEvent[]
        {
            posted,
            new DistinguishedTutorPurchasedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CharacterId, enrollment.TrackId, command.CommandId.ToTaggedString()),
        };
    }
}
