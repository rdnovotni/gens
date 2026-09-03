using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.RealEstate;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.PrivateInfrastructure;

/// <summary>§3's Irrigation Canal source gate — "built on a River-adjacent Plot, or fed by a private
/// branch off the settlement's own civic Aqueduct where no river is available." No standalone Aqueduct
/// entity exists anywhere in this codebase — <see cref="Land.SettlementStage"/>'s own doc comment is the
/// only place an Aqueduct is named at all ("a full city: a Basilica and an Aqueduct completed"), so this
/// item reads <see cref="SettlementStage.City"/> as the honest, real proxy for "this settlement's own
/// civic Aqueduct exists to branch off of" rather than inventing a separate Aqueduct-built flag with
/// nothing else to set it.</summary>
public enum IrrigationSourceType
{
    RiverAdjacent,
    PrivateAqueductBranch,
}

/// <summary>§3's Irrigation Canal (Phase 15 item 7).</summary>
public sealed record IrrigationCanal
{
    public required RuntimeId<Plot> PlotId { get; init; }
    public required IrrigationSourceType SourceType { get; init; }
    public required GameDate BuiltDate { get; init; }

    public InfrastructureConditionKey ConditionKey => new(InfrastructureStructureType.IrrigationCanal, PlotId.ToTaggedString());

    public static IrrigationCanal Create(RuntimeId<Plot> plotId, IrrigationSourceType sourceType, GameDate builtDate) =>
        new() { PlotId = plotId, SourceType = sourceType, BuiltDate = builtDate };
}

/// <summary>§3.1's lighter Well/Cistern alternative (Phase 15 item 7).</summary>
public enum WellOrCisternType
{
    Well,
    Cistern,
}

public sealed record WellOrCistern
{
    public required RuntimeId<Plot> PlotId { get; init; }
    public required WellOrCisternType Type { get; init; }
    public required GameDate BuiltDate { get; init; }

    public InfrastructureConditionKey ConditionKey => new(InfrastructureStructureType.WellOrCistern, PlotId.ToTaggedString());

    public static WellOrCistern Create(RuntimeId<Plot> plotId, WellOrCisternType type, GameDate builtDate) =>
        new() { PlotId = plotId, Type = type, BuiltDate = builtDate };
}

public sealed record IrrigationCanalBuiltEvent(
    RuntimeId<DomainEventEntity> EventId, GameDate OccurredDate, RuntimeId<Plot> PlotId, IrrigationSourceType SourceType,
    string? CausationId) : IDomainEvent
{
    public string Type => "privateInfrastructure.irrigationCanalBuilt";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { PlotId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public sealed record WellOrCisternBuiltEvent(
    RuntimeId<DomainEventEntity> EventId, GameDate OccurredDate, RuntimeId<Plot> PlotId, WellOrCisternType WellType,
    string? CausationId) : IDomainEvent
{
    public string Type => "privateInfrastructure.wellOrCisternBuilt";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { PlotId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>§3's Irrigation Canal construction (Phase 15 item 7). Gated on §3's own source rule (see
/// <see cref="IrrigationSourceType"/>'s doc comment); §11's own open question ("whether a single
/// structure can serve multiple Plots or whether each Plot needs its own dedicated construction") is
/// read as the latter — one Canal per Plot — the simpler, more conservative reading this item can
/// actually implement without inventing a multi-Plot service-area concept nothing else in this codebase
/// has a precedent for.</summary>
public sealed record BuildIrrigationCanalCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Plot> PlotId) : ICommand;

public static class BuildIrrigationCanalCommands
{
    public static readonly ValidationErrorCode PlotNotFound = new("privateInfrastructure.buildIrrigationCanal.plotNotFound");
    public static readonly ValidationErrorCode NotOwnedByHousehold = new("privateInfrastructure.buildIrrigationCanal.notOwnedByHousehold");
    public static readonly ValidationErrorCode NoEligibleSource = new("privateInfrastructure.buildIrrigationCanal.noEligibleSource");
    public static readonly ValidationErrorCode AlreadyBuilt = new("privateInfrastructure.buildIrrigationCanal.alreadyBuilt");
    public static readonly ValidationErrorCode InsufficientFunds = new("privateInfrastructure.buildIrrigationCanal.insufficientFunds");

    public static readonly CommandPipeline<WorldState, BuildIrrigationCanalCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, BuildIrrigationCanalCommand command)
    {
        if (!state.Plots.TryGet(command.PlotId, out var plot))
            return PlotNotFound;
        if (!PrivateInfrastructureOwnership.OwnedByHousehold(plot!, command.HouseholdId))
            return NotOwnedByHousehold;
        if (state.IrrigationCanals.TryGet(command.PlotId, out _))
            return AlreadyBuilt;
        if (!TryResolveSource(state, plot!, out _))
            return NoEligibleSource;

        var cost = PrivateInfrastructureCatalog.IrrigationCanalConstructionCost;
        var account = LedgerAccountKey.ForHousehold(command.HouseholdId);
        var balance = state.LedgerAccounts.TryGet(account, out var ledgerAccount) ? ledgerAccount!.Balance : Money.Zero;
        if (balance.RawValue < cost.RawValue)
            return InsufficientFunds;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, BuildIrrigationCanalCommand command)
    {
        state.Plots.TryGet(command.PlotId, out var plot);
        TryResolveSource(state, plot!, out var sourceType);

        var events = new List<IDomainEvent>();
        events.Add(LedgerService.Post(
            state, command.SubmittedDate, LedgerTransactionCategory.Construction,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(command.HouseholdId), -PrivateInfrastructureCatalog.IrrigationCanalConstructionCost),
                new LedgerPosting(LedgerAccountKey.Mint, PrivateInfrastructureCatalog.IrrigationCanalConstructionCost),
            },
            reference: $"privateInfrastructure.buildIrrigationCanal:{command.PlotId.ToTaggedString()}"));

        var canal = IrrigationCanal.Create(command.PlotId, sourceType, command.SubmittedDate);
        state.IrrigationCanals.Add(command.PlotId, canal);
        InfrastructureConditionResolver.Seed(state, canal.ConditionKey);
        PrivateInfrastructureOwnership.BumpPropertyValue(state, command.PlotId);

        events.Add(new IrrigationCanalBuiltEvent(state.EventIds.Issue(), command.SubmittedDate, command.PlotId, sourceType, command.CommandId.ToTaggedString()));
        return events.ToArray();
    }

    private static bool TryResolveSource(WorldState state, Plot plot, out IrrigationSourceType sourceType)
    {
        if (plot.Terrain == TerrainType.River || plot.Features.HasFlag(TerrainFeature.RiverAdjacent))
        {
            sourceType = IrrigationSourceType.RiverAdjacent;
            return true;
        }
        if (state.Settlements.TryGet(plot.SettlementId, out var settlement) && settlement!.Stage >= SettlementStage.City)
        {
            sourceType = IrrigationSourceType.PrivateAqueductBranch;
            return true;
        }
        sourceType = default;
        return false;
    }
}

/// <summary>§3.1's Well/Cistern construction (Phase 15 item 7) — no River/Aqueduct requirement at all,
/// per that section's own "the honest, lower-ceiling option for a household whose land simply isn't
/// positioned for a full Irrigation Canal."</summary>
public sealed record BuildWellOrCisternCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Plot> PlotId,
    WellOrCisternType WellType) : ICommand;

public static class BuildWellOrCisternCommands
{
    public static readonly ValidationErrorCode PlotNotFound = new("privateInfrastructure.buildWellOrCistern.plotNotFound");
    public static readonly ValidationErrorCode NotOwnedByHousehold = new("privateInfrastructure.buildWellOrCistern.notOwnedByHousehold");
    public static readonly ValidationErrorCode AlreadyBuilt = new("privateInfrastructure.buildWellOrCistern.alreadyBuilt");
    public static readonly ValidationErrorCode InsufficientFunds = new("privateInfrastructure.buildWellOrCistern.insufficientFunds");

    public static readonly CommandPipeline<WorldState, BuildWellOrCisternCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, BuildWellOrCisternCommand command)
    {
        if (!state.Plots.TryGet(command.PlotId, out var plot))
            return PlotNotFound;
        if (!PrivateInfrastructureOwnership.OwnedByHousehold(plot!, command.HouseholdId))
            return NotOwnedByHousehold;
        if (state.WellOrCisterns.TryGet(command.PlotId, out _))
            return AlreadyBuilt;

        var cost = command.WellType == WellOrCisternType.Well
            ? PrivateInfrastructureCatalog.WellConstructionCost
            : PrivateInfrastructureCatalog.CisternConstructionCost;
        var account = LedgerAccountKey.ForHousehold(command.HouseholdId);
        var balance = state.LedgerAccounts.TryGet(account, out var ledgerAccount) ? ledgerAccount!.Balance : Money.Zero;
        if (balance.RawValue < cost.RawValue)
            return InsufficientFunds;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, BuildWellOrCisternCommand command)
    {
        var cost = command.WellType == WellOrCisternType.Well
            ? PrivateInfrastructureCatalog.WellConstructionCost
            : PrivateInfrastructureCatalog.CisternConstructionCost;

        var events = new List<IDomainEvent>();
        events.Add(LedgerService.Post(
            state, command.SubmittedDate, LedgerTransactionCategory.Construction,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(command.HouseholdId), -cost),
                new LedgerPosting(LedgerAccountKey.Mint, cost),
            },
            reference: $"privateInfrastructure.buildWellOrCistern:{command.PlotId.ToTaggedString()}"));

        var structure = WellOrCistern.Create(command.PlotId, command.WellType, command.SubmittedDate);
        state.WellOrCisterns.Add(command.PlotId, structure);
        InfrastructureConditionResolver.Seed(state, structure.ConditionKey);
        PrivateInfrastructureOwnership.BumpPropertyValue(state, command.PlotId);

        events.Add(new WellOrCisternBuiltEvent(state.EventIds.Issue(), command.SubmittedDate, command.PlotId, command.WellType, command.CommandId.ToTaggedString()));
        return events.ToArray();
    }
}

/// <summary>Shared ownership/Property-Value helpers reused across every Build*Command in this
/// namespace, factored out of <see cref="BuildPavedRoadConnectionCommands"/> once a second command
/// needed the identical checks.</summary>
internal static class PrivateInfrastructureOwnership
{
    public static bool OwnedByHousehold(Plot plot, RuntimeId<Household> householdId)
    {
        if (plot.OwnerId is null)
            return false;
        try
        {
            var owner = PropertyOwnerRef.Parse(plot.OwnerId);
            return owner.Kind == PropertyOwnerKind.PlayerHousehold && owner.OwnerId == householdId.ToTaggedString();
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public static void BumpPropertyValue(WorldState state, RuntimeId<Plot> plotId)
    {
        var current = PlotPropertyResolver.Current(state, plotId);
        PlotPropertyResolver.Set(state, current with { Value = current.Value + PrivateInfrastructureCatalog.PropertyValueBonusPerStructure });
    }
}
