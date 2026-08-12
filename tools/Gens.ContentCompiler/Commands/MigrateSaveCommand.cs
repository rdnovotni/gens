using Gens.Simulation.Saves;
using Gens.Simulation.Saves.Migrations;

namespace Gens.ContentCompiler.Commands;

/// <summary><c>migrate-save &lt;path&gt; --out &lt;path&gt;</c>: loads a save at any registered
/// version (running its migration chain, ADR 0011), then re-saves it at <see
/// cref="SaveFormat.CurrentVersion"/>. Uses <see cref="SaveMigrationRegistry.Empty"/> today — real
/// migrations join this registry as real breaking save-format changes ship.</summary>
public static class MigrateSaveCommand
{
    public static int Run(string savePath, string outPath)
    {
        var loaded = SaveReader.Read(savePath, SaveMigrationRegistry.Empty);
        SaveWriter.Write(outPath, loaded.State, loaded.RandomStreams, loaded.Manifest.GameVersion, loaded.Manifest.ContentPackHash);

        Console.WriteLine($"Migrated '{savePath}' (v{loaded.Manifest.SaveFormatVersion}) to '{outPath}' (v{SaveFormat.CurrentVersion}).");
        return 0;
    }
}
