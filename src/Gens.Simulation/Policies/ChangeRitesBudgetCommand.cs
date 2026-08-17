using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Policies;

/// <summary>Changes a household's standing Rites Budget tier (<c>gens-policies-edicts-design.md</c>
/// §2.3) — Phase 9 item 2's "policy change command." Distinct from a one-off <see
/// cref="FundFestivalCommand"/>: this is a toggled, continuous dial, not a single-stroke spend
/// (§1's own "does it sit toggled, or does it fire once?" test).</summary>
public sealed record ChangeRitesBudgetCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RitesBudgetTier NewTier) : ICommand;

/// <summary>Emitted whenever a <see cref="ChangeRitesBudgetCommand"/> is accepted.</summary>
public sealed record RitesBudgetChangedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RitesBudgetTier PreviousTier,
    RitesBudgetTier NewTier,
    string? CausationId) : IDomainEvent
{
    public string Type => "policies.ritesBudgetChanged";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="ChangeRitesBudgetCommand"/> (ADR 0006).</summary>
public static class ChangeRitesBudgetCommands
{
    public static readonly ValidationErrorCode TierUnchanged = new("policies.changeRitesBudget.tierUnchanged");
    public static readonly ValidationErrorCode OnCooldown = new("policies.changeRitesBudget.onCooldown");

    public static readonly CommandPipeline<WorldState, ChangeRitesBudgetCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, ChangeRitesBudgetCommand command)
    {
        if (!state.HouseholdPolicies.TryGet(command.HouseholdId, out var existing))
            return null;

        if (existing!.RitesBudget == command.NewTier)
            return TierUnchanged;

        var monthsSinceLastChange = command.SubmittedDate.TotalMonths - existing.LastChangedDate.TotalMonths;
        if (monthsSinceLastChange < RitesBudgetCatalog.CooldownMonths)
            return OnCooldown;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, ChangeRitesBudgetCommand command)
    {
        var previousTier = state.HouseholdPolicies.TryGet(command.HouseholdId, out var existing)
            ? existing!.RitesBudget
            : RitesBudgetCatalog.Default;

        state.HouseholdPolicies.Remove(command.HouseholdId);
        state.HouseholdPolicies.Add(
            command.HouseholdId,
            new HouseholdPolicyState(command.HouseholdId, command.NewTier, command.SubmittedDate));

        return new IDomainEvent[]
        {
            new RitesBudgetChangedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, previousTier, command.NewTier,
                command.CommandId.ToTaggedString()),
        };
    }
}
