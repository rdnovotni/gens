using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.PrivateInfrastructure;

public enum BoundaryInfrastructureType
{
    Fence,
    Wall,
}

/// <summary>§7's Boundary Wall/Fence (Phase 15 item 7) — a real, functional counterpart to the
/// Terminus Stone Monument (Monuments &amp; Legacy Building §2.3, that document's own Phase 17,
/// confirmed unbuilt by direct search; the distinction this section draws is between this item's own
/// mechanical structure and a Monument nothing in this codebase can build yet, not between two built
/// things). §7.1's Frontier Security Posture tie-in is honestly narrowed: Policies &amp; Edicts
/// (<c>Gens.Simulation.Policies</c>) carries only the Rites Budget (Phase 9 item 2's own scope) —
/// confirmed by direct search, no Frontier Security Posture/Fortify-Patrol-Minimal Garrison dial exists
/// anywhere in this codebase — so <see cref="PairedWithFortifyPosture"/> is a real, always-<c>false</c>
/// field until that Policies &amp; Edicts item is built, rather than a faked read of a setting that does
/// not exist, matching this namespace's own identical honest-gap discipline for §7's rustling-risk
/// reduction (no Piracy &amp; Banditry raid system exists to consume that figure either).</summary>
public sealed record BoundaryInfrastructure
{
    public required RuntimeId<Plot> PlotId { get; init; }
    public required BoundaryInfrastructureType Type { get; init; }
    public required bool ConfinementBacking { get; init; }
    public required bool PairedWithFortifyPosture { get; init; }
    public required GameDate BuiltDate { get; init; }

    public InfrastructureConditionKey ConditionKey => new(InfrastructureStructureType.BoundaryInfrastructure, PlotId.ToTaggedString());

    public Numerics.Fixed64 RustlingRiskReduction => Type == BoundaryInfrastructureType.Wall
        ? PrivateInfrastructureCatalog.WallRustlingRiskReduction
        : PrivateInfrastructureCatalog.FenceRustlingRiskReduction;

    public static BoundaryInfrastructure Create(
        RuntimeId<Plot> plotId, BoundaryInfrastructureType type, bool confinementBacking, GameDate builtDate) => new()
        {
            PlotId = plotId,
            Type = type,
            ConfinementBacking = confinementBacking,
            PairedWithFortifyPosture = false,
            BuiltDate = builtDate,
        };
}

public sealed record BoundaryInfrastructureBuiltEvent(
    RuntimeId<DomainEventEntity> EventId, GameDate OccurredDate, RuntimeId<Plot> PlotId, BoundaryInfrastructureType BoundaryType,
    bool ConfinementBacking, string? CausationId) : IDomainEvent
{
    public string Type => "privateInfrastructure.boundaryInfrastructureBuilt";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { PlotId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>§7's Boundary Wall/Fence construction (Phase 15 item 7). §7's own "gives... Permitted
/// Freedoms axis a real physical backing on plots where confinement is the active policy" is read live
/// at build time off <see cref="RegimenResolver"/>'s own household-default read for the owning
/// household (<see cref="DutySlot"/>-independent, since no Plot-scoped Regimen concept exists — the
/// household's own whole-household default is the closest real, already-tracked setting) — <see
/// cref="FreedomsTier.Confined"/> or <see cref="FreedomsTier.Restricted"/> both count, per that axis's
/// own two non-<see cref="FreedomsTier.FreeMovement"/> tiers.</summary>
public sealed record BuildBoundaryInfrastructureCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Plot> PlotId,
    BoundaryInfrastructureType BoundaryType) : ICommand;

public static class BuildBoundaryInfrastructureCommands
{
    public static readonly ValidationErrorCode PlotNotFound = new("privateInfrastructure.buildBoundaryInfrastructure.plotNotFound");
    public static readonly ValidationErrorCode NotOwnedByHousehold = new("privateInfrastructure.buildBoundaryInfrastructure.notOwnedByHousehold");
    public static readonly ValidationErrorCode AlreadyBuilt = new("privateInfrastructure.buildBoundaryInfrastructure.alreadyBuilt");
    public static readonly ValidationErrorCode InsufficientFunds = new("privateInfrastructure.buildBoundaryInfrastructure.insufficientFunds");

    public static readonly CommandPipeline<WorldState, BuildBoundaryInfrastructureCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, BuildBoundaryInfrastructureCommand command)
    {
        if (!state.Plots.TryGet(command.PlotId, out var plot))
            return PlotNotFound;
        if (!PrivateInfrastructureOwnership.OwnedByHousehold(plot!, command.HouseholdId))
            return NotOwnedByHousehold;
        if (state.BoundaryInfrastructures.TryGet(command.PlotId, out _))
            return AlreadyBuilt;

        var cost = CostFor(command.BoundaryType);
        var account = LedgerAccountKey.ForHousehold(command.HouseholdId);
        var balance = state.LedgerAccounts.TryGet(account, out var ledgerAccount) ? ledgerAccount!.Balance : Money.Zero;
        if (balance.RawValue < cost.RawValue)
            return InsufficientFunds;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, BuildBoundaryInfrastructureCommand command)
    {
        var cost = CostFor(command.BoundaryType);
        var events = new List<IDomainEvent>();
        events.Add(LedgerService.Post(
            state, command.SubmittedDate, LedgerTransactionCategory.Construction,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(command.HouseholdId), -cost),
                new LedgerPosting(LedgerAccountKey.Mint, cost),
            },
            reference: $"privateInfrastructure.buildBoundaryInfrastructure:{command.PlotId.ToTaggedString()}"));

        var confinementBacking = IsConfinementActive(state, command.HouseholdId);
        var structure = BoundaryInfrastructure.Create(command.PlotId, command.BoundaryType, confinementBacking, command.SubmittedDate);
        state.BoundaryInfrastructures.Add(command.PlotId, structure);
        InfrastructureConditionResolver.Seed(state, structure.ConditionKey);
        PrivateInfrastructureOwnership.BumpPropertyValue(state, command.PlotId);

        events.Add(new BoundaryInfrastructureBuiltEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.PlotId, command.BoundaryType, confinementBacking, command.CommandId.ToTaggedString()));
        return events.ToArray();
    }

    private static Money CostFor(BoundaryInfrastructureType type) => type == BoundaryInfrastructureType.Wall
        ? PrivateInfrastructureCatalog.WallConstructionCost
        : PrivateInfrastructureCatalog.FenceConstructionCost;

    private static bool IsConfinementActive(WorldState state, RuntimeId<Household> householdId) =>
        state.HouseholdRegimenDefaults.TryGet(new HouseholdRegimenKey(householdId, null), out var regimen) &&
        regimen.Freedoms is FreedomsTier.Confined or FreedomsTier.Restricted;
}
