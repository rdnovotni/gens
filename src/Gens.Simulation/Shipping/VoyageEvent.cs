using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Shipping;

/// <summary>§6.2's real, live trigger set for a discrete Voyage Event (Phase 15 item 8). Every real
/// category §6.2 names is represented, matching <see cref="RealEstate.PropertyOwnerRef"/>'s and <see
/// cref="Legal.LegalCase.CaseType"/>'s own identical "every real category represented" precedent — see
/// <see cref="ShipVoyageRiskSystem"/>'s own doc comment for which of the four this item's own live
/// system actually fires (only <see cref="FenusNauticumFinanced"/> and <see cref="IsFlagship"/>; <see
/// cref="LuxuryRoute"/> and <see cref="NamedSignificantCargo"/> are real, reachable values no live
/// system in this item ever produces, since neither a route risk tier nor a one-off named-cargo concept
/// exists on <see cref="Economy.StandingContract"/> for this item to read).</summary>
public enum VoyageTriggerReason
{
    LuxuryRoute,
    FenusNauticumFinanced,
    NamedSignificantCargo,
    IsFlagship,
}

/// <summary>§6.2's/§11's <c>outcome</c> vocabulary, every real category represented. <see
/// cref="LostToPiracy"/>, <see cref="Captured"/>, and <see cref="PresumedLost"/> are real, reachable
/// values <see cref="ShipVoyageRiskSystem"/> never produces — see <see cref="ShipStatus"/>'s own doc
/// comment for the same honest narrowing applied to the Ship's own terminal status.</summary>
public enum VoyageOutcome
{
    ArrivedSafely,
    Damaged,
    LostToStorm,
    LostToPiracy,
    Captured,
    PresumedLost,
}

/// <summary>§11's <c>VoyageEvent</c> data model (Phase 15 item 8) — kept forever once resolved, matching
/// <see cref="Hazards.DisasterEvent"/>'s and <see cref="Health.EpidemicOutbreak"/>'s own identical
/// "resolved or not, kept for the campaign's lifetime" convention.</summary>
public sealed record VoyageEvent
{
    private VoyageEvent()
    {
    }

    public required RuntimeId<VoyageEvent> Id { get; init; }
    public required RuntimeId<MerchantShip> ShipId { get; init; }
    public required GameDate Month { get; init; }
    public required VoyageTriggerReason TriggerReason { get; init; }
    public required VoyageOutcome Outcome { get; init; }

    public static VoyageEvent Create(
        RuntimeId<VoyageEvent> id, RuntimeId<MerchantShip> shipId, GameDate month, VoyageTriggerReason triggerReason, VoyageOutcome outcome) =>
        new()
        {
            Id = id,
            ShipId = shipId,
            Month = month,
            TriggerReason = triggerReason,
            Outcome = outcome,
        };
}

/// <summary>Emitted whenever <see cref="ShipVoyageRiskSystem"/> resolves a discrete Voyage Event.
/// Public — matching <see cref="Hazards.DisasterEventOccurredEvent"/>'s own identical visibility for a
/// hazard-driven outcome everyone in the settlement would plausibly hear about.</summary>
public sealed record VoyageEventResolvedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<VoyageEvent> VoyageEventId,
    RuntimeId<MerchantShip> ShipId,
    VoyageTriggerReason TriggerReason,
    VoyageOutcome Outcome,
    bool WasFlagship) : IDomainEvent
{
    public string Type => "shipping.voyageEventResolved";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { ShipId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}
