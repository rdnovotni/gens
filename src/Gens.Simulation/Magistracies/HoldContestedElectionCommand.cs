using Gens.Simulation.Characters;
using Gens.Simulation.Clientela;
using Gens.Simulation.Commands;
using Gens.Simulation.Fame;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Magistracies;

/// <summary>
/// Resolves a contested election for <see cref="MagistracyOffice.Aedile"/>, <see
/// cref="MagistracyOffice.QuaestorLocal"/>, or <see cref="MagistracyOffice.Duumvir"/> (Phase 12 item 2;
/// §5.5) — "the player against an opposing Character... resolved as a weighted comparison." The
/// challenger must already hold an active Decurion seat at the same settlement (§5.1: Decurion "is the
/// gate that makes every office below available to contest"); §5.7's unchallenged-renewal path and
/// Decurion's own non-contested entry (<see cref="AppointDecurionCommand"/>) are the two paths this
/// command deliberately does not cover.
///
/// Score = <see cref="Character.GetEffectiveAttributes"/>'s Diplomacy (this implementation's own pick
/// for §5.5's unnamed "relevant Core Attribute" — see <see
/// cref="MagistracyCatalog.FactionAlignmentBonus"/>'s doc comment for the identical kind of judgment
/// call) + the candidate's household Dignitas + Influence actually spent this election, plus §5.5's
/// "soft thumb on the scale": a flat bonus when the candidate's <see cref="PoliticalFaction"/> matches
/// the settlement's Curia majority Faction, and (Phase 12 item 8) a second, independent flat bonus when
/// <paramref name="EndorsingCelebrityForChallenger"/>/<paramref name="EndorsingCelebrityForIncumbent"/>
/// names a Character whose own <see cref="Fame.CharacterFame"/> clears <see
/// cref="Fame.FameCatalog.EndorsementFameThreshold"/> — <c>gens-celebrities-influential-figures-design.md</c>
/// §5's own "a crowd that loves a famous charioteer is a crowd more receptive to whichever candidate
/// that charioteer is seen publicly favoring," the direct individual-scale complement to the
/// Faction-alignment bonus already above it. Both endorsement parameters default to <c>null</c> — an
/// election submitted the way every already-shipped Phase 12 item 2 test still submits one behaves
/// identically to before this item, exercising nothing new. The higher score wins outright — no
/// coin-flip tiebreak is specified or needed, since Influence spent (an integer the caller freely
/// chooses) makes an exact tie vanishingly unlikely, and a genuine tie deterministically favors the
/// incumbent (or, for an open seat, the challenger) as the simplest stable default.
///
/// <b>Scope note:</b> rival-candidate generation (§3's "a contested election... surfaces its opposing
/// figure as a Character") is deliberately not built into this command — <paramref
/// name="ChallengerCharacterId"/>/<paramref name="IncumbentCharacterId"/> are always already-resolved
/// Character ids the caller supplies, exactly as <see cref="Actors.LivingWorldActorHeadGenerator"/> and
/// <see cref="Characters.PromoteToNamedCommand"/> already generate/promote a Character for their own
/// callers rather than embedding generation inline. A UI or AI layer wanting a fresh rival reuses one of
/// those two generators (or promotes a Curiales pop-group member per §3's own sourcing rule) before
/// submitting this command — this command's own job is resolving the contest, not sourcing its cast.
/// Undermining a rival candidate via a Scheme (§9) is a separate, later hook onto this same command's
/// score inputs, not built here (see the roadmap doc's own item-2 scope notes).
/// </summary>
public sealed record HoldContestedElectionCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    MagistracyOffice Office,
    RuntimeId<Settlement> SettlementId,
    RuntimeId<Character>? IncumbentCharacterId,
    RuntimeId<Character> ChallengerCharacterId,
    int InfluenceSpentByChallenger,
    int InfluenceSpentByIncumbent,
    RuntimeId<Character>? EndorsingCelebrityForChallenger = null,
    RuntimeId<Character>? EndorsingCelebrityForIncumbent = null) : ICommand;

/// <summary>Emitted whenever a <see cref="HoldContestedElectionCommand"/> is accepted. Public, matching
/// <see cref="MagistracyAssumedEvent"/> — an election's outcome is exactly the kind of Curia-legible
/// fact that command's own doc comment already argues office-holding is.</summary>
public sealed record ElectionResolvedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    MagistracyOffice Office,
    RuntimeId<Settlement> SettlementId,
    RuntimeId<Character> WinnerId,
    RuntimeId<Character>? LoserId,
    int WinnerScore,
    int LoserScore,
    string? CausationId) : IDomainEvent
{
    public string Type => "magistracies.electionResolved";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => LoserId is { } loser
        ? new[] { WinnerId.ToTaggedString(), loser.ToTaggedString() }
        : new[] { WinnerId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="HoldContestedElectionCommand"/> (ADR 0006).</summary>
public static class HoldContestedElectionCommands
{
    public static readonly ValidationErrorCode NotAContestableOffice = new("magistracies.holdElection.notAContestableOffice");
    public static readonly ValidationErrorCode ChallengerNotFound = new("magistracies.holdElection.challengerNotFound");
    public static readonly ValidationErrorCode ChallengerDeceased = new("magistracies.holdElection.challengerDeceased");
    public static readonly ValidationErrorCode ChallengerNotADecurion = new("magistracies.holdElection.challengerNotADecurion");
    public static readonly ValidationErrorCode IncumbentNotFound = new("magistracies.holdElection.incumbentNotFound");
    public static readonly ValidationErrorCode IncumbentDeceased = new("magistracies.holdElection.incumbentDeceased");
    public static readonly ValidationErrorCode SameCandidate = new("magistracies.holdElection.sameCandidate");
    public static readonly ValidationErrorCode NegativeInfluenceSpend = new("magistracies.holdElection.negativeInfluenceSpend");
    public static readonly ValidationErrorCode InsufficientInfluence = new("magistracies.holdElection.insufficientInfluence");

    public static readonly CommandPipeline<WorldState, HoldContestedElectionCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, HoldContestedElectionCommand command)
    {
        if (command.Office == MagistracyOffice.Decurion)
            return NotAContestableOffice;
        if (command.InfluenceSpentByChallenger < 0 || command.InfluenceSpentByIncumbent < 0)
            return NegativeInfluenceSpend;
        if (!state.Characters.TryGet(command.ChallengerCharacterId, out var challenger))
            return ChallengerNotFound;
        if (!challenger!.IsAlive)
            return ChallengerDeceased;
        if (MagistracyResolver.ActiveRecord(state, command.SettlementId, MagistracyOffice.Decurion, command.ChallengerCharacterId) is null)
            return ChallengerNotADecurion;
        if (command.ChallengerCharacterId == command.IncumbentCharacterId)
            return SameCandidate;

        if (command.IncumbentCharacterId is { } incumbentId)
        {
            if (!state.Characters.TryGet(incumbentId, out var incumbent))
                return IncumbentNotFound;
            if (!incumbent!.IsAlive)
                return IncumbentDeceased;
        }

        if (command.InfluenceSpentByChallenger > InfluenceOf(state, challenger))
            return InsufficientInfluence;
        if (command.IncumbentCharacterId is { } incId && state.Characters.TryGet(incId, out var inc) &&
            command.InfluenceSpentByIncumbent > InfluenceOf(state, inc))
            return InsufficientInfluence;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, HoldContestedElectionCommand command)
    {
        state.Characters.TryGet(command.ChallengerCharacterId, out var challenger);

        if (challenger!.Household is { } challengerHousehold && command.InfluenceSpentByChallenger > 0)
            InfluenceResolver.Apply(state, challengerHousehold, -command.InfluenceSpentByChallenger);

        Character? incumbent = null;
        if (command.IncumbentCharacterId is { } incumbentId)
        {
            state.Characters.TryGet(incumbentId, out incumbent);
            if (incumbent!.Household is { } incumbentHousehold && command.InfluenceSpentByIncumbent > 0)
                InfluenceResolver.Apply(state, incumbentHousehold, -command.InfluenceSpentByIncumbent);
        }

        var challengerScore = Score(state, command.SettlementId, challenger, command.InfluenceSpentByChallenger, command.EndorsingCelebrityForChallenger);
        var incumbentScore = incumbent is null
            ? -1
            : Score(state, command.SettlementId, incumbent, command.InfluenceSpentByIncumbent, command.EndorsingCelebrityForIncumbent);

        var challengerWins = incumbent is null || challengerScore > incumbentScore;
        var winner = challengerWins ? command.ChallengerCharacterId : command.IncumbentCharacterId!.Value;
        var loser = challengerWins ? command.IncumbentCharacterId : command.ChallengerCharacterId;
        var winnerScore = challengerWins ? challengerScore : incumbentScore;
        var loserScore = challengerWins ? incumbentScore : challengerScore;

        var events = new List<IDomainEvent>();

        // A losing incumbent's seat ends here; a losing challenger simply doesn't gain one, so there's
        // nothing to end for them.
        if (challengerWins && command.IncumbentCharacterId is { } endedIncumbentId)
        {
            var incumbentRecord = MagistracyResolver.ActiveRecord(state, command.SettlementId, command.Office, endedIncumbentId);
            if (incumbentRecord is not null)
            {
                state.MagistracyRecords.Remove(incumbentRecord.RecordId);
                state.MagistracyRecords.Add(
                    incumbentRecord.RecordId,
                    incumbentRecord with { TermEndDate = command.SubmittedDate, LossReason = MagistracyLossReason.LostReelection });
            }
        }

        if (challengerWins)
        {
            var recordId = state.MagistracyRecordIds.Issue();
            state.MagistracyRecords.Add(
                recordId,
                new MagistracyRecord(recordId, command.ChallengerCharacterId, command.Office, command.SettlementId, command.SubmittedDate));
            events.Add(new MagistracyAssumedEvent(
                state.EventIds.Issue(), command.SubmittedDate, recordId, command.ChallengerCharacterId, command.Office,
                command.SettlementId, command.CommandId.ToTaggedString()));
        }

        events.Add(new ElectionResolvedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.Office, command.SettlementId, winner, loser,
            winnerScore, loserScore, command.CommandId.ToTaggedString()));

        return events.ToArray();
    }

    private static int InfluenceOf(WorldState state, Character? character) =>
        character?.Household is { } householdId ? InfluenceResolver.Current(state, householdId) : 0;

    private static int Score(
        WorldState state, RuntimeId<Settlement> settlementId, Character candidate, int influenceSpent,
        RuntimeId<Character>? endorsingCelebrity)
    {
        var attributeScore = candidate.GetEffectiveAttributes().Diplomacy;
        var dignitas = candidate.Household is { } householdId ? DignitasResolver.Current(state, householdId) : 0;
        var score = attributeScore + dignitas + influenceSpent;

        var candidateFaction = CharacterFactionResolver.Current(state, candidate.Id);
        if (candidateFaction is not null && candidateFaction == CuriaMajorityFaction(state, settlementId))
            score += MagistracyCatalog.FactionAlignmentBonus;

        if (endorsingCelebrity is { } endorserId && FameResolver.Current(state, endorserId) >= FameCatalog.EndorsementFameThreshold)
            score += FameCatalog.EndorsementScoreBonus;

        return score;
    }

    private static PoliticalFaction? CuriaMajorityFaction(WorldState state, RuntimeId<Settlement> settlementId)
    {
        var traditionalist = 0;
        var popularist = 0;
        foreach (var seat in MagistracyResolver.ActiveCuriaSeats(state, settlementId))
        {
            switch (CharacterFactionResolver.Current(state, seat.HolderId))
            {
                case PoliticalFaction.Traditionalist:
                    traditionalist++;
                    break;
                case PoliticalFaction.Popularist:
                    popularist++;
                    break;
            }
        }

        if (traditionalist == popularist)
            return null;
        return traditionalist > popularist ? PoliticalFaction.Traditionalist : PoliticalFaction.Popularist;
    }
}
