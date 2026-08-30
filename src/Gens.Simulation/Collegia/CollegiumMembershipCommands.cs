using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Collegia;

/// <summary>Joins a household onto a Collegium's roster (Phase 12 item 6; §2). A household can belong
/// to more than one Collegium at once (§2's own "layered, overlapping affiliation") — this command has
/// no exclusivity check against any other membership.</summary>
public sealed record JoinCollegiumCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Actor> CollegiumId,
    RuntimeId<Household> HouseholdId) : ICommand;

public sealed record LeaveCollegiumCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Actor> CollegiumId,
    RuntimeId<Household> HouseholdId) : ICommand;

/// <summary>Emitted whenever a <see cref="JoinCollegiumCommand"/> or <see cref="LeaveCollegiumCommand"/>
/// is accepted. Public — a Collegium's roster is a real, legible fact about the institution, the same
/// reasoning <see cref="CollegiumFoundedEvent"/> already gives for the institution's own existence.</summary>
public sealed record CollegiumMembershipChangedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Actor> CollegiumId,
    RuntimeId<Household> HouseholdId,
    bool Joined,
    string? CausationId) : IDomainEvent
{
    public string Type => "collegia.membershipChanged";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CollegiumId.ToTaggedString(), HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipelines for <see cref="JoinCollegiumCommand"/>/<see
/// cref="LeaveCollegiumCommand"/> (ADR 0006).</summary>
public static class CollegiumMembershipCommands
{
    public static readonly ValidationErrorCode CollegiumNotFound = new("collegia.membership.collegiumNotFound");
    public static readonly ValidationErrorCode HouseholdHasNoHead = new("collegia.membership.householdHasNoHead");
    public static readonly ValidationErrorCode AlreadyMember = new("collegia.membership.alreadyMember");
    public static readonly ValidationErrorCode NotAMember = new("collegia.membership.notAMember");

    public static readonly CommandPipeline<WorldState, JoinCollegiumCommand> JoinPipeline = new(
        validate: ValidateJoin,
        mutate: MutateJoin,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    public static readonly CommandPipeline<WorldState, LeaveCollegiumCommand> LeavePipeline = new(
        validate: ValidateLeave,
        mutate: MutateLeave,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? ValidateJoin(WorldState state, JoinCollegiumCommand command)
    {
        if (!state.Collegia.TryGet(command.CollegiumId, out var details))
            return CollegiumNotFound;
        if (!state.HouseholdHeadships.TryGet(command.HouseholdId, out _))
            return HouseholdHasNoHead;
        if (details!.MemberHouseholdIds.Contains(command.HouseholdId))
            return AlreadyMember;

        return null;
    }

    private static IDomainEvent[] MutateJoin(WorldState state, JoinCollegiumCommand command)
    {
        state.Collegia.TryGet(command.CollegiumId, out var details);
        var members = details!.MemberHouseholdIds.Append(command.HouseholdId).ToArray();
        state.Collegia.Remove(command.CollegiumId);
        state.Collegia.Add(command.CollegiumId, details with { MemberHouseholdIds = members });

        return new IDomainEvent[]
        {
            new CollegiumMembershipChangedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CollegiumId, command.HouseholdId,
                Joined: true, command.CommandId.ToTaggedString()),
        };
    }

    private static ValidationErrorCode? ValidateLeave(WorldState state, LeaveCollegiumCommand command)
    {
        if (!state.Collegia.TryGet(command.CollegiumId, out var details))
            return CollegiumNotFound;
        if (!details!.MemberHouseholdIds.Contains(command.HouseholdId))
            return NotAMember;

        return null;
    }

    private static IDomainEvent[] MutateLeave(WorldState state, LeaveCollegiumCommand command)
    {
        state.Collegia.TryGet(command.CollegiumId, out var details);
        var members = details!.MemberHouseholdIds.Where(id => id != command.HouseholdId).ToArray();
        state.Collegia.Remove(command.CollegiumId);
        state.Collegia.Add(command.CollegiumId, details with { MemberHouseholdIds = members });

        return new IDomainEvent[]
        {
            new CollegiumMembershipChangedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CollegiumId, command.HouseholdId,
                Joined: false, command.CommandId.ToTaggedString()),
        };
    }
}
