using Gens.Simulation.Actions;
using Gens.Simulation.Identity;
using Gens.Simulation.Policies;

namespace Gens.Simulation.Stewardship;

/// <summary>The minimum <see cref="StewardAutonomyLevel"/> each household <see
/// cref="ActionDefinition"/> requires before <see cref="StewardAutonomousDecisionSystem"/> will
/// consider it (§3): Conservative permits "only most routine upkeep... maintaining existing Standing
/// Policies" — changing a policy tier is not itself routine maintenance, so <see
/// cref="PolicyActionDefinitions.ChangeRitesBudget"/> requires at least Standard. §3's own example of
/// Full-Autonomy-only territory, "funding modest Funded Actions," is exactly <see
/// cref="PolicyActionDefinitions.FundFestival"/>. An action absent from this map defaults to requiring
/// <see cref="StewardAutonomyLevel.Standard"/> — the safer default for anything this catalog hasn't
/// been taught about yet, rather than silently permitting it at Conservative.</summary>
public static class StewardAutonomyGateCatalog
{
    private static readonly Dictionary<DefinitionId<ActionDefinition>, StewardAutonomyLevel> MinimumLevel =
        new()
        {
            [PolicyActionDefinitions.ChangeRitesBudget] = StewardAutonomyLevel.Standard,
            [PolicyActionDefinitions.FundFestival] = StewardAutonomyLevel.FullAutonomy,
        };

    public static bool IsAllowed(DefinitionId<ActionDefinition> id, StewardAutonomyLevel level)
    {
        var required = MinimumLevel.TryGetValue(id, out var minimum) ? minimum : StewardAutonomyLevel.Standard;
        return level >= required;
    }
}
