using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Time;

namespace Gens.Simulation.Romance;

/// <summary>The five shapes a <see cref="RomanticBond"/> can take (<c>gens-romance-sexuality-lineage-design.md</c>
/// §4-§6, §11, §16's data-model sketch). A single Character may simultaneously hold any number of
/// independent bonds of any type with different partners — §17's Open Questions settles this as
/// "allowed and unconstrained," with no jealousy/exclusivity mechanic — so a second bond forming
/// while one partner already holds a <see cref="Marriage"/> elsewhere is exactly what routes that
/// second bond into <see cref="Affair"/> territory, the intended outcome rather than a blocked
/// one.</summary>
public enum RomanticBondType
{
    /// <summary>An active, not-yet-formalized pursuit (§4).</summary>
    Courtship,

    /// <summary>Flavor/tracking data alongside a formal marriage — see this type's own doc comment
    /// for why this is not the source of truth for "is married."</summary>
    Marriage,

    /// <summary>The real, legally-recognized "middle ground" short of marriage (§6).</summary>
    Concubinage,

    /// <summary>An undiscovered or discovered extramarital bond (§11).</summary>
    Affair,

    /// <summary>A bond that has cooled to nothing, or been formally ended, but is kept on record
    /// rather than deleted — see <see cref="RomanticBondDecaySystem"/>'s own doc comment for why this
    /// value exists at all.</summary>
    PastRelationship,
}

/// <summary>
/// One <see cref="Romance.RomanticBondKey"/>-keyed pairing's tracked Affection and Attraction
/// (<c>gens-romance-sexuality-lineage-design.md</c> §3, §16): "a marriage of convenience with real
/// Affection but no Attraction (or the reverse) reads as a genuinely different story than one flat
/// number could tell." Follows <see cref="Characters.Relationship"/>'s exact shape — readonly record
/// struct, constructor-validated ranges, lazily created on first meaningful contact rather than
/// pre-allocated for every possible pair.
///
/// §16's data-model sketch also lists <c>isSameSex</c> and <c>powerImbalanced</c> fields; neither is
/// stored here. <c>IsSameSex</c> is pure flavor/reporting, cheaply derived at read time from both
/// Characters' <see cref="Characters.Sex"/> rather than duplicated as stored state that could drift
/// out of sync with it. <c>PowerImbalanced</c> is stronger than merely undesirable to store: it is
/// structurally impossible for a real <see cref="RomanticBond"/> to exist in that state at all, because
/// <see cref="Romance.RomanceEligibility.CheckPair"/> gates every single mutation point this type has
/// (<see cref="RecordRomanticInteractionCommand"/> included) — a bond reaching a power-imbalanced pair
/// is a bug for <see cref="RomanticBondReferentialIntegrityCheck"/>-style invariants to catch, not a
/// state this type needs a field to represent.
///
/// A <see cref="RomanticBondType.Marriage"/>-typed bond is flavor/tracking data layered on top of the
/// real source of truth for "is this Character married" — <see cref="Characters.Character.MaritalHistory"/>
/// (via <see cref="Characters.RecordMarriageCommand"/>) — not a replacement for it. Not every marriage
/// necessarily has a corresponding <see cref="RomanticBond"/> record (one could in principle be formed
/// through a path that never calls <see cref="RecordRomanticInteractionCommand"/>), and no code in this
/// module ever reads <see cref="BondType"/> to answer "is this pair married" — it reads
/// <see cref="Characters.Character.MaritalHistory"/> instead.
///
/// <i>Manus vs. sine manu</i> (§4.1's "what actually made a marriage" distinction) is explicit,
/// out-of-scope future Familia-only work — this type has no field for it, and nothing in this Romance
/// module derives it.
/// </summary>
public readonly record struct RomanticBond
{
    public const int MinScore = 0;
    public const int MaxScore = 100;

    public RomanticBond(
        RomanticBondType bondType,
        int affection,
        int attraction,
        bool isKnownPublicly,
        int discoveryRisk,
        GameDate formedDate,
        GameDate lastMeaningfulInteractionDate,
        string? provenanceEventId)
    {
        if (affection is < MinScore or > MaxScore)
            throw new ArgumentOutOfRangeException(
                nameof(affection), affection, $"Affection must be between {MinScore} and {MaxScore}.");
        if (attraction is < MinScore or > MaxScore)
            throw new ArgumentOutOfRangeException(
                nameof(attraction), attraction, $"Attraction must be between {MinScore} and {MaxScore}.");
        if (discoveryRisk is < MinScore or > MaxScore)
            throw new ArgumentOutOfRangeException(
                nameof(discoveryRisk), discoveryRisk, $"Discovery risk must be between {MinScore} and {MaxScore}.");
        if (lastMeaningfulInteractionDate.TotalMonths < formedDate.TotalMonths)
            throw new ArgumentException(
                "A romantic bond's last meaningful interaction cannot predate when it formed.",
                nameof(lastMeaningfulInteractionDate));

        BondType = bondType;
        Affection = affection;
        Attraction = attraction;
        IsKnownPublicly = isKnownPublicly;
        DiscoveryRisk = discoveryRisk;
        FormedDate = formedDate;
        LastMeaningfulInteractionDate = lastMeaningfulInteractionDate;
        ProvenanceEventId = provenanceEventId;
    }

    /// <summary>Which of the five shapes (§4-§6, §11) this bond currently is.</summary>
    public RomanticBondType BondType { get; }

    /// <summary>0 (none) to 100 (devoted): the emotional bond — warmth, genuine care, trust. Distinct
    /// from ordinary relationship-web <see cref="Characters.Relationship.Opinion"/>, which can be high
    /// purely from political utility with no romantic warmth behind it at all (§3).</summary>
    public int Affection { get; }

    /// <summary>0 (none) to 100 (intense): physical and romantic desire, independent of
    /// <see cref="Affection"/> (§3) — a passionate but emotionally shallow affair runs on high
    /// Attraction and low Affection; a beloved, trusted companion the initiator simply isn't drawn to
    /// runs the other way.</summary>
    public int Attraction { get; }

    /// <summary>Whether this bond is openly acknowledged rather than hidden. Always <c>true</c> at
    /// creation for a <see cref="RomanticBondType.Concubinage"/> (§6's "publicly acknowledged, unlike
    /// an affair"); meaningfully <c>false</c> for an undiscovered <see
    /// cref="RomanticBondType.Affair"/> (§11).</summary>
    public bool IsKnownPublicly { get; }

    /// <summary>0-100: how close an undiscovered <see cref="RomanticBondType.Affair"/> is to being
    /// found out (§11). Meaningless — always 0 and never advanced — for any other
    /// <see cref="BondType"/> or once <see cref="IsKnownPublicly"/> is true.</summary>
    public int DiscoveryRisk { get; }

    /// <summary>The tick this bond record was first created.</summary>
    public GameDate FormedDate { get; }

    /// <summary>The tick of the most recent interaction judged meaningful enough to matter — <see
    /// cref="RomanticBondDecaySystem"/> reads this the same way <see
    /// cref="Characters.RelationshipDecaySystem"/> reads its own equivalent field, and never decays a
    /// bond in the same tick it was just touched.</summary>
    public GameDate LastMeaningfulInteractionDate { get; }

    /// <summary>The tagged ID of the domain event that most recently produced this exact record,
    /// mirroring <see cref="Characters.Relationship.ProvenanceEventId"/>'s identical role. Null only
    /// for a record restored from a save written before this field existed.</summary>
    public string? ProvenanceEventId { get; }
}
