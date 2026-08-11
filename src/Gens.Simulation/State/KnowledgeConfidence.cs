namespace Gens.Simulation.State;

/// <summary>How certain an observer's knowledge of a fact is (ADR 0008). Shared between the
/// <c>Commands</c> namespace (event <c>Visibility</c>, ADR 0007) and the <c>State</c> namespace
/// (<see cref="KnowledgeState"/>) to avoid a circular reference between the two.</summary>
public enum KnowledgeConfidence
{
    Certain,
    Believed,
    Rumored,
}
