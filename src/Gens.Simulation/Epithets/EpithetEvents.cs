using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Epithets;

/// <summary>Emitted whenever <see cref="EpithetGenerationSystem"/> mints a new <see cref="Agnomen"/>
/// (Phase 11 item 5).</summary>
public sealed record AgnomenGrantedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Agnomen> AgnomenId,
    RuntimeId<Character> CharacterId,
    string Name,
    string? CausationId) : IDomainEvent
{
    public string Type => "epithets.agnomenGranted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>Emitted whenever <see cref="AdoptAgnomenAsCognomenCommand"/> is accepted (§5).</summary>
public sealed record CognomenAdoptedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<InheritedCognomenDecision> DecisionId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Agnomen> AgnomenId,
    string? CausationId) : IDomainEvent
{
    public string Type => "epithets.cognomenAdopted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>Emitted whenever <see cref="EpithetGenerationSystem"/> sets or changes a household's <see
/// cref="DynasticEpithet"/> (§6).</summary>
public sealed record DynasticEpithetChangedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    string EpithetText,
    string? CausationId) : IDomainEvent
{
    public string Type => "epithets.dynasticEpithetChanged";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}
