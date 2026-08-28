using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Funerary;

/// <summary>Emitted when a death opens a new <see cref="FuneralRecord"/> (§2.1's automatic, cost-free
/// <c>collocatio</c> — laying the deceased out is itself flavor-only; this event is what actually
/// marks the funeral sequence as begun).</summary>
public sealed record FuneralOpenedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<FuneralRecord> FuneralId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> DeceasedCharacterId) : IDomainEvent
{
    public string Type => "funerary.funeralOpened";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), DeceasedCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}

/// <summary>Emitted when a death starts (or extends) a household's <see cref="MourningPeriod"/> (§4.1).</summary>
public sealed record MourningPeriodStartedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> TriggeringDeathCharacterId,
    GameDate EndDate,
    bool Extended) : IDomainEvent
{
    public string Type => "funerary.mourningPeriodStarted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), TriggeringDeathCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}

/// <summary>
/// The monthly system that routes every household death through §2's funeral sequence before <see
/// cref="Succession.SuccessionHandoffSystem"/>'s inheritance resolution begins (Phase 11 item 4; §2's
/// "every death... now routes through the same real sequence before Succession &amp; Dynasty's
/// inheritance resolution begins"). Named <c>funerary.funeralOpening</c> deliberately: it declares no
/// <see cref="Prerequisites"/> against <see cref="Succession.SuccessionHandoffSystem"/> (the two never
/// actually read each other's writes), but its ID sorts ordinally before <c>succession.handoff</c>
/// within the same <see cref="TickPhase.RelationshipsActors"/> phase, so <see
/// cref="MonthlySimulation{TState}"/>'s deterministic same-phase tiebreak (ADR 0004/0005) always runs
/// this system first — matching the design doc's own sequencing intent without touching <see
/// cref="Succession.SuccessionHandoffSystem"/>'s own declared <see cref="Prerequisites"/>.
///
/// For every Character who died this tick (<see cref="Character.IsAlive"/> now false, read directly
/// from raw state exactly like <see cref="Succession.SuccessionHandoffSystem"/> already does — both
/// run in the same phase strictly after <see cref="Characters.CharacterLifecycleSystem"/>'s own earlier
/// <see cref="TickPhase.Lifecycle"/> phase, so this month's deaths are already visible) with a tracked
/// <see cref="Character.Household"/> and no <see cref="FuneralRecord"/> opened for them yet: opens a
/// new <see cref="FuneralStatus.Pending"/> <see cref="FuneralRecord"/>, then starts (or, if the
/// household is already mourning an earlier death, extends) that household's <see
/// cref="MourningPeriod"/>. A household member's death always opens a funeral, independent of whether
/// that Character was ever a tracked <see cref="Succession.HouseholdHeadship"/> head — §2's "every
/// death" is broader than Succession's own "who inherits" question.
/// </summary>
public sealed class FuneralOpeningSystem : IMonthlySystem<WorldState>
{
    public string Id => "funerary.funeralOpening";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "characters", "funeralRecords", "mourningPeriods" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "funeralRecords", "funeralRecordIds", "mourningPeriods", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();
        var alreadyOpenedFor = ExistingFuneralDeceasedIds(state);

        foreach (var entry in state.Characters.InAscendingOrder())
        {
            var character = entry.Value;
            if (character.IsAlive || character.Household is not { } householdId)
                continue;
            if (alreadyOpenedFor.Contains(character.Id))
                continue;

            var funeralId = state.FuneralRecordIds.Issue();
            state.FuneralRecords.Add(
                funeralId,
                new FuneralRecord(
                    funeralId, householdId, character.Id, context.Date, FuneralStatus.Pending,
                    Tier: null, BurialMethod: null, InterredAt: null, HeldDate: null, Cost: null, MemoriaGained: null));
            events.Add(new FuneralOpenedEvent(state.EventIds.Issue(), context.Date, funeralId, householdId, character.Id));

            events.Add(StartOrExtendMourning(state, householdId, character.Id, context.Date));
        }

        return events;
    }

    private static MourningPeriodStartedEvent StartOrExtendMourning(
        WorldState state, RuntimeId<Household> householdId, RuntimeId<Character> deceasedId, GameDate date)
    {
        var newEndDate = new GameDate(date.TotalMonths + FuneraryCatalog.MourningDurationMonths);

        if (state.MourningPeriods.TryGet(householdId, out var existing) && existing!.IsActiveOn(date))
        {
            var extendedEnd = existing.EndDate.TotalMonths >= newEndDate.TotalMonths ? existing.EndDate : newEndDate;
            state.MourningPeriods.Remove(householdId);
            state.MourningPeriods.Add(householdId, existing with { EndDate = extendedEnd });
            return new MourningPeriodStartedEvent(state.EventIds.Issue(), date, householdId, deceasedId, extendedEnd, Extended: true);
        }

        if (existing is not null)
            state.MourningPeriods.Remove(householdId);
        state.MourningPeriods.Add(householdId, new MourningPeriod(householdId, deceasedId, date, newEndDate));
        return new MourningPeriodStartedEvent(state.EventIds.Issue(), date, householdId, deceasedId, newEndDate, Extended: false);
    }

    private static HashSet<RuntimeId<Character>> ExistingFuneralDeceasedIds(WorldState state)
    {
        var ids = new HashSet<RuntimeId<Character>>();
        foreach (var entry in state.FuneralRecords.InAscendingOrder())
            ids.Add(entry.Value.DeceasedCharacterId);
        return ids;
    }
}
