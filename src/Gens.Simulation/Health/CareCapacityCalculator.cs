namespace Gens.Simulation.Health;

/// <summary>Pure math for how many standing <see cref="CharacterHealthCondition"/> cases one
/// household's Physician (<c>Characters.DutySlot.Physician</c>) can actually treat in a month — Phase
/// 14 item 1's "care capacity" facet: treatment is a bounded resource, not an unlimited service every
/// afflicted household member automatically receives. No numeric caseload exists anywhere in the design
/// corpus, so this implementation invents a modest, skill-scaling cap, deliberately small enough that a
/// household with several simultaneous cases and only one Physician still has to make a real
/// prioritization choice (<see cref="Health.CharacterHealthConditionSystem"/> treats the earliest-onset
/// cases first).</summary>
public static class CareCapacityCalculator
{
    /// <summary>Cases per month one household's Physician can treat, from that Physician's effective
    /// Medicine skill (0 with no Physician assigned at all — <c>Characters.Character.GetEffectiveSkills</c>'s
    /// already injury-adjusted Medicine value).</summary>
    public static int MonthlyCareCapacity(int physicianMedicineSkill)
    {
        if (physicianMedicineSkill <= 0)
            return 0;
        return Math.Clamp(1 + physicianMedicineSkill / 25, 1, 5);
    }
}
