namespace Gens.Simulation.Wanderers;

/// <summary>Where a tracked <see cref="Wanderer"/> stands relative to the §6 engagement and §7
/// competition mechanics. <see cref="Wandering"/> is the only status in which a Wanderer still advances
/// their own Itinerary (§3) and is still engageable at all.</summary>
public enum WandererStatus
{
    /// <summary>Independent and moving on their own Itinerary (§3) — Hostable and Recruitable by
    /// whichever household commits first (§7).</summary>
    Wandering,

    /// <summary>Recruited outright into a household (§6's Recruit), converted to a real <see
    /// cref="Characters.Character"/> and no longer independent — "a successful Recruit ends that
    /// Wanderer's own independent Itinerary entirely" (§6), realized as <see
    /// cref="Wanderer.IsActivelyTracked"/> going false and <see cref="Wanderer.Itinerary"/> being
    /// cleared by <see cref="RecruitWandererCommands"/>.</summary>
    Recruited,
}
