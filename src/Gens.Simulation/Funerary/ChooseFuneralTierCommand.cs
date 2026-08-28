using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Funerary;

/// <summary>Chooses a <see cref="FuneralTier"/> for a <see cref="FuneralStatus.Pending"/> <see
/// cref="FuneralRecord"/> and immediately holds it (Phase 11 item 4; §2.2) — a single command rather
/// than a separate "choose" then "hold" pair, since nothing in this pass's scope needs the two steps
/// to happen on different months (§2.1's <c>collocatio</c> viewing period is flavor-only, per that
/// section's own doc comment).</summary>
public sealed record ChooseFuneralTierCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<FuneralRecord> FuneralId,
    FuneralTier Tier) : ICommand;

/// <summary>The validate/mutate pipeline for <see cref="ChooseFuneralTierCommand"/> (ADR 0006).</summary>
public static class ChooseFuneralTierCommands
{
    public static readonly ValidationErrorCode FuneralNotFound = new("funerary.chooseFuneralTier.funeralNotFound");
    public static readonly ValidationErrorCode FuneralAlreadyHeld = new("funerary.chooseFuneralTier.funeralAlreadyHeld");
    public static readonly ValidationErrorCode InsufficientTreasury = new("funerary.chooseFuneralTier.insufficientTreasury");

    public static readonly CommandPipeline<WorldState, ChooseFuneralTierCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, ChooseFuneralTierCommand command)
    {
        if (!state.FuneralRecords.TryGet(command.FuneralId, out var funeral))
            return FuneralNotFound;
        if (funeral!.Status != FuneralStatus.Pending)
            return FuneralAlreadyHeld;

        var cost = FuneraryCatalog.TreasuryCost(command.Tier);
        var balance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(funeral.HouseholdId), out var account)
            ? account!.Balance
            : Money.Zero;
        if (balance < cost)
            return InsufficientTreasury;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, ChooseFuneralTierCommand command)
    {
        state.FuneralRecords.TryGet(command.FuneralId, out var funeral);
        var events = FuneralResolution.Hold(
            state, funeral!, command.Tier, command.SubmittedDate, autoResolved: false, command.CommandId.ToTaggedString());

        return events.ToArray();
    }
}
