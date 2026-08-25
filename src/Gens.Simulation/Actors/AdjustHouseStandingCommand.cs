using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Actors;

/// <summary>Which way an <see cref="AdjustHouseStandingCommand"/> nudges a house pair's <see
/// cref="HouseStandingLevel"/> — one step toward <see cref="HouseStandingLevel.Allied"/> or one step
/// toward <see cref="HouseStandingLevel.Feuding"/>, along the fixed Allied→Neutral→Rivalrous→Feuding
/// scale (<c>gens-rival-houses-design.md</c> §5.2).</summary>
public enum HouseStandingAdjustmentDirection
{
    TowardAlliance,
    TowardRivalry,
}

/// <summary>The first concrete Interaction Catalog entry between two <see cref="LivingWorldActor"/>s
/// (Phase 10 item 5; §5.1's "no new interactions — the full Interaction Catalog applies directly").
/// Seeking alliance or declaring rivalry are the only two Interaction Catalog verbs this phase actually
/// implements; the richer catalog (Befriend, Broker Alliance, Endorse/Undermine, Propose Marriage) is
/// unbuilt (Phase 12/17) and out of scope here. Actor-agnostic like every other command in this
/// codebase — a player action and <see cref="RivalAmbitionSystem"/>'s automated choice both submit
/// this exact command through the same <see cref="CommandPipeline{TState,TCommand}"/> (rule 2).</summary>
public sealed record AdjustHouseStandingCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Actor> InitiatorActorId,
    RuntimeId<Actor> TargetActorId,
    HouseStandingAdjustmentDirection Direction) : ICommand;

/// <summary>Emitted whenever an <see cref="AdjustHouseStandingCommand"/> is accepted. Public visibility:
/// house-to-house standing is the kind of fact §7's "Notable Families of the Region" ambient list
/// already treats as generally knowable, not a secret.</summary>
public sealed record HouseStandingChangedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Actor> ActorAId,
    RuntimeId<Actor> ActorBId,
    HouseStandingLevel PreviousStanding,
    HouseStandingLevel NewStanding,
    string? CausationId) : IDomainEvent
{
    public string Type => "actors.houseStandingChanged";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { ActorAId.ToTaggedString(), ActorBId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="AdjustHouseStandingCommand"/> (ADR 0006).</summary>
public static class AdjustHouseStandingCommands
{
    public static readonly ValidationErrorCode SameActor = new("actors.adjustHouseStanding.sameActor");
    public static readonly ValidationErrorCode UnknownActor = new("actors.adjustHouseStanding.unknownActor");
    public static readonly ValidationErrorCode AlreadyAtExtreme = new("actors.adjustHouseStanding.alreadyAtExtreme");

    public static readonly CommandPipeline<WorldState, AdjustHouseStandingCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, AdjustHouseStandingCommand command)
    {
        if (command.InitiatorActorId == command.TargetActorId)
            return SameActor;

        if (!state.Actors.TryGet(command.InitiatorActorId, out _) || !state.Actors.TryGet(command.TargetActorId, out _))
            return UnknownActor;

        var current = HouseStandingResolver.GetEffectiveStanding(state, command.InitiatorActorId, command.TargetActorId);
        if (command.Direction == HouseStandingAdjustmentDirection.TowardAlliance && current == HouseStandingLevel.Allied)
            return AlreadyAtExtreme;
        if (command.Direction == HouseStandingAdjustmentDirection.TowardRivalry && current == HouseStandingLevel.Feuding)
            return AlreadyAtExtreme;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, AdjustHouseStandingCommand command)
    {
        var key = HouseStandingKey.Between(command.InitiatorActorId, command.TargetActorId);
        var previous = HouseStandingResolver.GetEffectiveStanding(state, command.InitiatorActorId, command.TargetActorId);
        var existingGrudge = state.HouseStandings.TryGet(key, out var existing) ? existing!.Grudge : null;

        var next = Step(previous, command.Direction);
        state.HouseStandings.Remove(key);
        state.HouseStandings.Add(key, new HouseStanding(next, existingGrudge));

        return new IDomainEvent[]
        {
            new HouseStandingChangedEvent(
                state.EventIds.Issue(), command.SubmittedDate, key.ActorAId, key.ActorBId, previous, next,
                command.CommandId.ToTaggedString()),
        };
    }

    private static HouseStandingLevel Step(HouseStandingLevel current, HouseStandingAdjustmentDirection direction)
    {
        var delta = direction == HouseStandingAdjustmentDirection.TowardAlliance ? -1 : 1;
        var next = Math.Clamp((int)current + delta, (int)HouseStandingLevel.Allied, (int)HouseStandingLevel.Feuding);
        return (HouseStandingLevel)next;
    }
}
