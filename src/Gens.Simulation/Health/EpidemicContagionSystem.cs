using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Numerics;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Health;

/// <summary>The monthly §3/§4 Epidemic tick (Phase 14 item 2): closes out any <see
/// cref="EpidemicOutbreak"/> whose active case count has fallen to zero, ignites new outbreaks (<see
/// cref="EpidemicSpreadCalculator.MonthlyIgnitionProbability"/>) at settlements with none currently
/// active, and spreads every still-<see cref="EpidemicOutbreakStatus.Active"/> outbreak one more month
/// via <see cref="AfflictCharacterCommand"/> — the identical "real callers finally exist" payoff <see
/// cref="EndemicIllnessSystem"/> delivers for §2. <b>Contact graph, scoped honestly:</b> §3.1 names
/// "Group Interactions, shared housing, Travel, and shared Household Duty slots" as real contact; this
/// system only spreads Pestilence/Pox/Camp Fever through Household co-membership (<see
/// cref="Character.Household"/>) — <c>Interactions/</c> has no generic Group-Interaction contact-graph
/// concept yet (only Scheme progress), and <c>Travel/</c>'s <see cref="Travel.TravelParty"/> is a
/// point-to-point trip record, not a standing contact graph either, so both are left out rather than
/// bolted on as a fake contact source. Enteric Fever alone ignores this contact graph entirely, per
/// §3.2's own "water-borne, not contact-borne" — see <see cref="EpidemicSpreadCalculator.WaterborneSpreadProbability"/>.</summary>
public sealed class EpidemicContagionSystem : IMonthlySystem<WorldState>
{
    /// <summary>Invented onset severity for a newly-seeded or newly-caught epidemic case — higher than
    /// <see cref="EndemicIllnessSystem"/>'s own onset figure, matching item 1's own progression
    /// calculator framing that an Acute case is meant to hit harder than a Chronic one.</summary>
    private const int OnsetSeverity = 40;

    private const uint RollPrecision = 1_000_000;

    private readonly string _streamName;

    public EpidemicContagionSystem(string streamName)
    {
        if (string.IsNullOrWhiteSpace(streamName))
            throw new ArgumentException("An epidemic contagion random stream name is required.", nameof(streamName));
        _streamName = streamName;
    }

    public string Id => "health.epidemicContagion";
    public TickPhase Phase => TickPhase.Hazards;
    public IReadOnlyCollection<string> Reads { get; } = new[]
    {
        "settlements", "characters", "characterHealthConditions", "epidemicOutbreaks", "settlementSanitationInvestments", "popGroups",
    };
    public IReadOnlyCollection<string> Writes { get; } = new[]
    {
        "characterHealthConditions", "characterHealthConditionIds", "epidemicOutbreaks", "epidemicOutbreakIds", "eventIds", "commandIds",
        "popGroups",
    };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        EmitAntoninePlagueChainMarkers(state, context, events);
        CloseOutbreaksWithNoRemainingCases(state, context, events);
        ApplySettlementQuarantineContentmentCost(state, context);

        foreach (var settlementEntry in state.Settlements.InAscendingOrder())
        {
            var settlementId = settlementEntry.Key;
            var sanitationMultiplier = SanitationInvestmentCalculator.ExposureMultiplier(
                SanitationQueries.EffectiveTier(state, settlementId));
            var residents = state.Characters.InAscendingOrder()
                .Where(entry => entry.Value.IsAlive && entry.Value.Location == settlementId)
                .Select(entry => entry.Value)
                .ToArray();
            if (residents.Length == 0)
                continue;

            foreach (var diseaseProfile in DiseaseCatalog.EpidemicProfiles)
            {
                var outbreak = FindActiveOutbreak(state, settlementId, diseaseProfile.ConditionId);
                if (outbreak is null)
                {
                    TryIgnite(state, context, settlementId, diseaseProfile.ConditionId, sanitationMultiplier, residents, events);
                    continue;
                }

                var settlementMultiplier = QuarantineEffectCalculator.SettlementSpreadMultiplier(
                    outbreak.SettlementQuarantineActive, outbreak.ImperialScale);

                if (diseaseProfile.Vector == EpidemicVector.PersonToPerson)
                    SpreadPersonToPerson(state, context, diseaseProfile, residents, sanitationMultiplier, settlementMultiplier, events);
                else
                    SpreadWaterborne(state, context, diseaseProfile, residents, sanitationMultiplier, settlementMultiplier, events);
            }
        }

        return events;
    }

    private static void CloseOutbreaksWithNoRemainingCases(WorldState state, MonthlyTickContext context, List<IDomainEvent> events)
    {
        var activeOutbreaks = state.EpidemicOutbreaks.InAscendingOrder()
            .Where(entry => entry.Value.Status == EpidemicOutbreakStatus.Active)
            .Select(entry => entry.Value)
            .ToArray();

        foreach (var outbreak in activeOutbreaks)
        {
            var stillActive = state.CharacterHealthConditions.InAscendingOrder().Any(entry =>
                entry.Value.Status == CharacterHealthConditionStatus.Active &&
                entry.Value.ConditionId == outbreak.ConditionId &&
                state.Characters.TryGet(entry.Value.CharacterId, out var character) &&
                character.Location == outbreak.SettlementId);

            if (stillActive)
                continue;

            state.EpidemicOutbreaks.Remove(outbreak.Id);
            state.EpidemicOutbreaks.Add(outbreak.Id, outbreak with { Status = EpidemicOutbreakStatus.Ended, ResolvedDate = context.Date });
            events.Add(new EpidemicOutbreakEndedEvent(state.EventIds.Issue(), context.Date, outbreak.SettlementId, outbreak.Id, outbreak.ConditionId));
        }
    }

    /// <summary>Item 5's real Antonine Plague Event Chain markers (<see cref="AntoninePlagueEra"/>'s own
    /// doc comment): a single <see cref="AntoninePlagueOnsetEvent"/> the exact month <see
    /// cref="AntoninePlagueEra.Start"/> arrives, and a single <see cref="AntoninePlagueWaningEvent"/> the
    /// month after <see cref="AntoninePlagueEra.End"/> — each fires exactly once per campaign since
    /// <see cref="WorldState.Date"/> only ever equals either boundary on one real tick.</summary>
    private static void EmitAntoninePlagueChainMarkers(WorldState state, MonthlyTickContext context, List<IDomainEvent> events)
    {
        if (context.Date == AntoninePlagueEra.Start)
            events.Add(new AntoninePlagueOnsetEvent(state.EventIds.Issue(), context.Date));
        else if (context.Date == AntoninePlagueEra.End.NextMonth())
            events.Add(new AntoninePlagueWaningEvent(state.EventIds.Issue(), context.Date));
    }

    /// <summary>§4.2's real Contentment cost, closing <see cref="SetSettlementQuarantineCommand"/>'s own
    /// disclosed gap: every PopGroup at a settlement currently under an active <see
    /// cref="HealthQueries.IsSettlementUnderQuarantine"/> Settlement-Wide Quarantine takes <see
    /// cref="QuarantineEffectCalculator.ContentmentImpact"/>'s felt shock this month, the same
    /// "same-month shock, not a persisted debuff" shape <c>Hazards.NaturalDisasterSystem</c>'s own
    /// Contentment hit already established — <see cref="Characters.ContentmentSystem"/> recomputes from
    /// its own formula next month regardless, so a settlement that lifts Quarantine simply stops taking
    /// the hit rather than needing any decay/recovery mechanism here.</summary>
    private static void ApplySettlementQuarantineContentmentCost(WorldState state, MonthlyTickContext context)
    {
        foreach (var settlementEntry in state.Settlements.InAscendingOrder())
        {
            var settlementId = settlementEntry.Key;
            if (!HealthQueries.IsSettlementUnderQuarantine(state, settlementId))
                continue;

            foreach (var entry in state.PopGroups.InAscendingOrder().ToArray())
            {
                if (entry.Key.SettlementId != settlementId)
                    continue;

                var group = entry.Value;
                var newContentment = group.Contentment + QuarantineEffectCalculator.ContentmentImpact;
                if (newContentment < Fixed64.Zero)
                    newContentment = Fixed64.Zero;

                state.PopGroups.Remove(entry.Key);
                state.PopGroups.Add(entry.Key, group with { Contentment = newContentment });
            }
        }
    }

    private static EpidemicOutbreak? FindActiveOutbreak(
        WorldState state, RuntimeId<Settlement> settlementId, DefinitionId<HealthConditionDefinition> conditionId)
    {
        foreach (var entry in state.EpidemicOutbreaks.InAscendingOrder())
        {
            if (entry.Value.Status == EpidemicOutbreakStatus.Active &&
                entry.Value.SettlementId == settlementId && entry.Value.ConditionId == conditionId)
                return entry.Value;
        }

        return null;
    }

    private void TryIgnite(
        WorldState state, MonthlyTickContext context, RuntimeId<Settlement> settlementId,
        DefinitionId<HealthConditionDefinition> conditionId, double sanitationMultiplier,
        Character[] residents, List<IDomainEvent> events)
    {
        var antoninePlagueElevated = conditionId == DiseaseCatalog.Pestilence && AntoninePlagueEra.IsActive(context.Date);
        var ignitionProbability = antoninePlagueElevated
            ? EpidemicSpreadCalculator.AntoninePlagueIgnitionProbability(sanitationMultiplier)
            : EpidemicSpreadCalculator.MonthlyIgnitionProbability(sanitationMultiplier);
        var threshold = (uint)Math.Clamp(ignitionProbability * RollPrecision, 0, RollPrecision);
        var roll = context.RandomStreams.NextUInt(_streamName, RollPrecision);
        if (roll >= threshold)
            return;

        // Seed exactly one Character among this settlement's residents, deterministically chosen by
        // this same roll's remainder — a second draw is not needed, matching how a single RNG draw
        // already both decided ignition and (via its own value) which resident carries the spark.
        var seedIndex = (int)(roll % (uint)residents.Length);
        var seedCharacter = residents[seedIndex];
        if (HealthQueries.HasActiveCondition(state, seedCharacter.Id, conditionId) ||
            HealthQueries.IsImmune(state, seedCharacter.Id, conditionId))
            return;

        var outbreakId = state.EpidemicOutbreakIds.Issue();
        state.EpidemicOutbreaks.Add(
            outbreakId, EpidemicOutbreak.Create(outbreakId, settlementId, conditionId, context.Date, antoninePlagueElevated));
        events.Add(new EpidemicOutbreakIgnitedEvent(state.EventIds.Issue(), context.Date, settlementId, outbreakId, conditionId));

        var command = new AfflictCharacterCommand(
            state.CommandIds.Issue(), "system", context.Date, CausationId: null, seedCharacter.Id,
            conditionId, HealthConditionCategory.Acute, HasCure: DiseaseCatalog.EntericFever == conditionId, OnsetSeverity);
        var result = AfflictCharacterCommands.Pipeline.Execute(state, command);
        if (result.Accepted)
            events.AddRange(result.Events);
    }

    private void SpreadPersonToPerson(
        WorldState state, MonthlyTickContext context, EpidemicDiseaseProfile diseaseProfile,
        Character[] residents, double sanitationMultiplier, double settlementMultiplier, List<IDomainEvent> events)
    {
        var infectedByHousehold = new Dictionary<RuntimeId<Household>, List<CharacterHealthCondition>>();
        foreach (var character in residents)
        {
            if (character.Household is not { } householdId)
                continue;
            foreach (var condition in HealthQueries.ActiveConditionsFor(state, character.Id))
            {
                if (condition.ConditionId != diseaseProfile.ConditionId)
                    continue;
                if (!infectedByHousehold.TryGetValue(householdId, out var list))
                {
                    list = new List<CharacterHealthCondition>();
                    infectedByHousehold[householdId] = list;
                }

                list.Add(condition);
            }
        }

        if (infectedByHousehold.Count == 0)
            return;

        foreach (var character in residents)
        {
            if (character.Household is not { } householdId || !infectedByHousehold.TryGetValue(householdId, out var infectedCases))
                continue;
            if (HealthQueries.HasActiveCondition(state, character.Id, diseaseProfile.ConditionId) ||
                HealthQueries.IsImmune(state, character.Id, diseaseProfile.ConditionId))
                continue;

            // A Character already carrying the disease does not spread it to themselves — exclude
            // their own case(s) from the source count when they happen to be a household member of
            // another infected member (they simply aren't rolled at all, per the guard above, so no
            // extra exclusion logic is needed here beyond that guard).
            var sourceMultiplierSum = 0.0;
            foreach (var infected in infectedCases)
                sourceMultiplierSum += QuarantineEffectCalculator.PersonalSpreadMultiplier(infected.Quarantined);
            var averageSourceMultiplier = sourceMultiplierSum / infectedCases.Count;

            var probability = EpidemicSpreadCalculator.HouseholdContactSpreadProbability(
                infectedCases.Count, sanitationMultiplier, averageSourceMultiplier, settlementMultiplier);
            var threshold = (uint)Math.Clamp(probability * RollPrecision, 0, RollPrecision);
            if (threshold == 0)
                continue;

            var roll = context.RandomStreams.NextUInt(_streamName, RollPrecision);
            if (roll >= threshold)
                continue;

            var command = new AfflictCharacterCommand(
                state.CommandIds.Issue(), "system", context.Date, CausationId: null, character.Id,
                diseaseProfile.ConditionId, HealthConditionCategory.Acute, HasCure: false, OnsetSeverity);
            var result = AfflictCharacterCommands.Pipeline.Execute(state, command);
            if (result.Accepted)
                events.AddRange(result.Events);
        }
    }

    private void SpreadWaterborne(
        WorldState state, MonthlyTickContext context, EpidemicDiseaseProfile diseaseProfile,
        Character[] residents, double sanitationMultiplier, double settlementMultiplier, List<IDomainEvent> events)
    {
        var activeCases = residents.Count(character =>
            HealthQueries.HasActiveCondition(state, character.Id, diseaseProfile.ConditionId));
        if (activeCases == 0)
            return;

        var probability = EpidemicSpreadCalculator.WaterborneSpreadProbability(activeCases, sanitationMultiplier, settlementMultiplier);
        var threshold = (uint)Math.Clamp(probability * RollPrecision, 0, RollPrecision);
        if (threshold == 0)
            return;

        foreach (var character in residents)
        {
            if (HealthQueries.HasActiveCondition(state, character.Id, diseaseProfile.ConditionId) ||
                HealthQueries.IsImmune(state, character.Id, diseaseProfile.ConditionId))
                continue;

            var roll = context.RandomStreams.NextUInt(_streamName, RollPrecision);
            if (roll >= threshold)
                continue;

            var command = new AfflictCharacterCommand(
                state.CommandIds.Issue(), "system", context.Date, CausationId: null, character.Id,
                diseaseProfile.ConditionId, HealthConditionCategory.Acute, HasCure: true, OnsetSeverity);
            var result = AfflictCharacterCommands.Pipeline.Execute(state, command);
            if (result.Accepted)
                events.AddRange(result.Events);
        }
    }
}

/// <summary>Emitted when <see cref="EpidemicContagionSystem"/> ignites a new <see cref="EpidemicOutbreak"/>.</summary>
public sealed record EpidemicOutbreakIgnitedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Settlement> SettlementId,
    RuntimeId<EpidemicOutbreak> OutbreakId,
    DefinitionId<HealthConditionDefinition> ConditionId) : IDomainEvent
{
    public string Type => "health.epidemicOutbreakIgnited";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { SettlementId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}

/// <summary>Emitted when <see cref="EpidemicContagionSystem"/> closes an <see cref="EpidemicOutbreak"/>
/// whose active case count has returned to zero.</summary>
public sealed record EpidemicOutbreakEndedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Settlement> SettlementId,
    RuntimeId<EpidemicOutbreak> OutbreakId,
    DefinitionId<HealthConditionDefinition> ConditionId) : IDomainEvent
{
    public string Type => "health.epidemicOutbreakEnded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { SettlementId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}

/// <summary>Emitted exactly once, the month <see cref="AntoninePlagueEra.Start"/> arrives — §9's own real
/// Event Chain onset marker, Imperial in scope since it names no single settlement.</summary>
public sealed record AntoninePlagueOnsetEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate) : IDomainEvent
{
    public string Type => "health.antoninePlagueOnset";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => Array.Empty<string>();
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}

/// <summary>Emitted exactly once, the month after <see cref="AntoninePlagueEra.End"/> — §9's own real
/// Event Chain waning marker; no already-open outbreak is force-closed by this (an active case still
/// resolves through its own real recovery/fatality roll), it only ends the Empire-wide ignition
/// elevation and the <see cref="EpidemicOutbreak.ImperialScale"/> stamp for any newly-ignited Pestilence
/// outbreak from this month on.</summary>
public sealed record AntoninePlagueWaningEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate) : IDomainEvent
{
    public string Type => "health.antoninePlagueWaning";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => Array.Empty<string>();
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}
