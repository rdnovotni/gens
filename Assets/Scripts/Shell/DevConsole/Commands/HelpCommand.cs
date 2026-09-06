#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gens.Presentation.Shell.DevConsole.Commands;

/// <summary>`help` — lists every registered command's name and description, including itself.</summary>
public sealed class HelpCommand : IDevConsoleCommand
{
    private readonly IReadOnlyList<IDevConsoleCommand> _commands;

    public HelpCommand(IReadOnlyList<IDevConsoleCommand> commands) => _commands = commands;

    public string Name => "help";

    public string Description => "Lists every available command.";

    public string Execute(DevConsoleContext context, string[] args)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Available commands:");
        foreach (var command in _commands.OrderBy(c => c.Name))
            builder.Append(command.Name).Append(" - ").AppendLine(command.Description);
        return builder.ToString().TrimEnd();
    }
}
