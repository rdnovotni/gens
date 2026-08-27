using Gens.Simulation.Actions;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
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
    public IReadOnlyCollection<string> Reads { get; } = new[] { "stewardshipAssignments", "householdPolicies", "ledgerAccounts", "characters" };

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

            var (competence, loyalty) = ReadCompetenceAndLoyalty(state, assignment);

            StewardIncidentType? incidentType = null;
            if (loyalty < StewardIncidentCatalog.LoyaltyRiskThreshold)
            {
                var incidentRoll = context.RandomStreams.NextUInt(CampaignBootstrapper.StewardLoyaltyRiskStreamName, 100);
                if (incidentRoll < StewardIncidentCatalog.IncidentChancePercent)
                {
                    var typeRoll = context.RandomStreams.NextUInt(CampaignBootstrapper.StewardLoyaltyRiskStreamName, 100);
                    incidentType = typeRoll < StewardIncidentCatalog.SkimmingWeightPercent
                        ? StewardIncidentType.Skimming
                        : typeRoll < StewardIncidentCatalog.SkimmingWeightPercent + StewardIncidentCatalog.EmbezzlementWeightPercent
                            ? StewardIncidentType.Embezzlement
                            : StewardIncidentType.ActiveSabotage;
                }
            }

            var logId = state.AutonomousDecisionLogIds.Issue();

            if (incidentType is { } discoveredType)
            {
                var (incidentSummary, incidentEvents) =
                    ApplyIncident(state, assignment, discoveredType, logId, context.Date);
                outcome = incidentSummary;
                events.AddRange(incidentEvents);
            }

            state.AutonomousDecisionLogs.Add(
                logId,
                new AutonomousDecisionLog(logId, assignmentId, context.Date, decisionType, outcome, competence, loyalty, incidentType));
        }

        return events;
    }

    /// <summary>Reads the competence (<see cref="CoreAttributes.Stewardship"/>, injury-adjusted via
    /// <see cref="Character.GetEffectiveAttributes"/>) and Loyalty-risk figures §5-6 call for. For <see
    /// cref="StewardshipMode.SingleSteward"/> these are simply the appointee's own stats. §5-6 never
    /// resolve what a Council-mode figure should be — this package's own documented default is to
    /// average every filled seat's Stewardship (a Council's overall competence is the sum of its
    /// members, not any one seat) but take the *minimum* seat's Loyalty for the risk figure (per §6's
    /// "the real risk of unsupervised trust": a Council is only as trustworthy as its least loyal
    /// member, since any one seat with real Treasury access can still act alone).</summary>
    private static (int Competence, int Loyalty) ReadCompetenceAndLoyalty(WorldState state, StewardshipAssignment assignment)
    {
        if (assignment.Mode == StewardshipMode.SingleSteward)
        {
            if (assignment.AppointeeCharacterId is not { } appointeeId || !state.Characters.TryGet(appointeeId, out var appointee))
                return (0, 100);

            return (appointee!.GetEffectiveAttributes().Stewardship, appointee.Condition.Loyalty);
        }

        var members = assignment.CouncilMembers
            .Select(member => state.Characters.TryGet(member.CharacterId, out var character) ? character : null)
            .Where(character => character is not null)
            .Select(character => character!)
            .ToArray();

        if (members.Length == 0)
            return (0, 100);

        var averageCompetence = (int)Math.Round(members.Average(character => character.GetEffectiveAttributes().Stewardship));
        var minimumLoyalty = members.Min(character => character.Condition.Loyalty);
        return (averageCompetence, minimumLoyalty);
    }

    /// <summary>Applies one discovered incident's concrete effect (§6): Skimming/Embezzlement move
    /// money through <see cref="LedgerService.Post"/> — the single money-movement path (rule: never a
    /// second ledger writer) — debiting the household's own Treasury and crediting the <see
    /// cref="LedgerAccountKey.Mint"/> system account, the same "external conservation boundary" Phase 8
    /// already uses for bootstrap seeding, reused here as the symmetric outflow: a corrupt steward's
    /// personal gain leaves the campaign's tracked economy entirely rather than landing in some new,
    /// otherwise-unused account. Active Sabotage instead perturbs the household's own Rites Budget
    /// Standing Policy toward its worst (Frugal) tier via the ordinary <see
    /// cref="ChangeRitesBudgetCommand"/> pipeline, reusing that command rather than mutating <see
    /// cref="Policies.HouseholdPolicyState"/> directly — if that pipeline rejects it (e.g. already
    /// Frugal, or on cooldown), the incident is still logged as discovered but has no further mechanical
    /// effect this month. The posting's <c>reference</c> carries the log's own id, the linkage <see
    /// cref="StewardshipCommands.MutateEnd"/> uses to fold a Return Report's Treasury impact back out of
    /// the ledger.</summary>
    private static (string Summary, IReadOnlyList<IDomainEvent> Events) ApplyIncident(
        WorldState state, StewardshipAssignment assignment, StewardIncidentType incidentType,
        RuntimeId<AutonomousDecisionLog> logId, GameDate date)
    {
        var events = new List<IDomainEvent>();
        string summary;

        switch (incidentType)
        {
            case StewardIncidentType.Skimming:
            case StewardIncidentType.Embezzlement:
                {
                    var amount = Money.FromDenarii(
                        incidentType == StewardIncidentType.Skimming
                            ? StewardIncidentCatalog.SkimmingAmountDenarii
                            : StewardIncidentCatalog.EmbezzlementAmountDenarii);
                    var postings = new[]
                    {
                    new LedgerPosting(LedgerAccountKey.ForHousehold(assignment.HouseholdId), -amount),
                    new LedgerPosting(LedgerAccountKey.Mint, amount),
                };
                    var postedEvent = LedgerService.Post(state, date, LedgerTransactionCategory.Treasury, postings, logId.ToTaggedString());
                    events.Add(postedEvent);
                    summary = incidentType == StewardIncidentType.Skimming
                        ? $"The steward was discovered quietly skimming {amount.ToDisplayString()} denarii."
                        : $"The steward was discovered embezzling {amount.ToDisplayString()} denarii.";
                    break;
                }
            case StewardIncidentType.ActiveSabotage:
                {
                    var currentTier = HouseholdPolicyResolver.GetEffectiveRitesBudget(state, assignment.HouseholdId);
                    if (currentTier != RitesBudgetTier.Frugal)
                    {
                        var command = new ChangeRitesBudgetCommand(
                            state.CommandIds.Issue(), assignment.HouseholdId.ToTaggedString(), date, null,
                            assignment.HouseholdId, RitesBudgetTier.Frugal);
                        var result = ChangeRitesBudgetCommands.Pipeline.Execute(state, command);
                        if (result.Accepted)
                            events.AddRange(result.Events);
                    }

                    summary = "The steward was discovered deliberately undermining the household's Standing Policies.";
                    break;
                }
            default:
                throw new ArgumentOutOfRangeException(nameof(incidentType), incidentType, "Unknown steward incident type.");
        }

        events.Add(new StewardIncidentDiscoveredEvent(
            state.EventIds.Issue(), date, assignment.AssignmentId, assignment.HouseholdId, logId, incidentType, null));

        return (summary, events);
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

/// <summary>Emitted whenever a Skimming/Embezzlement/Active-Sabotage incident is discovered (Phase 10
/// package 13; §6/§8: "discovered only on the player's return"). Private to the household itself
/// rather than <see cref="Commands.Visibility.Public"/> — matching §8's own framing, this is a reveal
/// the player learns of through their own Return Report, not something the wider world witnesses at
/// the moment it happens.</summary>
public sealed record StewardIncidentDiscoveredEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<StewardshipAssignment> AssignmentId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<AutonomousDecisionLog> LogId,
    StewardIncidentType IncidentType,
    string? CausationId) : IDomainEvent
{
    public string Type => "stewardship.incidentDiscovered";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), AssignmentId.ToTaggedString() };
    public Visibility Visibility => Visibility.Private(HouseholdId.ToTaggedString());
}
