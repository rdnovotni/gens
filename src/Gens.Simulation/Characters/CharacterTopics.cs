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
    public const string LegalStatus = "character.legalStatus";
    public const string SocialClass = "character.socialClass";
    public const string Culture = "character.culture";
    public const string Location = "character.location";
    public const string Household = "character.household";
    public const string CoreAttributes = "character.attributes";
    public const string LaborSkills = "character.skills";
    public const string Condition = "character.condition";
}
