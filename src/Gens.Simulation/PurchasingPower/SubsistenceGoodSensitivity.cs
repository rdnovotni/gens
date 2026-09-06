using System.Linq;
#nullable enable
using System;
using Gens.Simulation.BusinessCompetition;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Markets;
using Gens.Simulation.NotableBusinesses;
using Gens.Simulation.Numerics;
using Gens.Simulation.Scandal;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.PurchasingPower;

/// <summary>
/// §4's/§9's <c>SubsistenceGoodSensitivity</c> data model, realized as a real, computed read-side query
/// rather than a further tracked partition — every field it names is either already a real, checkable
/// fact (<see cref="IsSubsistenceGood"/>, reusing <see cref="PurchasingPowerCalculator.GetGoodTier"/>) or
/// derivable on demand from <see cref="SettlementMarket"/>'s own already-persisted state (<see
/// cref="IsGenuineShortage"/>, <see cref="ContentmentPenalty"/>, <see cref="HoardingRiskMultiplier"/>),
/// matching <see cref="RealEstate.DistrictRentBurdenCalculator"/>'s own identical "a pure query over
/// already-tracked state, no new partition" shape.
/// </summary>
public static class SubsistenceGoodSensitivityQuery
{
    public static bool IsSubsistenceGood(DefinitionId<Good> goodId) =>
        PurchasingPowerCalculator.GetGoodTier(goodId) == WealthBand.Subsistence;

    /// <summary>§4's own real, checkable "genuine shortage" signal — reused directly from <see
    /// cref="BusinessCompetition.GrainHoardingResolutionSystem"/>'s own identical reasoning: <see
    /// cref="SettlementMarket.UnsatisfiedDemand"/> &gt; 0 for <see
    /// cref="NeedsConsumptionCalculator.ConsumptionGood"/>, the one real shortage indicator this codebase
    /// tracks (no separate Grain Dole/Cura Annonae shortage flag exists anywhere — confirmed unbuilt,
    /// matching that system's own identical finding).</summary>
    public static bool IsGenuineShortage(WorldState state, RuntimeId<Settlement> settlementId) =>
        state.MarketPrices.TryGet(new MarketGoodKey(settlementId, NeedsConsumptionCalculator.ConsumptionGood), out var market)
        && market!.UnsatisfiedDemand > 0;

    /// <summary>§4's "a direct Contentment crisis" — the real, subtracted Contentment term <see
    /// cref="Characters.ContentmentSystem"/> feeds into its new <see
    /// cref="Characters.ContentmentCalculator"/> overload for every <see
    /// cref="WealthBand.Subsistence"/>-tier <see cref="PopGroup"/> specifically (§4's own "the tier that
    /// depends on [subsistence goods] has no buffer at all" — a Modest Surplus or Elite Discretionary
    /// group never takes this penalty, since a grain-price spike is a real crisis only for the tier with
    /// no discretionary buffer to absorb it).</summary>
    public static Fixed64 ContentmentPenalty(WorldState state, RuntimeId<Settlement> settlementId, WealthBand groupWealthBand)
    {
        if (groupWealthBand != WealthBand.Subsistence)
            return Fixed64.Zero;

        return IsGenuineShortage(state, settlementId) ? PurchasingPowerCatalog.SubsistenceShortageContentmentPenalty : Fixed64.Zero;
    }

    /// <summary>§4's/§9's own "hoardingRiskMultiplier" — a real, computed reading with no autonomous
    /// caller yet; see <see cref="PurchasingPowerCatalog.ShortageHoardingRiskMultiplier"/>'s own doc
    /// comment for why.</summary>
    public static Fixed64 HoardingRiskMultiplier(WorldState state, RuntimeId<Settlement> settlementId) =>
        IsGenuineShortage(state, settlementId) ? PurchasingPowerCatalog.ShortageHoardingRiskMultiplier : Fixed64.One;
}

/// <summary>
/// §4's "a genuine Scandal risk (Scandal §4) if a Notable Business is seen profiting from it" — distinct
/// from Business Competition's own Grain Hoarding consequence chain (§5 of that document), which needs an
/// explicit <see cref="BusinessCompetition.DeclareGrainHoardingCommand"/> declaration first. This command
/// reveals a different, real ground truth reachable for <i>any</i> grain-trading business, hoarding or
/// not: a <see cref="NotableBusinessStatus.Tracked"/>, grain-trading (<see
/// cref="GrainHoardingResolver.IsGrainTrading"/>) business whose own <see
/// cref="NotableBusiness.Reputation"/> reads above <see
/// cref="NotableBusinessesCatalog.DefaultReputation"/> (visibly thriving, not merely surviving) while its
/// settlement's subsistence good genuinely IsGenuineShortage — "seen profiting" read literally as doing
/// well precisely when everyone else is not. Mirrors <see
/// cref="PublicWorks.RecordEuergetismNeglectScandalCommand"/>'s and <see
/// cref="RealEstate.AuditPropertyOperatorCommand"/>'s own identical "reveal, don't re-validate" shape,
/// and reuses <see cref="RecordBusinessScandalCommand"/> wholesale rather than adding a redundant <see
/// cref="ScandalSourceType"/> value — <see cref="ScandalSourceType.BusinessMisconduct"/>'s own doc
/// comment already names "price gouging" as one of the real conducts it covers.
/// </summary>
public sealed record RecordSubsistencePriceGougingScandalCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<NotableBusiness> BusinessId) : ICommand;

public static class RecordSubsistencePriceGougingScandalCommands
{
    public static readonly ValidationErrorCode BusinessNotFound = new("purchasingPower.priceGouging.businessNotFound");
    public static readonly ValidationErrorCode NotGrainTrading = new("purchasingPower.priceGouging.notGrainTrading");
    public static readonly ValidationErrorCode NoGenuineShortage = new("purchasingPower.priceGouging.noGenuineShortage");
    public static readonly ValidationErrorCode NotVisiblyThriving = new("purchasingPower.priceGouging.notVisiblyThriving");

    public static readonly CommandPipeline<WorldState, RecordSubsistencePriceGougingScandalCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, RecordSubsistencePriceGougingScandalCommand command)
    {
        if (!state.NotableBusinesses.TryGet(command.BusinessId, out var business) || business!.Status != NotableBusinessStatus.Tracked)
            return BusinessNotFound;
        if (!GrainHoardingResolver.IsGrainTrading(business))
            return NotGrainTrading;
        if (!TryResolveSettlement(state, business, out var settlementId))
            return BusinessNotFound;
        if (!SubsistenceGoodSensitivityQuery.IsGenuineShortage(state, settlementId))
            return NoGenuineShortage;
        if (business.Reputation <= NotableBusinessesCatalog.DefaultReputation)
            return NotVisiblyThriving;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, RecordSubsistencePriceGougingScandalCommand command) =>
        RecordBusinessScandalCommands.Pipeline.Execute(
            state, new RecordBusinessScandalCommand(
                state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                command.BusinessId, ScandalSeverity.PublicDisgrace)).Events.ToArray();

    private static bool TryResolveSettlement(WorldState state, NotableBusiness business, out RuntimeId<Settlement> settlementId)
    {
        if (business.DistrictId is { } districtId && state.Districts.TryGet(districtId, out var district))
        {
            settlementId = district!.SettlementId;
            return true;
        }

        settlementId = default;
        return false;
    }
}
