using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Characters;
using Gens.Simulation.State;

namespace Gens.Simulation.Education;

/// <summary>
/// Applies a signed delta to one of a Character's five Core Attributes, clamped back into <see
/// cref="StatRange"/>'s 0-100 range before constructing the replacement <see cref="CoreAttributes"/> —
/// <see cref="CoreAttributes"/>'s own constructor throws outside that range rather than clamping (see
/// that type's own doc comment), so every caller mutating <see cref="Character.Attributes"/> must clamp
/// explicitly first, mirroring <see cref="Character.GetEffectiveAttributes"/>'s identical <see
/// cref="Math.Clamp"/> pattern. <see cref="EducationalTrackProgressSystem"/> is this codebase's first-ever
/// writer of <see cref="Character.Attributes"/> post-creation — no prior command or system has ever
/// mutated it, so this helper establishes the pattern rather than following an existing one.
/// </summary>
public static class EducationAttributeAdjustor
{
    public static Character Apply(Character character, PermanentInjuryTarget attribute, int delta)
    {
        if (character is null)
            throw new ArgumentNullException(nameof(character));

        var a = character.Attributes;
        var adjusted = attribute switch
        {
            PermanentInjuryTarget.Diplomacy => new CoreAttributes(Clamp(a.Diplomacy, delta), a.Martial, a.Stewardship, a.Intrigue, a.Learning),
            PermanentInjuryTarget.Martial => new CoreAttributes(a.Diplomacy, Clamp(a.Martial, delta), a.Stewardship, a.Intrigue, a.Learning),
            PermanentInjuryTarget.Stewardship => new CoreAttributes(a.Diplomacy, a.Martial, Clamp(a.Stewardship, delta), a.Intrigue, a.Learning),
            PermanentInjuryTarget.Intrigue => new CoreAttributes(a.Diplomacy, a.Martial, a.Stewardship, Clamp(a.Intrigue, delta), a.Learning),
            PermanentInjuryTarget.Learning => new CoreAttributes(a.Diplomacy, a.Martial, a.Stewardship, a.Intrigue, Clamp(a.Learning, delta)),
            _ => throw new ArgumentOutOfRangeException(nameof(attribute), attribute, "Not a Core Attribute target."),
        };

        return character with { Attributes = adjusted };
    }

    private static int Clamp(int current, int delta) => Math.Clamp(current + delta, 0, 100);
}
