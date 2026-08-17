using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Events;

/// <summary>Emitted by every <see cref="SampleEventDefinitions"/> option's own <see
/// cref="EventOptionDefinition.Resolve"/> hook — the vertical slice's own worked example of "resolving
/// an option produces a real <see cref="IDomainEvent"/> through the command pipeline" (Phase 9 item 3),
/// mirroring how <see cref="Policies.FundFestivalCommand"/> stood in as item 2's one worked example.
/// Not meant as this system's permanent event vocabulary — a real event chain (Religion's Omens,
/// Natural Disasters' hazard rolls, per §1's own list of systems this engine exists for) will define
/// its own richer, chain-specific event types once that system lands.</summary>
public sealed record SampleEventOptionOutcomeEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<EventInstance> InstanceId,
    EventScope Scope,
    IReadOnlyList<string> SubjectIds,
    string OutcomeTag,
    string? CausationId) : IDomainEvent, IScopedEvent, IEventInstanceLinkedEvent
{
    public string Type => "events.sample.outcome";
    public int SchemaVersion => 1;
    public Visibility Visibility => EventVisibilityRules.For(Scope, SubjectIds);
}

/// <summary>A small, self-contained worked example proving every mechanic Phase 9 item 3 requires
/// end-to-end: one Personal-scope, Random-trigger, single-stage definition with two options (weighted
/// by an existing meter, <see cref="EventWeightInputs.FromCharacterLoyalty"/>), and one Household-scope,
/// Scripted-trigger, two-stage definition (a delayed second stage, §8). Neither is tuned or final
/// content — both are named <c>sample.*</c> deliberately, the same "vertical slice, not the final
/// content ceiling" framing the roadmap's own Phase 9 "vertical-slice contents" paragraph uses for
/// every other worked example in this pass.</summary>
public static class SampleEventDefinitions
{
    public static readonly DefinitionId<EventDefinition> DomesticMurmur = new("sample-domestic-murmur");
    public static readonly DefinitionId<EventOptionDefinition> ListenPatiently = new("listen-patiently");
    public static readonly DefinitionId<EventOptionDefinition> WaveItOff = new("wave-it-off");

    public static readonly DefinitionId<EventDefinition> HouseholdOmen = new("sample-household-omen");
    public static readonly DefinitionId<EventOptionDefinition> ConsultTheHaruspex = new("consult-the-haruspex");
    public static readonly DefinitionId<EventOptionDefinition> AcknowledgeTheOmen = new("acknowledge-the-omen");

    private const int OmenSecondStageDelayMonths = 2;

    public static EventCatalog BuildCatalog() => new(new[]
    {
        BuildDomesticMurmur(),
        BuildHouseholdOmen(),
    });

    private static EventDefinition BuildDomesticMurmur() => new(
        id: DomesticMurmur,
        scope: EventScope.Personal,
        nameKey: "events.sample-domestic-murmur.name",
        descriptionKey: "events.sample-domestic-murmur.description",
        triggerKind: EventTriggerKind.Random,
        eligibility: static (state, invocation) => IsLivingCharacter(state, invocation),
        stages: new[]
        {
            new EventStageDefinition(
                stageIndex: 0,
                nameKey: "events.sample-domestic-murmur.stage0.name",
                descriptionKey: "events.sample-domestic-murmur.stage0.description",
                options: new[]
                {
                    new EventOptionDefinition(
                        id: ListenPatiently,
                        nameKey: "events.sample-domestic-murmur.listen.name",
                        descriptionKey: "events.sample-domestic-murmur.listen.description",
                        eligibility: static (_, _) => null,
                        scoreForAi: static (_, _) => 1.0,
                        resolve: (state, instance, submittedDate, causationId) =>
                            AnnounceOutcome(state, instance, submittedDate, causationId, "listened")),
                    new EventOptionDefinition(
                        id: WaveItOff,
                        nameKey: "events.sample-domestic-murmur.wave-off.name",
                        descriptionKey: "events.sample-domestic-murmur.wave-off.description",
                        eligibility: static (_, _) => null,
                        scoreForAi: static (_, _) => 0.3,
                        resolve: (state, instance, submittedDate, causationId) =>
                            AnnounceOutcome(state, instance, submittedDate, causationId, "waved-off")),
                },
                expiryMonths: 2),
        },
        weight: static (state, invocation) => EventWeightInputs.FromCharacterLoyalty(state, invocation.SubjectIds[0]));

    private static EventDefinition BuildHouseholdOmen() => new(
        id: HouseholdOmen,
        scope: EventScope.Household,
        nameKey: "events.sample-household-omen.name",
        descriptionKey: "events.sample-household-omen.description",
        triggerKind: EventTriggerKind.Scripted,
        eligibility: static (state, invocation) => !EventHistory.HasEverFired(state, HouseholdOmen, invocation.SubjectIds),
        stages: new[]
        {
            new EventStageDefinition(
                stageIndex: 0,
                nameKey: "events.sample-household-omen.stage0.name",
                descriptionKey: "events.sample-household-omen.stage0.description",
                options: new[]
                {
                    new EventOptionDefinition(
                        id: ConsultTheHaruspex,
                        nameKey: "events.sample-household-omen.consult.name",
                        descriptionKey: "events.sample-household-omen.consult.description",
                        eligibility: static (_, _) => null,
                        scoreForAi: static (_, _) => 1.0,
                        resolve: static (state, instance, submittedDate, causationId) => new IDomainEvent[]
                        {
                            new SampleEventOptionOutcomeEvent(
                                state.EventIds.Issue(), submittedDate, instance.InstanceId, instance.Scope,
                                instance.SubjectIds, "consulted-haruspex", causationId),
                        }),
                },
                expiryMonths: 3,
                nextStageDelayMonths: OmenSecondStageDelayMonths),
            new EventStageDefinition(
                stageIndex: 1,
                nameKey: "events.sample-household-omen.stage1.name",
                descriptionKey: "events.sample-household-omen.stage1.description",
                options: new[]
                {
                    new EventOptionDefinition(
                        id: AcknowledgeTheOmen,
                        nameKey: "events.sample-household-omen.acknowledge.name",
                        descriptionKey: "events.sample-household-omen.acknowledge.description",
                        eligibility: static (_, _) => null,
                        scoreForAi: static (_, _) => 1.0,
                        resolve: static (state, instance, submittedDate, causationId) => new IDomainEvent[]
                        {
                            new SampleEventOptionOutcomeEvent(
                                state.EventIds.Issue(), submittedDate, instance.InstanceId, instance.Scope,
                                instance.SubjectIds, "omen-fulfilled", causationId),
                        }),
                },
                expiryMonths: 3),
        },
        scriptedCondition: static (_, _) => true,
        cadenceMonths: 1);

    private static bool IsLivingCharacter(WorldState state, EventInvocation invocation) =>
        state.Characters.TryGet(RuntimeId<Character>.Parse(invocation.SubjectIds[0]), out var character) &&
        character.DeathRecord is null;

    /// <summary>Announces the outcome only — deliberately does not mutate <see
    /// cref="WorldState.Characters"/> even though <see cref="BuildDomesticMurmur"/>'s own weight
    /// function reads <see cref="Character.Condition"/>'s Loyalty (via <see
    /// cref="EventWeightInputs.FromCharacterLoyalty"/>): <see cref="EventPoolSystem"/>'s own declared
    /// <c>Writes</c> set (ADR 0005) only names the Events-namespace partitions its own mechanics touch,
    /// so a content-authored <see cref="EventOptionDefinition.Resolve"/> hook invoked from inside its
    /// <c>Tick</c> mutating a further partition (e.g. Characters) would need that partition added
    /// there too — real content wiring an option to a concrete stat change is exactly where that
    /// extension happens; this vertical-slice sample deliberately stays inside the partitions
    /// <see cref="EventPoolSystem"/> already declares.</summary>
    private static IReadOnlyList<IDomainEvent> AnnounceOutcome(
        WorldState state, EventInstance instance, GameDate submittedDate, string? causationId, string outcomeTag) => new IDomainEvent[]
    {
        new SampleEventOptionOutcomeEvent(
            state.EventIds.Issue(), submittedDate, instance.InstanceId, instance.Scope, instance.SubjectIds, outcomeTag, causationId),
    };
}
