using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
using Gens.Simulation.Schemes;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;
using CharacterTestFixtures = Gens.Simulation.Tests.Characters.CharacterTestFixtures;

namespace Gens.Simulation.Tests.Schemes;

/// <summary>Phase 10 item 12 coverage for the Scheme engine's stages 2-5.</summary>
public sealed class SchemeProgressSystemTests
{
    private static readonly DefinitionId<Trait> BoldTrait = new("bold-test");

    private static TraitCatalog BuildTraitCatalog() => new(new[]
    {
        new TraitDefinition(BoldTrait, TraitCategory.Congenital, PersonalityAxis.Boldness, 25),
    });

    private static RandomStreamSet Streams(ulong seed)
    {
        var streams = new RandomStreamSet();
        streams.Add("schemes.progress", seed, 1);
        return streams;
    }

    private static SchemeProgressSystem MakeSystem() => new(BuildTraitCatalog(), "schemes.progress");

    private static RuntimeId<Character> AddCharacter(WorldState state, int intrigue, IReadOnlyList<DefinitionId<Trait>>? traits = null)
    {
        var id = state.CharacterIds.Issue();
        var character = CharacterTestFixtures.Minimal(id, traits: traits);
        // CharacterTestFixtures.Minimal hardcodes attributes, so rebuild with a controllable Intrigue.
        var withIntrigue = Character.Create(
            id: character.Id, praenomen: character.Praenomen, nomen: character.Nomen, cognomen: character.Cognomen,
            sex: character.Sex, birthDate: character.BirthDate, visualProfile: character.VisualProfile,
            status: character.LegalStatus, socialClass: character.SocialClass, culture: character.Culture,
            location: character.Location, household: character.Household,
            attributes: new CoreAttributes(10, 10, 10, intrigue, 10), skills: character.Skills, condition: character.Condition,
            source: character.Source, instantiatedAtMonth: character.InstantiatedAtMonth, traits: traits);
        state.Characters.Add(id, withIntrigue);
        return id;
    }

    [Test]
    public void DeclaresTheRelationshipsActorsPhase()
    {
        Assert.That(MakeSystem().Phase, Is.EqualTo(TickPhase.RelationshipsActors));
    }

    [Test]
    public void AStrongUndetectedInitiatorEventuallyReachesAFinalOutcomeAtFullProgress()
    {
        var state = new WorldState(new GameDate(0));
        var initiatorId = AddCharacter(state, intrigue: 100, traits: new[] { BoldTrait });
        var targetId = AddCharacter(state, intrigue: 0);
        var schemeId = state.SchemeIds.Issue();
        state.Schemes.Add(schemeId, new SchemeInstance(schemeId, SchemeType.FabricateHook, initiatorId, targetId, null, new GameDate(0), 0, 0, SchemeStage.Progressing));

        var system = MakeSystem();
        var streams = Streams(1);
        for (var month = 1; month <= 5; month++)
            system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));

        state.Schemes.TryGet(schemeId, out var resolved);
        Assert.Multiple(() =>
        {
            Assert.That(resolved!.Stage, Is.EqualTo(SchemeStage.Resolved));
            Assert.That(resolved.Outcome, Is.EqualTo(SchemeOutcome.Succeeded).Or.EqualTo(SchemeOutcome.FailedQuietly));
            Assert.That(resolved.DiscoveryRisk, Is.LessThan(SchemeProgressCatalog.DiscoveryThreshold));
        });
    }

    [Test]
    public void AWeakInitiatorAgainstAPerceptiveTargetGetsDiscoveredAndFoiledWhenTheCounterPlayWindowElapses()
    {
        var state = new WorldState(new GameDate(0));
        var initiatorId = AddCharacter(state, intrigue: 0);
        var targetId = AddCharacter(state, intrigue: 100);
        var schemeId = state.SchemeIds.Issue();
        state.Schemes.Add(schemeId, new SchemeInstance(schemeId, SchemeType.Sabotage, initiatorId, targetId, null, new GameDate(0), 0, 0, SchemeStage.Progressing));

        var system = MakeSystem();
        var streams = Streams(2);
        SchemeInstance? current = null;
        for (var month = 1; month <= 9; month++)
        {
            system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));
            state.Schemes.TryGet(schemeId, out current);
            if (current!.Stage == SchemeStage.Resolved)
                break;
        }

        Assert.Multiple(() =>
        {
            Assert.That(current!.Stage, Is.EqualTo(SchemeStage.Resolved));
            Assert.That(current.Outcome, Is.EqualTo(SchemeOutcome.DiscoveredAndFoiled));
            Assert.That(current.Progress, Is.LessThan(100));
        });
    }

    [Test]
    public void ATickOnceResolvedIsANoOp()
    {
        var state = new WorldState(new GameDate(0));
        var initiatorId = AddCharacter(state, intrigue: 10);
        var targetId = AddCharacter(state, intrigue: 10);
        var schemeId = state.SchemeIds.Issue();
        var resolved = new SchemeInstance(
            schemeId, SchemeType.Blackmail, initiatorId, targetId, null, new GameDate(0), 100, 10, SchemeStage.Resolved,
            Outcome: SchemeOutcome.Succeeded, ResolvedDate: new GameDate(3));
        state.Schemes.Add(schemeId, resolved);

        var events = MakeSystem().Tick(state, new MonthlyTickContext(new GameDate(4), Streams(3)));

        Assert.That(events, Is.Empty);
        state.Schemes.TryGet(schemeId, out var stillResolved);
        Assert.That(stillResolved, Is.EqualTo(resolved));
    }
}
