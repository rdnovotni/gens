using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Saves;

public sealed class WorldStateMapperTests
{
    [Test]
    public void ACharacterWithEveryFieldPopulatedRoundTripsThroughTheDtoAndCanonicalJson()
    {
        var state = new WorldState(new GameDate(42));
        var motherId = state.CharacterIds.Issue();
        var fatherId = state.CharacterIds.Issue();
        var spouseId = state.CharacterIds.Issue();
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
            HairStyle = HairStyle.Flowing,
            EyeColor = EyeColor.Green,
            NotableFeatures = new[] { NotableFeature.Scar, NotableFeature.GrayAtTemples },
            Portrait = PortraitRecipeGenerator.Generate(
                Height.Tall, Build.Muscular, FacialStructure.Angular, Complexion.Bronzed,
                HairColor.Auburn, HairStyle.Flowing, EyeColor.Green,
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
            instantiatedAtMonth: 42,
            motherId: motherId,
            fatherId: fatherId,
            legitimacy: Legitimacy.Illegitimate,
            maritalHistory: new[]
            {
                new MarriageRecord(spouseId, new GameDate(24), new GameDate(36), MarriageEndReason.Divorce),
            },
            permanentInjuries: new[]
            {
                new PermanentInjury(PermanentInjuryTarget.Fertility, 15, "difficult birth", new GameDate(30)),
            },
            traits: new[] { new DefinitionId<Trait>("bold"), new DefinitionId<Trait>("honest") },
            deathRecord: new DeathRecord(new GameDate(40), DeathCause.Childbirth, 27),
            duty: new DutyAssignment(householdId, DutySlot.Cook, new GameDate(20)));
        state.Characters.Add(characterId, character);

        var dto = WorldStateMapper.ToDto(state);
        var restoredState = WorldStateMapper.ToWorldState(dto);

        Assert.That(restoredState.Characters.TryGet(characterId, out var restored), Is.True);
        Assert.That(restored, Is.EqualTo(character));

        var bytesA = CanonicalJson.SerializeToCanonicalBytes(dto);
        var bytesB = CanonicalJson.SerializeToCanonicalBytes(WorldStateMapper.ToDto(restoredState));
        Assert.That(bytesB, Is.EqualTo(bytesA));
    }

    [Test]
    public void ARelationshipWithEveryFieldPopulatedRoundTripsThroughTheDtoAndCanonicalJson()
    {
        var state = new WorldState(new GameDate(42));
        var characterId = state.CharacterIds.Issue();
        var targetId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));
        state.Characters.Add(targetId, CharacterTestFixtures.Minimal(targetId));
        var key = new RelationshipKey(characterId, targetId);
        var relationship = new Relationship(
            -55,
            BondTag.Rival | BondTag.Nemesis | BondTag.BlackmailLeverage,
            RelationshipOrigin.Political,
            new GameDate(10),
            new GameDate(38),
            "evt_0000007");
        state.Relationships.Add(key, relationship);

        var dto = WorldStateMapper.ToDto(state);
        var restoredState = WorldStateMapper.ToWorldState(dto);

        Assert.That(restoredState.Relationships.TryGet(key, out var restored), Is.True);
        Assert.That(restored, Is.EqualTo(relationship));

        var bytesA = CanonicalJson.SerializeToCanonicalBytes(dto);
        var bytesB = CanonicalJson.SerializeToCanonicalBytes(WorldStateMapper.ToDto(restoredState));
        Assert.That(bytesB, Is.EqualTo(bytesA));
    }

    [Test]
    public void ARegionRoundTripsThroughTheDtoAndCanonicalJson()
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        var region = Region.Create(regionId, "Italian Heartland");
        state.Regions.Add(regionId, region);

        var dto = WorldStateMapper.ToDto(state);
        var restoredState = WorldStateMapper.ToWorldState(dto);

        Assert.That(restoredState.Regions.TryGet(regionId, out var restored), Is.True);
        Assert.That(restored, Is.EqualTo(region));

        var bytesA = CanonicalJson.SerializeToCanonicalBytes(dto);
        var bytesB = CanonicalJson.SerializeToCanonicalBytes(WorldStateMapper.ToDto(restoredState));
        Assert.That(bytesB, Is.EqualTo(bytesA));
    }

    [Test]
    public void ASettlementRoundTripsThroughTheDtoAndCanonicalJson()
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        var settlement = Settlement.Create(settlementId, regionId, SettlementStage.Vicus);
        state.Settlements.Add(settlementId, settlement);

        var dto = WorldStateMapper.ToDto(state);
        var restoredState = WorldStateMapper.ToWorldState(dto);

        Assert.That(restoredState.Settlements.TryGet(settlementId, out var restored), Is.True);
        Assert.That(restored, Is.EqualTo(settlement));

        var bytesA = CanonicalJson.SerializeToCanonicalBytes(dto);
        var bytesB = CanonicalJson.SerializeToCanonicalBytes(WorldStateMapper.ToDto(restoredState));
        Assert.That(bytesB, Is.EqualTo(bytesA));
    }

    [Test]
    public void APlotRoundTripsThroughTheDtoAndCanonicalJson()
    {
        var state = new WorldState(new GameDate(0));
        var settlementId = state.SettlementIds.Issue();
        var plotId = state.PlotIds.Issue();
        var holdingId = state.HoldingIds.Issue();
        var plot = Plot.Create(
            plotId, settlementId, TerrainType.Coast,
            TerrainFeature.Coastline | TerrainFeature.RiverAdjacent,
            new LandCondition(73), 4, true, "household_0000001", holdingId,
            new LandAcquisition(AcquisitionMethod.Purchase, new GameDate(8), "contract_0000002"));
        state.Plots.Add(plotId, plot);

        var dto = WorldStateMapper.ToDto(state);
        var restoredState = WorldStateMapper.ToWorldState(dto);

        Assert.That(restoredState.Plots.TryGet(plotId, out var restored), Is.True);
        Assert.That(restored, Is.EqualTo(plot));

        var bytesA = CanonicalJson.SerializeToCanonicalBytes(dto);
        var bytesB = CanonicalJson.SerializeToCanonicalBytes(WorldStateMapper.ToDto(restoredState));
        Assert.That(bytesB, Is.EqualTo(bytesA));
    }

    [Test]
    public void AHoldingRoundTripsThroughTheDtoAndCanonicalJson()
    {
        var state = new WorldState(new GameDate(0));
        var settlementId = state.SettlementIds.Issue();
        var holdingId = state.HoldingIds.Issue();
        var holding = Holding.Create(
            holdingId, settlementId, "household_0000001", "household_0000002", 12);
        state.Holdings.Add(holdingId, holding);

        var dto = WorldStateMapper.ToDto(state);
        var restoredState = WorldStateMapper.ToWorldState(dto);

        Assert.That(restoredState.Holdings.TryGet(holdingId, out var restored), Is.True);
        Assert.That(restored, Is.EqualTo(holding));

        var bytesA = CanonicalJson.SerializeToCanonicalBytes(dto);
        var bytesB = CanonicalJson.SerializeToCanonicalBytes(WorldStateMapper.ToDto(restoredState));
        Assert.That(bytesB, Is.EqualTo(bytesA));
    }
}
