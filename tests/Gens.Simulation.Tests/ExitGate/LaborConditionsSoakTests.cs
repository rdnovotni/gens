using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.ExitGate;

/// <summary>
/// Proves this item's slice of the Phase 6 exit gate: one estate's enslaved labor force runs for 120
/// months with deterministic, bounded output — Regimen, fatigue, and flight/pursuit/manumission state
/// transitions resolve consistently. This is not the full Phase 6 estate exit gate ("one estate
/// transforms inputs, labor, time, and maintenance into deterministic outputs for 120 months"), which
/// also needs items 1-5/7/8 (land, buildings, production chains); it covers only this item's labor
/// slice of that gate.
/// </summary>
public sealed class LaborConditionsSoakTests
{
    private static readonly NamePool RomanPool = NamePoolTestFixtures.Roman;

    [Test]
    public void AnEstateOfFourEnslavedLaborersRunsForOneHundredTwentyMonthsWithBoundedDeterministicState()
    {
        var config = BuildConfig(9001UL);
        var campaign = CampaignBootstrapper.Bootstrap(config);
        var state = campaign.State;
        var streams = campaign.RandomStreams;
        var simulation = NewSimulation();

        var laborerIds = SetupEstate(state, streams, campaign.SettlementId, campaign.HouseholdId);

        const int totalMonths = 120;
        for (var month = 0; month < totalMonths; month++)
        {
            simulation.Tick(state, state.Date, streams);
            state.AdvanceMonth();
        }

        Assert.That(state.Date.TotalMonths, Is.EqualTo(totalMonths));

        foreach (var id in laborerIds)
        {
            state.Characters.TryGet(id, out var character);

            // A fled Character is no longer on Duty and has no Household — the two states are mutually
            // exclusive by construction (LaborFlightSystem clears Duty/Household together).
            Assert.That(
                character.Flight is null || character.Duty is null,
                $"Character {id} must not hold a Duty while marked as fled.");

            // Condition's own constructor already enforces the 0-100 range on every write; this just
            // confirms 120 months of ticks never produced an out-of-band value some other way.
            Assert.That(character.Condition.Health, Is.InRange(0, 100));
            Assert.That(character.Condition.Fatigue, Is.InRange(0, 100));
            Assert.That(character.Condition.Loyalty, Is.InRange(0, 100));
        }
    }

    [Test]
    public void SameSeedReproducesIdenticalHashAfterAFullEstateSoakRun()
    {
        var config = BuildConfig(4242UL);

        Assert.That(RunSoak(config, 120), Is.EqualTo(RunSoak(config, 120)));
    }

    private static ulong RunSoak(CampaignConfig config, int months)
    {
        var campaign = CampaignBootstrapper.Bootstrap(config);
        var state = campaign.State;
        var streams = campaign.RandomStreams;

        SetupEstate(state, streams, campaign.SettlementId, campaign.HouseholdId);

        var simulation = NewSimulation();
        for (var month = 0; month < months; month++)
        {
            simulation.Tick(state, state.Date, streams);
            state.AdvanceMonth();
        }

        return StateHasher.Hash(state);
    }

    /// <summary>Promotes 4 enslaved laborers into the household, sets a harsh whole-household Regimen
    /// default and a gentler per-individual override for one of them, and assigns each a distinct duty
    /// slot (best-effort — a randomly-generated skill score may or may not clear the minimum
    /// competence gate, matching <c>FamiliaHouseholdSoakTests</c>' identical "bonus coverage, not
    /// asserted" convention for duty assignment).</summary>
    private static RuntimeId<Character>[] SetupEstate(
        WorldState state, RandomStreamSet streams, RuntimeId<Settlement> settlement, RuntimeId<Household> household)
    {
        var culture = new DefinitionId<Culture>("roman");

        state.PopGroups.Add(
            new PopGroupKey(settlement, PopGroupType.Operarii),
            PopGroup.Create(settlement, PopGroupType.Operarii, 10));

        var promotePipeline = PromoteToNamedCommands.CreatePipeline(streams);
        var slots = new[] { DutySlot.FieldHand, DutySlot.DomesticServant, DutySlot.Craftsman, DutySlot.Cook };
        var laborerIds = new RuntimeId<Character>[slots.Length];

        for (var i = 0; i < slots.Length; i++)
        {
            var result = promotePipeline.Execute(state, new PromoteToNamedCommand(
                state.CommandIds.Issue(), "system", state.Date, null,
                settlement, PopGroupType.Operarii, CharacterSource.LaborDutyHire,
                null, LegalStatus.Enslaved, null,
                culture, RomanPool, CampaignBootstrapper.CharacterPromotionStreamName, household));
            Assert.That(result.Accepted, Is.True, "PromoteToNamedCommand for an enslaved laborer must be accepted.");
            var id = ((CharacterPromotedEvent)result.Events.Single()).CharacterId;
            laborerIds[i] = id;

            AssignDutyCommands.Pipeline.Execute(state, new AssignDutyCommand(
                state.CommandIds.Issue(), "system", state.Date, null, id, household, slots[i]));
        }

        SetGroupRegimenCommands.Pipeline.Execute(state, new SetGroupRegimenCommand(
            state.CommandIds.Issue(), "system", state.Date, null, household, null,
            new RegimenSettings(DietTier.Meager, AccommodationTier.Bare, FreedomsTier.Confined, DisciplineTier.Harsh)));

        SetIndividualRegimenCommands.Pipeline.Execute(state, new SetIndividualRegimenCommand(
            state.CommandIds.Issue(), "system", state.Date, null, laborerIds[0],
            new RegimenSettings(DietTier.Generous, AccommodationTier.Comfortable, FreedomsTier.FreeMovement, DisciplineTier.Lenient)));

        return laborerIds;
    }

    private static WriteSetVerifyingSimulation NewSimulation() =>
        new(new IMonthlySystem<WorldState>[]
        {
            new ScheduledActionSystem(),
            new CharacterLifecycleSystem(CampaignBootstrapper.CharacterMortalityStreamName),
            new LaborOutputSystem(),
            new LaborFlightSystem(
                CampaignBootstrapper.CharacterLaborFlightStreamName, CampaignBootstrapper.CharacterPursuitOutcomeStreamName),
        });

    private static CampaignConfig BuildConfig(ulong seed) => new()
    {
        Seed = seed,
        StartDate = new GameDate(0),
        RulesetId = "classic",
        ContentPackHash = "content-hash-placeholder",
        RegionId = "latium",
        Difficulty = "standard",
    };
}
