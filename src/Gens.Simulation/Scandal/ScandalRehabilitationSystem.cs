using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Scandal;

/// <summary>Emitted whenever <see cref="ScandalRehabilitationSystem"/> grants <see
/// cref="ScandalCatalog.RehabilitatedTraitId"/>. Public, matching <see
/// cref="ScandalRecordedEvent"/>'s own reasoning — §8 frames Rehabilitation as "genuine, earned
/// redemption," every bit as real and legible a public fact as the original disgrace.</summary>
public sealed record CharacterRehabilitatedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Characters.Character> CharacterId,
    RuntimeId<Household> HouseholdId,
    string? CausationId) : IDomainEvent
{
    public string Type => "scandal.characterRehabilitated";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// §8's Rehabilitation payoff: "the real, existing payoff for sustained good conduct following a
/// Scandal... a real, sustained stretch without further incident, converting a lingering Scandal-Marked
/// stigma into genuine, earned redemption." Matches <see cref="Reputation.FavorExpirationSystem"/>'s and
/// <see cref="Magistracies.MagistracyTermSystem"/>'s own age-gated-check shape: every month, every living
/// Character who still carries <see cref="ScandalCatalog.ScandalMarkedTraitId"/> but not yet <see
/// cref="ScandalCatalog.RehabilitatedTraitId"/> is checked against their own household's most recent
/// <see cref="ScandalRecord"/> (<see cref="ScandalResolver.MostRecentScandalDate"/> — "a further
/// incident" resets the clock regardless of that later Scandal's own severity or whether it granted a
/// Trait). Additive, not a replacement: Rehabilitated is granted alongside Scandal-Marked, never
/// removing it — the earned redemption sits beside the enduring mark on the record, rather than erasing
/// the history that produced it. Runs in <see cref="TickPhase.RelationshipsActors"/>, the same phase
/// every other actor-standing system in this domain runs in.
/// </summary>
public sealed class ScandalRehabilitationSystem : IMonthlySystem<WorldState>
{
    public string Id => "scandal.rehabilitation";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "characters", "scandalRecords" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "characters", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();
        var toRehabilitate = new List<Characters.Character>();

        foreach (var entry in state.Characters.InAscendingOrder())
        {
            var character = entry.Value;
            if (!character.IsAlive || character.Household is not { } householdId)
                continue;
            if (!character.Traits.Contains(ScandalCatalog.ScandalMarkedTraitId))
                continue;
            if (character.Traits.Contains(ScandalCatalog.RehabilitatedTraitId))
                continue;

            var mostRecent = ScandalResolver.MostRecentScandalDate(state, householdId);
            if (mostRecent is not { } recordedDate)
                continue;

            var ageInMonths = context.Date.TotalMonths - recordedDate.TotalMonths;
            if (ageInMonths >= ScandalCatalog.RehabilitationAfterMonths)
                toRehabilitate.Add(character);
        }

        foreach (var character in toRehabilitate)
        {
            var updatedTraits = character.Traits.Append(ScandalCatalog.RehabilitatedTraitId).ToArray();
            state.Characters.Remove(character.Id);
            state.Characters.Add(character.Id, character with { Traits = updatedTraits });

            events.Add(new CharacterRehabilitatedEvent(
                state.EventIds.Issue(), context.Date, character.Id, character.Household!.Value, CausationId: null));
        }

        return events;
    }
}
