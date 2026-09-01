using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;

namespace Gens.Simulation.RealEstate;

/// <summary>
/// §3's "a management status flag... on any Plot the player owns" (Phase 15 item 1) — a new, sparse
/// <see cref="WorldState"/> partition keyed by <see cref="RuntimeId{Plot}"/>, matching <see
/// cref="Fame.CharacterFame"/>'s and <see cref="Land.DistantHolding"/>'s identical "wrap the existing
/// record in a parallel partition rather than editing its schema" convention — <see cref="Plot"/>
/// itself is Estate &amp; Settlement's (Phase 6) own record, and this item does not touch that file.
/// Present only once a Plot has actually been drawn into this item's leasing/District machinery; an
/// absent entry means the Plot's every-Plot default: <see
/// cref="PropertyManagementStatus.DirectlyManaged"/>, no Operator, no District (see <see
/// cref="PlotPropertyResolver.Current"/>).
/// </summary>
public sealed record PlotPropertyExtension
{
    private PlotPropertyExtension()
    {
    }

    public required RuntimeId<Plot> PlotId { get; init; }
    public RuntimeId<District>? DistrictId { get; init; }
    public required PropertyManagementStatus ManagementStatus { get; init; }
    public RuntimeId<Character>? OperatorCharacterId { get; init; }
    public bool OperatorIsSkimming { get; init; }

    /// <summary>§6.1's buyout precondition "has never skimmed" — unlike <see
    /// cref="OperatorIsSkimming"/> (this month's reading only, overwritten every tick so an audit
    /// always reads current truth), this stays true for the rest of the current Operator's assignment
    /// once any month sets it, so a later recovered Loyalty reading cannot quietly requalify an
    /// Operator who has, in fact, skimmed before. Reset to <c>false</c> only when the Operator
    /// changes, alongside every other per-assignment field.</summary>
    public bool OperatorHasEverSkimmed { get; init; }
    public int OperatorTenureMonths { get; init; }
    public bool OperatorBuyoutOffered { get; init; }

    /// <summary>§5's <c>ager publicus</c> lease for a Plot whose <see cref="Plot.OwnerId"/> resolves
    /// to <see cref="PropertyOwnerKind.RomanState"/> — mirrors <see cref="PropertyRecord.LesseeId"/>'s
    /// identical shape.</summary>
    public RuntimeId<Household>? LesseeId { get; init; }

    /// <summary>§9's tracked Value — Estate &amp; Settlement's own <see cref="Plot"/> carries no money
    /// figure at all, so this item tracks it here rather than editing that record. Starts at <see
    /// cref="Money.Zero"/> until <see cref="DistrictPropertyValueSystem"/> (or a real acquisition/sale
    /// price) first prices the Plot.</summary>
    public required Money Value { get; init; }

    public static PlotPropertyExtension Default(RuntimeId<Plot> plotId) => new()
    {
        PlotId = plotId,
        DistrictId = null,
        ManagementStatus = PropertyManagementStatus.DirectlyManaged,
        OperatorCharacterId = null,
        OperatorIsSkimming = false,
        OperatorHasEverSkimmed = false,
        OperatorTenureMonths = 0,
        OperatorBuyoutOffered = false,
        LesseeId = null,
        Value = Money.Zero,
    };

    /// <summary>Reconstructs a <see cref="PlotPropertyExtension"/> from persisted save data (ADR 0010).</summary>
    public static PlotPropertyExtension Restore(
        RuntimeId<Plot> plotId,
        RuntimeId<District>? districtId,
        PropertyManagementStatus managementStatus,
        RuntimeId<Character>? operatorCharacterId,
        bool operatorIsSkimming,
        bool operatorHasEverSkimmed,
        int operatorTenureMonths,
        bool operatorBuyoutOffered,
        RuntimeId<Household>? lesseeId,
        Money value) => new()
        {
            PlotId = plotId,
            DistrictId = districtId,
            ManagementStatus = managementStatus,
            OperatorCharacterId = operatorCharacterId,
            OperatorIsSkimming = operatorIsSkimming,
            OperatorHasEverSkimmed = operatorHasEverSkimmed,
            OperatorTenureMonths = operatorTenureMonths,
            OperatorBuyoutOffered = operatorBuyoutOffered,
            LesseeId = lesseeId,
            Value = value,
        };
}

/// <summary>Resolves a Plot's <see cref="PlotPropertyExtension"/>, defaulting an untouched Plot to
/// <see cref="PlotPropertyExtension.Default"/> — matching <see cref="Fame.FameResolver"/>'s identical
/// "no entry means the default" convention.</summary>
public static class PlotPropertyResolver
{
    public static PlotPropertyExtension Current(WorldState state, RuntimeId<Plot> plotId) =>
        state.PlotPropertyExtensions.TryGet(plotId, out var entry) ? entry! : PlotPropertyExtension.Default(plotId);

    /// <summary>Replaces (remove then re-add) the Plot's extension record, matching every other sparse
    /// partition's identical convention.</summary>
    public static void Set(WorldState state, PlotPropertyExtension extension)
    {
        if (state.PlotPropertyExtensions.TryGet(extension.PlotId, out _))
            state.PlotPropertyExtensions.Remove(extension.PlotId);
        state.PlotPropertyExtensions.Add(extension.PlotId, extension);
    }
}
