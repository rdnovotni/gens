namespace Gens.Simulation.Schemes;

/// <summary>A scheme's flavor — the Coercive/Intrigue Interaction Catalog entries
/// (<c>gens-characters-design.md</c> §9) that resolve through the shared engine in this namespace
/// rather than each inventing its own resolution logic. Each value is a bare tag today: the narrative/
/// mechanical payoff a specific type has on success (damaging a target's reputation, planting false
/// evidence, and so on) needs systems that do not exist yet in this codebase (Reputation, Legal &amp;
/// Court) — matching <see cref="Policies.RitesBudgetCatalog"/>'s own "the projection exists before its
/// consumer does" precedent, the engine here is real and complete; a type's unique success effect is
/// future integration work layered on top of it.</summary>
public enum SchemeType
{
    FabricateHook,
    Sabotage,
    Blackmail,
    Frame,
    Assassinate,
}
