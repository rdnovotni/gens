using Gens.Simulation.Identity;
using Gens.Simulation.Land;

namespace Gens.Simulation.Characters;

/// <summary>
/// One settlement's <c>gens-settlement-demographics-design.md</c> §15 <c>HousingAndLandCapacity</c>
/// record, computed fresh each tick by <see cref="HousingCapacitySystem"/> (Roadmap Phase 7 item 4)
/// exactly like <see cref="BackgroundJobCapacityCalculator"/>'s capacity tables — never persisted in
/// its own right, since every input (Buildings, Plots, Settlements) is already authoritative state.
/// </summary>
/// <param name="SettlementId">The settlement this capacity applies to.</param>
/// <param name="UrbanCapacity">§7.1's Insulae/Domus housing stock, shared across Operarii, Opifices,
/// Negotiatores, Aeditui, and Curiales — the settlement's actual urban residents. The design doc's own
/// <c>{ insulae, domus }</c> breakdown collapses to this one total here: no content currently
/// distinguishes an Insula from a Domus (both would just carry <see
/// cref="Buildings.BuildingDefinition.ResidentialCapacity"/>), so there is nothing yet to skew the mix
/// toward Operarii vs. Negotiatores/Opifices/Curiales the way §7.1 describes — a simplification left
/// for whenever Insula/Domus content actually exists.</param>
/// <param name="RuralCapacity">§7.2's unclaimed-fertile-plot capacity, shared across Coloni and
/// Veterans — population that lives on the land it works rather than in a building.</param>
public sealed record HousingAndLandCapacity(RuntimeId<Settlement> SettlementId, int UrbanCapacity, int RuralCapacity);
