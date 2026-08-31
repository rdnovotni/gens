using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Legal;
using Gens.Simulation.Random;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Edicts;

/// <summary>Emitted whenever a <see cref="GrantCitizenshipEdictCommand"/> is accepted — §5.1's
/// Declaration. Public, matching <see cref="ManumissionEdictIssuedEvent"/>'s own reasoning.</summary>
public sealed record CitizenshipEdictGrantedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<EdictRecord> EdictId,
    RuntimeId<Household> IssuingHouseholdId,
    RuntimeId<Character> TargetCharacterId,
    string? CausationId) : IDomainEvent
{
    public string Type => "edicts.citizenshipEdictGranted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { IssuingHouseholdId.ToTaggedString(), TargetCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// §5.6's Citizenship Grant — "extends citizenship... to a group or individual — real Social-War-era
/// stakes," narrowed here to a single named Character (this codebase's own group-vs-individual gap:
/// no bulk "grant to everyone in this pop group" primitive exists, matching Phase 12 item 6's own
/// "resolving each one's own lazily-generated head... no principled way" precedent for the identical
/// group-scale gap). Effect: a direct <see cref="Character.LegalStatus"/> mutation to <see
/// cref="LegalStatus.RomanCitizen"/> — no prior command grants citizenship this way, so this is a new,
/// first mutation of that field rather than a reopening of anything already tested — plus a real
/// Dignitas gain for the issuing household. Reception (§5.6: "Traditionalist alarm and a plausible Legal
/// &amp; Court challenge to the grant's own validity") is always a real Scandal, and additionally, when
/// <see cref="ChallengerHouseholdId"/> actually names a household with a resolved head, a real <see
/// cref="FileLawsuitCommand"/> (Political, Quick) contesting the grant — optional and defaulting to
/// null, matching Phase 12 item 8's own <c>EndorsingCelebrityForChallenger</c>/<c>ForIncumbent</c>
/// "both defaulting to null" precedent for an optional real integration this item cannot invent an
/// antagonist household to force. The challenge is always filed at <see
/// cref="LegalCaseDepth.Major"/> rather than Quick: <see cref="FileLawsuitCommands.CreatePipeline"/>
/// requires a <see cref="RandomStreamSet"/> only to roll a Quick case's own inline verdict — a Major
/// filing never touches it (confirmed directly in that command's own <c>Mutate</c>) — so this command
/// passes a fresh, unregistered <see cref="RandomStreamSet"/> rather than threading a real named stream
/// through a static <see cref="CommandPipeline{TState,TCommand}"/> that has nowhere to receive one; a
/// filed challenge instead proceeds through <see cref="LegalCaseAdvancementSystem"/>'s own already-shipped
/// Evidence-Gathering/Hearing progression like any other Major case.
/// </summary>
public sealed record GrantCitizenshipEdictCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> IssuingHouseholdId,
    RuntimeId<Character> TargetCharacterId,
    RuntimeId<Settlement> SettlementId,
    RuntimeId<Household>? ChallengerHouseholdId = null) : ICommand;

/// <summary>The validate/mutate pipeline for <see cref="GrantCitizenshipEdictCommand"/> (ADR 0006).</summary>
public static class GrantCitizenshipEdictCommands
{
    public static readonly ValidationErrorCode InsufficientInfluence = EdictIssuance.InsufficientInfluence;
    public static readonly ValidationErrorCode TargetNotFound = new("edicts.grantCitizenship.targetNotFound");
    public static readonly ValidationErrorCode TargetDeceased = new("edicts.grantCitizenship.targetDeceased");
    public static readonly ValidationErrorCode AlreadyCitizen = new("edicts.grantCitizenship.alreadyCitizen");

    public static readonly CommandPipeline<WorldState, GrantCitizenshipEdictCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, GrantCitizenshipEdictCommand command)
    {
        if (!state.Characters.TryGet(command.TargetCharacterId, out var target))
            return TargetNotFound;
        if (!target!.IsAlive)
            return TargetDeceased;
        if (target.LegalStatus == LegalStatus.RomanCitizen)
            return AlreadyCitizen;

        return EdictIssuance.ValidateAffordability(state, command.IssuingHouseholdId, EdictCatalog.CitizenshipGrantInfluenceCost);
    }

    private static IDomainEvent[] Mutate(WorldState state, GrantCitizenshipEdictCommand command)
    {
        var events = EdictIssuance.ChargeCosts(
            state, command.CommandId, command.ActorId, command.SubmittedDate, command.IssuingHouseholdId,
            EdictCatalog.CitizenshipGrantInfluenceCost, EdictCatalog.CitizenshipGrantDignitasCost,
            "Edict issued: Citizenship Grant");

        state.Characters.TryGet(command.TargetCharacterId, out var target);
        var previousStatus = target!.LegalStatus;
        var granted = target with
        {
            LegalStatus = LegalStatus.RomanCitizen,
            SocialClass = target.SocialClass ?? SocialClass.Plebeian,
        };
        state.Characters.Remove(command.TargetCharacterId);
        state.Characters.Add(command.TargetCharacterId, granted);

        events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
            state, new AdjustDignitasCommand(
                state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                command.IssuingHouseholdId, EdictCatalog.CitizenshipGrantDignitasGain,
                $"Citizenship Grant: {previousStatus} -> {LegalStatus.RomanCitizen}")).Events);

        var (scandalId, receptionEvents) = EdictIssuance.RecordReception(
            state, command.CommandId, command.ActorId, command.SubmittedDate, command.IssuingHouseholdId,
            EdictCatalog.CitizenshipGrantReceptionSeverity);
        events.AddRange(receptionEvents);

        RuntimeId<LegalCase>? legalCaseId = null;
        if (command.ChallengerHouseholdId is { } challengerId && challengerId != command.IssuingHouseholdId &&
            state.HouseholdHeadships.TryGet(challengerId, out var challengerHeadship))
        {
            var lawsuit = FileLawsuitCommands.CreatePipeline(new RandomStreamSet()).Execute(
                state, new FileLawsuitCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    LegalCaseType.Political, LegalCaseDepth.Major, challengerId, command.IssuingHouseholdId,
                    command.SettlementId, challengerHeadship!.HeadCharacterId));
            if (lawsuit.Accepted)
            {
                events.AddRange(lawsuit.Events);
                legalCaseId = lawsuit.Events.OfType<LawsuitFiledEvent>().Single().CaseId;
            }
        }

        var edictId = state.EdictRecordIds.Issue();
        state.EdictRecords.Add(edictId, new EdictRecord(
            edictId, command.IssuingHouseholdId, EdictType.CitizenshipGrant, command.SubmittedDate,
            EdictCatalog.CitizenshipGrantInfluenceCost, EdictCatalog.CitizenshipGrantDignitasCost, scandalId,
            LegalCaseId: legalCaseId));

        events.Add(new CitizenshipEdictGrantedEvent(
            state.EventIds.Issue(), command.SubmittedDate, edictId, command.IssuingHouseholdId, command.TargetCharacterId,
            command.CommandId.ToTaggedString()));

        return events.ToArray();
    }
}
