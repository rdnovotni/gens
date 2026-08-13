namespace Gens.Simulation.Characters;

/// <summary>The relationship-web bond vocabulary (<c>gens-familia-design.md</c> §2.7,
/// <c>gens-characters-design.md</c> §7): family relations (structural, persist regardless of
/// opinion), earned bonds, and the mechanical-hook tags later systems attach. A single directed <see
/// cref="Relationship"/> can carry more than one bond simultaneously (e.g. a Patron who is also a
/// Friend), so this is a <see cref="FlagsAttribute"/> enum rather than a single-value one.</summary>
[Flags]
public enum BondTag
{
    None = 0,

    // Family relations — structural, not opinion-based (§2.7). These mirror facts already recorded
    // elsewhere on Character (MotherId/FatherId, MaritalHistory) but are tracked here too so every
    // dyadic tie — family or otherwise — reads through one uniform relationship-web lookup.
    Parent = 1 << 0,
    Child = 1 << 1,
    Sibling = 1 << 2,
    Spouse = 1 << 3,

    // Earned bonds (§2.7).
    Friend = 1 << 4,
    Rival = 1 << 5,
    Lover = 1 << 6,
    Mentor = 1 << 7,
    Student = 1 << 8,

    /// <summary>The informal, legally-unrecognized union standing in for marriage among the enslaved
    /// (<c>gens-labor-slavery-design.md</c> §9), tracked the same way as any other bond but without a
    /// formal <see cref="Spouse"/> tag's legal weight.</summary>
    Contubernium = 1 << 9,

    /// <summary>An escalated <see cref="Rival"/>, past the point of ordinary political friction
    /// (<c>gens-characters-design.md</c> §7; Politics &amp; Patronage §9).</summary>
    Nemesis = 1 << 10,

    Patron = 1 << 11,
    Client = 1 << 12,

    /// <summary>The relationship-web mirror of an active Economy &amp; Finance <c>DebtRecord</c>
    /// (<c>gens-characters-design.md</c> §7).</summary>
    Debtor = 1 << 13,
    Creditor = 1 << 14,

    /// <summary>The Duumvir colleague relationship (<c>gens-characters-design.md</c> §7; Politics &amp;
    /// Patronage §5.4).</summary>
    CoMagistrate = 1 << 15,

    /// <summary>One party holds damaging material on the other — a standing, usable threat rather
    /// than just poor opinion (<c>gens-characters-design.md</c> §7; Espionage's forward hook).</summary>
    BlackmailLeverage = 1 << 16,
}
