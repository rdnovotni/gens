using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Diplomacy;

/// <summary>
/// The monthly tick for every <see cref="FrontierTreaty"/> (Phase 16 item 5 slice 1): posts a <see
/// cref="FrontierTreatyType.Tribute"/> treaty's monthly payment (falling into arrears — a goodwill
/// penalty with no ledger posting — on insufficient funds, rather than breaking the treaty outright;
/// tying arrears to real treaty-breach/raiding consequences is deferred, see the build roadmap's Phase
/// 16 item 5 progress note), and expires any treaty whose term has run out.
/// </summary>
public sealed class FrontierTreatySystem : IMonthlySystem<WorldState>
{
    public string Id => "relationshipsActors.frontierTreaty";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "frontierTreaties", "perPeopleStandings", "ledgerAccounts" };
    public IReadOnlyCollection<string> Writes { get; } = new[]
    {
        "frontierTreaties", "perPeopleStandings", "eventIds", "ledgerAccounts", "ledgerTransactions", "ledgerTransactionIds",
    };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        var activeTreaties = state.FrontierTreaties.InAscendingOrder()
            .Where(entry => entry.Value.Status == FrontierTreatyStatus.Active)
            .ToArray();

        foreach (var (treatyId, treaty) in activeTreaties)
        {
            if (treaty.ExpiresDate.TotalMonths <= context.Date.TotalMonths)
            {
                var expired = treaty with { Status = FrontierTreatyStatus.Expired, EndedDate = context.Date };
                state.FrontierTreaties.Remove(treatyId);
                state.FrontierTreaties.Add(treatyId, expired);

                events.Add(new FrontierTreatyEndedEvent(
                    state.EventIds.Issue(), context.Date, treatyId, treaty.HouseholdId, treaty.ForeignPeopleActorId,
                    FrontierTreatyStatus.Expired, CausationId: null));
                continue;
            }

            if (treaty.Type != FrontierTreatyType.Tribute || treaty.MonthlyTribute == Money.Zero)
                continue;

            PostMonthlyTribute(state, treaty, context, events);
        }

        return events;
    }

    private static void PostMonthlyTribute(WorldState state, FrontierTreaty treaty, MonthlyTickContext context, List<IDomainEvent> events)
    {
        var (payerAccount, payeeAccount) = treaty.TributeDirection == TributeDirection.HouseholdPaysPeople
            ? (LedgerAccountKey.ForHousehold(treaty.HouseholdId), LedgerAccountKey.ForActor(treaty.ForeignPeopleActorId))
            : (LedgerAccountKey.ForActor(treaty.ForeignPeopleActorId), LedgerAccountKey.ForHousehold(treaty.HouseholdId));

        var payerBalance = state.LedgerAccounts.TryGet(payerAccount, out var account) ? account!.Balance : Money.Zero;
        var key = new PerPeopleStandingKey(treaty.HouseholdId, treaty.ForeignPeopleActorId);

        if (payerBalance < treaty.MonthlyTribute)
        {
            PerPeopleStandingMutator.Apply(state, key, -FrontierDiplomacyCatalog.TributeArrearsGoodwillPenalty, context.Date);
            return;
        }

        events.Add(LedgerService.Post(
            state, context.Date, LedgerTransactionCategory.Treasury,
            new[]
            {
                new LedgerPosting(payerAccount, -treaty.MonthlyTribute),
                new LedgerPosting(payeeAccount, treaty.MonthlyTribute),
            },
            reference: $"diplomacy.frontierTreatyTribute:{treaty.TreatyId.ToTaggedString()}"));
    }
}
