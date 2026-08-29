using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Clientela;

/// <summary>
/// One Character's membership in a household's Clientela roster (Phase 12 item 2;
/// <c>gens-politics-patronage-design.md</c> §4.1, §11's <c>ClientelaRoster</c> sketch). Keyed by <see
/// cref="ClientId"/> rather than by (patron, client) pair — a client belongs to exactly one patron at a
/// time (§4.5's poaching flips this same entry to a new patron rather than creating a second one), so
/// <see cref="ClientId"/> alone is already a unique key, matching <see
/// cref="Reputation.HouseholdReputation"/>'s identical "the natural owning entity is already unique
/// enough" convention rather than inventing a composite key <see cref="RelationshipKey"/>-style.
///
/// Deliberately layered on top of, not a replacement for, Familia's own Patron/Client <see
/// cref="BondTag"/>: <see cref="RecruitClientCommand"/> writes both — this record for the
/// roster-specific bookkeeping (Specialty, favor cadence) §11's data model calls for, and a real <see
/// cref="Relationship"/> entry (via a direct write mirroring <see
/// cref="RecordInteractionCommand"/>'s shape) for the opinion/bond half every other relationship-driven
/// system already reads.
/// </summary>
/// <param name="LastFavorCalledDate">Null until <see cref="CallInClientFavorCommand"/> is used for the
/// first time — a client recruited but never yet drawn on. <see cref="ClientPoachingSystem"/> reads
/// "recruited but never used" as no different from "long overdue," per §4.5's own "favors keep going
/// unrewarded" framing.</param>
public sealed record ClientelaEntry(
    RuntimeId<Character> ClientId,
    RuntimeId<Household> PatronHouseholdId,
    ClientSpecialty Specialty,
    GameDate RecruitedDate,
    GameDate? LastFavorCalledDate = null);

/// <summary>Read-side helpers over <see cref="WorldState.ClientelaEntries"/>. <see cref="ClientsOf"/>
/// is a linear scan rather than a maintained per-patron index — Clientela rosters are, per §4's own
/// framing, a small, hand-curated list (not a population-scale collection the way <see
/// cref="PopGroup"/> is), so the same "no secondary index needed yet" judgment call <see
/// cref="Actors.RivalAmbitionSystem.FindCandidateTargets"/> already makes for house-standing pairs
/// applies here too.</summary>
public static class ClientelaResolver
{
    public static bool TryGetClient(WorldState state, RuntimeId<Character> clientId, out ClientelaEntry entry) =>
        state.ClientelaEntries.TryGet(clientId, out entry);

    public static IReadOnlyList<ClientelaEntry> ClientsOf(WorldState state, RuntimeId<Household> patronHouseholdId)
    {
        var clients = new List<ClientelaEntry>();
        foreach (var entry in state.ClientelaEntries.InAscendingOrder())
            if (entry.Value.PatronHouseholdId == patronHouseholdId)
                clients.Add(entry.Value);
        return clients;
    }

    /// <summary>Every distinct patron household currently holding at least one client, in ascending
    /// household-<see cref="RuntimeId{T}"/> order (ADR 0004) — the deterministic iteration set <see
    /// cref="SalutatioSystem"/> and <see cref="InfluenceCycleSystem"/> both scan each month, rather
    /// than each re-deriving its own.</summary>
    public static IReadOnlyList<RuntimeId<Household>> PatronsWithClients(WorldState state)
    {
        var patrons = new SortedSet<RuntimeId<Household>>();
        foreach (var entry in state.ClientelaEntries.InAscendingOrder())
            patrons.Add(entry.Value.PatronHouseholdId);
        return patrons.ToArray();
    }
}
