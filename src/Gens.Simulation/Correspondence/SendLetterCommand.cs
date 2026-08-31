using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Regions;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using Gens.Simulation.Travel;

namespace Gens.Simulation.Correspondence;

/// <summary>Commits an outbound <see cref="Letter"/> (§5's nine correspondence actions): resolves its
/// <see cref="LetterRoute"/> and starts it in transit. The <paramref name="DraftedByCharacterId"/> is
/// always this letter's own sender (§11's <c>senderCharacterOrActorId</c>) — a household head writing,
/// or dictating to a scribe, is still diegetically "the sender," matching real Roman correspondence's
/// own naming convention (§4).</summary>
public sealed record SendLetterCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> DraftedByCharacterId,
    string RecipientCharacterOrActorId,
    LetterAction Action,
    CourierType CourierType,
    RuntimeId<Character>? CourierCharacterId,
    DefinitionId<RegionProfileDefinition> SenderRegionId,
    DefinitionId<RegionProfileDefinition> RecipientRegionId,
    DefinitionId<Culture>? RecipientCultureId) : ICommand;

/// <summary>Emitted whenever a <see cref="SendLetterCommand"/> is accepted.</summary>
public sealed record LetterSentEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Letter> LetterId,
    RuntimeId<Character> DraftedByCharacterId,
    string RecipientCharacterOrActorId,
    LetterAction Action,
    int TransitTimeMonths,
    string? CausationId) : IDomainEvent
{
    public string Type => "correspondence.sent";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { DraftedByCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// The validate/mutate pipeline for <see cref="SendLetterCommand"/> (ADR 0006). Built per <see
/// cref="DistanceTierCatalog"/>/<see cref="CorrespondenceReachabilityCatalog"/>, matching <see
/// cref="Travel.BeginTravelCommands"/>'s identical "caller-loaded content, not embedded in the
/// save-state graph" shape.
///
/// Every <see cref="LetterAction"/> this pipeline accepts carries a real, correctly-shaped <see
/// cref="Letter"/> record regardless of whether its own target system exists yet — <see
/// cref="LetterAction.DirectPlacedSpy"/> (Espionage), <see cref="LetterAction.EarlyCourtship"/>
/// (Romance &amp; Seduction), and <see cref="LetterAction.WrittenInstructionsToDistantAppointee"/>
/// (Companions &amp; Court Positions' Procurator) all name systems this codebase has not built yet.
/// This command validates and transits the letter itself; whatever real game-logic payload each of
/// those three actions is ultimately supposed to trigger is that future system's own job to wire up
/// against this already-complete data model, not a fabricated stand-in this item invents now.
/// </summary>
public static class SendLetterCommands
{
    public static readonly ValidationErrorCode DrafterNotFound = new("correspondence.send.drafterNotFound");
    public static readonly ValidationErrorCode DrafterDeceased = new("correspondence.send.drafterDeceased");
    public static readonly ValidationErrorCode RecipientRequired = new("correspondence.send.recipientRequired");
    public static readonly ValidationErrorCode OralTraditionBlocksThisAction = new("correspondence.send.oralTraditionBlocksThisAction");

    public static CommandPipeline<WorldState, SendLetterCommand> BuildPipeline(
        DistanceTierCatalog distanceTiers, CorrespondenceReachabilityCatalog reachability)
    {
        if (distanceTiers is null)
            throw new ArgumentNullException(nameof(distanceTiers));
        if (reachability is null)
            throw new ArgumentNullException(nameof(reachability));

        return new CommandPipeline<WorldState, SendLetterCommand>(
            validate: (state, command) => Validate(state, command, distanceTiers, reachability),
            mutate: (state, command) => Mutate(state, command, distanceTiers, reachability),
            issueSequenceNumber: static state => state.IssueCommandSequenceNumber());
    }

    private static ValidationErrorCode? Validate(
        WorldState state, SendLetterCommand command, DistanceTierCatalog distanceTiers, CorrespondenceReachabilityCatalog reachability)
    {
        if (!state.Characters.TryGet(command.DraftedByCharacterId, out var drafter))
            return DrafterNotFound;
        if (!drafter.IsAlive)
            return DrafterDeceased;
        if (string.IsNullOrWhiteSpace(command.RecipientCharacterOrActorId))
            return RecipientRequired;

        var route = LetterRoute.Resolve(
            command.SenderRegionId, command.RecipientRegionId, command.RecipientCultureId,
            command.Action, command.CourierType, distanceTiers, reachability);
        if (route.Blocked)
            return OralTraditionBlocksThisAction;

        return null;
    }

    private static IDomainEvent[] Mutate(
        WorldState state, SendLetterCommand command, DistanceTierCatalog distanceTiers, CorrespondenceReachabilityCatalog reachability)
    {
        var route = LetterRoute.Resolve(
            command.SenderRegionId, command.RecipientRegionId, command.RecipientCultureId,
            command.Action, command.CourierType, distanceTiers, reachability);

        var letterId = state.LetterIds.Issue();
        var letter = Letter.Begin(
            letterId, LetterDirection.Outbound, command.Action,
            command.DraftedByCharacterId.ToTaggedString(), command.RecipientCharacterOrActorId,
            command.DraftedByCharacterId, route, command.CourierType, command.CourierCharacterId,
            command.SubmittedDate, requiresResponse: false);
        state.Letters.Add(letterId, letter);

        return new IDomainEvent[]
        {
            new LetterSentEvent(
                state.EventIds.Issue(), command.SubmittedDate, letterId, command.DraftedByCharacterId,
                command.RecipientCharacterOrActorId, command.Action, route.TransitTimeMonths,
                command.CommandId.ToTaggedString()),
        };
    }
}
