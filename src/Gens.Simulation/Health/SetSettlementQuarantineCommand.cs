using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Health;

/// <summary>§4.2 Settlement-Wide Quarantine — toggles <see
/// cref="EpidemicOutbreak.SettlementQuarantineActive"/> on one standing outbreak. Only meaningful
/// against an <see cref="EpidemicOutbreakStatus.Active"/> outbreak. §4.2's own "at a real Contentment
/// and Commerce cost" was item 2's own disclosed gap and is real as of Phase 14 item 5: <see
/// cref="HealthQueries.IsSettlementUnderQuarantine"/> is the shared read both costs hang off —
/// <see cref="EpidemicContagionSystem"/>'s own monthly felt Contentment shock (<see
/// cref="QuarantineEffectCalculator.ContentmentImpact"/>) and <see
/// cref="Markets.MarketClearingSystem"/>'s own supply-side Commerce multiplier (<see
/// cref="QuarantineEffectCalculator.CommerceSupplyMultiplier"/>) — alongside the spread-reduction effect
/// this item already delivered (<see cref="QuarantineEffectCalculator.SettlementSpreadMultiplier"/>).</summary>
public sealed record SetSettlementQuarantineCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<EpidemicOutbreak> OutbreakId,
    bool QuarantineActive) : ICommand;

/// <summary>Emitted whenever a <see cref="SetSettlementQuarantineCommand"/> is accepted.</summary>
public sealed record SettlementQuarantineChangedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Land.Settlement> SettlementId,
    RuntimeId<EpidemicOutbreak> OutbreakId,
    bool QuarantineActive,
    string? CausationId) : IDomainEvent
{
    public string Type => "health.settlementQuarantineChanged";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { SettlementId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class SetSettlementQuarantineCommands
{
    public static readonly ValidationErrorCode OutbreakNotFound = new("health.settlementQuarantine.outbreakNotFound");
    public static readonly ValidationErrorCode OutbreakNotActive = new("health.settlementQuarantine.outbreakNotActive");

    public static readonly CommandPipeline<WorldState, SetSettlementQuarantineCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, SetSettlementQuarantineCommand command)
    {
        if (!state.EpidemicOutbreaks.TryGet(command.OutbreakId, out var outbreak))
            return OutbreakNotFound;
        if (outbreak.Status != EpidemicOutbreakStatus.Active)
            return OutbreakNotActive;
        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, SetSettlementQuarantineCommand command)
    {
        state.EpidemicOutbreaks.TryGet(command.OutbreakId, out var outbreak);
        state.EpidemicOutbreaks.Remove(command.OutbreakId);
        state.EpidemicOutbreaks.Add(command.OutbreakId, outbreak with { SettlementQuarantineActive = command.QuarantineActive });

        return new IDomainEvent[]
        {
            new SettlementQuarantineChangedEvent(
                state.EventIds.Issue(), command.SubmittedDate, outbreak.SettlementId, command.OutbreakId,
                command.QuarantineActive, command.CommandId.ToTaggedString()),
        };
    }
}
