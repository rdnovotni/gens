#nullable enable

namespace Gens.Presentation.Shell.DevConsole.Commands;

/// <summary>`clear` — clears the console scrollback.</summary>
public sealed class ClearCommand : IDevConsoleCommand
{
    public string Name => "clear";

    public string Description => "Clears the console scrollback.";

    public string Execute(DevConsoleContext context, string[] args)
    {
        context.ClearOutput();
        return string.Empty;
    }
}
