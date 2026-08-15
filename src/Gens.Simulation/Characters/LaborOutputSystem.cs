using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>The monthly labor tick (Phase 6 item 6): for every living Character holding a Duty, computes
/// this month's output (<see cref="LaborOutputCalculator"/>), accrues Duty-driven Fatigue on top of <see
/// cref="CharacterLifecycleSystem"/>'s flat recovery, and — for the enslaved — applies the resolved
/// Regimen's Health/Loyalty trend. Deliberately runs after <see cref="TickPhase.Lifecycle"/> (phase
/// ordering alone guarantees this; no same-phase prerequisite is declared) so this month's recovery
/// applies before this month's strain.</summary>
public sealed class LaborOutputSystem : IMonthlySystem<WorldState>
{
    public string Id => "characters.laborOutput";
    public TickPhase Phase => TickPhase.Production;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "characters", "householdRegimenDefaults" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "characters", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // Materialize first: the loop body mutates state.Characters (Remove+Add) mid-iteration,
        // matching CharacterLifecycleSystem's identical precaution.
        var characters = state.Characters.InAscendingOrder().ToArray();

        foreach (var entry in characters)
        {
            if (!state.Characters.TryGet(entry.Key, out var character))
                continue;
            if (!character.IsAlive || character.Duty is not { } duty)
                continue;

            var regimen = RegimenResolver.GetEffective(state, character);
            var effectiveSkill = DutySlotCatalog.RelevantSkillValue(character.GetEffectiveSkills(), duty.Slot);
            var output = LaborOutputCalculator.ComputeOutput(
                effectiveSkill, character.Condition.Health, character.Condition.Fatigue, regimen);

            var newFatigue = LaborOutputCalculator.ApplyMonthlyFatigueAccrual(character.Condition.Fatigue);

            // The Regimen's Health/Loyalty trend is a treatment effect — only meaningful for someone
            // still enslaved under it; a freedman continuing the same Duty (§8's "Labor continuity")
            // is not "under Regimen" in that sense.
            var isUnderRegimen = character.LegalStatus == LegalStatus.Enslaved;
            var newHealth = isUnderRegimen
                ? LaborOutputCalculator.ApplyMonthlyHealthTrend(character.Condition.Health, regimen)
                : character.Condition.Health;
            var newLoyalty = isUnderRegimen
                ? LaborOutputCalculator.ApplyMonthlyLoyaltyTrend(character.Condition.Loyalty, regimen)
                : character.Condition.Loyalty;

            if (newFatigue != character.Condition.Fatigue || newHealth != character.Condition.Health ||
                newLoyalty != character.Condition.Loyalty)
            {
                var updated = character with
                {
                    Condition = new Condition(
                        newHealth, newFatigue, newLoyalty, character.Condition.Ambition, character.Condition.Fertility),
                };
                state.Characters.Remove(entry.Key);
                state.Characters.Add(entry.Key, updated);
            }

            events.Add(new LaborOutputComputedEvent(
                state.EventIds.Issue(), context.Date, character.Id, duty.HouseholdId, duty.Slot, output));
        }

        return events;
    }
}

/// <summary>Emitted every tick for every Character on Duty (Phase 6 item 8's "emit labor events even
/// before the economy consumes them") — ledger-ready for the later production-network item to
/// aggregate once buildings/goods are wired up.</summary>
public sealed record LaborOutputComputedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    RuntimeId<Household> HouseholdId,
    DutySlot Slot,
    int Output) : IDomainEvent
{
    public string Type => "characters.laborOutputComputed";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString(), HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}
