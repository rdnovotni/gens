using Gens.Simulation.Actions;
using Gens.Simulation.Identity;

namespace Gens.Simulation.Stewardship;

/// <summary>The Always-Held Decisions (§4) — never auto-resolved at any autonomy level: issuing an
/// Edict, Household Doctrine choices, arranging/approving marriage, initiating a military campaign,
/// severe Legal &amp; Court actions, an Alliance Against Rome, named-individual Manumission, and
/// anything touching Succession. None of those systems exist as <see cref="ActionDefinition"/>s in
/// this codebase yet (Edicts, Doctrine, Marriage, Military, Legal &amp; Court, and Succession are all
/// unbuilt future phases) — so this set is empty today, not because the rule is unenforced, but
/// because there is nothing yet registered that it would need to exclude. <see
/// cref="StewardAutonomousDecisionSystem"/> still consults this catalog on every decision so a future
/// phase that registers one of those action definitions gets the exclusion for free, without needing
/// to touch the steward system itself.</summary>
public static class StewardAlwaysHeldCatalog
{
    private static readonly HashSet<DefinitionId<ActionDefinition>> AlwaysHeld = new();

    public static bool IsAlwaysHeld(DefinitionId<ActionDefinition> id) => AlwaysHeld.Contains(id);
}
