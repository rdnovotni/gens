using Gens.Simulation.Identity;

namespace Gens.Simulation.Health;

/// <summary>Content-authored shape for one named health condition (Phase 14 item 1;
/// <c>gens-disease-public-health-design.md</c> §11's <c>EndemicIllness</c>/<c>EpidemicOutbreak</c>
/// <c>diseaseType</c>). This item builds the generic, disease-agnostic definition shape only — the
/// seven endemic and four epidemic diseases §2/§3 actually name (Roman Fever, Pestilence, and the rest
/// of that roster) are real content to be authored against this shape by Phase 14 item 2, which also
/// builds their terrain/sanitation/crowding-driven Exposure drivers and contagion mechanics; nothing
/// here hardcodes any of those seven/four names, mirroring how Phase 13 item 3 built <c>TravelParty</c>
/// scaffolding before item 4 filled in real Culture/Language content. <see cref="HasCure"/>
/// distinguishes §7's "manages severity without guaranteeing a cure" diseases (most endemic illness,
/// e.g. Roman Fever/Consumption) from ones a course of treatment can actually resolve outright.</summary>
public sealed record HealthConditionDefinition
{
    public HealthConditionDefinition(
        DefinitionId<HealthConditionDefinition> id,
        string name,
        HealthConditionCategory category,
        bool hasCure)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A health condition definition requires a non-empty name.", nameof(name));

        Id = id;
        Name = name;
        Category = category;
        HasCure = hasCure;
    }

    public DefinitionId<HealthConditionDefinition> Id { get; }
    public string Name { get; }
    public HealthConditionCategory Category { get; }
    public bool HasCure { get; }
}
