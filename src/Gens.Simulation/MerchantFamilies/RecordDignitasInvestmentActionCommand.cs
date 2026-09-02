using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.MerchantFamilies;

/// <summary>
/// §6's real, deliberate act of a merchant household actually making one of its three named
/// Dignitas-investment moves (Phase 15 item 3) — the one real mechanical follow-through behind §6's own
/// "closing that second gap through deliberate, visible investment rather than simply waiting." Applies
/// the move's real Dignitas award through <see cref="DignitasResolver"/> (rule 2's "one command path" —
/// this item does not poke <see cref="HouseholdReputation"/> directly, matching <see
/// cref="AdjustDignitasCommand"/>'s own already-established role as the one place Dignitas actually
/// moves) and appends it to the household's own <see cref="SenateEntryInvestmentLog"/>. This item does
/// not itself validate that a Games &amp; Spectacle event, a Public Works Funded Action, a marriage, or
/// a magistracy actually happened first — those are each a real, separate command elsewhere in this
/// codebase (Politics &amp; Patronage §5, Magistracies), and this command's own <see
/// cref="ActionType"/> records which of §6's three moves a caller has already carried out, the same
/// "reveal/record, don't re-validate the upstream trigger" scoping <see
/// cref="RealEstate.TransferPropertyCommand"/>'s own doc comment already gives for a <see
/// cref="RealEstate.PropertyTransferMethod.ForcedSale"/>'s own upstream legal trigger.
/// </summary>
public sealed record RecordDignitasInvestmentActionCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    DignitasInvestmentActionType ActionType) : ICommand;

public sealed record DignitasInvestmentActionRecordedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    DignitasInvestmentActionType ActionType,
    int DignitasEffect,
    string? CausationId) : IDomainEvent
{
    public string Type => "merchantFamilies.dignitasInvestmentActionRecorded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class RecordDignitasInvestmentActionCommands
{
    public static readonly ValidationErrorCode UnrecognizedActionType = new("merchantFamilies.recordDignitasInvestmentAction.unrecognizedActionType");

    public static readonly CommandPipeline<WorldState, RecordDignitasInvestmentActionCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, RecordDignitasInvestmentActionCommand command) =>
        command.ActionType is DignitasInvestmentActionType.FundedGamesOrPublicWorks
            or DignitasInvestmentActionType.StrategicMarriage or DignitasInvestmentActionType.LocalMagistracy
            ? null
            : UnrecognizedActionType;

    private static IDomainEvent[] Mutate(WorldState state, RecordDignitasInvestmentActionCommand command)
    {
        var effect = MerchantFamiliesCatalog.DignitasEffectFor(command.ActionType);
        DignitasResolver.Apply(state, command.HouseholdId, effect);

        var existingActions = state.SenateEntryInvestmentLogs.TryGet(command.HouseholdId, out var log)
            ? log!.Actions
            : Array.Empty<DignitasInvestmentAction>();
        var updatedActions = existingActions
            .Append(new DignitasInvestmentAction(command.ActionType, effect, command.SubmittedDate))
            .ToArray();

        if (state.SenateEntryInvestmentLogs.TryGet(command.HouseholdId, out _))
            state.SenateEntryInvestmentLogs.Remove(command.HouseholdId);
        state.SenateEntryInvestmentLogs.Add(command.HouseholdId, new SenateEntryInvestmentLog(command.HouseholdId, updatedActions));

        return new IDomainEvent[]
        {
            new DignitasInvestmentActionRecordedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, command.ActionType, effect,
                command.CommandId.ToTaggedString()),
        };
    }
}
