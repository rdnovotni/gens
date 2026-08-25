namespace Gens.Simulation.Actors;

/// <summary>Numeric constants for <see cref="RivalAmbitionSystem"/> (Phase 10 item 4;
/// <c>gens-characters-design.md</c> §8.3: "Ambition + Boldness + Vengefulness axes let ANY Character
/// initiate an interaction unprompted"). Neither that section nor <c>gens-rival-houses-design.md</c>
/// §10's Open Questions size how often a Noteworthy head actually acts; this catalog is where that
/// original engineering choice lives, matching <see cref="LivingWorldActorTieringCatalog"/>'s
/// identical convention.</summary>
public static class RivalAmbitionCatalog
{
    /// <summary>Percent chance (0-100) a Noteworthy head considers acting at all this month, before
    /// Ambition/Boldness scaling — most months, most heads do nothing.</summary>
    public const int BaseActChancePercent = 10;

    /// <summary>How much of <see cref="Characters.Condition.Ambition"/>'s 0-100 range folds into the
    /// act-chance roll, in percentage points at Ambition's maximum (e.g. 30 means a maximally Ambitious
    /// head's chance is up to 30 points higher than <see cref="BaseActChancePercent"/> alone).</summary>
    public const int AmbitionWeightPercent = 30;

    /// <summary>How much of the head's Boldness <see cref="Characters.PersonalityAxis"/> score (rescaled
    /// from its native -100..100 range to 0..100) folds into the act-chance roll, in the same units as
    /// <see cref="AmbitionWeightPercent"/>.</summary>
    public const int BoldnessWeightPercent = 20;
}
