namespace Gens.ContentCompiler.Commands;

/// <summary>The single place every save-writing command reads the game version from, so the value
/// stamped into a save's manifest is consistent across every CLI verb that can produce one.</summary>
internal static class GameVersionInfo
{
    public static string Current => typeof(GameVersionInfo).Assembly.GetName().Version?.ToString() ?? "0.0.0";
}
