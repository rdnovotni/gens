namespace Gens.Simulation.Health;

/// <summary>Pure, RNG-free monthly probability math for §3's Epidemic layer: how a new outbreak
/// ignites, and how an existing one spreads — extending <see
/// cref="EndemicExposureCalculator"/>'s own "every figure is this implementation's own invented number"
/// disclosure to the contagion side of the design corpus's identical §12 "All numeric sizing" open
/// question ("contagion spread rates"). Every probability here is scaled by the caller's own <see
/// cref="SanitationInvestmentCalculator.ExposureMultiplier"/> and <see
/// cref="QuarantineEffectCalculator"/> multipliers, matching §6's "a genuine multiplier on top of"
/// framing literally — this file's own base figures assume no Sanitation Investment and no
/// Quarantine.</summary>
public static class EpidemicSpreadCalculator
{
    private const double MaxMonthlyProbability = 0.5;

    /// <summary>The monthly probability a settlement with no currently <see
    /// cref="EpidemicOutbreakStatus.Active"/> outbreak of a given epidemic disease sparks a new one —
    /// standing in for §3's real-world introduction vectors (trade, travelers, returning soldiers) this
    /// codebase has no concrete Travel-arrival hook wired to yet (disclosed, not faked). Deliberately
    /// rare: an epidemic is meant to read as a genuine, occasional event, not a monthly certainty.</summary>
    public static double MonthlyIgnitionProbability(double sanitationMultiplier) =>
        Clamp(0.006 * Math.Max(0.0, sanitationMultiplier));

    /// <summary>§9's own Antonine Plague framing: Pestilence ignition "elevat[ed]... everywhere
    /// regardless of individual household preparation" for the real duration of <see
    /// cref="AntoninePlagueEra.IsActive"/>. Applied only to Pestilence's own ignition roll (<see
    /// cref="EpidemicContagionSystem"/>), on top of — not instead of — <paramref
    /// name="sanitationMultiplier"/>, since §9 itself only claims the pandemic overrides "individual
    /// household preparation," not settlement-level Sanitation Investment. This implementation's own
    /// invented multiplier, chosen only so a Pestilence outbreak reads as a genuinely different order of
    /// likelihood during the historical era than an ordinary local ignition roll.</summary>
    public static double AntoninePlagueIgnitionProbability(double sanitationMultiplier) =>
        Clamp(8.0 * 0.006 * Math.Max(0.0, sanitationMultiplier));

    /// <summary>§3.1's person-to-person vector (Pestilence, Pox, Camp Fever): the probability one
    /// susceptible Character catches the disease this month from <paramref
    /// name="infectedHouseholdMembers"/> already-infected Household co-members — the "real Contact"
    /// this item actually builds (household co-membership; see <see
    /// cref="EpidemicContagionSystem"/>'s own doc comment for why Group Interactions/Travel are not
    /// wired in as a contact graph here). Each infected co-member independently contributes, but the
    /// combined probability never exceeds <see cref="MaxMonthlyProbability"/>.</summary>
    public static double HouseholdContactSpreadProbability(
        int infectedHouseholdMembers, double sanitationMultiplier, double sourceSpreadMultiplier, double settlementSpreadMultiplier)
    {
        if (infectedHouseholdMembers <= 0)
            return 0.0;

        const double perSourceProbability = 0.18;
        var perSource = perSourceProbability * Math.Max(0.0, sanitationMultiplier) *
            Math.Max(0.0, sourceSpreadMultiplier) * Math.Max(0.0, settlementSpreadMultiplier);
        perSource = Math.Clamp(perSource, 0.0, 1.0);

        // Independent per-source misses compound: 1 - (1-p)^n, the standard "at least one of n
        // independent chances" combination, clamped the same way MaxMonthlyProbability caps every
        // other probability in this file.
        var probabilityOfNoInfection = Math.Pow(1.0 - perSource, infectedHouseholdMembers);
        return Clamp(1.0 - probabilityOfNoInfection);
    }

    /// <summary>§3.2's waterborne vector (Enteric Fever only): not contact-driven at all — every
    /// susceptible Character at a settlement with an active outbreak shares the same settlement-wide
    /// water-supply risk, scaled by how many active cases already exist there (a rough severity proxy)
    /// rather than by any specific contact.</summary>
    public static double WaterborneSpreadProbability(
        int activeCasesInSettlement, double sanitationMultiplier, double settlementSpreadMultiplier)
    {
        if (activeCasesInSettlement <= 0)
            return 0.0;

        var severityFactor = Math.Min(1.0, activeCasesInSettlement / 10.0);
        return Clamp(0.05 * severityFactor * Math.Max(0.0, sanitationMultiplier) * Math.Max(0.0, settlementSpreadMultiplier));
    }

    private static double Clamp(double probability) => Math.Clamp(probability, 0.0, MaxMonthlyProbability);
}
