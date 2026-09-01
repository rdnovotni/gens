using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Health;

/// <summary>Sets (or changes) a settlement's standing §6 Sanitation Investment tier — the player-
/// or steward-facing lever over <see cref="SettlementSanitationInvestment"/>, matching Religion's
/// <c>ChangeRitesBudgetCommand</c> as the identical "player picks a standing policy tier" shape.</summary>
public sealed record SetSanitationInvestmentCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Settlement> SettlementId,
    SanitationInvestmentTier Tier) : ICommand;

/// <summary>Emitted whenever a <see cref="SetSanitationInvestmentCommand"/> is accepted.</summary>
public sealed record SanitationInvestmentChangedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Settlement> SettlementId,
    SanitationInvestmentTier Tier,
    string? CausationId) : IDomainEvent
{
    public string Type => "health.sanitationInvestmentChanged";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { SettlementId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class SetSanitationInvestmentCommands
{
    public static readonly ValidationErrorCode SettlementNotFound = new("health.sanitation.settlementNotFound");

    public static readonly CommandPipeline<WorldState, SetSanitationInvestmentCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, SetSanitationInvestmentCommand command)
    {
        if (!state.Settlements.TryGet(command.SettlementId, out _))
            return SettlementNotFound;
        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, SetSanitationInvestmentCommand command)
    {
        state.SettlementSanitationInvestments.Remove(command.SettlementId);
        state.SettlementSanitationInvestments.Add(
            command.SettlementId,
            SettlementSanitationInvestment.Create(command.SettlementId, command.Tier));

        return new IDomainEvent[]
        {
            new SanitationInvestmentChangedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.SettlementId, command.Tier,
                command.CommandId.ToTaggedString()),
        };
    }
}
