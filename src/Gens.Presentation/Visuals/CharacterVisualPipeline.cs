#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Gens.Simulation.Characters;

namespace Gens.Presentation.Visuals;

public enum VisualAgeBand { Infant, Child, Adolescent, YoungAdult, Adult, Mature, Elderly }
public enum VisualChangeKind { Unchanged, Minor, Major }
public enum PortraitSourceKind { Procedural, Generated, Custom, Fallback }

/// <summary>Player-visible projection of authoritative facts; hidden traits never enter this record.</summary>
public sealed record CharacterVisualState(
    int VisualProfileVersion,
    string CharacterId,
    ulong VisualSeed,
    Sex Sex,
    VisualAgeBand AgeBand,
    CharacterVisualProfile Profile,
    LegalStatus LegalStatus,
    SocialClass? SocialClass,
    DutySlot? Duty,
    bool IsAlive)
{
    public string VisualStateHash => VisualHash.Compute(this);
}

public static class CharacterVisualStateProjector
{
    public const int CurrentProfileVersion = 1;
    public static CharacterVisualState Project(string characterId, Sex sex, int age, CharacterVisualProfile profile, LegalStatus legalStatus, SocialClass? socialClass, DutySlot? duty, bool isAlive = true) =>
        new(CurrentProfileVersion, characterId, DeriveSeed(characterId), sex, AgeBand(age), profile, legalStatus, socialClass, duty, isAlive);

    public static ulong DeriveSeed(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId)) throw new ArgumentException("A character ID is required.", nameof(characterId));
        byte[] bytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes("gens.visual.seed.v1\n" + characterId));
        return BitConverter.ToUInt64(bytes, 0);
    }

    public static VisualAgeBand AgeBand(int age) => age switch
    {
        < 0 => throw new ArgumentOutOfRangeException(nameof(age)),
        <= 3 => VisualAgeBand.Infant,
        <= 12 => VisualAgeBand.Child,
        <= 17 => VisualAgeBand.Adolescent,
        <= 29 => VisualAgeBand.YoungAdult,
        <= 44 => VisualAgeBand.Adult,
        <= 59 => VisualAgeBand.Mature,
        _ => VisualAgeBand.Elderly,
    };
}

public sealed record AppearanceDescription(string ShortDescription, string DetailedDescription, string PortraitDescription, string AccessibilityDescription);

public static class AppearanceDescriptionBuilder
{
    public static AppearanceDescription Build(CharacterVisualState state)
    {
        CharacterVisualProfile p = state.Profile;
        string subject = state.Sex == Sex.Female ? "woman" : "man";
        string age = Words(state.AgeBand);
        string features = p.NotableFeatures.Count == 0 ? "no prominent marks" : Join(p.NotableFeatures.Select(Words));
        string status = Status(state.LegalStatus, state.SocialClass, state.Duty);
        string shortText = $"A {age} {subject} with {Words(p.HairColor)} {Words(p.HairStyle)} hair and {Words(p.EyeColor)} eyes.";
        string detailed = $"A {age} Roman {subject} of {Words(p.Height)} height and {Words(p.Build)} build, with a {Words(p.FacialStructure)} face, {Words(p.Complexion)} complexion, {Words(p.EyeColor)} eyes, and {Words(p.HairStyle)} {Words(p.HairColor)} hair. Visible details include {features}. {status}";
        string portrait = $"Head-and-shoulders portrait; {age} Roman {subject}; {Words(p.FacialStructure)} face; {Words(p.Complexion)} complexion; {Words(p.EyeColor)} eyes; {Words(p.HairStyle)} {Words(p.HairColor)} hair; {features}; {status.ToLowerInvariant()}";
        string accessibility = $"Portrait of a {age} {subject} with a {Words(p.FacialStructure)} face, {Words(p.HairColor)} hair, {Words(p.EyeColor)} eyes, and {features}.";
        return new(shortText, detailed, portrait, accessibility);
    }

    private static string Status(LegalStatus legal, SocialClass? social, DutySlot? duty)
    {
        string value = social is null ? $"Clothing reflects {Words(legal)} status." : $"Clothing reflects the {Words(social.Value)} class.";
        return duty is null ? value : value + $" Duty markers indicate {Words(duty.Value)} service.";
    }
    private static string Words<T>(T value) where T : struct, Enum => SplitPascal(value.ToString()).ToLowerInvariant();
    private static string SplitPascal(string value)
    {
        var text = new StringBuilder(value.Length + 4);
        for (int i = 0; i < value.Length; i++) { if (i > 0 && char.IsUpper(value[i])) text.Append(' '); text.Append(value[i]); }
        return text.ToString();
    }
    private static string Join(IEnumerable<string> values)
    {
        string[] items = values.ToArray();
        return items.Length switch { 0 => string.Empty, 1 => items[0], 2 => items[0] + " and " + items[1], _ => string.Join(", ", items.Take(items.Length - 1)) + ", and " + items[^1] };
    }
}

public readonly record struct PortraitStyleId(string Value)
{
    public static PortraitStyleId ProceduralDefault { get; } = new("gens.procedural.default");
    public override string ToString() => Value;
}

public sealed record PortraitLayer(string Slot, string AssetId, string ColorToken);
public sealed record PortraitRecipe(int RecipeVersion, ulong VisualSeed, PortraitStyleId StyleId, string VisualStateHash, IReadOnlyList<PortraitLayer> Layers)
{
    public string CanonicalIdentity => string.Join("\n", new[] { RecipeVersion.ToString(CultureInfo.InvariantCulture), VisualSeed.ToString(CultureInfo.InvariantCulture), StyleId.Value, VisualStateHash }.Concat(Layers.Select(static layer => $"{layer.Slot}|{layer.AssetId}|{layer.ColorToken}")));
}

public static class PortraitRecipeBuilder
{
    public const int CurrentRecipeVersion = 1;
    public static PortraitRecipe Build(CharacterVisualState state, PortraitStyleId? style = null)
    {
        PortraitStyleId selected = style ?? PortraitStyleId.ProceduralDefault;
        CharacterVisualProfile p = state.Profile;
        var layers = new List<PortraitLayer>
        {
            new("background", Variant(state.VisualSeed, "portrait/background", 3), "background"),
            new("body", "portrait/body/" + Token(p.Build), "cloth-muted"),
            new("clothing", "portrait/clothing/" + Clothing(state), "cloth-status"),
            new("head", "portrait/head/" + Token(p.FacialStructure), "skin-" + Token(p.Complexion)),
            new("eyes", "portrait/eyes/default", "eye-" + Token(p.EyeColor)),
            new("hair", "portrait/hair/" + Token(p.HairStyle), "hair-" + Token(p.HairColor)),
        };
        foreach (NotableFeature feature in p.NotableFeatures.OrderBy(static feature => feature)) layers.Add(new("detail", "portrait/detail/" + Token(feature), "detail"));
        if (state.Duty is not null) layers.Add(new("office", "portrait/office/duty-marker", "status-gold"));
        if (!state.IsAlive) layers.Add(new("foreground", "portrait/foreground/memorial", "memorial"));
        return new(CurrentRecipeVersion, state.VisualSeed, selected, state.VisualStateHash, layers);
    }

    private static string Clothing(CharacterVisualState state) => state.SocialClass switch { SocialClass.Senatorial => "senatorial-toga", SocialClass.Equestrian => "equestrian-toga", _ => state.LegalStatus switch { LegalStatus.Enslaved => "plain-tunic", LegalStatus.Freedman => "freed-tunic", _ => "citizen-tunic" } };
    private static string Variant(ulong seed, string prefix, int count) => $"{prefix}-{(seed % (ulong)count) + 1:D2}";
    private static string Token<T>(T value) where T : struct, Enum
    {
        string name = value.ToString(); var text = new StringBuilder(name.Length + 3);
        for (int i = 0; i < name.Length; i++) { if (i > 0 && char.IsUpper(name[i])) text.Append('-'); text.Append(char.ToLowerInvariant(name[i])); }
        return text.ToString();
    }
}

public sealed record PortraitReference(PortraitSourceKind SourceKind, string ContentId, PortraitStyleId StyleId, string VisualStateHash, int RecipeVersion, string RendererVersion, int PixelSize);
public sealed record PortraitSnapshot(string SubjectId, string CampaignDate, string VisualStateHash, PortraitReference Portrait, int RecipeVersion);

public static class PortraitSourcePriority
{
    public static PortraitReference? Select(IEnumerable<PortraitReference> candidates)
    {
        if (candidates is null) throw new ArgumentNullException(nameof(candidates));
        return candidates.OrderBy(static candidate => candidate.SourceKind switch
        {
            PortraitSourceKind.Custom => 0,
            PortraitSourceKind.Generated => 1,
            PortraitSourceKind.Procedural => 2,
            _ => 3,
        }).FirstOrDefault();
    }
}

public static class VisualHash
{
    public static string Compute(CharacterVisualState state)
    {
        CharacterVisualProfile p = state.Profile;
        string canonical = string.Join("|", state.VisualProfileVersion, state.CharacterId, state.Sex, state.AgeBand, p.Height, p.Build, p.FacialStructure, p.Complexion, p.HairColor, p.HairStyle, p.EyeColor, string.Join(",", p.NotableFeatures.OrderBy(static x => x)), state.LegalStatus, state.SocialClass, state.Duty, state.IsAlive);
        byte[] bytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(canonical));
        return string.Concat(bytes.Select(static value => value.ToString("x2", CultureInfo.InvariantCulture)));
    }

    public static VisualChangeKind Classify(CharacterVisualState previous, CharacterVisualState current)
    {
        if (previous.VisualStateHash == current.VisualStateHash) return VisualChangeKind.Unchanged;
        return previous.AgeBand != current.AgeBand || previous.Profile != current.Profile || previous.IsAlive != current.IsAlive ? VisualChangeKind.Major : VisualChangeKind.Minor;
    }
}
