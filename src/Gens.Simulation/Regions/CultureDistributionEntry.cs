namespace Gens.Simulation.Regions;

/// <summary>One weighted row of a region's Population &amp; Culture Distribution table
/// (<c>gens-starting-regions-design.md</c> §4.7, §12: <c>cultureDistributionTable</c>). <see
/// cref="CultureRef"/> is a qualitative content tag — Cultures of the Known World (Phase 13 item 4,
/// not yet built) owns the real culture-definition catalog this eventually resolves against.</summary>
public sealed record CultureDistributionEntry
{
    public CultureDistributionEntry(string cultureRef, int weight, bool isOutlierResidual = false)
    {
        if (string.IsNullOrWhiteSpace(cultureRef))
            throw new ArgumentException("A culture distribution entry requires a non-empty culture reference.", nameof(cultureRef));
        if (weight <= 0)
            throw new ArgumentOutOfRangeException(nameof(weight), weight, "A culture distribution weight must be positive.");

        CultureRef = cultureRef;
        Weight = weight;
        IsOutlierResidual = isOutlierResidual;
    }

    public string CultureRef { get; }
    public int Weight { get; }

    /// <summary>§4.7's "short note on the region's realistic outlier range" — the one entry per region
    /// standing in for that residual rather than a dedicated free-floating field, so the table's
    /// weights always sum to the whole distribution.</summary>
    public bool IsOutlierResidual { get; }
}
