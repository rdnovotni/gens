using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Health;

/// <summary>The monthly tick for every <see cref="CharacterHealthConditionStatus.Active"/> <see
/// cref="CharacterHealthCondition"/> (Phase 14 item 1): assigns this month's Physician treatment within
/// each afflicted Household's bounded <see cref="CareCapacityCalculator"/> capacity, applies Health
/// drain, rolls recovery/fatality via <see cref="HealthConditionProgressionCalculator"/>, and — on a
/// fatal roll — kills the Character with <see cref="DeathCause.Disease"/> attributed to the specific
/// condition (§10's "the unrestricted death mechanism"), closing that Character's life exactly the way
/// <see cref="CharacterLifecycleSystem"/> closes an old-age one (same marriage-closing convention).
/// Runs in <see cref="TickPhase.Hazards"/> — the phase this same roadmap wave (Phase 14, "Add health,
/// disease, disasters, and mobile populations") exists to fill. A condition whose Character has no
/// Household, or whose Household has no living member holding the Physician duty slot, is simply never
/// treated this month: <see cref="CareCapacityCalculator.MonthlyCareCapacity"/> already returns zero for
/// that case, so no special-casing is needed in the loop below.</summary>
public sealed class CharacterHealthConditionSystem : IMonthlySystem<WorldState>
{
    private const uint RollPrecision = 1_000_000;

    private readonly string _progressionStreamName;

    /// <param name="progressionStreamName">The named <see cref="Random.RandomStreamSet"/> stream this
    /// system draws its monthly recovery/fatality rolls from — supplied by the caller (see <see
    /// cref="Campaign.CampaignBootstrapper.HealthConditionProgressionStreamName"/>) rather than
    /// hardcoded, matching <see cref="CharacterLifecycleSystem"/>'s identical constructor shape.</param>
    public CharacterHealthConditionSystem(string progressionStreamName)
    {
        if (string.IsNullOrWhiteSpace(progressionStreamName))
            throw new ArgumentException(
                "A health condition progression random stream name is required.", nameof(progressionStreamName));
        _progressionStreamName = progressionStreamName;
    }

    public string Id => "health.characterConditionProgression";
    public TickPhase Phase => TickPhase.Hazards;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "characterHealthConditions", "characters" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "characterHealthConditions", "characters", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // Materialize first: the loop body mutates state.CharacterHealthConditions (Remove+Add) and
        // state.Characters mid-iteration, matching CharacterLifecycleSystem's identical precaution.
        var activeConditions = state.CharacterHealthConditions.InAscendingOrder()
            .Where(entry => entry.Value.Status == CharacterHealthConditionStatus.Active)
            .Select(entry => entry.Value)
            .ToArray();

        if (activeConditions.Length == 0)
            return events;

        var treatedIds = DetermineTreatedThisMonth(state, activeConditions);

        foreach (var condition in activeConditions)
        {
            // Re-fetch: an earlier iteration this same tick may already have resolved this entry (its
            // owning Character having died from a different, earlier-processed condition of theirs).
            if (!state.CharacterHealthConditions.TryGet(condition.Id, out var current) ||
                current.Status != CharacterHealthConditionStatus.Active)
                continue;
            if (!state.Characters.TryGet(current.CharacterId, out var character) || !character.IsAlive)
                continue;

            var treated = treatedIds.Contains(current.Id);

            var drain = HealthConditionProgressionCalculator.MonthlyHealthDrain(current.Category, current.Severity, treated);
            var newHealth = Math.Max(1, character.Condition.Health - drain);
            if (newHealth != character.Condition.Health)
            {
                character = character with
                {
                    Condition = new Condition(
                        newHealth, character.Condition.Fatigue, character.Condition.Loyalty,
                        character.Condition.Ambition, character.Condition.Fertility),
                };
                state.Characters.Remove(character.Id);
                state.Characters.Add(character.Id, character);
            }

            var recoveryProbability = HealthConditionProgressionCalculator.MonthlyRecoveryProbability(
                current.Category, current.HasCure, treated);
            var recoveryThreshold = (uint)Math.Clamp(recoveryProbability * RollPrecision, 0, RollPrecision);
            var recoveryRoll = context.RandomStreams.NextUInt(_progressionStreamName, RollPrecision);

            if (recoveryRoll < recoveryThreshold)
            {
                var grantedImmunity = current.Category == HealthConditionCategory.Acute;
                state.CharacterHealthConditions.Remove(current.Id);
                state.CharacterHealthConditions.Add(current.Id, current with
                {
                    Status = CharacterHealthConditionStatus.Recovered,
                    TreatedByPhysician = treated,
                    GrantedImmunity = grantedImmunity,
                    ResolvedDate = context.Date,
                });
                events.Add(new CharacterHealthConditionRecoveredEvent(
                    state.EventIds.Issue(), context.Date, current.CharacterId, current.ConditionId, grantedImmunity));
                continue;
            }

            var fatalityProbability = HealthConditionProgressionCalculator.MonthlyFatalityProbability(
                current.Category, current.Severity, newHealth, treated);
            var fatalityThreshold = (uint)Math.Clamp(fatalityProbability * RollPrecision, 0, RollPrecision);
            var fatalityRoll = context.RandomStreams.NextUInt(_progressionStreamName, RollPrecision);

            if (fatalityRoll < fatalityThreshold)
            {
                state.CharacterHealthConditions.Remove(current.Id);
                state.CharacterHealthConditions.Add(current.Id, current with
                {
                    Status = CharacterHealthConditionStatus.Fatal,
                    TreatedByPhysician = treated,
                    ResolvedDate = context.Date,
                });

                var ageInYears = character.AgeInYears(context.Date);
                var deathRecord = new DeathRecord(context.Date, DeathCause.Disease, ageInYears, current.ConditionId);
                var deceased = character with { DeathRecord = deathRecord };

                var spouseId = deceased.CurrentSpouseId;
                if (spouseId is { } spouse)
                {
                    deceased = CloseOpenMarriage(deceased, spouse, context.Date);
                    if (state.Characters.TryGet(spouse, out var spouseCharacter) && spouseCharacter.IsAlive)
                    {
                        var updatedSpouse = CloseOpenMarriage(spouseCharacter, character.Id, context.Date);
                        state.Characters.Remove(spouse);
                        state.Characters.Add(spouse, updatedSpouse);
                    }

                    events.Add(new MarriageEndedEvent(
                        state.EventIds.Issue(), context.Date, character.Id, spouse, MarriageEndReason.Death, CausationId: null));
                }

                state.Characters.Remove(character.Id);
                state.Characters.Add(character.Id, deceased);

                events.Add(new CharacterDiedEvent(state.EventIds.Issue(), context.Date, character.Id, spouseId, deathRecord));
                continue;
            }

            var drift = HealthConditionProgressionCalculator.MonthlySeverityDrift(current.Category, treated);
            var newSeverity = Math.Clamp(current.Severity + drift, 1, 100);
            state.CharacterHealthConditions.Remove(current.Id);
            state.CharacterHealthConditions.Add(current.Id, current with { Severity = newSeverity, TreatedByPhysician = treated });
        }

        return events;
    }

    /// <summary>Groups this month's Active cases by their Character's Household, resolves each
    /// Household's Physician-driven <see cref="CareCapacityCalculator.MonthlyCareCapacity"/>, and
    /// returns the set of case IDs that capacity actually covers this month — earliest-onset (lowest
    /// <see cref="RuntimeId{T}"/>) cases first, since a household's own capacity may not cover every
    /// simultaneous case.</summary>
    private static HashSet<RuntimeId<CharacterHealthCondition>> DetermineTreatedThisMonth(
        WorldState state, IReadOnlyList<CharacterHealthCondition> activeConditions)
    {
        var treated = new HashSet<RuntimeId<CharacterHealthCondition>>();
        var byHousehold = new Dictionary<RuntimeId<Household>, List<CharacterHealthCondition>>();

        foreach (var condition in activeConditions)
        {
            if (!state.Characters.TryGet(condition.CharacterId, out var character))
                continue;
            var householdId = character.Household;
            if (householdId is null)
                continue;

            if (!byHousehold.TryGetValue(householdId.Value, out var list))
            {
                list = new List<CharacterHealthCondition>();
                byHousehold[householdId.Value] = list;
            }

            list.Add(condition);
        }

        foreach (var (householdId, conditions) in byHousehold)
        {
            var physicianSkill = 0;
            foreach (var entry in state.Characters.InAscendingOrder())
            {
                var member = entry.Value;
                if (!member.IsAlive || member.Duty is not { } duty || duty.Slot != DutySlot.Physician || duty.HouseholdId != householdId)
                    continue;
                physicianSkill = Math.Max(physicianSkill, member.GetEffectiveSkills().Medicine);
            }

            var capacity = CareCapacityCalculator.MonthlyCareCapacity(physicianSkill);
            if (capacity <= 0)
                continue;

            foreach (var condition in conditions.OrderBy(c => c.Id.Value).Take(capacity))
                treated.Add(condition.Id);
        }

        return treated;
    }

    private static Character CloseOpenMarriage(Character character, RuntimeId<Character> spouseId, GameDate endDate)
    {
        var history = character.MaritalHistory
            .Select(record => record.SpouseId == spouseId && record.EndDate is null
                ? new MarriageRecord(record.SpouseId, record.StartDate, endDate, MarriageEndReason.Death)
                : record)
            .ToArray();
        return character with { MaritalHistory = history };
    }
}

/// <summary>Emitted whenever a <see cref="CharacterHealthCondition"/> resolves into recovery. <see
/// cref="GrantedImmunity"/> mirrors the case's own field — see <see
/// cref="CharacterHealthCondition.GrantedImmunity"/>'s doc comment for when it's set.</summary>
public sealed record CharacterHealthConditionRecoveredEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    DefinitionId<HealthConditionDefinition> ConditionId,
    bool GrantedImmunity) : IDomainEvent
{
    public string Type => "health.characterHealthConditionRecovered";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}
