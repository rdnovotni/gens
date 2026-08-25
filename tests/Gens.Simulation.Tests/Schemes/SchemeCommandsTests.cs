using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Schemes;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;
using CharacterTestFixtures = Gens.Simulation.Tests.Characters.CharacterTestFixtures;

namespace Gens.Simulation.Tests.Schemes;

/// <summary>Phase 10 item 12 command coverage (Scheme engine stages 1 and 4).</summary>
public sealed class SchemeCommandsTests
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
    public void InitiateCreatesAProgressingSchemeAndEmitsAPrivateEvent()
    {
        var (state, initiatorId, targetId) = TwoCharacters();

        var result = SchemeCommands.InitiatePipeline.Execute(
            state,
            new InitiateSchemeCommand(
                state.CommandIds.Issue(), initiatorId.ToTaggedString(), state.Date, null, SchemeType.FabricateHook,
                initiatorId, targetId, null));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(result.Events, Has.Count.EqualTo(1));
            var evt = (SchemeInitiatedEvent)result.Events[0];
            Assert.That(evt.Visibility, Is.Not.EqualTo(Visibility.Public));

            var schemeId = evt.SchemeId;
            Assert.That(state.Schemes.TryGet(schemeId, out var scheme), Is.True);
            Assert.That(scheme!.Stage, Is.EqualTo(SchemeStage.Progressing));
            Assert.That(scheme.Progress, Is.EqualTo(0));
        });
    }

    [Test]
    public void InitiateRejectsTheSameCharacterAsInitiatorAndTarget()
    {
        var (state, initiatorId, _) = TwoCharacters();

        var result = SchemeCommands.InitiatePipeline.Execute(
            state,
            new InitiateSchemeCommand(
                state.CommandIds.Issue(), initiatorId.ToTaggedString(), state.Date, null, SchemeType.Sabotage,
                initiatorId, initiatorId, null));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Error, Is.EqualTo(SchemeCommands.SameCharacter));
        });
    }

    [Test]
    public void CounterPlayResolvesAnAwaitingSchemeAsDiscoveredAndFoiledWithAPublicEvent()
    {
        var (state, initiatorId, targetId) = TwoCharacters();
        var schemeId = state.SchemeIds.Issue();
        state.Schemes.Add(
            schemeId,
            new SchemeInstance(schemeId, SchemeType.Blackmail, initiatorId, targetId, null, new GameDate(0), 50, 80, SchemeStage.AwaitingCounterPlay, new GameDate(5)));

        var result = SchemeCommands.CounterPlayPipeline.Execute(
            state, new CounterPlaySchemeCommand(state.CommandIds.Issue(), targetId.ToTaggedString(), new GameDate(2), null, schemeId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            var evt = (SchemeResolvedEvent)result.Events[0];
            Assert.That(evt.Outcome, Is.EqualTo(SchemeOutcome.DiscoveredAndFoiled));
            Assert.That(evt.Visibility, Is.EqualTo(Visibility.Public));
            Assert.That(state.Schemes.TryGet(schemeId, out var scheme), Is.True);
            Assert.That(scheme!.Stage, Is.EqualTo(SchemeStage.Resolved));
        });
    }

    [Test]
    public void CounterPlayRejectsASchemeThatIsNotAwaitingCounterPlay()
    {
        var (state, initiatorId, targetId) = TwoCharacters();
        var schemeId = state.SchemeIds.Issue();
        state.Schemes.Add(schemeId, new SchemeInstance(schemeId, SchemeType.Frame, initiatorId, targetId, null, new GameDate(0), 10, 10, SchemeStage.Progressing));

        var result = SchemeCommands.CounterPlayPipeline.Execute(
            state, new CounterPlaySchemeCommand(state.CommandIds.Issue(), targetId.ToTaggedString(), new GameDate(2), null, schemeId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Error, Is.EqualTo(SchemeCommands.NotAwaitingCounterPlay));
        });
    }

    [Test]
    public void CounterPlayRejectsAnUnknownScheme()
    {
        var (state, _, targetId) = TwoCharacters();

        var result = SchemeCommands.CounterPlayPipeline.Execute(
            state,
            new CounterPlaySchemeCommand(
                state.CommandIds.Issue(), targetId.ToTaggedString(), new GameDate(2), null, RuntimeId<SchemeInstance>.Parse("scheme_0000099")));

        Assert.That(result.Error, Is.EqualTo(SchemeCommands.SchemeNotFound));
    }
}
