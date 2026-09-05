using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.MerchantFamilies;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.PublicWorks;

/// <summary>
/// §2/§9's <c>EuergetismObligation</c> data model (Phase 15 item 9) — the quiet, ongoing social pressure
/// a sufficiently wealthy household carries, distinct from ordinary Politics &amp; Patronage standing.
/// §9's own <c>prominenceTier</c> field (Events §5's Prominence) is confirmed, by direct search, not to
/// exist anywhere in this codebase (the same gap <see cref="Scandal.ScandalScope"/>'s own doc comment
/// already names), so this item reads the identical real, checkable proxy <see
/// cref="MerchantFamilies.EquestrianStatusQuery"/> already established instead — <see
/// cref="PublicWorksCatalog.ObligationNetWorthThreshold"/> against a household's own tracked Net Worth —
/// and this record carries no separate <c>prominenceTier</c> field of its own, only the real dates and
/// counts this item actually computes from.
/// </summary>
public sealed record EuergetismObligation(
    RuntimeId<Household> HouseholdId,
    int PublicWorksFundedCount,
    GameDate? FirstQualifiedDate,
    bool PerceivedAsNeglectful);

/// <summary>Read/write helpers over <see cref="WorldState.EuergetismObligations"/>, sparse and keyed by
/// the already-registered <see cref="RuntimeId{Household}"/> it describes — present only once a household
/// has either funded a Public Work or been read as wealthy enough to matter, matching <see
/// cref="RealEstate.PlotPropertyResolver"/>'s identical "no entry means the default" convention.</summary>
public static class EuergetismObligationResolver
{
    public static EuergetismObligation Current(WorldState state, RuntimeId<Household> householdId) =>
        state.EuergetismObligations.TryGet(householdId, out var entry)
            ? entry!
            : new EuergetismObligation(householdId, PublicWorksFundedCount: 0, FirstQualifiedDate: null, PerceivedAsNeglectful: false);

    public static void Set(WorldState state, EuergetismObligation obligation)
    {
        if (state.EuergetismObligations.TryGet(obligation.HouseholdId, out _))
            state.EuergetismObligations.Remove(obligation.HouseholdId);
        state.EuergetismObligations.Add(obligation.HouseholdId, obligation);
    }

    /// <summary>§2's obligation genuinely discharged the moment a household funds any real Public Work —
    /// this item's own reasoned reading of an unsized design question (§10's own "all numeric sizing...
    /// unsized" covers this too): a single real contribution resets the count upward and clears any
    /// standing neglect reading, rather than requiring some further, unspecified sustained pattern.</summary>
    public static void RecordFunded(WorldState state, RuntimeId<Household> householdId)
    {
        var current = Current(state, householdId);
        Set(state, current with { PublicWorksFundedCount = current.PublicWorksFundedCount + 1, PerceivedAsNeglectful = false });
    }
}

/// <summary>
/// §2's monthly resolution (Phase 15 item 9) — a static, unwired <c>Tick(state, date)</c> helper matching
/// every other Phase 15 item's identical "no central <c>IMonthlySystem</c> pipeline registry exists
/// anywhere in this codebase for any Phase 15 system to join" convention. For every <see
/// cref="Household"/> this codebase has ever assessed a Net Worth for, once that figure clears <see
/// cref="PublicWorksCatalog.ObligationNetWorthThreshold"/>, records the household's own first-qualified
/// date (if not already recorded) and, once <see cref="PublicWorksCatalog.ObligationGracePeriodMonths"/>
/// have passed with zero <see cref="EuergetismObligation.PublicWorksFundedCount"/>, marks it neglectful
/// and applies §2's own "real, quiet Dignitas cost" every month it stays that way. A household that later
/// funds a Public Work (<see cref="EuergetismObligationResolver.RecordFunded"/>) or whose Net Worth later
/// falls back below the threshold stops accruing the penalty (though its own first-qualified date and
/// funded count are never reset — a household's own real history is never erased, only its live neglect
/// reading).
/// </summary>
public static class EuergetismObligationSystem
{
    public static IReadOnlyList<IDomainEvent> Tick(WorldState state, GameDate date)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var entry in state.NetWorthAssessments.InAscendingOrder())
        {
            var householdId = entry.Key;
            var qualifies = entry.Value.Total >= PublicWorksCatalog.ObligationNetWorthThreshold;
            var obligation = EuergetismObligationResolver.Current(state, householdId);

            if (!qualifies)
            {
                // Falling back below the threshold resets the qualification clock, not just the neglect
                // flag — otherwise a household that re-qualifies later would have its new
                // `monthsQualified` measured from a stale `FirstQualifiedDate` spanning the entire
                // non-qualifying interval, skipping the grace period §2/§10 both assume is continuous.
                if (obligation.PerceivedAsNeglectful || obligation.FirstQualifiedDate is not null)
                    EuergetismObligationResolver.Set(state, obligation with { PerceivedAsNeglectful = false, FirstQualifiedDate = null });
                continue;
            }

            var firstQualifiedDate = obligation.FirstQualifiedDate ?? date;
            if (obligation.FirstQualifiedDate is null)
                EuergetismObligationResolver.Set(state, obligation with { FirstQualifiedDate = firstQualifiedDate });

            var monthsQualified = date.TotalMonths - firstQualifiedDate.TotalMonths;
            var neglectful = obligation.PublicWorksFundedCount == 0 && monthsQualified >= PublicWorksCatalog.ObligationGracePeriodMonths;

            var current = EuergetismObligationResolver.Current(state, householdId);
            if (current.PerceivedAsNeglectful != neglectful)
                EuergetismObligationResolver.Set(state, current with { PerceivedAsNeglectful = neglectful });

            if (neglectful)
            {
                events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
                    state, new AdjustDignitasCommand(
                        state.CommandIds.Issue(), "system", date, null, householdId,
                        PublicWorksCatalog.ObligationMonthlyDignitasPenalty, "euergetism obligation: unmet")).Events);
            }
        }

        return events;
    }
}
