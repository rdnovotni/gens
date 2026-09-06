#nullable enable

namespace Gens.Presentation.Shell.DevConsole;

/// <summary>One dev-console verb (mirroring <c>tools/Gens.ContentCompiler</c>'s <c>Commands/*Command</c>
/// shape). <see cref="Execute"/> returns the text to append to the console scrollback; it must never
/// mutate <c>WorldState</c> directly — reads go through <see cref="CampaignShell.Query{TProjection}"/>,
/// writes through <see cref="CampaignShell.Submit{TCommand}"/>, both reached via
/// <see cref="DevConsoleContext.Shell"/> (ADR 0013).</summary>
public interface IDevConsoleCommand
{
    string Name { get; }

    string Description { get; }

    string Execute(DevConsoleContext context, string[] args);
}
