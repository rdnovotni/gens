using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.State;

/// <summary>
/// The calendar-queue ordering key (Phase 4 item 4): due date first, then the issuing action ID as a
/// stable tiebreak, so two actions due the same month still drain in a fixed, deterministic order
/// (ADR 0004) rather than relying on <c>PriorityQueue&lt;T,T&gt;</c>'s undocumented tie-break
/// behavior. Because <see cref="OrderedRegistry{TId,TEntity}"/> always iterates in ascending
/// <typeparamref name="TId"/> order, keying it on this struct makes "drain everything due by this
/// tick's date" a plain ascending scan.
/// </summary>
public readonly record struct ScheduledActionKey(GameDate DueDate, RuntimeId<ScheduledAction> ActionId) : IComparable<ScheduledActionKey>
{
    public int CompareTo(ScheduledActionKey other)
    {
        var dueDateComparison = DueDate.TotalMonths.CompareTo(other.DueDate.TotalMonths);
        return dueDateComparison != 0 ? dueDateComparison : ActionId.CompareTo(other.ActionId);
    }

    public static bool operator <(ScheduledActionKey left, ScheduledActionKey right) => left.CompareTo(right) < 0;
    public static bool operator >(ScheduledActionKey left, ScheduledActionKey right) => left.CompareTo(right) > 0;
    public static bool operator <=(ScheduledActionKey left, ScheduledActionKey right) => left.CompareTo(right) <= 0;
    public static bool operator >=(ScheduledActionKey left, ScheduledActionKey right) => left.CompareTo(right) >= 0;
}

/// <summary>
/// One future-dated, not-yet-dispatched piece of work in the calendar queue. Dispatch happens
/// through the same one command path every other actor uses (rule 2): when a scheduled action
/// becomes due, it is submitted as a <see cref="Campaign.GenericCommand"/> through
/// <see cref="Commands.CommandPipeline{TState,TCommand}"/>, never applied as a direct mutation.
/// </summary>
public sealed record ScheduledActionEntry(
    RuntimeId<ScheduledAction> ActionId,
    GameDate DueDate,
    string ActorId,
    string ActionType,
    string PayloadJson,
    string? CausationId);
