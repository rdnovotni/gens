using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Regions;
using Gens.Simulation.State;
using Gens.Simulation.Stewardship;
using Gens.Simulation.Time;
using Gens.Simulation.Travel;

namespace Gens.Simulation.Land;

/// <summary>Registers a newly-acquired <see cref="Holding"/> as a <see cref="DistantHolding"/> once it
/// sits outside <paramref name="HomeRegionId"/> (§7's "acquire a second holding outside its home
/// region"). Pricing and the underlying land transfer are upstream concerns (<see
/// cref="AcquirePlotCommand"/>'s identical "atomic transfer" scoping) — this command only owns
/// recognizing that the transfer just made the household's holding footprint a distant one.</summary>
public sealed record AcquireDistantHoldingCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    DefinitionId<RegionProfileDefinition> HomeRegionId,
    DefinitionId<RegionProfileDefinition> HoldingRegionId,
    RuntimeId<Holding> HoldingId) : ICommand;

public sealed record DistantHoldingAcquiredEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<DistantHolding> DistantHoldingId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Holding> HoldingId,
    DistanceTier DistanceTier,
    string? CausationId) : IDomainEvent
{
    public string Type => "land.distantHoldingAcquired";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), HoldingId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>Appoints (or, per §7.2, reappoints after a vacancy) a <see cref="DistantHolding"/>'s
/// Procurator — §5.3's "evaluated exactly like any other Senior Position": the real appointment is the
/// same <see cref="StewardshipContext.SecondSettlementProcurator"/> <see cref="StewardshipAssignment"/>
/// every other Senior Position uses, via <see cref="StewardshipCommands.AppointPipeline"/>; this
/// command's own job is folding that assignment's outcome back onto the <see cref="DistantHolding"/>
/// record §12 keys the mismanagement-risk read off of.</summary>
public sealed record AppointProcuratorCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<DistantHolding> DistantHoldingId,
    RuntimeId<Character> ProcuratorCharacterId) : ICommand;

public sealed record ProcuratorAppointedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<DistantHolding> DistantHoldingId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> ProcuratorCharacterId,
    string? CausationId) : IDomainEvent
{
    public string Type => "land.procuratorAppointed";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), ProcuratorCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>Validate/mutate pipelines for the two Distant Holding commands (ADR 0006). Built per <see
/// cref="DistanceTierCatalog"/>, matching <see cref="Travel.BeginTravelCommands.BuildPipeline"/>'s
/// identical "caller-loaded content, not embedded in the save-state graph" shape.</summary>
public static class DistantHoldingCommands
{
    public static readonly ValidationErrorCode HoldingNotFound = new("land.distantHolding.acquire.holdingNotFound");
    public static readonly ValidationErrorCode HoldingNotOwnedByHousehold = new("land.distantHolding.acquire.holdingNotOwnedByHousehold");
    public static readonly ValidationErrorCode NotActuallyDistant = new("land.distantHolding.acquire.notActuallyDistant");
    public static readonly ValidationErrorCode AlreadyRegistered = new("land.distantHolding.acquire.alreadyRegistered");

    public static readonly ValidationErrorCode DistantHoldingNotFound = new("land.distantHolding.appointProcurator.distantHoldingNotFound");
    public static readonly ValidationErrorCode CandidateNotFound = new("land.distantHolding.appointProcurator.candidateNotFound");
    public static readonly ValidationErrorCode CandidateDeceased = new("land.distantHolding.appointProcurator.candidateDeceased");
    public static readonly ValidationErrorCode CandidateNotHouseholdMember = new("land.distantHolding.appointProcurator.candidateNotHouseholdMember");
    public static readonly ValidationErrorCode HouseholdAlreadyHasActiveAssignment = new("land.distantHolding.appointProcurator.householdAlreadyHasActiveAssignment");

    public static CommandPipeline<WorldState, AcquireDistantHoldingCommand> BuildAcquirePipeline(DistanceTierCatalog distanceTiers)
    {
        if (distanceTiers is null)
            throw new ArgumentNullException(nameof(distanceTiers));

        return new CommandPipeline<WorldState, AcquireDistantHoldingCommand>(
            validate: Validate,
            mutate: (state, command) => Mutate(state, command, distanceTiers),
            issueSequenceNumber: static state => state.IssueCommandSequenceNumber());
    }

    public static readonly CommandPipeline<WorldState, AppointProcuratorCommand> AppointProcuratorPipeline = new(
        validate: ValidateAppointProcurator, mutate: MutateAppointProcurator, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, AcquireDistantHoldingCommand command)
    {
        if (!state.Holdings.TryGet(command.HoldingId, out var holding))
            return HoldingNotFound;
        if (holding!.OwnerId != command.HouseholdId.ToTaggedString())
            return HoldingNotOwnedByHousehold;
        if (command.HomeRegionId.Equals(command.HoldingRegionId))
            return NotActuallyDistant;
        if (state.DistantHoldings.InAscendingOrder().Any(entry => entry.Value.HoldingId == command.HoldingId))
            return AlreadyRegistered;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, AcquireDistantHoldingCommand command, DistanceTierCatalog distanceTiers)
    {
        var tier = distanceTiers.Resolve(command.HomeRegionId, command.HoldingRegionId);
        var id = state.DistantHoldingIds.Issue();
        var distantHolding = DistantHolding.Begin(
            id, command.HouseholdId, command.HomeRegionId, command.HoldingRegionId, command.HoldingId, tier);
        state.DistantHoldings.Add(id, distantHolding);

        return new IDomainEvent[]
        {
            new DistantHoldingAcquiredEvent(
                state.EventIds.Issue(), command.SubmittedDate, id, command.HouseholdId, command.HoldingId, tier,
                command.CommandId.ToTaggedString()),
        };
    }

    private static ValidationErrorCode? ValidateAppointProcurator(WorldState state, AppointProcuratorCommand command)
    {
        if (!state.DistantHoldings.TryGet(command.DistantHoldingId, out var distantHolding))
            return DistantHoldingNotFound;
        if (!state.Characters.TryGet(command.ProcuratorCharacterId, out var candidate))
            return CandidateNotFound;
        if (!candidate!.IsAlive)
            return CandidateDeceased;
        if (candidate.Household != distantHolding!.HouseholdId)
            return CandidateNotHouseholdMember;

        var hasActiveAssignment = state.StewardshipAssignments.InAscendingOrder()
            .Any(entry => entry.Value.HouseholdId == distantHolding.HouseholdId && entry.Value.IsActive);
        if (hasActiveAssignment)
            return HouseholdAlreadyHasActiveAssignment;

        return null;
    }

    private static IDomainEvent[] MutateAppointProcurator(WorldState state, AppointProcuratorCommand command)
    {
        state.DistantHoldings.TryGet(command.DistantHoldingId, out var distantHolding);

        var appointCommand = new AppointStewardshipCommand(
            state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
            distantHolding!.HouseholdId, StewardshipContext.SecondSettlementProcurator, StewardshipMode.SingleSteward,
            command.ProcuratorCharacterId, CouncilMembers: null, CouncilHeadCharacterId: null,
            AutonomyLevel: StewardshipAssignment.DefaultAutonomyLevel);
        var appointResult = StewardshipCommands.AppointPipeline.Execute(state, appointCommand);
        if (!appointResult.Accepted)
        {
            // Unreachable in practice: ValidateAppointProcurator already re-checks every condition
            // AppointPipeline's own validate enforces (rule: a command that passed validation must
            // succeed in mutate). Left as a defensive no-op rather than a throw, matching
            // RegencySystem's identical guard around this same nested-pipeline call.
            return Array.Empty<IDomainEvent>();
        }

        var events = new List<IDomainEvent>(appointResult.Events);

        state.DistantHoldings.Remove(command.DistantHoldingId);
        state.DistantHoldings.Add(command.DistantHoldingId, distantHolding with
        {
            ProcuratorCharacterId = command.ProcuratorCharacterId,
            MismanagementRiskActive = DistantHoldingMismanagementRiskSystem.EvaluateRisk(
                distantHolding.DistanceTier, state.Characters.TryGet(command.ProcuratorCharacterId, out var procurator) ? procurator : null),
        });

        events.Add(new ProcuratorAppointedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.DistantHoldingId, distantHolding.HouseholdId,
            command.ProcuratorCharacterId, command.CommandId.ToTaggedString()));

        return events.ToArray();
    }
}
