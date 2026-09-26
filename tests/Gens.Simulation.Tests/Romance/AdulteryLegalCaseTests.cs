using System.Linq;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Legal;
using Gens.Simulation.Random;
using Gens.Simulation.Reputation;
using Gens.Simulation.Romance;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;
using CharacterTestFixtures = Gens.Simulation.Tests.Characters.CharacterTestFixtures;

namespace Gens.Simulation.Tests.Romance;

/// <summary>Phase 17 item 3 slice 9 coverage (<c>gens-romance-sexuality-lineage-design.md</c> §12, §13):
/// <see cref="FileAdulteryCaseCommand"/> opening a real §12 Adultery case, and a full Major-case
/// advancement to <see cref="LegalCaseVerdict.Convicted"/> exercising <see
/// cref="AdulteryResolutionHook"/>'s own <see cref="LegalSentence.Relegatio"/> sentence, partial-property
/// confiscation, additive Status/Role Dignitas modifier, and real <see cref="InfamiaStatus"/> grant.
/// Follows <see cref="Gens.Simulation.Tests.PublicContracts.PublicContractsTests"/>'s own established
/// "seed search until a Convicted verdict lands" idiom for driving a Major case to verdict.</summary>
public sealed class AdulteryLegalCaseTests
{
    private static readonly GameDate Epoch = new(0);

    private static (WorldState State, RuntimeId<Settlement> SettlementId) OneSettlement()
    {
        var state = new WorldState(Epoch);
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId, SettlementStage.Vicus));
        return (state, settlementId);
    }

    private static RuntimeId<Character> AddAdult(
        WorldState state, RuntimeId<Household> household, RuntimeId<Settlement> location,
        Sex sex = Sex.Male, LegalStatus status = LegalStatus.RomanCitizen)
    {
        var id = state.CharacterIds.Issue();
        state.Characters.Add(
            id,
            CharacterTestFixtures.Minimal(
                id, sex: sex, birthDate: new GameDate(-30 * 12), household: household, location: location, status: status));
        return id;
    }

    private static void Fund(WorldState state, LedgerAccountKey account, Money amount) =>
        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Treasury,
            new[] { new LedgerPosting(account, amount), new LedgerPosting(LedgerAccountKey.Mint, -amount) });

    private static (RuntimeId<AffairRecord> AffairId, AffairRecord Record) SeedUnresolvedHighStakesAffair(
        WorldState state, RuntimeId<Character> offender, RuntimeId<Character> thirdParty, RuntimeId<Character> wrongedSpouse)
    {
        var affairId = state.AffairRecordIds.Issue();
        var record = new AffairRecord(
            affairId, offender, thirdParty, wrongedSpouse, AffairStakesLevel.HighStakes,
            InvolvesRivalHouse: false, LegitimacyContested: false, ThreatensPoliticalMarriage: false,
            Resolution: null, StatusRoleDignitasModifier: 0, DiscoveredDate: Epoch);
        state.AffairRecords.Add(affairId, record);
        return (affairId, record);
    }

    [Test]
    public void FileAdulteryCaseCommandCreatesAMajorCaseWithTheRightHouseholdsAndLinksTheAffair()
    {
        var (state, settlementId) = OneSettlement();
        var accusingHouseholdId = state.HouseholdIds.Issue();
        var defendantHouseholdId = state.HouseholdIds.Issue();
        var wrongedSpouse = AddAdult(state, accusingHouseholdId, settlementId, sex: Sex.Female);
        var offender = AddAdult(state, defendantHouseholdId, settlementId);
        var thirdParty = AddAdult(state, defendantHouseholdId, settlementId, sex: Sex.Female);
        Fund(state, LedgerAccountKey.ForHousehold(accusingHouseholdId), LegalCatalog.MajorFilingCost);
        var (affairId, _) = SeedUnresolvedHighStakesAffair(state, offender, thirdParty, wrongedSpouse);

        var result = FileAdulteryCaseCommands.CreatePipeline(new RandomStreamSet()).Execute(
            state, new FileAdulteryCaseCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, affairId, settlementId, wrongedSpouse));

        Assert.That(result.Accepted, Is.True, $"Rejected: {result.Error}");

        var caseId = result.Events.OfType<LawsuitFiledEvent>().Single().CaseId;
        state.LegalCases.TryGet(caseId, out var legalCase);
        state.AdulteryCaseLinks.TryGet(caseId, out var link);
        state.AffairRecords.TryGet(affairId, out var record);

        Assert.Multiple(() =>
        {
            Assert.That(legalCase!.CaseType, Is.EqualTo(LegalCaseType.Adultery));
            Assert.That(legalCase.Depth, Is.EqualTo(LegalCaseDepth.Major));
            Assert.That(legalCase.PlaintiffId, Is.EqualTo(accusingHouseholdId));
            Assert.That(legalCase.DefendantId, Is.EqualTo(defendantHouseholdId));
            Assert.That(link!.AffairId, Is.EqualTo(affairId));
            Assert.That(record!.Resolution, Is.EqualTo(AffairResolution.ProsecutedAdultery));
            Assert.That(record.LegalCaseId, Is.EqualTo(caseId));
        });
    }

    [Test]
    public void FileAdulteryCaseCommandRejectsAnAffairAlreadyLinkedToACase()
    {
        var (state, settlementId) = OneSettlement();
        var accusingHouseholdId = state.HouseholdIds.Issue();
        var defendantHouseholdId = state.HouseholdIds.Issue();
        var wrongedSpouse = AddAdult(state, accusingHouseholdId, settlementId, sex: Sex.Female);
        var offender = AddAdult(state, defendantHouseholdId, settlementId);
        var thirdParty = AddAdult(state, defendantHouseholdId, settlementId, sex: Sex.Female);
        Fund(state, LedgerAccountKey.ForHousehold(accusingHouseholdId), Money.FromDenarii(40));
        var (affairId, _) = SeedUnresolvedHighStakesAffair(state, offender, thirdParty, wrongedSpouse);

        var streams = new RandomStreamSet();
        var first = FileAdulteryCaseCommands.CreatePipeline(streams).Execute(
            state, new FileAdulteryCaseCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, affairId, settlementId, wrongedSpouse));
        Assert.That(first.Accepted, Is.True, $"Rejected: {first.Error}");

        var second = FileAdulteryCaseCommands.CreatePipeline(streams).Execute(
            state, new FileAdulteryCaseCommand(
                state.CommandIds.Issue(), "player", new GameDate(2), null, affairId, settlementId, wrongedSpouse));

        Assert.That(second.Accepted, Is.False);
        Assert.That(second.Error, Is.EqualTo(FileAdulteryCaseCommands.AffairAlreadyResolved));
    }

    [Test]
    public void AFullAdvancementToConvictedYieldsRelegatioConfiscationTheStatusRoleModifierAndInfamia()
    {
        for (var seed = 1UL; seed <= 80UL; seed++)
        {
            var (state, settlementId) = OneSettlement();
            var accusingHouseholdId = state.HouseholdIds.Issue();
            var defendantHouseholdId = state.HouseholdIds.Issue();
            // A status gap (Roman Citizen offender, Freedman third party) so the Status/Role modifier is
            // real and nonzero (§13).
            var wrongedSpouse = AddAdult(state, accusingHouseholdId, settlementId, sex: Sex.Female);
            var offender = AddAdult(state, defendantHouseholdId, settlementId, status: LegalStatus.RomanCitizen);
            var thirdParty = AddAdult(state, defendantHouseholdId, settlementId, sex: Sex.Female, status: LegalStatus.Freedman);
            Fund(state, LedgerAccountKey.ForHousehold(accusingHouseholdId), Money.FromDenarii(1_000));
            Fund(state, LedgerAccountKey.ForHousehold(defendantHouseholdId), Money.FromDenarii(1_000));
            // Skew Dignitas heavily toward the plaintiff so the weighted verdict check reliably prevails,
            // matching PublicContractsTests' own identical seeding idiom.
            AdjustDignitasCommands.Pipeline.Execute(
                state, new AdjustDignitasCommand(state.CommandIds.Issue(), "system", Epoch, null, accusingHouseholdId, 500, "seed"));
            AdjustDignitasCommands.Pipeline.Execute(
                state, new AdjustDignitasCommand(state.CommandIds.Issue(), "system", Epoch, null, defendantHouseholdId, -500, "seed"));

            var expectedModifier = StatusRoleDignitasModifier.Calculate(state, offender, thirdParty);
            var (affairId, _) = SeedUnresolvedHighStakesAffair(state, offender, thirdParty, wrongedSpouse);

            var streams = new RandomStreamSet();
            streams.AddDerived(LegalCaseAdvancementSystem.VerdictOutcomeStreamName, seed);

            var filed = FileAdulteryCaseCommands.CreatePipeline(streams).Execute(
                state, new FileAdulteryCaseCommand(
                    state.CommandIds.Issue(), "player", new GameDate(20), null, affairId, settlementId, wrongedSpouse));
            Assert.That(filed.Accepted, Is.True, $"Filing was rejected: {filed.Error}");
            var caseId = filed.Events.OfType<LawsuitFiledEvent>().Single().CaseId;

            var dignitasBeforeRuling = DignitasResolver.Current(state, defendantHouseholdId);
            state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(defendantHouseholdId), out var accountBeforeRuling);
            var balanceBeforeRuling = accountBeforeRuling!.Balance;

            var system = new LegalCaseAdvancementSystem();
            system.Tick(state, new MonthlyTickContext(new GameDate(20 + LegalCatalog.MajorCaseEvidenceGatheringMonths), streams));
            system.Tick(state, new MonthlyTickContext(new GameDate(20 + LegalCatalog.MajorCaseEvidenceGatheringMonths + 1), streams));

            state.LegalCases.TryGet(caseId, out var legalCase);
            if (legalCase!.Verdict != LegalCaseVerdict.Convicted)
                continue;

            state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(defendantHouseholdId), out var accountAfterRuling);
            var expectedConfiscation = balanceBeforeRuling.Scale(RomanceCatalog.AdulteryConfiscationFraction);
            var dignitasAfterRuling = DignitasResolver.Current(state, defendantHouseholdId);

            state.InfamiaStatuses.TryGet(offender, out var infamia);

            Assert.Multiple(() =>
            {
                Assert.That(legalCase.Sentence, Is.EqualTo(LegalSentence.Relegatio));
                Assert.That(accountAfterRuling!.Balance, Is.EqualTo(balanceBeforeRuling - expectedConfiscation));
                Assert.That(
                    dignitasAfterRuling,
                    Is.EqualTo(dignitasBeforeRuling - LegalCatalog.ConvictedDignitasLoss + expectedModifier));
                Assert.That(infamia, Is.Not.Null, "Expected a real InfamiaStatus on the convicted offender.");
                Assert.That(infamia!.Source, Is.EqualTo(InfamiaSource.ConvictedAdultery));
            });
            return;
        }

        Assert.Fail("No seed in the searched range produced a Convicted adultery verdict.");
    }
}
