using Gens.Simulation.Buildings;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Numerics;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Hazards;

/// <summary>The monthly §3/§5 Disaster Event tick (Phase 14 item 3): for every settlement and each of
/// the eight standing hazards (<see cref="HazardType.VolcanicEruption"/> excepted — a rare, unrolled
/// "hook now, caller later" feature per <see cref="DormantVolcano"/>'s own doc comment), reads this
/// month's live <see cref="HazardExposureProfile"/>, rolls <see
/// cref="DisasterSeverityCalculator.MonthlyIgnitionProbability"/> and, on ignition, <see
/// cref="DisasterSeverityCalculator.RollSeverity"/>, then applies §5's real effects: building condition
/// loss via <see cref="BuildingInstance.ApplyDisasterDamage"/> (structural hazards only — Drought/Famine
/// and Blight &amp; Infestation never touch a building, matching §5.3's own "act on yield and Contentment
/// rather than physical structures"; Frost only from <see cref="DisasterSeverity.Severe"/> up, via <see
/// cref="DisasterDamageCalculator.FrostBuildingConditionStepsLost"/>'s own harsher perennial-crop-setback
/// figure), §5.3's Catastrophic-only population loss on every structural hazard, and a same-month
/// Contentment shock (<see cref="DisasterDamageCalculator.ContentmentImpact"/>) on every hazard
/// regardless of severity or structural/non-structural kind. §3.1's Storm-into-Flood chaining is rolled
/// immediately after a qualifying Storm resolves, on the same settlement, the same month. A standing
/// <see cref="DisasterEvent"/> record is written for every fired Event, structural or not, chained or
/// not — this system's entire realization of §5's "disaster instances" scope item.
///
/// <para><b>Deliberately not modeled here</b> (matching this namespace's own top-level, per-calculator
/// disclosures): livestock/cargo/vessel loss (§5.3 — no Pasture/livestock, vessel, or cargo concept
/// exists anywhere in this codebase yet); Disaster Relief as a Funded Action (§6.2, the design document's
/// own deferred territory); Religion's Omen skew (§6.1) and any other cross-system wiring beyond what
/// this item's own construction-order line names — Phase 14 item 5's own "goods, buildings, populations
/// ... events" integration wave, not this item's; and Volcanic Eruption's own real trigger (§2.2), which
/// stays the callerless hook <see cref="DesignateDormantVolcanoCommand"/> already established.</para>
///
/// <para>Item 5's own real "warnings where appropriate" (this Phase's own exit-gate language): a settlement
/// whose <see cref="HazardWarningCalculator.IsElevated"/> Exposure did not actually ignite this month still
/// gets a <see cref="HazardElevatedExposureWarningEvent"/> — the live <see
/// cref="HazardExposureProfile.ExposureFor"/> reading <see cref="HazardQueries"/>'s own doc comment already
/// named as its "forecast/knowledge" realization, now surfaced as a real domain event that <see
/// cref="Campaign.MonthlyReportProjector"/>'s existing generic event-projection already carries into the
/// Monthly Report with no further wiring needed.</para></summary>
public sealed class NaturalDisasterSystem : IMonthlySystem<WorldState>
{
    /// <summary>The eight standing hazards this system rolls every month, in the same top-to-bottom
    /// order §2's own table lists them — <see cref="HazardType.VolcanicEruption"/> is deliberately
    /// excluded (see this type's own doc comment).</summary>
    private static readonly HazardType[] StandingHazards =
    {
        HazardType.Fire, HazardType.Flood, HazardType.Earthquake, HazardType.DroughtFamine,
        HazardType.Storm, HazardType.Landslide, HazardType.BlightInfestation, HazardType.Frost,
    };

    /// <summary>The five structural hazards §5.2/§5.3 name as ones that produce genuine building/cargo
    /// damage — Frost is handled separately (only from <see cref="DisasterSeverity.Severe"/> up, via its
    /// own harsher step count) rather than listed here, since below Severe it is Contentment-only exactly
    /// like Drought/Famine and Blight &amp; Infestation.</summary>
    private static readonly HashSet<HazardType> StructuralHazards = new()
    {
        HazardType.Fire, HazardType.Flood, HazardType.Earthquake, HazardType.Storm, HazardType.Landslide,
    };

    public const uint RollPrecision = DisasterSeverityCalculator.RollPrecision;

    private readonly string _streamName;

    public NaturalDisasterSystem(string streamName)
    {
        if (string.IsNullOrWhiteSpace(streamName))
            throw new ArgumentException("A natural disaster random stream name is required.", nameof(streamName));
        _streamName = streamName;
    }

    public string Id => "hazards.naturalDisaster";
    public TickPhase Phase => TickPhase.Hazards;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "settlements", "plots", "buildings", "popGroups" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "buildings", "popGroups", "disasterEvents", "disasterEventIds", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var settlementEntry in state.Settlements.InAscendingOrder())
        {
            var settlementId = settlementEntry.Key;
            var profile = HazardExposureProfile.Compute(state, settlementId, context.Date);

            foreach (var hazardType in StandingHazards)
            {
                var exposure = profile.ExposureFor(hazardType);
                var ignitionProbability = DisasterSeverityCalculator.MonthlyIgnitionProbability(exposure);
                var ignitionThreshold = (uint)Math.Clamp(ignitionProbability * RollPrecision, 0, RollPrecision);
                var ignitionRoll = context.RandomStreams.NextUInt(_streamName, RollPrecision);
                if (ignitionRoll >= ignitionThreshold)
                {
                    if (HazardWarningCalculator.IsElevated(exposure))
                        events.Add(new HazardElevatedExposureWarningEvent(state.EventIds.Issue(), context.Date, settlementId, hazardType, exposure));
                    continue;
                }

                var severityRoll = context.RandomStreams.NextUInt(_streamName, RollPrecision);
                var severity = DisasterSeverityCalculator.RollSeverity(exposure, severityRoll);

                FireDisasterEvent(state, context, settlementId, hazardType, severity, triggeredByCompounding: false, events);

                if (hazardType != HazardType.Storm || profile.RiverAdjacentFraction <= 0.0)
                    continue;

                var chainProbability = DisasterCompoundingCalculator.StormToFloodChainProbability(severity);
                if (chainProbability <= 0.0)
                    continue;

                var chainThreshold = (uint)Math.Clamp(chainProbability * RollPrecision, 0, RollPrecision);
                var chainRoll = context.RandomStreams.NextUInt(_streamName, RollPrecision);
                if (chainRoll >= chainThreshold)
                    continue;

                var chainedFloodSeverity = DisasterCompoundingCalculator.ChainedFloodSeverity(severity);
                FireDisasterEvent(state, context, settlementId, HazardType.Flood, chainedFloodSeverity, triggeredByCompounding: true, events);
            }
        }

        return events;
    }

    private void FireDisasterEvent(
        WorldState state, MonthlyTickContext context, RuntimeId<Settlement> settlementId,
        HazardType hazardType, DisasterSeverity severity, bool triggeredByCompounding, List<IDomainEvent> events)
    {
        var perennialCropSetback = hazardType == HazardType.Frost && severity >= DisasterSeverity.Severe;
        var isStructural = StructuralHazards.Contains(hazardType) || perennialCropSetback;

        var buildingsDamaged = isStructural
            ? ApplyBuildingDamage(state, context, settlementId, hazardType, severity, perennialCropSetback)
            : 0;

        var populationLost = severity == DisasterSeverity.Catastrophic && StructuralHazards.Contains(hazardType)
            ? ApplyPopulationLoss(state, settlementId, severity)
            : 0;

        ApplyContentmentImpact(state, settlementId, severity);

        var eventId = state.DisasterEventIds.Issue();
        var disasterEvent = DisasterEvent.Create(
            eventId, settlementId, context.Date, hazardType, severity,
            triggeredByCompounding, buildingsDamaged, populationLost, perennialCropSetback);
        state.DisasterEvents.Add(eventId, disasterEvent);

        events.Add(new DisasterEventOccurredEvent(
            state.EventIds.Issue(), context.Date, settlementId, eventId, hazardType, severity, triggeredByCompounding));
    }

    private int ApplyBuildingDamage(
        WorldState state, MonthlyTickContext context, RuntimeId<Settlement> settlementId,
        HazardType hazardType, DisasterSeverity severity, bool perennialCropSetback)
    {
        var stepsLost = perennialCropSetback
            ? DisasterDamageCalculator.FrostBuildingConditionStepsLost(severity)
            : DisasterDamageCalculator.BuildingConditionStepsLost(severity);

        var eligiblePlotIds = state.Plots.InAscendingOrder()
            .Where(entry => entry.Value.SettlementId == settlementId && IsEligiblePlot(hazardType, entry.Value))
            .Select(entry => entry.Key)
            .ToHashSet();
        if (eligiblePlotIds.Count == 0)
            return 0;

        var hitThreshold = (uint)Math.Clamp(DisasterDamageCalculator.BuildingHitProbability(severity) * RollPrecision, 0, RollPrecision);
        var damaged = 0;

        foreach (var buildingEntry in state.Buildings.InAscendingOrder().ToArray())
        {
            var building = buildingEntry.Value;
            if (!eligiblePlotIds.Contains(building.PlotId) || building.Condition == BuildingCondition.Ruined)
                continue;

            var hitRoll = context.RandomStreams.NextUInt(_streamName, RollPrecision);
            if (hitRoll >= hitThreshold)
                continue;

            building.ApplyDisasterDamage(stepsLost);
            damaged++;
        }

        return damaged;
    }

    /// <summary>§2's own per-hazard terrain gating, read against the real <see cref="Plot"/> this
    /// building sits on: Flood only strikes River-adjacent plots, Landslide only strikes Hills plots,
    /// every other structural hazard (Fire's urban density, Earthquake's regional shake, Storm's
    /// coastal/inland reach, a Severe+ Frost's own perennial-crop setback) is not terrain-restricted at
    /// this level — <see cref="HazardExposureProfile"/> already read the terrain composition into the
    /// Exposure score itself for those.</summary>
    private static bool IsEligiblePlot(HazardType hazardType, Plot plot) => hazardType switch
    {
        HazardType.Flood => plot.Terrain == TerrainType.River || plot.Features.HasFlag(TerrainFeature.RiverAdjacent),
        HazardType.Landslide => plot.Terrain == TerrainType.Hills,
        _ => true,
    };

    private static int ApplyPopulationLoss(WorldState state, RuntimeId<Settlement> settlementId, DisasterSeverity severity)
    {
        var fraction = DisasterDamageCalculator.CatastrophicPopulationLossFraction(severity);
        if (fraction <= 0.0)
            return 0;

        var totalLost = 0;
        foreach (var entry in state.PopGroups.InAscendingOrder().ToArray())
        {
            if (entry.Key.SettlementId != settlementId)
                continue;

            var group = entry.Value;
            var lost = (int)Math.Round(group.Size * fraction, MidpointRounding.AwayFromZero);
            if (lost <= 0)
                continue;

            var newSize = Math.Max(0, group.Size - lost);
            var actualLost = group.Size - newSize;
            if (actualLost <= 0)
                continue;

            state.PopGroups.Remove(entry.Key);
            state.PopGroups.Add(entry.Key, group with
            {
                Size = newSize,
                LegalStatusDistribution = group.LegalStatusDistribution.Shrink(actualLost),
            });
            totalLost += actualLost;
        }

        return totalLost;
    }

    private static void ApplyContentmentImpact(WorldState state, RuntimeId<Settlement> settlementId, DisasterSeverity severity)
    {
        var impact = DisasterDamageCalculator.ContentmentImpact(severity);

        foreach (var entry in state.PopGroups.InAscendingOrder().ToArray())
        {
            if (entry.Key.SettlementId != settlementId)
                continue;

            var group = entry.Value;
            var newContentment = group.Contentment + impact;
            if (newContentment < Fixed64.Zero)
                newContentment = Fixed64.Zero;

            state.PopGroups.Remove(entry.Key);
            state.PopGroups.Add(entry.Key, group with { Contentment = newContentment });
        }
    }
}

/// <summary>Emitted whenever <see cref="NaturalDisasterSystem"/> fires a <see cref="DisasterEvent"/>,
/// ordinary or §3.1-chained.</summary>
public sealed record DisasterEventOccurredEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Settlement> SettlementId,
    RuntimeId<DisasterEvent> DisasterEventId,
    HazardType HazardType,
    DisasterSeverity Severity,
    bool TriggeredByCompounding) : IDomainEvent
{
    public string Type => "hazards.disasterEventOccurred";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { SettlementId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}

/// <summary>Emitted whenever <see cref="NaturalDisasterSystem"/> finds a settlement's live Exposure for
/// one hazard <see cref="HazardWarningCalculator.IsElevated"/> in a month no Event actually fired for
/// that hazard — a real, felt "the signs point this way" warning, distinct from the Event that would
/// follow if the risk actually lands.</summary>
public sealed record HazardElevatedExposureWarningEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Settlement> SettlementId,
    HazardType HazardType,
    int ExposureScore) : IDomainEvent
{
    public string Type => "hazards.elevatedExposureWarning";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { SettlementId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}
