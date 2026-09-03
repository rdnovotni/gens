using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.PrivateInfrastructure;

/// <summary>§10's <c>structureType</c> vocabulary, matching the data model's own four-value (plus
/// Boundary) enumeration exactly.</summary>
public enum InfrastructureStructureType
{
    PavedRoad,
    IrrigationCanal,
    WellOrCistern,
    PrivateBridge,
    BoundaryInfrastructure,
}

/// <summary>The key <see cref="InfrastructureCondition"/> is stored under — a structure type plus the
/// structure's own tagged ID string, since the structures this item tracks are keyed under different
/// shapes (some by <see cref="Identity.RuntimeId{Plot}"/>, some by their own <see
/// cref="Identity.RuntimeId{T}"/>) with no single ID type that could name every one of them, the same
/// "no single RuntimeId&lt;T&gt; could name every owner kind" reasoning <see
/// cref="RealEstate.PropertyOwnerRef"/>'s own doc comment already gives, applied here to structure
/// identity instead of ownership.</summary>
public readonly record struct InfrastructureConditionKey(InfrastructureStructureType StructureType, string StructureTag)
    : IComparable<InfrastructureConditionKey>
{
    public int CompareTo(InfrastructureConditionKey other)
    {
        var typeCompare = StructureType.CompareTo(other.StructureType);
        return typeCompare != 0 ? typeCompare : string.CompareOrdinal(StructureTag, other.StructureTag);
    }

    public static bool operator <(InfrastructureConditionKey left, InfrastructureConditionKey right) => left.CompareTo(right) < 0;
    public static bool operator >(InfrastructureConditionKey left, InfrastructureConditionKey right) => left.CompareTo(right) > 0;
    public static bool operator <=(InfrastructureConditionKey left, InfrastructureConditionKey right) => left.CompareTo(right) <= 0;
    public static bool operator >=(InfrastructureConditionKey left, InfrastructureConditionKey right) => left.CompareTo(right) >= 0;
}

/// <summary>§10's <c>InfrastructureCondition</c> — one entry per built structure this namespace tracks
/// (Phase 15 item 7), reading the same 0-100 scale <see cref="Land.LandCondition"/> already uses for a
/// Plot itself (§8's own "reads the same scale as Estate &amp; Settlement's Plot condition field").
/// Present only once a structure has actually been built — an absent entry is never queried directly;
/// each Build*Command seeds one at <see cref="PrivateInfrastructureCatalog.PristineCondition"/> the
/// same tick it constructs the structure it describes.</summary>
public sealed record InfrastructureCondition
{
    public required InfrastructureConditionKey Key { get; init; }
    public required int Condition { get; init; }
    public RuntimeId<Hazards.DisasterEvent>? LastDisasterEventRef { get; init; }

    public static InfrastructureCondition Pristine(InfrastructureConditionKey key) => new()
    {
        Key = key,
        Condition = PrivateInfrastructureCatalog.PristineCondition,
        LastDisasterEventRef = null,
    };

    public static InfrastructureCondition Restore(
        InfrastructureConditionKey key, int condition, RuntimeId<Hazards.DisasterEvent>? lastDisasterEventRef) => new()
        {
            Key = key,
            Condition = condition,
            LastDisasterEventRef = lastDisasterEventRef,
        };
}

/// <summary>Read/write helpers over <see cref="WorldState.InfrastructureConditions"/>, matching <see
/// cref="RealEstate.PlotPropertyResolver"/>'s identical "remove then re-add, current-or-seed" shape.</summary>
public static class InfrastructureConditionResolver
{
    public static InfrastructureCondition Current(WorldState state, InfrastructureConditionKey key) =>
        state.InfrastructureConditions.TryGet(key, out var entry) ? entry! : InfrastructureCondition.Pristine(key);

    public static void Seed(WorldState state, InfrastructureConditionKey key) => Set(state, InfrastructureCondition.Pristine(key));

    public static void Set(WorldState state, InfrastructureCondition condition)
    {
        if (state.InfrastructureConditions.TryGet(condition.Key, out _))
            state.InfrastructureConditions.Remove(condition.Key);
        state.InfrastructureConditions.Add(condition.Key, condition);
    }

    /// <summary>Whether a structure's own real effect (Commerce bonus, Fertility/Drought reduction,
    /// rustling-risk reduction) is still active — §8's "neglect degrades the improvement's own effect
    /// over time" read as a binary lapse below <see
    /// cref="PrivateInfrastructureCatalog.MinimumOperationalCondition"/>, mirroring <see
    /// cref="Buildings.BuildingInstance.IsOperational"/>'s own operational/not binary applied to
    /// condition.</summary>
    public static bool IsOperational(WorldState state, InfrastructureConditionKey key) =>
        Current(state, key).Condition >= PrivateInfrastructureCatalog.MinimumOperationalCondition;
}

/// <summary>§8's "recoverable through the same Repair action" — a real, funded restoration of one
/// structure's condition, mirroring <see cref="Buildings.BuildingInstance.Repair"/>'s own shape but as
/// a real command (this namespace's structures are immutable records, not mutable classes) rather than
/// a mutating method.</summary>
public sealed record RepairInfrastructureCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    InfrastructureConditionKey Key,
    RuntimeId<Household> PayingHouseholdId) : ICommand;

public sealed record InfrastructureRepairedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    InfrastructureConditionKey Key,
    int PreviousCondition,
    int NewCondition,
    string? CausationId) : IDomainEvent
{
    public string Type => "privateInfrastructure.repaired";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { Key.StructureTag };
    public Visibility Visibility => Visibility.Public;
}

public static class RepairInfrastructureCommands
{
    public static readonly ValidationErrorCode AlreadyPristine = new("privateInfrastructure.repair.alreadyPristine");
    public static readonly ValidationErrorCode InsufficientFunds = new("privateInfrastructure.repair.insufficientFunds");

    public static readonly CommandPipeline<WorldState, RepairInfrastructureCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, RepairInfrastructureCommand command)
    {
        var current = InfrastructureConditionResolver.Current(state, command.Key);
        if (current.Condition >= PrivateInfrastructureCatalog.PristineCondition)
            return AlreadyPristine;

        var pointsRestored = Math.Min(
            PrivateInfrastructureCatalog.RepairConditionRestored, PrivateInfrastructureCatalog.PristineCondition - current.Condition);
        var cost = PrivateInfrastructureCatalog.RepairCostPerConditionPoint.Scale(Numerics.Fixed64.FromInt(pointsRestored));
        var account = LedgerAccountKey.ForHousehold(command.PayingHouseholdId);
        var balance = state.LedgerAccounts.TryGet(account, out var ledgerAccount) ? ledgerAccount!.Balance : Money.Zero;
        if (balance.RawValue < cost.RawValue)
            return InsufficientFunds;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, RepairInfrastructureCommand command)
    {
        var current = InfrastructureConditionResolver.Current(state, command.Key);
        var pointsRestored = Math.Min(
            PrivateInfrastructureCatalog.RepairConditionRestored, PrivateInfrastructureCatalog.PristineCondition - current.Condition);
        var cost = PrivateInfrastructureCatalog.RepairCostPerConditionPoint.Scale(Numerics.Fixed64.FromInt(pointsRestored));
        var newCondition = Math.Min(PrivateInfrastructureCatalog.PristineCondition, current.Condition + pointsRestored);

        var events = new List<IDomainEvent>();
        events.Add(LedgerService.Post(
            state, command.SubmittedDate, LedgerTransactionCategory.Upkeep,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(command.PayingHouseholdId), -cost),
                new LedgerPosting(LedgerAccountKey.Mint, cost),
            },
            reference: $"privateInfrastructure.repair:{command.Key.StructureType}:{command.Key.StructureTag}"));

        InfrastructureConditionResolver.Set(state, current with { Condition = newCondition });

        events.Add(new InfrastructureRepairedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.Key, current.Condition, newCondition, command.CommandId.ToTaggedString()));
        return events.ToArray();
    }
}
