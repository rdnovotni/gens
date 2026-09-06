#nullable enable

using System;
using System.Linq;

namespace Gens.Presentation.Shell.DevConsole;

/// <summary>Splits one typed console line into a verb and its trailing arguments — pulled out of
/// <see cref="DevConsoleController"/> as plain, Unity-free logic so it can be unit-tested without a
/// <c>MonoBehaviour</c> host.</summary>
public static class DevConsoleLineParser
{
    /// <summary>Returns <c>null</c> for a blank/whitespace-only line. Otherwise splits on spaces,
    /// treating the first token as the verb and the rest as its arguments (a `submit` payload's JSON
    /// must therefore not itself contain a space).</summary>
    public static (string Verb, string[] Args)? Parse(string line)
    {
        var trimmed = line?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
            return null;

        var tokens = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return (tokens[0], tokens.Skip(1).ToArray());
    }
}
