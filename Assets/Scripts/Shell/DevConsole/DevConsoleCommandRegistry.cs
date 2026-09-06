#nullable enable

using System.Collections.Generic;
using Gens.Presentation.Shell.DevConsole.Commands;

namespace Gens.Presentation.Shell.DevConsole;

/// <summary>The dev console's fixed verb table (v1: help/state/hash/query/submit/clear). Fixed rather
/// than reflection-discovered, so every exposed command is a deliberate, reviewed addition.</summary>
public static class DevConsoleCommandRegistry
{
    public static IReadOnlyList<IDevConsoleCommand> BuildDefault()
    {
        var commands = new List<IDevConsoleCommand>
        {
            new StateCommand(),
            new HashCommand(),
            new QueryCommand(),
            new SubmitVerbCommand(),
            new ClearCommand(),
        };
        commands.Insert(0, new HelpCommand(commands));
        return commands;
    }
}
