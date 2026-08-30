using Gens.Simulation.Characters;
using Gens.Simulation.Clientela;
using Gens.Simulation.Commands;
using Gens.Simulation.Doctrine;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Legal;
using Gens.Simulation.Magistracies;
using Gens.Simulation.Policies;
using Gens.Simulation.Random;
using Gens.Simulation.Religion;
using Gens.Simulation.Reputation;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Doctrine;

/// <summary>Phase 12 item 9 coverage: <see cref="DoctrineResolutionSystem"/>'s monthly Affinity read
/// for the three real, reachable Doctrines (Mos Maiorum, Domus Pia, Domus Dura), the Emerging/Defining
/// threshold mechanic, and each Doctrine's own Defining capstone command (<see
/// cref="InvokeAncestralSanctionCommand"/>, <see cref="PerformGreatRiteCommand"/>, <see
/// cref="ActivateIronHandCommand"/>/<see cref="DoctrineLaborModifierQuery"/>).</summary>
public sealed class DoctrineTests
{
    private static (WorldState State, RuntimeId<Household> HouseholdId, RuntimeId<Character> HeadId) OneHousehold()
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));

        var householdId = state.HouseholdIds.Issue();
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, nomen: "Fabius", household: householdId));
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

    private static void TickResolution(WorldState state, int month) =>
        new DoctrineResolutionSystem().Tick(state, new MonthlyTickContext(new GameDate(month), new RandomStreamSet()));

    // ---- Mos Maiorum: Rites Budget + Traditionalist Faction --------------------------------------

    [Test]
    public void MosMaiorumAffinityRisesWithLavishRitesBudgetAndTraditionalistFactionAndReachesEmergingThenDefining()
    {
        var (state, householdId, headId) = OneHousehold();
        ChangeRitesBudgetCommands.Pipeline.Execute(
            state, new ChangeRitesBudgetCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, RitesBudgetTier.Lavish));
        CharacterFactionResolver.Set(state, headId, PoliticalFaction.Traditionalist);

        DoctrineTier tier = DoctrineTier.None;
        for (var month = 1; month <= 12; month++)
        {
            TickResolution(state, month);
            tier = HouseholdDoctrineResolver.Current(state, householdId, HouseholdDoctrineType.MosMaiorum).Tier;
            if (tier == DoctrineTier.Defining)
                break;
        }

        Assert.That(tier, Is.EqualTo(DoctrineTier.Defining));
    }

    [Test]
    public void UnfedAffinityDecaysAndNeverReachesEmerging()
    {
        var (state, householdId, _) = OneHousehold();

        for (var month = 1; month <= 6; month++)
            TickResolution(state, month);

        var doctrine = HouseholdDoctrineResolver.Current(state, householdId, HouseholdDoctrineType.MosMaiorum);
        Assert.Multiple(() =>
        {
            Assert.That(doctrine.Tier, Is.EqualTo(DoctrineTier.None));
            Assert.That(doctrine.AffinityScore, Is.EqualTo(0));
        });
    }

    [Test]
    public void ContradictingSignalsLowerAffinityFasterThanUnfedDecay()
    {
        var (state, householdId, headId) = OneHousehold();
        ChangeRitesBudgetCommands.Pipeline.Execute(
            state, new ChangeRitesBudgetCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, RitesBudgetTier.Lavish));
        CharacterFactionResolver.Set(state, headId, PoliticalFaction.Traditionalist);

        // Build up some Affinity first.
        for (var month = 1; month <= 5; month++)
            TickResolution(state, month);
        var built = HouseholdDoctrineResolver.Current(state, householdId, HouseholdDoctrineType.MosMaiorum).AffinityScore;
        Assert.That(built, Is.GreaterThan(0));

        // Now flip both signals to actively contradict.
        ChangeRitesBudgetCommands.Pipeline.Execute(
            state, new ChangeRitesBudgetCommand(state.CommandIds.Issue(), "player", new GameDate(10), null, householdId, RitesBudgetTier.Frugal));
        CharacterFactionResolver.Set(state, headId, PoliticalFaction.Popularist);
        TickResolution(state, 11);

        var after = HouseholdDoctrineResolver.Current(state, householdId, HouseholdDoctrineType.MosMaiorum).AffinityScore;
        Assert.That(after, Is.LessThan(built));
    }

    // ---- Domus Pia: Rites Budget + Piety trait + Priesthood --------------------------------------

    [Test]
    public void DomusPiaAffinityRisesWithLavishRitesBudgetPietyTraitAndPriesthood()
    {
        var (state, householdId, headId) = OneHousehold();
        ChangeRitesBudgetCommands.Pipeline.Execute(
            state, new ChangeRitesBudgetCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, RitesBudgetTier.Lavish));

        state.Characters.TryGet(headId, out var head);
        state.Characters.Remove(headId);
        state.Characters.Add(headId, head! with { Traits = new[] { ReligionCatalog.DevoutTraitId } });

        var settlementId = state.Settlements.InAscendingOrder().First().Key;
        var priesthoodRecordId = state.PriesthoodRecordIds.Issue();
        state.PriesthoodRecords.Add(
            priesthoodRecordId, new PriesthoodRecord(priesthoodRecordId, headId, PriesthoodOffice.Augur, settlementId, new GameDate(0)));

        for (var month = 1; month <= 15; month++)
            TickResolution(state, month);

        Assert.That(
            HouseholdDoctrineResolver.Current(state, householdId, HouseholdDoctrineType.DomusPia).Tier,
            Is.EqualTo(DoctrineTier.Defining));
    }

    // ---- Domus Dura: household Regimen + Proscription ---------------------------------------------

    [Test]
    public void DomusDuraAffinityRisesWithHarshConfinedRegimen()
    {
        var (state, householdId, _) = OneHousehold();
        var harsh = new RegimenSettings(DietTier.Meager, AccommodationTier.Bare, FreedomsTier.Confined, DisciplineTier.Harsh);
        SetGroupRegimenCommands.Pipeline.Execute(
            state, new SetGroupRegimenCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, null, harsh));

        for (var month = 1; month <= 12; month++)
            TickResolution(state, month);

        var doctrine = HouseholdDoctrineResolver.Current(state, householdId, HouseholdDoctrineType.DomusDura);
        Assert.That(doctrine.Tier, Is.EqualTo(DoctrineTier.Defining).Or.EqualTo(DoctrineTier.Emerging));
        Assert.That(doctrine.AffinityScore, Is.GreaterThan(0));
    }

    // ---- Mos Maiorum capstone: Ancestral Sanction --------------------------------------------------

    private static void ForceMosMaiorumToDefining(WorldState state, RuntimeId<Household> householdId) =>
        HouseholdDoctrineResolver.Set(
            state, new HouseholdDoctrineState(householdId, HouseholdDoctrineType.MosMaiorum, 100, DoctrineTier.Defining, CapstoneUnlocked: true));

    private static (RuntimeId<Household> DefendantId, RuntimeId<LegalCase> CaseId) ConvictedCaseAgainst(WorldState state, RuntimeId<Household> defendantId)
    {
        var plaintiffId = state.HouseholdIds.Issue();
        var caseId = state.LegalCaseIds.Issue();
        state.LegalCases.Add(caseId, new LegalCase(
            caseId, LegalCaseType.Political, plaintiffId, defendantId, state.Settlements.InAscendingOrder().First().Key,
            LegalCaseDepth.Quick, LegalCaseStage.Ruled, new GameDate(0), Verdict: LegalCaseVerdict.Convicted));
        return (defendantId, caseId);
    }

    [Test]
    public void InvokeAncestralSanctionRejectedWhenDoctrineNotDefining()
    {
        var (state, householdId, _) = OneHousehold();
        var (_, caseId) = ConvictedCaseAgainst(state, householdId);

        var result = InvokeAncestralSanctionCommands.Pipeline.Execute(
            state, new InvokeAncestralSanctionCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, caseId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Error, Is.EqualTo(InvokeAncestralSanctionCommands.DoctrineNotDefining));
        });
    }

    [Test]
    public void InvokeAncestralSanctionOverturnsVerdictAndRestoresDignitasThenCannotBeUsedTwice()
    {
        var (state, householdId, _) = OneHousehold();
        ForceMosMaiorumToDefining(state, householdId);
        var (_, caseId) = ConvictedCaseAgainst(state, householdId);
        AdjustDignitasCommands.Pipeline.Execute(
            state, new AdjustDignitasCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, -50, "conviction"));

        var before = DignitasResolver.Current(state, householdId);
        var result = InvokeAncestralSanctionCommands.Pipeline.Execute(
            state, new InvokeAncestralSanctionCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, caseId));

        state.LegalCases.TryGet(caseId, out var legalCase);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(legalCase!.Verdict, Is.EqualTo(LegalCaseVerdict.Dismissed));
            Assert.That(DignitasResolver.Current(state, householdId), Is.EqualTo(before + DoctrineCatalog.AncestralSanctionDignitasRestored));
        });

        var second = InvokeAncestralSanctionCommands.Pipeline.Execute(
            state, new InvokeAncestralSanctionCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, householdId, caseId));
        Assert.That(second.Accepted, Is.False);
        Assert.That(second.Error, Is.EqualTo(InvokeAncestralSanctionCommands.CapstoneAlreadyUsed));
    }

    // ---- Domus Pia capstone: The Great Rite --------------------------------------------------------

    [Test]
    public void PerformGreatRiteRejectedWithoutPatronDeity()
    {
        var (state, householdId, _) = OneHousehold();
        HouseholdDoctrineResolver.Set(
            state, new HouseholdDoctrineState(householdId, HouseholdDoctrineType.DomusPia, 100, DoctrineTier.Defining, CapstoneUnlocked: true));
        Fund(state, householdId, DoctrineCatalog.GreatRiteCost);

        var result = PerformGreatRiteCommands.Pipeline.Execute(
            state, new PerformGreatRiteCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Error, Is.EqualTo(PerformGreatRiteCommands.NoPatronDeity));
        });
    }

    [Test]
    public void PerformGreatRiteSpendsGrantsFavorAndDignitasThenCannotRepeat()
    {
        var (state, householdId, headId) = OneHousehold();
        SetPatronDeityCommands.Pipeline.Execute(
            state, new SetPatronDeityCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, PatronDeity.Jupiter, headId));
        HouseholdDoctrineResolver.Set(
            state, new HouseholdDoctrineState(householdId, HouseholdDoctrineType.DomusPia, 100, DoctrineTier.Defining, CapstoneUnlocked: true));
        Fund(state, householdId, DoctrineCatalog.GreatRiteCost);

        var favorBefore = state.HouseholdReligions.TryGet(householdId, out var religion) ? religion!.Favor : 0;
        var dignitasBefore = DignitasResolver.Current(state, householdId);

        var result = PerformGreatRiteCommands.Pipeline.Execute(
            state, new PerformGreatRiteCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId));

        state.HouseholdReligions.TryGet(householdId, out var religionAfter);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(religionAfter!.Favor, Is.EqualTo(favorBefore + DoctrineCatalog.GreatRiteFavorGain));
            Assert.That(DignitasResolver.Current(state, householdId), Is.EqualTo(dignitasBefore + DoctrineCatalog.GreatRiteDignitasGain));
            Assert.That(
                state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var account) ? account!.Balance : Money.Zero,
                Is.EqualTo(Money.Zero));
        });

        var second = PerformGreatRiteCommands.Pipeline.Execute(
            state, new PerformGreatRiteCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, householdId));
        Assert.That(second.Accepted, Is.False);
        Assert.That(second.Error, Is.EqualTo(PerformGreatRiteCommands.CapstoneAlreadyUsed));
    }

    // ---- Domus Dura capstone: Iron Hand -------------------------------------------------------------

    [Test]
    public void ActivateIronHandRequiresDefiningTierAndProjectsItsBonusOnceActive()
    {
        var (state, householdId, _) = OneHousehold();

        var early = ActivateIronHandCommands.Pipeline.Execute(
            state, new ActivateIronHandCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId));
        Assert.That(early.Accepted, Is.False);
        Assert.That(DoctrineLaborModifierQuery.OutputCeilingBonus(state, householdId), Is.EqualTo(0));

        HouseholdDoctrineResolver.Set(
            state, new HouseholdDoctrineState(householdId, HouseholdDoctrineType.DomusDura, 100, DoctrineTier.Defining, CapstoneUnlocked: true));
        var result = ActivateIronHandCommands.Pipeline.Execute(
            state, new ActivateIronHandCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, householdId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(DoctrineLaborModifierQuery.IsIronHandActive(state, householdId), Is.True);
            Assert.That(DoctrineLaborModifierQuery.OutputCeilingBonus(state, householdId), Is.EqualTo(DoctrineCatalog.IronHandOutputCeilingBonus));
            Assert.That(
                DoctrineLaborModifierQuery.FlightRiskBaselineIncrease(state, householdId),
                Is.EqualTo(DoctrineCatalog.IronHandFlightRiskBaselineIncrease));
        });
    }

    // ---- Save/load round trip ------------------------------------------------------------------------

    [Test]
    public void DoctrineStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, householdId, headId) = OneHousehold();
        ChangeRitesBudgetCommands.Pipeline.Execute(
            state, new ChangeRitesBudgetCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, RitesBudgetTier.Lavish));
        CharacterFactionResolver.Set(state, headId, PoliticalFaction.Traditionalist);
        for (var month = 1; month <= 12; month++)
            TickResolution(state, month);

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.HouseholdDoctrines.Count, Is.EqualTo(state.HouseholdDoctrines.Count));
            Assert.That(
                HouseholdDoctrineResolver.Current(restored, householdId, HouseholdDoctrineType.MosMaiorum).Tier,
                Is.EqualTo(HouseholdDoctrineResolver.Current(state, householdId, HouseholdDoctrineType.MosMaiorum).Tier));
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        });
    }
}
