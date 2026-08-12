using System.Text.Json;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Saves;

/// <summary>Maps between the live <see cref="WorldState"/> and its canonical <see
/// cref="WorldSaveDocument"/> persisted shape (ADR 0010). Every collection is sorted here, before it
/// ever reaches <see cref="CanonicalJson"/>, so a re-save of unchanged state is byte-identical.</summary>
public static class WorldStateMapper
{
    public static WorldSaveDocument ToDto(WorldState state)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        return new WorldSaveDocument
        {
            DateTotalMonths = state.Date.TotalMonths,
            NextCommandSequenceNumber = state.NextCommandSequenceNumber,
            Counters = new CounterSetDto
            {
                RegionIds = state.RegionIds.Peek,
                SettlementIds = state.SettlementIds.Peek,
                PlotIds = state.PlotIds.Peek,
                HouseholdIds = state.HouseholdIds.Peek,
                ActorIds = state.ActorIds.Peek,
                CharacterIds = state.CharacterIds.Peek,
                BuildingIds = state.BuildingIds.Peek,
                ContractIds = state.ContractIds.Peek,
                ActivityIds = state.ActivityIds.Peek,
                CommandIds = state.CommandIds.Peek,
                EventIds = state.EventIds.Peek,
            },
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            CharacterIds = state.Characters.InAscendingOrder().Select(entry => entry.Key.ToTaggedString()).ToArray(),
            // Already deterministic key order (ADR 0004) via KnowledgeState.All.
            Knowledge = state.Knowledge.All().Select(ToKnowledgeDto).ToArray(),
        };
    }

    public static WorldState ToWorldState(WorldSaveDocument dto)
    {
        if (dto is null)
            throw new ArgumentNullException(nameof(dto));

        var characters = OrderedRegistry<RuntimeId<Character>, object>.Restore(
            dto.CharacterIds.Select(tagged => new KeyValuePair<RuntimeId<Character>, object>(
                RuntimeId<Character>.Parse(tagged), new object())));

        var knowledge = KnowledgeState.Restore(dto.Knowledge.Select(FromKnowledgeDto));

        return new WorldState(
            date: new GameDate(dto.DateTotalMonths),
            regionIds: RuntimeIdCounter<Region>.Restore(dto.Counters.RegionIds),
            settlementIds: RuntimeIdCounter<Settlement>.Restore(dto.Counters.SettlementIds),
            plotIds: RuntimeIdCounter<Plot>.Restore(dto.Counters.PlotIds),
            householdIds: RuntimeIdCounter<Household>.Restore(dto.Counters.HouseholdIds),
            actorIds: RuntimeIdCounter<Actor>.Restore(dto.Counters.ActorIds),
            characterIds: RuntimeIdCounter<Character>.Restore(dto.Counters.CharacterIds),
            buildingIds: RuntimeIdCounter<Building>.Restore(dto.Counters.BuildingIds),
            contractIds: RuntimeIdCounter<Contract>.Restore(dto.Counters.ContractIds),
            activityIds: RuntimeIdCounter<Activity>.Restore(dto.Counters.ActivityIds),
            commandIds: RuntimeIdCounter<Command>.Restore(dto.Counters.CommandIds),
            eventIds: RuntimeIdCounter<DomainEventEntity>.Restore(dto.Counters.EventIds),
            characters: characters,
            knowledge: knowledge,
            nextCommandSequenceNumber: dto.NextCommandSequenceNumber);
    }

    private static KnowledgeEntryDto ToKnowledgeDto(KeyValuePair<KnowledgeKey, KnowledgeEntry> entry) => new()
    {
        ObserverId = entry.Key.ObserverId,
        SubjectId = entry.Key.SubjectId,
        Topic = entry.Key.Topic,
        ValueJson = JsonSerializer.Serialize(entry.Value.Value, entry.Value.Value.GetType(), CanonicalJson.Options),
        Confidence = entry.Value.Confidence.ToString(),
        AsOfDateTotalMonths = entry.Value.AsOfDate.TotalMonths,
        ProvenanceEventId = entry.Value.ProvenanceEventId,
    };

    private static KeyValuePair<KnowledgeKey, KnowledgeEntry> FromKnowledgeDto(KnowledgeEntryDto dto)
    {
        using var document = JsonDocument.Parse(dto.ValueJson);
        var value = document.RootElement.Clone();
        var confidence = Enum.Parse<KnowledgeConfidence>(dto.Confidence);
        var key = new KnowledgeKey(dto.ObserverId, dto.SubjectId, dto.Topic);
        var entry = new KnowledgeEntry(value, confidence, new GameDate(dto.AsOfDateTotalMonths), dto.ProvenanceEventId);
        return new KeyValuePair<KnowledgeKey, KnowledgeEntry>(key, entry);
    }
}
