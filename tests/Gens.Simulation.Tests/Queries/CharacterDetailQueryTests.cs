using Gens.Simulation.Characters;
using Gens.Simulation.Queries;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Queries;

public sealed class CharacterDetailQueryTests
{
    [Test]
    public void ProjectsTheFullSheetForTheRequestedCharacter()
    {
        var state = new WorldState(new GameDate(240));
        var motherId = state.CharacterIds.Issue();
        state.Characters.Add(motherId, CharacterTestFixtures.Minimal(motherId));

        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId,
            praenomen: "Gaius",
            nomen: "Julius",
            birthDate: new GameDate(0),
            motherId: motherId,
            status: LegalStatus.Enslaved,
            duty: new DutyAssignment(state.HouseholdIds.Issue(), DutySlot.Craftsman, new GameDate(0))));

        var projection = new CharacterDetailQuery(characterId).Execute(state, "player");

        Assert.Multiple(() =>
        {
            Assert.That(projection.FullName, Is.EqualTo("Gaius Julius"));
            Assert.That(projection.AgeInYears, Is.EqualTo(20));
            Assert.That(projection.IsAlive, Is.True);
            Assert.That(projection.LegalStatus, Is.EqualTo(LegalStatus.Enslaved));
            Assert.That(projection.DutySlot, Is.EqualTo(DutySlot.Craftsman));
            Assert.That(projection.MotherId, Is.EqualTo(motherId.ToTaggedString()));
        });
    }

    [Test]
    public void ThrowsWhenTheCharacterDoesNotExist()
    {
        var state = new WorldState(new GameDate(0));
        var missingId = state.CharacterIds.Issue();

        Assert.That(() => new CharacterDetailQuery(missingId).Execute(state, "player"),
            Throws.TypeOf<KeyNotFoundException>());
    }
}
