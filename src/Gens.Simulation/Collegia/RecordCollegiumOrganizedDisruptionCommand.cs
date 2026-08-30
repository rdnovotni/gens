using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Crime;
using Gens.Simulation.Identity;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Collegia;

/// <summary>
/// §6's real, historically-documented darker edge: a household orders its own sponsored Collegium to
/// disrupt a rival household's political standing at settlement scale — "a real, high-risk Coercive
/// Interaction... reading through Crime &amp; Punishment's own Justice Spectrum and Justified/Unjust
/// framework exactly as any other exercise of raw power would." This item deliberately builds only this
/// one <c>CollegiumPoliticalAction</c> type, not §6's other, legitimate half (an election endorsement):
/// §5.5's contested-election machinery (<see cref="Magistracies.HoldContestedElectionCommand"/>)
/// resolves synchronously in one command call with no persisted "election currently open" state for an
/// endorsement to attach to ahead of resolution, so there is nothing here yet for an endorsement to feed
/// into — a real, honest gap distinct from this command's own real, reachable Justified/Unjust check
/// against <see cref="PunishableOffenseResolver"/>. Gated on the instigating household actually
/// sponsoring the collegium (§4's patron relationship is the one real "who controls a collegium's
/// political muscle" tie this codebase has) — an unsponsored collegium cannot be ordered to act.
/// </summary>
public sealed record RecordCollegiumOrganizedDisruptionCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Actor> CollegiumId,
    RuntimeId<Household> TargetHouseholdId) : ICommand;

/// <summary>Emitted whenever a <see cref="RecordCollegiumOrganizedDisruptionCommand"/> is accepted.
/// Public — §6's own framing is that this is never a quiet act: "organized crowds... mobilized... to
/// disrupt a rival's election or assembly" is by nature a visible, settlement-scale event, the same
/// reasoning <see cref="Crime.CharacterImprisonedEvent"/> already gives for "everyone is watching how you
/// use power."</summary>
public sealed record CollegiumOrganizedDisruptionRecordedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Actor> CollegiumId,
    RuntimeId<Household> InstigatingHouseholdId,
    RuntimeId<Household> TargetHouseholdId,
    bool Justified,
    string? CausationId) : IDomainEvent
{
    public string Type => "collegia.organizedDisruptionRecorded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CollegiumId.ToTaggedString(), InstigatingHouseholdId.ToTaggedString(), TargetHouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="RecordCollegiumOrganizedDisruptionCommand"/>
/// (ADR 0006).</summary>
public static class RecordCollegiumOrganizedDisruptionCommands
{
    public static readonly ValidationErrorCode CollegiumNotFound = new("collegia.organizedDisruption.collegiumNotFound");
    public static readonly ValidationErrorCode CollegiumUnsponsored = new("collegia.organizedDisruption.collegiumUnsponsored");
    public static readonly ValidationErrorCode SameHousehold = new("collegia.organizedDisruption.sameHousehold");
    public static readonly ValidationErrorCode TargetHasNoHead = new("collegia.organizedDisruption.targetHasNoHead");
    public static readonly ValidationErrorCode InstigatorHasNoHead = new("collegia.organizedDisruption.instigatorHasNoHead");

    public static readonly CommandPipeline<WorldState, RecordCollegiumOrganizedDisruptionCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, RecordCollegiumOrganizedDisruptionCommand command)
    {
        if (!state.Collegia.TryGet(command.CollegiumId, out var details))
            return CollegiumNotFound;
        if (details!.PatronHouseholdId is not { } patronHouseholdId)
            return CollegiumUnsponsored;
        if (patronHouseholdId == command.TargetHouseholdId)
            return SameHousehold;
        if (!state.HouseholdHeadships.TryGet(command.TargetHouseholdId, out _))
            return TargetHasNoHead;
        if (!state.HouseholdHeadships.TryGet(patronHouseholdId, out _))
            return InstigatorHasNoHead;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, RecordCollegiumOrganizedDisruptionCommand command)
    {
        state.Collegia.TryGet(command.CollegiumId, out var details);
        var patronHouseholdId = details!.PatronHouseholdId!.Value;

        state.HouseholdHeadships.TryGet(patronHouseholdId, out var patronHeadship);
        state.HouseholdHeadships.TryGet(command.TargetHouseholdId, out var targetHeadship);
        var patronHeadId = patronHeadship!.HeadCharacterId;
        var targetHeadId = targetHeadship!.HeadCharacterId;

        var justified = PunishableOffenseResolver.HasActiveOffense(state, targetHeadId);
        var events = new List<IDomainEvent>();

        if (justified)
        {
            events.AddRange(RecordInteractionCommands.Pipeline.Execute(
                state, new RecordInteractionCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    patronHeadId, targetHeadId, -CollegiumCatalog.JustifiedDisruptionOpinionPenalty,
                    BondTag.None, BondTag.None, RelationshipOrigin.Political)).Events);
        }
        else
        {
            events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
                state, new AdjustDignitasCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    patronHouseholdId, -CollegiumCatalog.UnjustDisruptionDignitasPenalty,
                    $"unjust collegium-organized disruption of {command.TargetHouseholdId.ToTaggedString()}")).Events);

            events.AddRange(RecordInteractionCommands.Pipeline.Execute(
                state, new RecordInteractionCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    patronHeadId, targetHeadId, -CollegiumCatalog.UnjustDisruptionOpinionPenalty,
                    BondTag.Nemesis, BondTag.None, RelationshipOrigin.Political)).Events);

            // §7: "a formerly licit one caught using this darker tool once too often" — this
            // implementation treats a single Unjust use as sufficient, rather than inventing an unsized
            // repeated-offense counter the design doc never specifies.
            state.Collegia.Remove(command.CollegiumId);
            state.Collegia.Add(command.CollegiumId, details with { LegalStatus = CollegiumLegalStatus.Illicit });
        }

        events.Add(new CollegiumOrganizedDisruptionRecordedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.CollegiumId, patronHouseholdId,
            command.TargetHouseholdId, justified, command.CommandId.ToTaggedString()));

        return events.ToArray();
    }
}
