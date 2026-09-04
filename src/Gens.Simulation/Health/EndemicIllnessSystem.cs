using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Health;

/// <summary>The monthly §2 Endemic Illness tick (Phase 14 item 2): for every settlement, computes each
/// of the seven named endemic diseases' <see cref="EndemicExposureCalculator"/> probability from real,
/// available inputs (this settlement's own <see cref="Plot"/> terrain composition, its living
/// population's crowding ratio, and whether any of its background <see cref="PopGroup"/>s reads as
/// Lavish-consuming), scales it by <see cref="SanitationInvestmentCalculator.ExposureMultiplier"/>, and
/// rolls it against every living Character located there — a hit opens a new standing case through the
/// same <see cref="AfflictCharacterCommand"/> item 1 built as this exact future caller (that command's
/// own doc comment names "item 2's endemic-exposure rolls" explicitly). A Character already carrying an
/// active case of a given disease, or already immune to it, is simply never rolled a second time —
/// <see cref="AfflictCharacterCommands.Pipeline"/> rejects both cases on its own, so this system does
/// not duplicate that check before calling it. Phase 15 item 9 (<c>gens-public-works-euergetism-
/// design.md</c> §3) adds one further real, live multiplier on top of Sanitation Investment's own —
/// <see cref="PublicWorks.PublicWorksHealthQuery.SanitationMultiplier"/>, an operational Aqueduct and/or
/// Sewer Public Work each reducing this settlement's own exposure further — reading <c>Fixed64.One</c>
/// (no change) for a pre-item-9 save or any settlement with no such Public Works.</summary>
public sealed class EndemicIllnessSystem : IMonthlySystem<WorldState>
{
    /// <summary>Invented onset severity for a newly-afflicted endemic case (§12: "no numeric... curve
    /// exists anywhere in the design corpus") — modest, since Endemic Illness is meant to read as a
    /// slow background drain (§2) rather than an immediately dire condition.</summary>
    private const int OnsetSeverity = 20;

    private const uint RollPrecision = 1_000_000;

    private readonly string _streamName;

    public EndemicIllnessSystem(string streamName)
    {
        if (string.IsNullOrWhiteSpace(streamName))
            throw new ArgumentException("An endemic illness random stream name is required.", nameof(streamName));
        _streamName = streamName;
    }

    public string Id => "health.endemicIllness";
    public TickPhase Phase => TickPhase.Hazards;
    public IReadOnlyCollection<string> Reads { get; } = new[]
    {
        "settlements", "plots", "popGroups", "characters", "characterHealthConditions", "settlementSanitationInvestments",
    };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "characterHealthConditions", "characterHealthConditionIds", "eventIds", "commandIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var settlementEntry in state.Settlements.InAscendingOrder())
        {
            var settlementId = settlementEntry.Key;
            var profile = SettlementHealthProfile.Compute(state, settlementId);
            var sanitationMultiplier = SanitationInvestmentCalculator.ExposureMultiplier(
                SanitationQueries.EffectiveTier(state, settlementId));
            // Phase 15 item 9's own real, live extension (gens-public-works-euergetism-design.md §3): an
            // operational Aqueduct and/or Sewer Public Work each further reduce this settlement's own
            // Endemic Illness exposure, stacking multiplicatively on top of Sanitation Investment's own
            // tier multiplier. A pre-item-9 save (or any settlement with no such Public Works) reads
            // Fixed64.One here and multiplies through unchanged.
            sanitationMultiplier *= (double)PublicWorks.PublicWorksHealthQuery.SanitationMultiplier(state, settlementId).RawValue
                / Numerics.Fixed64.One.RawValue;

            var residents = state.Characters.InAscendingOrder()
                .Where(entry => entry.Value.IsAlive && entry.Value.Location == settlementId)
                .Select(entry => entry.Value)
                .ToArray();

            if (residents.Length == 0)
                continue;

            foreach (var diseaseProfile in DiseaseCatalog.EndemicProfiles)
            {
                var baseProbability = diseaseProfile.Driver switch
                {
                    EndemicExposureDriver.MarshTerrain => EndemicExposureCalculator.RomanFeverMonthlyProbability(profile.MarshFraction),
                    EndemicExposureDriver.PoorSanitation => EndemicExposureCalculator.TheFluxMonthlyProbability(),
                    EndemicExposureDriver.PopulationDensity => EndemicExposureCalculator.ConsumptionMonthlyProbability(profile.CrowdingRatio),
                    EndemicExposureDriver.TimeOnly => EndemicExposureCalculator.LeprosyMonthlyProbability(),
                    EndemicExposureDriver.LavishDiet => EndemicExposureCalculator.GoutMonthlyProbability(profile.IsLavish),
                    EndemicExposureDriver.RegionalFlavorUnmodeled => EndemicExposureCalculator.OphthalmiaMonthlyProbability(),
                    EndemicExposureDriver.LeadWealthOrMining => EndemicExposureCalculator.SaturnismMonthlyProbability(profile.IsLavish, profile.HillsFraction),
                    _ => throw new InvalidOperationException($"Unhandled endemic exposure driver '{diseaseProfile.Driver}'."),
                };

                var probability = Math.Clamp(baseProbability * sanitationMultiplier, 0.0, 1.0);
                var threshold = (uint)Math.Clamp(probability * RollPrecision, 0, RollPrecision);
                if (threshold == 0)
                    continue;

                foreach (var character in residents)
                {
                    var roll = context.RandomStreams.NextUInt(_streamName, RollPrecision);
                    if (roll >= threshold)
                        continue;

                    var command = new AfflictCharacterCommand(
                        state.CommandIds.Issue(), "system", context.Date, CausationId: null, character.Id,
                        diseaseProfile.ConditionId, HealthConditionCategory.Chronic, HasCure: false, OnsetSeverity);
                    var result = AfflictCharacterCommands.Pipeline.Execute(state, command);
                    if (result.Accepted)
                        events.AddRange(result.Events);
                }
            }
        }

        return events;
    }

    /// <summary>Per-settlement inputs computed once per tick rather than re-scanned per disease per
    /// Character — a plain snapshot, not a <c>WorldState</c> partition.</summary>
    private readonly record struct SettlementHealthProfile(double MarshFraction, double HillsFraction, double CrowdingRatio, bool IsLavish)
    {
        public static SettlementHealthProfile Compute(WorldState state, RuntimeId<Settlement> settlementId)
        {
            var plots = state.Plots.InAscendingOrder()
                .Where(entry => entry.Value.SettlementId == settlementId)
                .Select(entry => entry.Value)
                .ToArray();

            var totalPlots = plots.Length;
            var marshFraction = totalPlots == 0 ? 0.0 : plots.Count(p => p.Terrain == TerrainType.Marsh) / (double)totalPlots;
            var hillsFraction = totalPlots == 0 ? 0.0 : plots.Count(p => p.Terrain == TerrainType.Hills) / (double)totalPlots;
            var totalCapacity = plots.Sum(p => (double)p.Capacity);

            var namedPopulation = state.Characters.InAscendingOrder()
                .Count(entry => entry.Value.IsAlive && entry.Value.Location == settlementId);
            var backgroundPopulation = state.PopGroups.InAscendingOrder()
                .Where(entry => entry.Key.SettlementId == settlementId)
                .Sum(entry => (double)entry.Value.Size);
            var crowdingRatio = totalCapacity <= 0 ? 0.0 : (namedPopulation + backgroundPopulation) / totalCapacity;

            var isLavish = state.PopGroups.InAscendingOrder().Any(entry =>
                entry.Key.SettlementId == settlementId &&
                entry.Value.WealthBand == WealthBand.EliteDiscretionary &&
                entry.Value.NeedsProfile == DietTier.Generous);

            return new SettlementHealthProfile(marshFraction, hillsFraction, crowdingRatio, isLavish);
        }
    }
}
