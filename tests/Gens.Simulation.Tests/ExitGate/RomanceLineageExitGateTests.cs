using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Interactions;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Legal;
using Gens.Simulation.Random;
using Gens.Simulation.Reputation;
using Gens.Simulation.Romance;
using Gens.Simulation.Saves;
using Gens.Simulation.Scandal;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Time;
using NUnit.Framework;
using CharacterTestFixtures = Gens.Simulation.Tests.Characters.CharacterTestFixtures;

namespace Gens.Simulation.Tests.ExitGate;

/// <summary>
/// Proves Phase 17 item 3's own exit gate: the nine Romance, Sexuality &amp; Lineage slices compose into
/// one continuous run rather than each only working in the isolation of its own dedicated test file.
/// Covers, in one scenario: courtship building to a love-match <see cref="ProposeMarriageCommand"/>;
/// <see cref="EstablishConcubinageCommand"/>; a successful <see cref="SchemeType.Seduce"/> granting <see
/// cref="BondTag.BlackmailLeverage"/>; <see cref="AutonomousRomanceSystem"/> maturing a household bond
/// into a real <see cref="RomanticBondType.Affair"/>; <see cref="ConceptionSystem"/>/<see
/// cref="ChildbirthResolutionSystem"/> producing an <see cref="Legitimacy.Illegitimate"/> child;
/// <see cref="AcknowledgeIllegitimateChildCommand"/>'s real Dignitas cost; <see
/// cref="AffairDiscoverySystem"/> escalating to <see cref="AffairStakesLevel.HighStakes"/>; and <see
/// cref="ResolveAffairCommand"/>'s <see cref="AffairResolution.ProsecutedAdultery"/> branch driving a
/// real <see cref="LegalCaseType.Adultery"/> case to a <see cref="LegalCaseVerdict.Convicted"/> verdict
/// (<see cref="LegalSentence.Relegatio"/>, confiscation, the Status/Role Dignitas modifier, and a real
/// <see cref="InfamiaStatus"/>) — with save/load round trips and deterministic state-hash stability
/// checked at both the mid-run and final points, matching every other phase's own exit-gate convention
/// (e.g. <see cref="SuccessionDynastyExitGateTests"/>).
///
/// A second, smaller scenario proves the long-reserved <see
/// cref="Scandal.ScandalSourceType.AffairDiscovery"/> wiring point for the quieter, far more common
/// minor-stakes case, with its own save/load and hash check.
///
/// <b>Household layout, deliberately simplified:</b> only the Companion/Daughter pair shares a household
/// — every other pair (Son/Beloved, the two Concubinage partners, the Seduce initiator/target) is left
/// with no <see cref="Character.Household"/> at all, so <see cref="AutonomousRomanceSystem"/> (which only
/// ever scans pairs sharing a non-null household) never touches them. This avoids a second,
/// nondeterministic conception source competing for the Daughter's own single "unresolved pregnancy"
/// slot, and avoids the Son/Beloved love-match pair being mistaken by that same system for a second
/// eligible household pair once they marry each other.
///
/// Calls each <see cref="IMonthlySystem{TState}"/> directly in a controlled order and month count, the
/// same idiom every per-slice Romance test file already uses, rather than assembling a full <see
/// cref="CampaignBootstrapper"/>-driven <see cref="MonthlySimulation{TState}"/> — this gives exact control
/// over system order, which matters here so <see cref="AffairDiscoverySystem"/> reliably observes the
/// still-unresolved pregnancy before <see cref="ChildbirthResolutionSystem"/> resolves it.
/// </summary>
public sealed class RomanceLineageExitGateTests
{
    private static readonly GameDate Epoch = new(0);

    private static WorldState NewState() => new(Epoch);

    private static RandomStreamSet MainStreams(ulong rootSeed)
    {
        var streams = new RandomStreamSet();
        streams.AddDerived(CampaignBootstrapper.SchemeProgressStreamName, rootSeed);
        streams.AddDerived(CampaignBootstrapper.RomanceAutonomousInitiationStreamName, rootSeed);
        streams.AddDerived(CampaignBootstrapper.RomanceConceptionChanceStreamName, rootSeed);
        streams.AddDerived(CampaignBootstrapper.RomanceAffairDiscoveryStreamName, rootSeed);
        streams.AddDerived(CampaignBootstrapper.CharacterGenerationStreamName, rootSeed);
        return streams;
    }

    private static RuntimeId<Character> AddAdult(
        WorldState state, Sex sex, int intrigue = 10, int fertility = 50, int health = 80,
        RuntimeId<Household>? household = null, RuntimeId<Settlement>? location = null,
        MarriageRecord[]? maritalHistory = null, LegalStatus status = LegalStatus.RomanCitizen)
    {
        var id = state.CharacterIds.Issue();
        state.Characters.Add(
            id,
            CharacterTestFixtures.Minimal(
                id, sex: sex, birthDate: new GameDate(-30 * 12), household: household, location: location,
                status: status, condition: new Condition(health, 0, 50, 20, fertility),
                attributes: new CoreAttributes(10, 10, 10, intrigue, 10), maritalHistory: maritalHistory));
        return id;
    }

    private static void Fund(WorldState state, RuntimeId<Household> householdId, Money amount) =>
        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Treasury,
            new[] { new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), amount), new LedgerPosting(LedgerAccountKey.Mint, -amount) });

    private static WorldState RoundTrip(WorldState state, RandomStreamSet streams, string path) =>
        RoundTrip(state, streams, path, out _);

    private static WorldState RoundTrip(WorldState state, RandomStreamSet streams, string path, out ulong hashBefore)
    {
        hashBefore = StateHasher.Hash(state);
        SaveWriter.Write(path, state, streams, "0.0.0-test", "test-content-pack");
        var loaded = SaveReader.Read(path);
        Assert.That(StateHasher.Hash(loaded.State), Is.EqualTo(hashBefore), "Save/load round trip must preserve the deterministic state hash.");
        return loaded.State;
    }

    [Test]
    public void TheFullVerticalSliceComposesFromCourtshipThroughAdulteryConvictionWithConsistentSavesAndHash()
    {
        var state = NewState();
        var streams = MainStreams(rootSeed: 1);

        // ---- Step 1: courtship -> love-match marriage (Son, Beloved) -----------------------------
        var sonId = AddAdult(state, Sex.Male);
        var belovedId = AddAdult(state, Sex.Female);
        RecordRomanticInteractionCommands.Pipeline.Execute(
            state, new RecordRomanticInteractionCommand(
                state.CommandIds.Issue(), "player", Epoch, null, sonId, belovedId,
                RomanceCatalog.LoveMatchThreshold, RomanceCatalog.LoveMatchThreshold, null));
        var proposeResult = ProposeMarriageCommands.Pipeline.Execute(
            state, new ProposeMarriageCommand(state.CommandIds.Issue(), sonId.ToTaggedString(), Epoch, null, sonId, belovedId));
        Assert.That(proposeResult.Accepted, Is.True, $"ProposeMarriageCommand rejected: {proposeResult.Error}");

        // ---- Step 2: concubinage (ConcA, ConcB) --------------------------------------------------
        var concAId = AddAdult(state, Sex.Male);
        var concBId = AddAdult(state, Sex.Female);
        var concubinageResult = EstablishConcubinageCommands.Pipeline.Execute(
            state, new EstablishConcubinageCommand(state.CommandIds.Issue(), concAId.ToTaggedString(), Epoch, null, concAId, concBId));
        Assert.That(concubinageResult.Accepted, Is.True, $"EstablishConcubinageCommand rejected: {concubinageResult.Error}");

        // ---- Step 3: a deterministic Seduce Scheme success (Initiator, Target) ------------------
        var initiatorId = AddAdult(state, Sex.Male);
        var targetId = AddAdult(state, Sex.Female);
        RecordRomanticInteractionCommands.Pipeline.Execute(
            state, new RecordRomanticInteractionCommand(
                state.CommandIds.Issue(), "player", Epoch, null, initiatorId, targetId, 0, 100, null));
        var initiateResult = InitiateSchemeCommands.Pipeline.Execute(
            state, new InitiateSchemeCommand(
                state.CommandIds.Issue(), initiatorId.ToTaggedString(), Epoch, null, initiatorId, targetId, SchemeType.Seduce));
        Assert.That(initiateResult.Accepted, Is.True, $"InitiateSchemeCommand rejected: {initiateResult.Error}");
        var schemeId = initiateResult.Events.OfType<SchemeInitiatedEvent>().Single().SchemeId;
        // Direct state flip so the Scheme completes on the very next tick (Progress one month's gain
        // short of MaxValue) — a real roll cannot be scheduled to land on a specific month reliably, and
        // the pre-seeded maximal Attraction above already clamps the success chance to 100% regardless
        // of which roll the stream produces (see SeduceSchemeTests' own identical technique).
        state.Schemes.TryGet(schemeId, out var seededScheme);
        state.Schemes.Remove(schemeId);
        state.Schemes.Add(schemeId, seededScheme! with { Progress = Scheme.MaxValue - SchemeProgressCatalog.BaseProgressPerMonthPercent });
        new SchemeProgressSystem().Tick(state, new MonthlyTickContext(new GameDate(1), streams));
        state.Schemes.TryGet(schemeId, out var resolvedScheme);
        Assert.That(resolvedScheme!.Status, Is.EqualTo(SchemeStatus.Succeeded));

        // ---- Household A/B, the affair pair (Companion, Daughter married to SpouseB) ------------
        var householdAId = state.HouseholdIds.Issue();
        var householdBId = state.HouseholdIds.Issue();
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId, SettlementStage.Vicus));

        // A status gap (Companion is a Freedman, Daughter a Roman Citizen) so the Status/Role
        // Dignitas modifier (§13) is real and nonzero, matching AdulteryLegalCaseTests' own identical
        // seeding idiom.
        var companionId = AddAdult(state, Sex.Male, intrigue: 0, household: householdAId, status: LegalStatus.Freedman);
        var spouseBId = AddAdult(state, Sex.Male, intrigue: 100, household: householdBId, location: settlementId);
        var daughterId = AddAdult(
            state, Sex.Female, intrigue: 0, fertility: 100, household: householdAId,
            maritalHistory: new[] { new MarriageRecord(spouseBId, Epoch, null, null) });

        // ---- Step 4: abstract fertility risk, then let the household self-simulate --------------
        state = WithFertilityRiskAbstracted(state);

        var autonomousSystem = new AutonomousRomanceSystem();
        var conceptionSystem = new ConceptionSystem();
        var affairDiscoverySystem = new AffairDiscoverySystem();
        var childbirthSystem = new ChildbirthResolutionSystem();

        RuntimeId<AffairRecord>? affairId = null;
        AffairRecord? affairRecord = null;
        RuntimeId<Character>? childId = null;
        Legitimacy? childLegitimacy = null;
        var month = 0;

        const int MonthBudget = 2200;
        for (month = 1; month <= MonthBudget; month++)
        {
            var context = new MonthlyTickContext(new GameDate(month), streams);

            autonomousSystem.Tick(state, context);
            conceptionSystem.Tick(state, context);
            affairDiscoverySystem.Tick(state, context);
            var childbirthEvents = childbirthSystem.Tick(state, context);

            if (affairId is null)
            {
                foreach (var entry in state.AffairRecords.InAscendingOrder())
                {
                    if (entry.Value.OffenderCharacterId != daughterId && entry.Value.ThirdPartyCharacterId != daughterId)
                        continue;
                    affairId = entry.Key;
                    affairRecord = entry.Value;
                    break;
                }
            }

            if (childId is null)
            {
                var born = childbirthEvents.OfType<CharacterBornEvent>().FirstOrDefault(e => e.MotherId == daughterId);
                if (born is not null)
                {
                    childId = born.CharacterId;
                    childLegitimacy = born.Legitimacy;
                }
            }

            if (affairId is not null && childId is not null)
                break;
        }

        Assert.That(affairId, Is.Not.Null, $"Expected the Companion/Daughter bond to escalate into a discovered affair within {MonthBudget} months.");
        Assert.That(childId, Is.Not.Null, $"Expected the affair to produce a live-born child within {MonthBudget} months.");
        Assert.Multiple(() =>
        {
            Assert.That(affairRecord!.StakesLevel, Is.EqualTo(AffairStakesLevel.HighStakes));
            Assert.That(affairRecord.LegitimacyContested, Is.True);
            Assert.That(affairRecord.Resolution, Is.Null);
            Assert.That(childLegitimacy, Is.EqualTo(Legitimacy.Illegitimate));
        });

        state.Characters.TryGet(daughterId, out var daughterAfterDiscovery);
        state.Characters.TryGet(spouseBId, out var spouseBAfterDiscovery);
        Assert.Multiple(() =>
        {
            Assert.That(daughterAfterDiscovery!.Traits, Does.Contain(RomanceCatalog.AdulterousTraitId));
            Assert.That(spouseBAfterDiscovery!.Traits, Does.Contain(RomanceCatalog.HeartbrokenTraitId));
            Assert.That(spouseBAfterDiscovery.Traits, Does.Contain(RomanceCatalog.GuardedTraitId));
        });

        // ---- Step 5: legitimacy acknowledgment's real Dignitas cost ------------------------------
        state.HouseholdHeadships.Add(householdAId, new HouseholdHeadship(householdAId, companionId, Epoch));
        // Household A already carries the §13 Status/Role modifier AffairDiscoverySystem applied at
        // discovery time (the status gap seeded above) — capture that as the baseline rather than
        // assuming a bare 0, and confirm the acknowledgment cost stacks additively on top of it.
        var dignitasBeforeAcknowledgment = DignitasResolver.Current(state, householdAId);
        var acknowledgeResult = AcknowledgeIllegitimateChildCommands.Pipeline.Execute(
            state, new AcknowledgeIllegitimateChildCommand(
                state.CommandIds.Issue(), companionId.ToTaggedString(), new GameDate(month), null, householdAId, companionId, childId!.Value));
        Assert.That(acknowledgeResult.Accepted, Is.True, $"AcknowledgeIllegitimateChildCommand rejected: {acknowledgeResult.Error}");
        Assert.That(
            DignitasResolver.Current(state, householdAId),
            Is.EqualTo(dignitasBeforeAcknowledgment - RomanceCatalog.IllegitimateChildAcknowledgmentDignitasPenalty));

        // ---- Step 6: fund + skew Dignitas, mid-run save/load, then a seed-searched legal verdict -
        Fund(state, householdBId, Money.FromDenarii(1_000));
        Fund(state, householdAId, Money.FromDenarii(1_000));
        AdjustDignitasCommands.Pipeline.Execute(
            state, new AdjustDignitasCommand(state.CommandIds.Issue(), "system", state.Date, null, householdBId, 500, "seed"));
        AdjustDignitasCommands.Pipeline.Execute(
            state, new AdjustDignitasCommand(state.CommandIds.Issue(), "system", state.Date, null, householdAId, -500, "seed"));

        var preLegalPath = Path.Combine(Path.GetTempPath(), $"gens-romance-exitgate-{Guid.NewGuid():N}.gens");
        WorldState convictedState;
        RuntimeId<LegalCase> convictedCaseId;
        Money balanceBeforeRuling;
        try
        {
            RoundTrip(state, streams, preLegalPath);

            WorldState? found = null;
            RuntimeId<LegalCase>? foundCaseId = null;
            Money foundBalanceBeforeRuling = Money.Zero;
            var filingMonth = month;

            for (var seed = 1UL; seed <= 80UL && found is null; seed++)
            {
                var attemptState = SaveReader.Read(preLegalPath).State;

                var legalStreams = new RandomStreamSet();
                legalStreams.AddDerived(LegalCaseAdvancementSystem.VerdictOutcomeStreamName, seed);

                var resolveResult = ResolveAffairCommands.CreatePipeline(legalStreams).Execute(
                    attemptState, new ResolveAffairCommand(
                        attemptState.CommandIds.Issue(), spouseBId.ToTaggedString(), new GameDate(filingMonth), null,
                        affairId!.Value, AffairResolution.ProsecutedAdultery));
                Assert.That(resolveResult.Accepted, Is.True, $"ResolveAffairCommand rejected: {resolveResult.Error}");
                var caseId = resolveResult.Events.OfType<LawsuitFiledEvent>().Single().CaseId;

                attemptState.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdAId), out var accountBeforeRuling);
                var attemptBalanceBeforeRuling = accountBeforeRuling!.Balance;

                var advancementSystem = new LegalCaseAdvancementSystem();
                advancementSystem.Tick(
                    attemptState, new MonthlyTickContext(new GameDate(filingMonth + LegalCatalog.MajorCaseEvidenceGatheringMonths), legalStreams));
                advancementSystem.Tick(
                    attemptState, new MonthlyTickContext(new GameDate(filingMonth + LegalCatalog.MajorCaseEvidenceGatheringMonths + 1), legalStreams));

                attemptState.LegalCases.TryGet(caseId, out var legalCase);
                if (legalCase!.Verdict != LegalCaseVerdict.Convicted)
                    continue;

                found = attemptState;
                foundCaseId = caseId;
                foundBalanceBeforeRuling = attemptBalanceBeforeRuling;
            }

            Assert.That(found, Is.Not.Null, "No seed in the searched range produced a Convicted adultery verdict.");
            convictedState = found!;
            convictedCaseId = foundCaseId!.Value;
            balanceBeforeRuling = foundBalanceBeforeRuling;
        }
        finally
        {
            File.Delete(preLegalPath);
        }

        // ---- Final assertions: every earlier step still holds in the convicted clone ------------
        convictedState.Characters.TryGet(sonId, out var sonFinal);
        convictedState.RomanticBonds.TryGet(RomanticBondKey.Create(sonId, belovedId), out var sonBondFinal);
        convictedState.RomanticBonds.TryGet(RomanticBondKey.Create(concAId, concBId), out var concBondFinal);
        convictedState.Relationships.TryGet(new RelationshipKey(initiatorId, targetId), out var seduceRelationshipFinal);
        convictedState.HeirDesignations.TryGet(householdAId, out var heirDesignationFinal);
        convictedState.LegalCases.TryGet(convictedCaseId, out var finalLegalCase);
        convictedState.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdAId), out var accountAfterRuling);
        var expectedConfiscation = balanceBeforeRuling.Scale(RomanceCatalog.AdulteryConfiscationFraction);
        var expectedStatusRoleModifier = StatusRoleDignitasModifier.Calculate(convictedState, daughterId, companionId);
        convictedState.AffairRecords.TryGet(affairId!.Value, out var finalAffairRecord);
        convictedState.InfamiaStatuses.TryGet(daughterId, out var infamia);

        Assert.Multiple(() =>
        {
            Assert.That(sonFinal!.CurrentSpouseId, Is.EqualTo(belovedId));
            Assert.That(sonBondFinal.BondType, Is.EqualTo(RomanticBondType.Marriage));

            Assert.That(concBondFinal.BondType, Is.EqualTo(RomanticBondType.Concubinage));
            Assert.That(concBondFinal.IsKnownPublicly, Is.True);

            Assert.That(seduceRelationshipFinal.Bonds.HasFlag(BondTag.BlackmailLeverage), Is.True);

            Assert.That(heirDesignationFinal!.AcknowledgedIllegitimateChildIds, Does.Contain(childId!.Value));

            Assert.That(finalAffairRecord!.Resolution, Is.EqualTo(AffairResolution.ProsecutedAdultery));
            Assert.That(finalAffairRecord.LegalCaseId, Is.EqualTo(convictedCaseId));

            Assert.That(finalLegalCase!.Sentence, Is.EqualTo(LegalSentence.Relegatio));
            Assert.That(accountAfterRuling!.Balance, Is.EqualTo(balanceBeforeRuling - expectedConfiscation));

            Assert.That(infamia, Is.Not.Null, "Expected a real InfamiaStatus on the convicted offender.");
            Assert.That(infamia!.Source, Is.EqualTo(InfamiaSource.ConvictedAdultery));

            // Sanity check on the Status/Role modifier's own reachability — not re-deriving the exact
            // Dignitas arithmetic already covered by AdulteryLegalCaseTests.
            Assert.That(expectedStatusRoleModifier, Is.Not.EqualTo(0), "Expected a nonzero Status/Role modifier given the status gap seeded above.");
        });

        // ---- Final save/load + state-hash stability ---------------------------------------------
        var finalPath = Path.Combine(Path.GetTempPath(), $"gens-romance-exitgate-final-{Guid.NewGuid():N}.gens");
        try
        {
            RoundTrip(convictedState, new RandomStreamSet(), finalPath);
        }
        finally
        {
            File.Delete(finalPath);
        }
    }

    [Test]
    public void AMinorStakesAffairResolvesQuietlyThroughScandalWithConsistentSavesAndHash()
    {
        var state = NewState();
        var streams = new RandomStreamSet();
        streams.AddDerived(CampaignBootstrapper.RomanceAffairDiscoveryStreamName, rootSeed: 1);

        var wrongedSpouseId = AddAdult(state, Sex.Female, intrigue: 100);
        var householdId = state.HouseholdIds.Issue();
        var offenderId = AddAdult(
            state, Sex.Male, intrigue: 0, household: householdId,
            maritalHistory: new[] { new MarriageRecord(wrongedSpouseId, new GameDate(-24), null, null) });
        var thirdPartyId = AddAdult(state, Sex.Female, intrigue: 0);

        var key = RomanticBondKey.Create(offenderId, thirdPartyId);
        state.RomanticBonds.Add(key, new RomanticBond(RomanticBondType.Affair, 50, 50, false, 0, Epoch, Epoch, null));

        var system = new AffairDiscoverySystem();
        AffairRecord? record = null;
        for (var month = 1; month <= 20 && record is null; month++)
        {
            system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));
            record = state.AffairRecords.InAscendingOrder().Select(entry => entry.Value).FirstOrDefault();
        }

        Assert.That(record, Is.Not.Null, "Expected the affair to escalate within the test's month budget.");

        var scandal = state.ScandalRecords.InAscendingOrder()
            .Select(entry => entry.Value)
            .FirstOrDefault(s => s.SourceType == ScandalSourceType.AffairDiscovery);

        Assert.Multiple(() =>
        {
            Assert.That(record!.StakesLevel, Is.EqualTo(AffairStakesLevel.Minor));
            Assert.That(record.Resolution, Is.EqualTo(AffairResolution.QuietlyResolved));
            Assert.That(scandal, Is.Not.Null, "Expected the long-reserved ScandalSourceType.AffairDiscovery to gain a real caller.");
            Assert.That(scandal!.PrimaryHouseholdId, Is.EqualTo(householdId));
        });

        var path = Path.Combine(Path.GetTempPath(), $"gens-romance-exitgate-minor-{Guid.NewGuid():N}.gens");
        try
        {
            RoundTrip(state, streams, path);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>Flips <see cref="WorldState.RomanceContentSettings"/>'s <c>private set</c> the same way
    /// <c>PregnancyAndLegitimacyTests</c> does: a save/load round trip through <see
    /// cref="WorldStateMapper"/> with the DTO's own toggle forced on, rather than adding a test-only
    /// setter to production code.</summary>
    private static WorldState WithFertilityRiskAbstracted(WorldState state)
    {
        var dto = WorldStateMapper.ToDto(state);
        var abstractedDto = dto with { RomanceFertilityRiskAbstracted = true };
        return WorldStateMapper.ToWorldState(abstractedDto);
    }
}
