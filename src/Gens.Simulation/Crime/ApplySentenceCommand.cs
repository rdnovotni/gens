using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Legal;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Crime;

/// <summary>
/// §7/§8: applies one sentence from the catalog against a Character, resolved through §4's own
/// Justified/Unjust lens "scaled up to its natural maximum severity." Per the task's own explicit
/// direction to pick real, reachable resolution paths rather than modeling the entire catalog with
/// nothing behind it, this command actually carries out <see cref="SentenceType.Fine"/> (a real Ledger
/// charge), the exile-equivalent pair <see cref="SentenceType.Relegatio"/>/<see
/// cref="SentenceType.Deportatio"/> (recorded, with Deportatio adding a real property-confiscation
/// charge — <see cref="Legal.LegalCase"/>'s own <c>Exile</c> sentence has no such distinction, this item
/// being the pass that finally gives Legal &amp; Court's own thin sentence list "the real historical
/// depth and breadth it never had room for," per this document's own §1), <see
/// cref="SentenceType.HonorableExit"/> (a real, dignified death, distinct from an ordinary execution),
/// and <see cref="SentenceType.Crucifixion"/> (this item's one real, humiliores-tier Execution path,
/// "played straight" per §7's own restraint). Every other <see cref="SentenceType"/> value is rejected
/// with <see cref="ApplySentenceCommands.SentenceNotYetWired"/> — see that type's own doc comment for
/// why each remaining value is a deliberate, named cut rather than a silent no-op.
/// </summary>
public sealed record ApplySentenceCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    SentenceType Type,
    RuntimeId<Character>? SentencingCharacterId = null,
    RuntimeId<LegalCase>? SourceLegalCaseId = null) : ICommand;

/// <summary>Emitted whenever an <see cref="ApplySentenceCommand"/> is accepted.</summary>
public sealed record SentenceAppliedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<SentenceRecord> SentenceId,
    RuntimeId<Character> CharacterId,
    SentenceTier Tier,
    SentenceType SentenceType,
    bool WasJustified,
    bool ResultedInDeath,
    string? CausationId) : IDomainEvent
{
    public string Type => "crime.sentenceApplied";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="ApplySentenceCommand"/> (ADR 0006).</summary>
public static class ApplySentenceCommands
{
    public static readonly ValidationErrorCode CharacterNotFound = new("crime.applySentence.characterNotFound");
    public static readonly ValidationErrorCode CharacterDeceased = new("crime.applySentence.characterDeceased");

    /// <summary>§7's remaining catalog values (<see cref="SentenceType.Ignominia"/>, <see
    /// cref="SentenceType.Flogging"/>, <see cref="SentenceType.DamnatioAdMetalla"/>, <see
    /// cref="SentenceType.ServusPoenae"/>, <see cref="SentenceType.DamnatioAdBestias"/>) are real,
    /// named-in-the-enum values with no mechanical follow-through yet — see <see cref="SentenceType"/>'s
    /// own doc comment for why each one specifically is deferred.</summary>
    public static readonly ValidationErrorCode SentenceNotYetWired = new("crime.applySentence.sentenceNotYetWired");

    private static readonly LedgerAccountKey FineSink = new(LedgerAccountKind.System, "crime:fines");
    private static readonly LedgerAccountKey ConfiscationSink = new(LedgerAccountKind.System, "crime:confiscations");

    public static readonly CommandPipeline<WorldState, ApplySentenceCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, ApplySentenceCommand command)
    {
        if (!state.Characters.TryGet(command.CharacterId, out var character))
            return CharacterNotFound;
        if (!character!.IsAlive)
            return CharacterDeceased;

        var isWired = command.Type is SentenceType.Fine or SentenceType.Relegatio or SentenceType.Deportatio
            or SentenceType.HonorableExit or SentenceType.Crucifixion;
        if (!isWired)
            return SentenceNotYetWired;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, ApplySentenceCommand command)
    {
        state.Characters.TryGet(command.CharacterId, out var character);
        var tier = SentenceTierResolver.TierFor(character!);
        var justified = PunishableOffenseResolver.HasActiveOffense(state, command.CharacterId);

        var sentenceId = state.SentenceRecordIds.Issue();
        state.SentenceRecords.Add(
            sentenceId,
            new SentenceRecord(sentenceId, command.CharacterId, tier, command.Type, justified, command.SubmittedDate, command.SourceLegalCaseId));

        var events = new List<IDomainEvent>();
        var resultsInDeath = command.Type is SentenceType.HonorableExit or SentenceType.Crucifixion;

        if (resultsInDeath)
        {
            var deathCause = command.Type == SentenceType.HonorableExit ? DeathCause.Unspecified : DeathCause.Violence;
            state.Characters.Remove(command.CharacterId);
            state.Characters.Add(
                command.CharacterId,
                character! with { DeathRecord = new DeathRecord(command.SubmittedDate, deathCause, character.AgeInYears(command.SubmittedDate)) });
        }

        if (command.Type == SentenceType.Fine && character!.Household is { } fineHouseholdId)
        {
            events.Add(LedgerService.Post(
                state, command.SubmittedDate, LedgerTransactionCategory.Gifts,
                new[]
                {
                    new LedgerPosting(LedgerAccountKey.ForHousehold(fineHouseholdId), -CrimeCatalog.FineSentenceAmount),
                    new LedgerPosting(FineSink, CrimeCatalog.FineSentenceAmount),
                },
                reference: $"crime:fine:{command.CommandId.ToTaggedString()}"));
        }

        if (command.Type == SentenceType.Deportatio && character!.Household is { } deportatioHouseholdId)
        {
            events.Add(LedgerService.Post(
                state, command.SubmittedDate, LedgerTransactionCategory.Gifts,
                new[]
                {
                    new LedgerPosting(LedgerAccountKey.ForHousehold(deportatioHouseholdId), -CrimeCatalog.DeportatioPropertyConfiscation),
                    new LedgerPosting(ConfiscationSink, CrimeCatalog.DeportatioPropertyConfiscation),
                },
                reference: $"crime:deportatio:{command.CommandId.ToTaggedString()}"));
        }

        if (character!.Household is { } householdId)
        {
            var penalty = justified ? CrimeCatalog.JustifiedSentenceDignitasPenalty : CrimeCatalog.UnjustSentenceDignitasPenalty;
            events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
                state, new AdjustDignitasCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    householdId, -penalty, $"{command.Type} sentence applied")).Events);
        }

        // §4/§8's own relationship-web scar only has somewhere real to land when a specific sentencing
        // Character is named (an Imprison-and-sentence sequence exercised on personal authority) — a
        // sentence applied straight off a Legal & Court conviction has no single actor Character to
        // scar against (that verdict's own scar already lands household-to-household via <see
        // cref="Legal.LegalCaseRuling"/>), and, in practice, a Legal-conviction-sourced sentence is
        // always Justified in the first place (the conviction itself minted the offense).
        if (!justified && command.SentencingCharacterId is { } sentencingCharacterId && sentencingCharacterId != command.CharacterId)
        {
            events.AddRange(RecordInteractionCommands.Pipeline.Execute(
                state, new RecordInteractionCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    sentencingCharacterId, command.CharacterId, -CrimeCatalog.UnjustSentenceOpinionPenalty,
                    BondTag.Rival, BondTag.None, RelationshipOrigin.Political)).Events);
        }

        events.Add(new SentenceAppliedEvent(
            state.EventIds.Issue(), command.SubmittedDate, sentenceId, command.CharacterId, tier, command.Type,
            justified, resultsInDeath, command.CommandId.ToTaggedString()));

        return events.ToArray();
    }
}
