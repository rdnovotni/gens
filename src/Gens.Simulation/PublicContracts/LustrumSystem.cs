using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Magistracies;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.PublicContracts;

/// <summary>
/// §3's Lustrum tick (Phase 15 item 6), matching <see
/// cref="BusinessCompetition.GrainHoardingResolutionSystem"/>'s and every other Phase 15 item 5 system's
/// identical static, unwired <c>Tick(state, date)</c> convention — not registered against <see
/// cref="Time.IMonthlySystem{TState}"/>'s central pipeline (no such central registry exists anywhere in
/// this codebase for any Phase 15 system to join; see that system's own doc comment) — a caller (a
/// future campaign host, or a test) invokes this once per month the same way it would any other
/// <see cref="Time.IMonthlySystem{TState}"/>.
///
/// <b>Net Worth reassessment (§3):</b> Economy &amp; Finance's own <see
/// cref="Economy.InsolvencySystem"/> already recomputes every tracked household's <see
/// cref="Economy.NetWorth"/> every month, not only at a Lustrum — this item does not invent a second,
/// redundant computation; it snapshots whichever households currently carry a <see
/// cref="WorldState.NetWorthAssessments"/> entry into this Lustrum's own <see
/// cref="LustrumEvent.HouseholdsReassessed"/> list, honoring §3's "a full Net Worth reassessment across
/// every tracked household" as a real, periodic civic record of an already-real figure rather than a
/// second reassessment mechanism.
///
/// <b>Mandatory re-bid (§3):</b> "a mandatory re-bidding of every standing contract... whether or not its
/// current holder is performing well" — every <see cref="PublicContractStatus.Awarded"/> contract is
/// reopened (holder cleared, status reset to <see cref="PublicContractStatus.OpenForBidding"/>, any
/// in-progress cutting-corners state cleared along with it) regardless of that holder's own performance,
/// exactly as §3 states; a fresh <see cref="AwardPublicContractCommand"/> round (with
/// <c>isLustrumRebid: true</c>) is left to the caller, matching <see
/// cref="OpenPublicContractCommand"/>'s own "this command opens/reopens, awarding is a separate,
/// caller-driven step" shape.
///
/// <b>Censor term (§2/§3):</b> "duration = the census itself, not a fixed annual term" — every currently
/// active <see cref="MagistracyOffice.Censor"/> record ends here, at the Lustrum that concludes their
/// census, rather than at <see cref="MagistracyCatalog.TermLengthMonths"/>'s ordinary annual boundary
/// (<see cref="MagistracyTermSystem"/> excludes this office from that check for exactly this
/// reason). Re-filling the now-vacant Censorship is <see cref="ElectCensorsCommand"/>'s own job, not
/// automated here — matching §2's own "a Censor election (if the office is vacant or contested)"
/// phrasing, which names the Lustrum as the moment an election becomes due, not a caller-less
/// auto-election this item cannot generate a real rival candidate for (§9's own open Rival-AI-depth
/// question).
/// </summary>
public static class LustrumSystem
{
    public static bool IsLustrumMonth(GameDate date) =>
        date.TotalMonths > 0 && date.TotalMonths % PublicContractsCatalog.LustrumIntervalMonths == 0;

    public static IReadOnlyList<IDomainEvent> Tick(WorldState state, GameDate date)
    {
        if (!IsLustrumMonth(date))
            return Array.Empty<IDomainEvent>();

        var events = new List<IDomainEvent>();

        var householdsReassessed = state.NetWorthAssessments.InAscendingOrder().Select(entry => entry.Key).ToArray();

        var reopened = new List<RuntimeId<PublicContract>>();
        foreach (var entry in state.PublicContracts.InAscendingOrder().ToArray())
        {
            var contract = entry.Value;
            if (contract.Status != PublicContractStatus.Awarded)
                continue;

            state.PublicContracts.Remove(entry.Key);
            state.PublicContracts.Add(
                entry.Key,
                contract with
                {
                    Status = PublicContractStatus.OpenForBidding,
                    CurrentHolder = null,
                    ContractValue = Money.Zero,
                    AwardedDate = null,
                    AwardedViaLustrum = false,
                    IsCuttingCorners = false,
                    FraudDiscovered = false,
                    FraudDiscoveryRisk = 0,
                });
            reopened.Add(entry.Key);
        }

        foreach (var entry in state.MagistracyRecords.InAscendingOrder().ToArray())
        {
            var record = entry.Value;
            if (record.Office != MagistracyOffice.Censor || !MagistracyResolver.IsActive(record))
                continue;

            state.MagistracyRecords.Remove(entry.Key);
            state.MagistracyRecords.Add(entry.Key, record with { TermEndDate = date });
        }

        var lustrumId = state.LustrumEventIds.Issue();
        state.LustrumEvents.Add(lustrumId, new LustrumEvent(lustrumId, date, householdsReassessed, reopened));

        events.Add(new LustrumFiredEvent(state.EventIds.Issue(), date, lustrumId, householdsReassessed.Length, reopened.Count, CausationId: null));
        return events;
    }
}

public sealed record LustrumFiredEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<LustrumEvent> LustrumId,
    int HouseholdsReassessedCount,
    int ContractsReopenedCount,
    string? CausationId) : IDomainEvent
{
    public string Type => "publicContracts.lustrumFired";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { LustrumId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}
