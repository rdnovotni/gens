using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.PrivateInfrastructure;

public enum LandReclamationStatus
{
    InProgress,
    CompletedPartial,
    CompletedFull,
}

/// <summary>§5's resolved (not chosen) outcome — only meaningful once <see
/// cref="LandReclamationProject.Status"/> is no longer <see cref="LandReclamationStatus.InProgress"/>.</summary>
public enum LandReclamationOutcome
{
    PartialReclamation,
    FullReclamation,
}

/// <summary>§5/§10's Land Reclamation project (Phase 15 item 7): a real, slow, capital-and-Labor
/// investment that can improve a Marsh Plot's own underlying terrain classification. <c>Land.TerrainType</c>
/// carries no separate "Poor-land" value alongside <see cref="TerrainType.Marsh"/> — this item's own
/// honest narrowing of §5's "Marsh/Poor-land" framing to the one real terrain classification this
/// codebase actually has.</summary>
public sealed record LandReclamationProject
{
    public required RuntimeId<Plot> PlotId { get; init; }
    public required GameDate StartMonth { get; init; }
    public required int MonthsInvested { get; init; }
    public required int LaborAssigned { get; init; }
    public required LandReclamationStatus Status { get; init; }
    public LandReclamationOutcome? ResolvedOutcome { get; init; }

    public static LandReclamationProject Start(RuntimeId<Plot> plotId, GameDate startMonth) => new()
    {
        PlotId = plotId,
        StartMonth = startMonth,
        MonthsInvested = 0,
        LaborAssigned = PrivateInfrastructureCatalog.LandReclamationMonthlyLaborRequired,
        Status = LandReclamationStatus.InProgress,
        ResolvedOutcome = null,
    };
}

public sealed record LandReclamationStartedEvent(
    RuntimeId<DomainEventEntity> EventId, GameDate OccurredDate, RuntimeId<Plot> PlotId, string? CausationId) : IDomainEvent
{
    public string Type => "privateInfrastructure.landReclamationStarted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { PlotId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>§5.1's Full Reclamation — a genuine, rare achievement worth a real Dignitas award (per this
/// item's own <see cref="PrivateInfrastructureCatalog.FullReclamationDignitasAward"/> reading of "worth
/// real Dignitas") and a Chronicle entry (<see cref="Chronicle.ChronicleProjector"/> gains a matching
/// case, mirroring <see cref="PrivateInfrastructureBenefitsSystem.UnifiedEstateAchievedEvent"/>'s own
/// identical "genuinely rare achievement" treatment) — versus a Partial result, which is real and
/// permanent but carries neither.</summary>
public sealed record LandReclamationCompletedEvent(
    RuntimeId<DomainEventEntity> EventId, GameDate OccurredDate, RuntimeId<Plot> PlotId, LandReclamationOutcome Outcome,
    string? CausationId) : IDomainEvent
{
    public string Type => "privateInfrastructure.landReclamationCompleted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { PlotId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>§5's Land Reclamation start (Phase 15 item 7) — gated on <see cref="TerrainType.Marsh"/>
/// (see <see cref="LandReclamationProject"/>'s own doc comment for the honest "Poor-land" narrowing).
/// Only the household resolving as the Plot's own owner may start one; ownership does not need to be
/// re-supplied by the caller since <see cref="Land.Plot.OwnerId"/> already carries it.</summary>
public sealed record StartLandReclamationCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Plot> PlotId) : ICommand;

public static class StartLandReclamationCommands
{
    public static readonly ValidationErrorCode PlotNotFound = new("privateInfrastructure.startLandReclamation.plotNotFound");
    public static readonly ValidationErrorCode NotMarshTerrain = new("privateInfrastructure.startLandReclamation.notMarshTerrain");
    public static readonly ValidationErrorCode NotOwned = new("privateInfrastructure.startLandReclamation.notOwned");
    public static readonly ValidationErrorCode AlreadyInProgress = new("privateInfrastructure.startLandReclamation.alreadyInProgress");

    public static readonly CommandPipeline<WorldState, StartLandReclamationCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, StartLandReclamationCommand command)
    {
        if (!state.Plots.TryGet(command.PlotId, out var plot))
            return PlotNotFound;
        if (plot!.Terrain != TerrainType.Marsh)
            return NotMarshTerrain;
        if (plot.OwnerId is null)
            return NotOwned;
        if (state.LandReclamationProjects.TryGet(command.PlotId, out var existing) &&
            existing!.Status == LandReclamationStatus.InProgress)
            return AlreadyInProgress;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, StartLandReclamationCommand command)
    {
        if (state.LandReclamationProjects.TryGet(command.PlotId, out _))
            state.LandReclamationProjects.Remove(command.PlotId);
        state.LandReclamationProjects.Add(command.PlotId, LandReclamationProject.Start(command.PlotId, command.SubmittedDate));

        return new IDomainEvent[]
        {
            new LandReclamationStartedEvent(state.EventIds.Issue(), command.SubmittedDate, command.PlotId, command.CommandId.ToTaggedString()),
        };
    }
}

/// <summary>
/// §5's monthly Land Reclamation resolution (Phase 15 item 7), matching the same unwired
/// static-<c>Tick</c> convention every Phase 15 system uses. For every in-progress project whose owning
/// household actually pays this month's real Labor/denarii cost (<see
/// cref="PrivateInfrastructureCatalog.LandReclamationMonthlyCost"/> — paid from the Plot's own resolved
/// owner account; an unpaid month simply does not advance <see
/// cref="LandReclamationProject.MonthsInvested"/>, a real, forgiving stall rather than a hard failure),
/// advances progress; once <see cref="PrivateInfrastructureCatalog.LandReclamationDurationMonths"/> is
/// reached, rolls §5.1's real Partial/Full outcome against <see
/// cref="PrivateInfrastructureCatalog.FullReclamationProbability"/> using the supplied <see
/// cref="RandomStreamSet"/> (matching <see cref="PublicContracts.FileRepetundaeCaseCommand"/>'s own
/// precedent for a Phase-15 mutation needing real randomness). A Full result converts the Plot's own
/// <see cref="TerrainType"/> to <see cref="TerrainType.FertilePlain"/> outright; a Partial result raises
/// <see cref="Land.LandCondition"/> to <see
/// cref="PrivateInfrastructureCatalog.PartialReclamationConditionFloor"/> without touching Terrain.
/// §11's own open question — whether continued investment after a Partial result can ever push toward
/// Full — is left exactly that open; this item builds no continuation path past a resolved outcome.
/// </summary>
public static class LandReclamationResolutionSystem
{
    public const string StreamName = "privateInfrastructure.landReclamation";

    public static IReadOnlyList<IDomainEvent> Tick(WorldState state, GameDate date, RandomStreamSet randomStreams)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (randomStreams is null)
            throw new ArgumentNullException(nameof(randomStreams));

        var events = new List<IDomainEvent>();

        foreach (var entry in state.LandReclamationProjects.InAscendingOrder().ToArray())
        {
            var project = entry.Value;
            if (project.Status != LandReclamationStatus.InProgress)
                continue;
            if (!state.Plots.TryGet(project.PlotId, out var plot))
                continue;

            var paid = TryPayMonthlyCost(state, date, plot!);
            var monthsInvested = paid ? project.MonthsInvested + 1 : project.MonthsInvested;

            if (monthsInvested < PrivateInfrastructureCatalog.LandReclamationDurationMonths)
            {
                state.LandReclamationProjects.Remove(entry.Key);
                state.LandReclamationProjects.Add(entry.Key, project with { MonthsInvested = monthsInvested });
                continue;
            }

            // Fixed64.ScaleFactor and ReclamationRollPrecision are both 1,000,000 (Fixed64's own parts-
            // per-million precision), so the probability's raw value is directly usable as a roll
            // threshold with no further scaling.
            var roll = randomStreams.NextUInt(StreamName, PrivateInfrastructureCatalog.ReclamationRollPrecision);
            var fullThreshold = (uint)Math.Clamp(
                PrivateInfrastructureCatalog.FullReclamationProbability.RawValue, 0, PrivateInfrastructureCatalog.ReclamationRollPrecision);
            var outcome = roll < fullThreshold ? LandReclamationOutcome.FullReclamation : LandReclamationOutcome.PartialReclamation;

            state.LandReclamationProjects.Remove(entry.Key);
            state.LandReclamationProjects.Add(entry.Key, project with
            {
                MonthsInvested = monthsInvested,
                Status = outcome == LandReclamationOutcome.FullReclamation ? LandReclamationStatus.CompletedFull : LandReclamationStatus.CompletedPartial,
                ResolvedOutcome = outcome,
            });

            ApplyOutcome(state, plot!, outcome);
            var completedEvent = new LandReclamationCompletedEvent(state.EventIds.Issue(), date, project.PlotId, outcome, CausationId: null);
            events.Add(completedEvent);

            if (outcome == LandReclamationOutcome.FullReclamation && TryResolveOwningHousehold(plot!, out var ownerHouseholdId))
            {
                events.AddRange(Reputation.AdjustDignitasCommands.Pipeline.Execute(
                    state, new Reputation.AdjustDignitasCommand(
                        state.CommandIds.Issue(), "system", date, completedEvent.EventId.ToTaggedString(), ownerHouseholdId,
                        PrivateInfrastructureCatalog.FullReclamationDignitasAward, "privateInfrastructure.fullReclamation")).Events);
            }
        }

        return events;
    }

    private static bool TryResolveOwningHousehold(Plot plot, out RuntimeId<Household> householdId)
    {
        householdId = default;
        if (plot.OwnerId is null)
            return false;
        RealEstate.PropertyOwnerRef owner;
        try
        {
            owner = RealEstate.PropertyOwnerRef.Parse(plot.OwnerId);
        }
        catch (FormatException)
        {
            return false;
        }
        if (owner.Kind != RealEstate.PropertyOwnerKind.PlayerHousehold || owner.OwnerId is not { } ownerId)
            return false;

        householdId = RuntimeId<Household>.Parse(ownerId);
        return true;
    }

    private static bool TryPayMonthlyCost(WorldState state, GameDate date, Plot plot)
    {
        if (!TryResolveOwningHousehold(plot, out var householdId))
            return false;

        var account = LedgerAccountKey.ForHousehold(householdId);
        var balance = state.LedgerAccounts.TryGet(account, out var ledgerAccount) ? ledgerAccount!.Balance : Money.Zero;
        if (balance.RawValue < PrivateInfrastructureCatalog.LandReclamationMonthlyCost.RawValue)
            return false;

        LedgerService.Post(
            state, date, LedgerTransactionCategory.Construction,
            new[]
            {
                new LedgerPosting(account, -PrivateInfrastructureCatalog.LandReclamationMonthlyCost),
                new LedgerPosting(LedgerAccountKey.Mint, PrivateInfrastructureCatalog.LandReclamationMonthlyCost),
            },
            reference: $"privateInfrastructure.landReclamation:{plot.Id.ToTaggedString()}");
        return true;
    }

    private static void ApplyOutcome(WorldState state, Plot plot, LandReclamationOutcome outcome)
    {
        state.Plots.Remove(plot.Id);
        state.Plots.Add(plot.Id, outcome == LandReclamationOutcome.FullReclamation
            ? plot with { Terrain = TerrainType.FertilePlain, Condition = LandCondition.Pristine }
            : plot with { Condition = new LandCondition(Math.Max(plot.Condition.Value, PrivateInfrastructureCatalog.PartialReclamationConditionFloor)) });
    }
}
