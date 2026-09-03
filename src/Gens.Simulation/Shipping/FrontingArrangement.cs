using Gens.Simulation.Identity;
using Gens.Simulation.RealEstate;
using Gens.Simulation.State;

namespace Gens.Simulation.Shipping;

/// <summary>
/// §5's Fronting mechanic and §11's <c>FrontingArrangement</c> data model (Phase 15 item 8) — the *lex
/// Claudia de nave senatorum*'s real, if light, mechanical teeth, recapped from Land Ownership &amp;
/// Real Estate §7's own <c>PropertyOwnerKind.Societas</c>/fronting motivation (that record's own doc
/// comment already named the *lex Claudia* as "given real, if light, mechanical teeth via §5's Fronting
/// mechanic" — this item is that mechanic). <see cref="FrontingPersonOrSocietasId"/> reuses <see
/// cref="RealEstate.PropertyOwnerRef"/> directly rather than inventing a parallel tagged-owner type,
/// restricted to the two kinds §5's own text actually names ("a freedman Operator or a Societas the
/// senator quietly controls") — <see cref="PropertyOwnerRef.ForIndividualCharacter"/> for a freedman,
/// <see cref="PropertyOwnerRef.ForSocietasPlaceholder"/> for a Societas the household does not want its
/// own name attached to (deliberately not <see cref="MerchantShip.OwningSocietasId"/>'s own real,
/// resolvable Societas link — a Fronting arrangement's whole point is that the registered owner is
/// <i>not</i> traceable back to the real household the same way an ordinary Societas co-ownership is).
///
/// <b>Exposure is honestly not wired.</b> §5's own text names three plausible discovery paths — "an
/// Espionage discovery, a Legal &amp; Court proceeding, an unrelated Scandal pulling on the same
/// thread" — and §12's own Open Questions leaves "Fronting exposure's actual trigger conditions...
/// without specifying relative likelihood or a formal detection roll." <see cref="Exposed"/> and <see
/// cref="ExposureScandalRef"/> are real, queryable fields with no live system in this item ever setting
/// them, matching <see
/// cref="PrivateInfrastructure.PrivateInfrastructureCatalog"/>'s own <c>PairedWithFortifyPosture</c>/
/// <c>RustlingRiskReduction</c> "the primitive ships, the caller doesn't exist yet" precedent — a real
/// Scandal source (<c>Scandal.ScandalSourceType</c>) for the exposure consequence itself is not added by
/// this item either, since no live trigger would ever produce it.
/// </summary>
public sealed record FrontingArrangement
{
    private FrontingArrangement()
    {
    }

    public required RuntimeId<MerchantShip> ShipId { get; init; }
    public required RuntimeId<Household> RealOwnerHouseholdId { get; init; }
    public required PropertyOwnerRef FrontingPersonOrSocietasId { get; init; }
    public required bool Exposed { get; init; }
    public string? ExposureScandalRef { get; init; }

    public static FrontingArrangement Create(
        RuntimeId<MerchantShip> shipId, RuntimeId<Household> realOwnerHouseholdId, PropertyOwnerRef frontingPersonOrSocietasId) => new()
        {
            ShipId = shipId,
            RealOwnerHouseholdId = realOwnerHouseholdId,
            FrontingPersonOrSocietasId = frontingPersonOrSocietasId,
            Exposed = false,
            ExposureScandalRef = null,
        };

    public static FrontingArrangement Restore(
        RuntimeId<MerchantShip> shipId, RuntimeId<Household> realOwnerHouseholdId, PropertyOwnerRef frontingPersonOrSocietasId,
        bool exposed, string? exposureScandalRef) => new()
        {
            ShipId = shipId,
            RealOwnerHouseholdId = realOwnerHouseholdId,
            FrontingPersonOrSocietasId = frontingPersonOrSocietasId,
            Exposed = exposed,
            ExposureScandalRef = exposureScandalRef,
        };
}

/// <summary>Read/write helpers over <see cref="WorldState.ShipFrontingArrangements"/>, matching <see
/// cref="MerchantShipResolver"/>'s identical "remove then re-add" convention.</summary>
public static class FrontingArrangementResolver
{
    public static bool TryGetCurrent(WorldState state, RuntimeId<MerchantShip> shipId, out FrontingArrangement arrangement)
    {
        if (state.ShipFrontingArrangements.TryGet(shipId, out var entry))
        {
            arrangement = entry!;
            return true;
        }

        arrangement = null!;
        return false;
    }

    public static void Set(WorldState state, FrontingArrangement arrangement)
    {
        if (state.ShipFrontingArrangements.TryGet(arrangement.ShipId, out _))
            state.ShipFrontingArrangements.Remove(arrangement.ShipId);
        state.ShipFrontingArrangements.Add(arrangement.ShipId, arrangement);
    }
}
