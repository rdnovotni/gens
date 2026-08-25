using Gens.Simulation.Actions;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Schemes;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;
using CharacterTestFixtures = Gens.Simulation.Tests.Characters.CharacterTestFixtures;

namespace Gens.Simulation.Tests.Schemes;

/// <summary>Phase 10 item 13 coverage for <see cref="SchemeActionDefinitions"/>.</summary>
public sealed class SchemeActionDefinitionsTests
{
    private static (WorldState State, RuntimeId<Character> Initiator, RuntimeId<Character> Target) TwoCharacters()
    {
        var state = new WorldState(new GameDate(0));
        var initiatorId = state.CharacterIds.Issue();
        state.Characters.Add(initiatorId, CharacterTestFixtures.Minimal(initiatorId, praenomen: "Marcus"));
        var targetId = state.CharacterIds.Issue();
        state.Characters.Add(targetId, CharacterTestFixtures.Minimal(targetId, praenomen: "Gaius"));
        return (state, initiatorId, targetId);
    }

    [Test]
    public void BuildCatalogRegistersAllFiveSchemeTypes()
    {
        var catalog = SchemeActionDefinitions.BuildCatalog();

        Assert.Multiple(() =>
        {
            Assert.That(catalog.TryGet(SchemeActionDefinitions.FabricateHook, out _), Is.True);
            Assert.That(catalog.TryGet(SchemeActionDefinitions.Sabotage, out _), Is.True);
            Assert.That(catalog.TryGet(SchemeActionDefinitions.Blackmail, out _), Is.True);
            Assert.That(catalog.TryGet(SchemeActionDefinitions.Frame, out _), Is.True);
            Assert.That(catalog.TryGet(SchemeActionDefinitions.Assassinate, out _), Is.True);
        });
    }

    [Test]
    public void ToSchemeTypeMapsEveryRegisteredId()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SchemeActionDefinitions.ToSchemeType(SchemeActionDefinitions.FabricateHook), Is.EqualTo(SchemeType.FabricateHook));
            Assert.That(SchemeActionDefinitions.ToSchemeType(SchemeActionDefinitions.Assassinate), Is.EqualTo(SchemeType.Assassinate));
        });
    }

    [Test]
    public void ToSchemeTypeThrowsForAnUnknownId()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SchemeActionDefinitions.ToSchemeType(new DefinitionId<ActionDefinition>("not-registered")));
    }

    [Test]
    public void EligibilityRejectsAnAlreadyActiveSchemeOfTheSameType()
    {
        var (state, initiatorId, targetId) = TwoCharacters();
        var schemeId = state.SchemeIds.Issue();
        state.Schemes.Add(schemeId, new SchemeInstance(schemeId, SchemeType.Sabotage, initiatorId, targetId, null, new GameDate(0), 10, 10, SchemeStage.Progressing));

        var catalog = SchemeActionDefinitions.BuildCatalog();
        var definition = catalog.Get(SchemeActionDefinitions.Sabotage);
        var invocation = new ActionInvocation(initiatorId.ToTaggedString(), targetId.ToTaggedString(), new GameDate(1));

        Assert.That(definition.Eligibility(state, invocation), Is.Not.Null);
    }

    [Test]
    public void EligibilityAllowsADifferentSchemeTypeAgainstTheSameTarget()
    {
        var (state, initiatorId, targetId) = TwoCharacters();
        var schemeId = state.SchemeIds.Issue();
        state.Schemes.Add(schemeId, new SchemeInstance(schemeId, SchemeType.Sabotage, initiatorId, targetId, null, new GameDate(0), 10, 10, SchemeStage.Progressing));

        var catalog = SchemeActionDefinitions.BuildCatalog();
        var definition = catalog.Get(SchemeActionDefinitions.Blackmail);
        var invocation = new ActionInvocation(initiatorId.ToTaggedString(), targetId.ToTaggedString(), new GameDate(1));

        Assert.That(definition.Eligibility(state, invocation), Is.Null);
    }
}
