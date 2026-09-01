using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;

namespace Gens.Simulation.RealEstate;

/// <summary>A read-only snapshot of everything §5/§6/§9/§11's shared commands and systems need,
/// regardless of whether the underlying property is a <see cref="Plot"/> or a <see
/// cref="PropertyRecord"/> (<see cref="PropertyResolver"/>'s own doc comment).</summary>
public readonly record struct PropertyView(
    PropertySubjectRef Subject,
    PropertyOwnerRef Owner,
    RuntimeId<Settlement>? SettlementId,
    RuntimeId<District>? DistrictId,
    PropertyManagementStatus ManagementStatus,
    RuntimeId<Character>? OperatorCharacterId,
    bool OperatorIsSkimming,
    int OperatorTenureMonths,
    bool OperatorBuyoutOffered,
    RuntimeId<Household>? LesseeId,
    Money Value,
    LandCondition Condition);

/// <summary>
/// The one shared read/write surface over both property shapes this item supports (Phase 15 item 1;
/// <see cref="PropertySubjectRef"/>'s own doc comment) — every command and system in this namespace
/// reads and mutates a property through this resolver rather than switching on <see
/// cref="PropertySubjectKind"/> itself. A Plot's projection folds in its <see
/// cref="PlotPropertyExtension"/> (defaulting per <see cref="PlotPropertyResolver.Current"/> when the
/// Plot has never been touched by this item) and reads ownership off <see cref="Plot.OwnerId"/> via
/// <see cref="PropertyOwnerRef.Parse"/>; a <see cref="PropertyRecord"/> already carries every field
/// directly.
/// </summary>
public static class PropertyResolver
{
    public static bool TryResolve(WorldState state, PropertySubjectRef subject, out PropertyView view)
    {
        switch (subject.Kind)
        {
            case PropertySubjectKind.Plot:
                {
                    if (!state.Plots.TryGet(subject.AsPlotId(), out var plot) || plot.OwnerId is null)
                    {
                        view = default;
                        return false;
                    }

                    var extension = PlotPropertyResolver.Current(state, subject.AsPlotId());
                    view = new PropertyView(
                        subject, PropertyOwnerRef.Parse(plot.OwnerId), plot.SettlementId, extension.DistrictId,
                        extension.ManagementStatus, extension.OperatorCharacterId, extension.OperatorIsSkimming,
                        extension.OperatorTenureMonths, extension.OperatorBuyoutOffered, extension.LesseeId,
                        extension.Value, plot.Condition);
                    return true;
                }

            case PropertySubjectKind.PropertyRecord:
                {
                    if (!state.PropertyRecords.TryGet(subject.AsPropertyRecordId(), out var record))
                    {
                        view = default;
                        return false;
                    }

                    view = new PropertyView(
                        subject, record.Owner, record.SettlementId, record.DistrictId, record.ManagementStatus,
                        record.OperatorCharacterId, record.OperatorIsSkimming, record.OperatorTenureMonths,
                        record.OperatorBuyoutOffered, record.LesseeId, record.Value, record.Condition);
                    return true;
                }

            default:
                view = default;
                return false;
        }
    }

    /// <summary>§6's leasing flag itself: sets <see cref="PropertyManagementStatus"/> and the assigned
    /// Operator together, resetting tenure/skim/buyout state whenever the Operator actually changes
    /// (a fresh Operator has earned none of that history yet).</summary>
    public static void SetManagement(
        WorldState state, PropertySubjectRef subject, PropertyManagementStatus status, RuntimeId<Character>? operatorCharacterId)
    {
        TryResolve(state, subject, out var view);
        var operatorChanged = view.OperatorCharacterId != operatorCharacterId;

        Mutate(state, subject, view with
        {
            ManagementStatus = status,
            OperatorCharacterId = status == PropertyManagementStatus.LeasedOut ? operatorCharacterId : null,
            OperatorIsSkimming = operatorChanged ? false : view.OperatorIsSkimming,
            OperatorTenureMonths = operatorChanged ? 0 : view.OperatorTenureMonths,
            OperatorBuyoutOffered = operatorChanged ? false : view.OperatorBuyoutOffered,
        });
    }

    public static void SetOperatorState(
        WorldState state, PropertySubjectRef subject, bool isSkimming, int tenureMonths, bool buyoutOffered)
    {
        TryResolve(state, subject, out var view);
        Mutate(state, subject, view with
        {
            OperatorIsSkimming = isSkimming,
            OperatorTenureMonths = tenureMonths,
            OperatorBuyoutOffered = buyoutOffered,
        });
    }

    public static void SetOwner(WorldState state, PropertySubjectRef subject, PropertyOwnerRef owner, RuntimeId<Household>? lesseeId)
    {
        TryResolve(state, subject, out var view);
        Mutate(state, subject, view with { Owner = owner, LesseeId = lesseeId });
    }

    public static void SetValue(WorldState state, PropertySubjectRef subject, Money value)
    {
        TryResolve(state, subject, out var view);
        Mutate(state, subject, view with { Value = value });
    }

    public static void SetDistrict(WorldState state, PropertySubjectRef subject, RuntimeId<District>? districtId)
    {
        TryResolve(state, subject, out var view);
        Mutate(state, subject, view with { DistrictId = districtId });
    }

    private static void Mutate(WorldState state, PropertySubjectRef subject, PropertyView next)
    {
        switch (subject.Kind)
        {
            case PropertySubjectKind.Plot:
                {
                    var plotId = subject.AsPlotId();
                    state.Plots.TryGet(plotId, out var plot);
                    state.Plots.Remove(plotId);
                    state.Plots.Add(plotId, plot with { OwnerId = next.Owner.ToTaggedOwnerId() });

                    PlotPropertyResolver.Set(state, PlotPropertyExtension.Restore(
                        plotId, next.DistrictId, next.ManagementStatus, next.OperatorCharacterId, next.OperatorIsSkimming,
                        next.OperatorTenureMonths, next.OperatorBuyoutOffered, next.LesseeId, next.Value));
                    return;
                }

            case PropertySubjectKind.PropertyRecord:
                {
                    var recordId = subject.AsPropertyRecordId();
                    state.PropertyRecords.TryGet(recordId, out var record);
                    state.PropertyRecords.Remove(recordId);
                    state.PropertyRecords.Add(recordId, record with
                    {
                        Owner = next.Owner,
                        DistrictId = next.DistrictId,
                        ManagementStatus = next.ManagementStatus,
                        OperatorCharacterId = next.OperatorCharacterId,
                        OperatorIsSkimming = next.OperatorIsSkimming,
                        OperatorTenureMonths = next.OperatorTenureMonths,
                        OperatorBuyoutOffered = next.OperatorBuyoutOffered,
                        LesseeId = next.LesseeId,
                        Value = next.Value,
                    });
                    return;
                }
        }
    }
}
