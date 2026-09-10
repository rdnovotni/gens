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

namespace Gens.Simulation.Interactions;

/// <summary>Sets a household's <see cref="EstateSecurityInvestment.SecurityLevel"/> to <see
/// cref="TargetSecurityLevel"/> (<c>gens-piracy-banditry-design.md</c> §9's "the defensive investment").
/// Resolved immediately in <see cref="AdjustEstateSecurityInvestmentCommands.Pipeline"/>'s mutate step,
/// like <see cref="CounterEspionageSweepCommand"/> — a deliberate, explicit player/AI spending decision,
/// not ambient background drift.</summary>
public sealed record AdjustEstateSecurityInvestmentCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> SponsoringCharacterId,
    int TargetSecurityLevel) : ICommand;

/// <summary>Emitted whenever an <see cref="AdjustEstateSecurityInvestmentCommand"/> is accepted.</summary>
public sealed record EstateSecurityInvestmentAdjustedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    int PreviousSecurityLevel,
    int NewSecurityLevel,
    string? CausationId) : IDomainEvent
{
    public string Type => "interactions.estateSecurityInvestmentAdjusted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Private(HouseholdId.ToTaggedString());
}

/// <summary>The validate/mutate pipeline for <see cref="AdjustEstateSecurityInvestmentCommand"/> (ADR 0006).</summary>
public static class AdjustEstateSecurityInvestmentCommands
{
    public static readonly ValidationErrorCode SponsorNotFound = new("interactions.adjustEstateSecurityInvestment.sponsorNotFound");
    public static readonly ValidationErrorCode SponsorDeceased = new("interactions.adjustEstateSecurityInvestment.sponsorDeceased");
    public static readonly ValidationErrorCode SponsorNotOfHousehold = new("interactions.adjustEstateSecurityInvestment.sponsorNotOfHousehold");
    public static readonly ValidationErrorCode SecurityLevelOutOfRange = new("interactions.adjustEstateSecurityInvestment.securityLevelOutOfRange");
    public static readonly ValidationErrorCode InsufficientFunds = new("interactions.adjustEstateSecurityInvestment.insufficientFunds");

    public static readonly CommandPipeline<WorldState, AdjustEstateSecurityInvestmentCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, AdjustEstateSecurityInvestmentCommand command)
    {
        if (!state.Characters.TryGet(command.SponsoringCharacterId, out var sponsor))
            return SponsorNotFound;
        if (!sponsor.IsAlive)
            return SponsorDeceased;
        if (sponsor.Household != command.HouseholdId)
            return SponsorNotOfHousehold;
        if (command.TargetSecurityLevel < EstateSecurityInvestment.MinValue || command.TargetSecurityLevel > EstateSecurityInvestment.MaxValue)
            return SecurityLevelOutOfRange;

        var currentLevel = state.EstateSecurityInvestments.TryGet(command.HouseholdId, out var existing)
            ? existing!.SecurityLevel
            : EstateSecurityInvestment.MinValue;
        var levelsGained = command.TargetSecurityLevel - currentLevel;
        if (levelsGained > 0)
        {
            var cost = Money.FromDenarii(levelsGained * RaidThreatCatalog.SecurityLevelCostPerPointDenarii);
            var account = LedgerAccountKey.ForHousehold(command.HouseholdId);
            var balance = state.LedgerAccounts.TryGet(account, out var ledgerAccount) ? ledgerAccount!.Balance : Money.Zero;
            if (balance < cost)
                return InsufficientFunds;
        }

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, AdjustEstateSecurityInvestmentCommand command)
    {
        var events = new List<IDomainEvent>();

        var currentLevel = state.EstateSecurityInvestments.TryGet(command.HouseholdId, out var existing)
            ? existing!.SecurityLevel
            : EstateSecurityInvestment.MinValue;
        var levelsGained = command.TargetSecurityLevel - currentLevel;

        if (levelsGained > 0)
        {
            var cost = Money.FromDenarii(levelsGained * RaidThreatCatalog.SecurityLevelCostPerPointDenarii);
            var account = LedgerAccountKey.ForHousehold(command.HouseholdId);
            events.Add(LedgerService.Post(
                state, command.SubmittedDate, LedgerTransactionCategory.Purchases,
                new[]
                {
                    new LedgerPosting(account, -cost),
                    new LedgerPosting(LedgerAccountKey.Mint, cost),
                },
                reference: $"interactions.adjustEstateSecurityInvestment:{command.CommandId.ToTaggedString()}"));
        }

        var updated = existing is null
            ? EstateSecurityInvestment.CreateUnguarded(command.HouseholdId, command.SubmittedDate).WithLevel(command.TargetSecurityLevel, command.SubmittedDate)
            : existing.WithLevel(command.TargetSecurityLevel, command.SubmittedDate);

        if (existing is not null)
            state.EstateSecurityInvestments.Remove(command.HouseholdId);
        state.EstateSecurityInvestments.Add(command.HouseholdId, updated);

        events.Add(new EstateSecurityInvestmentAdjustedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, currentLevel, command.TargetSecurityLevel,
            command.CommandId.ToTaggedString()));

        return events.ToArray();
    }
}
