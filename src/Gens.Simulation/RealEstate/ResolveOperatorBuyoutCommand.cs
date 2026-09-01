using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.RealEstate;

/// <summary>§6.1's "an ambitious, successful Operator... can eventually accumulate enough of their own
/// wealth to offer to buy the property outright" (Phase 15 item 1) — the player's own response to a
/// buyout offer <see cref="OperatorLifecycleSystem"/> has already flagged (<see
/// cref="PropertyRecord.OperatorBuyoutOffered"/>/<see
/// cref="PlotPropertyExtension.OperatorBuyoutOffered"/>). Accepting converts a Leased Out property into
/// a genuine, independent <see cref="PropertyOwnerKind.IndividualCharacter"/> holding (§2) — "the seed
/// of that Character's own future gens" — settled through the Ledger exactly like any other <see
/// cref="TransferPropertyCommand"/> sale, at the property's own currently tracked Value (the Operator's
/// own accumulated wealth is deliberately abstract, per §14's own "all numeric sizing... unsized";
/// this item does not track a separate Operator personal-wealth figure to check the price
/// against — the offer having fired at all, per <see cref="OperatorLifecycleSystem"/>'s own gating, is
/// this item's stand-in for "the Operator can actually afford it"). Declining simply clears the flag —
/// §6.1's other, unremarkable branch: "the arrangement is unremarkable" continues exactly as
/// before.</summary>
public sealed record ResolveOperatorBuyoutCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    PropertySubjectRef Subject,
    bool Accept) : ICommand;

public sealed record OperatorBuyoutResolvedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    PropertySubjectRef Subject,
    RuntimeId<Character> OperatorCharacterId,
    bool Accepted,
    Money? Price,
    string? CausationId) : IDomainEvent
{
    public string Type => "realEstate.operatorBuyoutResolved";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { Subject.SubjectId, OperatorCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class ResolveOperatorBuyoutCommands
{
    public static readonly ValidationErrorCode SubjectNotFound = new("realEstate.resolveBuyout.subjectNotFound");
    public static readonly ValidationErrorCode NoBuyoutOffered = new("realEstate.resolveBuyout.noBuyoutOffered");

    public static readonly CommandPipeline<WorldState, ResolveOperatorBuyoutCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, ResolveOperatorBuyoutCommand command)
    {
        if (!PropertyResolver.TryResolve(state, command.Subject, out var view))
            return SubjectNotFound;
        if (!view.OperatorBuyoutOffered || view.OperatorCharacterId is null)
            return NoBuyoutOffered;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, ResolveOperatorBuyoutCommand command)
    {
        PropertyResolver.TryResolve(state, command.Subject, out var view);
        var operatorId = view.OperatorCharacterId!.Value;
        var events = new List<IDomainEvent>();
        Money? price = null;

        if (command.Accept)
        {
            var transferResult = TransferPropertyCommands.Pipeline.Execute(
                state,
                new TransferPropertyCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    command.Subject, PropertyTransferMethod.VoluntarySale, PropertyOwnerRef.ForIndividualCharacter(operatorId)));
            if (transferResult.Accepted)
            {
                events.AddRange(transferResult.Events);
                price = view.Value;

                // The former Operator now owns the property outright — Directly Managed by its own
                // new owner, not "leased to itself" (TransferPropertyCommand's own SetOwner call only
                // touches Owner/LesseeId, deliberately preserving management state for the ordinary
                // sale case where an existing lease arrangement plausibly carries over to a new
                // owner; a buyout is the one real exception, since here the buyer and the former
                // Operator are the same Character).
                PropertyResolver.SetManagement(state, command.Subject, PropertyManagementStatus.DirectlyManaged, operatorCharacterId: null);
            }
        }

        // Either branch clears the flag and, for a declined offer, keeps the lease running exactly as
        // before (§6.1's "unremarkable" continuation) — reset tenure/skim state only on acceptance,
        // since accepting hands the property to a now-independent owner whose Operator record no
        // longer applies (TransferPropertyCommand already reassigned ownership; PropertyResolver's own
        // TryResolve after the transfer would resolve a fresh, unmanaged property, so this command
        // does not need a second write there).
        if (!command.Accept)
            PropertyResolver.SetOperatorState(
                state, command.Subject, view.OperatorIsSkimming, view.OperatorHasEverSkimmed, view.OperatorTenureMonths,
                buyoutOffered: false);

        events.Add(new OperatorBuyoutResolvedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.Subject, operatorId, command.Accept, price,
            command.CommandId.ToTaggedString()));
        return events.ToArray();
    }
}
