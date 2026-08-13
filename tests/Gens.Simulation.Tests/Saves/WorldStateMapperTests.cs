using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Saves;

public sealed class WorldStateMapperTests
{
    [Test]
    public void ACharacterWithEveryFieldPopulatedRoundTripsThroughTheDtoAndCanonicalJson()
    {
        var state = new WorldState(new GameDate(42));
        var characterId = state.CharacterIds.Issue();
        var householdId = state.HouseholdIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        var visualProfile = new CharacterVisualProfile
        {
            Height = Height.Tall,
            Build = Build.Muscular,
            FacialStructure = FacialStructure.Angular,
            Complexion = Complexion.Bronzed,
            HairColor = HairColor.Auburn,
            HairStyle = HairStyle.Long,
            EyeColor = EyeColor.Green,
            NotableFeatures = new[] { NotableFeature.Scar, NotableFeature.GrayAtTemples },
            Portrait = PortraitRecipeGenerator.Generate(
                Height.Tall, Build.Muscular, FacialStructure.Angular, Complexion.Bronzed,
                HairColor.Auburn, HairStyle.Long, EyeColor.Green,
                new[] { NotableFeature.Scar, NotableFeature.GrayAtTemples }),
        };
        var character = Character.Create(
            id: characterId,
            praenomen: "Gaia",
            nomen: "Aurelia",
            cognomen: "Prima",
            sex: Sex.Female,
            birthDate: new GameDate(12),
            visualProfile: visualProfile,
            status: LegalStatus.RomanCitizen,
            socialClass: SocialClass.Senatorial,
            culture: new DefinitionId<Culture>("roman"),
            location: settlementId,
            household: householdId,
            attributes: new CoreAttributes(10, 20, 30, 40, 50),
            skills: new LaborSkills(1, 2, 3, 4, 5),
            condition: new Condition(60, 70, 80, 90, 100),
            source: CharacterSource.Guest,
            instantiatedAtMonth: 42);
        state.Characters.Add(characterId, character);

        var dto = WorldStateMapper.ToDto(state);
        var restoredState = WorldStateMapper.ToWorldState(dto);

        Assert.That(restoredState.Characters.TryGet(characterId, out var restored), Is.True);
        Assert.That(restored, Is.EqualTo(character));

        var bytesA = CanonicalJson.SerializeToCanonicalBytes(dto);
        var bytesB = CanonicalJson.SerializeToCanonicalBytes(WorldStateMapper.ToDto(restoredState));
        Assert.That(bytesB, Is.EqualTo(bytesA));
    }
}
