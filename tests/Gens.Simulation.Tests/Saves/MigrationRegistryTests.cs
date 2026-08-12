using System.Text.Json;
using Gens.Simulation.Saves;
using Gens.Simulation.Saves.Migrations;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Saves;

/// <summary>
/// Proves ADR 0011's registry/CI wiring using its own deliberately-manufactured proof migration:
/// "the first migration this registry needs to actually exercise in CI is deliberately manufactured
/// (a trivial 1→2 fixture) before any real breaking change occurs." <see cref="SaveFormat.CurrentVersion"/>
/// stays 1 — this fixture is exercised directly against the registry, not through <see
/// cref="SaveReader"/>'s normal load path, since no real save is ever actually at a version needing it.
/// </summary>
public sealed class MigrationRegistryTests
{
    private static readonly string FixturesDirectory =
        Path.Combine(TestContext.CurrentContext.TestDirectory, "Saves", "Fixtures");

    [Test]
    public void ManufacturedMigrationMatchesItsPermanentFixture()
    {
        var preMigrationWorld = File.ReadAllBytes(Path.Combine(FixturesDirectory, "v1-empty-campaign-world.json"));
        var expectedPostMigrationWorld = File.ReadAllBytes(Path.Combine(FixturesDirectory, "v1-to-v2-expected-world.json"));

        var registry = ManufacturedMigrations.BuildRegistryWithProofMigration();

        using var document = JsonDocument.Parse(preMigrationWorld);
        using var migrated = registry.MigrateToVersion(document, fromVersion: 1, toVersion: 2);
        var actualBytes = JsonSerializer.SerializeToUtf8Bytes(migrated.RootElement, CanonicalJson.Options);

        Assert.That(actualBytes, Is.EqualTo(expectedPostMigrationWorld));
    }

    [Test]
    public void MigratingToTheSameVersionIsANoOp()
    {
        var registry = ManufacturedMigrations.BuildRegistryWithProofMigration();
        using var document = JsonDocument.Parse("{\"a\":1}");

        using var result = registry.MigrateToVersion(document, fromVersion: 1, toVersion: 1);

        Assert.That(result.RootElement.GetProperty("a").GetInt32(), Is.EqualTo(1));
    }

    [Test]
    public void MissingMigrationPathIsAHardFailure()
    {
        var registry = SaveMigrationRegistry.Empty;
        using var document = JsonDocument.Parse("{\"a\":1}");

        Assert.That(
            () => registry.MigrateToVersion(document, fromVersion: 1, toVersion: 2),
            Throws.TypeOf<SaveMigrationException>());
    }

    [Test]
    public void CannotMigrateBackwards()
    {
        var registry = SaveMigrationRegistry.Empty;
        using var document = JsonDocument.Parse("{\"a\":1}");

        Assert.That(
            () => registry.MigrateToVersion(document, fromVersion: 2, toVersion: 1),
            Throws.ArgumentException);
    }

    [Test]
    public void ThePermanentGoldenSaveFixtureIsStillLoadableAtTheCurrentSaveFormatVersion()
    {
        var fixturePath = Path.Combine(FixturesDirectory, "v1-empty-campaign.gens");

        var loaded = SaveReader.Read(fixturePath);

        Assert.That(loaded.Manifest.SaveFormatVersion, Is.EqualTo(1));
        Assert.That(loaded.State.Date.TotalMonths, Is.EqualTo(0));
    }
}
