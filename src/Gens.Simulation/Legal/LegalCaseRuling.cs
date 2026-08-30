using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Magistracies;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Legal;

/// <summary>Emitted once a <see cref="LegalCase"/> reaches <see cref="LegalCaseStage.Ruled"/> — from
/// either <see cref="FileLawsuitCommand"/>'s own inline Quick Resolution or <see
/// cref="LegalCaseAdvancementSystem"/>'s Major-case Hearing. Public, matching <see
/// cref="FileLawsuitCommand"/>'s own "a formal, on-the-record civic act" precedent for every other event
/// this domain emits.</summary>
public sealed record LegalCaseRuledEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<LegalCase> CaseId,
    LegalCaseType CaseType,
    RuntimeId<Household> PlaintiffId,
    RuntimeId<Household> DefendantId,
    LegalCaseVerdict Verdict,
    LegalSentence? Sentence,
    bool IsPatriaPotestasCase,
    string? CausationId) : IDomainEvent
{
    public string Type => "legal.caseRuled";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { PlaintiffId.ToTaggedString(), DefendantId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// §9's shared "verdict lands, consequences ripple outward" logic — the one place both <see
/// cref="FileLawsuitCommand"/>'s Quick Resolution and <see cref="LegalCaseAdvancementSystem"/>'s Major
/// Hearing apply a rolled verdict, rather than each reimplementing §9's own consequence table. Every
/// consequence routes through this codebase's already-established shared command paths (rule 2) — <see
/// cref="AdjustDignitasCommand"/> for the Dignitas shift, <see cref="RecordInteractionCommand"/> for the
/// relationship-web scar, <see cref="EndMagistracyForConvictionCommand"/> for a Political conviction's
/// loss of office (finally minting <see cref="MagistracyLossReason.LegalConviction"/>, kept modeled-but-
/// unreachable by Phase 12 item 2's own doc comment until this item existed to wire it) — rather than
/// this domain poking any of those partitions directly.
///
/// §6's Patria Potestas scope cut: the household-level party simplification (<see cref="LegalCase"/>'s
/// own doc comment) means the Scandal-Marked <see cref="Trait"/> lands on the defendant household's own
/// recorded head (<see cref="Succession.HouseholdHeadship"/>) rather than the specific dependent the
/// authority was exercised against — the same "no Character-level standing to target instead" limitation
/// that flag already accepts.
/// </summary>
internal static class LegalCaseRuling
{
    private static readonly LedgerAccountKey FineSink = new(LedgerAccountKind.System, "legal:fines");

    public static IReadOnlyList<IDomainEvent> Apply(
        WorldState state, LegalCase legalCase, LegalCaseVerdict verdict, LegalSentence? sentence,
        GameDate date, string? causationId)
    {
        var events = new List<IDomainEvent>();

        state.LegalCases.Remove(legalCase.CaseId);
        var ruled = legalCase with { Stage = LegalCaseStage.Ruled, Verdict = verdict, Sentence = sentence, RuledDate = date };
        state.LegalCases.Add(legalCase.CaseId, ruled);

        var (plaintiffDelta, defendantDelta, scar, stripDefendantOffice) = verdict switch
        {
            LegalCaseVerdict.Dismissed => (
                -(legalCase.IsPatriaPotestasCase ? LegalCatalog.PatriaPotestasCaseDignitasPenalty : LegalCatalog.DismissalDignitasPenalty),
                0, false, false),
            LegalCaseVerdict.Plaintiff => (LegalCatalog.WinnerDignitasGain, -LegalCatalog.LoserDignitasLoss, true, false),
            LegalCaseVerdict.Defendant => (-LegalCatalog.LoserDignitasLoss, LegalCatalog.WinnerDignitasGain, true, false),
            LegalCaseVerdict.SplitCompromise => (LegalCatalog.SplitCompromiseDignitasSwing, -LegalCatalog.SplitCompromiseDignitasSwing, false, false),
            LegalCaseVerdict.Acquitted => (-LegalCatalog.LoserDignitasLoss, LegalCatalog.AcquittedDignitasGain, true, false),
            LegalCaseVerdict.Convicted => (LegalCatalog.WinnerDignitasGain, -LegalCatalog.ConvictedDignitasLoss, true, legalCase.CaseType == LegalCaseType.Political),
            _ => (0, 0, false, false),
        };

        if (plaintiffDelta != 0)
        {
            var reason = $"legal case {legalCase.CaseId.ToTaggedString()} ruled {verdict}";
            var result = AdjustDignitasCommands.Pipeline.Execute(
                state, new AdjustDignitasCommand(state.CommandIds.Issue(), "system", date, causationId, legalCase.PlaintiffId, plaintiffDelta, reason));
            events.AddRange(result.Events);
        }

        if (defendantDelta != 0)
        {
            var reason = $"legal case {legalCase.CaseId.ToTaggedString()} ruled {verdict}";
            var result = AdjustDignitasCommands.Pipeline.Execute(
                state, new AdjustDignitasCommand(state.CommandIds.Issue(), "system", date, causationId, legalCase.DefendantId, defendantDelta, reason));
            events.AddRange(result.Events);
        }

        if (scar)
            events.AddRange(ApplyRelationshipScar(state, legalCase, date, causationId));

        if (verdict == LegalCaseVerdict.Convicted && sentence == LegalSentence.Fine)
        {
            events.Add(LedgerService.Post(
                state, date, LedgerTransactionCategory.Gifts,
                new[]
                {
                    new LedgerPosting(LedgerAccountKey.ForHousehold(legalCase.DefendantId), -LegalCatalog.FineSentenceAmount),
                    new LedgerPosting(FineSink, LegalCatalog.FineSentenceAmount),
                },
                reference: $"legal:fine:{legalCase.CaseId.ToTaggedString()}"));
        }

        if (stripDefendantOffice && MagistracyResolver.ActiveOfficeCountForHousehold(state, legalCase.DefendantId) > 0)
        {
            var result = EndMagistracyForConvictionCommands.Pipeline.Execute(
                state, new EndMagistracyForConvictionCommand(state.CommandIds.Issue(), "system", date, causationId, legalCase.DefendantId));
            events.AddRange(result.Events);
        }

        if (legalCase.IsPatriaPotestasCase)
            events.AddRange(ApplyScandalMark(state, legalCase, date));

        events.Add(new LegalCaseRuledEvent(
            state.EventIds.Issue(), date, legalCase.CaseId, legalCase.CaseType, legalCase.PlaintiffId, legalCase.DefendantId,
            verdict, sentence, legalCase.IsPatriaPotestasCase, causationId));

        return events;
    }

    private static IReadOnlyList<IDomainEvent> ApplyRelationshipScar(WorldState state, LegalCase legalCase, GameDate date, string? causationId)
    {
        if (!state.HouseholdHeadships.TryGet(legalCase.PlaintiffId, out var plaintiffHeadship) ||
            !state.HouseholdHeadships.TryGet(legalCase.DefendantId, out var defendantHeadship))
            return Array.Empty<IDomainEvent>();

        var plaintiffHeadId = plaintiffHeadship!.HeadCharacterId;
        var defendantHeadId = defendantHeadship!.HeadCharacterId;

        if (!state.Characters.TryGet(plaintiffHeadId, out var plaintiffHead) || !plaintiffHead!.IsAlive ||
            !state.Characters.TryGet(defendantHeadId, out var defendantHead) || !defendantHead!.IsAlive)
            return Array.Empty<IDomainEvent>();

        var events = new List<IDomainEvent>();
        events.AddRange(RecordInteractionCommands.Pipeline.Execute(
            state, new RecordInteractionCommand(
                state.CommandIds.Issue(), "system", date, causationId, plaintiffHeadId, defendantHeadId,
                LegalCatalog.RelationshipScarOpinionDelta, BondTag.Nemesis, BondTag.None, RelationshipOrigin.Political)).Events);
        events.AddRange(RecordInteractionCommands.Pipeline.Execute(
            state, new RecordInteractionCommand(
                state.CommandIds.Issue(), "system", date, causationId, defendantHeadId, plaintiffHeadId,
                LegalCatalog.RelationshipScarOpinionDelta, BondTag.Nemesis, BondTag.None, RelationshipOrigin.Political)).Events);
        return events;
    }

    private static IReadOnlyList<IDomainEvent> ApplyScandalMark(WorldState state, LegalCase legalCase, GameDate date)
    {
        if (!state.HouseholdHeadships.TryGet(legalCase.DefendantId, out var headship))
            return Array.Empty<IDomainEvent>();
        if (!state.Characters.TryGet(headship!.HeadCharacterId, out var head) || head is null)
            return Array.Empty<IDomainEvent>();
        if (head.Traits.Contains(LegalCatalog.ScandalMarkedTraitId))
            return Array.Empty<IDomainEvent>();

        var updatedTraits = head.Traits.Append(LegalCatalog.ScandalMarkedTraitId).ToArray();
        state.Characters.Remove(headship.HeadCharacterId);
        state.Characters.Add(headship.HeadCharacterId, head with { Traits = updatedTraits });

        return Array.Empty<IDomainEvent>();
    }
}
