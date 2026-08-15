using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>The three-way outcome <see cref="LaborFlightSystem"/> rolls once an active pursuit's
/// countdown reaches zero (<c>gens-labor-slavery-design.md</c> §7).</summary>
public enum PursuitOutcome
{
    Recaptured,
    PermanentlyLost,
    CapturedWithHarm,
}

/// <summary>The monthly flight/pursuit/Testamento tick (Phase 6 item 6; <c>gens-labor-slavery-design.md</c>
/// §7-§8). Three passes over living Characters each tick: resolves any pending Testamento manumission
/// whose grantor has since died, rolls a flight-opportunity check for every enslaved Character with a
/// Duty and no standing <see cref="FledRecord"/>, and counts down/resolves every active <see
/// cref="PursuitRecord"/>.</summary>
public sealed class LaborFlightSystem : IMonthlySystem<WorldState>
{
    /// <summary>Invented baseline (§12 leaves the pursuit window/duration untuned): how many months a
    /// dispatched pursuit runs before its countdown resolves an outcome.</summary>
    public const int PursuitDurationMonths = 3;

    /// <summary>Invented baseline: the flat three-way outcome split once a pursuit's countdown ends —
    /// §7's weighting by a pursuer's Martial/Intrigue is deferred (no Companion/bounty-hunter system
    /// exists yet to weight against).</summary>
    private const uint RecaptureThreshold = 400_000; // 40% recapture.
    private const uint PermanentLossThreshold = 800_000; // + 40% permanently lost; remaining 20% harmed.

    private const int RecaptureLoyaltyPenalty = 15;

    private readonly string _flightOpportunityStreamName;
    private readonly string _pursuitOutcomeStreamName;

    /// <param name="flightOpportunityStreamName">The named stream this system draws its monthly
    /// flight-opportunity roll from.</param>
    /// <param name="pursuitOutcomeStreamName">The named stream this system draws its pursuit-outcome
    /// roll from — kept distinct from <paramref name="flightOpportunityStreamName"/> so a change to
    /// one draw sequence never perturbs the other (rule 8).</param>
    public LaborFlightSystem(string flightOpportunityStreamName, string pursuitOutcomeStreamName)
    {
        if (string.IsNullOrWhiteSpace(flightOpportunityStreamName))
            throw new ArgumentException("A flight-opportunity random stream name is required.", nameof(flightOpportunityStreamName));
        if (string.IsNullOrWhiteSpace(pursuitOutcomeStreamName))
            throw new ArgumentException("A pursuit-outcome random stream name is required.", nameof(pursuitOutcomeStreamName));

        _flightOpportunityStreamName = flightOpportunityStreamName;
        _pursuitOutcomeStreamName = pursuitOutcomeStreamName;
    }

    public string Id => "characters.laborFlight";
    public TickPhase Phase => TickPhase.Hazards;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "characters", "householdRegimenDefaults" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "characters", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();
        var characters = state.Characters.InAscendingOrder().ToArray();

        foreach (var entry in characters)
        {
            if (!state.Characters.TryGet(entry.Key, out var character))
                continue;
            if (!character.IsAlive)
                continue;

            if (character.ManumissionPlan is { } plan &&
                state.Characters.TryGet(plan.GrantorId, out var grantor) && !grantor.IsAlive)
            {
                character = character with
                {
                    LegalStatus = LegalStatus.Freedman,
                    Nomen = grantor.Nomen,
                    Regimen = null,
                    Flight = null,
                    Pursuit = null,
                    ManumissionPlan = null,
                };
                state.Characters.Remove(entry.Key);
                state.Characters.Add(entry.Key, character);
                events.Add(new TestamentoManumissionResolvedEvent(state.EventIds.Issue(), context.Date, character.Id, plan.GrantorId));
            }

            if (character.LegalStatus == LegalStatus.Enslaved && character.Flight is null && character.Duty is { } duty)
            {
                var regimen = RegimenResolver.GetEffective(state, character);
                var riskScore = FlightRiskCalculator.ComputeRiskScore(character.Condition.Loyalty, regimen);
                var threshold = FlightRiskCalculator.MonthlyProbabilityThreshold(riskScore);
                var roll = context.RandomStreams.NextUInt(_flightOpportunityStreamName, FlightRiskCalculator.RollPrecision);

                if (roll < threshold)
                {
                    var formerHousehold = duty.HouseholdId;
                    character = character with
                    {
                        Flight = new FledRecord(context.Date, formerHousehold, character.Location),
                        Duty = null,
                        Household = null,
                    };
                    state.Characters.Remove(entry.Key);
                    state.Characters.Add(entry.Key, character);
                    events.Add(new CharacterFledEvent(state.EventIds.Issue(), context.Date, character.Id, formerHousehold));
                }
            }

            if (character.Pursuit is { } pursuit)
            {
                var monthsRemaining = pursuit.MonthsRemaining - 1;
                if (monthsRemaining > 0)
                {
                    state.Characters.Remove(entry.Key);
                    state.Characters.Add(entry.Key, character with { Pursuit = pursuit with { MonthsRemaining = monthsRemaining } });
                }
                else
                {
                    var roll = context.RandomStreams.NextUInt(_pursuitOutcomeStreamName, FlightRiskCalculator.RollPrecision);
                    Character resolved;
                    PursuitOutcome outcome;

                    if (roll < RecaptureThreshold)
                    {
                        outcome = PursuitOutcome.Recaptured;
                        resolved = character with
                        {
                            Household = character.Flight!.Value.FormerHousehold,
                            Flight = null,
                            Pursuit = null,
                            Condition = new Condition(
                                character.Condition.Health, character.Condition.Fatigue,
                                Math.Max(0, character.Condition.Loyalty - RecaptureLoyaltyPenalty),
                                character.Condition.Ambition, character.Condition.Fertility),
                        };
                    }
                    else if (roll < PermanentLossThreshold)
                    {
                        outcome = PursuitOutcome.PermanentlyLost;
                        resolved = character with { Pursuit = null };
                    }
                    else
                    {
                        outcome = PursuitOutcome.CapturedWithHarm;
                        resolved = character with
                        {
                            Pursuit = null,
                            DeathRecord = new DeathRecord(context.Date, DeathCause.Violence, character.AgeInYears(context.Date)),
                        };
                    }

                    state.Characters.Remove(entry.Key);
                    state.Characters.Add(entry.Key, resolved);
                    events.Add(new PursuitResolvedEvent(state.EventIds.Issue(), context.Date, character.Id, outcome));
                }
            }
        }

        return events;
    }
}

/// <summary>Emitted whenever <see cref="LaborFlightSystem"/> rolls a successful flight opportunity.</summary>
public sealed record CharacterFledEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    RuntimeId<Household> FormerHouseholdId) : IDomainEvent
{
    public string Type => "characters.fled";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString(), FormerHouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}

/// <summary>Emitted whenever <see cref="LaborFlightSystem"/> resolves an active pursuit's countdown.</summary>
public sealed record PursuitResolvedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    PursuitOutcome Outcome) : IDomainEvent
{
    public string Type => "characters.pursuitResolved";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}

/// <summary>Emitted whenever <see cref="LaborFlightSystem"/> resolves a pending Testamento manumission
/// after its grantor's death.</summary>
public sealed record TestamentoManumissionResolvedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    RuntimeId<Character> GrantorId) : IDomainEvent
{
    public string Type => "characters.testamentoManumissionResolved";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString(), GrantorId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}
