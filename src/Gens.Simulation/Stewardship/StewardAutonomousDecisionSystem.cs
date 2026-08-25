using Gens.Simulation.Actions;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Policies;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Stewardship;

/// <summary>
/// The steward/Council autonomous decision loop (Phase 10 item 2/10; roadmap step 1's shared <see
/// cref="ActionSelector"/> applied to delegation rather than rival ambition). For each active <see
/// cref="StewardshipAssignment"/>, ranks the household's own <see cref="ActionCatalog"/> exactly as
/// <see cref="Actors.RivalAmbitionSystem"/> ranks a rival's, then filters out anything <see
/// cref="StewardAlwaysHeldCatalog"/> marks Always-Held or <see cref="StewardAutonomyGateCatalog"/>
/// judges above the assignment's own <see cref="StewardAutonomyLevel"/> — never an autonomy-level
/// check ActionSelector itself needs to know about, since it is specific to this one caller. The
/// highest-ranked candidate that still domain-validates (e.g. Rites Budget is already at its own
/// resting default) is submitted as a real command through its own ordinary pipeline; every month,
/// win or hold, is written to <see cref="AutonomousDecisionLog"/>.
/// </summary>
public sealed class StewardAutonomousDecisionSystem : IMonthlySystem<WorldState>
{
    private readonly ActionCatalog _catalog;
    private readonly Func<RuntimeId<Household>, RuntimeId<Settlement>> _resolveHomeSettlement;

    public StewardAutonomousDecisionSystem(ActionCatalog catalog, Func<RuntimeId<Household>, RuntimeId<Settlement>> resolveHomeSettlement)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _resolveHomeSettlement = resolveHomeSettlement ?? throw new ArgumentNullException(nameof(resolveHomeSettlement));
    }

    public string Id => "stewardship.autonomousDecision";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "stewardshipAssignments", "householdPolicies", "ledgerAccounts" };

    public IReadOnlyCollection<string> Writes { get; } =
        new[] { "householdPolicies", "ledgerAccounts", "ledgerTransactions", "autonomousDecisionLogs" };

    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // Materialize first: a chosen command's Mutate step can replace entries in state.StewardshipAssignments-
        // adjacent partitions mid-iteration, matching every other system's identical guard in this codebase.
        var activeAssignments = state.StewardshipAssignments.InAscendingOrder()
            .Where(entry => entry.Value.IsActive)
            .ToArray();

        foreach (var (assignmentId, assignment) in activeAssignments)
        {
            var invocation = new ActionInvocation(assignment.HouseholdId.ToTaggedString(), null, context.Date);
            var ranked = ActionSelector.Rank(state, _catalog, invocation)
                .Where(candidate => !StewardAlwaysHeldCatalog.IsAlwaysHeld(candidate.Definition.Id))
                .Where(candidate => StewardAutonomyGateCatalog.IsAllowed(candidate.Definition.Id, assignment.AutonomyLevel))
                .ToArray();

            var decisionType = "none";
            var outcome = "held";

            foreach (var candidate in ranked)
            {
                var (accepted, summary, producedEvents) = TryExecute(state, assignment, candidate.Definition.Id, context.Date);
                if (!accepted)
                    continue;

                events.AddRange(producedEvents);
                decisionType = candidate.Definition.Id.Value;
                outcome = summary;
                break;
            }

            var logId = state.AutonomousDecisionLogIds.Issue();
            state.AutonomousDecisionLogs.Add(
                logId,
                // CompetenceRollFactor/LoyaltyRiskRollFactor/IncidentType are this package's placeholder
                // values — see AutonomousDecisionLog's own doc comment for why (package 11's scope).
                new AutonomousDecisionLog(logId, assignmentId, context.Date, decisionType, outcome, 0, 0));
        }

        return events;
    }

    /// <summary>Maps a ranked candidate's <see cref="ActionDefinition.Id"/> to the concrete command it
    /// actually submits — the generic layer's <see cref="ActionInvocation"/> carries no field for a
    /// command's own extra parameters (e.g. which Rites Budget tier), matching <see
    /// cref="Actors.RivalHouseActionDefinitions"/>'s identical "small hardcoded mapping" convention.
    /// Returns <c>Accepted: false</c> both when the underlying command pipeline rejects it and when a
    /// domain-specific check here decides there is nothing useful to do (e.g. Rites Budget is already
    /// at its resting default) — either way, the caller falls through to the next-ranked candidate.</summary>
    private (bool Accepted, string Summary, IReadOnlyList<IDomainEvent> Events) TryExecute(
        WorldState state, StewardshipAssignment assignment, DefinitionId<ActionDefinition> definitionId, GameDate date)
    {
        if (definitionId == PolicyActionDefinitions.ChangeRitesBudget)
        {
            // A sensible-steward heuristic this codebase invents (matching its own "§10 untuned
            // numbers" convention): restore the household's resting default tier whenever it has
            // drifted away from it, rather than picking an arbitrary new tier.
            var current = HouseholdPolicyResolver.GetEffectiveRitesBudget(state, assignment.HouseholdId);
            if (current == RitesBudgetCatalog.Default)
                return (false, "held", Array.Empty<IDomainEvent>());

            var command = new ChangeRitesBudgetCommand(
                state.CommandIds.Issue(), assignment.HouseholdId.ToTaggedString(), date, null,
                assignment.HouseholdId, RitesBudgetCatalog.Default);
            var result = ChangeRitesBudgetCommands.Pipeline.Execute(state, command);
            return result.Accepted
                ? (true, $"Restored the Rites Budget to its {RitesBudgetCatalog.Default} default.", result.Events)
                : (false, "held", Array.Empty<IDomainEvent>());
        }

        if (definitionId == PolicyActionDefinitions.FundFestival)
        {
            var settlementId = _resolveHomeSettlement(assignment.HouseholdId);
            var command = new FundFestivalCommand(
                state.CommandIds.Issue(), assignment.HouseholdId.ToTaggedString(), date, null,
                assignment.HouseholdId, settlementId, PolicyActionDefinitions.DefaultFestivalAmount);
            var result = FundFestivalCommands.Pipeline.Execute(state, command);
            return result.Accepted
                ? (true, $"Funded a Festival for {PolicyActionDefinitions.DefaultFestivalAmount.ToDisplayString()} denarii.", result.Events)
                : (false, "held", Array.Empty<IDomainEvent>());
        }

        return (false, "held", Array.Empty<IDomainEvent>());
    }
}
