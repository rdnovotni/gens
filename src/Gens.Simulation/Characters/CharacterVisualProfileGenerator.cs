using Gens.Simulation.Random;

namespace Gens.Simulation.Characters;

/// <summary>Deterministically rolls a fresh <see cref="CharacterVisualProfile"/> from a named <see
/// cref="RandomStreamSet"/> stream (roadmap Phase 5 item 2). Every attribute is drawn from an
/// explicit, hand-listed value order rather than <c>Enum.GetValues</c> — ADR 0004's "never rely on...
/// reflection discovery order" applies just as much to an enum's runtime value order as to any other
/// collection.</summary>
public static class CharacterVisualProfileGenerator
{
    /// <summary>Independent per-feature roll chance: low enough that most Characters carry none, high
    /// enough that a household of several people plausibly has at least one marked member.</summary>
    private const uint NotableFeatureChancePercent = 12;

    private static readonly Height[] HeightValues = { Height.Diminutive, Height.Average, Height.Tall };

    private static readonly Build[] BuildValues =
        { Build.Slight, Build.Average, Build.Muscular, Build.Heavyset };

    private static readonly FacialStructure[] FacialStructureValues =
    {
        FacialStructure.Angular, FacialStructure.Round, FacialStructure.Square,
        FacialStructure.Oval, FacialStructure.Gaunt,
    };

    private static readonly Complexion[] ComplexionValues =
        { Complexion.Fair, Complexion.Olive, Complexion.Bronzed, Complexion.Dark };

    private static readonly HairColor[] HairColorValues =
    {
        HairColor.Black, HairColor.Brown, HairColor.Auburn,
        HairColor.Blond, HairColor.Gray, HairColor.White,
    };

    private static readonly HairStyle[] HairStyleValues =
        { HairStyle.Cropped, HairStyle.ShoulderLength, HairStyle.Flowing, HairStyle.BoundUp };

    private static readonly EyeColor[] EyeColorValues =
        { EyeColor.Brown, EyeColor.Hazel, EyeColor.Green, EyeColor.Blue, EyeColor.Gray };

    private static readonly NotableFeature[] NotableFeatureValues =
    {
        NotableFeature.Scar, NotableFeature.BrokenNose, NotableFeature.Freckled,
        NotableFeature.Birthmark, NotableFeature.GrayAtTemples,
    };

    public static CharacterVisualProfile Generate(RandomStreamSet streams, string streamName)
    {
        if (streams is null)
            throw new ArgumentNullException(nameof(streams));

        var height = Pick(streams, streamName, HeightValues);
        var build = Pick(streams, streamName, BuildValues);
        var facialStructure = Pick(streams, streamName, FacialStructureValues);
        var complexion = Pick(streams, streamName, ComplexionValues);
        var hairColor = Pick(streams, streamName, HairColorValues);
        var hairStyle = Pick(streams, streamName, HairStyleValues);
        var eyeColor = Pick(streams, streamName, EyeColorValues);

        var notableFeatures = new List<NotableFeature>();
        foreach (var feature in NotableFeatureValues)
            if (streams.NextUInt(streamName, 100) < NotableFeatureChancePercent)
                notableFeatures.Add(feature);

        var portrait = PortraitRecipeGenerator.Generate(
            height, build, facialStructure, complexion, hairColor, hairStyle, eyeColor, notableFeatures);

        return new CharacterVisualProfile
        {
            Height = height,
            Build = build,
            FacialStructure = facialStructure,
            Complexion = complexion,
            HairColor = hairColor,
            HairStyle = hairStyle,
            EyeColor = eyeColor,
            NotableFeatures = notableFeatures,
            Portrait = portrait,
        };
    }

    private static T Pick<T>(RandomStreamSet streams, string streamName, IReadOnlyList<T> values) =>
        values[(int)streams.NextUInt(streamName, (uint)values.Count)];
}
