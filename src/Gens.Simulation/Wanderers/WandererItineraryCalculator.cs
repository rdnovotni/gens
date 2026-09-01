using Gens.Simulation.Identity;
using Gens.Simulation.Regions;

namespace Gens.Simulation.Wanderers;

/// <summary>
/// Pure, RNG-free math for §3's Itinerary: how strongly a given <see cref="WandererType"/> is drawn to
/// a given <see cref="GazetteerLocationDefinition"/>, and — given a single already-drawn roll — which
/// destination that weighting actually selects. Extends <c>Characters.MortalityCalculator</c>'s and
/// <c>Health.HealthConditionProgressionCalculator</c>'s own "documented as invented, pending
/// playtesting" precedent: §11's own first open question names "itinerary movement frequency" as
/// unsized, so every constant below is this implementation's own invented figure, chosen only so that a
/// type's own preferred roles dominate an indifferent location, a Prominence-seeking type measurably
/// prefers a <see cref="ProminenceTier.ProvincialSeat"/> over an <see cref="ProminenceTier.Outpost"/>,
/// and no eligible location is ever weighted to zero — §3's "weighted by that Wanderer's own
/// type-specific logic rather than pure randomness" is a skew, not a rail.
///
/// <para>The single <c>uint</c> roll is passed in rather than drawn here, matching
/// <c>Hazards.DisasterSeverityCalculator</c>'s identical "the calculator stays pure; the System owns
/// the stream" split (rule 8).</para>
/// </summary>
public static class WandererItineraryCalculator
{
    /// <summary>The weight every eligible destination carries before any type-specific skew — the
    /// floor that keeps an indifferent location genuinely reachable.</summary>
    public const int BaseWeight = 1;

    /// <summary>Added once per <see cref="WandererTypeProfile.PreferredRoles"/> entry the destination
    /// actually carries — a place that is both this type's roles at once is proportionally more
    /// attractive, matching <see cref="GazetteerRole"/>'s own "a single entry can carry more than one
    /// role" framing.</summary>
    public const int PreferredRoleWeight = 4;

    /// <summary>Multiplied by the destination's own Prominence step (Outpost 0, RegionalCenter 1,
    /// ProvincialSeat 2) and added only for a <see cref="WandererTypeProfile.PrefersHighProminence"/>
    /// type — §3's "and other high-Prominence Gazetteer locations."</summary>
    public const int ProminenceStepWeight = 3;

    /// <summary>How many months a Wanderer dwells at one stop before <see cref="WandererSystem"/> moves
    /// them on. A season, deliberately: §7.1's own worked example says a touring rhetorician is
    /// "expected to move on within a season," the one concrete pacing figure the design document
    /// gives.</summary>
    public const int MonthsPerStop = 3;

    /// <summary>How many stops a Wanderer's <see cref="Wanderer.Itinerary"/> retains, oldest dropped
    /// first. Bounded so a long-lived Wanderer's record cannot grow without limit across a campaign —
    /// there is no design-document requirement for a complete lifetime travel history, and an unbounded
    /// list would inflate both the save file and the state hash forever.</summary>
    public const int MaxItineraryLength = 8;

    /// <summary>The Prominence step <see cref="ProminenceStepWeight"/> scales — a plain ordinal read of
    /// <see cref="ProminenceTier"/>'s own three-step declaration order.</summary>
    public static int ProminenceStep(ProminenceTier tier) => tier switch
    {
        ProminenceTier.Outpost => 0,
        ProminenceTier.RegionalCenter => 1,
        ProminenceTier.ProvincialSeat => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unknown prominence tier."),
    };

    /// <summary>How strongly <paramref name="profile"/>'s type is drawn to <paramref name="location"/>.
    /// Always at least <see cref="BaseWeight"/>.</summary>
    public static int MovementWeight(WandererTypeProfile profile, GazetteerLocationDefinition location)
    {
        if (profile is null)
            throw new ArgumentNullException(nameof(profile));
        if (location is null)
            throw new ArgumentNullException(nameof(location));

        var weight = BaseWeight;
        foreach (var role in profile.PreferredRoles)
        {
            if (location.Roles.Contains(role))
                weight += PreferredRoleWeight;
        }

        if (profile.PrefersHighProminence)
            weight += ProminenceStep(location.ProminenceTier) * ProminenceStepWeight;

        return weight;
    }

    /// <summary>Every destination <paramref name="currentLocationId"/> can move on to, with its weight,
    /// in the catalog's own deterministic ordering (region declaration order, then gazetteer declaration
    /// order — the order <see cref="RegionProfileCatalog.All"/> and <see
    /// cref="RegionProfileDefinition.Gazetteer"/> already guarantee). The current location is excluded:
    /// §3's Itinerary "advances" the current Location, so a stop is always a move.</summary>
    public static IReadOnlyList<(GazetteerLocationDefinition Location, int Weight)> WeightedDestinations(
        WandererTypeProfile profile,
        RegionProfileCatalog catalog,
        DefinitionId<GazetteerLocationDefinition> currentLocationId)
    {
        if (catalog is null)
            throw new ArgumentNullException(nameof(catalog));

        var destinations = new List<(GazetteerLocationDefinition, int)>();
        foreach (var region in catalog.All().OrderBy(static region => region.Id.Value, StringComparer.Ordinal))
        {
            foreach (var location in region.Gazetteer)
            {
                if (location.Id.Equals(currentLocationId))
                    continue;
                destinations.Add((location, MovementWeight(profile, location)));
            }
        }

        return destinations;
    }

    /// <summary>The destination <paramref name="roll"/> selects from <paramref name="catalog"/>, or
    /// null when the catalog offers nowhere else to go (a single-entry roster, or an empty one) — in
    /// which case <see cref="WandererSystem"/> simply leaves the Wanderer where they are rather than
    /// inventing a destination. <paramref name="roll"/> is interpreted modulo the total weight, so any
    /// draw range a caller's stream produces resolves deterministically.</summary>
    public static DefinitionId<GazetteerLocationDefinition>? SelectNextDestination(
        WandererTypeProfile profile,
        RegionProfileCatalog catalog,
        DefinitionId<GazetteerLocationDefinition> currentLocationId,
        uint roll)
    {
        var destinations = WeightedDestinations(profile, catalog, currentLocationId);
        if (destinations.Count == 0)
            return null;

        var total = destinations.Sum(static destination => destination.Weight);
        var target = (int)(roll % (uint)total);
        foreach (var (location, weight) in destinations)
        {
            target -= weight;
            if (target < 0)
                return location.Id;
        }

        // Unreachable: the cumulative weights sum to exactly `total` and `target` starts strictly below
        // it. Kept as an explicit fallback rather than a throw so a future weighting change can never
        // turn a rounding surprise into a mid-tick crash.
        return destinations[^1].Location.Id;
    }

    /// <summary>Whether a Wanderer standing at <paramref name="arrivalMonth"/> has dwelled long enough
    /// to move on as of <paramref name="currentMonth"/> (<see cref="MonthsPerStop"/>).</summary>
    public static bool IsDueToMove(int arrivalMonth, int currentMonth) =>
        currentMonth - arrivalMonth >= MonthsPerStop;

    /// <summary>Appends <paramref name="stop"/> to <paramref name="itinerary"/>, dropping the oldest
    /// stops so the result never exceeds <see cref="MaxItineraryLength"/>.</summary>
    public static IReadOnlyList<WandererItineraryStop> Append(
        IReadOnlyList<WandererItineraryStop> itinerary, WandererItineraryStop stop)
    {
        if (itinerary is null)
            throw new ArgumentNullException(nameof(itinerary));

        var appended = itinerary.Append(stop).ToArray();
        return appended.Length <= MaxItineraryLength
            ? appended
            : appended[^MaxItineraryLength..];
    }
}
