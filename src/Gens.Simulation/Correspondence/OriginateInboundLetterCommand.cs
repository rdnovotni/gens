using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Regions;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using Gens.Simulation.Travel;

namespace Gens.Simulation.Correspondence;

/// <summary>Starts an inbound <see cref="Letter"/> in transit — §6's Inbox: "other Living World Actors
/// ... can send a letter to the player." This command is the entry point whatever future system
/// actually decides a patron/rival/client should write to the player (an Actor AI decision system, a
/// scripted content trigger, or a test/debug tool) calls; deciding *when* an NPC should write is
/// explicitly out of this item's scope (§12's own open "Inbox volume and pacing" question) — this item
/// only builds the mechanism a decision like that would drive. <see cref="ICommand.ActorId"/> is
/// expected to be the reserved <c>"system"</c> sentinel per that interface's own doc comment, since no
/// single player-controlled Character originates another Actor's own mail.</summary>
public sealed record OriginateInboundLetterCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    string SenderCharacterOrActorId,
    RuntimeId<Character> RecipientCharacterId,
    LetterAction Action,
    CourierType CourierType,
    DefinitionId<RegionProfileDefinition> SenderRegionId,
    DefinitionId<RegionProfileDefinition> RecipientRegionId,
    DefinitionId<Culture>? SenderCultureId,
    bool RequiresResponse) : ICommand;

/// <summary>Emitted whenever an <see cref="OriginateInboundLetterCommand"/> is accepted.</summary>
public sealed record LetterOriginatedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Letter> LetterId,
    string SenderCharacterOrActorId,
    RuntimeId<Character> RecipientCharacterId,
    LetterAction Action,
    int TransitTimeMonths,
    string? CausationId) : IDomainEvent
{
    public string Type => "correspondence.originated";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { RecipientCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="OriginateInboundLetterCommand"/> (ADR 0006).</summary>
public static class OriginateInboundLetterCommands
{
    public static readonly ValidationErrorCode RecipientNotFound = new("correspondence.originate.recipientNotFound");
    public static readonly ValidationErrorCode RecipientDeceased = new("correspondence.originate.recipientDeceased");
    public static readonly ValidationErrorCode SenderRequired = new("correspondence.originate.senderRequired");
    public static readonly ValidationErrorCode OralTraditionBlocksThisAction = new("correspondence.originate.oralTraditionBlocksThisAction");

    public static CommandPipeline<WorldState, OriginateInboundLetterCommand> BuildPipeline(
        DistanceTierCatalog distanceTiers, CorrespondenceReachabilityCatalog reachability)
    {
        if (distanceTiers is null)
            throw new ArgumentNullException(nameof(distanceTiers));
        if (reachability is null)
            throw new ArgumentNullException(nameof(reachability));

        return new CommandPipeline<WorldState, OriginateInboundLetterCommand>(
            validate: (state, command) => Validate(state, command, distanceTiers, reachability),
            mutate: (state, command) => Mutate(state, command, distanceTiers, reachability),
            issueSequenceNumber: static state => state.IssueCommandSequenceNumber());
    }

    private static ValidationErrorCode? Validate(
        WorldState state, OriginateInboundLetterCommand command, DistanceTierCatalog distanceTiers, CorrespondenceReachabilityCatalog reachability)
    {
        if (!state.Characters.TryGet(command.RecipientCharacterId, out var recipient))
            return RecipientNotFound;
        if (!recipient.IsAlive)
            return RecipientDeceased;
        if (string.IsNullOrWhiteSpace(command.SenderCharacterOrActorId))
            return SenderRequired;

        var route = LetterRoute.Resolve(
            command.SenderRegionId, command.RecipientRegionId, command.SenderCultureId,
            command.Action, command.CourierType, distanceTiers, reachability);
        if (route.Blocked)
            return OralTraditionBlocksThisAction;

        return null;
    }

    private static IDomainEvent[] Mutate(
        WorldState state, OriginateInboundLetterCommand command, DistanceTierCatalog distanceTiers, CorrespondenceReachabilityCatalog reachability)
    {
        var route = LetterRoute.Resolve(
            command.SenderRegionId, command.RecipientRegionId, command.SenderCultureId,
            command.Action, command.CourierType, distanceTiers, reachability);

        var letterId = state.LetterIds.Issue();
        var letter = Letter.Begin(
            letterId, LetterDirection.Inbound, command.Action,
            command.SenderCharacterOrActorId, command.RecipientCharacterId.ToTaggedString(),
            draftedByCharacterId: null, route, command.CourierType, courierCharacterId: null,
            command.SubmittedDate, command.RequiresResponse);
        state.Letters.Add(letterId, letter);

        return new IDomainEvent[]
        {
            new LetterOriginatedEvent(
                state.EventIds.Issue(), command.SubmittedDate, letterId, command.SenderCharacterOrActorId,
                command.RecipientCharacterId, command.Action, route.TransitTimeMonths,
                command.CommandId.ToTaggedString()),
        };
    }
}
