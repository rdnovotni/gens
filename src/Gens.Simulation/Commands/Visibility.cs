using Gens.Simulation.State;

namespace Gens.Simulation.Commands;

/// <summary>
/// Who, in-fiction, can know a domain event happened, at what confidence, and through what channel
/// (ADR 0007, ADR 0008). Mandatory on every <see cref="IDomainEvent"/> — an event with no defined
/// visibility cannot be constructed. Phase 2 only fixes this shape; propagating a <see
/// cref="Visibility"/> into <see cref="KnowledgeState"/> is a later phase's job, once real events
/// carry real payloads.
/// </summary>
public readonly record struct Visibility(IReadOnlyList<string> ObserverIds, KnowledgeConfidence DefaultConfidence, string Channel)
{
    /// <summary>Known to everyone. <see cref="ObserverIds"/> is empty by convention — "empty means
    /// everyone", documented here since an empty list would otherwise read as "known to no one".</summary>
    public static Visibility Public { get; } = new(Array.Empty<string>(), KnowledgeConfidence.Certain, "public");

    /// <summary>Known only to the named observers, at certain confidence, via direct witness.</summary>
    public static Visibility Private(params string[] observerIds) =>
        new(observerIds, KnowledgeConfidence.Certain, "witnessed");
}
