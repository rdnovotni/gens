using System.Numerics;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Numerics;

namespace Gens.Simulation.Economy;

/// <summary>
/// Pure math and shared constants for Phase 8 items 5-6 (household purchasing/sales, wages, rents,
/// taxes, and debt interest), factored out of the systems that use them the same way <see
/// cref="Markets.MarketClearingCalculator"/> sits beside <see cref="Markets.MarketClearingSystem"/>.
/// Every numeric constant here is this implementation's own invented baseline — no figure for these
/// specific rates exists in <c>gens-economy-finance-design.md</c>, which explicitly leaves "all numeric
/// sizing" (§13) unsized — documented as invented, pending playtesting, in the same spirit as <see
/// cref="Characters.EmploymentMatchingSystem"/>'s wage table and <see
/// cref="Markets.MarketPriceBoundConfig"/>'s bound constants.
/// </summary>
public static class HouseholdEconomyCalculator
{
    /// <summary>Flat monthly wage paid per staffed <see cref="Buildings.BuildingInstance"/> slot
    /// worker (Phase 8 item 6's Wages, §4.1) — a single flat rate rather than <see
    /// cref="Characters.EmploymentMatchingSystem"/>'s per-group table, since named building staff are
    /// not one of that table's background pop-group types.</summary>
    public static readonly Money WagePerWorkerPerMonth = Money.FromDenarii(6);

    /// <summary>Flat monthly rent charged per unit of a Holding's <see cref="Holding.ResidentCapacity"/>
    /// (Phase 8 item 6's Rents, §3.1) — this implementation's own invented stand-in for §3.1's
    /// unsized "share of harvest"/"per-occupant charge" formulas, using capacity (the only per-holding
    /// occupancy figure that already exists) rather than inventing new occupancy state.</summary>
    public static readonly Money RentPerResidentCapacity = Money.FromDenarii(2);

    /// <summary>The Decuma-style direct tax rate (§5.2), levied on top of <see
    /// cref="RentPerResidentCapacity"/> rather than as an independent figure — "a real, felt double-take
    /// from the same population," per §5.2's own framing. Applied without the Curia/magistracy political
    /// gate §5.2 describes (Politics &amp; Patronage does not exist yet); the settlement itself is this
    /// implementation's simplified stand-in tax authority.</summary>
    public static readonly Fixed64 DecumaTaxRate = Fixed64.FromRaw(100_000); // 0.10

    /// <summary>Monthly interest rate a <see cref="DebtRecord"/> starts at (§6.1) — interest-only,
    /// non-amortizing: the monthly obligation is <c>Principal * InterestRate</c>, and Principal itself
    /// only changes on an asset-seizure resolution, never on an ordinary payment. A simplification of
    /// §6's fuller model, appropriate for a first debt slice.</summary>
    public static readonly Fixed64 InitialDebtInterestRate = Fixed64.FromRaw(50_000); // 0.05/month

    /// <summary>How much a missed payment inflates the rate by (§6.3 rung 1, "interest escalation") —
    /// multiplicative, applied once per missed month.</summary>
    public static readonly Fixed64 DebtInterestEscalationFactor = Fixed64.FromRaw(1_200_000); // x1.2

    /// <summary>Consecutive missed payments before a <see cref="DebtRecord"/> moves from
    /// <see cref="DebtStatus.Overdue"/> to <see cref="DebtStatus.Defaulted"/> (§6.3 rung 2-3, collapsed:
    /// this implementation skips the Legal &amp; Court case §6.3 rung 2 calls for — that system does not
    /// exist yet — and goes directly from sustained default to rung 3's asset seizure, documented as a
    /// deliberate scope reduction, not an oversight).</summary>
    public const int DebtDefaultThresholdMonths = 3;

    /// <summary>The fraction of a defaulted debtor's own Holding stockpile (by quantity, of whichever
    /// good has a known settlement market price) seized and liquidated toward the debt at asset-seizure
    /// resolution (§6.3 rung 3). Debt bondage (§6.3 rung 4) is explicitly out of scope — see
    /// <see cref="DebtSystem"/>'s own doc comment for why: it requires Legal &amp; Court and Legal Status
    /// modeling this codebase doesn't build until a later phase.</summary>
    public static readonly Fixed64 DebtAssetSeizureFraction = Fixed64.FromRaw(500_000); // 0.5

    /// <summary>The Net Worth floor below which a household starts accumulating months toward
    /// Insolvency (§9: "substantially negative," left unsized by §13) — this implementation's own
    /// invented threshold.</summary>
    public static readonly Money InsolvencyNetWorthFloor = Money.FromDenarii(-1_000);

    /// <summary>Consecutive months below <see cref="InsolvencyNetWorthFloor"/> before each <see
    /// cref="InsolvencyStage"/> is reached (§9, unsized by §13). Indexed by stage: AtRisk, Insolvent,
    /// Ruined (Solvent has no threshold — it is simply "0 months below").</summary>
    public const int InsolvencyAtRiskMonths = 3;
    public const int InsolvencyInsolventMonths = 6;
    public const int InsolvencyRuinedMonths = 12;

    /// <summary>The fraction of a household's available stock (of whichever good has a known market
    /// price) forcibly liquidated each month it remains Insolvent or Ruined (§9 rung 1).</summary>
    public static readonly Fixed64 ForcedLiquidationFraction = Fixed64.FromRaw(250_000); // 0.25

    /// <summary>Forced liquidation sells "at a worse-than-Market rate" (§9 rung 1) — this fraction of
    /// the cleared market price.</summary>
    public static readonly Fixed64 ForcedLiquidationPriceFactor = Fixed64.FromRaw(500_000); // 0.5

    /// <summary>Net Worth thresholds (this implementation's own invented figures) mapping a computed
    /// <see cref="NetWorth.Total"/> onto <see cref="HouseholdWealthBand"/> — the "net-worth bands" the
    /// roadmap's Phase 8 item 6 names, kept distinct from <see cref="Characters.WealthBand"/> (that
    /// enum classifies a background PopGroup's position in the population-wide wealth pyramid,
    /// <c>gens-population-wealth-purchasing-power-design.md</c> §2 — a different scale and a different
    /// subject: one household's own Net Worth, not a population cohort's).</summary>
    public static HouseholdWealthBand BandFor(Money netWorthTotal) => netWorthTotal.RawValue switch
    {
        < 0 => HouseholdWealthBand.Ruined,
        < 100_00 => HouseholdWealthBand.Modest, // < 100 denarii
        < 1_000_00 => HouseholdWealthBand.Comfortable, // < 1,000 denarii
        _ => HouseholdWealthBand.Wealthy,
    };

    /// <summary>Resolves the <see cref="Household"/> that owns <paramref name="holding"/>, per <see
    /// cref="Holding.OwnerId"/>'s tagged-string convention (<see cref="LedgerAccountKey"/>'s own doc
    /// comment notes this same "an owner is a tagged string" pattern). Returns false for an
    /// unowned holding or one owned by something other than a household (a civic body, a Character
    /// directly, etc.) — Phase 8 items 5-8 only reach a holding whose owner parses as a household.</summary>
    public static bool TryGetOwningHousehold(Holding holding, out RuntimeId<Household> householdId)
    {
        if (holding is null)
            throw new ArgumentNullException(nameof(holding));

        if (holding.OwnerId is not null)
        {
            try
            {
                householdId = RuntimeId<Household>.Parse(holding.OwnerId);
                return true;
            }
            catch (FormatException)
            {
                // Owned by something other than a household — not this phase's concern.
            }
        }

        householdId = default;
        return false;
    }

    /// <summary>This household's monthly purchase need for <see
    /// cref="NeedsConsumptionCalculator.ConsumptionGood"/> (Phase 8 item 5), reusing that calculator's
    /// per-capita table with <see cref="Holding.ResidentCapacity"/> standing in for population size —
    /// the only per-holding occupancy figure that already exists, matching this class's own
    /// <see cref="RentPerResidentCapacity"/> substitution.</summary>
    public static long ComputeHouseholdNeed(int residentCapacity) =>
        NeedsConsumptionCalculator.ComputeDemand(DietTier.Adequate, residentCapacity);

    /// <summary>Saturating <c>floor(quantity * fraction)</c> — used to size a partial-stock action
    /// (asset seizure, forced liquidation) as a fraction of an available whole-unit quantity.</summary>
    public static long ApplyFraction(long quantity, Fixed64 fraction)
    {
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Quantity cannot be negative.");
        if (fraction < Fixed64.Zero)
            throw new ArgumentOutOfRangeException(nameof(fraction), fraction, "Fraction cannot be negative.");

        var product = (BigInteger)quantity * fraction.RawValue / Fixed64.ScaleFactor;
        if (product > long.MaxValue)
            return long.MaxValue;
        return (long)product;
    }

    /// <summary>Saturating <c>unitPrice * quantity</c> — the Money-times-long counterpart to <see
    /// cref="Money.Scale(Fixed64)"/> needed because a trade's total price is a whole-unit count times a
    /// per-unit <see cref="Money"/> price, not a <see cref="Fixed64"/> ratio.</summary>
    public static Money MultiplyByQuantity(Money unitPrice, long quantity)
    {
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Quantity cannot be negative.");

        var product = (BigInteger)unitPrice.RawValue * quantity;
        if (product > long.MaxValue)
            return Money.MaxValue;
        if (product < long.MinValue)
            return Money.MinValue;
        return Money.FromMinorUnits((long)product);
    }
}

/// <summary>One household's Net Worth wealth classification (Phase 8 item 6's "net-worth bands") — see
/// <see cref="HouseholdEconomyCalculator.BandFor"/> for why this is a distinct type from <see
/// cref="Characters.WealthBand"/>.</summary>
public enum HouseholdWealthBand
{
    Ruined,
    Modest,
    Comfortable,
    Wealthy,
}
