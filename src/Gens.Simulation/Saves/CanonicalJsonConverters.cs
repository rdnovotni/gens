using System.Text.Json;
using System.Text.Json.Serialization;
using Gens.Simulation.Identity;
using Gens.Simulation.Numerics;
using Gens.Simulation.Random;
using Gens.Simulation.Time;

namespace Gens.Simulation.Saves;

/// <summary>Persists a <see cref="Fixed64"/> as its raw scaled <see cref="long"/> (ADR 0010) — never
/// as decimal text, which would be a lossy round trip through text formatting.</summary>
public sealed class Fixed64JsonConverter : JsonConverter<Fixed64>
{
    public override Fixed64 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        Fixed64.FromRaw(reader.GetInt64());

    public override void Write(Utf8JsonWriter writer, Fixed64 value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value.RawValue);
}

/// <summary>Persists a <see cref="GameDate"/> as its raw <see cref="GameDate.TotalMonths"/> integer
/// (ADR 0003's compact authoritative representation), never a derived calendar string.</summary>
public sealed class GameDateJsonConverter : JsonConverter<GameDate>
{
    public override GameDate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetInt32());

    public override void Write(Utf8JsonWriter writer, GameDate value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value.TotalMonths);
}

/// <summary>Persists a <see cref="Pcg32State"/> with an explicit, fixed field order (ADR 0010) rather
/// than relying on reflection/declaration order.</summary>
public sealed class Pcg32StateJsonConverter : JsonConverter<Pcg32State>
{
    public override Pcg32State Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected a Pcg32State object.");

        int? algorithmVersion = null;
        ulong? state = null;
        ulong? increment = null;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            var propertyName = reader.GetString();
            reader.Read();
            switch (propertyName)
            {
                case "algorithmVersion":
                    algorithmVersion = reader.GetInt32();
                    break;
                case "state":
                    state = reader.GetUInt64();
                    break;
                case "increment":
                    increment = reader.GetUInt64();
                    break;
                default:
                    throw new JsonException($"Unexpected Pcg32State property '{propertyName}'.");
            }
        }

        if (algorithmVersion is null || state is null || increment is null)
            throw new JsonException("Pcg32State is missing a required field.");

        return new Pcg32State(algorithmVersion.Value, state.Value, increment.Value);
    }

    public override void Write(Utf8JsonWriter writer, Pcg32State value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("algorithmVersion", value.AlgorithmVersion);
        writer.WriteNumber("state", value.State);
        writer.WriteNumber("increment", value.Increment);
        writer.WriteEndObject();
    }
}

/// <summary>Persists a <see cref="RuntimeId{T}"/> as its save-file tagged-string cross-reference form
/// (e.g. <c>char_0000042</c>, ADR 0001) rather than its raw <see cref="long"/> value.</summary>
public sealed class RuntimeIdJsonConverter<T> : JsonConverter<RuntimeId<T>>
{
    public override RuntimeId<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        RuntimeId<T>.Parse(reader.GetString() ?? throw new JsonException("Expected a tagged runtime ID string."));

    public override void Write(Utf8JsonWriter writer, RuntimeId<T> value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToTaggedString());
}

/// <summary>Creates a <see cref="RuntimeIdJsonConverter{T}"/> for whichever closed <see
/// cref="RuntimeId{T}"/> generic is encountered, without registering one converter per entity kind.</summary>
public sealed class RuntimeIdJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(RuntimeId<>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var entityKind = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(RuntimeIdJsonConverter<>).MakeGenericType(entityKind);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

/// <summary>Persists a <see cref="DefinitionId{T}"/> as its plain content-authored string value.</summary>
public sealed class DefinitionIdJsonConverter<T> : JsonConverter<DefinitionId<T>>
{
    public override DefinitionId<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("Expected a definition ID string."));

    public override void Write(Utf8JsonWriter writer, DefinitionId<T> value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}

/// <summary>Creates a <see cref="DefinitionIdJsonConverter{T}"/> for whichever closed <see
/// cref="DefinitionId{T}"/> generic is encountered, without registering one converter per content family.</summary>
public sealed class DefinitionIdJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(DefinitionId<>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var entityKind = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(DefinitionIdJsonConverter<>).MakeGenericType(entityKind);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}
