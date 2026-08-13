using Gens.Simulation.Identity;

namespace Gens.Simulation.Characters;

/// <summary>Why <see cref="TraitCatalog.CheckExclusivity"/> refused a candidate trait.</summary>
public enum TraitExclusivityReason
{
    /// <summary>The Character already holds this exact trait.</summary>
    AlreadyHeld,

    /// <summary>The candidate and an already-held trait are an opposed pair
    /// (<c>gens-traits-design.md</c> Characters §4: "can't hold both sides").</summary>
    OpposedPair,

    /// <summary>The candidate and an already-held trait belong to the same tiered spectrum
    /// (<c>gens-traits-design.md</c> §3: "holding one tier's tag actively excludes every other tier
    /// in that same spectrum").</summary>
    SameSpectrum,
}

/// <summary>Why a candidate trait cannot be added alongside an already-held one, and which held trait
/// it conflicts with.</summary>
public readonly record struct TraitExclusivityViolation(DefinitionId<Trait> ConflictingTraitId, TraitExclusivityReason Reason);

/// <summary>The runtime lookup over a compiled Trait content pack (Phase 5 item 4). Owns the two
/// things a bare <c>DefinitionId&lt;Trait&gt;</c> reference on a <see cref="Character"/> can't answer
/// on its own: whether acquiring a candidate trait would violate opposed-pair or tiered-spectrum
/// exclusivity, and what a Character's held traits add up to on each <see cref="PersonalityAxis"/>.
/// Deliberately independent of <c>WorldState</c> — the same shape the project's Culture/Good
/// <c>DefinitionId</c> references already use, where the compiled content catalog is resolved by
/// whichever caller has it loaded, not embedded in the save-state graph itself.</summary>
public sealed class TraitCatalog
{
    private readonly IReadOnlyDictionary<DefinitionId<Trait>, TraitDefinition> _byId;

    public TraitCatalog(IEnumerable<TraitDefinition> definitions)
    {
        if (definitions is null)
            throw new ArgumentNullException(nameof(definitions));

        var byId = new Dictionary<DefinitionId<Trait>, TraitDefinition>();
        foreach (var definition in definitions)
        {
            if (!byId.TryAdd(definition.Id, definition))
                throw new ArgumentException($"Duplicate trait definition ID '{definition.Id}' in catalog.", nameof(definitions));
        }

        _byId = byId;
    }

    public bool TryGet(DefinitionId<Trait> id, out TraitDefinition definition) => _byId.TryGetValue(id, out definition);

    public TraitDefinition Get(DefinitionId<Trait> id) =>
        TryGet(id, out var definition)
            ? definition
            : throw new KeyNotFoundException($"No trait definition '{id}' is registered in this catalog.");

    /// <summary>Whether granting <paramref name="candidate"/> to a Character who already holds
    /// <paramref name="existingTraits"/> is allowed. Returns <c>null</c> if it is; otherwise the
    /// specific held trait it conflicts with and why. Every existing/candidate trait must be
    /// registered in this catalog (a dangling reference here is a content-authoring bug the compiler
    /// should already have caught, per ADR 0012 — so this throws rather than silently allowing it).</summary>
    public TraitExclusivityViolation? CheckExclusivity(IEnumerable<DefinitionId<Trait>> existingTraits, DefinitionId<Trait> candidate)
    {
        if (existingTraits is null)
            throw new ArgumentNullException(nameof(existingTraits));

        var candidateDefinition = Get(candidate);

        foreach (var existingId in existingTraits)
        {
            if (existingId == candidate)
                return new TraitExclusivityViolation(existingId, TraitExclusivityReason.AlreadyHeld);

            var existingDefinition = Get(existingId);

            if (candidateDefinition.Opposes == existingId || existingDefinition.Opposes == candidate)
                return new TraitExclusivityViolation(existingId, TraitExclusivityReason.OpposedPair);

            if (candidateDefinition.Spectrum is not null && candidateDefinition.Spectrum == existingDefinition.Spectrum)
                return new TraitExclusivityViolation(existingId, TraitExclusivityReason.SameSpectrum);
        }

        return null;
    }

    /// <summary>Sums <paramref name="traits"/>' nudges toward <paramref name="axis"/>
    /// (<c>gens-characters-design.md</c> §5), clamped to the axis's valid <see
    /// cref="PersonalityAxes.MinScore"/>-<see cref="PersonalityAxes.MaxScore"/> range. A trait with no
    /// nudge on this axis, or that isn't registered in this catalog, contributes nothing.</summary>
    public int GetAxisScore(IEnumerable<DefinitionId<Trait>> traits, PersonalityAxis axis)
    {
        if (traits is null)
            throw new ArgumentNullException(nameof(traits));

        var total = 0;
        foreach (var id in traits)
            if (TryGet(id, out var definition) && definition.Axis == axis)
                total += definition.AxisNudge!.Value;

        return Math.Clamp(total, PersonalityAxes.MinScore, PersonalityAxes.MaxScore);
    }
}
