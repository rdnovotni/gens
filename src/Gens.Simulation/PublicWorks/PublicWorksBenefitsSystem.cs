using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.NotableBusinesses;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.PublicWorks;

/// <summary>
/// §3's Aqueduct/Sewer real Disease &amp; Public Health integration (Phase 15 item 9) — read by <see
/// cref="Health.EndemicIllnessSystem"/> as one further multiplicative factor stacked on top of <see
/// cref="Health.SanitationInvestmentCalculator.ExposureMultiplier"/>, the same "gains one further real
/// input, computed by a later item, without needing to know about that item's own types" shape <see
/// cref="Hazards.HazardExposureProfile.Compute"/> already established for Phase 15 item 7's own
/// irrigated-fraction reduction. Independent per work type (both apply, multiplicatively, when a
/// settlement has an operational Aqueduct <b>and</b> Sewer) — §3's own "a further real... improvement,
/// distinct from an aqueduct's own clean-water contribution."
/// </summary>
public static class PublicWorksHealthQuery
{
    public static Numerics.Fixed64 SanitationMultiplier(WorldState state, RuntimeId<Settlement> settlementId)
    {
        var multiplier = Numerics.Fixed64.One;
        foreach (var entry in state.PublicWorks.InAscendingOrder())
        {
            var work = entry.Value;
            if (work.SettlementId != settlementId || !PublicWorkResolver.IsOperational(work))
                continue;

            if (work.WorkType == PublicWorkType.Aqueduct)
                multiplier = Numerics.Fixed64.Multiply(multiplier, PublicWorksCatalog.AqueductSanitationMultiplier);
            else if (work.WorkType == PublicWorkType.Sewer)
                multiplier = Numerics.Fixed64.Multiply(multiplier, PublicWorksCatalog.SewerSanitationMultiplier);
        }

        return multiplier;
    }
}

/// <summary>
/// §3's Sewer real Settlement Demographics Contentment integration — read by <see
/// cref="Characters.ContentmentSystem"/> as one further additive term in <see
/// cref="Characters.ContentmentCalculator.ComputeContentment(Numerics.Fixed64,Numerics.Fixed64,
/// Numerics.Fixed64,Numerics.Fixed64,Numerics.Fixed64)"/>, matching that calculator's own established
/// "gains one new optional parameter" precedent (Phase 15 item 1's own <c>rentBurden</c> addition).
/// Settlement-wide per <see cref="PublicWorksCatalog.SewerContentmentBonus"/>'s own doc comment — <see
/// cref="Characters.PopGroupKey"/> carries no District attribution for a narrower reading.
/// </summary>
public static class PublicWorksContentmentQuery
{
    public static Numerics.Fixed64 CivicInfrastructureBonus(WorldState state, RuntimeId<Settlement> settlementId)
    {
        foreach (var entry in state.PublicWorks.InAscendingOrder())
        {
            var work = entry.Value;
            if (work.SettlementId == settlementId && work.WorkType == PublicWorkType.Sewer && PublicWorkResolver.IsOperational(work))
                return PublicWorksCatalog.SewerContentmentBonus;
        }

        return Numerics.Fixed64.Zero;
    }
}

/// <summary>
/// §3's Road, Harbor, and Marketplace/Basilica real monthly benefits (Phase 15 item 9) — a static,
/// unwired <c>Tick(state, date)</c> helper matching every other Phase 15 item's identical "no central
/// <c>IMonthlySystem</c> pipeline registry exists" convention. §3's Aqueduct/Sewer Health integration and
/// Sewer Contentment integration are instead read live by <see cref="Health.EndemicIllnessSystem"/> and
/// <see cref="Characters.ContentmentSystem"/> themselves (<see cref="PublicWorksHealthQuery"/>/<see
/// cref="PublicWorksContentmentQuery"/>), and §3's Bridge District-value bump is a one-time effect applied
/// directly by <see cref="FundPublicWorkCommands"/> at construction — this system covers the three
/// remaining, genuinely recurring monthly effects.
/// </summary>
public static class PublicWorksBenefitsSystem
{
    public static IReadOnlyList<IDomainEvent> Tick(WorldState state, GameDate date)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var entry in state.PublicWorks.InAscendingOrder())
        {
            var work = entry.Value;
            if (!PublicWorkResolver.IsOperational(work))
                continue;

            switch (work.WorkType)
            {
                case PublicWorkType.Road:
                    PostTreasuryBonus(state, date, work.SettlementId, PublicWorksCatalog.RoadTreasuryMonthlyBonus, "road", events);
                    break;
                case PublicWorkType.Harbor:
                    PostTreasuryBonus(state, date, work.SettlementId, PublicWorksCatalog.HarborTreasuryMonthlyBonus, "harbor", events);
                    break;
                case PublicWorkType.MarketplaceOrBasilica when work.DistrictId is { } districtId:
                    ApplyMarketplaceBonus(state, date, districtId, events);
                    break;
            }
        }

        return events;
    }

    private static void PostTreasuryBonus(
        WorldState state, GameDate date, RuntimeId<Settlement> settlementId, Money bonus, string reference, List<IDomainEvent> events)
    {
        events.Add(LedgerService.Post(
            state, date, LedgerTransactionCategory.Sales,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.Mint, -bonus),
                new LedgerPosting(LedgerAccountKey.ForSettlementTreasury(settlementId), bonus),
            },
            reference: $"publicWorks.{reference}:{settlementId.ToTaggedString()}"));
    }

    /// <summary>§3's Marketplace/Basilica — a real monthly income credit to every <see
    /// cref="NotableBusiness"/> in the linked District whose owner resolves to a real household, per <see
    /// cref="PublicWorksCatalog.MarketplaceBusinessMonthlyBonus"/>'s own doc comment. Only a <see
    /// cref="RealEstate.PropertyOwnerKind.PlayerHousehold"/> owner is actually paid — the same honest
    /// "only some owner kinds resolve against a real, checkable balance" narrowing every other Phase 15
    /// item already applies.</summary>
    private static void ApplyMarketplaceBonus(
        WorldState state, GameDate date, RuntimeId<RealEstate.District> districtId, List<IDomainEvent> events)
    {
        foreach (var entry in state.NotableBusinesses.InAscendingOrder())
        {
            var business = entry.Value;
            if (business.DistrictId != districtId || business.Owner.Kind != RealEstate.PropertyOwnerKind.PlayerHousehold)
                continue;

            var householdId = RuntimeId<Household>.Parse(business.Owner.OwnerId!);
            events.Add(LedgerService.Post(
                state, date, LedgerTransactionCategory.Sales,
                new[]
                {
                    new LedgerPosting(LedgerAccountKey.Mint, -PublicWorksCatalog.MarketplaceBusinessMonthlyBonus),
                    new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), PublicWorksCatalog.MarketplaceBusinessMonthlyBonus),
                },
                reference: $"publicWorks.marketplace:{entry.Key.ToTaggedString()}"));
        }
    }
}
