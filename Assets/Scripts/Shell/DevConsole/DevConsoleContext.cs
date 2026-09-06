#nullable enable

using System;

namespace Gens.Presentation.Shell.DevConsole;

/// <summary>Everything a <see cref="IDevConsoleCommand"/> needs to read or write the running campaign
/// through the sanctioned <see cref="CampaignShell"/> boundary — commands never receive a raw
/// <c>WorldState</c> reference, only this context.</summary>
public sealed class DevConsoleContext
{
    /// <summary>The actor ID every dev-console-submitted command defaults to, so a chronicle/report
    /// reader can always tell a manual dev action apart from a player or "system" one.</summary>
    public const string DevConsoleActorId = "dev-console";

    private readonly CampaignShellBehaviour _shellBehaviour;
    private readonly Action _clearOutput;

    public DevConsoleContext(CampaignShellBehaviour shellBehaviour, Action clearOutput)
    {
        _shellBehaviour = shellBehaviour ?? throw new ArgumentNullException(nameof(shellBehaviour));
        _clearOutput = clearOutput ?? throw new ArgumentNullException(nameof(clearOutput));
    }

    /// <summary>The active campaign shell, or null when no campaign has been started yet (e.g. the
    /// console was opened from the Main Menu before "New Campaign"/"Load Campaign").</summary>
    public CampaignShell? Shell => _shellBehaviour.Shell;

    public void ClearOutput() => _clearOutput();
}
