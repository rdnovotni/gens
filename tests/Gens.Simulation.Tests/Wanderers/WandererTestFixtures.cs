using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Regions;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using Gens.Simulation.Wanderers;

namespace Gens.Simulation.Tests.Wanderers;

/// <summary>A small, deterministic Gazetteer roster covering every <see cref="GazetteerRole"/> and every
/// <see cref="ProminenceTier"/> the itinerary weighting reads, plus the shared helpers this namespace's
/// tests build on. Mirrors <c>Regions.SampleRegionProfileDefinitions</c>'s "fixture, not authored
/// content" framing — it exists so a movement test can prove a weighting, not to seed a campaign.</summary>
public static class WandererTestFixtures
{
    public static readonly DefinitionId<RegionProfileDefinition> Region = new("wanderer-test-region");
    public static readonly DefinitionId<GazetteerLocationDefinition> Seat = new("wanderer-test-seat");
    public static readonly DefinitionId<GazetteerLocationDefinition> Port = new("wanderer-test-port");
    public static readonly DefinitionId<GazetteerLocationDefinition> Shrine = new("wanderer-test-shrine");
    public static readonly DefinitionId<GazetteerLocationDefinition> Outpost = new("wanderer-test-outpost");
    public static readonly DefinitionId<Culture> Culture = new("wanderer-test-culture");

    public static readonly WandererTypeCatalog TypeCatalog = WandererTypeCatalog.BuildDefault();

    public static GazetteerLocationDefinition SeatLocation { get; } = new(
        Seat, Region, "Test Provincial Seat",
        new[] { GazetteerRole.ProvincialSeat, GazetteerRole.MarketHub },
        ProminenceTier.ProvincialSeat,
        "Fixture stand-in for a regional capital.");

    public static GazetteerLocationDefinition PortLocation { get; } = new(
        Port, Region, "Test Major Port",
        new[] { GazetteerRole.MajorPort },
        ProminenceTier.RegionalCenter,
        "Fixture stand-in for a trading port.");

    public static GazetteerLocationDefinition ShrineLocation { get; } = new(
        Shrine, Region, "Test Sanctuary",
        new[] { GazetteerRole.Sanctuary },
        ProminenceTier.RegionalCenter,
        "Fixture stand-in for a pilgrimage site.");

    public static GazetteerLocationDefinition OutpostLocation { get; } = new(
        Outpost, Region, "Test Frontier Outpost",
        new[] { GazetteerRole.FrontierOutpost, GazetteerRole.LegionaryBase },
        ProminenceTier.Outpost,
        "Fixture stand-in for a frontier post.");

    public static RegionProfileCatalog BuildRegionCatalog() => new(new[] { BuildRegion() });

    public static RegionProfileDefinition BuildRegion() => new(
        id: Region,
        name: "Wanderer Test Region",
        status: RegionStatus.ExtensibleSlate,
        terrainProfileRef: "fixture-terrain",
        economicCharacterTag: "fixture-economy",
        politicalLegalProfileRef: "fixture-politics",
        diplomaticMilitaryProfileRef: "fixture-diplomacy",
        religiousCulturalDefaultRef: "fixture-religion",
        regionalGoodsProfileRef: "fixture-goods",
        cultureDistributionTable: new[]
        {
            new CultureDistributionEntry("wanderer-test-culture", weight: 90),
            new CultureDistributionEntry("outlier", weight: 10, isOutlierResidual: true),
        },
        reputationDuality: new DatedRule<ReputationDualityMode>(ReputationDualityMode.Full),
        homeAnchorLocationId: Seat,
        gazetteer: new[] { SeatLocation, PortLocation, ShrineLocation, OutpostLocation });

    /// <summary>Adds a tracked Wanderer directly to <paramref name="state"/>, bypassing <see
    /// cref="InstantiateWandererCommand"/> so a test can pin an exact Fame/type/location rather than
    /// depending on that command's own rolls.</summary>
    public static Wanderer AddWanderer(
        WorldState state,
        WandererType type = WandererType.PhilosopherRhetorician,
        DefinitionId<GazetteerLocationDefinition>? location = null,
        int fame = 50,
        GameDate? arrivalDate = null)
    {
        var id = state.WandererIds.Issue();
        var wanderer = Wanderer.Create(
            id: id,
            name: new CharacterName("Marcus", "Aurelius", "Sophista"),
            sex: Sex.Male,
            birthDate: new GameDate(-360),
            status: LegalStatus.Peregrine,
            culture: Culture,
            type: type,
            currentLocationId: location ?? Seat,
            fame: fame,
            arrivalDate: arrivalDate ?? new GameDate(0));
        state.Wanderers.Add(id, wanderer);
        return wanderer;
    }
}
