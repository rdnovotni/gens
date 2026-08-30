using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Legal;
using Gens.Simulation.Magistracies;
using Gens.Simulation.Random;
using Gens.Simulation.Reputation;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Legal;

/// <summary>Phase 12 item 4 coverage: filing and Quick Resolution (§4), presiding assignment and
/// recusal (§3), the Major-case Evidence/Hearing/Ruling progression (§5), testimony/evidence/bribery
/// inputs (§7-8), verdict consequences including Political office loss and Patria Potestas's forced
/// Dismissal (§6, §9), and a save/load round trip.</summary>
public sealed class LegalTests
{
    private static (WorldState State, RuntimeId<Settlement> SettlementId, RuntimeId<Household> PlaintiffId, RuntimeId<Household> DefendantId, RuntimeId<Character> PlaintiffHeadId, RuntimeId<Character> DefendantHeadId)
        TwoHouseholds()
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));

        var plaintiffId = state.HouseholdIds.Issue();
        var defendantId = state.HouseholdIds.Issue();

        var plaintiffHeadId = state.CharacterIds.Issue();
        state.Characters.Add(plaintiffHeadId, CharacterTestFixtures.Minimal(plaintiffHeadId, nomen: "Cato", household: plaintiffId));
        state.HouseholdHeadships.Add(plaintiffId, new HouseholdHeadship(plaintiffId, plaintiffHeadId, new GameDate(0)));

        var defendantHeadId = state.CharacterIds.Issue();
        state.Characters.Add(defendantHeadId, CharacterTestFixtures.Minimal(defendantHeadId, nomen: "Fabius", household: defendantId));
        state.HouseholdHeadships.Add(defendantId, new HouseholdHeadship(defendantId, defendantHeadId, new GameDate(0)));

        return (state, settlementId, plaintiffId, defendantId, plaintiffHeadId, defendantHeadId);
    }

    private static void Fund(WorldState state, RuntimeId<Household> householdId, Money amount)
    {
        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Treasury,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), amount),
                new LedgerPosting(new LedgerAccountKey(LedgerAccountKind.System, "test:seed"), -amount),
            });
    }

    private static RuntimeId<Character> SeatDecurion(WorldState state, RuntimeId<Settlement> settlementId, RuntimeId<Household> householdId, GameDate termStart)
    {
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId, nomen: "Decurio", household: householdId));
        var recordId = state.MagistracyRecordIds.Issue();
        state.MagistracyRecords.Add(recordId, new MagistracyRecord(recordId, characterId, MagistracyOffice.Decurion, settlementId, termStart));
        return characterId;
    }

    private static CommandPipeline<WorldState, FileLawsuitCommand> QuickPipeline(ulong seed = 1) =>
        FileLawsuitCommands.CreatePipeline(StreamsFor(seed));

    private static RandomStreamSet StreamsFor(ulong seed)
    {
        var streams = new RandomStreamSet();
        streams.AddDerived(FileLawsuitCommands.QuickResolutionStreamName, seed);
        return streams;
    }

    // ---- Filing & presiding assignment -----------------------------------------------------

    [Test]
    public void FileLawsuitCommandChargesTheQuickFilingFeeAndOpensACase()
    {
        var (state, settlementId, plaintiffId, defendantId, plaintiffHeadId, _) = TwoHouseholds();
        Fund(state, plaintiffId, Money.FromDenarii(100));

        var result = QuickPipeline().Execute(
            state, new FileLawsuitCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null,
                LegalCaseType.PropertyLand, LegalCaseDepth.Quick, plaintiffId, defendantId, settlementId, plaintiffHeadId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(plaintiffId), out var account);
            Assert.That(account!.Balance, Is.EqualTo(Money.FromDenarii(100) - LegalCatalog.QuickFilingCost));
            Assert.That(result.Events.OfType<LawsuitFiledEvent>().Single().Depth, Is.EqualTo(LegalCaseDepth.Quick));
            // A Quick case resolves inline, in the same submission — it ends up Ruled, never lingering Filed.
            Assert.That(state.LegalCases.Count, Is.EqualTo(1));
        });
    }

    [Test]
    public void FileLawsuitCommandAssignsAnUnconflictedDecurionAsPresider()
    {
        var (state, settlementId, plaintiffId, defendantId, plaintiffHeadId, _) = TwoHouseholds();
        Fund(state, plaintiffId, Money.FromDenarii(100));
        var neutralDecurion = SeatDecurion(state, settlementId, state.HouseholdIds.Issue(), new GameDate(0));

        var result = QuickPipeline().Execute(
            state, new FileLawsuitCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null,
                LegalCaseType.PropertyLand, LegalCaseDepth.Quick, plaintiffId, defendantId, settlementId, plaintiffHeadId));

        var filed = result.Events.OfType<LawsuitFiledEvent>().Single();
        Assert.That(filed.PresidingCharacterId, Is.EqualTo(neutralDecurion));
    }

    [Test]
    public void FileLawsuitCommandRecusesAConflictedDecurionAndFallsBackToAnEligibleOne()
    {
        var (state, settlementId, plaintiffId, defendantId, plaintiffHeadId, _) = TwoHouseholds();
        Fund(state, plaintiffId, Money.FromDenarii(100));
        SeatDecurion(state, settlementId, plaintiffId, new GameDate(0)); // conflicted — plaintiff's own household
        var neutralDecurion = SeatDecurion(state, settlementId, state.HouseholdIds.Issue(), new GameDate(0));

        var result = QuickPipeline().Execute(
            state, new FileLawsuitCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null,
                LegalCaseType.PropertyLand, LegalCaseDepth.Quick, plaintiffId, defendantId, settlementId, plaintiffHeadId));

        var filed = result.Events.OfType<LawsuitFiledEvent>().Single();
        Assert.That(filed.PresidingCharacterId, Is.EqualTo(neutralDecurion));
    }

    [Test]
    public void FileLawsuitCommandLeavesTheCasePresiderlessWhenNoEligibleDecurionExists()
    {
        var (state, settlementId, plaintiffId, defendantId, plaintiffHeadId, _) = TwoHouseholds();
        Fund(state, plaintiffId, Money.FromDenarii(100));

        var result = QuickPipeline().Execute(
            state, new FileLawsuitCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null,
                LegalCaseType.PropertyLand, LegalCaseDepth.Quick, plaintiffId, defendantId, settlementId, plaintiffHeadId));

        Assert.That(result.Events.OfType<LawsuitFiledEvent>().Single().PresidingCharacterId, Is.Null);
    }

    // ---- Quick Resolution consequences (RNG-independent path) -------------------------------

    [Test]
    public void APatriaPotestasCaseAlwaysResolvesDismissedRegardlessOfTheRoll()
    {
        var (state, settlementId, plaintiffId, defendantId, plaintiffHeadId, defendantHeadId) = TwoHouseholds();
        Fund(state, plaintiffId, Money.FromDenarii(100));
        AdjustDignitasCommands.Pipeline.Execute(state, new AdjustDignitasCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, plaintiffId, 50, "seed"));

        var result = QuickPipeline(seed: 999).Execute(
            state, new FileLawsuitCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null,
                LegalCaseType.Family, LegalCaseDepth.Quick, plaintiffId, defendantId, settlementId, plaintiffHeadId,
                IsPatriaPotestasCase: true));

        var ruled = result.Events.OfType<LegalCaseRuledEvent>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(ruled.Verdict, Is.EqualTo(LegalCaseVerdict.Dismissed));
            // The harsher Patria Potestas dismissal penalty applies, not the ordinary one.
            Assert.That(DignitasResolver.Current(state, plaintiffId), Is.EqualTo(50 - LegalCatalog.PatriaPotestasCaseDignitasPenalty));
            // The defendant's household head is Scandal-Marked even though the case was dismissed.
            state.Characters.TryGet(defendantHeadId, out var defendantHead);
            Assert.That(defendantHead!.Traits, Does.Contain(LegalCatalog.ScandalMarkedTraitId));
            // No relationship scar on a Dismissal, and no office to strip on a non-Political case.
            Assert.That(state.Relationships.Count, Is.EqualTo(0));
        });
    }

    [Test]
    public void APatriaPotestasScandalMarkIsAppliedOnlyOnce()
    {
        var (state, settlementId, plaintiffId, defendantId, plaintiffHeadId, defendantHeadId) = TwoHouseholds();
        Fund(state, plaintiffId, Money.FromDenarii(200));

        for (var i = 0; i < 2; i++)
        {
            QuickPipeline(seed: (ulong)(999 + i)).Execute(
                state, new FileLawsuitCommand(
                    state.CommandIds.Issue(), "player", new GameDate(1 + i), null,
                    LegalCaseType.Family, LegalCaseDepth.Quick, plaintiffId, defendantId, settlementId, plaintiffHeadId,
                    IsPatriaPotestasCase: true));
        }

        state.Characters.TryGet(defendantHeadId, out var defendantHead);
        Assert.That(defendantHead!.Traits.Count(id => id == LegalCatalog.ScandalMarkedTraitId), Is.EqualTo(1));
    }

    // ---- Major case progression --------------------------------------------------------------

    [Test]
    public void SubmitTestimonyCommandIncreasesTheSupportedSidesCaseStrengthWithALegalScholarBonus()
    {
        var (state, settlementId, plaintiffId, defendantId, plaintiffHeadId, _) = TwoHouseholds();
        Fund(state, plaintiffId, Money.FromDenarii(100));
        var filed = QuickPipeline().Execute(
            state, new FileLawsuitCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null,
                LegalCaseType.Contract, LegalCaseDepth.Major, plaintiffId, defendantId, settlementId, plaintiffHeadId));
        var caseId = filed.Events.OfType<LawsuitFiledEvent>().Single().CaseId;

        var scholarId = state.CharacterIds.Issue();
        state.Characters.Add(scholarId, CharacterTestFixtures.Minimal(scholarId, nomen: "Scholar", household: plaintiffId, traits: new[] { LegalCatalog.LegalScholarTraitId }));

        var result = SubmitTestimonyCommands.Pipeline.Execute(
            state, new SubmitTestimonyCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, caseId, scholarId, LegalCaseSide.Plaintiff));

        state.LegalCases.TryGet(caseId, out var legalCase);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(legalCase!.PlaintiffCaseStrength, Is.EqualTo(LegalCatalog.TestimonyCaseStrengthGain + LegalCatalog.LegalScholarCaseStrengthBonus));
            Assert.That(legalCase.DefendantCaseStrength, Is.EqualTo(0));
        });
    }

    [Test]
    public void GatherEvidenceCommandRejectsOutsideTheEvidenceGatheringWindow()
    {
        var (state, settlementId, plaintiffId, defendantId, plaintiffHeadId, _) = TwoHouseholds();
        Fund(state, plaintiffId, Money.FromDenarii(100));
        var filed = QuickPipeline().Execute(
            state, new FileLawsuitCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null,
                LegalCaseType.Contract, LegalCaseDepth.Quick, plaintiffId, defendantId, settlementId, plaintiffHeadId));
        var caseId = filed.Events.OfType<LawsuitFiledEvent>().Single().CaseId;

        var result = GatherEvidenceCommands.Pipeline.Execute(
            state, new GatherEvidenceCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, caseId, plaintiffHeadId, LegalCaseSide.Plaintiff));

        Assert.That(result.Error, Is.EqualTo(GatherEvidenceCommands.NotAMajorCase));
    }

    [Test]
    public void OfferBribeCommandSpendsMoneyAndCapsAccumulatedWeight()
    {
        var (state, settlementId, plaintiffId, defendantId, plaintiffHeadId, _) = TwoHouseholds();
        Fund(state, plaintiffId, Money.FromDenarii(10_000));
        SeatDecurion(state, settlementId, state.HouseholdIds.Issue(), new GameDate(0));
        var filed = QuickPipeline().Execute(
            state, new FileLawsuitCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null,
                LegalCaseType.Contract, LegalCaseDepth.Major, plaintiffId, defendantId, settlementId, plaintiffHeadId));
        var caseId = filed.Events.OfType<LawsuitFiledEvent>().Single().CaseId;

        var result = OfferBribeCommands.Pipeline.Execute(
            state, new OfferBribeCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, caseId, plaintiffId, Money.FromDenarii(1_000)));

        state.LegalCases.TryGet(caseId, out var legalCase);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(legalCase!.PlaintiffBriberyWeight, Is.EqualTo(LegalCatalog.MaxBriberyWeight));
            state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(plaintiffId), out var account);
            Assert.That(account!.Balance, Is.EqualTo(Money.FromDenarii(10_000) - LegalCatalog.MajorFilingCost - Money.FromDenarii(1_000)));
        });
    }

    [Test]
    public void LegalCaseAdvancementSystemMovesEvidenceGatheringIntoAHearingAfterTheWindowCloses()
    {
        var (state, settlementId, plaintiffId, defendantId, plaintiffHeadId, _) = TwoHouseholds();
        Fund(state, plaintiffId, Money.FromDenarii(100));
        var filed = QuickPipeline().Execute(
            state, new FileLawsuitCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null,
                LegalCaseType.Contract, LegalCaseDepth.Major, plaintiffId, defendantId, settlementId, plaintiffHeadId));
        var caseId = filed.Events.OfType<LawsuitFiledEvent>().Single().CaseId;

        var events = new LegalCaseAdvancementSystem().Tick(
            state, new MonthlyTickContext(new GameDate(LegalCatalog.MajorCaseEvidenceGatheringMonths), new RandomStreamSet()));

        state.LegalCases.TryGet(caseId, out var legalCase);
        Assert.Multiple(() =>
        {
            Assert.That(legalCase!.Stage, Is.EqualTo(LegalCaseStage.Hearing));
            Assert.That(events.OfType<LegalCaseHearingHeldEvent>().Any(), Is.True);
        });
    }

    [Test]
    public void LegalCaseAdvancementSystemRulesTheCaseTheMonthAfterTheHearing()
    {
        var (state, settlementId, plaintiffId, defendantId, plaintiffHeadId, _) = TwoHouseholds();
        Fund(state, plaintiffId, Money.FromDenarii(100));
        var filed = QuickPipeline().Execute(
            state, new FileLawsuitCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null,
                LegalCaseType.Contract, LegalCaseDepth.Major, plaintiffId, defendantId, settlementId, plaintiffHeadId));
        var caseId = filed.Events.OfType<LawsuitFiledEvent>().Single().CaseId;

        var system = new LegalCaseAdvancementSystem();
        var streams = new RandomStreamSet();
        streams.AddDerived(LegalCaseAdvancementSystem.VerdictOutcomeStreamName, 42UL);
        system.Tick(state, new MonthlyTickContext(new GameDate(LegalCatalog.MajorCaseEvidenceGatheringMonths), streams));
        var events = system.Tick(state, new MonthlyTickContext(new GameDate(LegalCatalog.MajorCaseEvidenceGatheringMonths + 1), streams));

        state.LegalCases.TryGet(caseId, out var legalCase);
        Assert.Multiple(() =>
        {
            Assert.That(legalCase!.Stage, Is.EqualTo(LegalCaseStage.Ruled));
            Assert.That(legalCase.Verdict, Is.Not.Null);
            Assert.That(events.OfType<LegalCaseRuledEvent>().Any(), Is.True);
        });
    }

    // ---- A Political conviction strips office (searches a bounded range of seeds for a real
    // Convicted roll, since the verdict itself is a genuine weighted random draw — see
    // LegalCaseResolver.RollVerdict) --------------------------------------------------------

    [Test]
    public void APoliticalConvictionStripsTheDefendantsOfficeAndCollectsTheFine()
    {
        for (var seed = 1UL; seed <= 60UL; seed++)
        {
            var (state, settlementId, plaintiffId, defendantId, plaintiffHeadId, _) = TwoHouseholds();
            Fund(state, plaintiffId, Money.FromDenarii(1_000));
            AdjustDignitasCommands.Pipeline.Execute(state, new AdjustDignitasCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, plaintiffId, 500, "seed"));
            AdjustDignitasCommands.Pipeline.Execute(state, new AdjustDignitasCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, defendantId, -500, "seed"));
            var recordId = state.MagistracyRecordIds.Issue();
            var defendantOfficerId = state.CharacterIds.Issue();
            state.Characters.Add(defendantOfficerId, CharacterTestFixtures.Minimal(defendantOfficerId, nomen: "Officer", household: defendantId));
            state.MagistracyRecords.Add(recordId, new MagistracyRecord(recordId, defendantOfficerId, MagistracyOffice.Decurion, settlementId, new GameDate(0)));

            var result = QuickPipeline(seed).Execute(
                state, new FileLawsuitCommand(
                    state.CommandIds.Issue(), "player", new GameDate(1), null,
                    LegalCaseType.Political, LegalCaseDepth.Quick, plaintiffId, defendantId, settlementId, plaintiffHeadId));

            var ruled = result.Events.OfType<LegalCaseRuledEvent>().Single();
            if (ruled.Verdict != LegalCaseVerdict.Convicted)
                continue;

            state.MagistracyRecords.TryGet(recordId, out var record);
            Assert.Multiple(() =>
            {
                Assert.That(MagistracyResolver.IsActive(record!), Is.False);
                Assert.That(record!.LossReason, Is.EqualTo(MagistracyLossReason.LegalConviction));
                Assert.That(result.Events.OfType<MagistracyLostEvent>().Any(e => e.LossReason == MagistracyLossReason.LegalConviction), Is.True);
            });
            return;
        }

        Assert.Fail("Expected at least one seed in range to roll a Convicted verdict.");
    }

    // ---- Save/load round trip -----------------------------------------------------------------

    [Test]
    public void LegalCaseStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, settlementId, plaintiffId, defendantId, plaintiffHeadId, _) = TwoHouseholds();
        Fund(state, plaintiffId, Money.FromDenarii(200));
        var filed = QuickPipeline().Execute(
            state, new FileLawsuitCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null,
                LegalCaseType.Contract, LegalCaseDepth.Major, plaintiffId, defendantId, settlementId, plaintiffHeadId));
        Assume.That(filed.Accepted, Is.True);

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.LegalCases.Count, Is.EqualTo(state.LegalCases.Count));
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        });
    }
}
