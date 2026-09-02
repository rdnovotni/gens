using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.RealEstate;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using Gens.Simulation.Wanderers;

namespace Gens.Simulation.NotableBusinesses;

/// <summary>§6's Named Suppliers (Phase 15 item 4): sets or clears a Notable Business's own <see
/// cref="NotableBusiness.MainSupplier"/>. Validates only that the referenced entity actually exists —
/// a Household is not itself checked (no dedicated Household existence registry exists anywhere in this
/// codebase, matching <see cref="RealEstate.PropertyOwnerRef"/>'s own identical "household existence is
/// not directly checkable" narrowing), a Character must be alive, a <see cref="PropertyRecord"/> must be
/// registered, and a <see cref="Wanderer"/> must be registered.</summary>
public sealed record SetMainSupplierCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<NotableBusiness> BusinessId,
    NotableBusinessSupplierRef? Supplier) : ICommand;

public sealed record MainSupplierSetEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<NotableBusiness> BusinessId,
    NotableBusinessSupplierRef? Supplier,
    string? CausationId) : IDomainEvent
{
    public string Type => "notableBusinesses.mainSupplierSet";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { BusinessId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class SetMainSupplierCommands
{
    public static readonly ValidationErrorCode BusinessNotFound = new("notableBusinesses.setMainSupplier.businessNotFound");
    public static readonly ValidationErrorCode SupplierNotFound = new("notableBusinesses.setMainSupplier.supplierNotFound");

    public static readonly CommandPipeline<WorldState, SetMainSupplierCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, SetMainSupplierCommand command)
    {
        if (!state.NotableBusinesses.TryGet(command.BusinessId, out _))
            return BusinessNotFound;

        if (command.Supplier is { } supplier && !SupplierResolvable(state, supplier))
            return SupplierNotFound;

        return null;
    }

    private static bool SupplierResolvable(WorldState state, NotableBusinessSupplierRef supplier) => supplier.Kind switch
    {
        NotableBusinessSupplierKind.Household => true,
        NotableBusinessSupplierKind.Character =>
            state.Characters.TryGet(RuntimeId<Character>.Parse(supplier.RefId), out var character) && character!.IsAlive,
        NotableBusinessSupplierKind.PropertyRecord => state.PropertyRecords.TryGet(RuntimeId<PropertyRecord>.Parse(supplier.RefId), out _),
        NotableBusinessSupplierKind.Wanderer => state.Wanderers.TryGet(RuntimeId<Wanderer>.Parse(supplier.RefId), out _),
        _ => false,
    };

    private static IDomainEvent[] Mutate(WorldState state, SetMainSupplierCommand command)
    {
        state.NotableBusinesses.TryGet(command.BusinessId, out var business);
        state.NotableBusinesses.Remove(command.BusinessId);
        // A new supplier relationship starts with a clean slate, per NotableBusiness.SupplierDisruptionApplied's
        // own doc comment.
        state.NotableBusinesses.Add(
            command.BusinessId, business! with { MainSupplier = command.Supplier, SupplierDisruptionApplied = false });

        return new IDomainEvent[]
        {
            new MainSupplierSetEvent(state.EventIds.Issue(), command.SubmittedDate, command.BusinessId, command.Supplier, command.CommandId.ToTaggedString()),
        };
    }
}

/// <summary>§6's "a supplier's own bad harvest, bankruptcy, or a... loss genuinely disrupts the
/// dependent business's own Output" (Phase 15 item 4) — the one real, checkable half of that sentence
/// this codebase can actually drive a monthly tick from: a <see
/// cref="NotableBusinessSupplierKind.Household"/> supplier's own <see cref="InsolvencyState"/>. A
/// supplier's "bad harvest" has no single real signal to read (Resources &amp; Goods carries no
/// per-household harvest-failure flag), and "a Piracy &amp; Banditry loss" needs that system, confirmed
/// unbuilt (Phase 16) by direct search — both are honestly left unwired rather than approximated.
/// Applies <see cref="NotableBusinessesCatalog.SupplierDisruptionReputationLoss"/> exactly once per
/// disruption bout (<see cref="NotableBusiness.SupplierDisruptionApplied"/> guards a repeat penalty
/// every month the same Insolvency persists); a fresh <see cref="SetMainSupplierCommand"/> clears that
/// guard for the new relationship.</summary>
public static class SupplierDisruptionSystem
{
    public static IReadOnlyList<IDomainEvent> Tick(WorldState state, GameDate date)
    {
        var events = new List<IDomainEvent>();

        foreach (var entry in state.NotableBusinesses.InAscendingOrder().ToArray())
        {
            var business = entry.Value;
            if (business.Status != NotableBusinessStatus.Tracked || business.SupplierDisruptionApplied)
                continue;
            if (business.MainSupplier is not { Kind: NotableBusinessSupplierKind.Household } supplier)
                continue;

            var householdId = RuntimeId<Household>.Parse(supplier.RefId);
            if (!state.InsolvencyStates.TryGet(householdId, out var insolvency))
                continue;
            if (insolvency!.Stage is not (InsolvencyStage.Insolvent or InsolvencyStage.Ruined))
                continue;

            events.AddRange(AdjustBusinessReputationCommands.Pipeline.Execute(
                state, new AdjustBusinessReputationCommand(
                    state.CommandIds.Issue(), "system", date, null, entry.Key,
                    -NotableBusinessesCatalog.SupplierDisruptionReputationLoss, BusinessReputationChangeReason.SupplyFailure)).Events);

            state.NotableBusinesses.TryGet(entry.Key, out var updated);
            state.NotableBusinesses.Remove(entry.Key);
            state.NotableBusinesses.Add(entry.Key, updated! with { SupplierDisruptionApplied = true });
        }

        return events;
    }
}
