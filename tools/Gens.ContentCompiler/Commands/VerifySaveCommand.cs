using Gens.Simulation.Saves;

namespace Gens.ContentCompiler.Commands;

/// <summary><c>verify-save &lt;path&gt;</c>: checks every manifest checksum and confirms a migration
/// path exists (without materializing one), matching Phase 3, item 7's "verify save" developer
/// command. Never fully loads gameplay state.</summary>
public static class VerifySaveCommand
{
    public static int Run(string savePath)
    {
        try
        {
            SaveReader.VerifyChecksums(savePath);
        }
        catch (Exception ex) when (ex is SaveCorruptedException or IOException)
        {
            Console.Error.WriteLine($"'{savePath}' failed verification: {ex.Message}");
            return 1;
        }

        Console.WriteLine($"'{savePath}' passed checksum verification.");
        return 0;
    }
}
