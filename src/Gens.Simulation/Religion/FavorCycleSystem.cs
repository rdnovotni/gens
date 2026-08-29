using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Policies;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Religion;

/// <summary>
/// The monthly Rites Budget tick (Phase 12 item 3; §3.1) — the real consumer <see
/// cref="Policies.RitesBudgetCatalog"/>'s own doc comments have named as forthcoming since Phase 9 item
/// 2 shipped that catalog's projections "ahead of [Religion's] own pass": "Religion (§6, future) does
/// not exist yet to actually spend Incense or track Divine Favor, so this pass's own <c>
/// RitesBudgetCatalog</c> projects the modifiers that system will eventually consume without yet wiring
/// them into a monthly tick." This system is that wiring, closing the forward reference directly rather
/// than building a second, parallel Rites Budget mechanism — the task's own instruction to model the
/// Rites Budget "analogous to... existing policy patterns" is realized here by consuming the policy that
/// already exists (<see cref="RitesBudgetTier"/>, <see cref="ChangeRitesBudgetCommand"/>, <see
/// cref="HouseholdPolicyResolver"/>) rather than re-authoring a duplicate tier/command/resolver set.
///
/// Each month, every household with a chosen Patron Deity (<see
/// cref="WorldState.HouseholdReligions"/>) pays <see cref="RitesBudgetCatalog.TreasuryDrawPerMonth"/>
/// for its currently-effective tier into this domain's own ledger sink, and its Favor moves by <see
/// cref="RitesBudgetCatalog.DivineFavorStabilityModifier"/> for that same tier — a silent background
/// drift with no dedicated event of its own, matching <see cref="Clientela.InfluenceCycleSystem"/>'s
/// identical "a quiet resource trickle... nothing downstream needs a per-tick event for a number that
/// already reads directly off [its] Resolver" precedent; the ledger draw's own <see
/// cref="LedgerTransactionPostedEvent"/> is still emitted, matching every other monthly system that
/// posts to the Ledger (e.g. <see cref="Economy.RentAndTaxSystem"/>). The Ledger allows a household's
/// balance to run negative (<see cref="Money"/>'s own doc comment), so this draw is posted
/// unconditionally rather than skipped or partially applied when the treasury can't fully cover it —
/// the same "the cost is real and competes with every other line on the Ledger" framing §3.1 itself
/// gives the Rites Budget.
/// </summary>
public sealed class FavorCycleSystem : IMonthlySystem<WorldState>
{
    private static readonly LedgerAccountKey RitesBudgetSink = new(LedgerAccountKind.System, "religion:ritesBudget");

    public string Id => "religion.favorCycle";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "householdReligions", "householdPolicies", "ledgerAccounts" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "householdReligions", "ledgerAccounts", "ledgerTransactions", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // Materialize first: ApplyFavorDelta replaces the entry being iterated, matching
        // ClientPoachingSystem's identical "snapshot before mutating" guard.
        foreach (var householdId in state.HouseholdReligions.InAscendingOrder().Select(entry => entry.Key).ToArray())
        {
            var tier = HouseholdPolicyResolver.GetEffectiveRitesBudget(state, householdId);
            var draw = RitesBudgetCatalog.TreasuryDrawPerMonth(tier);

            var posted = LedgerService.Post(
                state, context.Date, LedgerTransactionCategory.Gifts,
                new[]
                {
                    new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), -draw),
                    new LedgerPosting(RitesBudgetSink, draw),
                },
                reference: $"religion:ritesBudget:{householdId.ToTaggedString()}:{context.Date.TotalMonths}");
            events.Add(posted);

            HouseholdReligionResolver.ApplyFavorDelta(state, householdId, RitesBudgetCatalog.DivineFavorStabilityModifier(tier));
        }

        return events;
    }
}
