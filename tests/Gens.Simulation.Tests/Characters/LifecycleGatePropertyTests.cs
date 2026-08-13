using FsCheck;
using FsCheck.Fluent;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Tests.Characters;

/// <summary>Property-based coverage of the Adolescent-or-older lifecycle gate shared by <see
/// cref="BirthCharacterCommands"/> (a mother must be past Child) and <see cref="AssignDutyCommands"/>
/// (an assignee must be past Child), plus the underlying monotonicity guarantee <see
/// cref="Character.GetLifecycleStage"/> itself relies on: a Character's stage as of a later date is
/// never earlier than its stage as of an earlier date.</summary>
public sealed class LifecycleGatePropertyTests
{
    private const string StreamName = "test-generation";

    [FsCheck.NUnit.Property]
    public FsCheck.Property LifecycleStageNeverRegressesAsTimeAdvances(NonNegativeInt ageAYears, NonNegativeInt ageBYears)
    {
        var a = ageAYears.Get % 150;
        var b = ageBYears.Get % 150;
        var earlierYears = Math.Min(a, b);
        var laterYears = Math.Max(a, b);

        var birthDate = new GameDate(0);
        var character = CharacterTestFixtures.Minimal(new RuntimeIdCounter<Character>().Issue(), birthDate: birthDate);

        var earlierStage = character.GetLifecycleStage(new GameDate(earlierYears * 12));
        var laterStage = character.GetLifecycleStage(new GameDate(laterYears * 12));

        return ((int)laterStage >= (int)earlierStage).ToProperty();
    }

    [FsCheck.NUnit.Property]
    public FsCheck.Property BirthCommandIsGatedExactlyOnWhetherTheMotherIsPastChildhood(NonNegativeInt motherAgeYears)
    {
        var age = motherAgeYears.Get % 100;
        var submittedDate = new GameDate(1200);
        var motherBirthDate = new GameDate(1200 - age * 12);

        var state = new WorldState(submittedDate);
        var motherId = state.CharacterIds.Issue();
        state.Characters.Add(motherId, CharacterTestFixtures.Minimal(motherId, birthDate: motherBirthDate));

        var streams = new RandomStreamSet();
        streams.Add(StreamName, 7, 1);
        var pipeline = BirthCharacterCommands.CreatePipeline(streams);
        var command = new BirthCharacterCommand(
            state.CommandIds.Issue(), "player", submittedDate, null, motherId, null, Sex.Male,
            LegalStatus.RomanCitizen, null, NamePoolTestFixtures.Roman, StreamName);

        var result = pipeline.Execute(state, command);
        var expectedAccepted = age > 12;

        if (expectedAccepted)
            return result.Accepted.ToProperty();

        return (!result.Accepted && result.Error == BirthCharacterCommands.MotherNotOfAge).ToProperty();
    }

    [FsCheck.NUnit.Property]
    public FsCheck.Property AssignDutyCommandIsGatedExactlyOnWhetherTheCharacterIsPastChildhood(NonNegativeInt ageYears)
    {
        var age = ageYears.Get % 100;
        var submittedDate = new GameDate(1200);
        var birthDate = new GameDate(1200 - age * 12);

        var state = new WorldState(submittedDate);
        var householdId = state.HouseholdIds.Issue();
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId, birthDate: birthDate, household: householdId));

        var command = new AssignDutyCommand(
            state.CommandIds.Issue(), "player", submittedDate, null, characterId, householdId, DutySlot.Cook);
        var result = AssignDutyCommands.Pipeline.Execute(state, command);
        var expectedAccepted = age > 12;

        if (expectedAccepted)
            return result.Accepted.ToProperty();

        return (!result.Accepted && result.Error == AssignDutyCommands.TooYoung).ToProperty();
    }
}
