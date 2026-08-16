using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>
/// The monthly employment-matching tick (Roadmap Phase 7 item 3;
/// <c>gens-settlement-demographics-design.md</c> §4.2's Employment Ratio, taken to its conclusion). <see
/// cref="JobCapacitySystem"/> (item 2) already answers "how many background job slots exist"; this
/// system answers "how many of a group's people actually hold one this month, what happens to the rest,
/// and what the filled slots pay" — using the exact same <see
/// cref="BackgroundJobCapacityCalculator.ComputeCapacityByGroup"/> slot counts item 2 uses, rather than
/// back-deriving them from <see cref="PopGroup.EmploymentRatio"/>'s rounded ratio.
///
/// **Matching.** For a group of <see cref="PopGroup.Size"/> people competing for <c>capacity</c> slots,
/// <c>Employed = min(Size, capacity)</c> and <c>Underemployed = Size - Employed</c>: a background pop
/// group is a Victoria-style aggregate with no per-person tracking (§9's own "background labor
/// deliberately doesn't get its own Regimen, Punishment ladder, or flight mechanic" rules out simulating
/// individual job-seekers), so matching can only ever be this aggregate split, not an assignment of
/// specific people to specific openings.
///
/// **Underemployment** is the <c>Underemployed</c> remainder made a first-class, explicitly reported
/// number rather than left implicit in a sub-1.0 Employment Ratio: the population still counted in the
/// group (still fed, housed, and taxed as members of it) but not drawing this month's wage. Later Phase
/// 7 items (needs demand, contentment, migration, mobility) are the ones that react to it — this system
/// only measures and reports it.
///
/// **Wages offered.** <see cref="MonthlyWageDenarii"/> is this implementation's own invented per-group
/// wage table, in the same spirit as <see cref="RegimenCatalog"/>'s and <see
/// cref="DutySlotCatalog"/>'s hardcoded baselines: no wage figure exists anywhere in the design corpus
/// except <c>gens-vertical-slice-quantification.md</c> §5's proposed defaults ("4 denarii/month for an
/// unskilled Operarius-equivalent background laborer; 8 denarii/month for a skilled Opifex-equivalent"),
/// which this table adopts verbatim for Operarii/Coloni (the §6.1 pairing that already treats them as
/// one Meager/Adequate-consumption tier) and Opifices, extrapolating the remaining figures — documented
/// as invented, pending playtesting like every other unsized figure in this codebase. <c>WagesOffered =
/// Employed * MonthlyWageDenarii[GroupType]</c> — a monthly figure only, computed and reported here; no
/// Treasury/Ledger account exists yet to actually debit it (Roadmap Phase 8), so this is groundwork in
/// the same "ledger-ready before the ledger exists" spirit as <see cref="BackgroundJobCapacityComputedEvent"/>.
///
/// **Named/background labor boundary.** This system's scope is deliberately drawn to match the two
/// systems it sits beside rather than overlap them, resolving open question B9 for the background half
/// of the boundary: named Characters are paid or provisioned by <see cref="LaborOutputSystem"/> and <see
/// cref="RegimenResolver"/> (a hired free/freed Duty-holder's actual wage payment is Phase 8's Economy
/// &amp; Finance scope; an enslaved Duty-holder's Regimen tier is the parallel non-monetary "pay" that
/// system already resolves) — this system never touches a <see cref="Character"/>. And within the
/// background tier itself, <see cref="PopGroupType.NonHouseholdEnslaved"/> is excluded from the wage
/// table entirely: enslaved background labor has no wage, mirroring named enslaved labor's Regimen-not-
/// wage treatment, and <see cref="PopGroupType.Veterans"/> is excluded because their livelihood is the
/// land grant §7.2 describes, not a job-capacity slot <see cref="JobCapacitySystem"/> ever computes for
/// them. Only the six group types <see cref="BackgroundJobCapacityCalculator.ComputeCapacityByGroup"/>
/// actually sizes — Coloni, Operarii, Opifices, Negotiatores, Aeditui, Curiales — are matched and paid.
/// </summary>
public sealed class EmploymentMatchingSystem : IMonthlySystem<WorldState>
{
    /// <summary>This implementation's own invented monthly wage table in denarii, per <see
    /// cref="PopGroupType"/> — see this class's own doc comment for provenance. Coloni and Operarii
    /// (unskilled) and Opifices (skilled) reuse <c>gens-vertical-slice-quantification.md</c> §5's
    /// proposed 4/8 figures verbatim; Aeditui (a comparably skilled, stable role) matches Opifices;
    /// Negotiatores and Curiales (§6.1's higher Adequate/Generous-consumption tier) are extrapolated
    /// upward, capped below §5's 15-denarii Companion-tier figure reserved for named positions.</summary>
    private static readonly Dictionary<PopGroupType, int> MonthlyWageDenarii = new Dictionary<PopGroupType, int>
    {
        [PopGroupType.Coloni] = 4,
        [PopGroupType.Operarii] = 4,
        [PopGroupType.Opifices] = 8,
        [PopGroupType.Aeditui] = 8,
        [PopGroupType.Negotiatores] = 10,
        [PopGroupType.Curiales] = 12,
    };

    public string Id => "characters.employmentMatching";
    public TickPhase Phase => TickPhase.EmploymentNeeds;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "buildings", "plots", "settlements", "popGroups" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "eventIds" };
    // Same-phase prerequisite: reads the exact capacity math JobCapacitySystem also reads, and its own
    // doc comment frames this system as building directly on that one's "always emitted" groundwork.
    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "characters.jobCapacity" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();
        var capacityBySettlement = BackgroundJobCapacityCalculator.SumBuildingCapacityBySettlement(state);

        foreach (var settlementEntry in state.Settlements.InAscendingOrder().ToArray())
        {
            var settlementId = settlementEntry.Key;
            var settlement = settlementEntry.Value;
            capacityBySettlement.TryGetValue(settlementId, out var sectorCapacity);

            var totalPopulation = 0;
            foreach (var popGroupEntry in state.PopGroups.InAscendingOrder())
            {
                if (popGroupEntry.Key.SettlementId == settlementId)
                    totalPopulation += popGroupEntry.Value.Size;
            }

            var capacityByGroup = BackgroundJobCapacityCalculator.ComputeCapacityByGroup(
                sectorCapacity, totalPopulation, settlement.Stage);

            var lines = new List<EmploymentMatchLine>();
            foreach (var (groupType, wage) in MonthlyWageDenarii.OrderBy(pair => pair.Key))
            {
                var size = state.PopGroups.TryGet(new PopGroupKey(settlementId, groupType), out var popGroup)
                    ? popGroup.Size
                    : 0;
                var capacity = capacityByGroup.TryGetValue(groupType, out var slots) ? slots : 0;

                var employed = Math.Min(size, capacity);
                var underemployed = size - employed;
                var wagesOffered = employed * wage;

                lines.Add(new EmploymentMatchLine(groupType, employed, underemployed, wagesOffered));
            }

            events.Add(new EmploymentMatchedEvent(state.EventIds.Issue(), context.Date, settlementId, lines));
        }

        return events;
    }
}

/// <summary>One Settlement's computed employment match for one wage-eligible pop group this month (this
/// class's own doc comment), part of <see cref="EmploymentMatchedEvent"/>.</summary>
public sealed record EmploymentMatchLine(PopGroupType GroupType, int Employed, int Underemployed, int WagesOfferedDenarii);

/// <summary>Emitted for every Settlement, every month, whether or not matching changed — ledger-ready
/// groundwork for Phase 8's Economy &amp; Finance ledger to eventually consume, matching <see
/// cref="BackgroundJobCapacityComputedEvent"/>'s and <see cref="LaborOutputComputedEvent"/>'s identical
/// "always emitted" convention.</summary>
public sealed record EmploymentMatchedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Settlement> SettlementId,
    IReadOnlyList<EmploymentMatchLine> Matches) : IDomainEvent
{
    public string Type => "characters.employmentMatched";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { SettlementId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}
