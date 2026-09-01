namespace Gens.Simulation.RealEstate;

/// <summary>§6's "any developed property the household owns can be flagged Leased Out rather than
/// Directly Managed" (Phase 15 item 1). Applies uniformly to a Plot-linked property (via <see
/// cref="PropertyManagementState"/>) and a <see cref="PropertyRecord"/> alike.</summary>
public enum PropertyManagementStatus
{
    /// <summary>The default for every property until a player deliberately delegates it (§6) — the
    /// owner runs it directly, at full profit margin and full attention cost.</summary>
    DirectlyManaged,

    /// <summary>A named Operator Character runs the property day to day (§6); §11's Administrative
    /// Burden treats a Leased Out property as delegated rather than counting it against the owner's
    /// own oversight capacity.</summary>
    LeasedOut,
}

/// <summary>§5's four ways to acquire (or, read the other direction, §9's ways to sell) property that
/// already exists and is already owned by someone else — one shared vocabulary since both directions
/// are the same underlying ownership transfer (<see cref="TransferPropertyCommand"/>'s own doc
/// comment).</summary>
public enum PropertyTransferMethod
{
    /// <summary>§5's default case: any owner sells outright at a price scaled by District Property
    /// Value and the asset's own condition/income history (§9). Also §9's "sold directly to a specific,
    /// named party."</summary>
    VoluntarySale,

    /// <summary>§5's forced-sale case: an Insolvent household, a Legal &amp; Court judgment, or a
    /// confiscation puts a specific, named property up for acquisition. Pricing and the underlying
    /// legal/financial trigger are upstream concerns (<see cref="TransferPropertyCommand"/>'s own doc
    /// comment) — this method only marks that the transfer was not the seller's free choice.</summary>
    ForcedSale,

    /// <summary>§5's <c>ager publicus</c> case: the Roman state's own public land is never bought
    /// outright, only leased — this method does not change <see cref="PropertyRecord.Owner"/> (which
    /// stays <see cref="PropertyOwnerRef.RomanState"/>), it only sets <see
    /// cref="PropertyRecord.LesseeId"/> to the leasing household.</summary>
    AgerPublicusLease,

    /// <summary>§5's "persuading a Temple or Collegium" — a real Influence expenditure (Politics &amp;
    /// Patronage) moves an owner that "rarely sells outright" to part with a specific holding.</summary>
    Persuasion,

    /// <summary>§9's "sold back to the general market — an abstract buyer, resolving at current Value
    /// minus a standard friction." The buyer side of the transfer is the market itself, not a named
    /// <see cref="PropertyOwnerRef"/> — <see cref="TransferPropertyCommand.BuyerId"/> is ignored for
    /// this method (see that command's own doc comment).</summary>
    MarketSale,
}
