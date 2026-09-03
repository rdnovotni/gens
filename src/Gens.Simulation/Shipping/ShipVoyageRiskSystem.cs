using Gens.Simulation.Commands;
using Gens.Simulation.Economy;
using Gens.Simulation.Hazards;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Numerics;
using Gens.Simulation.Random;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Shipping;

/// <summary>
/// §6.2's discrete Voyage Event resolution and §8's Storm loss (Phase 15 item 8), matching <see
/// cref="PrivateInfrastructure.InfrastructureDisasterVulnerabilitySystem"/>'s own "read the real <see
/// cref="Hazards.DisasterEvent"/>s this same month already produced, roll a real per-asset hit against
/// them" shape — this system never re-rolls ignition or severity, only reuses <see
/// cref="Hazards.DisasterDamageCalculator.BuildingHitProbability"/> directly against every qualifying
/// Ship in the Storm's own settlement.
///
/// <b>Which Ships actually roll.</b> §6.2 names four real triggers for a discrete Voyage Event; this
/// item's own live system fires for exactly two of them — <see cref="VoyageTriggerReason.IsFlagship"/>
/// and <see cref="VoyageTriggerReason.FenusNauticumFinanced"/> — both real, directly checkable facts on
/// <see cref="MerchantShip"/> itself. §6.2's other two triggers, a named higher-risk luxury Trade Route
/// and a named one-off significant cargo, are honestly not wired: <see
/// cref="Economy.StandingContract"/> carries no route-risk-tier field and no per-shipment cargo concept
/// for this item to read (confirmed by direct search of that record's own schema) — both remain real,
/// listed <see cref="VoyageTriggerReason"/> values no live system in this item ever produces. A
/// qualifying Ship must also be <see cref="ShipStatus.Active"/> and have <see
/// cref="MerchantShip.AssignedTradeRouteId"/> set — an unassigned or already-lost Ship is never at sea
/// to begin with. Every other Ship (an ordinary Corbita on an ordinary route, with no fenus nauticum
/// riding on it) stays on §6.1's aggregate default and is never touched by this system at all — the
/// direct, literal reading of "no per-voyage roll... for routine trade."
///
/// <b>Piracy is honestly not wired.</b> §6's own "Damaged, Lost to Storm, Lost to Piracy, or Captured"
/// outcome set names Piracy alongside Storm — Piracy &amp; Banditry is Phase 16, confirmed unbuilt by
/// direct search (no raid/capture machinery of any kind exists anywhere in this codebase), so this
/// system only ever resolves the Storm half of §8; <see cref="VoyageOutcome.LostToPiracy"/>, <see
/// cref="VoyageOutcome.Captured"/>, and <see cref="VoyageOutcome.PresumedLost"/> are real, listed values
/// this system never produces, matching <see cref="ShipStatus"/>'s own identical narrowing.
///
/// <b>Fenus nauticum resolves for real.</b> §8's "a Ship lost while financed this way simply forgives
/// the associated debt" is a real, live mutation here — <see cref="Economy.DebtRecord.Status"/>'s own
/// <see cref="Economy.DebtStatus.Forgiven"/> case was, per that enum's own doc comment, "not reachable by
/// any system in this implementation... named so it has a real terminal state to reach once one does."
/// This is that system.
/// </summary>
public static class ShipVoyageRiskSystem
{
    public const string StreamName = "shipping.voyageRisk";

    public static IReadOnlyList<IDomainEvent> Tick(
        WorldState state, GameDate date, IReadOnlyList<DisasterEventOccurredEvent> disasterEventsThisMonth, RandomStreamSet randomStreams)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (disasterEventsThisMonth is null)
            throw new ArgumentNullException(nameof(disasterEventsThisMonth));
        if (randomStreams is null)
            throw new ArgumentNullException(nameof(randomStreams));

        var events = new List<IDomainEvent>();

        foreach (var disaster in disasterEventsThisMonth)
        {
            if (disaster.HazardType != HazardType.Storm)
                continue;

            var qualifyingShips = state.MerchantShips.InAscendingOrder()
                .Select(entry => entry.Value)
                .Where(ship => ship.Status == ShipStatus.Active
                    && ship.AssignedTradeRouteId is not null
                    && ship.HomeSettlementId == disaster.SettlementId
                    && (ship.IsFlagship || ship.FenusNauticumRecordId is not null))
                .ToArray();

            foreach (var ship in qualifyingShips)
                ResolveVoyage(state, date, disaster, ship, randomStreams, events);
        }

        return events;
    }

    private static void ResolveVoyage(
        WorldState state, GameDate date, DisasterEventOccurredEvent disaster, MerchantShip ship, RandomStreamSet randomStreams,
        List<IDomainEvent> events)
    {
        var triggerReason = ship.IsFlagship ? VoyageTriggerReason.IsFlagship : VoyageTriggerReason.FenusNauticumFinanced;

        var baseProbability = Fixed64.FromRaw((long)(DisasterDamageCalculator.BuildingHitProbability(disaster.Severity) * Fixed64.ScaleFactor));
        var probability = baseProbability;
        probability = Fixed64.Multiply(probability, ShippingCatalog.StormResistanceMultiplier(ship.VesselClass));
        if (ship.BlessedLaunch)
            probability = Fixed64.Multiply(probability, ShippingCatalog.BlessedLaunchRiskMultiplier);
        if (ship.ReputationTier == ShipReputationTier.LuckyShip)
            probability = Fixed64.Multiply(probability, ShippingCatalog.LuckyShipRiskMultiplier);
        if (ship.ReputationTier == ShipReputationTier.BadReputation)
            probability = Fixed64.Multiply(probability, ShippingCatalog.BadReputationRiskMultiplier);

        var hitThreshold = (uint)Math.Clamp(probability.RawValue, 0L, (long)NaturalDisasterSystem.RollPrecision);
        var hitRoll = randomStreams.NextUInt(StreamName, NaturalDisasterSystem.RollPrecision);
        var hit = hitRoll < hitThreshold;

        var outcome = !hit
            ? VoyageOutcome.ArrivedSafely
            : disaster.Severity >= DisasterSeverity.Severe ? VoyageOutcome.LostToStorm : VoyageOutcome.Damaged;

        var voyageEventId = state.VoyageEventIds.Issue();
        state.VoyageEvents.Add(voyageEventId, VoyageEvent.Create(voyageEventId, ship.Id, date, triggerReason, outcome));
        events.Add(new VoyageEventResolvedEvent(state.EventIds.Issue(), date, voyageEventId, ship.Id, triggerReason, outcome, ship.IsFlagship));

        switch (outcome)
        {
            case VoyageOutcome.ArrivedSafely:
                ApplySafeArrival(state, date, ship, events);
                break;
            case VoyageOutcome.Damaged:
                ApplyDamage(state, disaster, ship);
                break;
            case VoyageOutcome.LostToStorm:
                ApplyLoss(state, date, ship, voyageEventId, events);
                break;
        }
    }

    private static void ApplySafeArrival(WorldState state, GameDate date, MerchantShip ship, List<IDomainEvent> events)
    {
        var voyagesCompleted = ship.VoyagesCompleted + 1;
        var reputationTier = ship.ReputationTier;
        var dignitasAward = 0;

        if (reputationTier == ShipReputationTier.None && voyagesCompleted >= ShippingCatalog.LuckyShipVoyageThreshold)
        {
            reputationTier = ShipReputationTier.LuckyShip;
            dignitasAward = ShippingCatalog.LuckyShipDignitasAward;
        }

        MerchantShipResolver.Set(state, ship with
        {
            VoyagesCompleted = voyagesCompleted,
            ConsecutiveBadOutcomes = 0,
            ReputationTier = reputationTier,
        });

        if (dignitasAward != 0)
        {
            events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
                state, new AdjustDignitasCommand(
                    state.CommandIds.Issue(), "system", date, null, ship.ActualOwnerHouseholdId, dignitasAward,
                    "a ship earned a lucky reputation")).Events);
        }
    }

    private static void ApplyDamage(WorldState state, DisasterEventOccurredEvent disaster, MerchantShip ship)
    {
        var stepsLost = DisasterDamageCalculator.BuildingConditionStepsLost(disaster.Severity);
        var pointsLost = stepsLost * ShippingCatalog.ConditionPointsPerBuildingConditionStep;
        var newConditionValue = Math.Max(0, ship.Condition.Value - pointsLost);
        var consecutiveBadOutcomes = ship.ConsecutiveBadOutcomes + 1;
        var reputationTier = ship.ReputationTier == ShipReputationTier.None && consecutiveBadOutcomes >= ShippingCatalog.BadReputationVoyageThreshold
            ? ShipReputationTier.BadReputation
            : ship.ReputationTier;

        MerchantShipResolver.Set(state, ship with
        {
            Status = ShipStatus.Damaged,
            Condition = new LandCondition(newConditionValue),
            ConsecutiveBadOutcomes = consecutiveBadOutcomes,
            ReputationTier = reputationTier,
        });
    }

    private static void ApplyLoss(WorldState state, GameDate date, MerchantShip ship, RuntimeId<VoyageEvent> voyageEventId, List<IDomainEvent> events)
    {
        MerchantShipResolver.Set(state, ship with { Status = ShipStatus.LostToStorm, ConsecutiveBadOutcomes = 0 });

        if (ship.FenusNauticumRecordId is { } debtId && state.DebtRecords.TryGet(debtId, out var debt))
        {
            state.DebtRecords.Remove(debtId);
            state.DebtRecords.Add(debtId, debt! with { Status = DebtStatus.Forgiven });
        }

        var penalty = ship.IsFlagship ? ShippingCatalog.FlagshipLossDignitasPenalty : ShippingCatalog.OrdinaryLossDignitasPenalty;
        events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
            state, new AdjustDignitasCommand(
                state.CommandIds.Issue(), "system", date, voyageEventId.ToTaggedString(), ship.ActualOwnerHouseholdId, penalty,
                ship.IsFlagship ? "lost the household's own Flagship to a storm" : "lost a ship to a storm")).Events);
    }
}
