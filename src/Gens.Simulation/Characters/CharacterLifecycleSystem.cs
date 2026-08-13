using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>
/// The monthly lifecycle tick (Phase 5 item 3): stage-transition detection, Fatigue recovery, Elderly
/// Health decline, and the mortality roll, for every living Character whose <see
/// cref="Character.BirthDate"/> is strictly before the current tick — a Character born this same tick
/// has no prior stage to transition from and no first-month mortality roll, so this system only
/// starts acting on them the tick after they exist.
/// </summary>
public sealed class CharacterLifecycleSystem : IMonthlySystem<WorldState>
{
    /// <summary>Points of Fatigue recovered per month. No labor/duty system feeds Fatigue upward yet
    /// (that is later work), so this establishes the recovery mechanism other systems will later add
    /// strain on top of. Nothing in the design corpus specifies a numeric monthly rate — this is this
    /// implementation's own invented baseline.</summary>
    private const int MonthlyFatigueRecovery = 10;

    /// <summary>Health points lost per year of age while Elderly, applied only on the Character's
    /// birthday month. Nothing in the design corpus specifies a numeric decline rate
    /// (<c>gens-familia-design.md</c> §3 only says Health "gradually" declines while Elderly) — this
    /// is this implementation's own invented annual rate. Aging alone never drives Health below 1:
    /// only the separate mortality roll below can end a life.</summary>
    private const int AnnualElderlyHealthDecline = 2;

    /// <summary>The mortality roll's precision: a monthly probability is compared against a draw in
    /// [0, 1,000,000) rather than [0, 100), so a probability as small as one in a million is still
    /// representable.</summary>
    private const uint MortalityRollPrecision = 1_000_000;

    private readonly string _mortalityStreamName;

    /// <param name="mortalityStreamName">The named <see cref="Random.RandomStreamSet"/> stream this
    /// system draws its mortality roll from — supplied by the caller (see <see
    /// cref="Campaign.CampaignBootstrapper.CharacterMortalityStreamName"/>) rather than hardcoded, so a
    /// stream-naming change never needs an edit here.</param>
    public CharacterLifecycleSystem(string mortalityStreamName)
    {
        if (string.IsNullOrWhiteSpace(mortalityStreamName))
            throw new ArgumentException("A mortality random stream name is required.", nameof(mortalityStreamName));
        _mortalityStreamName = mortalityStreamName;
    }

    public string Id => "characters.lifecycle";
    public TickPhase Phase => TickPhase.Lifecycle;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "characters" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "characters", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // Materialize first: the loop body mutates state.Characters (Remove+Add) mid-iteration.
        var characters = state.Characters.InAscendingOrder().ToArray();

        foreach (var entry in characters)
        {
            // Re-fetch: an earlier iteration in this same tick may already have closed this
            // Character's marriage (they were an already-processed, lower-ID spouse who died).
            if (!state.Characters.TryGet(entry.Key, out var character))
                continue;
            if (!character.IsAlive || character.BirthDate.TotalMonths >= context.Date.TotalMonths)
                continue;

            var previousStage = character.GetLifecycleStage(new GameDate(context.Date.TotalMonths - 1));
            var currentStage = character.GetLifecycleStage(context.Date);

            var newFatigue = Math.Max(0, character.Condition.Fatigue - MonthlyFatigueRecovery);
            var newHealth = character.Condition.Health;
            var isBirthdayMonth = (context.Date.TotalMonths - character.BirthDate.TotalMonths) % 12 == 0;
            if (currentStage == LifecycleStage.Elderly && isBirthdayMonth)
                newHealth = Math.Max(1, newHealth - AnnualElderlyHealthDecline);

            var updatedCharacter = newFatigue != character.Condition.Fatigue || newHealth != character.Condition.Health
                ? character with
                {
                    Condition = new Condition(
                        newHealth, newFatigue, character.Condition.Loyalty, character.Condition.Ambition, character.Condition.Fertility),
                }
                : character;

            if (previousStage != currentStage)
                events.Add(new CharacterLifecycleStageChangedEvent(state.EventIds.Issue(), context.Date, character.Id, previousStage, currentStage));

            var ageInYears = character.AgeInYears(context.Date);
            var monthlyProbability = MortalityCalculator.MonthlyProbability(currentStage, ageInYears, newHealth);
            var threshold = (uint)Math.Clamp(monthlyProbability * MortalityRollPrecision, 0, MortalityRollPrecision);
            var roll = context.RandomStreams.NextUInt(_mortalityStreamName, MortalityRollPrecision);

            if (roll < threshold)
            {
                var cause = currentStage == LifecycleStage.Elderly ? DeathCause.OldAge
                    : currentStage == LifecycleStage.Infant ? DeathCause.Disease
                    : DeathCause.Unspecified;
                var deathRecord = new DeathRecord(context.Date, cause, ageInYears);
                updatedCharacter = updatedCharacter with { DeathRecord = deathRecord };

                var spouseId = updatedCharacter.CurrentSpouseId;
                if (spouseId is { } spouse)
                {
                    updatedCharacter = CloseOpenMarriage(updatedCharacter, spouse, context.Date);
                    if (state.Characters.TryGet(spouse, out var spouseCharacter) && spouseCharacter.IsAlive)
                    {
                        var updatedSpouse = CloseOpenMarriage(spouseCharacter, character.Id, context.Date);
                        state.Characters.Remove(spouse);
                        state.Characters.Add(spouse, updatedSpouse);
                    }

                    events.Add(new MarriageEndedEvent(
                        state.EventIds.Issue(), context.Date, character.Id, spouse, MarriageEndReason.Death, CausationId: null));
                }

                events.Add(new CharacterDiedEvent(state.EventIds.Issue(), context.Date, character.Id, spouseId, deathRecord));
            }

            if (!ReferenceEquals(updatedCharacter, character))
            {
                state.Characters.Remove(entry.Key);
                state.Characters.Add(entry.Key, updatedCharacter);
            }
        }

        return events;
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

/// <summary>Emitted whenever <see cref="CharacterLifecycleSystem"/> detects a Character crossing a
/// <see cref="LifecycleStage"/> boundary this tick.</summary>
public sealed record CharacterLifecycleStageChangedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    LifecycleStage PreviousStage,
    LifecycleStage CurrentStage) : IDomainEvent
{
    public string Type => "characters.lifecycleStageChanged";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}

/// <summary>Emitted whenever a Character dies (<c>gens-familia-design.md</c> §3). <see
/// cref="SpouseId"/> and <see cref="SubjectIds"/> include the deceased's spouse only when a marriage
/// was closed as part of this death — <see cref="MarriageEndedEvent"/> records that closure in its
/// own right alongside this event.</summary>
public sealed record CharacterDiedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    RuntimeId<Character>? SpouseId,
    DeathRecord DeathRecord) : IDomainEvent
{
    public string Type => "characters.died";
    public int SchemaVersion => 1;

    public IReadOnlyList<string> SubjectIds => SpouseId is { } spouse
        ? new[] { CharacterId.ToTaggedString(), spouse.ToTaggedString() }
        : new[] { CharacterId.ToTaggedString() };

    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}

/// <summary>Emitted whenever a <see cref="MarriageRecord"/> closes, whether by <see
/// cref="CharacterLifecycleSystem"/> (a spouse's death — <see cref="CausationId"/> is <c>null</c>,
/// system-originated) or by an accepted <see cref="EndMarriageCommand"/> (<see cref="CausationId"/>
/// is that command's ID).</summary>
public sealed record MarriageEndedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    RuntimeId<Character> SpouseId,
    MarriageEndReason Reason,
    string? CausationId) : IDomainEvent
{
    public string Type => "characters.marriageEnded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString(), SpouseId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}
