namespace Gens.Art;

public sealed class CharacterPortraitPromptCompiler : IArtPromptCompiler<CharacterPortraitSubject>
{
    public const string CompilerVersion = "portrait-prompt-v1";
    public string Version => CompilerVersion;

    public CompiledArtPrompt Compile(CharacterPortraitSubject subject, ArtStyleId style, ArtGenerationProfile profile)
    {
        ArgumentNullException.ThrowIfNull(subject);
        ArgumentNullException.ThrowIfNull(profile);
        var negative = new List<string> { "modern clothing", "modern objects", "text", "watermarks", "extra limbs", "duplicate faces", "graphic violence", "sexualized pose" };
        if (subject.IsMinor || profile.SafetyProfile == ArtSafetyProfile.MinorPortrait)
            negative.AddRange(["adult styling", "revealing clothing"]);
        return new(
            subject.Appearance.PortraitDescription + ".",
            $"Historical setting: {subject.HistoricalContext}.",
            "Use only the clothing and status already stated in the structured subject description.",
            "Head-and-shoulders portrait, neutral expression, natural proportions, plain muted background.",
            "Soft natural light with restrained contrast.",
            StyleText(style),
            ["anatomically coherent", "single subject", "historically plausible", "age appropriate"],
            negative);
    }

    private static string StyleText(ArtStyleId style) => style.Value switch
    {
        "painted-realism" => "Restrained painted realism.",
        "mosaic" => "Historically inspired Roman mosaic illustration.",
        _ => $"Visual style: {style.Value}.",
    };
}
