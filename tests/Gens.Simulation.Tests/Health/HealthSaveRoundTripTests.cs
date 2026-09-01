using Gens.Simulation.Characters;
using Gens.Simulation.Health;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using Gens.Simulation.Tests.Characters;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Health;

public sealed class HealthSaveRoundTripTests
{
    private static readonly DefinitionId<HealthConditionDefinition> TestFever = new("test-fever");

    [Test]
    public void CharacterHealthConditionStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var state = new WorldState(new GameDate(20));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));

        var activeCaseId = state.CharacterHealthConditionIds.Issue();
        state.CharacterHealthConditions.Add(activeCaseId, CharacterHealthCondition.Create(
            activeCaseId, characterId, TestFever, HealthConditionCategory.Chronic, hasCure: true, severity: 42, new GameDate(15)));

        var deceasedId = state.CharacterIds.Issue();
        state.Characters.Add(deceasedId, CharacterTestFixtures.Minimal(
            deceasedId, nomen: "Secundus",
            deathRecord: new DeathRecord(new GameDate(18), DeathCause.Disease, 30, TestFever)));
        var resolvedCaseId = state.CharacterHealthConditionIds.Issue();
        var resolvedCase = CharacterHealthCondition.Create(
            resolvedCaseId, deceasedId, TestFever, HealthConditionCategory.Acute, hasCure: false, severity: 90, new GameDate(10));
        resolvedCase = resolvedCase with { Status = CharacterHealthConditionStatus.Fatal, ResolvedDate = new GameDate(18) };
        state.CharacterHealthConditions.Add(resolvedCaseId, resolvedCase);

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.CharacterHealthConditions.Count, Is.EqualTo(state.CharacterHealthConditions.Count));
            Assert.That(restored.CharacterHealthConditionIds.Peek, Is.EqualTo(state.CharacterHealthConditionIds.Peek));

            restored.CharacterHealthConditions.TryGet(activeCaseId, out var restoredActive);
            Assert.That(restoredActive.Status, Is.EqualTo(CharacterHealthConditionStatus.Active));
            Assert.That(restoredActive.Severity, Is.EqualTo(42));

            restored.Characters.TryGet(deceasedId, out var restoredDeceased);
            Assert.That(restoredDeceased.DeathRecord!.Value.ConditionId, Is.EqualTo(TestFever));

            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        });
    }

    [Test]
    public void Item2PartitionsRoundTripThroughTheDtoAndDeterministicHashStaysStable()
    {
        var state = new WorldState(new GameDate(20));
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        state.SettlementSanitationInvestments.Add(
            settlementId, SettlementSanitationInvestment.Create(settlementId, SanitationInvestmentTier.Standard));

        var activeOutbreakId = state.EpidemicOutbreakIds.Issue();
        state.EpidemicOutbreaks.Add(activeOutbreakId, EpidemicOutbreak.Create(
            activeOutbreakId, settlementId, DiseaseCatalog.Pestilence, new GameDate(15)) with { SettlementQuarantineActive = true });

        var endedOutbreakId = state.EpidemicOutbreakIds.Issue();
        var ended = EpidemicOutbreak.Create(endedOutbreakId, settlementId, DiseaseCatalog.EntericFever, new GameDate(10))
            with { Status = EpidemicOutbreakStatus.Ended, ImperialScale = true, ResolvedDate = new GameDate(18) };
        state.EpidemicOutbreaks.Add(endedOutbreakId, ended);

        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));
        var caseId = state.CharacterHealthConditionIds.Issue();
        state.CharacterHealthConditions.Add(caseId, CharacterHealthCondition.Create(
            caseId, characterId, DiseaseCatalog.Pestilence, HealthConditionCategory.Acute, hasCure: false, severity: 40, new GameDate(15))
            with { Quarantined = true });

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(SanitationQueries.EffectiveTier(restored, settlementId), Is.EqualTo(SanitationInvestmentTier.Standard));
            Assert.That(restored.EpidemicOutbreakIds.Peek, Is.EqualTo(state.EpidemicOutbreakIds.Peek));
            Assert.That(restored.EpidemicOutbreaks.Count, Is.EqualTo(2));

            restored.EpidemicOutbreaks.TryGet(activeOutbreakId, out var restoredActive);
            Assert.That(restoredActive.Status, Is.EqualTo(EpidemicOutbreakStatus.Active));
            Assert.That(restoredActive.SettlementQuarantineActive, Is.True);

            restored.EpidemicOutbreaks.TryGet(endedOutbreakId, out var restoredEnded);
            Assert.That(restoredEnded.Status, Is.EqualTo(EpidemicOutbreakStatus.Ended));
            Assert.That(restoredEnded.ImperialScale, Is.True);
            Assert.That(restoredEnded.ResolvedDate, Is.EqualTo(new GameDate(18)));

            restored.CharacterHealthConditions.TryGet(caseId, out var restoredCase);
            Assert.That(restoredCase.Quarantined, Is.True);

            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        });
    }
}
