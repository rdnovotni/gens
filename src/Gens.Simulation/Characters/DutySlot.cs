namespace Gens.Simulation.Characters;

/// <summary>The Labor duty slot tier of <c>gens-familia-design.md</c> §4 ("Household Roles &amp;
/// Duties"): one slot per <see cref="LaborSkills"/> axis, open to any eligible Adolescent-or-older
/// household member regardless of <see cref="LegalStatus"/>. §4 lists its examples (Field Hand,
/// Domestic Servant, Cook, Groundskeeper, Craftsman, Tutor's Aide) as non-exhaustive ("etc."); this
/// picks one canonical slot per Labor Skill rather than forcing every listed example into the five
/// available stats.</summary>
public enum DutySlot
{
    /// <summary>Reads <see cref="LaborSkills.Fieldwork"/>.</summary>
    FieldHand,

    /// <summary>Reads <see cref="LaborSkills.DomesticService"/>.</summary>
    DomesticServant,

    /// <summary>Reads <see cref="LaborSkills.Craft"/>.</summary>
    Craftsman,

    /// <summary>Reads <see cref="LaborSkills.Culinary"/>.</summary>
    Cook,

    /// <summary>Reads <see cref="LaborSkills.Medicine"/> (lay/practical medicine, §2.2).</summary>
    Physician,
}

/// <summary>The per-slot rules <c>gens-familia-design.md</c> §4 leaves unspecified numerically:
/// which <see cref="LaborSkills"/> stat a slot draws from, the minimum competence needed to hold it,
/// and how many household members may hold the same slot at once. None of these numbers are given by
/// the design corpus — they are this implementation's own invented baseline, matching <see
/// cref="RelationshipDecaySystem"/>'s identical disclaimer for its own invented decay rate.</summary>
public static class DutySlotCatalog
{
    /// <summary>The minimum effective Labor Skill (0-100) a Character needs to hold any duty slot —
    /// the "competence check" gate. Deliberately low: §4 frames duty slots as open to "any eligible"
    /// member, not just the already-skilled.</summary>
    public const int MinimumCompetence = 10;

    /// <summary>The Fatigue (0-100) at or above which a Character is too worn out to take on a new
    /// duty — the "availability" gate distinct from simply already holding one.</summary>
    public const int MaxWorkableFatigue = 90;

    private static readonly IReadOnlyDictionary<DutySlot, int> Capacities = new Dictionary<DutySlot, int>
    {
        [DutySlot.FieldHand] = 4,
        [DutySlot.DomesticServant] = 3,
        [DutySlot.Craftsman] = 2,
        [DutySlot.Cook] = 1,
        [DutySlot.Physician] = 1,
    };

    /// <summary>How many household members may simultaneously hold <paramref name="slot"/> — the
    /// "capacity" gate.</summary>
    public static int Capacity(DutySlot slot) =>
        Capacities.TryGetValue(slot, out var capacity)
            ? capacity
            : throw new ArgumentOutOfRangeException(nameof(slot), slot, "Unknown duty slot.");

    /// <summary>The <paramref name="skills"/> value <paramref name="slot"/> reads from.</summary>
    public static int RelevantSkillValue(LaborSkills skills, DutySlot slot) => slot switch
    {
        DutySlot.FieldHand => skills.Fieldwork,
        DutySlot.DomesticServant => skills.DomesticService,
        DutySlot.Craftsman => skills.Craft,
        DutySlot.Cook => skills.Culinary,
        DutySlot.Physician => skills.Medicine,
        _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, "Unknown duty slot."),
    };
}
