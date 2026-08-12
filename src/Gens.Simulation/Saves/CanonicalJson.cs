using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Gens.Simulation.Saves;

/// <summary>
/// The single canonical-JSON contract (ADR 0010) every persisted save DTO and content DTO in this
/// project serializes through: <see cref="CultureInfo.InvariantCulture"/>, no indentation in the
/// persisted bytes, UTF-8 without a byte-order mark, and every property emitted through an explicit
/// <see cref="System.Text.Json.Serialization.JsonPropertyOrderAttribute"/> rather than reflection or
/// declaration order. Every collection a DTO exposes must already be sorted (ADR 0001/0004's
/// ascending-<c>RuntimeId</c>/ordinal-string order) before it reaches these writers — this class does
/// not sort anything itself.
/// </summary>
public static class CanonicalJson
{
    /// <summary>The saved-artifact options: compact, byte-stable for identical input.</summary>
    public static readonly JsonSerializerOptions Options = Build(indented: false);

    /// <summary>A human-readable debug-export variant. Never the bytes actually persisted to a
    /// <c>.gens</c> archive or a compiled content pack — those always use <see cref="Options"/>.</summary>
    public static readonly JsonSerializerOptions IndentedOptions = Build(indented: true);

    private static JsonSerializerOptions Build(bool indented) => new()
    {
        WriteIndented = indented,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters =
        {
            new Fixed64JsonConverter(),
            new GameDateJsonConverter(),
            new Pcg32StateJsonConverter(),
            new RuntimeIdJsonConverterFactory(),
            new DefinitionIdJsonConverterFactory(),
            new System.Text.Json.Serialization.JsonStringEnumConverter(),
        },
    };

    /// <summary>UTF-8 bytes without a byte-order mark, using <see cref="Options"/> — the exact bytes a
    /// save entry or a compiled content file persists.</summary>
    public static byte[] SerializeToCanonicalBytes<T>(T value)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false, SkipValidation = false }))
            JsonSerializer.Serialize(writer, value, Options);
        return stream.ToArray();
    }

    public static T Deserialize<T>(ReadOnlySpan<byte> canonicalBytes) =>
        JsonSerializer.Deserialize<T>(canonicalBytes, Options) ??
        throw new JsonException($"Deserializing {typeof(T).Name} produced null.");

    /// <summary>SHA-256 over the canonical bytes, hex-encoded lowercase — the checksum shape ADR 0010
    /// (save manifests) and ADR 0012 (content manifests) both use for entry/source integrity.</summary>
    public static string Sha256Hex(byte[] canonicalBytes)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(canonicalBytes);
        var builder = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
            builder.Append(b.ToString("x2", CultureInfo.InvariantCulture));
        return builder.ToString();
    }
}
