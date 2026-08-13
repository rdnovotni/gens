namespace Gens.Simulation.Characters;

/// <summary>How a Character record first came to exist (<c>gens-characters-design.md</c> §14's
/// <c>source</c> enum), read alongside <see cref="Character.InstantiatedAtMonth"/>. <see
/// cref="Familia"/> is the one value <see cref="PromoteToNamedCommand"/> refuses (<see
/// cref="PromoteToNamedCommands.InvalidSource"/>): it means "born inside the simulation," which is
/// structurally different from converting an aggregate <see cref="PopGroup"/> entry — every other
/// value is a valid promotion trigger, matching <c>gens-settlement-demographics-design.md</c> §11's
/// trigger list (a Labor Duty Slot or Overseer post hire is <see cref="LaborDutyHire"/>, a Court
/// Position is <see cref="CourtPosition"/>, a marriage proposal targeting a named Curiales individual
/// is <see cref="MarriageProposal"/>, a Travel or Events encounter is <see cref="TravelEncounter"/> or
/// <see cref="EventEncounter"/>, and a direct Slave Market purchase from the Non-Household Enslaved
/// cohort is <see cref="SlaveMarketPurchase"/>).</summary>
public enum CharacterSource
{
    Familia,
    CourtPosition,
    CuriaSeat,
    RivalGenerated,
    TravelEncounter,
    EventEncounter,
    Guest,

    /// <summary>A deliberate hire into a Labor Duty Slot or Overseer post
    /// (<c>gens-settlement-demographics-design.md</c> §11).</summary>
    LaborDutyHire,

    /// <summary>A marriage proposal targeting a named Curiales individual (§11).</summary>
    MarriageProposal,

    /// <summary>A direct Slave Market purchase from the Non-Household Enslaved cohort (§11, §9).</summary>
    SlaveMarketPurchase,
}
