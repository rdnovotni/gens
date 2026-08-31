using Gens.Simulation.Time;

namespace Gens.Simulation.Regions;

/// <summary>One dated override window on a <see cref="DatedRule{TValue}"/>: <paramref name="Value"/>
/// applies from <paramref name="EffectiveFrom"/> (inclusive, <c>null</c> means "since the dawn of the
/// campaign range") until <paramref name="EffectiveUntil"/> (exclusive, <c>null</c> means "still in
/// effect at the end of the range"). Modeled directly on §6's tapering case — Iberian Colony's
/// Cantabrian Wars closing 29-19 BC changing how "live" Reputation Duality still reads depending on
/// campaign start year — but generic enough for any other date-gated §4 field.</summary>
public sealed record DatedOverride<TValue>
{
    public DatedOverride(TValue value, GameDate? effectiveFrom = null, GameDate? effectiveUntil = null)
    {
        if (effectiveFrom is not null && effectiveUntil is not null &&
            effectiveFrom.Value.TotalMonths >= effectiveUntil.Value.TotalMonths)
        {
            throw new ArgumentException(
                "An override's EffectiveFrom must precede its EffectiveUntil.", nameof(effectiveFrom));
        }

        Value = value;
        EffectiveFrom = effectiveFrom;
        EffectiveUntil = effectiveUntil;
    }

    public TValue Value { get; }
    public GameDate? EffectiveFrom { get; }
    public GameDate? EffectiveUntil { get; }

    public bool Covers(GameDate date) =>
        (EffectiveFrom is null || date.TotalMonths >= EffectiveFrom.Value.TotalMonths) &&
        (EffectiveUntil is null || date.TotalMonths < EffectiveUntil.Value.TotalMonths);
}

/// <summary>
/// The general date-aware rule-override mechanism Phase 13 item 1 asks for: a base value plus a set of
/// non-overlapping <see cref="DatedOverride{TValue}"/> windows, resolving to "the effective value as of
/// a given <see cref="GameDate"/>" (<see cref="EffectiveAsOf"/>). Deliberately independent of any one
/// region field's type — <see cref="RegionProfileDefinition.ReputationDuality"/> is this pass's one
/// concrete use (expressing §6's tapering shape), but any other date-varying region field can reuse the
/// same wrapper rather than each inventing its own if/else date check.
/// </summary>
public sealed record DatedRule<TValue>
{
    public DatedRule(TValue baseValue, IReadOnlyList<DatedOverride<TValue>>? overrides = null)
    {
        BaseValue = baseValue;
        Overrides = overrides ?? Array.Empty<DatedOverride<TValue>>();

        var ordered = Overrides
            .Select(o => (o.EffectiveFrom?.TotalMonths ?? long.MinValue, o.EffectiveUntil?.TotalMonths ?? long.MaxValue))
            .OrderBy(window => window.Item1)
            .ToArray();
        for (var i = 1; i < ordered.Length; i++)
        {
            if (ordered[i].Item1 < ordered[i - 1].Item2)
                throw new ArgumentException("A DatedRule's override windows must not overlap.", nameof(overrides));
        }

        Overrides = Overrides.OrderBy(o => o.EffectiveFrom?.TotalMonths ?? long.MinValue).ToArray();
    }

    public TValue BaseValue { get; }
    public IReadOnlyList<DatedOverride<TValue>> Overrides { get; }

    /// <summary>The effective value as of <paramref name="date"/>: the (unique, since windows never
    /// overlap) covering override's value, or <see cref="BaseValue"/> when no override covers the
    /// date.</summary>
    public TValue EffectiveAsOf(GameDate date)
    {
        foreach (var candidate in Overrides)
        {
            if (candidate.Covers(date))
                return candidate.Value;
        }

        return BaseValue;
    }
}
