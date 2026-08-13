using FsCheck;
using FsCheck.Fluent;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Tests.Characters;

/// <summary>Property-based coverage of the genealogy invariants <see cref="Character.Create"/> and
/// <see cref="BirthCharacterCommands"/> enforce: a Character can never be its own parent, legitimacy
/// tracks the mother's currently-open marriage exactly, and a chain of births — each newborn later
/// becoming a mother in her own right — never produces a parent/child cycle, since every Character ID
/// this project issues is strictly increasing (ADR 0001/0004).</summary>
public sealed class GenealogyPropertyTests
{
    private const string StreamName = "test-generation";

    private static RuntimeId<Character> MakeId(int offset)
    {
        var counter = new RuntimeIdCounter<Character>();
        var id = counter.Issue();
        for (var i = 0; i < offset; i++)
            id = counter.Issue();
        return id;
    }

    [FsCheck.NUnit.Property]
    public FsCheck.Property ACharacterCanNeverBeItsOwnMother(NonNegativeInt idOffset)
    {
        var id = MakeId(idOffset.Get % 500);

        var threw = false;
        try
        {
            CharacterTestFixtures.Minimal(id, motherId: id);
        }
        catch (ArgumentException)
        {
            threw = true;
        }

        return threw.ToProperty();
    }

    [FsCheck.NUnit.Property]
    public FsCheck.Property ACharacterCanNeverBeItsOwnFather(NonNegativeInt idOffset)
    {
        var id = MakeId(idOffset.Get % 500);

        var threw = false;
        try
        {
            CharacterTestFixtures.Minimal(id, fatherId: id);
        }
        catch (ArgumentException)
        {
            threw = true;
        }

        return threw.ToProperty();
    }

    [FsCheck.NUnit.Property]
    public FsCheck.Property LegitimacyExactlyTracksWhetherTheFatherIsTheMothersCurrentSpouse(bool fatherIsCurrentSpouse)
    {
        var date = new GameDate(300);
        var state = new WorldState(date);
        var motherId = state.CharacterIds.Issue();
        var fatherId = state.CharacterIds.Issue();
        var otherManId = state.CharacterIds.Issue();
        var adultBirthDate = new GameDate(0);
        state.Characters.Add(fatherId, CharacterTestFixtures.Minimal(fatherId, birthDate: adultBirthDate));
        state.Characters.Add(otherManId, CharacterTestFixtures.Minimal(otherManId, birthDate: adultBirthDate));

        var currentSpouseId = fatherIsCurrentSpouse ? fatherId : otherManId;
        state.Characters.Add(motherId, CharacterTestFixtures.Minimal(
            motherId, birthDate: adultBirthDate,
            maritalHistory: new[] { new MarriageRecord(currentSpouseId, new GameDate(0), null, null) }));

        var streams = new RandomStreamSet();
        streams.Add(StreamName, 7, 1);
        var pipeline = BirthCharacterCommands.CreatePipeline(streams);
        var command = new BirthCharacterCommand(
            state.CommandIds.Issue(), "player", date, null, motherId, fatherId, Sex.Male,
            LegalStatus.RomanCitizen, null, NamePoolTestFixtures.Roman, StreamName);

        var result = pipeline.Execute(state, command);
        var born = (CharacterBornEvent)result.Events.Single();
        var expected = fatherIsCurrentSpouse ? Legitimacy.Legitimate : Legitimacy.Illegitimate;

        return (born.Legitimacy == expected).ToProperty();
    }

    /// <summary>Walks a chain of <see cref="BirthCharacterCommand"/>s where each newborn later gives
    /// birth herself (after enough simulated time to clear the mother-lifecycle-stage gate), and checks
    /// that every child's ID exceeds its mother's — i.e. the genealogy graph this produces can never
    /// contain a cycle, since a Character can only ever be born after every ancestor it names already
    /// exists.</summary>
    [FsCheck.NUnit.Property]
    public FsCheck.Property ABirthChainNeverProducesAGenealogyCycle(PositiveInt generationCountRaw)
    {
        var generations = Math.Min(generationCountRaw.Get, 6);
        const int monthsPerGeneration = 20 * 12;

        var date = new GameDate(0);
        var state = new WorldState(date);
        var streams = new RandomStreamSet();
        streams.Add(StreamName, 7, 1);
        var pipeline = BirthCharacterCommands.CreatePipeline(streams);

        var founderId = state.CharacterIds.Issue();
        state.Characters.Add(founderId, CharacterTestFixtures.Minimal(founderId, birthDate: new GameDate(-monthsPerGeneration)));

        var motherId = founderId;
        var seenIds = new HashSet<RuntimeId<Character>> { founderId };

        for (var generation = 0; generation < generations; generation++)
        {
            date = new GameDate(date.TotalMonths + monthsPerGeneration);
            var command = new BirthCharacterCommand(
                state.CommandIds.Issue(), "player", date, null, motherId, null, Sex.Female,
                LegalStatus.RomanCitizen, null, NamePoolTestFixtures.Roman, StreamName);

            var result = pipeline.Execute(state, command);
            if (!result.Accepted)
                return false.ToProperty();

            var born = (CharacterBornEvent)result.Events.Single();
            if (born.CharacterId.Value <= motherId.Value)
                return false.ToProperty();
            if (!seenIds.Add(born.CharacterId))
                return false.ToProperty();

            motherId = born.CharacterId;
        }

        return true.ToProperty();
    }
}
