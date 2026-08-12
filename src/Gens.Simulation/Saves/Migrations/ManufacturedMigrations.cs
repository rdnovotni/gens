using System.Text.Json;

namespace Gens.Simulation.Saves.Migrations;

/// <summary>
/// ADR 0011's deliberately-manufactured proof migration: "the first migration this registry needs to
/// actually exercise in CI is deliberately manufactured (a trivial 1→2 fixture) before any real
/// breaking change occurs, so the registry and CI wiring are proven before they're needed for real."
/// This is infrastructure-proving only — <see cref="SaveFormat.CurrentVersion"/> stays <c>1</c>; no
/// real save is ever migrated by this function through the normal <see cref="SaveReader"/> path. It
/// is exercised directly, against its own fixture, by <c>MigrationRegistryTests</c>.
/// </summary>
public static class ManufacturedMigrations
{
    /// <summary>Adds a harmless additive field (<c>migrationProof</c>) with a defined default — per
    /// ADR 0011's additive-preferring policy, a real instance of this shape would need no migration
    /// function at all; this one exists solely to prove the chained-registry mechanism runs a
    /// registered from-version-1 migration correctly.</summary>
    public static JsonDocument Migrate1To2(JsonDocument worldV1)
    {
        if (worldV1 is null)
            throw new ArgumentNullException(nameof(worldV1));

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            foreach (var property in worldV1.RootElement.EnumerateObject())
                property.WriteTo(writer);
            writer.WriteBoolean("migrationProof", true);
            writer.WriteEndObject();
        }

        stream.Position = 0;
        return JsonDocument.Parse(stream);
    }

    public static SaveMigrationRegistry BuildRegistryWithProofMigration() => new(
        new Dictionary<int, Func<JsonDocument, JsonDocument>>
        {
            [1] = Migrate1To2,
        });
}
