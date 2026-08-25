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

    /// <summary>Reconciliation is blocked while an active <see cref="AncestralGrudge"/> stands between
    /// the pair (§5.2's "can keep houses Rivalrous for generations") — the only thing that clears it is
    /// time, via <see cref="AncestralGrudgeCatalog.IsActive"/> eventually returning false, never a
    /// direct command.</summary>
    public static readonly ValidationErrorCode BlockedByAncestralGrudge = new("actors.adjustHouseStanding.blockedByAncestralGrudge");

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

        if (command.Direction == HouseStandingAdjustmentDirection.TowardAlliance)
        {
            var key = HouseStandingKey.Between(command.InitiatorActorId, command.TargetActorId);
            if (state.HouseStandings.TryGet(key, out var existing) && existing!.Grudge is { } grudge &&
                AncestralGrudgeCatalog.IsActive(command.SubmittedDate, grudge))
                return BlockedByAncestralGrudge;
        }

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, AdjustHouseStandingCommand command)
    {
        var key = HouseStandingKey.Between(command.InitiatorActorId, command.TargetActorId);
        var previous = HouseStandingResolver.GetEffectiveStanding(state, command.InitiatorActorId, command.TargetActorId);
        var existingGrudge = state.HouseStandings.TryGet(key, out var existing) ? existing!.Grudge : null;

        var next = Step(previous, command.Direction);

        // Reaching Feuding for the first time is this codebase's stand-in for §5.2's "a Feud resolving
        // in Catastrophic Defeat" trigger — no Military & Combat engagement record exists yet (Phase
        // 16) to distinguish an ordinary Feud from a catastrophic one, so the standing transition itself
        // is what originates the grudge. Validate's AlreadyAtExtreme guard means this branch only ever
        // runs on the actual transition into Feuding, never while already there.
        var grudge = next == HouseStandingLevel.Feuding
            ? existingGrudge ?? new AncestralGrudge(command.CommandId.ToTaggedString(), command.SubmittedDate)
            : existingGrudge;

        state.HouseStandings.Remove(key);
        state.HouseStandings.Add(key, new HouseStanding(next, grudge));

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
