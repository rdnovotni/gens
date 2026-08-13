using Gens.Simulation.Characters;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.State;

public sealed class StateHasherTests
{
    [Test]
    public void IdenticalOperationSequencesProduceIdenticalHashes()
    {
        var stateA = BuildState();
        var stateB = BuildState();

        Assert.That(StateHasher.Hash(stateA), Is.EqualTo(StateHasher.Hash(stateB)));
    }

    [Test]
    public void DifferentCounterValueChangesTheHash()
    {
        var baseline = BuildState();
        var baselineHash = StateHasher.Hash(baseline);

        var different = BuildState();
        different.PlotIds.Issue();

        Assert.That(StateHasher.Hash(different), Is.Not.EqualTo(baselineHash));
    }

    [Test]
    public void DifferentDateChangesTheHash()
    {
        var atMonthZero = new WorldState(new GameDate(0));
        var atMonthOne = new WorldState(new GameDate(1));

        Assert.That(StateHasher.Hash(atMonthZero), Is.Not.EqualTo(StateHasher.Hash(atMonthOne)));
    }

    [Test]
    public void DifferentCharacterFieldChangesTheHash()
    {
        var baseline = new WorldState(new GameDate(10));
        var id = baseline.CharacterIds.Issue();
        baseline.Characters.Add(id, CharacterTestFixtures.Minimal(id));
        var baselineHash = StateHasher.Hash(baseline);

        var changed = new WorldState(new GameDate(10));
        var changedId = changed.CharacterIds.Issue();
        var changedCharacter = CharacterTestFixtures.Minimal(changedId) with
        {
            Condition = new Condition(50, 0, 50, 20, 50),
        };
        changed.Characters.Add(changedId, changedCharacter);

        Assert.That(StateHasher.Hash(changed), Is.Not.EqualTo(baselineHash));
    }

    [Test]
    public void DifferentParentageChangesTheHash()
    {
        var baseline = new WorldState(new GameDate(10));
        var id = baseline.CharacterIds.Issue();
        baseline.Characters.Add(id, CharacterTestFixtures.Minimal(id));
        var baselineHash = StateHasher.Hash(baseline);

        var changed = new WorldState(new GameDate(10));
        var motherId = changed.CharacterIds.Issue();
        var changedId = changed.CharacterIds.Issue();
        changed.Characters.Add(changedId, CharacterTestFixtures.Minimal(changedId, motherId: motherId));

        Assert.That(StateHasher.Hash(changed), Is.Not.EqualTo(baselineHash));
    }

    [Test]
    public void DifferentMaritalHistoryChangesTheHash()
    {
        var baseline = new WorldState(new GameDate(10));
        var id = baseline.CharacterIds.Issue();
        baseline.Characters.Add(id, CharacterTestFixtures.Minimal(id));
        var baselineHash = StateHasher.Hash(baseline);

        var changed = new WorldState(new GameDate(10));
        var spouseId = changed.CharacterIds.Issue();
        var changedId = changed.CharacterIds.Issue();
        changed.Characters.Add(changedId, CharacterTestFixtures.Minimal(changedId,
            maritalHistory: new[] { new MarriageRecord(spouseId, new GameDate(0), null, null) }));

        Assert.That(StateHasher.Hash(changed), Is.Not.EqualTo(baselineHash));
    }

    [Test]
    public void DifferentPermanentInjuriesChangeTheHash()
    {
        var baseline = new WorldState(new GameDate(10));
        var id = baseline.CharacterIds.Issue();
        baseline.Characters.Add(id, CharacterTestFixtures.Minimal(id));
        var baselineHash = StateHasher.Hash(baseline);

        var changed = new WorldState(new GameDate(10));
        var changedId = changed.CharacterIds.Issue();
        changed.Characters.Add(changedId, CharacterTestFixtures.Minimal(changedId,
            permanentInjuries: new[] { new PermanentInjury(PermanentInjuryTarget.Martial, 10, "wound", new GameDate(0)) }));

        Assert.That(StateHasher.Hash(changed), Is.Not.EqualTo(baselineHash));
    }

    [Test]
    public void ADeathRecordChangesTheHash()
    {
        var baseline = new WorldState(new GameDate(10));
        var id = baseline.CharacterIds.Issue();
        baseline.Characters.Add(id, CharacterTestFixtures.Minimal(id));
        var baselineHash = StateHasher.Hash(baseline);

        var changed = new WorldState(new GameDate(10));
        var changedId = changed.CharacterIds.Issue();
        changed.Characters.Add(changedId, CharacterTestFixtures.Minimal(changedId,
            deathRecord: new DeathRecord(new GameDate(5), DeathCause.Disease, 20)));

        Assert.That(StateHasher.Hash(changed), Is.Not.EqualTo(baselineHash));
    }

    [Test]
    public void ADifferentRelationshipChangesTheHash()
    {
        var baseline = BuildState();
        var baselineHash = StateHasher.Hash(baseline);

        var withRelationship = BuildState();
        var characterIds = withRelationship.Characters.InAscendingOrder().Select(entry => entry.Key).ToArray();
        withRelationship.Relationships.Add(
            new RelationshipKey(characterIds[0], characterIds[1]),
            new Relationship(10, BondTag.Friend, RelationshipOrigin.Encounter, withRelationship.Date, withRelationship.Date, null));

        Assert.That(StateHasher.Hash(withRelationship), Is.Not.EqualTo(baselineHash));
    }

    [Test]
    public void DirectionMattersForTheRelationshipHash()
    {
        var forward = BuildState();
        var forwardIds = forward.Characters.InAscendingOrder().Select(entry => entry.Key).ToArray();
        forward.Relationships.Add(
            new RelationshipKey(forwardIds[0], forwardIds[1]),
            new Relationship(10, BondTag.Friend, RelationshipOrigin.Encounter, forward.Date, forward.Date, null));

        var reverse = BuildState();
        var reverseIds = reverse.Characters.InAscendingOrder().Select(entry => entry.Key).ToArray();
        reverse.Relationships.Add(
            new RelationshipKey(reverseIds[1], reverseIds[0]),
            new Relationship(10, BondTag.Friend, RelationshipOrigin.Encounter, reverse.Date, reverse.Date, null));

        Assert.That(StateHasher.Hash(forward), Is.Not.EqualTo(StateHasher.Hash(reverse)));
    }

    [Test]
    public void KnowledgeEntryChangesTheHash()
    {
        var baseline = BuildState();
        var baselineHash = StateHasher.Hash(baseline);

        var withKnowledge = BuildState();
        withKnowledge.Knowledge.Set(
            new KnowledgeKey("player", "char_0000001", "location"),
            new KnowledgeEntry("Rome", KnowledgeConfidence.Certain, withKnowledge.Date, null));

        Assert.That(StateHasher.Hash(withKnowledge), Is.Not.EqualTo(baselineHash));
    }

    [Test]
    public void DifferentKnowledgeValueWithSameKeyChangesTheHash()
    {
        var stateRome = BuildState();
        stateRome.Knowledge.Set(
            new KnowledgeKey("player", "char_0000001", "location"),
            new KnowledgeEntry("Rome", KnowledgeConfidence.Certain, stateRome.Date, null));

        var stateCarthage = BuildState();
        stateCarthage.Knowledge.Set(
            new KnowledgeKey("player", "char_0000001", "location"),
            new KnowledgeEntry("Carthage", KnowledgeConfidence.Certain, stateCarthage.Date, null));

        Assert.That(StateHasher.Hash(stateRome), Is.Not.EqualTo(StateHasher.Hash(stateCarthage)));
    }

    private static WorldState BuildState()
    {
        var state = new WorldState(new GameDate(10));
        var first = state.CharacterIds.Issue();
        var second = state.CharacterIds.Issue();
        state.Characters.Add(first, CharacterTestFixtures.Minimal(first, praenomen: "First"));
        state.Characters.Add(second, CharacterTestFixtures.Minimal(second, praenomen: "Second"));
        return state;
    }
}
