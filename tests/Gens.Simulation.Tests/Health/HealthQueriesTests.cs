using Gens.Simulation.Characters;
using Gens.Simulation.Health;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using Gens.Simulation.Tests.Characters;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Health;

public sealed class HealthQueriesTests
{
    private static readonly DefinitionId<HealthConditionDefinition> TestFever = new("test-fever");
    private static readonly DefinitionId<HealthConditionDefinition> TestGout = new("test-gout");

    [Test]
    public void IsImmuneIsTrueOnlyForARecoveredCaseWithGrantedImmunity()
    {
        var (state, characterId) = OneCharacter();
        Add(state, characterId, TestFever, CharacterHealthConditionStatus.Recovered, grantedImmunity: true);

        Assert.That(HealthQueries.IsImmune(state, characterId, TestFever), Is.True);
        Assert.That(HealthQueries.IsImmune(state, characterId, TestGout), Is.False);
    }

    [Test]
    public void IsImmuneIsFalseForARecoveredCaseWithoutGrantedImmunity()
    {
        var (state, characterId) = OneCharacter();
        Add(state, characterId, TestFever, CharacterHealthConditionStatus.Recovered, grantedImmunity: false);

        Assert.That(HealthQueries.IsImmune(state, characterId, TestFever), Is.False);
    }

    [Test]
    public void IsImmuneIsFalseWhileStillActive()
    {
        var (state, characterId) = OneCharacter();
        Add(state, characterId, TestFever, CharacterHealthConditionStatus.Active, grantedImmunity: false);

        Assert.That(HealthQueries.IsImmune(state, characterId, TestFever), Is.False);
    }

    [Test]
    public void HasActiveConditionIsTrueOnlyWhileActive()
    {
        var (state, characterId) = OneCharacter();
        Add(state, characterId, TestFever, CharacterHealthConditionStatus.Active, grantedImmunity: false);

        Assert.That(HealthQueries.HasActiveCondition(state, characterId, TestFever), Is.True);
        Assert.That(HealthQueries.HasActiveCondition(state, characterId, TestGout), Is.False);
    }

    [Test]
    public void ActiveConditionsForExcludesResolvedCases()
    {
        var (state, characterId) = OneCharacter();
        Add(state, characterId, TestFever, CharacterHealthConditionStatus.Active, grantedImmunity: false);
        Add(state, characterId, TestGout, CharacterHealthConditionStatus.Recovered, grantedImmunity: false);

        var active = HealthQueries.ActiveConditionsFor(state, characterId).ToArray();

        Assert.That(active, Has.Length.EqualTo(1));
        Assert.That(active[0].ConditionId, Is.EqualTo(TestFever));
    }

    private static (WorldState State, RuntimeId<Character> CharacterId) OneCharacter()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));
        return (state, characterId);
    }

    private static void Add(
        WorldState state, RuntimeId<Character> characterId, DefinitionId<HealthConditionDefinition> conditionId,
        CharacterHealthConditionStatus status, bool grantedImmunity)
    {
        var id = state.CharacterHealthConditionIds.Issue();
        state.CharacterHealthConditions.Add(id, new CharacterHealthCondition
        {
            Id = id,
            CharacterId = characterId,
            ConditionId = conditionId,
            Category = HealthConditionCategory.Acute,
            HasCure = false,
            Severity = 50,
            OnsetDate = new GameDate(1),
            Status = status,
            GrantedImmunity = grantedImmunity,
            ResolvedDate = status == CharacterHealthConditionStatus.Active ? null : new GameDate(5),
        });
    }
}
