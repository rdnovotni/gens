using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Legal;
using Gens.Simulation.Numerics;
using Gens.Simulation.RealEstate;
using Gens.Simulation.Random;
using Gens.Simulation.Reputation;
using Gens.Simulation.Saves;
using Gens.Simulation.Societates;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Societates;

/// <summary>Phase 15 item 2 coverage — real partnership types and governance models (§2, §4),
/// formation via the negotiated <c>lex societatis</c> (§5), §3's unlimited liability, §7's partner
/// disputes (suspected skimming/fraud, early exit, profit distribution disagreement) resolved through
/// §6's <c>actio pro socio</c> on the existing Legal &amp; Court structure, uncontested dissolution, and
/// a save/load round trip (<c>gens-societates-business-partnerships-design.md</c>).</summary>
public sealed class SocietatesTests
{
    private static (WorldState State, RuntimeId<Settlement> SettlementId) OneSettlement()
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        return (state, settlementId);
    }

    private static (RuntimeId<Household> HouseholdId, RuntimeId<Character> HeadId) HouseholdWithHead(
        WorldState state, string nomen, int loyalty = 80, int ambition = 30, bool greedy = false)
    {
        var householdId = state.HouseholdIds.Issue();
        var headId = state.CharacterIds.Issue();
        var traits = greedy ? new[] { SocietatesCatalog.GreedyTraitId } : Array.Empty<DefinitionId<Trait>>();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(
            headId, nomen: nomen, household: householdId,
            condition: new Condition(80, 20, loyalty, ambition, 50), traits: traits));
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, headId, new GameDate(0)));
        return (householdId, headId);
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

    private static SocietasPartner EqualPartner(RuntimeId<Household> householdId, Fixed64 share) =>
        new(PropertyOwnerRef.ForPlayerHousehold(householdId), share);

    private static RandomStreamSet StreamsFor(ulong seed)
    {
        var streams = new RandomStreamSet();
        streams.AddDerived(FileLawsuitCommands.QuickResolutionStreamName, seed);
        streams.AddDerived(LegalCaseAdvancementSystem.VerdictOutcomeStreamName, seed);
        return streams;
    }

    // ---- Formation (§2, §4, §5) --------------------------------------------------------------

    [Test]
    public void FormSocietasCommandFormsAnEqualPartnersUnusReiSocietas()
    {
        var (state, _) = OneSettlement();
        var (aliceId, _) = HouseholdWithHead(state, "Alice");
        var (bobId, _) = HouseholdWithHead(state, "Bob");

        var result = FormSocietasCommands.Pipeline.Execute(
            state, new FormSocietasCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null,
                PartnershipType.UnusRei, SocietasGovernanceModel.EqualPartners, "One shipping voyage to Ostia",
                new[] { EqualPartner(aliceId, Fixed64.FromRaw(500_000)), EqualPartner(bobId, Fixed64.FromRaw(500_000)) }));

        Assert.That(result.Accepted, Is.True);
        var societasId = result.Events.OfType<SocietasFormedEvent>().Single().SocietasId;
        state.Societates.TryGet(societasId, out var societas);
        Assert.Multiple(() =>
        {
            Assert.That(societas!.IsActive, Is.True);
            Assert.That(societas.Partners.Count, Is.EqualTo(2));
            Assert.That(societas.PartnershipType, Is.EqualTo(PartnershipType.UnusRei));
        });
    }

    [Test]
    public void FormSocietasCommandFormsAllThreeGovernanceModels()
    {
        var (state, _) = OneSettlement();
        var (aliceId, _) = HouseholdWithHead(state, "Alice");
        var (bobId, _) = HouseholdWithHead(state, "Bob");
        var alice = PropertyOwnerRef.ForPlayerHousehold(aliceId);

        foreach (var model in new[] { SocietasGovernanceModel.EqualPartners, SocietasGovernanceModel.DominantPartner, SocietasGovernanceModel.SilentPartner })
        {
            var result = FormSocietasCommands.Pipeline.Execute(
                state, new FormSocietasCommand(
                    state.CommandIds.Issue(), "player", new GameDate(1), null,
                    PartnershipType.OmniumBonorum, model, "A long-standing household alliance",
                    new[] { EqualPartner(aliceId, Fixed64.FromRaw(500_000)), EqualPartner(bobId, Fixed64.FromRaw(500_000)) },
                    DesignatedPartner: model == SocietasGovernanceModel.EqualPartners ? null : alice));

            Assert.That(result.Accepted, Is.True, $"{model} should form.");
        }
    }

    [Test]
    public void FormSocietasCommandRejectsFewerThanTwoPartners()
    {
        var (state, _) = OneSettlement();
        var (aliceId, _) = HouseholdWithHead(state, "Alice");

        var result = FormSocietasCommands.Pipeline.Execute(
            state, new FormSocietasCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null,
                PartnershipType.UnusRei, SocietasGovernanceModel.EqualPartners, "solo venture",
                new[] { EqualPartner(aliceId, Fixed64.One) }));

        Assert.That(result.Error, Is.EqualTo(FormSocietasCommands.TooFewPartners));
    }

    [Test]
    public void FormSocietasCommandRejectsShareFractionsThatDoNotSumToOne()
    {
        var (state, _) = OneSettlement();
        var (aliceId, _) = HouseholdWithHead(state, "Alice");
        var (bobId, _) = HouseholdWithHead(state, "Bob");

        var result = FormSocietasCommands.Pipeline.Execute(
            state, new FormSocietasCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null,
                PartnershipType.UnusRei, SocietasGovernanceModel.EqualPartners, "uneven split",
                new[] { EqualPartner(aliceId, Fixed64.FromRaw(500_000)), EqualPartner(bobId, Fixed64.FromRaw(600_000)) }));

        Assert.That(result.Error, Is.EqualTo(FormSocietasCommands.ShareFractionsDoNotSumToOne));
    }

    [Test]
    public void FormSocietasCommandRejectsADominantPartnerModelWithNoDesignatedPartner()
    {
        var (state, _) = OneSettlement();
        var (aliceId, _) = HouseholdWithHead(state, "Alice");
        var (bobId, _) = HouseholdWithHead(state, "Bob");

        var result = FormSocietasCommands.Pipeline.Execute(
            state, new FormSocietasCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null,
                PartnershipType.UnusRei, SocietasGovernanceModel.DominantPartner, "senator-front venture",
                new[] { EqualPartner(aliceId, Fixed64.FromRaw(500_000)), EqualPartner(bobId, Fixed64.FromRaw(500_000)) }));

        Assert.That(result.Error, Is.EqualTo(FormSocietasCommands.DesignatedPartnerRequired));
    }

    [Test]
    public void FormSocietasCommandRejectsAnEqualPartnersModelWithADesignatedPartner()
    {
        var (state, _) = OneSettlement();
        var (aliceId, _) = HouseholdWithHead(state, "Alice");
        var (bobId, _) = HouseholdWithHead(state, "Bob");

        var result = FormSocietasCommands.Pipeline.Execute(
            state, new FormSocietasCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null,
                PartnershipType.UnusRei, SocietasGovernanceModel.EqualPartners, "even venture",
                new[] { EqualPartner(aliceId, Fixed64.FromRaw(500_000)), EqualPartner(bobId, Fixed64.FromRaw(500_000)) },
                DesignatedPartner: PropertyOwnerRef.ForPlayerHousehold(aliceId)));

        Assert.That(result.Error, Is.EqualTo(FormSocietasCommands.DesignatedPartnerNotAllowed));
    }

    // ---- Unlimited liability (§3) ------------------------------------------------------------

    [Test]
    public void TriggerUnlimitedLiabilityCommandDebitsTheExposedPartnersHousehold()
    {
        var (state, _) = OneSettlement();
        var (aliceId, _) = HouseholdWithHead(state, "Alice");
        var (bobId, _) = HouseholdWithHead(state, "Bob");
        Fund(state, aliceId, Money.FromDenarii(10_000));
        var alice = PropertyOwnerRef.ForPlayerHousehold(aliceId);

        var formed = FormSocietasCommands.Pipeline.Execute(
            state, new FormSocietasCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null,
                PartnershipType.UnusRei, SocietasGovernanceModel.EqualPartners, "voyage",
                new[] { EqualPartner(aliceId, Fixed64.FromRaw(500_000)), EqualPartner(bobId, Fixed64.FromRaw(500_000)) }));
        var societasId = formed.Events.OfType<SocietasFormedEvent>().Single().SocietasId;

        var result = TriggerUnlimitedLiabilityCommands.Pipeline.Execute(
            state, new TriggerUnlimitedLiabilityCommand(
                state.CommandIds.Issue(), "system", new GameDate(2), null, societasId, alice, TriggeringPartnerFailure: true));

        state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(aliceId), out var account);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(account!.Balance, Is.EqualTo(Money.FromDenarii(10_000) - SocietatesCatalog.BaseUnlimitedLiabilityAmount));
            Assert.That(result.Events.OfType<UnlimitedLiabilityEvent>().Single().AmountExposed, Is.EqualTo(SocietatesCatalog.BaseUnlimitedLiabilityAmount));
        });
    }

    [Test]
    public void OmniumBonorumExposesMoreThanUnusReiForTheIdenticalUnfundedFailure()
    {
        var (state, _) = OneSettlement();
        var (aliceId, _) = HouseholdWithHead(state, "Alice");
        var (bobId, _) = HouseholdWithHead(state, "Bob");
        var alice = PropertyOwnerRef.ForPlayerHousehold(aliceId);
        var partners = new[] { EqualPartner(aliceId, Fixed64.FromRaw(500_000)), EqualPartner(bobId, Fixed64.FromRaw(500_000)) };

        var unusRei = FormSocietasCommands.Pipeline.Execute(
            state, new FormSocietasCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, PartnershipType.UnusRei, SocietasGovernanceModel.EqualPartners, "voyage", partners));
        var omniumBonorum = FormSocietasCommands.Pipeline.Execute(
            state, new FormSocietasCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, PartnershipType.OmniumBonorum, SocietasGovernanceModel.EqualPartners, "pooled fortune", partners));

        var unusReiExposure = SocietatesCatalog.BaseUnlimitedLiabilityAmount;
        state.Societates.TryGet(omniumBonorum.Events.OfType<SocietasFormedEvent>().Single().SocietasId, out var omniumBonorumSocietas);
        var omniumBonorumExposure = TriggerUnlimitedLiabilityCommands.ComputeExposure(state, omniumBonorumSocietas!);

        Assert.That(omniumBonorumExposure, Is.GreaterThan(unusReiExposure));
        _ = unusRei;
    }

    // ---- Partner skimming ground truth and audit (§7) ----------------------------------------

    [Test]
    public void PartnerSkimmingRiskSystemFlagsALowLoyaltyGreedyPartnerButNotAnHonestOne()
    {
        var (state, _) = OneSettlement();
        var (aliceId, _) = HouseholdWithHead(state, "Alice", loyalty: 20, greedy: true);
        var (bobId, _) = HouseholdWithHead(state, "Bob", loyalty: 90, greedy: false);
        var alice = PropertyOwnerRef.ForPlayerHousehold(aliceId);
        var bob = PropertyOwnerRef.ForPlayerHousehold(bobId);

        var formed = FormSocietasCommands.Pipeline.Execute(
            state, new FormSocietasCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null,
                PartnershipType.UnusRei, SocietasGovernanceModel.EqualPartners, "voyage",
                new[] { EqualPartner(aliceId, Fixed64.FromRaw(500_000)), EqualPartner(bobId, Fixed64.FromRaw(500_000)) }));
        var societasId = formed.Events.OfType<SocietasFormedEvent>().Single().SocietasId;

        new PartnerSkimmingRiskSystem().Tick(state, new MonthlyTickContext(new GameDate(2), new RandomStreamSet()));

        state.Societates.TryGet(societasId, out var societas);
        SocietasResolver.TryGetPartner(societas!, alice, out var alicePartner);
        SocietasResolver.TryGetPartner(societas!, bob, out var bobPartner);
        Assert.Multiple(() =>
        {
            Assert.That(alicePartner.IsSuspectedSkimming, Is.True);
            Assert.That(bobPartner.IsSuspectedSkimming, Is.False);
        });
    }

    [Test]
    public void AuditPartnerCommandPenalizesAFalseAccusationAgainstAnHonestPartner()
    {
        var (state, _) = OneSettlement();
        var (aliceId, aliceHeadId) = HouseholdWithHead(state, "Alice", loyalty: 90, greedy: false);
        var (bobId, _) = HouseholdWithHead(state, "Bob");
        var alice = PropertyOwnerRef.ForPlayerHousehold(aliceId);

        var formed = FormSocietasCommands.Pipeline.Execute(
            state, new FormSocietasCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null,
                PartnershipType.UnusRei, SocietasGovernanceModel.EqualPartners, "voyage",
                new[] { EqualPartner(aliceId, Fixed64.FromRaw(500_000)), EqualPartner(bobId, Fixed64.FromRaw(500_000)) }));
        var societasId = formed.Events.OfType<SocietasFormedEvent>().Single().SocietasId;
        new PartnerSkimmingRiskSystem().Tick(state, new MonthlyTickContext(new GameDate(2), new RandomStreamSet()));

        var result = AuditPartnerCommands.Pipeline.Execute(
            state, new AuditPartnerCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, societasId, alice));

        state.Characters.TryGet(aliceHeadId, out var aliceHead);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(result.Events.OfType<PartnerAuditedEvent>().Single().WasSkimming, Is.False);
            Assert.That(aliceHead!.Condition.Loyalty, Is.EqualTo(90 - SocietatesCatalog.FalseAuditAccusationLoyaltyPenalty));
        });
    }

    // ---- Actio pro socio (§6) and its resolution (§7) ----------------------------------------

    private static (WorldState State, RuntimeId<Settlement> SettlementId, RuntimeId<Household> PlaintiffId, RuntimeId<Household> DefendantId, RuntimeId<Character> PlaintiffHeadId, RuntimeId<Societas> SocietasId)
        FormedSocietasWithTwoHouseholdPartners(PartnerDisputeType disputeType, out RuntimeId<LegalCase> caseId, ulong seed = 1)
    {
        var (state, settlementId) = OneSettlement();
        var (plaintiffId, plaintiffHeadId) = HouseholdWithHead(state, "Plaintiff");
        var (defendantId, _) = HouseholdWithHead(state, "Defendant");
        Fund(state, plaintiffId, Money.FromDenarii(1_000));

        var formed = FormSocietasCommands.Pipeline.Execute(
            state, new FormSocietasCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null,
                PartnershipType.OmniumBonorum, SocietasGovernanceModel.EqualPartners, "family alliance",
                new[] { EqualPartner(plaintiffId, Fixed64.FromRaw(500_000)), EqualPartner(defendantId, Fixed64.FromRaw(500_000)) }));
        var societasId = formed.Events.OfType<SocietasFormedEvent>().Single().SocietasId;

        // Push the plaintiff's Dignitas up and the defendant's down so the weighted verdict check
        // (Legal.LegalCaseResolver.RollVerdict) reliably favors the plaintiff — the same "tune the
        // score margin rather than the RNG" approach LegalTests.APoliticalConvictionStripsThe...
        // uses for its own guaranteed-outcome tests.
        AdjustDignitasCommands.Pipeline.Execute(state, new AdjustDignitasCommand(state.CommandIds.Issue(), "system", new GameDate(1), null, plaintiffId, 500, "seed"));
        AdjustDignitasCommands.Pipeline.Execute(state, new AdjustDignitasCommand(state.CommandIds.Issue(), "system", new GameDate(1), null, defendantId, -500, "seed"));

        var filed = FileActioProSocioCommands.CreatePipeline(StreamsFor(seed)).Execute(
            state, new FileActioProSocioCommand(
                state.CommandIds.Issue(), "player", new GameDate(2), null, societasId,
                PropertyOwnerRef.ForPlayerHousehold(plaintiffId), PropertyOwnerRef.ForPlayerHousehold(defendantId),
                settlementId, plaintiffHeadId, disputeType));
        Assume.That(filed.Accepted, Is.True);
        caseId = filed.Events.OfType<LawsuitFiledEvent>().Single().CaseId;

        return (state, settlementId, plaintiffId, defendantId, plaintiffHeadId, societasId);
    }

    private static LegalCaseVerdict RunHearingToRuling(WorldState state, RandomStreamSet streams)
    {
        var system = new LegalCaseAdvancementSystem();
        system.Tick(state, new MonthlyTickContext(new GameDate(2 + LegalCatalog.MajorCaseEvidenceGatheringMonths), streams));
        system.Tick(state, new MonthlyTickContext(new GameDate(3 + LegalCatalog.MajorCaseEvidenceGatheringMonths), streams));
        return state.LegalCases.InAscendingOrder().Last().Value.Verdict!.Value;
    }

    [Test]
    public void FileActioProSocioCommandOpensAPartnershipDisputeMajorCaseWithARealLink()
    {
        var (state, _, _, _, _, societasId) = FormedSocietasWithTwoHouseholdPartners(PartnerDisputeType.SuspectedFraud, out var caseId);

        state.LegalCases.TryGet(caseId, out var legalCase);
        state.ActioProSocioLinks.TryGet(caseId, out var link);
        Assert.Multiple(() =>
        {
            Assert.That(legalCase!.CaseType, Is.EqualTo(LegalCaseType.PartnershipDispute));
            Assert.That(legalCase.Depth, Is.EqualTo(LegalCaseDepth.Major));
            Assert.That(legalCase.Stage, Is.EqualTo(LegalCaseStage.EvidenceGathering));
            Assert.That(link!.SocietasId, Is.EqualTo(societasId));
            Assert.That(link.DisputeType, Is.EqualTo(PartnerDisputeType.SuspectedFraud));
        });
    }

    [Test]
    public void FileActioProSocioCommandRejectsARespondentThatIsNotAPlayerHousehold()
    {
        var (state, settlementId) = OneSettlement();
        var (plaintiffId, plaintiffHeadId) = HouseholdWithHead(state, "Plaintiff");
        Fund(state, plaintiffId, Money.FromDenarii(1_000));
        var rival = PropertyOwnerRef.ForRivalGens(state.ActorIds.Issue());

        var formed = FormSocietasCommands.Pipeline.Execute(
            state, new FormSocietasCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null,
                PartnershipType.UnusRei, SocietasGovernanceModel.EqualPartners, "mixed venture",
                new[] { EqualPartner(plaintiffId, Fixed64.FromRaw(500_000)), new SocietasPartner(rival, Fixed64.FromRaw(500_000)) }));
        var societasId = formed.Events.OfType<SocietasFormedEvent>().Single().SocietasId;

        var result = FileActioProSocioCommands.CreatePipeline(StreamsFor(1)).Execute(
            state, new FileActioProSocioCommand(
                state.CommandIds.Issue(), "player", new GameDate(2), null, societasId,
                PropertyOwnerRef.ForPlayerHousehold(plaintiffId), rival, settlementId, plaintiffHeadId,
                PartnerDisputeType.SuspectedFraud));

        Assert.That(result.Error, Is.EqualTo(FileActioProSocioCommands.PartiesMustBeHouseholds));
    }

    [Test]
    public void AConfirmedFraudRulingDissolvesTheSocietasAndCallsInUnlimitedLiability()
    {
        LegalCaseVerdict verdict = default;
        for (var seed = 1UL; seed <= 40UL; seed++)
        {
            var (state, _, _, defendantId, _, societasId) = FormedSocietasWithTwoHouseholdPartners(PartnerDisputeType.SuspectedFraud, out var caseId, seed);
            verdict = RunHearingToRuling(state, StreamsFor(seed));
            if (verdict != LegalCaseVerdict.Plaintiff)
                continue;

            state.Societates.TryGet(societasId, out var societas);
            state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(defendantId), out var defendantAccount);
            Assert.Multiple(() =>
            {
                Assert.That(societas!.IsActive, Is.False);
                Assert.That(societas.DissolutionTrigger, Is.EqualTo(SocietasDissolutionTrigger.Fraud));
                Assert.That(defendantAccount!.Balance, Is.LessThan(Money.Zero));
            });
            _ = caseId;
            return;
        }

        Assert.Fail($"Expected at least one seed in range to roll a Plaintiff verdict (last verdict tried: {verdict}).");
    }

    [Test]
    public void AGrantedEarlyExitDisputeWithdrawsThePlaintiffPartner()
    {
        for (var seed = 1UL; seed <= 40UL; seed++)
        {
            var (state, _, plaintiffId, _, _, societasId) = FormedSocietasWithTwoHouseholdPartners(PartnerDisputeType.EarlyExitDispute, out var caseId, seed);
            var verdict = RunHearingToRuling(state, StreamsFor(seed));
            if (verdict != LegalCaseVerdict.Plaintiff)
                continue;

            // Only two partners were in this Societas, so a granted exit leaves fewer than two and
            // dissolves it outright (WithdrawPartnerCommands' own "not a partnership at all" branch).
            state.Societates.TryGet(societasId, out var societas);
            Assert.That(societas!.IsActive, Is.False);
            Assert.That(societas.DissolutionTrigger, Is.EqualTo(SocietasDissolutionTrigger.MutualAgreement));
            _ = plaintiffId;
            _ = caseId;
            return;
        }

        Assert.Fail("Expected at least one seed in range to roll a Plaintiff verdict.");
    }

    [Test]
    public void WithdrawPartnerCommandRedistributesSharesAcrossThreeOrMorePartners()
    {
        var (state, _) = OneSettlement();
        var (aliceId, _) = HouseholdWithHead(state, "Alice");
        var (bobId, _) = HouseholdWithHead(state, "Bob");
        var (carolId, _) = HouseholdWithHead(state, "Carol");
        var alice = PropertyOwnerRef.ForPlayerHousehold(aliceId);

        var formed = FormSocietasCommands.Pipeline.Execute(
            state, new FormSocietasCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null,
                PartnershipType.OmniumBonorum, SocietasGovernanceModel.EqualPartners, "three-way pool",
                new[]
                {
                    EqualPartner(aliceId, Fixed64.FromRaw(500_000)),
                    EqualPartner(bobId, Fixed64.FromRaw(250_000)),
                    EqualPartner(carolId, Fixed64.FromRaw(250_000)),
                }));
        var societasId = formed.Events.OfType<SocietasFormedEvent>().Single().SocietasId;

        var result = WithdrawPartnerCommands.Pipeline.Execute(
            state, new WithdrawPartnerCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, societasId, alice));

        state.Societates.TryGet(societasId, out var societas);
        var total = societas!.Partners.Aggregate(Fixed64.Zero, (acc, p) => acc + p.ShareFraction);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(societas.IsActive, Is.True);
            Assert.That(societas.Partners.Count, Is.EqualTo(2));
            Assert.That(total, Is.EqualTo(Fixed64.One));
        });
    }

    // ---- Uncontested dissolution (§6) ----------------------------------------------------------

    [Test]
    public void DissolveSocietasCommandResolvesAMutualAgreementWithNoCaseRequired()
    {
        var (state, _) = OneSettlement();
        var (aliceId, _) = HouseholdWithHead(state, "Alice");
        var (bobId, _) = HouseholdWithHead(state, "Bob");

        var formed = FormSocietasCommands.Pipeline.Execute(
            state, new FormSocietasCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null,
                PartnershipType.UnusRei, SocietasGovernanceModel.EqualPartners, "voyage",
                new[] { EqualPartner(aliceId, Fixed64.FromRaw(500_000)), EqualPartner(bobId, Fixed64.FromRaw(500_000)) }));
        var societasId = formed.Events.OfType<SocietasFormedEvent>().Single().SocietasId;

        var result = DissolveSocietasCommands.Pipeline.Execute(
            state, new DissolveSocietasCommand(state.CommandIds.Issue(), "player", new GameDate(4), null, societasId, SocietasDissolutionTrigger.VentureComplete));

        state.Societates.TryGet(societasId, out var societas);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(societas!.IsActive, Is.False);
            Assert.That(societas.DissolutionTrigger, Is.EqualTo(SocietasDissolutionTrigger.VentureComplete));
            Assert.That(state.LegalCases.Count, Is.EqualTo(0));
        });
    }

    [Test]
    public void DissolveSocietasCommandRejectsAPlayerSubmittedFraudTrigger()
    {
        var (state, _) = OneSettlement();
        var (aliceId, _) = HouseholdWithHead(state, "Alice");
        var (bobId, _) = HouseholdWithHead(state, "Bob");

        var formed = FormSocietasCommands.Pipeline.Execute(
            state, new FormSocietasCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null,
                PartnershipType.UnusRei, SocietasGovernanceModel.EqualPartners, "voyage",
                new[] { EqualPartner(aliceId, Fixed64.FromRaw(500_000)), EqualPartner(bobId, Fixed64.FromRaw(500_000)) }));
        var societasId = formed.Events.OfType<SocietasFormedEvent>().Single().SocietasId;

        var result = DissolveSocietasCommands.Pipeline.Execute(
            state, new DissolveSocietasCommand(state.CommandIds.Issue(), "player", new GameDate(4), null, societasId, SocietasDissolutionTrigger.Fraud));

        Assert.That(result.Error, Is.EqualTo(DissolveSocietasCommands.FraudRequiresActioProSocio));
    }

    // ---- Partner dispute risk query (§7/§9, Ambition) ------------------------------------------

    [Test]
    public void PartnerDisputeRiskQueryReadsAmbitionForAnEarlyExitSignal()
    {
        var (state, _) = OneSettlement();
        var (ambitiousId, _) = HouseholdWithHead(state, "Ambitious", ambition: 90);
        var (contentId, _) = HouseholdWithHead(state, "Content", ambition: 10);

        Assert.Multiple(() =>
        {
            Assert.That(PartnerDisputeRiskQuery.EarlyExitLikely(state, PropertyOwnerRef.ForPlayerHousehold(ambitiousId)), Is.True);
            Assert.That(PartnerDisputeRiskQuery.EarlyExitLikely(state, PropertyOwnerRef.ForPlayerHousehold(contentId)), Is.False);
        });
    }

    // ---- Save/load round trip and deterministic hash stability ---------------------------------

    [Test]
    public void SocietatesStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, _, _, _, _, societasId) = FormedSocietasWithTwoHouseholdPartners(PartnerDisputeType.ProfitDistributionDisagreement, out var caseId);

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
            Assert.That(restored.Societates.Count, Is.EqualTo(1));
            Assert.That(restored.ActioProSocioLinks.Count, Is.EqualTo(1));
        });

        restored.Societates.TryGet(societasId, out var restoredSocietas);
        restored.ActioProSocioLinks.TryGet(caseId, out var restoredLink);
        Assert.Multiple(() =>
        {
            Assert.That(restoredSocietas!.PartnershipType, Is.EqualTo(PartnershipType.OmniumBonorum));
            Assert.That(restoredSocietas.Partners.Count, Is.EqualTo(2));
            Assert.That(restoredLink!.DisputeType, Is.EqualTo(PartnerDisputeType.ProfitDistributionDisagreement));
        });
    }
}
