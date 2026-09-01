namespace Gens.Simulation.Wanderers;

/// <summary>
/// Pure, RNG-free math for §4's Fame: how a successful engagement raises it, how sustained obscurity
/// erodes it, and how the resulting <see cref="WandererFameTrend"/> is read off the movement.
///
/// <para><b>Trend shape mirrors Rival Houses exactly, as §4 and §9 both instruct.</b>
/// <see cref="Trend"/> is a straight "compare this month's reading against last month's" derivation —
/// up is <see cref="WandererFameTrend.Rising"/>, down is <see cref="WandererFameTrend.Declining"/>,
/// unchanged is <see cref="WandererFameTrend.Established"/> — the same three-state trajectory
/// <c>Actors.LivingWorldActorDriftCatalog</c> encodes for a house ("Rising ⇒ up, Declining ⇒ down,
/// Established ⇒ no change") read in the opposite direction. Rival Houses <i>drifts the trend and lets
/// the fortune follow</i> because a Background house has no observable fortune to read; a Wanderer's
/// Fame is a real, observable number every month, so the trend is derived from it rather than rolled
/// independently — the same shape applied to the layer that actually has data. That derivation is
/// deliberately not a random walk: it never needs <c>Actors.BackgroundHouseDriftSystem</c>'s own
/// <c>StandingTrendDriftChancePercent</c> roll.</para>
///
/// <para><b>Every constant here is this implementation's own invented figure</b>, disclosed exactly the
/// way <c>Health.HealthConditionProgressionCalculator</c> and <c>Hazards.DisasterSeverityCalculator</c>
/// disclose theirs — §11's own first open question names "Fame growth/decay rates" as unsized. They are
/// chosen only so that a Wanderer left unengaged for long enough measurably fades (§4: "a Wanderer
/// whose Fame has quietly faded is easy to engage and easy to lose interest in"), a single engagement
/// buys back several months of that fade, and the fade is slow enough that a Wanderer does not vanish
/// from relevance within one season of inattention.</para>
/// </summary>
public static class WandererFameCalculator
{
    /// <summary>How many months of no engagement a Wanderer gets before obscurity starts eating their
    /// Fame — one dwell period plus a month, so a Wanderer who is engaged at every second stop never
    /// decays at all.</summary>
    public const int ObscurityGracePeriodMonths = 4;

    /// <summary>Fame lost per month once <see cref="ObscurityGracePeriodMonths"/> has elapsed. One
    /// point, matching <c>Fame.FameCatalog</c>'s own flat monthly Character-Fame decay shape rather
    /// than inventing a second, differently-shaped curve for the same 0-100 field (§4: "not a
    /// Wanderer-specific mechanic").</summary>
    public const int ObscurityDecayPerMonth = 1;

    /// <summary>The Fame at or above which §7's competition becomes real — "a sufficiently high-Fame
    /// Wanderer is a real, visible object of interest to more than just the player." Below it, <see
    /// cref="RegisterWandererInterestCommands"/> refuses to record a rival's interest at all: an obscure
    /// Wanderer is nobody's race. §11 names the Prominence threshold for §5's direct approach as
    /// unsized; this is the Fame-side counterpart and is equally this implementation's own figure.</summary>
    public const int CompetitionVisibilityThreshold = 40;

    /// <summary>The Fame band a freshly-instantiated Wanderer starts in, inclusive
    /// (<see cref="InstantiateWandererCommands"/> rolls within it). Deliberately straddles <see
    /// cref="CompetitionVisibilityThreshold"/>: some sampled Wanderers are immediately contested, most
    /// are not.</summary>
    public const int MinimumStartingFame = 20;

    /// <inheritdoc cref="MinimumStartingFame"/>
    public const int MaximumStartingFame = 60;

    /// <summary>Applies a signed Fame delta clamped to [0, 100] — the identical clamp
    /// <c>Fame.FameResolver.Apply</c> already enforces for the universal Character-level field, kept
    /// as a pure function here because a Wanderer has no <c>WorldState</c> Character entry to apply
    /// it to (§8).</summary>
    public static int ApplyDelta(int fame, int delta) => Math.Clamp(fame + delta, 0, 100);

    /// <summary>The Fame this month's obscurity costs a Wanderer who has gone <paramref
    /// name="monthsSinceLastEngagement"/> months without a successful engagement — zero inside the
    /// grace period.</summary>
    public static int MonthlyObscurityDecay(int monthsSinceLastEngagement)
    {
        if (monthsSinceLastEngagement < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(monthsSinceLastEngagement), monthsSinceLastEngagement, "Months since last engagement cannot be negative.");
        }

        return monthsSinceLastEngagement >= ObscurityGracePeriodMonths ? ObscurityDecayPerMonth : 0;
    }

    /// <summary>Reads the trend off an actual month-over-month Fame movement — see this type's own doc
    /// comment for why this is derived rather than drifted.</summary>
    public static WandererFameTrend Trend(int previousFame, int newFame)
    {
        if (newFame > previousFame)
            return WandererFameTrend.Rising;
        return newFame < previousFame ? WandererFameTrend.Declining : WandererFameTrend.Established;
    }

    /// <summary>Whether <paramref name="fame"/> makes this Wanderer a real object of §7 competition.</summary>
    public static bool IsCompetitionVisible(int fame) => fame >= CompetitionVisibilityThreshold;
}
