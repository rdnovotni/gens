using Gens.Simulation.Actions;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Policies;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Stewardship;

/// <summary>
/// The steward/Council autonomous decision loop (Phase 10 items 2/10/11; roadmap step 1's shared <see
/// cref="ActionSelector"/> applied to delegation rather than rival ambition). For each active <see
/// cref="StewardshipAssignment"/>: a competence roll (§5, Stewardship + Learning) first decides whether
/// the steward acts decisively at all this month; if it does, <see cref="ActionSelector"/> ranks the
/// household's own <see cref="ActionCatalog"/> exactly as <see cref="Actors.RivalAmbitionSystem"/>
/// ranks a rival's, filtered by <see cref="StewardAlwaysHeldCatalog"/> and <see
/// cref="StewardAutonomyGateCatalog"/>, and the highest-ranked candidate that still domain-validates is
/// submitted as a real command through its own ordinary pipeline. Independently, every month, a
/// Loyalty risk roll (§6) can produce an incident — never revealed as it happens, only recorded, per
/// §8's "dramatic reveal on return." Every month, whatever happened, is written to one <see
/// cref="AutonomousDecisionLog"/> entry.
/// </summary>
public sealed class StewardAutonomousDecisionSystem : IMonthlySystem<WorldState>
{
    private readonly ActionCatalog _catalog;
    private readonly Func<RuntimeId<Household>, RuntimeId<Settlement>> _resolveHomeSettlement;
    private readonly string _competenceStreamName;
    private readonly string _loyaltyStreamName;

    public StewardAutonomousDecisionSystem(
        ActionCatalog catalog,
        Func<RuntimeId<Household>, RuntimeId<Settlement>> resolveHomeSettlement,
        string competenceStreamName,
        string loyaltyStreamName)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _resolveHomeSettlement = resolveHomeSettlement ?? throw new ArgumentNullException(nameof(resolveHomeSettlement));
        _competenceStreamName = string.IsNullOrEmpty(competenceStreamName)
            ? throw new ArgumentException("A competence random stream name is required.", nameof(competenceStreamName))
            : competenceStreamName;
        _loyaltyStreamName = string.IsNullOrEmpty(loyaltyStreamName)
            ? throw new ArgumentException("A loyalty random stream name is required.", nameof(loyaltyStreamName))
            : loyaltyStreamName;
    }

    public string Id => "stewardship.autonomousDecision";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "stewardshipAssignments", "characters", "householdPolicies", "ledgerAccounts" };

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
            var stewardId = PrimaryStewardCharacterId(assignment);
            state.Characters.TryGet(stewardId, out var steward);

            var decisionType = "none";
            var outcome = "held";
            var treasuryImpact = Money.Zero;

            var competenceRoll = CompetenceExecutionChancePercent(steward);
            if (steward is not null && context.RandomStreams.NextUInt(_competenceStreamName, 100) < (uint)competenceRoll)
            {
                var invocation = new ActionInvocation(assignment.HouseholdId.ToTaggedString(), null, context.Date);
                var ranked = ActionSelector.Rank(state, _catalog, invocation)
                    .Where(candidate => !StewardAlwaysHeldCatalog.IsAlwaysHeld(candidate.Definition.Id))
                    .Where(candidate => StewardAutonomyGateCatalog.IsAllowed(candidate.Definition.Id, assignment.AutonomyLevel))
                    .ToArray();

                foreach (var candidate in ranked)
                {
                    var (accepted, summary, impact, producedEvents) = TryExecute(state, assignment, candidate.Definition.Id, context.Date);
                    if (!accepted)
                        continue;

                    events.AddRange(producedEvents);
                    decisionType = candidate.Definition.Id.Value;
                    outcome = summary;
                    treasuryImpact += impact;
                    break;
                }
            }

            var loyaltyRoll = LoyaltyIncidentChancePercent(steward);
            StewardIncidentType? incidentType = null;
            if (steward is not null && context.RandomStreams.NextUInt(_loyaltyStreamName, 100) < (uint)loyaltyRoll)
            {
                incidentType = IncidentSeverityFor(steward.Condition.Loyalty);
                var loss = StewardLoyaltyCatalog.AmountFor(incidentType.Value);
                LedgerService.Post(
                    state, context.Date, LedgerTransactionCategory.Treasury,
                    new[]
                    {
                        new LedgerPosting(LedgerAccountKey.ForHousehold(assignment.HouseholdId), -loss),
                        new LedgerPosting(LedgerAccountKey.Mint, loss),
                    },
                    reference: $"steward incident on assignment {assignmentId.ToTaggedString()}");
                treasuryImpact -= loss;
            }

            var logId = state.AutonomousDecisionLogIds.Issue();
            state.AutonomousDecisionLogs.Add(
                logId,
                new AutonomousDecisionLog(
                    logId, assignmentId, context.Date, decisionType, outcome, competenceRoll, loyaltyRoll, incidentType, treasuryImpact));
        }

        return events;
    }

    /// <summary>The Character whose Stewardship/Learning/Loyalty this month's rolls read (§2.1: "if
    /// Rationalis filled, natural tiebreaker; else ordinary Steward remains sole contact"). Falls back
    /// to the first filled Council seat when no head is recorded — <see
    /// cref="StewardshipAssignment.Create"/> already guarantees a Council assignment has at least
    /// one.</summary>
    private static RuntimeId<Character> PrimaryStewardCharacterId(StewardshipAssignment assignment) =>
        assignment.Mode == StewardshipMode.SingleSteward
            ? assignment.AppointeeCharacterId!.Value
            : assignment.CouncilHeadCharacterId ?? assignment.CouncilMembers[0].CharacterId;

    private static int CompetenceExecutionChancePercent(Character? steward) =>
        steward is null
            ? 0
            : Math.Clamp(
                StewardCompetenceCatalog.BaseExecutionChancePercent
                    + (steward.Attributes.Stewardship * StewardCompetenceCatalog.StewardshipWeightPercent / 100)
                    + (steward.Attributes.Learning * StewardCompetenceCatalog.LearningWeightPercent / 100),
                0, 100);

    private static int LoyaltyIncidentChancePercent(Character? steward) =>
        steward is null
            ? 0
            : Math.Clamp(
                StewardLoyaltyCatalog.MaxIncidentChancePercent * (100 - steward.Condition.Loyalty) / 100,
                0, 100);

    private static StewardIncidentType IncidentSeverityFor(int loyalty) =>
        loyalty < StewardLoyaltyCatalog.ActiveSabotageLoyaltyThreshold ? StewardIncidentType.ActiveSabotage
        : loyalty < StewardLoyaltyCatalog.EmbezzlementLoyaltyThreshold ? StewardIncidentType.Embezzlement
        : StewardIncidentType.Skimming;

    /// <summary>Maps a ranked candidate's <see cref="ActionDefinition.Id"/> to the concrete command it
    /// actually submits — the generic layer's <see cref="ActionInvocation"/> carries no field for a
    /// command's own extra parameters (e.g. which Rites Budget tier), matching <see
    /// cref="Actors.RivalHouseActionDefinitions"/>'s identical "small hardcoded mapping" convention.
    /// Returns <c>Accepted: false</c> both when the underlying command pipeline rejects it and when a
    /// domain-specific check here decides there is nothing useful to do (e.g. Rites Budget is already
    /// at its resting default) — either way, the caller falls through to the next-ranked candidate.</summary>
    private (bool Accepted, string Summary, Money TreasuryImpact, IReadOnlyList<IDomainEvent> Events) TryExecute(
        WorldState state, StewardshipAssignment assignment, DefinitionId<ActionDefinition> definitionId, GameDate date)
    {
        if (definitionId == PolicyActionDefinitions.ChangeRitesBudget)
        {
            // A sensible-steward heuristic this codebase invents (matching its own "§10 untuned
            // numbers" convention): restore the household's resting default tier whenever it has
            // drifted away from it, rather than picking an arbitrary new tier.
            var current = HouseholdPolicyResolver.GetEffectiveRitesBudget(state, assignment.HouseholdId);
            if (current == RitesBudgetCatalog.Default)
                return (false, "held", Money.Zero, Array.Empty<IDomainEvent>());

            var command = new ChangeRitesBudgetCommand(
                state.CommandIds.Issue(), assignment.HouseholdId.ToTaggedString(), date, null,
                assignment.HouseholdId, RitesBudgetCatalog.Default);
            var result = ChangeRitesBudgetCommands.Pipeline.Execute(state, command);
            return result.Accepted
                ? (true, $"Restored the Rites Budget to its {RitesBudgetCatalog.Default} default.", Money.Zero, result.Events)
                : (false, "held", Money.Zero, Array.Empty<IDomainEvent>());
        }

        if (definitionId == PolicyActionDefinitions.FundFestival)
        {
            var settlementId = _resolveHomeSettlement(assignment.HouseholdId);
            var command = new FundFestivalCommand(
                state.CommandIds.Issue(), assignment.HouseholdId.ToTaggedString(), date, null,
                assignment.HouseholdId, settlementId, PolicyActionDefinitions.DefaultFestivalAmount);
            var result = FundFestivalCommands.Pipeline.Execute(state, command);
            return result.Accepted
                ? (true, $"Funded a Festival for {PolicyActionDefinitions.DefaultFestivalAmount.ToDisplayString()} denarii.",
                    PolicyActionDefinitions.DefaultFestivalAmount, result.Events)
                : (false, "held", Money.Zero, Array.Empty<IDomainEvent>());
        }

        return (false, "held", Money.Zero, Array.Empty<IDomainEvent>());
    }
}
