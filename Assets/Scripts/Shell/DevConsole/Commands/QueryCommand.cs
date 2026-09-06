#nullable enable

using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Queries;

namespace Gens.Presentation.Shell.DevConsole.Commands;

/// <summary>`query &lt;name&gt; [arg]` — runs one of the existing named <c>IWorldQuery&lt;T&gt;</c>
/// implementations against the live campaign and prints the resulting projection record. The set of
/// names is fixed (not reflection-driven) so every exposure is a deliberate, reviewed one rather than
/// an accidental one.</summary>
public sealed class QueryCommand : IDevConsoleCommand
{
    private const string ObserverId = DevConsoleContext.DevConsoleActorId;

    public string Name => "query";

    public string Description =>
        "query <inkBar|householdRoster|estateSettlement|householdFinancials|characterDetail> [characterId] - runs a named query.";

    public string Execute(DevConsoleContext context, string[] args)
    {
        var shell = context.Shell;
        if (shell is null)
            return "No active campaign.";

        if (args.Length == 0)
            return "Usage: " + Description;

        switch (args[0])
        {
            case "inkBar":
                return shell.Query(new InkBarQuery(shell.HouseholdId), ObserverId).ToString();
            case "householdRoster":
                return shell.Query(new HouseholdRosterQuery(shell.HouseholdId), ObserverId).ToString();
            case "estateSettlement":
                return shell.Query(new EstateSettlementQuery(shell.SettlementId, shell.HouseholdId), ObserverId).ToString();
            case "householdFinancials":
                return shell.Query(new HouseholdFinancialsQuery(shell.HouseholdId), ObserverId).ToString();
            case "characterDetail":
                if (args.Length < 2)
                    return "Usage: query characterDetail <characterId>";
                var characterId = RuntimeId<Character>.Parse(args[1]);
                return shell.Query(new CharacterDetailQuery(characterId), ObserverId).ToString();
            default:
                return $"Unknown query '{args[0]}'. " + Description;
        }
    }
}
