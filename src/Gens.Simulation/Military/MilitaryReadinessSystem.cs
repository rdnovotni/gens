#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Gens.Simulation.Commands;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Military;

/// <summary>Idle squads drill and recover; paid squads incur real wages. An unpaid squad loses morale
/// and deserts deterministically, returning local recruits to their source population cohort.</summary>
public sealed class MilitaryReadinessSystem : IMonthlySystem<WorldState>
{
    public string Id => "military.readiness";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "estateForces", "ledgerAccounts" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "squads", "popGroups", "ledgerAccounts", "ledgerTransactions", "ledgerTransactionIds", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        var events = new List<IDomainEvent>();
        foreach (var entry in state.Squads.InAscendingOrder().ToArray())
        {
            var squad = entry.Value;
            if (squad.Status != SquadStatus.Ready || squad.Manpower == 0 || !state.EstateForces.TryGet(squad.ForceSettlementId, out var force))
                continue;

            var wagePerSoldier = squad.RecruitmentSource == SquadRecruitmentSource.Mercenaries
                ? MilitaryCatalog.MercenaryWageDenariiPerSoldier
                : squad.RecruitmentSource == SquadRecruitmentSource.EnslavedMilitia ? 0 : MilitaryCatalog.RegularWageDenariiPerSoldier;
            var wage = Money.FromDenarii(squad.Manpower * wagePerSoldier);
            var accountKey = LedgerAccountKey.ForHousehold(force!.HouseholdId);
            var balance = state.LedgerAccounts.TryGet(accountKey, out var account) ? account!.Balance : Money.Zero;
            var paid = wage == Money.Zero || balance >= wage;
            if (paid && wage > Money.Zero)
                events.Add(LedgerService.Post(state, context.Date, LedgerTransactionCategory.Wages,
                    new[] { new LedgerPosting(accountKey, -wage), new LedgerPosting(LedgerAccountKey.Mint, wage) },
                    $"military.wages:{squad.Id.ToTaggedString()}"));

            var readiness = Math.Min(MilitaryCatalog.MaxCondition, squad.Readiness + MilitaryCatalog.MonthlyReadinessRecovery);
            var morale = paid ? Math.Min(MilitaryCatalog.MaxCondition, squad.Morale + MilitaryCatalog.MonthlyMoraleRecovery) : Math.Max(0, squad.Morale - 15);
            var desertions = paid || morale >= 25 ? 0 : Math.Min(squad.Manpower, Math.Max(1, squad.Manpower / (squad.RecruitmentSource == SquadRecruitmentSource.Mercenaries ? 5 : 10)));
            if (desertions > 0 && squad.SourcePopGroup is { } source)
                MilitaryCommands.AddPopulation(state, squad.ForceSettlementId, source, desertions);

            MilitaryCommands.SetSquad(state, squad with
            {
                Manpower = squad.Manpower - desertions,
                Readiness = readiness,
                Morale = morale,
                Status = squad.Manpower == desertions ? SquadStatus.Destroyed : squad.Status,
            });
            if (desertions > 0)
                events.Add(new MilitaryStateChangedEvent(state.EventIds.Issue(), context.Date, "desertion",
                    new[] { squad.Id.ToTaggedString() }, null));
        }
        return events;
    }
}
