namespace Gens.Simulation.Characters;

/// <summary>How a <see cref="Relationship"/> first came to exist — its provenance
/// (Phase 5 item 5), mirroring how <see cref="CharacterSource"/> records provenance for a Character
/// itself. Set once, from the interaction that first creates the record, and never overwritten by a
/// later interaction between the same two Characters.</summary>
public enum RelationshipOrigin
{
    /// <summary>Born into the tie — parent/child, sibling, or a marriage's initial record.</summary>
    Family,

    /// <summary>Formed through shared household membership (fellow slaves, master and servant,
    /// companions under one roof) rather than blood or marriage.</summary>
    Household,

    /// <summary>Formed through a political tie — Patron/Client, Co-Magistrate, Faction alignment.</summary>
    Political,

    /// <summary>Formed through a commercial tie — Debtor/Creditor, a business partnership.</summary>
    Commercial,

    /// <summary>Formed through courtship or an affair (Romance &amp; Seduction).</summary>
    Romantic,

    /// <summary>Deliberately brokered by a third party (<c>gens-characters-design.md</c> §9's
    /// "Introduce" action: "Broker a new relationship-web link between two others").</summary>
    Introduction,

    /// <summary>Formed through an unplanned meeting — a travel encounter, a chance meeting at a
    /// public event.</summary>
    Encounter,

    /// <summary>Formed by a system-originated process with no single in-fiction moment of first
    /// contact (e.g. a rival candidate generated already opposed to the player).</summary>
    SystemGenerated,
}
