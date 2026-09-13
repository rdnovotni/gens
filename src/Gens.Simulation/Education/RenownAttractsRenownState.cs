using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Education;

/// <summary>One household's Renown Attracts Renown standing (Phase 17 item 2; §12's own model), keyed by
/// household. Sparse: a household that has never crossed the recognition threshold has no entry, matching
/// <see cref="HouseholdCulturalPrestige"/>'s identical convention.</summary>
public sealed record RenownAttractsRenownState(RuntimeId<Household> HouseholdId, bool IncomingForeignStudentOpportunityActive);

/// <summary>Emitted the first time a household crosses the Renown Attracts Renown recognition threshold.</summary>
public sealed record RenownAttractsRenownThresholdCrossedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    string? CausationId) : IDomainEvent
{
    public string Type => "education.renownAttractsRenownThresholdCrossed";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// Versioned constant for §12's Renown Attracts Renown recognition threshold — this implementation's own
/// unsized baseline, matching <see cref="CulturalPrestigeCatalog"/>'s identical disclaimer.
/// </summary>
public static class RenownAttractsRenownCatalog
{
    public const int RecognitionPrestigeThreshold = 25;
}

/// <summary>
/// The monthly Renown Attracts Renown check (Phase 17 item 2; §12): "a rare, earned endgame payoff," per
/// the design's own framing — this ticket only flips the flag and records the tie the first time a
/// household's Cultural Prestige clears <see
/// cref="RenownAttractsRenownCatalog.RecognitionPrestigeThreshold"/> while it also owns/occupies an
/// operational Academia or Schola (§12's own "household School/Academy presence" condition, checked via
/// <see cref="EducationBuildingResolver.HasOperationalBuilding"/> — correctness fix: this used to check
/// for an active Cultural Patronage commitment instead, which is a different, unrelated §7 concept with no
/// building-ownership implication at all). No Library/Bibliotheca building exists anywhere in this
/// codebase's <see cref="Buildings.BuildingDefinition"/> catalog, so "Academia at minimum" per this fix's
/// own scope decision also counts a plain <see cref="KnownEducationTracks.Schola"/> — matching this file's
/// own prior "School/Academy" phrasing, and Rhetoric/Philosophy already treat Schola/Academia as
/// interchangeable "delivers a Track" buildings (see <see cref="KnownEducationTracks.Catalog"/>) rather
/// than Academia alone being some stricter, more prestigious tier. Not a full foreign-student gameplay
/// loop — no Clientela-adjacent consumer is wired here, matching this ticket's own confirmed scope
/// decision.
/// </summary>
public sealed class RenownAttractsRenownSystem : IMonthlySystem<WorldState>
{
    public string Id => "education.renownAttractsRenown";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } =
        new[] { "householdCulturalPrestiges", "holdings", "plots", "buildings", "renownAttractsRenownStates" };

    private static readonly IReadOnlyCollection<DefinitionId<Identity.Building>> QualifyingBuildingIds =
        new[] { KnownEducationTracks.Academia, KnownEducationTracks.Schola };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "renownAttractsRenownStates", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var entry in state.HouseholdCulturalPrestiges.InAscendingOrder().ToArray())
        {
            var householdId = entry.Key;
            if (state.RenownAttractsRenownStates.TryGet(householdId, out var existing) &&
                existing.IncomingForeignStudentOpportunityActive)
            {
                continue;
            }

            var clearsThreshold = entry.Value.Prestige >= RenownAttractsRenownCatalog.RecognitionPrestigeThreshold;
            var hasQualifyingBuilding = EducationBuildingResolver.HasOperationalBuilding(state, householdId, QualifyingBuildingIds);

            if (!clearsThreshold || !hasQualifyingBuilding)
                continue;

            if (state.RenownAttractsRenownStates.TryGet(householdId, out _))
                state.RenownAttractsRenownStates.Remove(householdId);
            state.RenownAttractsRenownStates.Add(householdId, new RenownAttractsRenownState(householdId, true));

            events.Add(new RenownAttractsRenownThresholdCrossedEvent(state.EventIds.Issue(), context.Date, householdId, CausationId: null));
        }

        return events;
    }
}
