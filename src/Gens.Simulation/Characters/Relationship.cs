using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>
/// One directed tie in the relationship web (Phase 5 item 5; <c>gens-familia-design.md</c> §2.7,
/// <c>gens-characters-design.md</c> §7): "opinion (-100 to 100) plus bond tags, between any two named
/// individuals." A <see cref="Character"/> holding a <see cref="Relationship"/> toward another does
/// not imply the reverse holds any record at all, let alone an equal one — <c>A</c>'s opinion of
/// <c>B</c> and <c>B</c>'s opinion of <c>A</c> are two independent records, keyed by direction (<see
/// cref="RelationshipKey"/>), each created only once something actually needs recording. That's the
/// "sparse" half of "sparse directed records": most Character pairs in a campaign never interact at
/// all and so never get an entry, rather than every pair pre-allocating a mutual, symmetric slot.
/// </summary>
public readonly record struct Relationship
{
    public const int MinOpinion = -100;
    public const int MaxOpinion = 100;

    public Relationship(
        int opinion,
        BondTag bonds,
        RelationshipOrigin origin,
        GameDate formedDate,
        GameDate lastMeaningfulInteractionDate,
        string? provenanceEventId)
    {
        if (opinion is < MinOpinion or > MaxOpinion)
            throw new ArgumentOutOfRangeException(
                nameof(opinion), opinion, $"Opinion must be between {MinOpinion} and {MaxOpinion}.");
        if (lastMeaningfulInteractionDate.TotalMonths < formedDate.TotalMonths)
            throw new ArgumentException(
                "A relationship's last meaningful interaction cannot predate when it formed.",
                nameof(lastMeaningfulInteractionDate));

        Opinion = opinion;
        Bonds = bonds;
        Origin = origin;
        FormedDate = formedDate;
        LastMeaningfulInteractionDate = lastMeaningfulInteractionDate;
        ProvenanceEventId = provenanceEventId;
    }

    /// <summary>-100 (hatred) to 100 (devotion); 0 is neutral/unknown (<c>gens-familia-design.md</c> §2.7).</summary>
    public int Opinion { get; }

    /// <summary>Zero or more simultaneously-held bond tags (<see cref="BondTag"/> is a flags enum).</summary>
    public BondTag Bonds { get; }

    /// <summary>How this tie first formed. Set once, at creation, and never changed by later
    /// interactions between the same pair.</summary>
    public RelationshipOrigin Origin { get; }

    /// <summary>The tick this record was first created.</summary>
    public GameDate FormedDate { get; }

    /// <summary>The tick of the most recent interaction judged meaningful enough to matter —
    /// <see cref="RelationshipDecaySystem"/> reads this to decide how long opinion has been drifting
    /// unattended, and never decays a relationship in the same tick it was just touched.</summary>
    public GameDate LastMeaningfulInteractionDate { get; }

    /// <summary>The tagged ID of the domain event that most recently produced this exact record —
    /// this type's provenance trail, mirroring <see cref="State.KnowledgeEntry.ProvenanceEventId"/>'s
    /// identical "which event do I trace back to" role. Null only for a record restored from a save
    /// written before this field existed.</summary>
    public string? ProvenanceEventId { get; }

    public bool HasBond(BondTag bond) => bond != BondTag.None && (Bonds & bond) == bond;

    /// <summary>Whether this record carries no meaningful state at all — neutral opinion and no
    /// bonds. <see cref="RelationshipDecaySystem"/> prunes a record once it decays into this state,
    /// keeping the store sparse rather than accumulating dead, fully-decayed entries forever.</summary>
    public bool IsEmpty => Opinion == 0 && Bonds == BondTag.None;
}
