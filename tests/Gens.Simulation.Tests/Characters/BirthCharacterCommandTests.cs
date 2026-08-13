using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class BirthCharacterCommandTests
{
    private const string StreamName = "test-generation";

    [Test]
    public void ValidBirthAddsANewCharacterAndEmitsCharacterBornEvent()
    {
        var state = new WorldState(new GameDate(300));
        var motherId = state.CharacterIds.Issue();
        state.Characters.Add(motherId, CharacterTestFixtures.Minimal(motherId, birthDate: new GameDate(300 - 25 * 12)));
        var streams = new RandomStreamSet();
        streams.Add(StreamName, 7, 1);
        var pipeline = BirthCharacterCommands.CreatePipeline(streams);

        var command = new BirthCharacterCommand(
            state.CommandIds.Issue(), "player", new GameDate(300), null,
            motherId, null, Sex.Female, LegalStatus.RomanCitizen, SocialClass.Plebeian,
            NamePoolTestFixtures.Roman, StreamName);

        var result = pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        var born = (CharacterBornEvent)result.Events.Single();
        Assert.That(born.MotherId, Is.EqualTo(motherId));
        Assert.That(state.Characters.TryGet(born.CharacterId, out var child), Is.True);
        Assert.That(child.MotherId, Is.EqualTo(motherId));
        Assert.That(child.Sex, Is.EqualTo(Sex.Female));
        Assert.That(child.BirthDate, Is.EqualTo(new GameDate(300)));
    }

    [Test]
    public void ChildIsLegitimateWhenFatherIsTheMothersCurrentSpouse()
    {
        var state = new WorldState(new GameDate(300));
        var motherId = state.CharacterIds.Issue();
        var fatherId = state.CharacterIds.Issue();
        state.Characters.Add(fatherId, CharacterTestFixtures.Minimal(fatherId, birthDate: new GameDate(300 - 25 * 12)));
        state.Characters.Add(motherId, CharacterTestFixtures.Minimal(
            motherId, birthDate: new GameDate(300 - 25 * 12),
            maritalHistory: new[] { new MarriageRecord(fatherId, new GameDate(0), null, null) }));
        var streams = new RandomStreamSet();
        streams.Add(StreamName, 7, 1);
        var pipeline = BirthCharacterCommands.CreatePipeline(streams);

        var command = new BirthCharacterCommand(
            state.CommandIds.Issue(), "player", new GameDate(300), null,
            motherId, fatherId, null, LegalStatus.RomanCitizen, SocialClass.Plebeian,
            NamePoolTestFixtures.Roman, StreamName);

        var result = pipeline.Execute(state, command);

        var born = (CharacterBornEvent)result.Events.Single();
        Assert.That(born.Legitimacy, Is.EqualTo(Legitimacy.Legitimate));
    }

    [Test]
    public void ChildIsIllegitimateWhenFatherIsNotTheMothersCurrentSpouse()
    {
        var state = new WorldState(new GameDate(300));
        var motherId = state.CharacterIds.Issue();
        var otherManId = state.CharacterIds.Issue();
        var fatherId = state.CharacterIds.Issue();
        state.Characters.Add(fatherId, CharacterTestFixtures.Minimal(fatherId, birthDate: new GameDate(300 - 25 * 12)));
        state.Characters.Add(motherId, CharacterTestFixtures.Minimal(
            motherId, birthDate: new GameDate(300 - 25 * 12),
            maritalHistory: new[] { new MarriageRecord(otherManId, new GameDate(0), null, null) }));
        var streams = new RandomStreamSet();
        streams.Add(StreamName, 7, 1);
        var pipeline = BirthCharacterCommands.CreatePipeline(streams);

        var command = new BirthCharacterCommand(
            state.CommandIds.Issue(), "player", new GameDate(300), null,
            motherId, fatherId, null, LegalStatus.RomanCitizen, SocialClass.Plebeian,
            NamePoolTestFixtures.Roman, StreamName);

        var result = pipeline.Execute(state, command);

        var born = (CharacterBornEvent)result.Events.Single();
        Assert.That(born.Legitimacy, Is.EqualTo(Legitimacy.Illegitimate));
    }

    [Test]
    public void ValidationRejectsAMissingMother()
    {
        var state = new WorldState(new GameDate(300));
        var streams = new RandomStreamSet();
        streams.Add(StreamName, 7, 1);
        var pipeline = BirthCharacterCommands.CreatePipeline(streams);

        var command = new BirthCharacterCommand(
            state.CommandIds.Issue(), "player", new GameDate(300), null,
            new RuntimeIdCounter<Character>().Issue(), null, Sex.Male, LegalStatus.RomanCitizen, null,
            NamePoolTestFixtures.Roman, StreamName);

        var result = pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(BirthCharacterCommands.MotherNotFound));
    }

    [Test]
    public void ValidationRejectsAMotherWhoIsTooYoung()
    {
        var state = new WorldState(new GameDate(60));
        var motherId = state.CharacterIds.Issue();
        state.Characters.Add(motherId, CharacterTestFixtures.Minimal(motherId, birthDate: new GameDate(50)));
        var streams = new RandomStreamSet();
        streams.Add(StreamName, 7, 1);
        var pipeline = BirthCharacterCommands.CreatePipeline(streams);

        var command = new BirthCharacterCommand(
            state.CommandIds.Issue(), "player", new GameDate(60), null,
            motherId, null, Sex.Male, LegalStatus.RomanCitizen, null,
            NamePoolTestFixtures.Roman, StreamName);

        var result = pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(BirthCharacterCommands.MotherNotOfAge));
    }

    [Test]
    public void ValidationRejectsADeceasedMother()
    {
        var state = new WorldState(new GameDate(300));
        var motherId = state.CharacterIds.Issue();
        state.Characters.Add(motherId, CharacterTestFixtures.Minimal(
            motherId, birthDate: new GameDate(300 - 25 * 12),
            deathRecord: new DeathRecord(new GameDate(200), DeathCause.Disease, 20)));
        var streams = new RandomStreamSet();
        streams.Add(StreamName, 7, 1);
        var pipeline = BirthCharacterCommands.CreatePipeline(streams);

        var command = new BirthCharacterCommand(
            state.CommandIds.Issue(), "player", new GameDate(300), null,
            motherId, null, Sex.Male, LegalStatus.RomanCitizen, null,
            NamePoolTestFixtures.Roman, StreamName);

        var result = pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(BirthCharacterCommands.MotherDeceased));
    }

    [Test]
    public void ValidationRejectsMotherBeingRecordedAsFather()
    {
        var state = new WorldState(new GameDate(300));
        var motherId = state.CharacterIds.Issue();
        state.Characters.Add(motherId, CharacterTestFixtures.Minimal(motherId, birthDate: new GameDate(300 - 25 * 12)));
        var streams = new RandomStreamSet();
        streams.Add(StreamName, 7, 1);
        var pipeline = BirthCharacterCommands.CreatePipeline(streams);

        var command = new BirthCharacterCommand(
            state.CommandIds.Issue(), "player", new GameDate(300), null,
            motherId, motherId, Sex.Male, LegalStatus.RomanCitizen, null,
            NamePoolTestFixtures.Roman, StreamName);

        var result = pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(BirthCharacterCommands.MotherIsFather));
    }

    [Test]
    public void ValidationRejectsAMissingFather()
    {
        var state = new WorldState(new GameDate(300));
        var motherId = state.CharacterIds.Issue();
        state.Characters.Add(motherId, CharacterTestFixtures.Minimal(motherId, birthDate: new GameDate(300 - 25 * 12)));
        var streams = new RandomStreamSet();
        streams.Add(StreamName, 7, 1);
        var pipeline = BirthCharacterCommands.CreatePipeline(streams);

        var nonExistentFatherId = state.CharacterIds.Issue();
        var command = new BirthCharacterCommand(
            state.CommandIds.Issue(), "player", new GameDate(300), null,
            motherId, nonExistentFatherId, Sex.Male, LegalStatus.RomanCitizen, null,
            NamePoolTestFixtures.Roman, StreamName);

        var result = pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(BirthCharacterCommands.FatherNotFound));
    }
}
