using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Collegia;

/// <summary>Seats a Character as a Collegium's Magister (Phase 12 item 6; §3, §9) — deliberately no
/// citizenship or Legal Status gate, the one sharp contrast with <see
/// cref="Magistracies.AppointDecurionCommand"/>'s identical-shaped citizenship check: §9's whole point
/// is that collegium leadership is "a real, genuine, respected achievement" precisely for a Freedman or
/// Peregrine categorically excluded from the Curia and the cursus honorum, so this command gates on
/// nothing beyond the candidate being a real, living Character. Written directly onto the underlying
/// <see cref="LivingWorldActor.HeadCharacterId"/> (replace-in-place, matching <see
/// cref="BackgroundHouseDriftSystem"/>'s identical remove-then-re-add convention) rather than a second,
/// parallel "head" field — the Magister <i>is</i> this actor's head, per <see
/// cref="CollegiumDetails"/>'s own doc comment.</summary>
public sealed record ElectMagisterCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Actor> CollegiumId,
    RuntimeId<Character> CandidateId) : ICommand;

/// <summary>Appoints a Collegium's Quinquennalis (Phase 12 item 6; §3) — the census-cycle membership/
/// financial officer. §12's own open question notes the real census-cycle trigger this game's monthly
/// tick has no equivalent for; this command is direct appointment only, with no automatic monthly
/// re-appointment or term.</summary>
public sealed record AppointQuinquennalisCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Actor> CollegiumId,
    RuntimeId<Character> CandidateId) : ICommand;

/// <summary>Emitted whenever an <see cref="ElectMagisterCommand"/> or <see
/// cref="AppointQuinquennalisCommand"/> is accepted. Public — an officer's own standing within the
/// collegium is real and legible, the same reasoning <see cref="Magistracies.MagistracyAssumedEvent"/>
/// already gives for a Local Magistracy seat (§9's own "a different ladder, not a lesser one").</summary>
public sealed record CollegiumOfficerAppointedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Actor> CollegiumId,
    RuntimeId<Character> CandidateId,
    bool IsMagister,
    string? CausationId) : IDomainEvent
{
    public string Type => "collegia.officerAppointed";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CollegiumId.ToTaggedString(), CandidateId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipelines for <see cref="ElectMagisterCommand"/>/<see
/// cref="AppointQuinquennalisCommand"/> (ADR 0006).</summary>
public static class CollegiumOfficerCommands
{
    public static readonly ValidationErrorCode CollegiumNotFound = new("collegia.officer.collegiumNotFound");
    public static readonly ValidationErrorCode CandidateNotFound = new("collegia.officer.candidateNotFound");
    public static readonly ValidationErrorCode CandidateDeceased = new("collegia.officer.candidateDeceased");

    public static readonly CommandPipeline<WorldState, ElectMagisterCommand> ElectMagisterPipeline = new(
        validate: ValidateMagister,
        mutate: MutateMagister,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    public static readonly CommandPipeline<WorldState, AppointQuinquennalisCommand> AppointQuinquennalisPipeline = new(
        validate: ValidateQuinquennalis,
        mutate: MutateQuinquennalis,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? ValidateCandidate(WorldState state, RuntimeId<Actor> collegiumId, RuntimeId<Character> candidateId)
    {
        if (!state.Collegia.TryGet(collegiumId, out _))
            return CollegiumNotFound;
        if (!state.Characters.TryGet(candidateId, out var candidate))
            return CandidateNotFound;
        if (!candidate!.IsAlive)
            return CandidateDeceased;

        return null;
    }

    private static ValidationErrorCode? ValidateMagister(WorldState state, ElectMagisterCommand command) =>
        ValidateCandidate(state, command.CollegiumId, command.CandidateId);

    private static IDomainEvent[] MutateMagister(WorldState state, ElectMagisterCommand command)
    {
        state.Actors.TryGet(command.CollegiumId, out var actor);
        state.Actors.Remove(command.CollegiumId);
        state.Actors.Add(command.CollegiumId, actor! with { HeadCharacterId = command.CandidateId });

        return new IDomainEvent[]
        {
            new CollegiumOfficerAppointedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CollegiumId, command.CandidateId,
                IsMagister: true, command.CommandId.ToTaggedString()),
        };
    }

    private static ValidationErrorCode? ValidateQuinquennalis(WorldState state, AppointQuinquennalisCommand command) =>
        ValidateCandidate(state, command.CollegiumId, command.CandidateId);

    private static IDomainEvent[] MutateQuinquennalis(WorldState state, AppointQuinquennalisCommand command)
    {
        state.Collegia.TryGet(command.CollegiumId, out var details);
        state.Collegia.Remove(command.CollegiumId);
        state.Collegia.Add(command.CollegiumId, details! with { QuinquennalisCharacterId = command.CandidateId });

        return new IDomainEvent[]
        {
            new CollegiumOfficerAppointedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CollegiumId, command.CandidateId,
                IsMagister: false, command.CommandId.ToTaggedString()),
        };
    }
}
