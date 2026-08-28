using Gens.Simulation.Characters;
using Gens.Simulation.Chronicle;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Actors;

/// <summary>Untuned baseline for <see cref="RivalDossierRefresh"/> (Phase 10 package 14) — the design
/// doc gives no sizing for how much dossier content to retain, matching this codebase's convention for
/// every other unsized number.</summary>
public static class RivalDossierCatalog
{
    /// <summary>How many <see cref="RivalDossier.RecentChronicleEntries"/> a refresh keeps — old
    /// entries fall off the front once this many accumulate, oldest first (append-and-trim, matching
    /// how a "recent" list reads everywhere else in this codebase).</summary>
    public const int MaxRecentChronicleEntries = 5;
}

/// <summary>
/// Refreshes a <see cref="RivalDossier"/> on genuine player contact (Phase 10 package 14;
/// <c>gens-rival-houses-design.md</c> §7's "Dossier isn't omnisciently live" staleness rule: refreshed
/// only "when new information actually reaches the player (contact, correspondence, a shared event)").
/// Reuses whatever narrative summary the triggering command/event already produced rather than
/// authoring new prose — <see cref="Stewardship.AutonomousDecisionLog.Outcome"/>'s identical "reuse the
/// projection's own summary" convention. <see cref="RivalDossier.LastUpdatedDate"/> never regresses:
/// an out-of-order or earlier-dated event replayed against an already-fresher dossier is a no-op.
/// Deliberately not a scheduled/monthly system — it is invoked directly from whichever command's own
/// mutate step already represents genuine contact (<see cref="AdjustHouseStandingCommand"/>, <see
/// cref="Interactions.SchemeProgressSystem"/>'s resolution), never from ambient background drift (<see
/// cref="BackgroundHouseDriftSystem"/> touches no dossier at all).
/// </summary>
public static class RivalDossierRefresh
{
    /// <summary>Refreshes <paramref name="actorId"/>'s dossier directly. Creates a fresh entry if none
    /// exists yet — first contact is exactly when a dossier should first appear (<see
    /// cref="RivalDossier"/>'s own "sparse: no entry until contact" doc comment).</summary>
    public static void Refresh(
        WorldState state, RuntimeId<Actor> actorId, GameDate eventDate, string summary,
        RuntimeId<ChronicleEntry>? chronicleEntryId = null)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (summary is null)
            throw new ArgumentNullException(nameof(summary));

        var hasExisting = state.RivalDossiers.TryGet(actorId, out var existing);
        if (hasExisting && existing!.LastUpdatedDate.TotalMonths >= eventDate.TotalMonths)
            return;

        var recentEntries = existing?.RecentChronicleEntries ?? Array.Empty<RuntimeId<ChronicleEntry>>();
        if (chronicleEntryId is { } entryId)
        {
            recentEntries = recentEntries
                .Append(entryId)
                .TakeLast(RivalDossierCatalog.MaxRecentChronicleEntries)
                .ToArray();
        }

        var updated = new RivalDossier(actorId, summary, existing?.HeadComboTitle, eventDate, recentEntries);

        if (hasExisting)
            state.RivalDossiers.Remove(actorId);
        state.RivalDossiers.Add(actorId, updated);
    }

    /// <summary>Resolves <paramref name="characterId"/> back to the <see cref="LivingWorldActor"/> it
    /// heads (if any — a player-household character resolves to nothing) and refreshes that actor's
    /// dossier. The only lookup direction <see cref="Interactions.Scheme"/> needs, since a Scheme is
    /// keyed by Character rather than Actor (<c>gens-characters-design.md</c> §10's own participant
    /// shape).</summary>
    public static void RefreshForCharacter(
        WorldState state, RuntimeId<Character> characterId, GameDate eventDate, string summary,
        RuntimeId<ChronicleEntry>? chronicleEntryId = null)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        foreach (var entry in state.Actors.InAscendingOrder())
        {
            if (entry.Value.HeadCharacterId == characterId)
            {
                Refresh(state, entry.Key, eventDate, summary, chronicleEntryId);
                return;
            }
        }
    }
}
