using Gens.Simulation.Characters;
using Gens.Simulation.Clientela;
using Gens.Simulation.Commands;
using Gens.Simulation.Edicts;
using Gens.Simulation.Identity;
using Gens.Simulation.Policies;
using Gens.Simulation.Religion;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Doctrine;

/// <summary>Emitted whenever <see cref="DoctrineResolutionSystem"/> actually moves a household's <see
/// cref="DoctrineTier"/> across a threshold (either direction) — a quiet monthly Affinity nudge that
/// doesn't cross a threshold produces no event, matching <see
/// cref="Clientela.InfluenceCycleSystem"/>'s own "a quiet resource trickle... nothing downstream needs
/// a per-tick event" precedent for the ordinary case, while a real tier change is exactly the kind of
/// fact §3.1/§7 call "Chronicle-worthy" and "regional recognition," so it is public.</summary>
public sealed record DoctrineTierChangedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    HouseholdDoctrineType DoctrineType,
    DoctrineTier PreviousTier,
    DoctrineTier NewTier) : IDomainEvent
{
    public string Type => "doctrine.tierChanged";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public string? CausationId => null;
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// The monthly Affinity read-and-resolve tick for §3.1's three real, reachable Doctrines (Phase 12 item
/// 9) — "each of the seven Doctrines carries its own hidden Affinity score (0-100), read monthly
/// against the household's actual Standing Policy settings and Edict history." Runs in <see
/// cref="TickPhase.RelationshipsActors"/>, the same phase every other actor-standing/reputation system
/// in this codebase runs in (<see cref="Reputation.FavorExpirationSystem"/>, <see
/// cref="Scandal.ScandalDecaySystem"/>).
///
/// Iterates <see cref="WorldState.HouseholdHeadships"/> as its own real, already-populated "every
/// household that actually exists" enumeration — the same set <see
/// cref="Scandal.RecordScandalCommand"/> already resolves a recorded head against.
///
/// <b>Only three of the seven Doctrines ever move off <see cref="DoctrineTier.None"/> here</b> — see
/// <see cref="HouseholdDoctrineType"/>'s own doc comment for exactly which unbuilt Standing Policies the
/// other four would need. Each real Doctrine's own signal is a small, explicit sum of real,
/// already-shipped facts (never a stand-in field): a positive point per matching condition, a negative
/// point per actively contradicting one, per §3.1's "matching choices raise Affinity; contradicting
/// choices lower it."
///
/// <list type="bullet">
/// <item><b>Mos Maiorum</b> (§3.2): a Lavish Rites Budget (<see cref="RitesBudgetCatalog"/>, Phase 9
/// item 2/Phase 12 item 3) and a Traditionalist <see cref="CharacterFactionAlignment"/> on the
/// household's recorded head (Phase 12 item 2, §3.1) — both real, reachable signals; a Frugal budget or
/// a Popularist lean each contradict. §3.2's own further two conditions (sustained Sumptuary
/// enforcement, Volunteer-Only recruitment) are left out entirely rather than faked: neither the
/// Sumptuary Edict Standing Policy (§2.4) nor Recruitment Doctrine's own Source Doctrine dial (§2.5) has
/// any real stored state anywhere in this codebase to read (confirmed by direct search — the only
/// pre-existing Policies partition is <see cref="HouseholdPolicyState.RitesBudget"/>), so this item
/// reads the two conditions it actually can rather than inventing the other two's storage just to
/// complete a four-part formula.</item>
/// <item><b>Domus Pia</b> (§3.2): the same Rites Budget signal, plus the household head carrying <see
/// cref="ReligionCatalog.DevoutTraitId"/>/<see cref="ReligionCatalog.ZealousTraitId"/> and the head
/// holding any active <see cref="Religion.PriesthoodRecord"/> (Phase 12 item 3) — §3.2's own "frequent
/// funded Festivals" condition is left out: no per-household "how many Festivals funded recently"
/// counter exists anywhere (<see cref="Religion.FundFestivalCelebrationCommand"/> posts a Ledger spend
/// and a Favor/Dignitas payoff directly but keeps no frequency ledger of its own), and inventing one
/// solely to feed this formula would be building that counter's own future item early.</item>
/// <item><b>Domus Dura</b> (§3.2): the household-wide <see cref="RegimenSettings"/> default (<see
/// cref="WorldState.HouseholdRegimenDefaults"/> at <see cref="HouseholdRegimenKey.Slot"/> null — Phase
/// 6's own household-level Regimen posture, which already is §2.2's "Household Regimen Posture" Standing
/// Policy in practice) reading Harsh Discipline/Confined Freedoms as a match and Lenient/Free Movement as
/// a contradiction, plus a real, heavily-weighted signal for any <see
/// cref="EdictResolver.HasIssuedProscription"/> — §3.2's own "at least one Proscription issued," which
/// this item's own <see cref="IssueProscriptionCommand"/> makes real and reachable. §3.2's own
/// Slave-Militia Reliance condition is left out for the identical unbuilt-Recruitment-Doctrine reason
/// named above for Mos Maiorum.</item>
/// </list>
/// </summary>
public sealed class DoctrineResolutionSystem : IMonthlySystem<WorldState>
{
    public string Id => "doctrine.resolution";
    public TickPhase Phase => TickPhase.RelationshipsActors;

    public IReadOnlyCollection<string> Reads { get; } = new[]
    {
        "householdHeadships", "householdPolicies", "characterFactionAlignments", "characters",
        "priesthoodRecords", "householdRegimenDefaults", "edictRecords", "householdDoctrines",
    };

    public IReadOnlyCollection<string> Writes { get; } = new[] { "householdDoctrines", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var entry in state.HouseholdHeadships.InAscendingOrder())
        {
            var householdId = entry.Key;
            var headId = entry.Value.HeadCharacterId;

            Resolve(state, context, householdId, HouseholdDoctrineType.MosMaiorum, MosMaiorumSignal(state, headId), events);
            Resolve(state, context, householdId, HouseholdDoctrineType.DomusPia, DomusPiaSignal(state, headId), events);
            Resolve(state, context, householdId, HouseholdDoctrineType.DomusDura, DomusDuraSignal(state, householdId), events);
        }

        return events;
    }

    private static void Resolve(
        WorldState state, MonthlyTickContext context, RuntimeId<Household> householdId, HouseholdDoctrineType type,
        int signal, List<IDomainEvent> events)
    {
        var current = HouseholdDoctrineResolver.Current(state, householdId, type);

        var delta = signal switch
        {
            > 0 => signal * DoctrineCatalog.MatchGainPerMonth,
            < 0 => signal * DoctrineCatalog.MismatchLossPerMonth,
            _ => current.AffinityScore > 0 ? -DoctrineCatalog.UnfedDecayPerMonth : 0,
        };

        var nextAffinity = Math.Clamp(current.AffinityScore + delta, 0, 100);
        var nextTier = nextAffinity >= DoctrineCatalog.DefiningThreshold ? DoctrineTier.Defining
            : nextAffinity >= DoctrineCatalog.EmergingThreshold ? DoctrineTier.Emerging
            : DoctrineTier.None;

        var capstoneUnlocked = current.CapstoneUnlocked || nextTier == DoctrineTier.Defining;

        HouseholdDoctrineResolver.Set(
            state,
            current with { AffinityScore = nextAffinity, Tier = nextTier, CapstoneUnlocked = capstoneUnlocked });

        if (nextTier != current.Tier)
        {
            events.Add(new DoctrineTierChangedEvent(
                state.EventIds.Issue(), context.Date, householdId, type, current.Tier, nextTier));
        }
    }

    private static int MosMaiorumSignal(WorldState state, RuntimeId<Character> headId)
    {
        var signal = 0;
        signal += RitesBudgetSignal(state, headId);
        var faction = CharacterFactionResolver.Current(state, headId);
        signal += faction switch
        {
            PoliticalFaction.Traditionalist => 1,
            PoliticalFaction.Popularist => -1,
            _ => 0,
        };
        return signal;
    }

    private static int DomusPiaSignal(WorldState state, RuntimeId<Character> headId)
    {
        var signal = RitesBudgetSignal(state, headId);

        if (state.Characters.TryGet(headId, out var head) && head is not null &&
            (head.Traits.Contains(ReligionCatalog.DevoutTraitId) || head.Traits.Contains(ReligionCatalog.ZealousTraitId)))
            signal += 1;

        if (PriesthoodResolver.AnyActiveRecordFor(state, headId) is not null)
            signal += 1;

        return signal;
    }

    private static int DomusDuraSignal(WorldState state, RuntimeId<Household> householdId)
    {
        var regimen = state.HouseholdRegimenDefaults.TryGet(new HouseholdRegimenKey(householdId, null), out var stored)
            ? stored
            : RegimenCatalog.Default;

        var signal = regimen.Discipline switch
        {
            DisciplineTier.Harsh => 1,
            DisciplineTier.Lenient => -1,
            _ => 0,
        };
        signal += regimen.Freedoms switch
        {
            FreedomsTier.Confined => 1,
            FreedomsTier.FreeMovement => -1,
            _ => 0,
        };
        if (EdictResolver.HasIssuedProscription(state, householdId))
            signal += 2;

        return signal;
    }

    /// <summary>This household's own Rites Budget signal — the Doctrine feed <see
    /// cref="MosMaiorumSignal"/> and <see cref="DomusPiaSignal"/> both share, since a household with no
    /// <see cref="HouseholdPolicyState"/> entry yet resolves to <see cref="RitesBudgetCatalog.Default"/>
    /// (Standard, matching <see cref="HouseholdPolicyResolver"/>'s own convention), which is neutral —
    /// this reads the resolver, not the household id directly against the household's headship, since
    /// Rites Budget itself is keyed by household.</summary>
    private static int RitesBudgetSignal(WorldState state, RuntimeId<Character> headId)
    {
        if (!state.Characters.TryGet(headId, out var head) || head?.Household is not { } householdId)
            return 0;

        return HouseholdPolicyResolver.GetEffectiveRitesBudget(state, householdId) switch
        {
            RitesBudgetTier.Lavish => 1,
            RitesBudgetTier.Frugal => -1,
            _ => 0,
        };
    }
}
