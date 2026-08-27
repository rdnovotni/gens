using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Stewardship;
using Gens.Simulation.Time;

namespace Gens.Simulation.Succession;

/// <summary>
/// Which Character (if any) the player is directly controlling right now, distinct from <see
/// cref="HouseholdHeadship"/>'s "who legally heads the household" (Phase 11 item 2;
/// <c>gens-succession-dynasty-design.md</c> §6.2's "worth stating plainly who the player actually
/// plays during this stretch"). A minor head with a non-family Regent still has a nominal
/// <see cref="HouseholdHeadship.HeadCharacterId"/> even though nobody is directly controlled that
/// month — this enum is what actually answers "who does the player play right now".
/// </summary>
public enum PlayerControlMode
{
    /// <summary>The ordinary case (§6.1): the current head themself is directly controlled.</summary>
    DirectHead,

    /// <summary>A minor heir's surviving spouse is holding the estate in trust, and the player
    /// controls that Regent directly (§6.2: "the player controls the Regent directly... a real, if
    /// interim, protagonist, not a spectator").</summary>
    RegentInTrust,

    /// <summary>A minor heir's Regent is a non-family appointee (no surviving spouse, or one who
    /// declined/was unable to serve) — the player has no single character to directly control until
    /// the heir comes of age; the household runs on Steward/Council auto-management alone, exactly
    /// like an away-on-Travel household (§6.2: "this stretch runs on Steward/Council auto-management
    /// alone... the player's real point of contact is the same automation-plus-report pattern").</summary>
    AutoManaged,

    /// <summary>The player's household has no headship left at all (§7.1) — the line has ended.</summary>
    Extinguished,
}

/// <summary>Which Character the player currently controls for one Household (Phase 11 item 2), keyed
/// by household exactly like <see cref="HouseholdHeadship"/>. There is at most one entry across the
/// whole campaign today (one player household), but this is still modeled as a registry keyed by
/// household rather than a bespoke singleton field, reusing the same save/hash/partition-versioning
/// machinery every other <c>WorldState</c> partition already gets instead of inventing a parallel
/// mechanism.</summary>
public sealed record PlayerControlState(
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character>? ControlledCharacterId,
    PlayerControlMode Mode);

/// <summary>Declares a Household the player's own — a one-time, explicit act (mirrors <see
/// cref="EstablishHouseholdHeadCommand"/>'s "established explicitly, not inferred" convention), not
/// something the engine infers from any other state. Only one Household may ever be established this
/// way per campaign (§9's data model has no notion of the player controlling more than one Household
/// at once).</summary>
public sealed record EstablishPlayerControlCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId) : ICommand;

/// <summary>Emitted once, when the player's Household is first established (Phase 11 item 2).</summary>
public sealed record PlayerControlEstablishedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character>? ControlledCharacterId,
    PlayerControlMode Mode,
    string? CausationId) : IDomainEvent
{
    public string Type => "succession.playerControlEstablished";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => ControlledCharacterId is { } controlled
        ? new[] { HouseholdId.ToTaggedString(), controlled.ToTaggedString() }
        : new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>Emitted whenever who (or whether anyone) the player directly controls changes — a
/// handoff to a new head, a Regency starting or ending, or extinction (Phase 11 item 2; §6). Distinct
/// from <see cref="HouseholdHeadTransferredEvent"/>: a headship change and a player-control change
/// often coincide but are not the same fact — a non-family Regency starting changes who is head
/// without changing who is directly controlled (nobody), while it changes <see
/// cref="PlayerControlMode"/> from <see cref="PlayerControlMode.DirectHead"/> to <see
/// cref="PlayerControlMode.AutoManaged"/> without a headship transfer at all.</summary>
public sealed record PlayerControlChangedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character>? PreviousCharacterId,
    RuntimeId<Character>? NewCharacterId,
    PlayerControlMode PreviousMode,
    PlayerControlMode NewMode) : IDomainEvent
{
    public string Type => "succession.playerControlChanged";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds
    {
        get
        {
            var ids = new List<string> { HouseholdId.ToTaggedString() };
            if (PreviousCharacterId is { } previous)
                ids.Add(previous.ToTaggedString());
            if (NewCharacterId is { } next)
                ids.Add(next.ToTaggedString());
            return ids;
        }
    }

    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}

/// <summary>
/// Computes the <see cref="PlayerControlState"/> a Household's current <see cref="HouseholdHeadship"/>
/// and <see cref="StewardshipAssignment"/> state imply (Phase 11 item 2; §6.2). Pure — reads <see
/// cref="WorldState"/> but never mutates it, matching <see cref="HeirEligibilityService"/>'s identical
/// "pure math a system calls into" convention. Shared between <see cref="PlayerControlCommands"/>
/// (the initial computation at establishment) and <see cref="PlayerControlHandoffSystem"/> (the
/// monthly recomputation), so the branching logic that decides §6.2's four modes lives in exactly one
/// place.
/// </summary>
internal static class PlayerControlResolver
{
    internal static PlayerControlState Resolve(WorldState state, RuntimeId<Household> householdId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        if (!state.HouseholdHeadships.TryGet(householdId, out var headship))
            return new PlayerControlState(householdId, ControlledCharacterId: null, PlayerControlMode.Extinguished);

        if (headship!.RegentCharacterId is null)
            return new PlayerControlState(householdId, headship.HeadCharacterId, PlayerControlMode.DirectHead);

        var hasActiveRegencyAssignment = state.StewardshipAssignments.InAscendingOrder().Any(entry =>
            entry.Value.HouseholdId == householdId && entry.Value.IsActive && entry.Value.Context == StewardshipContext.Regency);

        return hasActiveRegencyAssignment
            ? new PlayerControlState(householdId, ControlledCharacterId: null, PlayerControlMode.AutoManaged)
            : new PlayerControlState(householdId, headship.RegentCharacterId, PlayerControlMode.RegentInTrust);
    }
}

/// <summary>The validate/mutate pipeline for <see cref="EstablishPlayerControlCommand"/> (ADR 0006).</summary>
public static class PlayerControlCommands
{
    public static readonly ValidationErrorCode HouseholdHasNoHead = new("succession.establishPlayerControl.householdHasNoHead");
    public static readonly ValidationErrorCode AlreadyEstablished = new("succession.establishPlayerControl.alreadyEstablished");

    public static readonly CommandPipeline<WorldState, EstablishPlayerControlCommand> EstablishPipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, EstablishPlayerControlCommand command)
    {
        if (!state.HouseholdHeadships.TryGet(command.HouseholdId, out _))
            return HouseholdHasNoHead;
        if (state.PlayerControls.InAscendingOrder().Any())
            return AlreadyEstablished;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, EstablishPlayerControlCommand command)
    {
        var initial = PlayerControlResolver.Resolve(state, command.HouseholdId);
        state.PlayerControls.Add(command.HouseholdId, initial);

        return new IDomainEvent[]
        {
            new PlayerControlEstablishedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, initial.ControlledCharacterId,
                initial.Mode, command.CommandId.ToTaggedString()),
        };
    }
}
