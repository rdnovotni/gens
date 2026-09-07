using System.Security.Cryptography;
using System.Text;

namespace Gens.Art;

public static class ArtRequestFingerprint
{
    public static string Compute(ArtGenerationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var fields = new List<string>
        {
            "art-request-v1", request.Purpose.ToString(), request.SubjectVisualHash, request.StyleId.Value,
            request.PromptCompilerVersion, request.RecipeVersion, request.Width.ToString(System.Globalization.CultureInfo.InvariantCulture),
            request.Height.ToString(System.Globalization.CultureInfo.InvariantCulture), request.Seed?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "",
            request.GenerationProfile.ProfileId, request.GenerationProfile.SafetyProfile.ToString(),
            request.Prompt.SubjectDescription, request.Prompt.HistoricalContext, request.Prompt.ClothingAndStatus,
            request.Prompt.Composition, request.Prompt.Lighting, request.Prompt.VisualStyle,
            string.Join("\u001f", request.Prompt.QualityConstraints), string.Join("\u001f", request.Prompt.NegativeConstraints),
        };
        AppendSorted(fields, request.GenerationProfile.Parameters);
        AppendSorted(fields, request.Metadata);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("\u001e", fields)))).ToLowerInvariant();
    }

    private static void AppendSorted(List<string> fields, IReadOnlyDictionary<string, string>? values)
    {
        if (values is null) return;
        foreach (var pair in values.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
        {
            fields.Add(pair.Key);
            fields.Add(pair.Value);
        }
    }
}
