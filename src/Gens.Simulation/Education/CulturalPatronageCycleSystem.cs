using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Education;

/// <summary>
/// The monthly Cultural Patronage tick (Phase 17 item 2; §7) — the ongoing-commitment half of both <see
/// cref="SetLiteraryPatronCommand"/> and <see cref="HostRecurringSymposiumCommand"/>, mirroring <see
/// cref="Religion.FavorCycleSystem"/>'s identical "draw the Ledger cost for every active record, move the
/// tracked score by the catalog-fixed amount for its tier" shape. Each month, every active <see
/// cref="CulturalPatronageRecord"/> (see <see cref="CulturalPatronageResolver.AllActive"/>) pays its
/// type's monthly cost (<see cref="CulturalPrestigeCatalog.LiteraryPatronMonthlyCost"/>/<see
/// cref="CulturalPrestigeCatalog.RecurringSymposiumMonthlyCost"/>) into this domain's own ledger sink and
/// accrues Cultural Prestige by that type's monthly gain — a silent background trickle with no dedicated
/// event of its own beyond the Ledger's own <see cref="LedgerTransactionPostedEvent"/>, matching <see
/// cref="Religion.FavorCycleSystem"/>'s identical "a quiet resource trickle" precedent. The Ledger allows
/// a negative balance (<see cref="Money"/>'s own doc comment), so this draw is posted unconditionally
/// rather than gated on solvency, matching <see cref="Religion.FavorCycleSystem"/>'s identical choice for
/// an ongoing commitment (as opposed to <see cref="Religion.FundFestivalCelebrationCommand"/>'s one-time,
/// solvency-gated spend).
///
/// §7's "reads a hosting Character's <c>CharacterInstitutionCredential</c> for a bonus" is not wired
/// here: that credential does not exist until Phase 17 item 2's own later Institutions of Renown slice —
/// see that slice's amendment to this system for the bonus, matching <see
/// cref="Religion.PatronDeity"/>'s own "shared-axis-first, bespoke-hooks-later" precedent for a
/// forward-referenced but not-yet-built consumer.
/// </summary>
public sealed class CulturalPatronageCycleSystem : IMonthlySystem<WorldState>
{
    private static readonly LedgerAccountKey PatronageSink = new(LedgerAccountKind.System, "education:culturalPatronage");

    public string Id => "education.culturalPatronageCycle";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "culturalPatronageRecords", "ledgerAccounts" };
    public IReadOnlyCollection<string> Writes { get; } =
        new[] { "householdCulturalPrestiges", "ledgerAccounts", "ledgerTransactions", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // Materialize first: this system only writes HouseholdCulturalPrestiges, not
        // CulturalPatronageRecords itself, but matches FavorCycleSystem's identical
        // "snapshot before iterating a partition another write could invalidate" guard for consistency.
        foreach (var record in CulturalPatronageResolver.AllActive(state).ToArray())
        {
            var cost = record.Type == CulturalPatronageType.LiteraryPatron
                ? CulturalPrestigeCatalog.LiteraryPatronMonthlyCost
                : CulturalPrestigeCatalog.RecurringSymposiumMonthlyCost;
            var gain = record.Type == CulturalPatronageType.LiteraryPatron
                ? CulturalPrestigeCatalog.LiteraryPatronMonthlyPrestigeGain
                : CulturalPrestigeCatalog.RecurringSymposiumMonthlyPrestigeGain;

            var posted = LedgerService.Post(
                state, context.Date, LedgerTransactionCategory.Gifts,
                new[]
                {
                    new LedgerPosting(LedgerAccountKey.ForHousehold(record.HouseholdId), -cost),
                    new LedgerPosting(PatronageSink, cost),
                },
                reference: $"education:culturalPatronage:{record.RecordId.ToTaggedString()}:{context.Date.TotalMonths}");
            events.Add(posted);

            CulturalPrestigeResolver.Apply(state, record.HouseholdId, gain);
        }

        return events;
    }
}
