using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Actors;

/// <summary>
/// The shared Background↔Noteworthy promotion/demotion rule (Phase 10 item 3;
/// <c>gens-rival-houses-design.md</c> §2.3-§2.4): promote the instant real player contact occurs
/// (marriage candidate drawn from it, election opponent, Private Feud, contested plot, poached/poaching
/// Clientela, Ransom negotiation — enumerated in §2.3), and freeze a quiet <see
/// cref="LivingWorldActorTier.Noteworthy"/> actor back to <see cref="LivingWorldActorTier.Background"/>
/// once it has gone <see cref="LivingWorldActorTieringCatalog.DemotionQuietPeriodMonths"/> without a
/// fresh contact (§2.4). Written as a generic utility over <see cref="LivingWorldActor"/> rather than
/// rival-house-specific code, since §6 (Collegia, Foreign Peoples, Bandit Confederations, Religious
/// Institutions) explicitly reuses the identical pattern.
/// </summary>
public static class LivingWorldActorTieringService
{
    /// <summary>Records real player contact with <paramref name="actorId"/> and, if it is currently
    /// <see cref="LivingWorldActorTier.Background"/>, promotes it to <see
    /// cref="LivingWorldActorTier.Noteworthy"/> in the same step (§2.3). Idempotent to call again on an
    /// already-Noteworthy actor: it simply refreshes <see cref="LivingWorldActor.LastContactDate"/>,
    /// which is exactly what keeps a live thread from demoting under <see cref="DemoteIfQuiet"/>.</summary>
    public static LivingWorldActor RecordContactAndPromote(WorldState state, RuntimeId<Actor> actorId, GameDate contactDate)
    {
        var actor = GetOrThrow(state, actorId);
        var updated = actor with { Tier = LivingWorldActorTier.Noteworthy, LastContactDate = contactDate };
        Replace(state, actorId, updated);
        return updated;
    }

    /// <summary>Records real player contact with <paramref name="actorId"/> without changing its
    /// current tier. Exists separately from <see cref="RecordContactAndPromote"/> for a contact that
    /// does not itself justify promotion (e.g. background-tier ambient interaction that Phase 10
    /// package 5's abstract tick may generate) but should still reset the demotion clock.</summary>
    public static LivingWorldActor RecordContact(WorldState state, RuntimeId<Actor> actorId, GameDate contactDate)
    {
        var actor = GetOrThrow(state, actorId);
        var updated = actor with { LastContactDate = contactDate };
        Replace(state, actorId, updated);
        return updated;
    }

    /// <summary>Freezes <paramref name="actorId"/> back to <see cref="LivingWorldActorTier.Background"/>
    /// if it is currently <see cref="LivingWorldActorTier.Noteworthy"/> and has gone at least <see
    /// cref="LivingWorldActorTieringCatalog.DemotionQuietPeriodMonths"/> since its last recorded
    /// contact (or was never contacted at all) — §2.4's "no explicit downgrade action," just a
    /// last-known-state freeze: every other field is left exactly as it stood, including <see
    /// cref="LivingWorldActor.HeadCharacterId"/>, since nothing about a frozen entry is deleted, only
    /// no longer given extra simulation fidelity going forward. A no-op, returning the actor unchanged,
    /// when it is already <see cref="LivingWorldActorTier.Background"/> or still within the quiet
    /// window.</summary>
    public static LivingWorldActor DemoteIfQuiet(WorldState state, RuntimeId<Actor> actorId, GameDate currentDate)
    {
        var actor = GetOrThrow(state, actorId);
        if (actor.Tier != LivingWorldActorTier.Noteworthy)
            return actor;

        var monthsSinceContact = actor.LastContactDate is null
            ? int.MaxValue
            : currentDate.TotalMonths - actor.LastContactDate.Value.TotalMonths;

        if (monthsSinceContact < LivingWorldActorTieringCatalog.DemotionQuietPeriodMonths)
            return actor;

        var demoted = actor with { Tier = LivingWorldActorTier.Background };
        Replace(state, actorId, demoted);
        return demoted;
    }

    private static LivingWorldActor GetOrThrow(WorldState state, RuntimeId<Actor> actorId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        if (!state.Actors.TryGet(actorId, out var actor))
            throw new ArgumentException($"No LivingWorldActor with ID '{actorId}' is registered.", nameof(actorId));

        return actor!;
    }

    private static void Replace(WorldState state, RuntimeId<Actor> actorId, LivingWorldActor updated)
    {
        state.Actors.Remove(actorId);
        state.Actors.Add(actorId, updated);
    }
}
