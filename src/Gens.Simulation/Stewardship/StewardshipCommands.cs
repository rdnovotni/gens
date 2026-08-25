using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Stewardship;

/// <summary>Creates a new <see cref="StewardshipAssignment"/> for a household (Phase 10 item 2).</summary>
public sealed record AppointStewardshipCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    StewardshipContext Context,
    StewardshipMode Mode,
    RuntimeId<Character>? AppointeeCharacterId,
    IReadOnlyList<CouncilMember>? CouncilMembers,
    RuntimeId<Character>? CouncilHeadCharacterId,
    StewardAutonomyLevel AutonomyLevel) : ICommand;

/// <summary>Changes an active assignment's <see cref="StewardAutonomyLevel"/> (§3: player-adjustable,
/// including remotely mid-absence via Written Instructions — that transport mechanism is Correspondence
/// &amp; Letters' own future concern; this command is the underlying state change either path uses).</summary>
public sealed record ChangeStewardshipAutonomyCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<StewardshipAssignment> AssignmentId,
    StewardAutonomyLevel NewLevel) : ICommand;

/// <summary>Ends an active assignment (Travel return, Regency's natural end, §8's <c>endMonth</c>).</summary>
public sealed record EndStewardshipAssignmentCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<StewardshipAssignment> AssignmentId) : ICommand;

public sealed record StewardshipAssignedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<StewardshipAssignment> AssignmentId,
    RuntimeId<Household> HouseholdId,
    StewardshipContext Context,
    string? CausationId) : IDomainEvent
{
    public string Type => "stewardship.assigned";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), AssignmentId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public sealed record StewardshipAutonomyChangedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<StewardshipAssignment> AssignmentId,
    StewardAutonomyLevel PreviousLevel,
    StewardAutonomyLevel NewLevel,
    string? CausationId) : IDomainEvent
{
    public string Type => "stewardship.autonomyChanged";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { AssignmentId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public sealed record StewardshipEndedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<StewardshipAssignment> AssignmentId,
    RuntimeId<Household> HouseholdId,
    string? CausationId) : IDomainEvent
{
    public string Type => "stewardship.ended";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), AssignmentId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>Emitted alongside <see cref="StewardshipEndedEvent"/> whenever a <see
/// cref="ReturnReport"/> is generated (Phase 10 item 11) — the "dramatic reveal" moment itself when
/// <see cref="ChronicleWorthy"/> is true (§8). Public visibility, matching every other stewardship
/// event: routing this into a per-observer knowledge/staleness model is future integration work — no
/// propagation-from-events mechanism exists anywhere in this codebase yet (<see
/// cref="State.KnowledgeState"/> remains storage-only, per its own doc comment).</summary>
public sealed record ReturnReportGeneratedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<ReturnReport> ReportId,
    RuntimeId<StewardshipAssignment> AssignmentId,
    RuntimeId<Household> HouseholdId,
    bool ChronicleWorthy,
    string? CausationId) : IDomainEvent
{
    public string Type => "stewardship.returnReportGenerated";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), AssignmentId.ToTaggedString(), ReportId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>Validate/mutate pipelines for the three stewardship commands (ADR 0006).</summary>
public static class StewardshipCommands
{
    public static readonly ValidationErrorCode AlreadyHasActiveAssignment = new("stewardship.appoint.alreadyHasActiveAssignment");
    public static readonly ValidationErrorCode SingleStewardRequiresAppointee = new("stewardship.appoint.singleStewardRequiresAppointee");
    public static readonly ValidationErrorCode SingleStewardCannotHaveCouncilSeats = new("stewardship.appoint.singleStewardCannotHaveCouncilSeats");
    public static readonly ValidationErrorCode CouncilRequiresAtLeastOneSeat = new("stewardship.appoint.councilRequiresAtLeastOneSeat");
    public static readonly ValidationErrorCode CouncilCannotHaveASingleAppointee = new("stewardship.appoint.councilCannotHaveASingleAppointee");
    public static readonly ValidationErrorCode CouncilHeadMustBeASeatMember = new("stewardship.appoint.councilHeadMustBeASeatMember");
    public static readonly ValidationErrorCode AssignmentNotFound = new("stewardship.assignmentNotFound");
    public static readonly ValidationErrorCode AssignmentNotActive = new("stewardship.assignmentNotActive");
    public static readonly ValidationErrorCode AutonomyLevelUnchanged = new("stewardship.changeAutonomy.levelUnchanged");

    public static readonly CommandPipeline<WorldState, AppointStewardshipCommand> AppointPipeline = new(
        validate: ValidateAppoint, mutate: MutateAppoint, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    public static readonly CommandPipeline<WorldState, ChangeStewardshipAutonomyCommand> ChangeAutonomyPipeline = new(
        validate: ValidateChangeAutonomy, mutate: MutateChangeAutonomy, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    public static readonly CommandPipeline<WorldState, EndStewardshipAssignmentCommand> EndPipeline = new(
        validate: ValidateEnd, mutate: MutateEnd, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? ValidateAppoint(WorldState state, AppointStewardshipCommand command)
    {
        var hasActiveAssignment = state.StewardshipAssignments.InAscendingOrder()
            .Any(entry => entry.Value.HouseholdId == command.HouseholdId && entry.Value.IsActive);
        if (hasActiveAssignment)
            return AlreadyHasActiveAssignment;

        var members = command.CouncilMembers ?? Array.Empty<CouncilMember>();
        if (command.Mode == StewardshipMode.SingleSteward)
        {
            if (command.AppointeeCharacterId is null)
                return SingleStewardRequiresAppointee;
            if (members.Count > 0)
                return SingleStewardCannotHaveCouncilSeats;
        }
        else
        {
            if (members.Count == 0)
                return CouncilRequiresAtLeastOneSeat;
            if (command.AppointeeCharacterId is not null)
                return CouncilCannotHaveASingleAppointee;
            if (command.CouncilHeadCharacterId is { } headId && members.All(m => m.CharacterId != headId))
                return CouncilHeadMustBeASeatMember;
        }

        return null;
    }

    private static IDomainEvent[] MutateAppoint(WorldState state, AppointStewardshipCommand command)
    {
        var assignmentId = state.StewardshipAssignmentIds.Issue();
        var assignment = StewardshipAssignment.Create(
            assignmentId, command.HouseholdId, command.Context, command.Mode, command.AppointeeCharacterId,
            command.CouncilMembers, command.CouncilHeadCharacterId, command.AutonomyLevel, command.SubmittedDate);
        state.StewardshipAssignments.Add(assignmentId, assignment);

        return new IDomainEvent[]
        {
            new StewardshipAssignedEvent(
                state.EventIds.Issue(), command.SubmittedDate, assignmentId, command.HouseholdId, command.Context,
                command.CommandId.ToTaggedString()),
        };
    }

    private static ValidationErrorCode? ValidateChangeAutonomy(WorldState state, ChangeStewardshipAutonomyCommand command)
    {
        if (!state.StewardshipAssignments.TryGet(command.AssignmentId, out var assignment))
            return AssignmentNotFound;
        if (!assignment!.IsActive)
            return AssignmentNotActive;
        if (assignment.AutonomyLevel == command.NewLevel)
            return AutonomyLevelUnchanged;

        return null;
    }

    private static IDomainEvent[] MutateChangeAutonomy(WorldState state, ChangeStewardshipAutonomyCommand command)
    {
        state.StewardshipAssignments.TryGet(command.AssignmentId, out var existing);
        var previousLevel = existing!.AutonomyLevel;

        state.StewardshipAssignments.Remove(command.AssignmentId);
        state.StewardshipAssignments.Add(command.AssignmentId, existing with { AutonomyLevel = command.NewLevel });

        return new IDomainEvent[]
        {
            new StewardshipAutonomyChangedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.AssignmentId, previousLevel, command.NewLevel,
                command.CommandId.ToTaggedString()),
        };
    }

    private static ValidationErrorCode? ValidateEnd(WorldState state, EndStewardshipAssignmentCommand command)
    {
        if (!state.StewardshipAssignments.TryGet(command.AssignmentId, out var assignment))
            return AssignmentNotFound;
        if (!assignment!.IsActive)
            return AssignmentNotActive;

        return null;
    }

    private static IDomainEvent[] MutateEnd(WorldState state, EndStewardshipAssignmentCommand command)
    {
        state.StewardshipAssignments.TryGet(command.AssignmentId, out var existing);
        var householdId = existing!.HouseholdId;

        state.StewardshipAssignments.Remove(command.AssignmentId);
        state.StewardshipAssignments.Add(command.AssignmentId, existing with { EndDate = command.SubmittedDate });

        // §8: the Return Report is built once, right here, at the moment the assignment ends —
        // ReturnReportGenerator is a pure read over the AutonomousDecisionLog entries already recorded.
        var reportId = state.ReturnReportIds.Issue();
        var report = ReturnReportGenerator.Generate(state, reportId, command.AssignmentId);
        state.ReturnReports.Add(reportId, report);

        var events = new List<IDomainEvent>
        {
            new StewardshipEndedEvent(state.EventIds.Issue(), command.SubmittedDate, command.AssignmentId, householdId, command.CommandId.ToTaggedString()),
            new ReturnReportGeneratedEvent(
                state.EventIds.Issue(), command.SubmittedDate, reportId, command.AssignmentId, householdId, report.ChronicleWorthy,
                command.CommandId.ToTaggedString()),
        };

        return events.ToArray();
    }
}
