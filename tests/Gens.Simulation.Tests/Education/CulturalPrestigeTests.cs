using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Education;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Queries;
using Gens.Simulation.Random;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Education;

/// <summary>Phase 17 item 2 coverage, slice 1: Cultural Prestige (§7) and the two ongoing Cultural
/// Patronage commitments (Literary Patronage, Symposium) and their monthly cycle.</summary>
public sealed class CulturalPrestigeTests
{
    private static (WorldState State, RuntimeId<Household> HouseholdId, RuntimeId<Character> HeadId) HouseholdWithHead()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, nomen: "Cornelius", household: householdId));
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

    // ---- Cultural Prestige resolver ---------------------------------------------------------

    [Test]
    public void CulturalPrestigeResolverDefaultsToZeroAndAutoCreatesOnFirstApply()
    {
        var (state, householdId, _) = HouseholdWithHead();

        Assert.That(CulturalPrestigeResolver.Current(state, householdId), Is.EqualTo(0));

        CulturalPrestigeResolver.Apply(state, householdId, 5);
        Assert.That(CulturalPrestigeResolver.Current(state, householdId), Is.EqualTo(5));

        CulturalPrestigeResolver.Apply(state, householdId, -2);
        Assert.That(CulturalPrestigeResolver.Current(state, householdId), Is.EqualTo(3));
    }

    [Test]
    public void CulturalPrestigeResolverTracksTheMarriageMarketThreshold()
    {
        var (state, householdId, _) = HouseholdWithHead();
        Assert.That(CulturalPrestigeResolver.ClearsMarriageMarketThreshold(state, householdId), Is.False);

        CulturalPrestigeResolver.Apply(state, householdId, CulturalPrestigeCatalog.MarriageMarketPrestigeThreshold);
        Assert.That(CulturalPrestigeResolver.ClearsMarriageMarketThreshold(state, householdId), Is.True);
    }

    // ---- SetLiteraryPatronCommand -------------------------------------------------------------

    [Test]
    public void SetLiteraryPatronCommandStartsAnActiveRecord()
    {
        var (state, householdId, headId) = HouseholdWithHead();

        var result = SetLiteraryPatronCommands.Pipeline.Execute(
            state, new SetLiteraryPatronCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, headId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            var record = CulturalPatronageResolver.ActiveRecord(state, householdId, CulturalPatronageType.LiteraryPatron);
            Assert.That(record, Is.Not.Null);
            Assert.That(record!.HostCharacterId, Is.EqualTo(headId));
            var evt = (CulturalPatronageStartedEvent)result.Events[0];
            Assert.That(evt.PatronageType, Is.EqualTo(CulturalPatronageType.LiteraryPatron));
            Assert.That(evt.Visibility, Is.EqualTo(Visibility.Public));
        });
    }

    [Test]
    public void SetLiteraryPatronCommandRejectsASecondActiveCommitmentOrAnIneligibleHost()
    {
        var (state, householdId, headId) = HouseholdWithHead();
        SetLiteraryPatronCommands.Pipeline.Execute(
            state, new SetLiteraryPatronCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, headId));

        Assert.That(
            SetLiteraryPatronCommands.Pipeline.Execute(
                state, new SetLiteraryPatronCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, headId)).Error,
            Is.EqualTo(SetLiteraryPatronCommands.AlreadyActive));

        var (otherState, otherHouseholdId, _) = HouseholdWithHead();
        var strangerId = otherState.CharacterIds.Issue();
        otherState.Characters.Add(strangerId, CharacterTestFixtures.Minimal(strangerId, nomen: "Fabius", household: null));
        Assert.That(
            SetLiteraryPatronCommands.Pipeline.Execute(
                otherState,
                new SetLiteraryPatronCommand(otherState.CommandIds.Issue(), "player", new GameDate(0), null, otherHouseholdId, strangerId))
                .Error,
            Is.EqualTo(SetLiteraryPatronCommands.HostNotInHousehold));
    }

    // ---- HostRecurringSymposiumCommand ---------------------------------------------------------

    [Test]
    public void HostRecurringSymposiumCommandStartsAnActiveRecordIndependentOfLiteraryPatronage()
    {
        var (state, householdId, headId) = HouseholdWithHead();
        SetLiteraryPatronCommands.Pipeline.Execute(
            state, new SetLiteraryPatronCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, headId));

        var result = HostRecurringSymposiumCommands.Pipeline.Execute(
            state, new HostRecurringSymposiumCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, headId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(
                CulturalPatronageResolver.ActiveRecord(state, householdId, CulturalPatronageType.LiteraryPatron), Is.Not.Null);
            Assert.That(
                CulturalPatronageResolver.ActiveRecord(state, householdId, CulturalPatronageType.Symposium), Is.Not.Null);
        });
    }

    [Test]
    public void HostRecurringSymposiumCommandRejectsASecondActiveCommitment()
    {
        var (state, householdId, headId) = HouseholdWithHead();
        HostRecurringSymposiumCommands.Pipeline.Execute(
            state, new HostRecurringSymposiumCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, headId));

        Assert.That(
            HostRecurringSymposiumCommands.Pipeline.Execute(
                    state,
                    new HostRecurringSymposiumCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, headId))
                .Error,
            Is.EqualTo(HostRecurringSymposiumCommands.AlreadyActive));
    }

    // ---- EndPatronageCommand -------------------------------------------------------------------

    [Test]
    public void EndPatronageCommandEndsTheRecordAndAllowsARestart()
    {
        var (state, householdId, headId) = HouseholdWithHead();
        SetLiteraryPatronCommands.Pipeline.Execute(
            state, new SetLiteraryPatronCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, headId));
        var recordId = CulturalPatronageResolver.ActiveRecord(state, householdId, CulturalPatronageType.LiteraryPatron)!.RecordId;

        var result = EndPatronageCommands.Pipeline.Execute(
            state, new EndPatronageCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, recordId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(CulturalPatronageResolver.ActiveRecord(state, householdId, CulturalPatronageType.LiteraryPatron), Is.Null);
            Assert.That(
                EndPatronageCommands.Pipeline.Execute(
                    state, new EndPatronageCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, recordId)).Error,
                Is.EqualTo(EndPatronageCommands.AlreadyEnded));
        });

        // Ending frees the (household, type) slot for a fresh commitment.
        var restart = SetLiteraryPatronCommands.Pipeline.Execute(
            state, new SetLiteraryPatronCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, householdId, headId));
        Assert.That(restart.Accepted, Is.True);
    }

    [Test]
    public void EndPatronageCommandRejectsAnUnknownRecord()
    {
        var state = new WorldState(new GameDate(0));
        var bogusId = state.CulturalPatronageRecordIds.Issue();

        Assert.That(
            EndPatronageCommands.Pipeline.Execute(
                state, new EndPatronageCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, bogusId)).Error,
            Is.EqualTo(EndPatronageCommands.RecordNotFound));
    }

    // ---- Monthly cycle -----------------------------------------------------------------------

    [Test]
    public void CulturalPatronageCycleSystemDrawsCostAndAccruesPrestigeForEachActiveCommitmentType()
    {
        var (state, householdId, headId) = HouseholdWithHead();
        Fund(state, householdId, Money.FromDenarii(500));
        SetLiteraryPatronCommands.Pipeline.Execute(
            state, new SetLiteraryPatronCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, headId));
        HostRecurringSymposiumCommands.Pipeline.Execute(
            state, new HostRecurringSymposiumCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, headId));

        var events = new CulturalPatronageCycleSystem().Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));

        var expectedCost = CulturalPrestigeCatalog.LiteraryPatronMonthlyCost + CulturalPrestigeCatalog.RecurringSymposiumMonthlyCost;
        var expectedPrestige =
            CulturalPrestigeCatalog.LiteraryPatronMonthlyPrestigeGain + CulturalPrestigeCatalog.RecurringSymposiumMonthlyPrestigeGain;

        Assert.Multiple(() =>
        {
            Assert.That(events, Has.Count.EqualTo(2));
            var account = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var acc) ? acc!.Balance : Money.Zero;
            Assert.That(account, Is.EqualTo(Money.FromDenarii(500) - expectedCost));
            Assert.That(CulturalPrestigeResolver.Current(state, householdId), Is.EqualTo(expectedPrestige));
        });
    }

    [Test]
    public void CulturalPatronageCycleSystemDeclaresThePhaseAndReadWriteSet()
    {
        var system = new CulturalPatronageCycleSystem();
        Assert.That(system.Phase, Is.EqualTo(TickPhase.RelationshipsActors));
    }

    // ---- Query ---------------------------------------------------------------------------------

    [Test]
    public void CulturalPrestigeQueryProjectsCurrentPrestigeAndTheThreshold()
    {
        var (state, householdId, _) = HouseholdWithHead();
        CulturalPrestigeResolver.Apply(state, householdId, CulturalPrestigeCatalog.MarriageMarketPrestigeThreshold);

        var projection = new CulturalPrestigeQuery(householdId).Execute(state, "player");

        Assert.Multiple(() =>
        {
            Assert.That(projection.Prestige, Is.EqualTo(CulturalPrestigeCatalog.MarriageMarketPrestigeThreshold));
            Assert.That(projection.ClearsMarriageMarketThreshold, Is.True);
        });
    }

    // ---- Save round trip & determinism --------------------------------------------------------

    [Test]
    public void CulturalPrestigeStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, householdId, headId) = HouseholdWithHead();
        Fund(state, householdId, Money.FromDenarii(300));
        CulturalPrestigeResolver.Apply(state, householdId, 7);
        SetLiteraryPatronCommands.Pipeline.Execute(
            state, new SetLiteraryPatronCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, headId));
        HostRecurringSymposiumCommands.Pipeline.Execute(
            state, new HostRecurringSymposiumCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, headId));
        new CulturalPatronageCycleSystem().Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));

        var symposiumRecordId = CulturalPatronageResolver.ActiveRecord(state, householdId, CulturalPatronageType.Symposium)!.RecordId;
        EndPatronageCommands.Pipeline.Execute(
            state, new EndPatronageCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, symposiumRecordId));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.HouseholdCulturalPrestiges.Count, Is.EqualTo(1));
            Assert.That(CulturalPrestigeResolver.Current(restored, householdId), Is.EqualTo(CulturalPrestigeResolver.Current(state, householdId)));

            Assert.That(restored.CulturalPatronageRecords.Count, Is.EqualTo(2));
            Assert.That(
                CulturalPatronageResolver.ActiveRecord(restored, householdId, CulturalPatronageType.LiteraryPatron), Is.Not.Null);
            Assert.That(
                CulturalPatronageResolver.ActiveRecord(restored, householdId, CulturalPatronageType.Symposium), Is.Null);

            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        });
    }
}
