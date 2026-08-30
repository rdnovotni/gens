using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Clientela;
using Gens.Simulation.Commands;
using Gens.Simulation.Crime;
using Gens.Simulation.Economy;
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

namespace Gens.Simulation.Tests.Crime;

/// <summary>Phase 12 item 5 coverage: Punishable Offenses (§3, including the real wired-through Legal
/// &amp; Court conviction source), the Imprison action's Justified/Unjust split across every real
/// authority basis (§4), Detention and its escape risk/attempt (§5), the honestiores/humiliores
/// sentencing catalog's real resolution paths (§7-§8), Ransom negotiation including its real Rival
/// Houses Standing bridge (§10), and a save/load round trip.</summary>
public sealed class CrimeTests
{
    private static (WorldState State, RuntimeId<Household> HouseholdId, RuntimeId<Character> HeadId)
        OneHousehold(string nomen = "Cato")
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));

        var householdId = state.HouseholdIds.Issue();
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, nomen: nomen, household: householdId, location: settlementId));
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, headId, new GameDate(0)));

        return (state, householdId, headId);
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

    // ---- Punishable Offense (§3) --------------------------------------------------------------

    [Test]
    public void RecordPunishableOffenseCommandRecordsAFabricatedOffenseAsARealActiveOne()
    {
        var (state, _, headId) = OneHousehold();

        var result = RecordPunishableOffenseCommands.Pipeline.Execute(
            state, new RecordPunishableOffenseCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, headId,
                PunishableOffenseSource.Fabricated, OffenseSeverity.Serious, IsFabricated: true));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(PunishableOffenseResolver.HasActiveOffense(state, headId), Is.True);
            var offense = PunishableOffenseResolver.MostSevere(state, headId);
            Assert.That(offense!.IsFabricated, Is.True);
            Assert.That(offense.FabricationDiscovered, Is.False);
        });
    }

    [Test]
    public void APoliticalConvictionRecordsARealPunishableOffenseAgainstTheDefendantsHead()
    {
        for (var seed = 1UL; seed <= 60UL; seed++)
        {
            var (state, plaintiffId, plaintiffHeadId) = OneHousehold("Cato");
            var defendantId = state.HouseholdIds.Issue();
            var defendantHeadId = state.CharacterIds.Issue();
            state.Characters.Add(defendantHeadId, CharacterTestFixtures.Minimal(defendantHeadId, nomen: "Fabius", household: defendantId));
            state.HouseholdHeadships.Add(defendantId, new HouseholdHeadship(defendantId, defendantHeadId, new GameDate(0)));

            Fund(state, plaintiffId, Money.FromDenarii(1_000));
            AdjustDignitasCommands.Pipeline.Execute(state, new AdjustDignitasCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, plaintiffId, 500, "seed"));
            AdjustDignitasCommands.Pipeline.Execute(state, new AdjustDignitasCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, defendantId, -500, "seed"));

            var settlementId = state.Settlements.InAscendingOrder().First().Key;
            var streams = new RandomStreamSet();
            streams.AddDerived(FileLawsuitCommands.QuickResolutionStreamName, seed);
            var result = FileLawsuitCommands.CreatePipeline(streams).Execute(
                state, new FileLawsuitCommand(
                    state.CommandIds.Issue(), "player", new GameDate(1), null,
                    LegalCaseType.Political, LegalCaseDepth.Quick, plaintiffId, defendantId, settlementId, plaintiffHeadId));

            var ruled = result.Events.OfType<LegalCaseRuledEvent>().Single();
            if (ruled.Verdict != LegalCaseVerdict.Convicted)
                continue;

            Assert.Multiple(() =>
            {
                Assert.That(PunishableOffenseResolver.HasActiveOffense(state, defendantHeadId), Is.True);
                var offense = PunishableOffenseResolver.MostSevere(state, defendantHeadId);
                Assert.That(offense!.Source, Is.EqualTo(PunishableOffenseSource.LegalConviction));
                Assert.That(offense.Severity, Is.EqualTo(OffenseSeverity.Capital));
            });
            return;
        }

        Assert.Fail("Expected at least one seed in range to roll a Convicted verdict.");
    }

    // ---- Imprison (§4) -------------------------------------------------------------------------

    [Test]
    public void ImprisonCommandRejectsAnActorWithNoRealAuthorityOverTheTarget()
    {
        var (state, _, actorHeadId) = OneHousehold("Cato");
        var unrelatedTargetHousehold = state.HouseholdIds.Issue();
        var targetId = state.CharacterIds.Issue();
        state.Characters.Add(targetId, CharacterTestFixtures.Minimal(targetId, nomen: "Stranger", household: unrelatedTargetHousehold));

        var result = ImprisonCommands.Pipeline.Execute(
            state, new ImprisonCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, actorHeadId, targetId,
                ImprisonAuthorityBasis.PatriaPotestas, DetentionLocationType.PublicCarcer));

        Assert.That(result.Error, Is.EqualTo(ImprisonCommands.NoRealAuthority));
    }

    [Test]
    public void ImprisonCommandOnADependentWithNoOffenseIsUnjustAndPenalizesTheActor()
    {
        var (state, householdId, headId) = OneHousehold();
        var dependentId = state.CharacterIds.Issue();
        state.Characters.Add(dependentId, CharacterTestFixtures.Minimal(dependentId, nomen: "Filius", household: householdId));

        var result = ImprisonCommands.Pipeline.Execute(
            state, new ImprisonCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, headId, dependentId,
                ImprisonAuthorityBasis.PatriaPotestas, DetentionLocationType.PrivateErgastulum));

        var imprisoned = result.Events.OfType<CharacterImprisonedEvent>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(imprisoned.Justified, Is.False);
            Assert.That(DignitasResolver.Current(state, householdId), Is.EqualTo(-CrimeCatalog.UnjustImprisonDignitasPenalty));
            Assert.That(DetentionResolver.ActiveFor(state, dependentId), Is.Not.Null);
            Assert.That(DetentionResolver.ActiveFor(state, dependentId)!.Justified, Is.False);
        });
    }

    [Test]
    public void ImprisonCommandOnADependentWithARealOffenseIsJustifiedAndCostsNoDignitas()
    {
        var (state, householdId, headId) = OneHousehold();
        var dependentId = state.CharacterIds.Issue();
        state.Characters.Add(dependentId, CharacterTestFixtures.Minimal(dependentId, nomen: "Filius", household: householdId));
        RecordPunishableOffenseCommands.Pipeline.Execute(
            state, new RecordPunishableOffenseCommand(
                state.CommandIds.Issue(), "system", new GameDate(0), null, dependentId,
                PunishableOffenseSource.Fabricated, OffenseSeverity.Serious, IsFabricated: true));

        var result = ImprisonCommands.Pipeline.Execute(
            state, new ImprisonCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, headId, dependentId,
                ImprisonAuthorityBasis.PatriaPotestas, DetentionLocationType.PrivateErgastulum));

        var imprisoned = result.Events.OfType<CharacterImprisonedEvent>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(imprisoned.Justified, Is.True);
            Assert.That(DignitasResolver.Current(state, householdId), Is.EqualTo(0));
        });
    }

    [Test]
    public void ImprisonCommandViaClientelaAuthoritySucceedsForThePatronHousesHead()
    {
        var (state, patronHouseholdId, patronHeadId) = OneHousehold("Patronus");
        var clientId = state.CharacterIds.Issue();
        state.Characters.Add(clientId, CharacterTestFixtures.Minimal(clientId, nomen: "Cliens"));
        RecruitClientCommands.Pipeline.Execute(
            state, new RecruitClientCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, patronHouseholdId, clientId, ClientSpecialty.Legal));

        var result = ImprisonCommands.Pipeline.Execute(
            state, new ImprisonCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, patronHeadId, clientId,
                ImprisonAuthorityBasis.ClientelaAuthority, DetentionLocationType.PublicCarcer));

        Assert.That(result.Accepted, Is.True);
    }

    [Test]
    public void ImprisonCommandViaMagisterialJurisdictionSucceedsForAnActiveDecurionAtTheSameSettlement()
    {
        var (state, _, magistrateHeadId) = OneHousehold("Magistratus");
        var settlementId = state.Settlements.InAscendingOrder().First().Key;
        var recordId = state.MagistracyRecordIds.Issue();
        state.MagistracyRecords.Add(recordId, new MagistracyRecord(recordId, magistrateHeadId, MagistracyOffice.Decurion, settlementId, new GameDate(0)));

        var targetId = state.CharacterIds.Issue();
        state.Characters.Add(targetId, CharacterTestFixtures.Minimal(targetId, nomen: "Reus", location: settlementId));

        var result = ImprisonCommands.Pipeline.Execute(
            state, new ImprisonCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, magistrateHeadId, targetId,
                ImprisonAuthorityBasis.MagisterialJurisdiction, DetentionLocationType.PublicCarcer));

        Assert.That(result.Accepted, Is.True);
    }

    [Test]
    public void ImprisonCommandRejectsATargetAlreadyDetained()
    {
        var (state, householdId, headId) = OneHousehold();
        var dependentId = state.CharacterIds.Issue();
        state.Characters.Add(dependentId, CharacterTestFixtures.Minimal(dependentId, nomen: "Filius", household: householdId));
        ImprisonCommands.Pipeline.Execute(
            state, new ImprisonCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, headId, dependentId,
                ImprisonAuthorityBasis.PatriaPotestas, DetentionLocationType.PrivateErgastulum));

        var result = ImprisonCommands.Pipeline.Execute(
            state, new ImprisonCommand(
                state.CommandIds.Issue(), "player", new GameDate(2), null, headId, dependentId,
                ImprisonAuthorityBasis.PatriaPotestas, DetentionLocationType.PrivateErgastulum));

        Assert.That(result.Error, Is.EqualTo(ImprisonCommands.AlreadyDetained));
    }

    // ---- Detention & escape risk (§5) ----------------------------------------------------------

    [Test]
    public void DetentionEscapeRiskScoreFallsBackToALoyaltyOnlyFormulaForAFreeDetainee()
    {
        var (state, householdId, headId) = OneHousehold();
        var dependentId = state.CharacterIds.Issue();
        state.Characters.Add(
            dependentId,
            CharacterTestFixtures.Minimal(dependentId, nomen: "Filius", household: householdId, condition: new Condition(80, 0, 30, 20, 50)));

        var riskScore = DetentionResolver.ComputeRiskScore(state, dependentId);

        Assert.That(riskScore, Is.EqualTo(70)); // 100 - Loyalty(30).
    }

    [Test]
    public void DetentionEscapeRiskScoreReusesTheFlightRiskCalculatorWhenARegimenExists()
    {
        var (state, _, _) = OneHousehold();
        var enslavedId = state.CharacterIds.Issue();
        var regimen = new RegimenSettings(DietTier.Adequate, AccommodationTier.Basic, FreedomsTier.Restricted, DisciplineTier.Firm);
        state.Characters.Add(
            enslavedId,
            CharacterTestFixtures.Minimal(
                enslavedId, nomen: "Servus", status: LegalStatus.Enslaved, condition: new Condition(80, 0, 30, 20, 50), regimen: regimen));

        var expected = FlightRiskCalculator.ComputeRiskScore(30, regimen);
        var actual = DetentionResolver.ComputeRiskScore(state, enslavedId);

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void AttemptDetentionEscapeCommandRejectsACharacterNotDetained()
    {
        var (state, _, headId) = OneHousehold();
        var streams = new RandomStreamSet();
        streams.AddDerived(AttemptDetentionEscapeCommands.EscapeAttemptStreamName, 1UL);

        var result = AttemptDetentionEscapeCommands.CreatePipeline(streams).Execute(
            state, new AttemptDetentionEscapeCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, headId));

        Assert.That(result.Error, Is.EqualTo(AttemptDetentionEscapeCommands.NotDetained));
    }

    [Test]
    public void AttemptDetentionEscapeCommandCanBothSucceedAndFailAcrossSeeds()
    {
        var succeededOnce = false;
        var failedOnce = false;

        for (var seed = 1UL; seed <= 400UL && !(succeededOnce && failedOnce); seed++)
        {
            var (state, householdId, headId) = OneHousehold();
            var dependentId = state.CharacterIds.Issue();
            state.Characters.Add(
                dependentId,
                CharacterTestFixtures.Minimal(dependentId, nomen: "Filius", household: householdId, condition: new Condition(80, 0, 10, 20, 50)));
            ImprisonCommands.Pipeline.Execute(
                state, new ImprisonCommand(
                    state.CommandIds.Issue(), "player", new GameDate(1), null, headId, dependentId,
                    ImprisonAuthorityBasis.PatriaPotestas, DetentionLocationType.PrivateErgastulum));

            var streams = new RandomStreamSet();
            streams.AddDerived(AttemptDetentionEscapeCommands.EscapeAttemptStreamName, seed);
            var result = AttemptDetentionEscapeCommands.CreatePipeline(streams).Execute(
                state, new AttemptDetentionEscapeCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, dependentId));

            var attempted = result.Events.OfType<DetentionEscapeAttemptedEvent>().Single();
            state.Characters.TryGet(dependentId, out var dependent);

            if (attempted.Succeeded)
            {
                succeededOnce = true;
                Assert.That(DetentionResolver.ActiveFor(state, dependentId), Is.Null);
            }
            else
            {
                failedOnce = true;
                Assert.That(dependent!.Condition.Loyalty, Is.EqualTo(0)); // Max(0, 10 - 10).
                Assert.That(DetentionResolver.ActiveFor(state, dependentId), Is.Not.Null);
            }
        }

        Assert.Multiple(() =>
        {
            Assert.That(succeededOnce, Is.True, "Expected at least one seed to roll a successful escape.");
            Assert.That(failedOnce, Is.True, "Expected at least one seed to roll a failed escape attempt.");
        });
    }

    // ---- Mercy release (§10) -------------------------------------------------------------------

    [Test]
    public void ReleaseFromDetentionCommandEndsDetentionAndGrantsDignitasAndOpinion()
    {
        var (state, householdId, headId) = OneHousehold();
        var dependentId = state.CharacterIds.Issue();
        state.Characters.Add(dependentId, CharacterTestFixtures.Minimal(dependentId, nomen: "Filius", household: householdId));
        ImprisonCommands.Pipeline.Execute(
            state, new ImprisonCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, headId, dependentId,
                ImprisonAuthorityBasis.PatriaPotestas, DetentionLocationType.PrivateErgastulum));

        var result = ReleaseFromDetentionCommands.Pipeline.Execute(
            state, new ReleaseFromDetentionCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, dependentId, headId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(DetentionResolver.ActiveFor(state, dependentId), Is.Null);
            Assert.That(DignitasResolver.Current(state, householdId), Is.EqualTo(CrimeCatalog.RansomPaidOrMercyDignitasGain - CrimeCatalog.UnjustImprisonDignitasPenalty));
        });
    }

    // ---- Sentencing (§7-§8) ---------------------------------------------------------------------

    [Test]
    public void ApplySentenceCommandRejectsAModeledButUnwiredSentenceType()
    {
        var (state, _, headId) = OneHousehold();

        var result = ApplySentenceCommands.Pipeline.Execute(
            state, new ApplySentenceCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, headId, SentenceType.Flogging));

        Assert.That(result.Error, Is.EqualTo(ApplySentenceCommands.SentenceNotYetWired));
    }

    [Test]
    public void ApplySentenceCommandChargesARealFineForAHumilioresCharacter()
    {
        var (state, householdId, headId) = OneHousehold();
        Fund(state, householdId, Money.FromDenarii(200));

        var result = ApplySentenceCommands.Pipeline.Execute(
            state, new ApplySentenceCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, headId, SentenceType.Fine));

        state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var account);
        var applied = result.Events.OfType<SentenceAppliedEvent>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(applied.Tier, Is.EqualTo(SentenceTier.Humiliores));
            Assert.That(applied.ResultedInDeath, Is.False);
            Assert.That(account!.Balance, Is.EqualTo(Money.FromDenarii(200) - CrimeCatalog.FineSentenceAmount));
        });
    }

    [Test]
    public void ApplySentenceCommandConfiscatesPropertyForDeportatio()
    {
        var (state, householdId, headId) = OneHousehold();
        Fund(state, householdId, Money.FromDenarii(500));

        ApplySentenceCommands.Pipeline.Execute(
            state, new ApplySentenceCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, headId, SentenceType.Deportatio));

        state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var account);
        Assert.That(account!.Balance, Is.EqualTo(Money.FromDenarii(500) - CrimeCatalog.DeportatioPropertyConfiscation));
    }

    [Test]
    public void ApplySentenceCommandHonorableExitEndsLifeWithoutBeingClassifiedAsViolence()
    {
        var (state, _, headId) = OneHousehold();

        var result = ApplySentenceCommands.Pipeline.Execute(
            state, new ApplySentenceCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, headId, SentenceType.HonorableExit));

        state.Characters.TryGet(headId, out var head);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(head!.IsAlive, Is.False);
            Assert.That(head.DeathRecord!.Value.Cause, Is.EqualTo(DeathCause.Unspecified));
        });
    }

    [Test]
    public void ApplySentenceCommandCrucifixionExecutesAsViolence()
    {
        var (state, _, headId) = OneHousehold();

        var result = ApplySentenceCommands.Pipeline.Execute(
            state, new ApplySentenceCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, headId, SentenceType.Crucifixion));

        state.Characters.TryGet(headId, out var head);
        var applied = result.Events.OfType<SentenceAppliedEvent>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(head!.IsAlive, Is.False);
            Assert.That(head.DeathRecord!.Value.Cause, Is.EqualTo(DeathCause.Violence));
            Assert.That(applied.ResultedInDeath, Is.True);
        });
    }

    [Test]
    public void ApplySentenceCommandUnjustSentenceScarsTheRelationshipWithTheNamedSentencingCharacter()
    {
        var (state, householdId, headId) = OneHousehold();
        var sentencerId = state.CharacterIds.Issue();
        state.Characters.Add(sentencerId, CharacterTestFixtures.Minimal(sentencerId, nomen: "Iudex"));

        ApplySentenceCommands.Pipeline.Execute(
            state, new ApplySentenceCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, headId, SentenceType.Fine,
                SentencingCharacterId: sentencerId));

        var key = new RelationshipKey(sentencerId, headId);
        state.Relationships.TryGet(key, out var relationship);
        Assert.Multiple(() =>
        {
            Assert.That(DignitasResolver.Current(state, householdId), Is.EqualTo(-CrimeCatalog.UnjustSentenceDignitasPenalty));
            Assert.That(relationship.Opinion, Is.EqualTo(-CrimeCatalog.UnjustSentenceOpinionPenalty));
        });
    }

    // ---- Ransom (§10) ---------------------------------------------------------------------------

    [Test]
    public void RansomNegotiationPaidReleasesTheCaptiveAndMovesTheMoney()
    {
        var (state, capturingHouseholdId, capturingHeadId) = OneHousehold("Captor");
        var (targetHouseholdId, targetHeadId) = SecondHousehold(state, "Familia");
        var captiveId = state.CharacterIds.Issue();
        state.Characters.Add(captiveId, CharacterTestFixtures.Minimal(captiveId, nomen: "Captivus", household: targetHouseholdId));
        ImprisonCommands.Pipeline.Execute(
            state, new ImprisonCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, targetHeadId, captiveId,
                ImprisonAuthorityBasis.PatriaPotestas, DetentionLocationType.PublicCarcer));
        Fund(state, targetHouseholdId, Money.FromDenarii(1_000));

        var opened = OpenRansomNegotiationCommands.Pipeline.Execute(
            state, new OpenRansomNegotiationCommand(
                state.CommandIds.Issue(), "player", new GameDate(2), null, captiveId, capturingHouseholdId, targetHouseholdId, Money.FromDenarii(300)));
        var negotiationId = opened.Events.OfType<RansomNegotiationOpenedEvent>().Single().NegotiationId;

        var result = ResolveRansomNegotiationCommands.Pipeline.Execute(
            state, new ResolveRansomNegotiationCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, negotiationId, RansomResolution.Paid));

        state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(targetHouseholdId), out var targetAccount);
        state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(capturingHouseholdId), out var capturingAccount);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(DetentionResolver.ActiveFor(state, captiveId), Is.Null);
            Assert.That(targetAccount!.Balance, Is.EqualTo(Money.FromDenarii(1_000) - Money.FromDenarii(300)));
            Assert.That(capturingAccount!.Balance, Is.EqualTo(Money.FromDenarii(300)));
            Assert.That(DignitasResolver.Current(state, capturingHouseholdId), Is.EqualTo(CrimeCatalog.RansomPaidOrMercyDignitasGain));
        });
    }

    [Test]
    public void RansomNegotiationRefusedLeavesTheCaptiveDetainedAndCostsOpinion()
    {
        var (state, capturingHouseholdId, capturingHeadId) = OneHousehold("Captor");
        var (targetHouseholdId, targetHeadId) = SecondHousehold(state, "Familia");
        var captiveId = state.CharacterIds.Issue();
        state.Characters.Add(captiveId, CharacterTestFixtures.Minimal(captiveId, nomen: "Captivus", household: targetHouseholdId));
        ImprisonCommands.Pipeline.Execute(
            state, new ImprisonCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, targetHeadId, captiveId,
                ImprisonAuthorityBasis.PatriaPotestas, DetentionLocationType.PublicCarcer));

        var opened = OpenRansomNegotiationCommands.Pipeline.Execute(
            state, new OpenRansomNegotiationCommand(
                state.CommandIds.Issue(), "player", new GameDate(2), null, captiveId, capturingHouseholdId, targetHouseholdId, Money.FromDenarii(300)));
        var negotiationId = opened.Events.OfType<RansomNegotiationOpenedEvent>().Single().NegotiationId;

        var result = ResolveRansomNegotiationCommands.Pipeline.Execute(
            state, new ResolveRansomNegotiationCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, negotiationId, RansomResolution.Refused));

        var key = new RelationshipKey(capturingHeadId, targetHeadId);
        state.Relationships.TryGet(key, out var relationship);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(DetentionResolver.ActiveFor(state, captiveId), Is.Not.Null);
            Assert.That(relationship.Opinion, Is.EqualTo(-CrimeCatalog.RansomRefusedOpinionPenalty));
        });
    }

    [Test]
    public void RansomNegotiationPaidMovesRivalHousesStandingTowardAllianceWhenBothSidesAreTrackedActors()
    {
        var (state, capturingHouseholdId, capturingHeadId) = OneHousehold("Captor");
        var (targetHouseholdId, targetHeadId) = SecondHousehold(state, "Familia");

        var regionId = state.Regions.InAscendingOrder().First().Key;
        var settlementId = state.Settlements.InAscendingOrder().First().Key;
        var capturingActorId = state.ActorIds.Issue();
        state.Actors.Add(
            capturingActorId,
            LivingWorldActor.Create(
                capturingActorId, LivingWorldActorType.Gens, "Gens Captoria", LivingWorldActorTier.Noteworthy,
                LivingWorldActorStandingTrend.Established, LivingWorldActorOrigin.Ancient, parentActorId: null,
                LivingWorldActorIdentity.None, dignitas: 0, new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null),
                new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest), regionId, settlementId, headCharacterId: capturingHeadId));
        var targetActorId = state.ActorIds.Issue();
        state.Actors.Add(
            targetActorId,
            LivingWorldActor.Create(
                targetActorId, LivingWorldActorType.Gens, "Gens Familia", LivingWorldActorTier.Noteworthy,
                LivingWorldActorStandingTrend.Established, LivingWorldActorOrigin.Ancient, parentActorId: null,
                LivingWorldActorIdentity.None, dignitas: 0, new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null),
                new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest), regionId, settlementId, headCharacterId: targetHeadId));

        var captiveId = state.CharacterIds.Issue();
        state.Characters.Add(captiveId, CharacterTestFixtures.Minimal(captiveId, nomen: "Captivus", household: targetHouseholdId));
        ImprisonCommands.Pipeline.Execute(
            state, new ImprisonCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, targetHeadId, captiveId,
                ImprisonAuthorityBasis.PatriaPotestas, DetentionLocationType.PublicCarcer));
        Fund(state, targetHouseholdId, Money.FromDenarii(1_000));

        var opened = OpenRansomNegotiationCommands.Pipeline.Execute(
            state, new OpenRansomNegotiationCommand(
                state.CommandIds.Issue(), "player", new GameDate(2), null, captiveId, capturingHouseholdId, targetHouseholdId, Money.FromDenarii(300)));
        var negotiationId = opened.Events.OfType<RansomNegotiationOpenedEvent>().Single().NegotiationId;

        ResolveRansomNegotiationCommands.Pipeline.Execute(
            state, new ResolveRansomNegotiationCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, negotiationId, RansomResolution.Paid));

        Assert.That(HouseStandingResolver.GetEffectiveStanding(state, capturingActorId, targetActorId), Is.EqualTo(HouseStandingLevel.Allied));
    }

    // ---- Save/load round trip -------------------------------------------------------------------

    [Test]
    public void CrimeStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, householdId, headId) = OneHousehold();
        var dependentId = state.CharacterIds.Issue();
        state.Characters.Add(dependentId, CharacterTestFixtures.Minimal(dependentId, nomen: "Filius", household: householdId));
        RecordPunishableOffenseCommands.Pipeline.Execute(
            state, new RecordPunishableOffenseCommand(
                state.CommandIds.Issue(), "system", new GameDate(0), null, dependentId,
                PunishableOffenseSource.Fabricated, OffenseSeverity.Serious, IsFabricated: true));
        ImprisonCommands.Pipeline.Execute(
            state, new ImprisonCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, headId, dependentId,
                ImprisonAuthorityBasis.PatriaPotestas, DetentionLocationType.PrivateErgastulum));
        Fund(state, householdId, Money.FromDenarii(500));
        ApplySentenceCommands.Pipeline.Execute(
            state, new ApplySentenceCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, headId, SentenceType.Fine));

        var (otherHouseholdId, otherHeadId) = SecondHousehold(state, "Alia");
        var captiveId = state.CharacterIds.Issue();
        state.Characters.Add(captiveId, CharacterTestFixtures.Minimal(captiveId, nomen: "Captivus", household: otherHouseholdId));
        ImprisonCommands.Pipeline.Execute(
            state, new ImprisonCommand(
                state.CommandIds.Issue(), "player", new GameDate(3), null, otherHeadId, captiveId,
                ImprisonAuthorityBasis.PatriaPotestas, DetentionLocationType.PublicCarcer));
        var opened = OpenRansomNegotiationCommands.Pipeline.Execute(
            state, new OpenRansomNegotiationCommand(
                state.CommandIds.Issue(), "player", new GameDate(4), null, captiveId, householdId, otherHouseholdId, Money.FromDenarii(50)));
        Assume.That(opened.Accepted, Is.True);

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.PunishableOffenses.Count, Is.EqualTo(state.PunishableOffenses.Count));
            Assert.That(restored.DetentionRecords.Count, Is.EqualTo(state.DetentionRecords.Count));
            Assert.That(restored.SentenceRecords.Count, Is.EqualTo(state.SentenceRecords.Count));
            Assert.That(restored.RansomNegotiations.Count, Is.EqualTo(state.RansomNegotiations.Count));
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        });
    }

    private static (RuntimeId<Household> HouseholdId, RuntimeId<Character> HeadId) SecondHousehold(WorldState state, string nomen)
    {
        var householdId = state.HouseholdIds.Issue();
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, nomen: nomen, household: householdId));
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, headId, new GameDate(0)));
        return (householdId, headId);
    }
}
