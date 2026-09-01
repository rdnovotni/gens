using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Health;

/// <summary>One Character's standing case of a <see cref="HealthConditionDefinition"/> (Phase 14 item
/// 1; <c>gens-disease-public-health-design.md</c> §11's <c>CharacterInfectionStatus</c>, generalized to
/// cover both the Endemic Illness and Epidemic layers rather than only the latter). Deliberately not
/// named <c>HealthCondition</c>: that would collide with the pre-existing five-stat <see
/// cref="Condition"/> struct (Familia's Health/Fatigue/Loyalty/Ambition/Fertility block,
/// <c>gens-familia-design.md</c> §2.3) — a wholly different concept this record's own <see
/// cref="CharacterHealthConditionSystem"/> reads and writes against, never replaces. Immunity (§5) is
/// not a separate record: a case that resolves as <see cref="CharacterHealthConditionStatus.Recovered"/>
/// with <see cref="GrantedImmunity"/> true is kept in the <c>WorldState</c> registry forever (never
/// removed, matching <c>Events.EventInstances</c>'s identical convention) — <see
/// cref="HealthQueries.IsImmune"/> is simply a scan for exactly that shape.</summary>
public sealed record CharacterHealthCondition
{
    public required RuntimeId<CharacterHealthCondition> Id { get; init; }
    public required RuntimeId<Character> CharacterId { get; init; }
    public required DefinitionId<HealthConditionDefinition> ConditionId { get; init; }

    /// <summary>Snapshotted from the <see cref="HealthConditionDefinition"/> at onset (<see
    /// cref="AfflictCharacterCommand"/>) rather than re-resolved from a <see
    /// cref="HealthConditionCatalog"/> every tick — this record's own <see
    /// cref="CharacterHealthConditionSystem"/> and <see cref="HealthConditionProgressionCalculator"/>
    /// never need catalog access at all, the same way a <see cref="PermanentInjury"/> never re-resolves
    /// anything from content either.</summary>
    public required HealthConditionCategory Category { get; init; }

    /// <summary>Snapshotted from <see cref="HealthConditionDefinition.HasCure"/> at onset, for the same
    /// "no catalog access needed at tick time" reason as <see cref="Category"/>.</summary>
    public required bool HasCure { get; init; }

    public required int Severity { get; init; }
    public required GameDate OnsetDate { get; init; }
    public required CharacterHealthConditionStatus Status { get; init; }

    /// <summary>Whether <see cref="CharacterHealthConditionSystem"/> assigned this case one of the
    /// afflicted Household's bounded <see cref="CareCapacityCalculator"/> treatment slots this most
    /// recent tick — recomputed every month, not a permanent flag.</summary>
    public bool TreatedByPhysician { get; init; }

    /// <summary>True only when <see cref="Status"/> is <see
    /// cref="CharacterHealthConditionStatus.Recovered"/> and <see cref="Category"/> was <see
    /// cref="HealthConditionCategory.Acute"/> (§5: "a Character who survives an Epidemic gains real,
    /// lasting Immunity to that specific disease" — Chronic/Endemic recovery grants none).</summary>
    public bool GrantedImmunity { get; init; }

    /// <summary>Set once <see cref="Status"/> leaves <see cref="CharacterHealthConditionStatus.Active"/>;
    /// null while still Active.</summary>
    public GameDate? ResolvedDate { get; init; }

    /// <summary>Personal Quarantine (§4.1, Phase 14 item 2) — set by <see
    /// cref="SetPersonalQuarantineCommands"/>, mirroring §11's <c>CharacterInfectionStatus.quarantined</c>
    /// field directly rather than a separate record. Only meaningful while <see cref="Status"/> is <see
    /// cref="CharacterHealthConditionStatus.Active"/>: <see cref="EpidemicContagionSystem"/> reads it as
    /// a spread-reduction multiplier on this case's own contagiousness, and <see
    /// cref="HealthConditionProgressionCalculator.MonthlyRecoveryProbability"/> reads it as §4.1's own
    /// "at a real cost to their own recovery odds" penalty — isolation helps everyone else, not the
    /// isolated Character.</summary>
    public bool Quarantined { get; init; }

    public static CharacterHealthCondition Create(
        RuntimeId<CharacterHealthCondition> id,
        RuntimeId<Character> characterId,
        DefinitionId<HealthConditionDefinition> conditionId,
        HealthConditionCategory category,
        bool hasCure,
        int severity,
        GameDate onsetDate)
    {
        StatRange.Validate(severity, nameof(severity));

        return new CharacterHealthCondition
        {
            Id = id,
            CharacterId = characterId,
            ConditionId = conditionId,
            Category = category,
            HasCure = hasCure,
            Severity = severity,
            OnsetDate = onsetDate,
            Status = CharacterHealthConditionStatus.Active,
        };
    }
}
