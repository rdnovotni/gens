using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Interactions;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Interactions;

/// <summary>Phase 10 item 6 coverage for <see cref="InitiateSchemeCommand"/>.</summary>
public sealed class InitiateSchemeCommandTests
{
    private static WorldState StateWithTwoLivingCharacters(out RuntimeId<Character> initiatorId, out RuntimeId<Character> targetId)
    {
        var state = new WorldState(new GameDate(0));
        initiatorId = state.CharacterIds.Issue();
        state.Characters.Add(initiatorId, CharacterTestFixtures.Minimal(initiatorId));
        targetId = state.CharacterIds.Issue();
        state.Characters.Add(targetId, CharacterTestFixtures.Minimal(targetId));
        return state;
    }

    [Test]
    public void AcceptsAFreshSchemeBetweenTwoLivingCharacters()
    {
        var state = StateWithTwoLivingCharacters(out var initiatorId, out var targetId);
        var command = new InitiateSchemeCommand(
            state.CommandIds.Issue(), initiatorId.ToTaggedString(), new GameDate(0), CausationId: null,
            initiatorId, targetId, SchemeType.Coercive);

        var result = InitiateSchemeCommands.Pipeline.Execute(state, command);

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(result.Events, Has.Count.EqualTo(1));
            Assert.That(result.Events[0], Is.InstanceOf<SchemeInitiatedEvent>());
            Assert.That(state.Schemes.Count, Is.EqualTo(1));
        });
    }

    [Test]
    public void RejectsASchemeTargetingItsOwnInitiator()
    {
        var state = StateWithTwoLivingCharacters(out var initiatorId, out _);
        var command = new InitiateSchemeCommand(
            state.CommandIds.Issue(), initiatorId.ToTaggedString(), new GameDate(0), CausationId: null,
            initiatorId, initiatorId, SchemeType.Coercive);

        var result = InitiateSchemeCommands.Pipeline.Execute(state, command);

        Assert.That(result.Error, Is.EqualTo(InitiateSchemeCommands.SelfTargeted));
    }

    [Test]
    public void RejectsWhenTheInitiatorIsDeceased()
    {
        var state = new WorldState(new GameDate(0));
        var initiatorId = state.CharacterIds.Issue();
        state.Characters.Add(initiatorId, CharacterTestFixtures.Minimal(
            initiatorId, deathRecord: new DeathRecord(new GameDate(0), DeathCause.OldAge, 70)));
        var targetId = state.CharacterIds.Issue();
        state.Characters.Add(targetId, CharacterTestFixtures.Minimal(targetId));

        var command = new InitiateSchemeCommand(
            state.CommandIds.Issue(), initiatorId.ToTaggedString(), new GameDate(0), CausationId: null,
            initiatorId, targetId, SchemeType.Coercive);

        var result = InitiateSchemeCommands.Pipeline.Execute(state, command);

        Assert.That(result.Error, Is.EqualTo(InitiateSchemeCommands.InitiatorDeceased));
    }

    [Test]
    public void RejectsADuplicateInProgressSchemeOfTheSameTypeAndPair()
    {
        var state = StateWithTwoLivingCharacters(out var initiatorId, out var targetId);
        var first = new InitiateSchemeCommand(
            state.CommandIds.Issue(), initiatorId.ToTaggedString(), new GameDate(0), CausationId: null,
            initiatorId, targetId, SchemeType.Coercive);
        InitiateSchemeCommands.Pipeline.Execute(state, first);

        var second = new InitiateSchemeCommand(
            state.CommandIds.Issue(), initiatorId.ToTaggedString(), new GameDate(0), CausationId: null,
            initiatorId, targetId, SchemeType.Coercive);
        var result = InitiateSchemeCommands.Pipeline.Execute(state, second);

        Assert.That(result.Error, Is.EqualTo(InitiateSchemeCommands.AlreadyInProgress));
    }
}
