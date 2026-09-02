using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Crime;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Magistracies;
using Gens.Simulation.PublicContracts;
using Gens.Simulation.Reputation;
using Gens.Simulation.Scandal;
using Gens.Simulation.Societates;
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

    public static IDomainEvent[] Apply(
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
        {
            events.AddRange(ApplyScandalMark(state, legalCase, date));
            events.AddRange(RecordWeaponizedLegalCaseScandal(state, legalCase, date, causationId));
        }

        if (verdict == LegalCaseVerdict.Convicted)
            events.AddRange(RecordConvictionAsPunishableOffense(state, legalCase, date, causationId));

        if (legalCase.CaseType == LegalCaseType.PartnershipDispute)
            events.AddRange(ActioProSocioResolutionHook.Apply(state, legalCase, verdict, date, causationId));

        if (legalCase.CaseType == LegalCaseType.Repetundae)
            events.AddRange(RepetundaeResolutionHook.Apply(state, legalCase, verdict, date, causationId));

        events.Add(new LegalCaseRuledEvent(
            state.EventIds.Issue(), date, legalCase.CaseId, legalCase.CaseType, legalCase.PlaintiffId, legalCase.DefendantId,
            verdict, sentence, legalCase.IsPatriaPotestasCase, causationId));

        return events.ToArray();
    }

    private static IDomainEvent[] ApplyRelationshipScar(WorldState state, LegalCase legalCase, GameDate date, string? causationId)
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
        return events.ToArray();
    }

    /// <summary>Phase 12 item 5's real, immediately reachable <see cref="PunishableOffenseSource.LegalConviction"/>
    /// source (<c>gens-crime-punishment-imprisonment-design.md</c> §3): a <see
    /// cref="LegalCaseVerdict.Convicted"/> verdict mints a real <see cref="PunishableOffense"/> against
    /// the defendant household's own recorded head (<see cref="Succession.HouseholdHeadship"/>) — the
    /// same "lands on the household's recorded head" simplification <see cref="ApplyScandalMark"/>
    /// already accepts for the identical household-level-party reason. Severity follows this case's own
    /// capital/non-capital shape directly.</summary>
    private static IDomainEvent[] RecordConvictionAsPunishableOffense(WorldState state, LegalCase legalCase, GameDate date, string? causationId)
    {
        if (!state.HouseholdHeadships.TryGet(legalCase.DefendantId, out var headship))
            return Array.Empty<IDomainEvent>();
        if (!state.Characters.TryGet(headship!.HeadCharacterId, out var head) || !head!.IsAlive)
            return Array.Empty<IDomainEvent>();

        var severity = legalCase.CaseType is LegalCaseType.Criminal or LegalCaseType.Political or LegalCaseType.Repetundae
            ? OffenseSeverity.Capital
            : OffenseSeverity.Serious;

        return RecordPunishableOffenseCommands.Pipeline.Execute(
            state, new RecordPunishableOffenseCommand(
                state.CommandIds.Issue(), "system", date, causationId, headship.HeadCharacterId,
                PunishableOffenseSource.LegalConviction, severity, false, legalCase.CaseId)).Events.ToArray();
    }

    /// <summary>Phase 12 item 7's real, immediately reachable <see
    /// cref="ScandalSourceType.WeaponizedLegalCase"/> source (<c>gens-scandal-design.md</c> §4: "a
    /// politically-weaponized Legal &amp; Court case... a case brought with no real chance of winning,
    /// purely to generate exactly this kind of public airing" — precisely what a Patria Potestas case
    /// already is, per §6). An additive call alongside <see cref="ApplyScandalMark"/>, not a reopening
    /// of it (that already-shipped, already-tested method's own behavior is untouched): this domain's
    /// own harsher <see cref="LegalCatalog.PatriaPotestasCaseDignitasPenalty"/> is already applied above,
    /// so <see cref="RecordScandalCommand.ApplyOrdinaryDignitasPenalty"/> is deliberately off here — the
    /// only genuinely new consequence this adds is the <see cref="Scandal.ScandalRecord"/> itself. <see
    /// cref="RecordScandalCommand.ApplyTraitGrant"/> stays on: <see
    /// cref="Scandal.RecordScandalCommands.ApplyScandalMarkedTrait"/> is idempotent (a Contains-check
    /// before it ever appends), so re-running it after <see cref="ApplyScandalMark"/> already granted the
    /// Trait grants nothing a second time — it only lets this new record honestly stamp <see
    /// cref="Scandal.ScandalRecord.ScandalMarkedTraitApplied"/> true.</summary>
    private static IDomainEvent[] RecordWeaponizedLegalCaseScandal(WorldState state, LegalCase legalCase, GameDate date, string? causationId) =>
        RecordScandalCommands.Pipeline.Execute(
            state, new RecordScandalCommand(
                state.CommandIds.Issue(), "system", date, causationId, legalCase.DefendantId,
                ScandalSourceType.WeaponizedLegalCase, ScandalSeverity.PublicDisgrace,
                ApplyOrdinaryDignitasPenalty: false)).Events.ToArray();

    private static IDomainEvent[] ApplyScandalMark(WorldState state, LegalCase legalCase, GameDate date)
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
