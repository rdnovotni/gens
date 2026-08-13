using System.Text;

namespace Gens.Simulation.Characters;

/// <summary>Deterministically compiles an already-generated set of appearance attributes into an
/// ordered <see cref="PortraitRecipe"/>. Pure and RNG-free by design: the randomness lives entirely in
/// <see cref="CharacterVisualProfileGenerator"/>'s attribute rolls, so this step can never perturb a
/// random stream and always produces the same recipe for the same attributes.</summary>
public static class PortraitRecipeGenerator
{
    /// <summary>Fixed enum-declaration order, not the caller's <paramref name="notableFeatures"/> list
    /// order, so the recipe's feature layers stay identical regardless of which order the generator
    /// happened to roll each feature in.</summary>
    private static readonly NotableFeature[] FeatureLayerOrder =
    {
        NotableFeature.Scar,
        NotableFeature.BrokenNose,
        NotableFeature.Freckled,
        NotableFeature.Birthmark,
        NotableFeature.GrayAtTemples,
    };

    public static PortraitRecipe Generate(
        Height height,
        Build build,
        FacialStructure facialStructure,
        Complexion complexion,
        HairColor hairColor,
        HairStyle hairStyle,
        EyeColor eyeColor,
        IReadOnlyList<NotableFeature> notableFeatures)
    {
        if (notableFeatures is null)
            throw new ArgumentNullException(nameof(notableFeatures));

        var layers = new List<string>
        {
            $"body/height/{ToToken(height)}",
            $"body/build/{ToToken(build)}",
            $"face/structure/{ToToken(facialStructure)}",
            $"face/complexion/{ToToken(complexion)}",
            $"eyes/color/{ToToken(eyeColor)}",
            $"hair/color/{ToToken(hairColor)}",
            $"hair/style/{ToToken(hairStyle)}",
        };

        foreach (var feature in FeatureLayerOrder)
            if (notableFeatures.Contains(feature))
                layers.Add($"feature/{ToToken(feature)}");

        return new PortraitRecipe { Layers = layers };
    }

    /// <summary>PascalCase enum name to kebab-case token (e.g. <c>ShoulderLength</c> to
    /// <c>shoulder-length</c>), matching <see cref="Identity.DefinitionId{T}"/>'s own kebab-case
    /// convention for content-facing identifiers.</summary>
    private static string ToToken<T>(T value) where T : struct, Enum
    {
        var name = value.ToString();
        var builder = new StringBuilder(name.Length + 4);
        for (var i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i]))
                builder.Append('-');
            builder.Append(char.ToLowerInvariant(name[i]));
        }

        return builder.ToString();
    }
}
