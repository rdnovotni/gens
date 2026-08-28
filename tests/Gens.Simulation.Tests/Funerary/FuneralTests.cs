using Gens.Simulation.Characters;
using Gens.Simulation.Chronicle;
using Gens.Simulation.Funerary;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Random;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Funerary;

/// <summary>Phase 11 item 4 coverage: funeral opening/tier choice/auto-resolution, mourning periods,
/// the Memoria axis, and the <c>Parentalia</c> Manes-cult trickle
/// (<c>gens-ancestor-veneration-funerary-customs-design.md</c>).</summary>
public sealed class FuneralTests
{
    private static RuntimeId<Household> Establish(WorldState state, RuntimeId<Character> headId, GameDate since)
    {
        var householdId = state.HouseholdIds.Issue();
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, headId, since));
        return householdId;
    }

    private static void Fund(WorldState state, RuntimeId<Household> householdId, Money amount)
    {
        LedgerService.Post(
            state, new GameDate(0), LedgerTransactionCategory.Treasury,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.Mint, -amount),
                new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), amount),
            });
    }

    private static (WorldState State, RuntimeId<Household> HouseholdId, RuntimeId<Character> DeceasedId) DeadHouseholdMember(GameDate deathDate)
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        var householdId = Establish(state, headId, new GameDate(0));
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, household: householdId));

        var memberId = state.CharacterIds.Issue();
        state.Characters.Add(memberId, CharacterTestFixtures.Minimal(
            memberId, household: householdId, deathRecord: new DeathRecord(deathDate, DeathCause.OldAge, 70)));

        return (state, householdId, memberId);
    }

    [Test]
    public void FuneralOpeningSystemDeclaresTheRelationshipsActorsPhaseAndReadWriteSet()
    {
        var system = new FuneralOpeningSystem();
        Assert.Multiple(() =>
        {
            Assert.That(system.Phase, Is.EqualTo(TickPhase.RelationshipsActors));
            Assert.That(system.Reads, Is.EquivalentTo(new[] { "characters", "funeralRecords", "mourningPeriods" }));
            Assert.That(system.Writes, Is.EquivalentTo(new[] { "funeralRecords", "funeralRecordIds", "mourningPeriods", "eventIds" }));
        });
    }

    [Test]
    public void ADeathOpensAPendingFuneralAndStartsAMourningPeriod()
    {
        var (state, householdId, deceasedId) = DeadHouseholdMember(new GameDate(1));

        var events = new FuneralOpeningSystem().Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));

        Assert.Multiple(() =>
        {
            Assert.That(events.OfType<FuneralOpenedEvent>().Count(), Is.EqualTo(1));
            Assert.That(events.OfType<MourningPeriodStartedEvent>().Count(), Is.EqualTo(1));
            Assert.That(state.FuneralRecords.Count, Is.EqualTo(1));
            var funeral = state.FuneralRecords.InAscendingOrder().First().Value;
            Assert.That(funeral.Status, Is.EqualTo(FuneralStatus.Pending));
            Assert.That(funeral.HouseholdId, Is.EqualTo(householdId));
            Assert.That(funeral.DeceasedCharacterId, Is.EqualTo(deceasedId));

            Assert.That(state.MourningPeriods.TryGet(householdId, out var mourning), Is.True);
            Assert.That(mourning!.IsActiveOn(new GameDate(1)), Is.True);
            Assert.That(mourning.BrokenEarly, Is.False);
        });
    }

    [Test]
    public void OpeningIsIdempotentAcrossRepeatedTicksForTheSameDeath()
    {
        var (state, _, _) = DeadHouseholdMember(new GameDate(1));
        var context = new MonthlyTickContext(new GameDate(1), new RandomStreamSet());
        var system = new FuneralOpeningSystem();

        system.Tick(state, context);
        var secondTickEvents = system.Tick(state, new MonthlyTickContext(new GameDate(2), new RandomStreamSet()));

        Assert.Multiple(() =>
        {
            Assert.That(state.FuneralRecords.Count, Is.EqualTo(1));
            Assert.That(secondTickEvents, Is.Empty);
        });
    }

    [Test]
    public void ASecondDeathDuringActiveMourningExtendsRatherThanStacks()
    {
        var (state, householdId, _) = DeadHouseholdMember(new GameDate(1));
        var secondId = state.CharacterIds.Issue();
        state.Characters.Add(secondId, CharacterTestFixtures.Minimal(secondId, household: householdId));

        var openingSystem = new FuneralOpeningSystem();
        openingSystem.Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));
        var firstEnd = state.MourningPeriods.InAscendingOrder().First().Value.EndDate;

        // The second household member dies only now, in month 2 — mirrors how
        // CharacterLifecycleSystem sets DeathRecord the same tick death actually occurs, rather than
        // both deaths being visible to the very first Tick call.
        state.Characters.Remove(secondId);
        state.Characters.Add(secondId, CharacterTestFixtures.Minimal(
            secondId, household: householdId, deathRecord: new DeathRecord(new GameDate(2), DeathCause.OldAge, 40)));

        openingSystem.Tick(state, new MonthlyTickContext(new GameDate(2), new RandomStreamSet()));
        var extendedEnd = state.MourningPeriods.InAscendingOrder().First().Value.EndDate;

        Assert.Multiple(() =>
        {
            Assert.That(state.FuneralRecords.Count, Is.EqualTo(2));
            Assert.That(extendedEnd.TotalMonths, Is.GreaterThan(firstEnd.TotalMonths));
        });
    }

    [Test]
    public void ChoosingAFuneralTierRaisesMemoriaAndSpendsTheTreasury()
    {
        var (state, householdId, deceasedId) = DeadHouseholdMember(new GameDate(1));
        Fund(state, householdId, Money.FromDenarii(1000));
        new FuneralOpeningSystem().Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));
        var funeralId = state.FuneralRecords.InAscendingOrder().First().Key;

        var command = new ChooseFuneralTierCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, funeralId, FuneralTier.Proper);
        var result = ChooseFuneralTierCommands.Pipeline.Execute(state, command);
        Assert.That(result.Accepted, Is.True);
        var events = result.Events;

        Assert.Multiple(() =>
        {
            Assert.That(events.OfType<FuneralHeldEvent>().Count(), Is.EqualTo(1));
            var held = events.OfType<FuneralHeldEvent>().Single();
            Assert.That(held.Tier, Is.EqualTo(FuneralTier.Proper));
            Assert.That(held.DeceasedCharacterId, Is.EqualTo(deceasedId));
            Assert.That(held.AutoResolved, Is.False);

            state.FuneralRecords.TryGet(funeralId, out var funeral);
            Assert.That(funeral!.Status, Is.EqualTo(FuneralStatus.Held));
            Assert.That(funeral.BurialMethod, Is.EqualTo(BurialMethod.Cremation));

            Assert.That(MemoriaResolver.Current(state, householdId), Is.EqualTo(FuneraryCatalog.BaseMemoriaGain(FuneralTier.Proper)));

            state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var account);
            Assert.That(account!.Balance, Is.EqualTo(Money.FromDenarii(1000) - FuneraryCatalog.TreasuryCost(FuneralTier.Proper)));
        });
    }

    [Test]
    public void AGrandFuneralYieldScalesWithExistingAncestralChronicleAchievement()
    {
        var (state, householdId, _) = DeadHouseholdMember(new GameDate(1));
        Fund(state, householdId, Money.FromDenarii(1000));

        // Seed two pre-existing Major-tier Chronicle entries for this household, simulating real
        // ancestral achievement already on record before this funeral.
        for (var i = 0; i < 2; i++)
        {
            var entryId = state.ChronicleEntryIds.Issue();
            state.ChronicleEntries.Add(entryId, new ChronicleEntry(
                entryId, householdId, new GameDate(0), ChronicleCategory.PoliticsAndOffice, ChronicleTier.Major,
                "An ancestor's achievement.", Array.Empty<RuntimeId<Character>>(), "test.seed", ChronicleEntrySource.System));
        }

        new FuneralOpeningSystem().Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));
        var funeralId = state.FuneralRecords.InAscendingOrder().First().Key;

        var command = new ChooseFuneralTierCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, funeralId, FuneralTier.Grand);
        var result = ChooseFuneralTierCommands.Pipeline.Execute(state, command);

        var held = result.Events.OfType<FuneralHeldEvent>().Single();
        var expectedBonus = Math.Min(2 * FuneraryCatalog.AncestralAchievementMemoriaPerEntry, FuneraryCatalog.AncestralAchievementMemoriaCap);

        Assert.Multiple(() =>
        {
            Assert.That(held.MemoriaGained, Is.EqualTo(FuneraryCatalog.BaseMemoriaGain(FuneralTier.Grand) + expectedBonus));
            Assert.That(held.ImaginesDisplayed, Is.True);
        });
    }

    [Test]
    public void ChoosingATierTwiceForTheSameFuneralFails()
    {
        var (state, householdId, _) = DeadHouseholdMember(new GameDate(1));
        Fund(state, householdId, Money.FromDenarii(1000));
        new FuneralOpeningSystem().Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));
        var funeralId = state.FuneralRecords.InAscendingOrder().First().Key;

        var first = new ChooseFuneralTierCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, funeralId, FuneralTier.Modest);
        ChooseFuneralTierCommands.Pipeline.Execute(state, first);

        var second = new ChooseFuneralTierCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, funeralId, FuneralTier.Grand);
        var result = ChooseFuneralTierCommands.Pipeline.Execute(state, second);

        Assert.That(result.Error, Is.EqualTo(ChooseFuneralTierCommands.FuneralAlreadyHeld));
    }

    [Test]
    public void InsufficientTreasuryRejectsTheChosenTier()
    {
        var (state, _, _) = DeadHouseholdMember(new GameDate(1));
        new FuneralOpeningSystem().Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));
        var funeralId = state.FuneralRecords.InAscendingOrder().First().Key;

        var command = new ChooseFuneralTierCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, funeralId, FuneralTier.Grand);
        var result = ChooseFuneralTierCommands.Pipeline.Execute(state, command);

        Assert.That(result.Error, Is.EqualTo(ChooseFuneralTierCommands.InsufficientTreasury));
    }

    [Test]
    public void AStalePendingFuneralAutoResolvesAtTheDefaultTier()
    {
        var (state, householdId, _) = DeadHouseholdMember(new GameDate(1));
        Fund(state, householdId, Money.FromDenarii(1000));
        new FuneralOpeningSystem().Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));

        var dueDate = new GameDate(1 + FuneraryCatalog.FuneralAutoResolutionAfterMonths);
        var events = new FuneralAutoResolutionSystem().Tick(state, new MonthlyTickContext(dueDate, new RandomStreamSet()));

        var held = events.OfType<FuneralHeldEvent>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(held.Tier, Is.EqualTo(FuneraryCatalog.AutoResolutionDefaultTier));
            Assert.That(held.AutoResolved, Is.True);
            Assert.That(held.CausationId, Is.Null);
        });
    }

    [Test]
    public void BreakingMourningEarlySetsTheFlagAndFailsWithoutAnActivePeriod()
    {
        var (state, householdId, deceasedId) = DeadHouseholdMember(new GameDate(1));
        new FuneralOpeningSystem().Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));

        var command = new BreakMourningEarlyCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId);
        var result = BreakMourningEarlyCommands.Pipeline.Execute(state, command);
        Assert.That(result.Accepted, Is.True);

        state.MourningPeriods.TryGet(householdId, out var mourning);
        Assert.Multiple(() =>
        {
            Assert.That(mourning!.BrokenEarly, Is.True);
            Assert.That(result.Events, Has.Count.EqualTo(1));
            Assert.That(result.Events[0], Is.InstanceOf<MourningBrokenEarlyEvent>());
        });

        var repeat = new BreakMourningEarlyCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId);
        Assert.That(
            BreakMourningEarlyCommands.Pipeline.Execute(state, repeat).Error, Is.EqualTo(BreakMourningEarlyCommands.AlreadyBroken));

        var neverMourned = state.HouseholdIds.Issue();
        var noPeriod = new BreakMourningEarlyCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, neverMourned);
        Assert.That(
            BreakMourningEarlyCommands.Pipeline.Execute(state, noPeriod).Error,
            Is.EqualTo(BreakMourningEarlyCommands.NoActiveMourningPeriod));

        _ = deceasedId;
    }

    [Test]
    public void ManesObservanceSystemDeclaresTheRelationshipsActorsPhaseAndReadWriteSet()
    {
        var system = new ManesObservanceSystem();
        Assert.Multiple(() =>
        {
            Assert.That(system.Phase, Is.EqualTo(TickPhase.RelationshipsActors));
            Assert.That(system.Reads, Is.EquivalentTo(new[] { "householdHeadships", "memoriaStates", "chronicleEntries", "ledgerAccounts" }));
        });
    }

    [Test]
    public void ManesObservanceSystemOnlyActsInFebruary()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId));
        var householdId = Establish(state, headId, new GameDate(0));
        Fund(state, householdId, Money.FromDenarii(1000));

        // TotalMonths 0 is January of the epoch year; TotalMonths 1 is February.
        var januaryEvents = new ManesObservanceSystem().Tick(state, new MonthlyTickContext(new GameDate(0), new RandomStreamSet()));
        Assert.That(januaryEvents, Is.Empty);

        var februaryEvents = new ManesObservanceSystem().Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));
        Assert.That(februaryEvents.OfType<ParentaliaObservedEvent>().Count(), Is.EqualTo(1));
    }

    [Test]
    public void AWellFundedParentaliaRaisesMemoriaAndASkippedOneLowersIt()
    {
        var funded = new WorldState(new GameDate(0));
        var fundedHead = funded.CharacterIds.Issue();
        funded.Characters.Add(fundedHead, CharacterTestFixtures.Minimal(fundedHead));
        var fundedHouseholdId = Establish(funded, fundedHead, new GameDate(0));
        Fund(funded, fundedHouseholdId, Money.FromDenarii(1000));

        new ManesObservanceSystem().Tick(funded, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));
        Assert.That(MemoriaResolver.Current(funded, fundedHouseholdId), Is.EqualTo(FuneraryCatalog.ParentaliaBaseMemoriaGain));
        funded.MemoriaStates.TryGet(fundedHouseholdId, out var fundedMemoria);
        Assert.That(fundedMemoria!.LastParentaliaObservedDate, Is.EqualTo(new GameDate(1)));

        var poor = new WorldState(new GameDate(0));
        var poorHead = poor.CharacterIds.Issue();
        poor.Characters.Add(poorHead, CharacterTestFixtures.Minimal(poorHead));
        var poorHouseholdId = Establish(poor, poorHead, new GameDate(0));
        // No Treasury funding at all — the household cannot afford even the modest offering.

        var skippedEvents = new ManesObservanceSystem().Tick(poor, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));

        Assert.Multiple(() =>
        {
            Assert.That(skippedEvents.OfType<ParentaliaSkippedEvent>().Count(), Is.EqualTo(1));
            Assert.That(MemoriaResolver.Current(poor, poorHouseholdId), Is.EqualTo(-FuneraryCatalog.ParentaliaSkippedMemoriaLoss));
        });
    }

    [Test]
    public void AHeldFuneralAndABrokenMourningPeriodBothProduceChronicleEntries()
    {
        var (state, householdId, deceasedId) = DeadHouseholdMember(new GameDate(1));
        Fund(state, householdId, Money.FromDenarii(1000));

        var openingEvents = new FuneralOpeningSystem().Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));
        ChronicleGenerationSystem.Generate(state, openingEvents);

        var funeralId = state.FuneralRecords.InAscendingOrder().First().Key;
        var chooseCommand = new ChooseFuneralTierCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, funeralId, FuneralTier.Grand);
        var heldEvents = ChooseFuneralTierCommands.Pipeline.Execute(state, chooseCommand).Events;
        ChronicleGenerationSystem.Generate(state, heldEvents);

        var breakCommand = new BreakMourningEarlyCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId);
        var brokenEvents = BreakMourningEarlyCommands.Pipeline.Execute(state, breakCommand).Events;
        ChronicleGenerationSystem.Generate(state, brokenEvents);

        var entries = state.ChronicleEntries.InAscendingOrder().Select(e => e.Value).ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(entries.Any(e => e.Category == ChronicleCategory.BirthsAndDeaths && e.Tier == ChronicleTier.Major
                && e.LinkedCharacterIds.Contains(deceasedId)), Is.True);
            Assert.That(entries.Any(e => e.Category == ChronicleCategory.FaithAndScandal && e.Tier == ChronicleTier.Notable), Is.True);
        });
    }

    [Test]
    public void FuneraryStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, householdId, deceasedId) = DeadHouseholdMember(new GameDate(1));
        Fund(state, householdId, Money.FromDenarii(1000));
        new FuneralOpeningSystem().Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));
        var funeralId = state.FuneralRecords.InAscendingOrder().First().Key;

        var chooseCommand = new ChooseFuneralTierCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, funeralId, FuneralTier.Proper);
        ChooseFuneralTierCommands.Pipeline.Execute(state, chooseCommand);

        var breakCommand = new BreakMourningEarlyCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId);
        BreakMourningEarlyCommands.Pipeline.Execute(state, breakCommand);

        new ManesObservanceSystem().Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.FuneralRecords.TryGet(funeralId, out var funeral), Is.True);
            Assert.That(funeral!.Status, Is.EqualTo(FuneralStatus.Held));
            Assert.That(funeral.Tier, Is.EqualTo(FuneralTier.Proper));
            Assert.That(funeral.DeceasedCharacterId, Is.EqualTo(deceasedId));

            Assert.That(restored.MourningPeriods.TryGet(householdId, out var mourning), Is.True);
            Assert.That(mourning!.BrokenEarly, Is.True);

            Assert.That(restored.MemoriaStates.TryGet(householdId, out var memoria), Is.True);
            Assert.That(memoria!.Memoria, Is.EqualTo(MemoriaResolver.Current(state, householdId)));

            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        });
    }
}
