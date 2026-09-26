using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Commands;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Romance;

/// <summary>
/// The monthly Affection/Attraction decay tick, mirroring <see
/// cref="Characters.RelationshipDecaySystem"/>'s shape exactly: every tick a <see
/// cref="RomanticBond"/> goes untouched, both its <see cref="RomanticBond.Affection"/> and <see
/// cref="RomanticBond.Attraction"/> drift one point toward zero — the same invented "use it or lose
/// it" baseline, disclaimed the same way (see <see cref="RomanceCatalog"/>'s own disclaimer). <see
/// cref="RomanticBondType.Marriage"/> and <see cref="RomanticBondType.Concubinage"/> are sticky,
/// exempt from decay entirely, matching <see cref="Characters.RelationshipDecaySystem"/>'s own
/// sustained-commitment exemption for structural bonds.
///
/// Where this system deliberately diverges from its model: <see
/// cref="Characters.RelationshipDecaySystem"/> prunes a fully-decayed record outright. This system
/// does not, for a <see cref="RomanticBondType.Courtship"/>, <see cref="RomanticBondType.Affair"/>, or
/// <see cref="RomanticBondType.PastRelationship"/> bond that decays all the way to (0, 0) — instead of
/// deleting it, a <see cref="RomanticBondType.Courtship"/> or <see cref="RomanticBondType.Affair"/> is
/// retagged <see cref="RomanticBondType.PastRelationship"/>, so a cooled-off courtship or a spent
/// affair is distinguishable, on record, from a pair that never had any romantic history at all — a
/// distinction this implementation judges worth the extra sparse-store entries. Only a bond already
/// tagged <see cref="RomanticBondType.PastRelationship"/> that decays (or has already decayed) to (0,
/// 0) is pruned outright, so the store does not accumulate every spent romance forever once it has
/// nothing left even the retag needs to preserve.
/// </summary>
public sealed class RomanticBondDecaySystem : IMonthlySystem<WorldState>
{
    /// <summary>Affection/Attraction points moved toward zero per month a bond goes without a
    /// meaningful interaction — <see cref="RomanceCatalog.MonthlyAffectionAttractionDecay"/>.</summary>
    private const int MonthlyDecay = RomanceCatalog.MonthlyAffectionAttractionDecay;

    public string Id => "romance.romanticBondDecay";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "romanticBonds" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "romanticBonds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        // Materialize first: the loop body mutates state.RomanticBonds (Remove, and possibly Add)
        // mid-iteration, matching RelationshipDecaySystem's identical "snapshot before mutating" guard
        // against invalidating the enumerator.
        var bonds = state.RomanticBonds.InAscendingOrder().ToArray();

        foreach (var entry in bonds)
        {
            var key = entry.Key;
            var bond = entry.Value;

            // Just interacted this same tick, or the bond was formed this same tick — nothing to
            // decay yet.
            if (bond.LastMeaningfulInteractionDate.TotalMonths >= context.Date.TotalMonths)
                continue;

            if (bond.BondType is RomanticBondType.Marriage or RomanticBondType.Concubinage)
                continue;

            if (bond.Affection == 0 && bond.Attraction == 0)
            {
                if (bond.BondType == RomanticBondType.PastRelationship)
                    state.RomanticBonds.Remove(key);
                continue;
            }

            var decayedAffection = bond.Affection > 0 ? Math.Max(0, bond.Affection - MonthlyDecay) : bond.Affection;
            var decayedAttraction = bond.Attraction > 0 ? Math.Max(0, bond.Attraction - MonthlyDecay) : bond.Attraction;
            var decayedType =
                decayedAffection == 0 && decayedAttraction == 0
                && bond.BondType is RomanticBondType.Courtship or RomanticBondType.Affair or RomanticBondType.PastRelationship
                    ? RomanticBondType.PastRelationship
                    : bond.BondType;

            var decayed = new RomanticBond(
                decayedType, decayedAffection, decayedAttraction, bond.IsKnownPublicly, bond.DiscoveryRisk,
                bond.FormedDate, bond.LastMeaningfulInteractionDate, bond.ProvenanceEventId);
            state.RomanticBonds.Remove(key);
            state.RomanticBonds.Add(key, decayed);
        }

        // Decay is a silent background drift, not an event-worthy occurrence — matching
        // RelationshipDecaySystem's identical choice to emit nothing for its own equivalent tick.
        return Array.Empty<IDomainEvent>();
    }
}
