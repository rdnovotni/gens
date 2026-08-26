using Gens.Simulation.Actions;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Interactions;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Interactions;

/// <summary>Phase 10 item 6 coverage for <see cref="InteractionActionDefinitions"/> — the worked
/// Quick/Multi-stage pair wired into the same reusable action-definition layer the player and any NPC
/// decision loop both select over.</summary>
public sealed class InteractionActionDefinitionsTests
{
    private static (WorldState State, RuntimeId<Character> Initiator, RuntimeId<Character> Target) TwoLivingCharacters()
    {
        var state = new WorldState(new GameDate(0));
        var initiatorId = state.CharacterIds.Issue();
        state.Characters.Add(initiatorId, CharacterTestFixtures.Minimal(initiatorId));
        var targetId = state.CharacterIds.Issue();
        state.Characters.Add(targetId, CharacterTestFixtures.Minimal(targetId));
        return (state, initiatorId, targetId);
    }

    [Test]
    public void BuildCatalogRegistersBothEntries()
    {
        var catalog = InteractionActionDefinitions.BuildCatalog();

        Assert.Multiple(() =>
        {
            Assert.That(catalog.TryGet(InteractionActionDefinitions.Befriend, out var befriend), Is.True);
            Assert.That(befriend!.TargetKind, Is.EqualTo(ActionTargetKind.Character));
            Assert.That(befriend.Confirmation, Is.EqualTo(ActionConfirmationSeverity.Ordinary));
            Assert.That(catalog.TryGet(InteractionActionDefinitions.InitiateScheme, out var scheme), Is.True);
            Assert.That(scheme!.TargetKind, Is.EqualTo(ActionTargetKind.Character));
            Assert.That(scheme.Confirmation, Is.EqualTo(ActionConfirmationSeverity.WaxSeal));
        });
    }

    [Test]
    public void BothEntriesAreEligibleForAFreshLivingPair()
    {
        var (state, initiatorId, targetId) = TwoLivingCharacters();
        var catalog = InteractionActionDefinitions.BuildCatalog();
        var invocation = new ActionInvocation(initiatorId.ToTaggedString(), targetId.ToTaggedString(), new GameDate(0));

        Assert.Multiple(() =>
        {
            Assert.That(catalog.Get(InteractionActionDefinitions.Befriend).Eligibility(state, invocation), Is.Null);
            Assert.That(catalog.Get(InteractionActionDefinitions.InitiateScheme).Eligibility(state, invocation), Is.Null);
        });
    }

    [Test]
    public void RankPrefersBefriendOverInitiateSchemeByDefault()
    {
        var (state, initiatorId, targetId) = TwoLivingCharacters();
        var catalog = InteractionActionDefinitions.BuildCatalog();
        var invocation = new ActionInvocation(initiatorId.ToTaggedString(), targetId.ToTaggedString(), new GameDate(0));

        var best = ActionSelector.SelectBest(state, catalog, invocation);

        Assert.That(best!.Value.Definition.Id, Is.EqualTo(InteractionActionDefinitions.Befriend));
    }

    [Test]
    public void InitiateSchemeIsIneligibleWhenTheSameSchemeIsAlreadyInProgress()
    {
        var (state, initiatorId, targetId) = TwoLivingCharacters();
        var command = InteractionActionDefinitions.ToInitiateSchemeCommand(
            state.CommandIds.Issue(), new ActionInvocation(initiatorId.ToTaggedString(), targetId.ToTaggedString(), new GameDate(0)));
        InitiateSchemeCommands.Pipeline.Execute(state, command);

        var catalog = InteractionActionDefinitions.BuildCatalog();
        var invocation = new ActionInvocation(initiatorId.ToTaggedString(), targetId.ToTaggedString(), new GameDate(0));

        Assert.That(
            catalog.Get(InteractionActionDefinitions.InitiateScheme).Eligibility(state, invocation),
            Is.EqualTo(InitiateSchemeCommands.AlreadyInProgress));
    }

    [Test]
    public void ToRecordInteractionCommandBuildsAFriendBond()
    {
        var (state, initiatorId, targetId) = TwoLivingCharacters();
        var invocation = new ActionInvocation(initiatorId.ToTaggedString(), targetId.ToTaggedString(), new GameDate(0));

        var command = InteractionActionDefinitions.ToRecordInteractionCommand(state.CommandIds.Issue(), invocation);

        Assert.Multiple(() =>
        {
            Assert.That(command.CharacterId, Is.EqualTo(initiatorId));
            Assert.That(command.TargetId, Is.EqualTo(targetId));
            Assert.That(command.BondsGranted, Is.EqualTo(BondTag.Friend));
            Assert.That(command.OpinionDelta, Is.EqualTo(InteractionActionDefinitions.BefriendOpinionDelta));
        });
    }
}
