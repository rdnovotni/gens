using Gens.Simulation.Saves;

namespace Gens.ContentCompiler.Commands;

/// <summary>
/// <c>save &lt;savePath&gt; --out &lt;path&gt;</c>: the headless runner's standalone "save" verb
/// (Phase 4 item 3) — loads a save and immediately re-saves it unchanged at <paramref
/// name="outPath"/>, exercising the same load/save path <c>advance</c> and <c>submit-command</c> use
/// internally, without also ticking the clock or submitting a command.
/// </summary>
public static class SaveCommand
{
    public static int Run(string savePath, string outPath)
    {
        var loaded = SaveReader.Read(savePath);
        SaveWriter.Write(outPath, loaded.State, loaded.RandomStreams, GameVersionInfo.Current, loaded.Manifest.ContentPackHash);

        Console.WriteLine($"Loaded '{savePath}' and saved to '{outPath}'.");
        return 0;
    }
}
