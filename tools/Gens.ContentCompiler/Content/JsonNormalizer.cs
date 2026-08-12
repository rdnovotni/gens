using System.Text.Json;

namespace Gens.ContentCompiler.Content;

/// <summary>Recursively sorts every JSON object's property names ordinally (ADR 0012's "deterministic
/// normalized output"), so two source files authoring the same definition with different key order
/// compile to byte-identical output. Array element order is never touched — it can be meaningful
/// (e.g. a building recipe's input list).</summary>
public static class JsonNormalizer
{
    /// <summary>Returns a new, independently-owned <see cref="JsonElement"/> with every nested
    /// object's properties sorted ordinally.</summary>
    public static JsonElement Normalize(JsonElement element)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
            WriteNormalized(writer, element);
        stream.Position = 0;
        using var document = JsonDocument.Parse(stream);
        return document.RootElement.Clone();
    }

    public static void WriteNormalized(Utf8JsonWriter writer, JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in element.EnumerateObject().OrderBy(static p => p.Name, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(property.Name);
                    WriteNormalized(writer, property.Value);
                }
                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                    WriteNormalized(writer, item);
                writer.WriteEndArray();
                break;
            default:
                element.WriteTo(writer);
                break;
        }
    }
}
