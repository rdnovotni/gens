using System.Text.Json;

namespace Gens.Simulation.Saves.Migrations;

/// <summary>
/// The single lookup table every load path consults (ADR 0011): a migration is a pure function from
/// the canonical <c>world.json</c> DOM at version <c>N</c> to the DOM at version <c>N+1</c>, keyed by
/// its from-version. Migrations chain strictly sequentially — there is no direct shortcut from an old
/// version straight to the current one, even where one would be simpler to write.
/// </summary>
public sealed class SaveMigrationRegistry
{
    private readonly IReadOnlyDictionary<int, Func<JsonDocument, JsonDocument>> _migrations;

    public SaveMigrationRegistry(IReadOnlyDictionary<int, Func<JsonDocument, JsonDocument>> migrations) =>
        _migrations = migrations ?? throw new ArgumentNullException(nameof(migrations));

    /// <summary>An empty registry — every save is already at <see cref="SaveFormat.CurrentVersion"/>,
    /// which is the real state of this project until its first breaking save-format change.</summary>
    public static SaveMigrationRegistry Empty { get; } = new(new Dictionary<int, Func<JsonDocument, JsonDocument>>());

    /// <summary>Chains every registered migration from <paramref name="fromVersion"/> up to <paramref
    /// name="toVersion"/>. A missing entry anywhere in that range is a hard failure (ADR 0011) — never
    /// a best-effort attempt to load the save anyway.</summary>
    public JsonDocument MigrateToVersion(JsonDocument world, int fromVersion, int toVersion)
    {
        if (world is null)
            throw new ArgumentNullException(nameof(world));
        if (fromVersion > toVersion)
            throw new ArgumentException(
                $"Cannot migrate from version {fromVersion} down to {toVersion}; saves only migrate forward.",
                nameof(fromVersion));

        var current = world;
        for (var version = fromVersion; version < toVersion; version++)
        {
            if (!_migrations.TryGetValue(version, out var migrate))
                throw new SaveMigrationException(
                    $"No migration path from save version {version} to {version + 1}.");
            current = migrate(current);
        }

        return current;
    }
}

public sealed class SaveMigrationException : Exception
{
    public SaveMigrationException(string message)
        : base(message)
    {
    }
}
