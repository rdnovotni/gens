using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Interactions;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Magistracies;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Policies;

/// <summary>Phase 16 item 6's Magistracies Funded Action: a household spend that raises its own <see
/// cref="EstateSecurityInvestment.SecurityLevel"/>, mirroring <see cref="FundFestivalCommand"/>/<see
/// cref="FundDisasterReliefCommand"/>'s exact shape (a one-off ledger spend into a named system sink,
/// plus the standard Funded-Action Dignitas payoff). No design doc gates raising a private force or
/// ratifying a treaty on holding office (verified directly — no file under <c>Magistracies/</c>
/// references Military, Combat, or Diplomacy anywhere), so this item does not invent such a gate; instead
/// it extends <see cref="MagistracyOffice.Aedile"/>'s own documented "occasional real duty" framing with
/// a modest bonus when the funding household's own head holds that office at the target settlement —
/// reusing the office-holding check <see cref="Crime.ImprisonCommand"/>'s own <c>HoldsActiveOfficeAtTargetSettlement</c>
/// already demonstrates, narrowed to <see cref="MagistracyOffice.Aedile"/> specifically.</summary>
public sealed record FundLocalDefenseCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Settlement> SettlementId,
    Money Amount) : ICommand;

/// <summary>Emitted whenever a <see cref="FundLocalDefenseCommand"/> is accepted, alongside the <see
/// cref="LedgerTransactionPostedEvent"/> <see cref="LedgerService.Post"/> itself produces, matching <see
/// cref="FestivalFundedEvent"/>'s identical "both the ledger receipt and the domain-specific event"
/// convention.</summary>
public sealed record LocalDefenseFundedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Settlement> SettlementId,
    Money Amount,
    int SecurityLevelsGained,
    int DignitasGained,
    bool AedileBonusApplied,
    RuntimeId<LedgerTransaction> TransactionId,
    string? CausationId) : IDomainEvent
{
    public string Type => "policies.localDefenseFunded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), SettlementId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="FundLocalDefenseCommand"/> (ADR 0006).</summary>
public static class FundLocalDefenseCommands
{
    public static readonly ValidationErrorCode AmountMustBePositive = new("policies.fundLocalDefense.amountMustBePositive");
    public static readonly ValidationErrorCode InsufficientTreasury = new("policies.fundLocalDefense.insufficientTreasury");

    /// <summary>§6.2's own real Dignitas payoff for a Funded Action, sized like <see
    /// cref="FundDisasterReliefCommands.DignitasGain"/> — this implementation's own invented figure, a
    /// flat gain rather than spend-scaled, matching that command's identical "the visible act of
    /// patronage, not its exact price tag" framing.</summary>
    public const int DignitasGain = 6;

    /// <summary>The Aedile bonus multiplier (percent) applied to <see cref="DignitasGain"/> when the
    /// funding household's own head holds an active <see cref="MagistracyOffice.Aedile"/> seat at the
    /// target settlement — this implementation's own invented figure, a modest extension of that
    /// office's documented "occasional real duty," not a new invented authority (no gate, just a bonus
    /// on an action any household could already take).</summary>
    public const int AedileBonusPercent = 50;

    private static readonly LedgerAccountKey LocalDefenseSink = new(LedgerAccountKind.System, "fundedaction:localdefense");

    public static readonly CommandPipeline<WorldState, FundLocalDefenseCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, FundLocalDefenseCommand command)
    {
        if (command.Amount <= Money.Zero)
            return AmountMustBePositive;

        var balance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(command.HouseholdId), out var account)
            ? account!.Balance
            : Money.Zero;
        if (balance < command.Amount)
            return InsufficientTreasury;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, FundLocalDefenseCommand command)
    {
        var posted = LedgerService.Post(
            state, command.SubmittedDate, LedgerTransactionCategory.Purchases,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(command.HouseholdId), -command.Amount),
                new LedgerPosting(LocalDefenseSink, command.Amount),
            },
            reference: $"fundedAction:localDefense:{command.CommandId.ToTaggedString()}");

        var denarii = command.Amount.RawValue / Money.ScaleFactor;
        var levelsGained = (int)(denarii / RaidThreatCatalog.SecurityLevelCostPerPointDenarii);

        var currentLevel = state.EstateSecurityInvestments.TryGet(command.HouseholdId, out var existing)
            ? existing!.SecurityLevel
            : EstateSecurityInvestment.MinValue;
        var updated = existing is null
            ? EstateSecurityInvestment.CreateUnguarded(command.HouseholdId, command.SubmittedDate).WithLevel(currentLevel + levelsGained, command.SubmittedDate)
            : existing.WithLevel(currentLevel + levelsGained, command.SubmittedDate);
        if (existing is not null)
            state.EstateSecurityInvestments.Remove(command.HouseholdId);
        state.EstateSecurityInvestments.Add(command.HouseholdId, updated);

        var aedileBonusApplied = HoldsActiveAedileAtSettlement(state, command.HouseholdId, command.SettlementId);
        var dignitasGain = aedileBonusApplied ? DignitasGain * (100 + AedileBonusPercent) / 100 : DignitasGain;
        DignitasResolver.Apply(state, command.HouseholdId, dignitasGain);

        return new IDomainEvent[]
        {
            posted,
            new LocalDefenseFundedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, command.SettlementId, command.Amount,
                updated.SecurityLevel - currentLevel, dignitasGain, aedileBonusApplied, posted.TransactionId,
                command.CommandId.ToTaggedString()),
        };
    }

    /// <summary>Mirrors <see cref="Crime.ImprisonCommand"/>'s own <c>HoldsActiveOfficeAtTargetSettlement</c>
    /// linear-scan check, narrowed to <see cref="MagistracyOffice.Aedile"/> specifically and resolved
    /// against the funding household's own current head (this command carries no separate sponsor field,
    /// matching <see cref="FundFestivalCommand"/>'s own household-level-only shape) rather than a
    /// caller-supplied Character id.</summary>
    private static bool HoldsActiveAedileAtSettlement(WorldState state, RuntimeId<Household> householdId, RuntimeId<Settlement> settlementId)
    {
        if (!state.HouseholdHeadships.TryGet(householdId, out var headship))
            return false;

        foreach (var entry in state.MagistracyRecords.InAscendingOrder())
        {
            var record = entry.Value;
            if (MagistracyResolver.IsActive(record) && record.Office == MagistracyOffice.Aedile &&
                record.HolderId == headship!.HeadCharacterId && record.SettlementId == settlementId)
                return true;
        }

        return false;
    }
}
