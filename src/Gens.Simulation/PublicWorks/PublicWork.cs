using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.RealEstate;
using Gens.Simulation.Societates;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.PublicWorks;

/// <summary>§3's six real public-works categories (Phase 15 item 9;
/// <c>gens-public-works-euergetism-design.md</c> §3), every one carrying its own distinct mechanical
/// effect rather than a uniform Dignitas-and-nothing-else payoff — see <see
/// cref="PublicWorksBenefitsSystem"/>'s own doc comment for exactly which real, live effect each value
/// actually drives.</summary>
public enum PublicWorkType
{
    Aqueduct,
    Road,
    Bridge,
    Sewer,
    MarketplaceOrBasilica,
    Harbor,
}

/// <summary>§7's two real, distinct funding sources — <see cref="PrivateEuergetism"/> (a single wealthy
/// patron or a Societas, §2's real obligation, carrying real Dignitas and inscription credit) versus
/// <see cref="StateTaxRevenue"/> (the settlement's own Treasury, no individual patron's name and no
/// personal Dignitas payoff).</summary>
public enum PublicWorkFundingSource
{
    PrivateEuergetism,
    StateTaxRevenue,
}

/// <summary>
/// §9's <c>PublicWork</c> data model (Phase 15 item 9) — one funded, built civic work. Condition is
/// carried directly on the record (0-100, matching <see cref="Land.LandCondition"/>'s own scale exactly
/// like <see cref="Shipping.MerchantShip.Condition"/>'s identical reasoning: a Public Work, like a Ship,
/// is already a single owned record with nowhere else its own condition could live) rather than a
/// separate keyed partition the way <see cref="PrivateInfrastructure.InfrastructureCondition"/> tracks
/// several different structure shapes at once — this domain has exactly one structure shape.
/// </summary>
public sealed record PublicWork
{
    private PublicWork()
    {
    }

    public required RuntimeId<PublicWork> Id { get; init; }
    public required RuntimeId<Settlement> SettlementId { get; init; }

    /// <summary>§3's Sewer ("the District it actually serves") and Marketplace/Basilica read this as a
    /// real gate on their own benefit; every other work type may still carry one (a Bridge unlocking a
    /// specific District's own newly-accessible land, §3), but it is not required for them.</summary>
    public RuntimeId<District>? DistrictId { get; init; }

    public required PublicWorkType WorkType { get; init; }
    public required PublicWorkFundingSource FundingSource { get; init; }

    /// <summary>§9's <c>fundingPatronHouseholdOrSocietasId</c>, one real named patron half of it — null
    /// whenever <see cref="FundingSource"/> is <see cref="PublicWorkFundingSource.StateTaxRevenue"/> (§7:
    /// "carrying no individual patron's name") or the work was instead jointly funded by a real Societas
    /// (<see cref="FundingSocietasId"/>). Restricted to <see cref="PropertyOwnerKind.PlayerHousehold"/> or
    /// <see cref="PropertyOwnerKind.RivalGens"/> — the only two <see cref="PropertyOwnerRef"/> kinds this
    /// codebase can actually resolve to a real, checkable household-like entity (matching <see
    /// cref="MerchantFamilies.EquestrianStatusQuery"/>'s and <see
    /// cref="NotableBusinesses.NotableBusiness.Owner"/>'s own identical narrowing) — <see
    /// cref="PropertyOwnerKind.Societas"/> is deliberately never used here, since that kind is always
    /// narrative-only (<see cref="PropertyOwnerRef.IsNarrativeOnly"/>'s own doc comment): a real joint
    /// Societas venture uses <see cref="FundingSocietasId"/> instead, the same real <see
    /// cref="Societates.Societas"/> record <see cref="Shipping.MerchantShip.OwningSocietasId"/> already
    /// ties into rather than that narrative placeholder.</summary>
    public PropertyOwnerRef? FundingPatronId { get; init; }

    /// <summary>§7's "a Societas of several" (Societates &amp; Business Partnerships §8's own Societas
    /// Unius Rei application) — set only for a work jointly funded by a real, active <see
    /// cref="Societates.Societas"/>, mutually exclusive with <see cref="FundingPatronId"/>. Both are null
    /// exactly when <see cref="FundingSource"/> is <see cref="PublicWorkFundingSource.StateTaxRevenue"/>.</summary>
    public RuntimeId<Societas>? FundingSocietasId { get; init; }

    /// <summary>§4's inscription practice — always true for <see
    /// cref="PublicWorkFundingSource.PrivateEuergetism"/> (the real, physical mechanism behind a
    /// household's public works becoming genuinely remembered), always false for <see
    /// cref="PublicWorkFundingSource.StateTaxRevenue"/> per §7's own "no individual patron's name."</summary>
    public required bool HasInscription { get; init; }

    /// <summary>§6/§8 — reads the same 0-100 scale <see cref="Land.LandCondition"/> and <see
    /// cref="PrivateInfrastructure.InfrastructureCondition"/> both already use.</summary>
    public required int Condition { get; init; }

    /// <summary>§6's neglect clock — how many consecutive months this work's upkeep has gone unpaid,
    /// reset to zero the moment a month's upkeep is actually paid (<see
    /// cref="PublicWorksMaintenanceSystem"/>) or a funded <see cref="FundPublicWorkUpkeepCommand"/>
    /// restores it. Drives §6's own "in a severe case of visible neglect, risks a real Scandal" once it
    /// crosses <see cref="PublicWorksCatalog.SevereNeglectConsecutiveMonths"/> alongside a low enough
    /// Condition — see <see cref="RecordEuergetismNeglectScandalCommand"/>.</summary>
    public required int ConsecutiveNeglectedMonths { get; init; }

    public required GameDate BuiltDate { get; init; }

    public static PublicWork Create(
        RuntimeId<PublicWork> id,
        RuntimeId<Settlement> settlementId,
        RuntimeId<District>? districtId,
        PublicWorkType workType,
        PublicWorkFundingSource fundingSource,
        PropertyOwnerRef? fundingPatronId,
        RuntimeId<Societas>? fundingSocietasId,
        GameDate builtDate) => new()
        {
            Id = id,
            SettlementId = settlementId,
            DistrictId = districtId,
            WorkType = workType,
            FundingSource = fundingSource,
            FundingPatronId = fundingPatronId,
            FundingSocietasId = fundingSocietasId,
            HasInscription = fundingSource == PublicWorkFundingSource.PrivateEuergetism,
            Condition = PublicWorksCatalog.PristineCondition,
            ConsecutiveNeglectedMonths = 0,
            BuiltDate = builtDate,
        };

    /// <summary>Reconstructs a <see cref="PublicWork"/> from persisted save data (ADR 0010).</summary>
    public static PublicWork Restore(
        RuntimeId<PublicWork> id,
        RuntimeId<Settlement> settlementId,
        RuntimeId<District>? districtId,
        PublicWorkType workType,
        PublicWorkFundingSource fundingSource,
        PropertyOwnerRef? fundingPatronId,
        RuntimeId<Societas>? fundingSocietasId,
        bool hasInscription,
        int condition,
        int consecutiveNeglectedMonths,
        GameDate builtDate) => new()
        {
            Id = id,
            SettlementId = settlementId,
            DistrictId = districtId,
            WorkType = workType,
            FundingSource = fundingSource,
            FundingPatronId = fundingPatronId,
            FundingSocietasId = fundingSocietasId,
            HasInscription = hasInscription,
            Condition = condition,
            ConsecutiveNeglectedMonths = consecutiveNeglectedMonths,
            BuiltDate = builtDate,
        };
}

/// <summary>Read/write helpers over <see cref="WorldState.PublicWorks"/>, matching <see
/// cref="Shipping.MerchantShipResolver"/>'s identical "remove then re-add" convention.</summary>
public static class PublicWorkResolver
{
    public static bool TryGetCurrent(WorldState state, RuntimeId<PublicWork> id, out PublicWork work)
    {
        if (state.PublicWorks.TryGet(id, out var entry))
        {
            work = entry!;
            return true;
        }

        work = null!;
        return false;
    }

    public static void Set(WorldState state, PublicWork work)
    {
        if (state.PublicWorks.TryGet(work.Id, out _))
            state.PublicWorks.Remove(work.Id);
        state.PublicWorks.Add(work.Id, work);
    }

    /// <summary>§8's "an aqueduct... reading directly against Buildings' own existing condition-and-decay
    /// mechanics" realized as a binary lapse below <see
    /// cref="PublicWorksCatalog.MinimumOperationalCondition"/>, matching <see
    /// cref="PrivateInfrastructure.InfrastructureConditionResolver.IsOperational"/>'s identical
    /// convention — every real benefit in <see cref="PublicWorksBenefitsSystem"/> gates on this rather
    /// than scaling continuously with Condition.</summary>
    public static bool IsOperational(PublicWork work) => work.Condition >= PublicWorksCatalog.MinimumOperationalCondition;
}
