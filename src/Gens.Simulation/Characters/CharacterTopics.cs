namespace Gens.Simulation.Characters;

/// <summary>
/// Canonical <c>KnowledgeState</c> <c>Topic</c> names (ADR 0008) for each <see cref="Character"/>
/// field group — the field-level visibility contract a future event's <c>Visibility</c> descriptor
/// writes into. This item only names the contract; no system yet emits a Character event to actually
/// populate a <see cref="State.KnowledgeState"/> entry under one of these topics (that starts once
/// lifecycle transitions and interactions exist, Phase 5 items 3+), matching how ADR 0007's own
/// <c>Visibility</c> field was fixed in shape before any event carried a real payload.
/// </summary>
public static class CharacterTopics
{
    public const string Identity = "character.identity";
    public const string Appearance = "character.appearance";
    public const string LegalStatus = "character.legalStatus";
    public const string SocialClass = "character.socialClass";
    public const string Culture = "character.culture";
    public const string Location = "character.location";
    public const string Household = "character.household";
    public const string CoreAttributes = "character.attributes";
    public const string LaborSkills = "character.skills";
    public const string Condition = "character.condition";

    // Added alongside lifecycle transitions, birth, marriage, and death (Phase 5 item 3).
    public const string LifecycleStage = "character.lifecycleStage";
    public const string Parentage = "character.parentage";
    public const string Legitimacy = "character.legitimacy";
    public const string MaritalHistory = "character.maritalHistory";
    public const string PermanentInjuries = "character.permanentInjuries";
    public const string Death = "character.death";

    // Added alongside traits, opposed-pair enforcement, and Personality Axes (Phase 5 item 4).
    public const string Traits = "character.traits";

    // Added alongside the relationship web (Phase 5 item 5).
    public const string Relationships = "character.relationships";
}
