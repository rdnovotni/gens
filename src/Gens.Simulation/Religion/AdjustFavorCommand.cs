using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Religion;

/// <summary>
/// The one command path every future Favor-moving trigger routes through (Phase 12 item 3), the direct
/// Favor analog of <see cref="Reputation.AdjustDignitasCommand"/> — itself explicitly cited by the task
/// that spawned this item as "the one path everything routes Favor changes through." §2.2/§2.3 name a
/// wide set of already-established sources (sustained worship, a funded festival, a correctly-heeded
/// Omen, holding a Priesthood, an unmaintained shrine, a broken religious oath feeding a future Legal
/// &amp; Court sacrilege case, a scandal involving a priest) — this domain's own commands/systems (<see
/// cref="RespondToOmenCommand"/>, <see cref="CommissionAuspicesCommand"/>, <see
/// cref="FundFestivalCelebrationCommand"/>, <see cref="ObserveFeastDayCommand"/>, <see
/// cref="PriesthoodTrickleSystem"/>, <see cref="FavorCycleSystem"/>) are this item's own real callers of
/// it, exactly the way <see cref="Magistracies.FundAedileWorksCommand"/> and <see
/// cref="Clientela.SalutatioSystem"/> became <c>AdjustDignitasCommand</c>'s own first real callers in
/// item 2; the remainder (a broken religious oath, a priest's scandal) belong to Legal &amp; Court and
/// Scandal — Phase 12 items 4 and 7, both unbuilt — and are left as future callers of this same command,
/// matching <c>AdjustDignitasCommand</c>'s own "no such caller exists yet" precedent from item 1.
/// </summary>
public sealed record AdjustFavorCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    int Delta,
    string Reason) : ICommand;

/// <summary>Emitted whenever an <see cref="AdjustFavorCommand"/> is accepted. <see cref="Visibility"/>
/// is <see cref="Commands.Visibility.Public"/>, matching <see
/// cref="Reputation.AdjustDignitasCommand"/>'s own <c>DignitasChangedEvent</c> reasoning: §2.3 itself
/// treats a household's Favor standing as something "a Traditionalist-leaning political audience...
/// reads... less favorably independent of its actual Dignitas" — legible to outside observers by
/// definition, the same way Dignitas is, not a private two-party fact the way a Clientela favor call-in
/// is.</summary>
public sealed record FavorChangedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    int PreviousFavor,
    int NewFavor,
    string Reason,
    string? CausationId) : IDomainEvent
{
    public string Type => "religion.favorChanged";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="AdjustFavorCommand"/> (ADR 0006).</summary>
public static class AdjustFavorCommands
{
    public static readonly ValidationErrorCode ZeroDelta = new("religion.adjustFavor.zeroDelta");
    public static readonly ValidationErrorCode NoPatronDeityYet = new("religion.adjustFavor.noPatronDeityYet");

    public static readonly CommandPipeline<WorldState, AdjustFavorCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, AdjustFavorCommand command)
    {
        if (command.Delta == 0)
            return ZeroDelta;
        if (!HouseholdReligionResolver.HasChosenPatron(state, command.HouseholdId))
            return NoPatronDeityYet;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, AdjustFavorCommand command)
    {
        var previous = HouseholdReligionResolver.CurrentFavor(state, command.HouseholdId);
        HouseholdReligionResolver.ApplyFavorDelta(state, command.HouseholdId, command.Delta);
        var next = previous + command.Delta;

        return new IDomainEvent[]
        {
            new FavorChangedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, previous, next,
                command.Reason, command.CommandId.ToTaggedString()),
        };
    }
}
