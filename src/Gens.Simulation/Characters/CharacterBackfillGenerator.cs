using Gens.Simulation.Random;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>
/// Rolls the "age-appropriate backfill" <c>gens-characters-design.md</c> §11 describes for a
/// lazily-instantiated adult: a plausible apparent age plus a Core Attributes/Labor Skills spread,
/// standing in for "a rough life history compressed into an instant" rather than the zero-everything
/// start a household-born infant gets from <see cref="BirthCharacterCommand"/>. No numeric formula for
/// either exists in the design corpus (the same gap <see cref="CharacterConditionGenerator"/>'s own doc
/// comment flags for baseline Condition), so the bands below are this implementation's own choice.
/// Draw order is fixed — age first, then Core Attributes (Diplomacy, Martial, Stewardship, Intrigue,
/// Learning), then Labor Skills (Fieldwork, DomesticService, Craft, Culinary, Medicine), then Condition
/// (Health, Fatigue, Loyalty, Ambition, Fertility) — matching each record's own declared field order —
/// so a later addition here never perturbs an earlier draw.
/// </summary>
public static class CharacterBackfillGenerator
{
    private const int MinAdultAgeYears = 18;
    private const int MaxAdultAgeYears = 65;
    private const int StatBandMin = 10;
    private const int StatBandMax = 60;
    private const int HealthMin = 60;
    private const int HealthMax = 95;
    private const int FatigueMin = 0;
    private const int FatigueMax = 40;
    private const int MidBandMin = 30;
    private const int MidBandMax = 70;

    /// <summary>Rolls a plausible adult apparent age and converts it to a birth date as of <paramref
    /// name="asOf"/> — the "moment of first contact" §11 names as when a lazily-instantiated Character
    /// is generated.</summary>
    public static GameDate RollAdultBirthDate(RandomStreamSet streams, string streamName, GameDate asOf)
    {
        if (streams is null)
            throw new ArgumentNullException(nameof(streams));

        var ageYears = RollInclusive(streams, streamName, MinAdultAgeYears, MaxAdultAgeYears);
        return new GameDate(asOf.TotalMonths - ageYears * 12);
    }

    /// <summary>Rolls a mid-band Core Attributes/Labor Skills spread standing in for an unseen life
    /// history — nothing in this implementation treats the result as less "real" than attributes
    /// accumulated in real play (§11's own framing).</summary>
    public static (CoreAttributes Attributes, LaborSkills Skills) RollAttributesAndSkills(RandomStreamSet streams, string streamName)
    {
        if (streams is null)
            throw new ArgumentNullException(nameof(streams));

        var attributes = new CoreAttributes(
            RollInclusive(streams, streamName, StatBandMin, StatBandMax),
            RollInclusive(streams, streamName, StatBandMin, StatBandMax),
            RollInclusive(streams, streamName, StatBandMin, StatBandMax),
            RollInclusive(streams, streamName, StatBandMin, StatBandMax),
            RollInclusive(streams, streamName, StatBandMin, StatBandMax));

        var skills = new LaborSkills(
            RollInclusive(streams, streamName, StatBandMin, StatBandMax),
            RollInclusive(streams, streamName, StatBandMin, StatBandMax),
            RollInclusive(streams, streamName, StatBandMin, StatBandMax),
            RollInclusive(streams, streamName, StatBandMin, StatBandMax),
            RollInclusive(streams, streamName, StatBandMin, StatBandMax));

        return (attributes, skills);
    }

    /// <summary>Rolls a lived-in-adulthood <see cref="Condition"/> — lower Health headroom and a
    /// nonzero Fatigue band, unlike <see cref="CharacterConditionGenerator"/>'s newborn bands (near-full
    /// Health, zero Fatigue), since a backfilled adult has had decades to accumulate wear a newborn
    /// hasn't. Loyalty, Ambition, and Fertility reuse the same mid-range band <see
    /// cref="CharacterConditionGenerator"/> already established for those three disposition stats.</summary>
    public static Condition RollCondition(RandomStreamSet streams, string streamName)
    {
        if (streams is null)
            throw new ArgumentNullException(nameof(streams));

        var health = RollInclusive(streams, streamName, HealthMin, HealthMax);
        var fatigue = RollInclusive(streams, streamName, FatigueMin, FatigueMax);
        var loyalty = RollInclusive(streams, streamName, MidBandMin, MidBandMax);
        var ambition = RollInclusive(streams, streamName, MidBandMin, MidBandMax);
        var fertility = RollInclusive(streams, streamName, MidBandMin, MidBandMax);

        return new Condition(health, fatigue, loyalty, ambition, fertility);
    }

    private static int RollInclusive(RandomStreamSet streams, string streamName, int min, int max) =>
        min + (int)streams.NextUInt(streamName, (uint)(max - min + 1));
}
