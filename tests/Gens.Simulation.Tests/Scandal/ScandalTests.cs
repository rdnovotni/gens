using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Chronicle;
using Gens.Simulation.Clientela;
using Gens.Simulation.Collegia;
using Gens.Simulation.Commands;
using Gens.Simulation.Crime;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Legal;
using Gens.Simulation.Magistracies;
using Gens.Simulation.Random;
using Gens.Simulation.Reputation;
using Gens.Simulation.Saves;
using Gens.Simulation.Scandal;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Scandal;

/// <summary>Phase 12 item 7 coverage: <see cref="RecordScandalCommand"/>'s own shared "ordinary case"
/// bundle (§7) including Faction-dependent reception (§7/§10), the four real wired sources — an Unjust
/// <see cref="ImprisonCommand"/>/<see cref="ApplySentenceCommand"/> (§4), <see
/// cref="DiscoverFabricationCommand"/> (§4), <see cref="LegalCaseRuling"/>'s Patria Potestas ruling
/// (§4), and <see cref="DissolveCollegiumCommand"/> (§4) — each verified not to double an
/// already-tested call site's own existing Dignitas/relationship consequences, the lifecycle decay/fade
/// (§9), Rehabilitation's trigger and Trait grant (§8), Chronicle projection for severe cases (§9), and
/// a save/load round trip.</summary>
public sealed class ScandalTests
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
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, nomen: nomen, household: householdId));
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, headId, new GameDate(0)));

        return (state, householdId, headId);
    }

    private static (RuntimeId<Household> HouseholdId, RuntimeId<Character> HeadId) SecondHousehold(WorldState state, string nomen)
    {
        var householdId = state.HouseholdIds.Issue();
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, nomen: nomen, household: householdId));
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

    // ---- RecordScandalCommand's own ordinary-case bundle (§7) --------------------------------

    [Test]
    public void RecordScandalCommandAppliesDignitasScarAndTraitTogether()
    {
        var (state, householdId, headId) = OneHousehold();
        var (otherHouseholdId, otherHeadId) = SecondHousehold(state, "Rival");

        var result = RecordScandalCommands.Pipeline.Execute(
            state, new RecordScandalCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, householdId,
                ScandalSourceType.UnjustAction, ScandalSeverity.PublicDisgrace,
                ScarredAgainstCharacterId: otherHeadId));

        state.Characters.TryGet(headId, out var head);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(DignitasResolver.Current(state, householdId), Is.EqualTo(-ScandalCatalog.PublicDisgraceDignitasPenalty));
            Assert.That(head!.Traits, Does.Contain(ScandalCatalog.ScandalMarkedTraitId));
            Assert.That(state.Relationships.Count, Is.EqualTo(1));
            Assert.That(result.Events.OfType<ScandalRecordedEvent>().Single().ScandalMarkedTraitApplied, Is.True);
        });
    }

    [Test]
    public void MinorEmbarrassmentNeverGrantsTheScandalMarkedTrait()
    {
        var (state, householdId, headId) = OneHousehold();

        RecordScandalCommands.Pipeline.Execute(
            state, new RecordScandalCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, householdId,
                ScandalSourceType.UnjustAction, ScandalSeverity.MinorEmbarrassment));

        state.Characters.TryGet(headId, out var head);
        Assert.That(head!.Traits, Does.Not.Contain(ScandalCatalog.ScandalMarkedTraitId));
    }

    [Test]
    public void FactionDependentReceptionReadsCharacterFactionAlignmentDirectly()
    {
        var (state, householdId, headId) = OneHousehold();
        SetCharacterFactionCommands.Pipeline.Execute(
            state, new SetCharacterFactionCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, headId, PoliticalFaction.Traditionalist));

        var result = RecordScandalCommands.Pipeline.Execute(
            state, new RecordScandalCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, householdId,
                ScandalSourceType.UnjustAction, ScandalSeverity.PublicDisgrace));

        var recorded = result.Events.OfType<ScandalRecordedEvent>().Single();
        state.ScandalRecords.TryGet(recorded.ScandalId, out var record);
        Assert.Multiple(() =>
        {
            Assert.That(
                record!.FactionReception.TraditionalistReading - record.FactionReception.PopularistReading,
                Is.EqualTo(ScandalCatalog.FactionAlignedReadingPenalty));
        });
    }

    [Test]
    public void NoRecordedFactionReadsEquallyForBothAudiences()
    {
        var (state, householdId, _) = OneHousehold();

        var result = RecordScandalCommands.Pipeline.Execute(
            state, new RecordScandalCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, householdId,
                ScandalSourceType.UnjustAction, ScandalSeverity.PublicDisgrace));

        var recorded = result.Events.OfType<ScandalRecordedEvent>().Single();
        state.ScandalRecords.TryGet(recorded.ScandalId, out var record);
        Assert.That(record!.FactionReception.TraditionalistReading, Is.EqualTo(record.FactionReception.PopularistReading));
    }

    // ---- Real wired sources (§4) --------------------------------------------------------------

    [Test]
    public void AnUnjustImprisonAdditivelyRecordsAScandalWithoutDoublingTheDignitasPenalty()
    {
        var (state, householdId, headId) = OneHousehold();
        var dependentId = state.CharacterIds.Issue();
        state.Characters.Add(dependentId, CharacterTestFixtures.Minimal(dependentId, nomen: "Filius", household: householdId));

        var result = ImprisonCommands.Pipeline.Execute(
            state, new ImprisonCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, headId, dependentId,
                ImprisonAuthorityBasis.PatriaPotestas, DetentionLocationType.PrivateErgastulum));

        state.Characters.TryGet(headId, out var head);
        Assert.Multiple(() =>
        {
            // The command's own already-tested behavior is unchanged: exactly one Unjust penalty.
            Assert.That(DignitasResolver.Current(state, householdId), Is.EqualTo(-CrimeCatalog.UnjustImprisonDignitasPenalty));
            Assert.That(result.Events.OfType<ScandalRecordedEvent>().Single().SourceType, Is.EqualTo(ScandalSourceType.UnjustAction));
            Assert.That(head!.Traits, Does.Contain(ScandalCatalog.ScandalMarkedTraitId));
        });
    }

    [Test]
    public void AnUnjustExecutionWithANamedSentencerRecordsAScandalOnTheSentencersHousehold()
    {
        var (state, victimHouseholdId, victimHeadId) = OneHousehold("Victim");
        var (sentencerHouseholdId, sentencerHeadId) = SecondHousehold(state, "Sentencer");

        var result = ApplySentenceCommands.Pipeline.Execute(
            state, new ApplySentenceCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, victimHeadId,
                SentenceType.Crucifixion, SentencingCharacterId: sentencerHeadId));

        state.Characters.TryGet(sentencerHeadId, out var sentencer);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            var recorded = result.Events.OfType<ScandalRecordedEvent>().Single();
            Assert.That(recorded.PrimaryHouseholdId, Is.EqualTo(sentencerHouseholdId));
            Assert.That(sentencer!.Traits, Does.Contain(ScandalCatalog.ScandalMarkedTraitId));
        });
    }

    [Test]
    public void AJustifiedExecutionNeverRecordsAScandal()
    {
        var (state, _, victimHeadId) = OneHousehold("Guilty");
        RecordPunishableOffenseCommands.Pipeline.Execute(
            state, new RecordPunishableOffenseCommand(
                state.CommandIds.Issue(), "system", new GameDate(0), null, victimHeadId,
                PunishableOffenseSource.Fabricated, OffenseSeverity.Capital));
        var (_, sentencerHeadId) = SecondHousehold(state, "Sentencer");

        var result = ApplySentenceCommands.Pipeline.Execute(
            state, new ApplySentenceCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, victimHeadId,
                SentenceType.Crucifixion, SentencingCharacterId: sentencerHeadId));

        Assert.That(result.Events.OfType<ScandalRecordedEvent>(), Is.Empty);
    }

    [Test]
    public void DiscoverFabricationCommandFlipsTheOffenseAndRecordsANotaCensoriaEligibleScandal()
    {
        var (state, householdId, headId) = OneHousehold();
        var recorded = RecordPunishableOffenseCommands.Pipeline.Execute(
            state, new RecordPunishableOffenseCommand(
                state.CommandIds.Issue(), "system", new GameDate(0), null, headId,
                PunishableOffenseSource.Fabricated, OffenseSeverity.Serious, IsFabricated: true));
        var offenseId = recorded.Events.OfType<PunishableOffenseRecordedEvent>().Single().OffenseId;

        var result = DiscoverFabricationCommands.Pipeline.Execute(
            state, new DiscoverFabricationCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, offenseId));

        state.PunishableOffenses.TryGet(offenseId, out var offense);
        var scandal = result.Events.OfType<ScandalRecordedEvent>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(offense!.FabricationDiscovered, Is.True);
            Assert.That(scandal.SourceType, Is.EqualTo(ScandalSourceType.DiscoveredFabrication));
            Assert.That(scandal.Severity, Is.EqualTo(ScandalSeverity.NotaCensoriaEligible));
            Assert.That(scandal.PrimaryHouseholdId, Is.EqualTo(householdId));
        });
    }

    [Test]
    public void DiscoverFabricationCommandRejectsANonFabricatedOffenseAndADoubleDiscovery()
    {
        var (state, _, headId) = OneHousehold();
        var recorded = RecordPunishableOffenseCommands.Pipeline.Execute(
            state, new RecordPunishableOffenseCommand(
                state.CommandIds.Issue(), "system", new GameDate(0), null, headId,
                PunishableOffenseSource.LegalConviction, OffenseSeverity.Serious));
        var realOffenseId = recorded.Events.OfType<PunishableOffenseRecordedEvent>().Single().OffenseId;

        Assert.That(
            DiscoverFabricationCommands.Pipeline.Execute(
                state, new DiscoverFabricationCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, realOffenseId)).Error,
            Is.EqualTo(DiscoverFabricationCommands.NotFabricated));

        var fabricated = RecordPunishableOffenseCommands.Pipeline.Execute(
            state, new RecordPunishableOffenseCommand(
                state.CommandIds.Issue(), "system", new GameDate(1), null, headId,
                PunishableOffenseSource.Fabricated, OffenseSeverity.Serious, IsFabricated: true));
        var fabricatedOffenseId = fabricated.Events.OfType<PunishableOffenseRecordedEvent>().Single().OffenseId;

        DiscoverFabricationCommands.Pipeline.Execute(
            state, new DiscoverFabricationCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, fabricatedOffenseId));

        Assert.That(
            DiscoverFabricationCommands.Pipeline.Execute(
                state, new DiscoverFabricationCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, fabricatedOffenseId)).Error,
            Is.EqualTo(DiscoverFabricationCommands.AlreadyDiscovered));
    }

    [Test]
    public void APatriaPotestasRulingAdditivelyRecordsAScandalWithoutDoublingDignitasOrRelationships()
    {
        var (state, settlementId, plaintiffId, defendantId, plaintiffHeadId, defendantHeadId) = TwoHouseholdsForLegal();
        Fund(state, plaintiffId, Money.FromDenarii(100));

        var streams = new RandomStreamSet();
        streams.AddDerived(FileLawsuitCommands.QuickResolutionStreamName, 1UL);

        var result = FileLawsuitCommands.CreatePipeline(streams).Execute(
            state, new FileLawsuitCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null,
                LegalCaseType.Family, LegalCaseDepth.Quick, plaintiffId, defendantId, settlementId, plaintiffHeadId,
                IsPatriaPotestasCase: true));

        state.Characters.TryGet(defendantHeadId, out var defendantHead);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            var ruled = result.Events.OfType<LegalCaseRuledEvent>().Single();
            Assert.That(ruled.Verdict, Is.EqualTo(LegalCaseVerdict.Dismissed));
            var scandal = result.Events.OfType<ScandalRecordedEvent>().Single();
            Assert.That(scandal.SourceType, Is.EqualTo(ScandalSourceType.WeaponizedLegalCase));
            Assert.That(scandal.PrimaryHouseholdId, Is.EqualTo(defendantId));
            // The harsher Patria Potestas penalty applied above is the only Dignitas movement — no
            // doubling from this additive Scandal call.
            Assert.That(DignitasResolver.Current(state, plaintiffId), Is.EqualTo(-LegalCatalog.PatriaPotestasCaseDignitasPenalty));
            Assert.That(defendantHead!.Traits.Count(id => id == ScandalCatalog.ScandalMarkedTraitId), Is.EqualTo(1));
            Assert.That(state.Relationships.Count, Is.EqualTo(0));
            Assert.That(state.ScandalRecords.Count, Is.EqualTo(1));
        });
    }

    [Test]
    public void DissolvingAnIllicitCollegiumAdditivelyRecordsAScandalWithoutDoublingTheDignitasPenalty()
    {
        var (state, settlementId, patronHouseholdId, patronHeadId) = HouseholdWithHead("Patron");
        var collegiumId = FoundOpificum(state, settlementId);
        SponsorCollegiumCommands.Pipeline.Execute(
            state, new SponsorCollegiumCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, collegiumId, patronHouseholdId));
        var (targetHouseholdId, _) = AddHouseholdWithHead(state, "Target");
        RecordCollegiumOrganizedDisruptionCommands.Pipeline.Execute(
            state, new RecordCollegiumOrganizedDisruptionCommand(
                state.CommandIds.Issue(), "player", new GameDate(2), null, collegiumId, targetHouseholdId));

        var recordId = state.MagistracyRecordIds.Issue();
        state.MagistracyRecords.Add(
            recordId, new MagistracyRecord(recordId, patronHeadId, MagistracyOffice.Decurion, settlementId, new GameDate(0)));

        var beforeDignitas = DignitasResolver.Current(state, patronHouseholdId);
        var result = DissolveCollegiumCommands.Pipeline.Execute(
            state, new DissolveCollegiumCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, collegiumId, patronHeadId));

        state.Characters.TryGet(patronHeadId, out var patronHead);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(
                beforeDignitas - DignitasResolver.Current(state, patronHouseholdId),
                Is.EqualTo(CollegiumCatalog.IllicitPatronDignitasPenalty));
            var scandal = result.Events.OfType<ScandalRecordedEvent>().Single();
            Assert.That(scandal.SourceType, Is.EqualTo(ScandalSourceType.IllicitCollegiumExposure));
            Assert.That(patronHead!.Traits, Does.Contain(ScandalCatalog.ScandalMarkedTraitId));
        });
    }

    // ---- Lifecycle decay (§9) and Rehabilitation (§8) -----------------------------------------

    [Test]
    public void ScandalDecaySystemFadesSeverityThenDeactivates()
    {
        var (state, householdId, _) = OneHousehold();
        var recorded = RecordScandalCommands.Pipeline.Execute(
            state, new RecordScandalCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, householdId,
                ScandalSourceType.UnjustAction, ScandalSeverity.NotaCensoriaEligible));
        var scandalId = recorded.Events.OfType<ScandalRecordedEvent>().Single().ScandalId;

        var system = new ScandalDecaySystem();

        var tooEarly = system.Tick(state, new MonthlyTickContext(new GameDate(ScandalCatalog.SeverityFadeAfterMonths - 1), new RandomStreamSet()));
        Assert.That(tooEarly, Is.Empty);

        system.Tick(state, new MonthlyTickContext(new GameDate(ScandalCatalog.SeverityFadeAfterMonths), new RandomStreamSet()));
        state.ScandalRecords.TryGet(scandalId, out var fadedOnce);
        Assert.Multiple(() =>
        {
            Assert.That(fadedOnce!.Severity, Is.EqualTo(ScandalSeverity.PublicDisgrace));
            Assert.That(fadedOnce.IsActive, Is.True);
        });

        system.Tick(state, new MonthlyTickContext(new GameDate(ScandalCatalog.DeactivateAfterMonths), new RandomStreamSet()));
        state.ScandalRecords.TryGet(scandalId, out var deactivated);
        Assert.That(deactivated!.IsActive, Is.False);
    }

    [Test]
    public void ScandalRehabilitationSystemGrantsTheTraitOnlyAfterTheSustainedGate()
    {
        var (state, householdId, headId) = OneHousehold();
        RecordScandalCommands.Pipeline.Execute(
            state, new RecordScandalCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, householdId,
                ScandalSourceType.UnjustAction, ScandalSeverity.PublicDisgrace));

        var system = new ScandalRehabilitationSystem();

        system.Tick(state, new MonthlyTickContext(new GameDate(ScandalCatalog.RehabilitationAfterMonths - 1), new RandomStreamSet()));
        state.Characters.TryGet(headId, out var tooEarly);
        Assert.That(tooEarly!.Traits, Does.Not.Contain(ScandalCatalog.RehabilitatedTraitId));

        var events = system.Tick(state, new MonthlyTickContext(new GameDate(ScandalCatalog.RehabilitationAfterMonths), new RandomStreamSet()));
        state.Characters.TryGet(headId, out var rehabilitated);
        Assert.Multiple(() =>
        {
            Assert.That(events.OfType<CharacterRehabilitatedEvent>().Single().CharacterId, Is.EqualTo(headId));
            Assert.That(rehabilitated!.Traits, Does.Contain(ScandalCatalog.RehabilitatedTraitId));
            // Additive, not a replacement — the original mark stays on the record.
            Assert.That(rehabilitated.Traits, Does.Contain(ScandalCatalog.ScandalMarkedTraitId));
        });
    }

    [Test]
    public void AFurtherScandalResetsTheRehabilitationClock()
    {
        var (state, householdId, headId) = OneHousehold();
        RecordScandalCommands.Pipeline.Execute(
            state, new RecordScandalCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, householdId,
                ScandalSourceType.UnjustAction, ScandalSeverity.PublicDisgrace));
        RecordScandalCommands.Pipeline.Execute(
            state, new RecordScandalCommand(
                state.CommandIds.Issue(), "player", new GameDate(ScandalCatalog.RehabilitationAfterMonths - 1), null, householdId,
                ScandalSourceType.UnjustAction, ScandalSeverity.MinorEmbarrassment));

        var system = new ScandalRehabilitationSystem();
        system.Tick(state, new MonthlyTickContext(new GameDate(ScandalCatalog.RehabilitationAfterMonths), new RandomStreamSet()));

        state.Characters.TryGet(headId, out var head);
        Assert.That(head!.Traits, Does.Not.Contain(ScandalCatalog.RehabilitatedTraitId));
    }

    // ---- Chronicle projection (§9) -------------------------------------------------------------

    [Test]
    public void OnlyASevereScandalIsChronicled()
    {
        var (state, householdId, _) = OneHousehold();

        var minor = RecordScandalCommands.Pipeline.Execute(
            state, new RecordScandalCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, householdId,
                ScandalSourceType.UnjustAction, ScandalSeverity.MinorEmbarrassment));
        Assert.That(ChronicleProjector.Project(state, minor.Events), Is.Empty);

        var severe = RecordScandalCommands.Pipeline.Execute(
            state, new RecordScandalCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, householdId,
                ScandalSourceType.UnjustAction, ScandalSeverity.PublicDisgrace));
        var drafts = ChronicleProjector.Project(state, severe.Events);
        Assert.That(drafts.Single().Category, Is.EqualTo(ChronicleCategory.FaithAndScandal));
    }

    // ---- Save/load round trip -------------------------------------------------------------------

    [Test]
    public void ScandalStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, householdId, headId) = OneHousehold();
        var (otherHouseholdId, otherHeadId) = SecondHousehold(state, "Rival");

        RecordScandalCommands.Pipeline.Execute(
            state, new RecordScandalCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, householdId,
                ScandalSourceType.UnjustAction, ScandalSeverity.PublicDisgrace, ScarredAgainstCharacterId: otherHeadId));

        new ScandalDecaySystem().Tick(state, new MonthlyTickContext(new GameDate(ScandalCatalog.SeverityFadeAfterMonths), new RandomStreamSet()));
        new ScandalRehabilitationSystem().Tick(state, new MonthlyTickContext(new GameDate(ScandalCatalog.RehabilitationAfterMonths), new RandomStreamSet()));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.ScandalRecords.Count, Is.EqualTo(state.ScandalRecords.Count));
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        });
    }

    // ---- Shared fixtures for the Legal/Collegia integration tests -----------------------------

    private static (WorldState State, RuntimeId<Settlement> SettlementId, RuntimeId<Household> PlaintiffId, RuntimeId<Household> DefendantId, RuntimeId<Character> PlaintiffHeadId, RuntimeId<Character> DefendantHeadId)
        TwoHouseholdsForLegal()
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

    private static (WorldState State, RuntimeId<Settlement> SettlementId, RuntimeId<Household> HouseholdId, RuntimeId<Character> HeadId)
        HouseholdWithHead(string nomen)
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        var householdId = state.HouseholdIds.Issue();
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, nomen: nomen, household: householdId));
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, headId, new GameDate(0)));

        return (state, settlementId, householdId, headId);
    }

    private static (RuntimeId<Household> HouseholdId, RuntimeId<Character> HeadId) AddHouseholdWithHead(WorldState state, string nomen)
    {
        var householdId = state.HouseholdIds.Issue();
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, nomen: nomen, household: householdId));
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, headId, new GameDate(0)));
        return (householdId, headId);
    }

    private static RuntimeId<Actor> FoundOpificum(WorldState state, RuntimeId<Settlement> settlementId)
    {
        var result = FoundCollegiumCommands.Pipeline.Execute(
            state,
            new FoundCollegiumCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, "Collegium Fabrorum", settlementId,
                CollegiumType.Opificum, LinkedPopGroupType: PopGroupType.Opifices));
        return ((CollegiumFoundedEvent)result.Events[0]).CollegiumId;
    }
}
