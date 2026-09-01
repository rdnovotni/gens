using Gens.Simulation.Identity;
using Gens.Simulation.Regions;

namespace Gens.Simulation.Wanderers;

/// <summary>One entry in a Wanderer's <see cref="Wanderer.Itinerary"/>
/// (<c>gens-wandering-populations-design.md</c> §3/§10's <c>{ locationId, arrivalMonth }</c>).
///
/// <para><b>Location is a Gazetteer entry, not a Settlement.</b> §3 allows either ("a settlement, or,
/// where relevant, a specific region document's own named Gazetteer entry"), and this item picks the
/// Gazetteer entry for a concrete, mechanical reason rather than a stylistic one: §3's whole point is
/// that movement is "weighted by that Wanderer's own type-specific logic rather than pure randomness,"
/// and <see cref="GazetteerLocationDefinition"/> is the only place in this codebase that actually
/// carries the <see cref="ProminenceTier"/> and <see cref="GazetteerRole"/> data that weighting reads
/// (see <see cref="WandererItineraryCalculator"/>). A runtime <see cref="Land.Settlement"/> carries
/// neither, and nothing yet links a <see cref="Land.Settlement"/> back to the content region profile or
/// Gazetteer entry it sits in — exactly the gap <see cref="Travel.TravelLocation"/>'s own doc comment
/// already discloses ("nothing yet links a runtime Settlement back to the content region profile it
/// sits in"). Settlement-anchored Wanderer locations are therefore deferred, not faked, and the
/// consequence is disclosed on <see cref="HostWandererCommands"/>: no engagement command can check
/// co-location between a household and a Wanderer this pass.</para></summary>
/// <param name="LocationId">The Gazetteer entry the Wanderer arrived at.</param>
/// <param name="ArrivalMonth">The <see cref="Time.GameDate.TotalMonths"/> of arrival — stored as the
/// raw month count rather than a <see cref="Time.GameDate"/> to match §10's own <c>arrivalMonth</c>
/// field name and shape.</param>
public readonly record struct WandererItineraryStop(
    DefinitionId<GazetteerLocationDefinition> LocationId,
    int ArrivalMonth);
