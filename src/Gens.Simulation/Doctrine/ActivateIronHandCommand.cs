using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Doctrine;

/// <summary>Emitted whenever an <see cref="ActivateIronHandCommand"/> is accepted. Public, matching
/// <see cref="AncestralSanctionInvokedEvent"/>'s and <see cref="GreatRitePerformedEvent"/>'s own
/// reasoning — a household openly running its estate this way is not a secret.</summary>
public sealed record IronHandActivatedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    string? CausationId) : IDomainEvent
{
    public string Type => "doctrine.ironHandActivated";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// Domus Dura's Defining capstone (§3.2: "Iron Hand — the single highest sustained labor-output
/// multiplier in the project — genuinely double-edged, arriving with a permanent Unrest/flight-risk/
/// Legal-scrutiny baseline increase that doesn't recede even if the household's policies later
/// soften"). Unlike <see cref="InvokeAncestralSanctionCommand"/> and <see
/// cref="PerformGreatRiteCommand"/>, this capstone has no other state to mutate beyond the flag itself
/// — its real numeric effects (<see cref="DoctrineCatalog.IronHandOutputCeilingBonus"/>, <see
/// cref="DoctrineCatalog.IronHandFlightRiskBaselineIncrease"/>) are read by <see
/// cref="DoctrineLaborModifierQuery"/> off <see cref="HouseholdDoctrineState.CapstoneUsedThisGeneration"/>
/// directly, matching <see cref="Epithets.Agnomen"/>'s own "the flag is the documented hook" precedent:
/// <see cref="Characters.LaborOutputSystem"/> and <see cref="Characters.LaborFlightSystem"/> are both
/// already-shipped, already-tested systems this item does not reopen to actually fold that projection
/// into their own live formulas (see <see cref="DoctrineLaborModifierQuery"/>'s own doc comment).
/// Deliberately never clears once set — "doesn't recede" is the entire point of this capstone, so unlike
/// every other capstone command's <c>CapstoneUsedThisGeneration</c> flag (which exists only to block a
/// second invocation), this one is also the permanent effect itself.
/// </summary>
public sealed record ActivateIronHandCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId) : ICommand;

/// <summary>The validate/mutate pipeline for <see cref="ActivateIronHandCommand"/> (ADR 0006).</summary>
public static class ActivateIronHandCommands
{
    public static readonly ValidationErrorCode DoctrineNotDefining = new("doctrine.activateIronHand.doctrineNotDefining");
    public static readonly ValidationErrorCode AlreadyActive = new("doctrine.activateIronHand.alreadyActive");

    public static readonly CommandPipeline<WorldState, ActivateIronHandCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, ActivateIronHandCommand command)
    {
        var doctrine = HouseholdDoctrineResolver.Current(state, command.HouseholdId, HouseholdDoctrineType.DomusDura);
        if (doctrine.Tier != DoctrineTier.Defining)
            return DoctrineNotDefining;
        if (doctrine.CapstoneUsedThisGeneration)
            return AlreadyActive;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, ActivateIronHandCommand command)
    {
        var doctrine = HouseholdDoctrineResolver.Current(state, command.HouseholdId, HouseholdDoctrineType.DomusDura);
        HouseholdDoctrineResolver.Set(state, doctrine with { CapstoneUsedThisGeneration = true });

        return new IDomainEvent[]
        {
            new IronHandActivatedEvent(state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, command.CommandId.ToTaggedString()),
        };
    }
}

/// <summary>A pure, non-mutating read of the Iron Hand's own projected numeric effects (<see
/// cref="DoctrineCatalog"/>) — matching <see cref="Policies.RitesBudgetCatalog"/>'s and <see
/// cref="Policies.HouseholdPolicyModifiersQuery"/>'s own "the projection exists before its consumer
/// does" precedent. Neither <see cref="Characters.LaborOutputSystem"/> nor <see
/// cref="Characters.LaborFlightSystem"/> reads this query in this item — see <see
/// cref="ActivateIronHandCommand"/>'s own doc comment for why reopening either is out of this item's
/// scope.</summary>
public static class DoctrineLaborModifierQuery
{
    public static bool IsIronHandActive(WorldState state, RuntimeId<Household> householdId) =>
        HouseholdDoctrineResolver.Current(state, householdId, HouseholdDoctrineType.DomusDura).CapstoneUsedThisGeneration;

    public static int OutputCeilingBonus(WorldState state, RuntimeId<Household> householdId) =>
        IsIronHandActive(state, householdId) ? DoctrineCatalog.IronHandOutputCeilingBonus : 0;

    public static int FlightRiskBaselineIncrease(WorldState state, RuntimeId<Household> householdId) =>
        IsIronHandActive(state, householdId) ? DoctrineCatalog.IronHandFlightRiskBaselineIncrease : 0;
}
