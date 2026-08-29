using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Epithets;

/// <summary>§5: formally adopts <see cref="AgnomenId"/> as a standing part of <see
/// cref="HouseholdId"/>'s own family cognomen going forward. Requires the Agnomen's own Character to
/// currently belong to this household (the head, or an ancestor whose <see cref="Character.Household"/>
/// membership never changed) — a direct, simple standing check rather than walking full lineage, since
/// nothing in this codebase yet models a formal "this Character was ever head of this household"
/// history beyond <see cref="Chronicle.GenerationalChapter"/>, which this command deliberately does not
/// require (a still-living head's own Agnomen is adoptable immediately, not only after their chapter
/// closes).</summary>
public sealed record AdoptAgnomenAsCognomenCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Agnomen> AgnomenId) : ICommand;

/// <summary>The validate/mutate pipeline for <see cref="AdoptAgnomenAsCognomenCommand"/> (ADR 0006).</summary>
public static class AdoptAgnomenAsCognomenCommands
{
    public static readonly ValidationErrorCode HouseholdHasNoHead = new("epithets.adoptCognomen.householdHasNoHead");
    public static readonly ValidationErrorCode AgnomenNotFound = new("epithets.adoptCognomen.agnomenNotFound");
    public static readonly ValidationErrorCode AgnomenCharacterNotOfHousehold = new("epithets.adoptCognomen.agnomenCharacterNotOfHousehold");
    public static readonly ValidationErrorCode AlreadyAdopted = new("epithets.adoptCognomen.alreadyAdopted");

    public static readonly CommandPipeline<WorldState, AdoptAgnomenAsCognomenCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, AdoptAgnomenAsCognomenCommand command)
    {
        if (!state.HouseholdHeadships.TryGet(command.HouseholdId, out _))
            return HouseholdHasNoHead;
        if (!state.Agnomens.TryGet(command.AgnomenId, out var agnomen))
            return AgnomenNotFound;
        if (!state.Characters.TryGet(agnomen!.CharacterId, out var character) || character!.Household != command.HouseholdId)
            return AgnomenCharacterNotOfHousehold;

        foreach (var entry in state.InheritedCognomenDecisions.InAscendingOrder())
        {
            if (entry.Value.OriginalAgnomenId == command.AgnomenId && entry.Value.AdoptedAsPermanentCognomen)
                return AlreadyAdopted;
        }

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, AdoptAgnomenAsCognomenCommand command)
    {
        var decisionId = state.InheritedCognomenDecisionIds.Issue();
        var decision = new InheritedCognomenDecision(
            decisionId, command.AgnomenId, command.HouseholdId, AdoptedAsPermanentCognomen: true, command.SubmittedDate);
        state.InheritedCognomenDecisions.Add(decisionId, decision);

        return new IDomainEvent[]
        {
            new CognomenAdoptedEvent(
                state.EventIds.Issue(), command.SubmittedDate, decisionId, command.HouseholdId, command.AgnomenId,
                command.CommandId.ToTaggedString()),
        };
    }
}
