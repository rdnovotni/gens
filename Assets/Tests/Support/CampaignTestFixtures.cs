#nullable enable

using System;
using Gens.Presentation.Shell;
using Gens.Simulation.Buildings;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Time;

namespace Gens.Presentation.Tests.Support;

/// <summary>Shared campaign-bootstrapping helpers for the Phase 9 item 9 presentation-layer test
/// suites (EditMode adapter/shell tests, the PlayMode golden-path test, and the EditMode 24-month
/// soak test). Mirrors <c>tests/Gens.Simulation.Tests</c>' own fixture convention (e.g.
/// <c>CharacterTestFixtures</c>, <c>EstateChainFixtures</c>) rather than duplicating campaign setup in
/// every test file — that assembly is not reachable from a Unity asmdef, so this is a self-contained
/// equivalent scoped to what the presentation layer's own tests need.</summary>
public static class CampaignTestFixtures
{
    public const ulong DefaultSeed = 918273645UL;

    /// <summary>25 in-game years after the epoch, so a Character born at <see cref="GameDate(0)"/>
    /// (see <see cref="AddAdultHouseholdMember"/>) is already well past <c>LifecycleStage.Adolescent</c>
    /// (13+) the moment the campaign begins — a fresh <see cref="CampaignBootstrapper"/> run has no
    /// Characters of its own to draw on (Phase 4's "empty campaign"), so every test that needs a
    /// labor-eligible household member has to instantiate one itself.</summary>
    public const int DefaultStartMonths = 300;

    public static CampaignConfig BuildConfig(ulong seed = DefaultSeed, int startMonths = DefaultStartMonths) => new()
    {
        Seed = seed,
        StartDate = new GameDate(startMonths),
        RulesetId = "classic",
        ContentPackHash = "content-hash-placeholder",
        RegionId = "latium",
        Difficulty = "standard",
    };

    public static CampaignShell Bootstrap(ulong seed = DefaultSeed, int startMonths = DefaultStartMonths) =>
        CampaignShell.Bootstrap(BuildConfig(seed, startMonths), out _);

    /// <summary>Adds one living, Adult, labor-eligible member to <paramref name="shell"/>'s household
    /// directly onto <see cref="CampaignShell.State"/> — test setup, not a UI action, mirroring
    /// <c>HeadlessCampaignSoakTests</c>' own "seed state directly, exercise commands afterward"
    /// convention. Every subsequent Labor duty assignment in these suites goes through the real <see
    /// cref="AssignDutyCommand"/> pipeline via <see cref="CampaignShell.Submit{TCommand}"/> — only the
    /// character's existence is test scaffolding.</summary>
    public static RuntimeId<Character> AddAdultHouseholdMember(
        CampaignShell shell, string praenomen = "Marcus", string nomen = "Aurelius", LaborSkills? skills = null)
    {
        if (shell is null)
            throw new ArgumentNullException(nameof(shell));

        var characterId = shell.State.CharacterIds.Issue();
        var character = Character.Create(
            id: characterId,
            praenomen: praenomen,
            nomen: nomen,
            cognomen: null,
            sex: Sex.Male,
            birthDate: new GameDate(0),
            visualProfile: MinimalVisualProfile,
            status: LegalStatus.RomanCitizen,
            socialClass: SocialClass.Plebeian,
            culture: new DefinitionId<Culture>("roman"),
            location: shell.SettlementId,
            household: shell.HouseholdId,
            attributes: new CoreAttributes(10, 10, 10, 10, 10),
            skills: skills ?? new LaborSkills(40, 40, 40, 40, 40),
            condition: new Condition(80, 0, 50, 20, 50),
            source: CharacterSource.Familia,
            instantiatedAtMonth: shell.State.Date.TotalMonths);

        shell.State.Characters.Add(characterId, character);
        return characterId;
    }

    /// <summary>Provisions one Settlement/Plot/Holding/Stockpile for <paramref name="shell"/>'s
    /// household so the estate/settlement screen and the construction/production systems have
    /// somewhere to build — <see cref="CampaignBootstrapper"/> deliberately issues only bare IDs for a
    /// fresh campaign (Phase 4's "empty campaign"; no Region/Settlement/Land content-loading exists at
    /// bootstrap time), so tests exercising "build/produce" have to seed the same land records
    /// <c>ProductionNetworkTests</c> constructs by hand.</summary>
    public static RuntimeId<Holding> SeedHouseholdHolding(CampaignShell shell, long stockpileCapacity = 1000)
    {
        if (shell is null)
            throw new ArgumentNullException(nameof(shell));

        var state = shell.State;
        var householdTag = shell.HouseholdId.ToTaggedString();

        if (!state.Settlements.TryGet(shell.SettlementId, out _))
        {
            var regionId = state.RegionIds.Issue();
            state.Settlements.Add(shell.SettlementId, Settlement.Create(shell.SettlementId, regionId));
        }

        var holdingId = state.HoldingIds.Issue();
        var plotId = state.PlotIds.Issue();

        state.Holdings.Add(holdingId, Holding.Create(holdingId, shell.SettlementId, ownerId: householdTag, occupantId: householdTag));
        state.Plots.Add(plotId, Plot.Create(
            plotId, shell.SettlementId, ownerId: householdTag, occupyingHoldingId: holdingId, capacity: 4));
        state.Stockpiles.Add(holdingId, new Stockpile(stockpileCapacity));

        return holdingId;
    }

    public static RuntimeId<Plot> FirstPlotOf(CampaignShell shell, RuntimeId<Holding> holdingId)
    {
        foreach (var entry in shell.State.Plots.InAscendingOrder())
        {
            if (entry.Value.OccupyingHoldingId == holdingId)
                return entry.Key;
        }

        throw new InvalidOperationException($"Holding '{holdingId}' occupies no plot.");
    }

    /// <summary>A single-good, no-input "raw producer" building definition — the same compact shape
    /// <c>EstateChainFixtures.Ager()</c> uses, kept minimal here since these suites only need one
    /// operational recipe to prove the construction→production hand-off, not a full content-authored
    /// catalog.</summary>
    public static readonly DefinitionId<Good> GrainId = new("grain");

    public static BuildingDefinition GrainFieldDefinition() => new(
        new DefinitionId<Building>("ager"),
        BuildingTier.Tier1,
        constructionMonths: 1,
        plotCapacity: 1,
        recipe: new ProductionRecipe(Array.Empty<RecipeLine>(), new[] { new RecipeLine(GrainId, 4) }));

    public static readonly GoodDefinition[] GoodCatalog =
    {
        new(GrainId, Perishability.NonPerishable),
    };

    private static readonly CharacterVisualProfile MinimalVisualProfile = new()
    {
        Height = Height.Average,
        Build = Build.Average,
        FacialStructure = FacialStructure.Oval,
        Complexion = Complexion.Olive,
        HairColor = HairColor.Brown,
        HairStyle = HairStyle.Cropped,
        EyeColor = EyeColor.Brown,
        NotableFeatures = Array.Empty<NotableFeature>(),
        Portrait = PortraitRecipeGenerator.Generate(
            Height.Average, Build.Average, FacialStructure.Oval, Complexion.Olive,
            HairColor.Brown, HairStyle.Cropped, EyeColor.Brown, Array.Empty<NotableFeature>()),
    };
}
