using Gens.Simulation.Saves;
using Gens.Simulation.State;

namespace Gens.ContentCompiler.Commands;

/// <summary>
/// <c>compare-hashes &lt;pathA&gt; &lt;pathB&gt;</c>: the headless runner's "compare hashes" verb
/// (Phase 4 item 3) — loads two saves and reports whether their <see cref="StateHasher"/> hashes
/// match, the check the exit gate's "reproduces identical hashes" claim reduces to in practice.
/// </summary>
public static class CompareHashesCommand
{
    public static int Run(string pathA, string pathB)
    {
        var hashA = StateHasher.Hash(SaveReader.Read(pathA).State);
        var hashB = StateHasher.Hash(SaveReader.Read(pathB).State);

        var equal = hashA == hashB;
        Console.WriteLine($"'{pathA}': {hashA:x16}");
        Console.WriteLine($"'{pathB}': {hashB:x16}");
        Console.WriteLine(equal ? "Hashes match." : "Hashes differ.");
        return equal ? 0 : 1;
    }
}
