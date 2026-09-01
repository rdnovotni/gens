using System.Text;
using System.Text.Json;
using Gens.Simulation.Actors;
using Gens.Simulation.Buildings;
using Gens.Simulation.Characters;
using Gens.Simulation.Clientela;
using Gens.Simulation.Collegia;
using Gens.Simulation.Correspondence;
using Gens.Simulation.Funerary;
using Gens.Simulation.Goods;
using Gens.Simulation.Health;
using Gens.Simulation.History;
using Gens.Simulation.Interactions;
using Gens.Simulation.Land;
using Gens.Simulation.Languages;
using Gens.Simulation.Ledger;
using Gens.Simulation.Magistracies;
using Gens.Simulation.Markets;
using Gens.Simulation.Reputation;
using Gens.Simulation.Scandal;
using Gens.Simulation.Stewardship;
using Gens.Simulation.Succession;
using Gens.Simulation.Travel;
using Gens.Simulation.Wanderers;

namespace Gens.Simulation.State;

/// <summary>
/// Folds <see cref="WorldState"/>'s ordered partitions (ADR 0004) into a stable 64-bit hash — the
/// literal mechanism the Phase 2 exit gate depends on ("the same seed plus the same ordered
/// commands produces identical event logs and state hashes across repeated headless runs"). Every
/// input is already canonically ordered, so no separate "sort before hashing" step is needed. This
/// never calls <see cref="object.GetHashCode"/> on a string: that method is randomized per process
/// in modern .NET and would silently break reproducibility across separate runs while still passing
/// every single-process test. All hashing here is over raw UTF-8 bytes and integers instead.
/// </summary>
public static class StateHasher
{
    private const ulong OffsetBasis = 14695981039346656037UL;
    private const ulong Prime = 1099511628211UL;

    public static ulong Hash(WorldState state)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var hash = OffsetBasis;
        hash = MixLong(hash, state.Date.TotalMonths);
        hash = MixLong(hash, state.RegionIds.Peek);
        hash = MixLong(hash, state.SettlementIds.Peek);
        hash = MixLong(hash, state.PlotIds.Peek);
        hash = MixLong(hash, state.HouseholdIds.Peek);
        hash = MixLong(hash, state.ActorIds.Peek);
        hash = MixLong(hash, state.CharacterIds.Peek);
        hash = MixLong(hash, state.BuildingIds.Peek);
        hash = MixLong(hash, state.ContractIds.Peek);
        hash = MixLong(hash, state.ActivityIds.Peek);
        hash = MixLong(hash, state.CommandIds.Peek);
        hash = MixLong(hash, state.EventIds.Peek);
        hash = MixLong(hash, state.ScheduledActionIds.Peek);
        hash = MixLong(hash, state.HoldingIds.Peek);
        hash = MixLong(hash, state.LedgerTransactionIds.Peek);
        hash = MixLong(hash, state.SchemeIds.Peek);
        hash = MixLong(hash, state.SuccessionDisputeIds.Peek);
        hash = MixLong(hash, state.FuneralRecordIds.Peek);
        hash = MixLong(hash, state.AgnomenIds.Peek);
        hash = MixLong(hash, state.InheritedCognomenDecisionIds.Peek);
        hash = MixLong(hash, state.FavorObligationIds.Peek);
        hash = MixLong(hash, state.MagistracyRecordIds.Peek);
        hash = MixLong(hash, state.DistantHoldingIds.Peek);
        hash = MixLong(hash, state.CharacterHealthConditionIds.Peek);
        hash = MixLong(hash, state.EpidemicOutbreakIds.Peek);
        hash = MixLong(hash, state.DisasterEventIds.Peek);
        hash = MixLong(hash, state.WandererIds.Peek);
        hash = MixLong(hash, state.WandererEngagementIds.Peek);
        hash = MixLong(hash, state.NextCommandSequenceNumber);

        foreach (var entry in state.Characters.InAscendingOrder())
            hash = MixCharacter(hash, entry.Value);

        // Already ascending (From, To) order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.Relationships.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.From.Value);
            hash = MixLong(hash, entry.Key.To.Value);
            hash = MixLong(hash, entry.Value.Opinion);
            hash = MixLong(hash, (long)entry.Value.Bonds);
            hash = MixLong(hash, (long)entry.Value.Origin);
            hash = MixLong(hash, entry.Value.FormedDate.TotalMonths);
            hash = MixLong(hash, entry.Value.LastMeaningfulInteractionDate.TotalMonths);
            hash = MixString(hash, entry.Value.ProvenanceEventId ?? string.Empty);
        }

        // Already ascending (settlement, group type) order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.PopGroups.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.SettlementId.Value);
            hash = MixLong(hash, (long)entry.Key.GroupType);
            hash = MixLong(hash, entry.Value.Size);
            hash = MixLong(hash, entry.Value.LegalStatusDistribution.Citizen);
            hash = MixLong(hash, entry.Value.LegalStatusDistribution.LatinRights);
            hash = MixLong(hash, entry.Value.LegalStatusDistribution.Peregrine);
            hash = MixLong(hash, entry.Value.LegalStatusDistribution.Freedman);
            hash = MixString(hash, entry.Value.Culture.Value);
            hash = MixLong(hash, (long)entry.Value.WealthBand);
            hash = MixLong(hash, (long)entry.Value.NeedsProfile);
            hash = MixLong(hash, entry.Value.EmploymentRatio.RawValue);
            hash = MixLong(hash, entry.Value.HousingSatisfaction.RawValue);
            hash = MixLong(hash, entry.Value.Contentment.RawValue);
            hash = MixLong(hash, entry.Value.HealthExposure.RawValue);
        }

        // Already ascending (due date, action ID) order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.ScheduledActions.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Value.ActionId.Value);
            hash = MixLong(hash, entry.Value.DueDate.TotalMonths);
            hash = MixString(hash, entry.Value.ActorId);
            hash = MixString(hash, entry.Value.ActionType);
            hash = MixString(hash, entry.Value.PayloadJson);
            hash = MixString(hash, entry.Value.CausationId ?? string.Empty);
        }

        foreach (var entry in state.Knowledge.All())
        {
            hash = MixString(hash, entry.Key.ObserverId);
            hash = MixString(hash, entry.Key.SubjectId);
            hash = MixString(hash, entry.Key.Topic);
            hash = MixLong(hash, (long)entry.Value.Confidence);
            hash = MixLong(hash, entry.Value.AsOfDate.TotalMonths);
            hash = MixString(hash, entry.Value.ProvenanceEventId ?? string.Empty);
            hash = MixString(hash, JsonSerializer.Serialize(entry.Value.Value));
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.Regions.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Value.Id.Value);
            hash = MixString(hash, entry.Value.Name);
        }

        foreach (var entry in state.Settlements.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Value.Id.Value);
            hash = MixLong(hash, entry.Value.RegionId.Value);
            hash = MixLong(hash, (long)entry.Value.Stage);
        }

        foreach (var entry in state.Plots.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Value.Id.Value);
            hash = MixLong(hash, entry.Value.SettlementId.Value);
        }

        // Already ascending (household, slot) order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.HouseholdRegimenDefaults.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.HouseholdId.Value);
            hash = MixLong(hash, entry.Key.Slot is null ? -1L : (long)entry.Key.Slot.Value);
            hash = MixLong(hash, (long)entry.Value.Diet);
            hash = MixLong(hash, (long)entry.Value.Accommodation);
            hash = MixLong(hash, (long)entry.Value.Freedoms);
            hash = MixLong(hash, (long)entry.Value.Discipline);
        }

        foreach (var entry in state.Holdings.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Value.Id.Value);
            hash = MixLong(hash, entry.Value.SettlementId.Value);
            hash = MixString(hash, entry.Value.OwnerId ?? string.Empty);
            hash = MixString(hash, entry.Value.OccupantId ?? string.Empty);
            hash = MixLong(hash, entry.Value.ResidentCapacity);
            hash = MixLong(hash, entry.Value.Villa is null ? -1 : (long)entry.Value.Villa.Stage);
            if (entry.Value.Villa is not null)
            {
                hash = MixLong(hash, entry.Value.Villa.IsOutpost ? 1 : 0);
                foreach (var room in entry.Value.Villa.Rooms)
                {
                    hash = MixString(hash, room.Key);
                    hash = MixString(hash, room.Definition.Key);
                    hash = MixLong(hash, (long)room.Definition.MinimumStage);
                    hash = MixLong(hash, room.Definition.MaximumTier);
                    hash = MixLong(hash, room.Definition.UsesRoomSlot ? 1 : 0);
                    hash = MixLong(hash, room.Tier);
                    hash = MixLong(hash, (long)room.CapacityTier);
                    hash = MixLong(hash, (long)room.Condition);
                    hash = MixString(hash, room.AssignedTo ?? string.Empty);
                }
            }
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.Buildings.InAscendingOrder())
            hash = MixBuilding(hash, entry.Value);

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.Stockpiles.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixStockpile(hash, entry.Value);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.ConstructionSchedules.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            foreach (var project in entry.Value.Projects)
            {
                hash = MixLong(hash, project.Sequence);
                hash = MixLong(hash, project.PlotId.Value);
                hash = MixBuildingDefinition(hash, project.Definition);
                hash = MixLong(hash, project.CompletedMonths);
            }
        }

        // Already ascending LedgerAccountKey order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.LedgerAccounts.InAscendingOrder())
        {
            hash = MixLong(hash, (long)entry.Key.Kind);
            hash = MixString(hash, entry.Key.OwnerId);
            hash = MixLong(hash, entry.Value.Balance.RawValue);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.LedgerTransactions.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.OccurredDate.TotalMonths);
            hash = MixLong(hash, (long)entry.Value.Category);
            foreach (var posting in entry.Value.Postings)
            {
                hash = MixLong(hash, (long)posting.Account.Kind);
                hash = MixString(hash, posting.Account.OwnerId);
                hash = MixLong(hash, posting.Amount.RawValue);
            }

            hash = MixString(hash, entry.Value.Reference ?? string.Empty);
        }

        // Already ascending MarketGoodKey order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.MarketPrices.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.SettlementId.Value);
            hash = MixString(hash, entry.Key.GoodId.Value);
            hash = MixLong(hash, entry.Value.Price.RawValue);
            hash = MixLong(hash, entry.Value.PreviousPrice.RawValue);
            hash = MixLong(hash, entry.Value.Supply);
            hash = MixLong(hash, entry.Value.Demand);
            hash = MixLong(hash, entry.Value.ClearedQuantity);
            hash = MixLong(hash, entry.Value.UnsatisfiedDemand);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.Actors.InAscendingOrder())
            hash = MixLivingWorldActor(hash, entry.Value);

        // Already ascending HouseStandingKey order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.HouseStandings.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.ActorAId.Value);
            hash = MixLong(hash, entry.Key.ActorBId.Value);
            hash = MixLong(hash, (long)entry.Value.Standing);
            hash = MixLong(hash, entry.Value.Grudge is null ? 0 : 1);
            hash = MixString(hash, entry.Value.Grudge?.OriginEngagementId ?? string.Empty);
            hash = MixLong(hash, entry.Value.Grudge?.OriginDate.TotalMonths ?? 0);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.RivalDossiers.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixString(hash, entry.Value.Summary);
            hash = MixString(hash, entry.Value.HeadComboTitle ?? string.Empty);
            hash = MixLong(hash, entry.Value.LastUpdatedDate.TotalMonths);
            foreach (var chronicleEntryId in entry.Value.RecentChronicleEntries)
                hash = MixLong(hash, chronicleEntryId.Value);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.RegionalFamiliesEntries.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixString(hash, entry.Value.Name);
            hash = MixLong(hash, (long)entry.Value.StandingTrend);
            hash = MixLong(hash, entry.Value.IdentityEconomic is null ? -1L : (long)entry.Value.IdentityEconomic.Value);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.StewardshipAssignments.InAscendingOrder())
            hash = MixStewardshipAssignment(hash, entry.Value);

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.AutonomousDecisionLogs.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Value.LogId.Value);
            hash = MixLong(hash, entry.Value.AssignmentId.Value);
            hash = MixLong(hash, entry.Value.Month.TotalMonths);
            hash = MixString(hash, entry.Value.DecisionType);
            hash = MixString(hash, entry.Value.Outcome);
            hash = MixLong(hash, entry.Value.CompetenceRollFactor);
            hash = MixLong(hash, entry.Value.LoyaltyRiskRollFactor);
            hash = MixLong(hash, entry.Value.IncidentType is null ? -1L : (long)entry.Value.IncidentType.Value);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.Schemes.InAscendingOrder())
            hash = MixScheme(hash, entry.Value);

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.ReturnReports.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Value.ReportId.Value);
            hash = MixLong(hash, entry.Value.AssignmentId.Value);
            foreach (var summaryEntry in entry.Value.SummaryEntries)
                hash = MixString(hash, summaryEntry);
            hash = MixLong(hash, entry.Value.TotalTreasuryImpact.RawValue);
            foreach (var incidentLogId in entry.Value.IncidentsDiscovered)
                hash = MixLong(hash, incidentLogId.Value);
            hash = MixLong(hash, entry.Value.ChronicleWorthy ? 1L : 0L);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.HouseholdHeadships.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.HeadCharacterId.Value);
            hash = MixLong(hash, entry.Value.SinceDate.TotalMonths);
            hash = MixLong(hash, entry.Value.RegentCharacterId?.Value ?? -1L);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.HeirDesignations.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.PreferredHeirId?.Value ?? -1L);
            hash = MixLong(hash, entry.Value.FormallyDeclaredHeirId?.Value ?? -1L);
            hash = MixLong(hash, entry.Value.DeclaredDate?.TotalMonths ?? -1L);
            foreach (var id in entry.Value.DisownedCharacterIds)
                hash = MixLong(hash, id.Value);
            foreach (var id in entry.Value.AdoptedChildIds)
                hash = MixLong(hash, id.Value);
            foreach (var id in entry.Value.AcknowledgedIllegitimateChildIds)
                hash = MixLong(hash, id.Value);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.SuccessionDisputes.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Value.DisputeId.Value);
            hash = MixLong(hash, entry.Value.HouseholdId.Value);
            hash = MixLong(hash, entry.Value.DeceasedHeadId.Value);
            foreach (var id in entry.Value.ClaimantIds)
                hash = MixLong(hash, id.Value);
            hash = MixLong(hash, entry.Value.OpenedDate.TotalMonths);
            hash = MixLong(hash, entry.Value.ResolutionDueDate.TotalMonths);
            hash = MixLong(hash, (long)entry.Value.Status);
            hash = MixLong(hash, entry.Value.WinnerCharacterId?.Value ?? -1L);
            hash = MixLong(hash, entry.Value.SplinterClaimantId?.Value ?? -1L);
            hash = MixLong(hash, entry.Value.SplinterHouseholdId?.Value ?? -1L);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.PlayerControls.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.ControlledCharacterId?.Value ?? -1L);
            hash = MixLong(hash, (long)entry.Value.Mode);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.ChronicleEntries.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.HouseholdId?.Value ?? -1L);
            hash = MixLong(hash, entry.Value.Month.TotalMonths);
            hash = MixLong(hash, (long)entry.Value.Category);
            hash = MixLong(hash, (long)entry.Value.Tier);
            hash = MixString(hash, entry.Value.Prose);
            foreach (var characterId in entry.Value.LinkedCharacterIds)
                hash = MixLong(hash, characterId.Value);
            hash = MixString(hash, entry.Value.SourceSystem);
            hash = MixLong(hash, (long)entry.Value.Source);
            hash = MixLong(hash, entry.Value.Pinned ? 1L : 0L);
            hash = MixString(hash, entry.Value.PlayerAnnotation ?? string.Empty);
            hash = MixLong(hash, entry.Value.CrossHouseLinkedEntryId?.Value ?? -1L);
        }

        // Already ascending GenerationalChapterKey order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.GenerationalChapters.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.HouseholdId.Value);
            hash = MixLong(hash, entry.Key.StartMonthTotalMonths);
            hash = MixLong(hash, entry.Value.HeadCharacterId.Value);
            hash = MixLong(hash, entry.Value.EndMonth?.TotalMonths ?? -1L);
            hash = MixString(hash, entry.Value.ChapterSummary);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.FuneralRecords.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.HouseholdId.Value);
            hash = MixLong(hash, entry.Value.DeceasedCharacterId.Value);
            hash = MixLong(hash, entry.Value.DeathDate.TotalMonths);
            hash = MixLong(hash, (long)entry.Value.Status);
            hash = MixLong(hash, entry.Value.Tier is { } tier ? (long)tier : -1L);
            hash = MixLong(hash, entry.Value.BurialMethod is { } burial ? (long)burial : -1L);
            hash = MixLong(hash, entry.Value.InterredAt is { } interredAt ? (long)interredAt : -1L);
            hash = MixLong(hash, entry.Value.HeldDate?.TotalMonths ?? -1L);
            hash = MixLong(hash, entry.Value.Cost?.RawValue ?? -1L);
            hash = MixLong(hash, entry.Value.MemoriaGained ?? -1L);
            hash = MixLong(hash, entry.Value.ImaginesDisplayed ? 1L : 0L);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.MourningPeriods.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.TriggeringDeathCharacterId.Value);
            hash = MixLong(hash, entry.Value.StartDate.TotalMonths);
            hash = MixLong(hash, entry.Value.EndDate.TotalMonths);
            hash = MixLong(hash, entry.Value.BrokenEarly ? 1L : 0L);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.MemoriaStates.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.Memoria);
            hash = MixLong(hash, entry.Value.LastParentaliaObservedDate?.TotalMonths ?? -1L);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.Agnomens.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.CharacterId.Value);
            hash = MixLong(hash, (long)entry.Value.AgnomenType);
            hash = MixString(hash, entry.Value.Name);
            hash = MixLong(hash, (long)entry.Value.GrantMethod);
            hash = MixLong(hash, entry.Value.GrantedDate.TotalMonths);
            foreach (var sourceId in entry.Value.SourceChronicleEntryIds)
                hash = MixLong(hash, sourceId.Value);
            hash = MixLong(hash, entry.Value.SourceSuccessionDisputeId?.Value ?? -1L);
            hash = MixLong(hash, entry.Value.DignitasEffect ?? long.MinValue);
            hash = MixLong(hash, entry.Value.FameEffect ?? long.MinValue);
            hash = MixLong(hash, entry.Value.IsSuppressible ? 1L : 0L);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.InheritedCognomenDecisions.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.OriginalAgnomenId.Value);
            hash = MixLong(hash, entry.Value.DecidingHouseholdId.Value);
            hash = MixLong(hash, entry.Value.AdoptedAsPermanentCognomen ? 1L : 0L);
            hash = MixLong(hash, entry.Value.EffectiveFromDate.TotalMonths);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.DynasticEpithets.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixString(hash, entry.Value.EpithetText);
            foreach (var sourceId in entry.Value.DerivedFromChronicleEntryIds)
                hash = MixLong(hash, sourceId.Value);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.HouseholdReputations.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.Dignitas);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.FavorObligations.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.GrantorId.Value);
            hash = MixLong(hash, entry.Value.BeneficiaryId.Value);
            hash = MixString(hash, entry.Value.Kind);
            hash = MixLong(hash, entry.Value.GrantedDate.TotalMonths);
            hash = MixLong(hash, (long)entry.Value.Status);
            hash = MixLong(hash, entry.Value.ResolvedDate?.TotalMonths ?? -1L);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.ClientelaEntries.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.PatronHouseholdId.Value);
            hash = MixLong(hash, (long)entry.Value.Specialty);
            hash = MixLong(hash, entry.Value.RecruitedDate.TotalMonths);
            hash = MixLong(hash, entry.Value.LastFavorCalledDate?.TotalMonths ?? -1L);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.HouseholdInfluences.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.Influence);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.CharacterFactionAlignments.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, (long)entry.Value.Faction);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.MagistracyRecords.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.HolderId.Value);
            hash = MixLong(hash, (long)entry.Value.Office);
            hash = MixLong(hash, entry.Value.SettlementId.Value);
            hash = MixLong(hash, entry.Value.TermStartDate.TotalMonths);
            hash = MixLong(hash, entry.Value.TermEndDate?.TotalMonths ?? -1L);
            hash = MixLong(hash, entry.Value.LossReason is { } reason ? (long)reason : -1L);
            hash = MixLong(hash, entry.Value.CoHolderId?.Value ?? -1L);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.Collegia.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, (long)entry.Value.CollegiumType);
            hash = MixLong(hash, (long)entry.Value.LegalStatus);
            hash = MixLong(hash, entry.Value.LinkedPopGroupType is { } popGroupType ? (long)popGroupType : -1L);
            hash = MixLong(hash, entry.Value.LinkedPatronDeity is { } patronDeity ? (long)patronDeity : -1L);
            hash = MixString(hash, entry.Value.ScholaPropertyId ?? string.Empty);
            hash = MixLong(hash, entry.Value.PatronHouseholdId?.Value ?? -1L);
            hash = MixLong(hash, entry.Value.QuinquennalisCharacterId?.Value ?? -1L);
            foreach (var memberId in entry.Value.MemberHouseholdIds)
                hash = MixLong(hash, memberId.Value);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.ScandalRecords.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.PrimaryHouseholdId.Value);
            hash = MixLong(hash, (long)entry.Value.SourceType);
            hash = MixLong(hash, (long)entry.Value.Severity);
            hash = MixLong(hash, (long)entry.Value.Scope);
            hash = MixLong(hash, entry.Value.RecordedDate.TotalMonths);
            hash = MixLong(hash, entry.Value.OriginatedViaLibellusFamosus ? 1L : 0L);
            hash = MixLong(hash, entry.Value.CurrentFameEffect ?? long.MinValue);
            hash = MixLong(hash, entry.Value.ScandalMarkedTraitApplied ? 1L : 0L);
            hash = MixLong(hash, entry.Value.NotaCensoriaIssued ? 1L : 0L);
            hash = MixLong(hash, entry.Value.FactionReception.TraditionalistReading);
            hash = MixLong(hash, entry.Value.FactionReception.PopularistReading);
            hash = MixLong(hash, entry.Value.IsActive ? 1L : 0L);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.CharacterFames.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.Fame);
        }

        // Already ascending (household, Doctrine type) order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.HouseholdDoctrines.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.HouseholdId.Value);
            hash = MixLong(hash, (long)entry.Key.DoctrineType);
            hash = MixLong(hash, entry.Value.AffinityScore);
            hash = MixLong(hash, (long)entry.Value.Tier);
            hash = MixLong(hash, entry.Value.CapstoneUnlocked ? 1L : 0L);
            hash = MixLong(hash, entry.Value.CapstoneUsedThisGeneration ? 1L : 0L);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.EdictRecords.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.IssuingHouseholdId.Value);
            hash = MixLong(hash, (long)entry.Value.Type);
            hash = MixLong(hash, entry.Value.IssuedDate.TotalMonths);
            hash = MixLong(hash, entry.Value.InfluenceCost);
            hash = MixLong(hash, entry.Value.DignitasCostToIssue);
            hash = MixLong(hash, entry.Value.ScandalId.Value);
            hash = MixLong(hash, entry.Value.LegalCaseId?.Value ?? -1L);
            hash = MixLong(hash, entry.Value.DemonstrationEffectTriggered ? 1L : 0L);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.TravelTrips.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.Party.TravelerId.Value);
            foreach (var retinueId in entry.Value.Party.RetinueIds)
                hash = MixLong(hash, retinueId.Value);
            hash = MixTravelLocation(hash, entry.Value.Origin);
            hash = MixTravelLocation(hash, entry.Value.Destination);
            hash = MixLong(hash, (long)entry.Value.DistanceTier);
            hash = MixLong(hash, (long)entry.Value.RiskExposure);
            hash = MixLong(hash, entry.Value.TravelTimeMonths);
            hash = MixLong(hash, entry.Value.MonthsElapsed);
            hash = MixLong(hash, entry.Value.DepartedDate.TotalMonths);
            hash = MixLong(hash, (long)entry.Value.Status);
            hash = MixLong(hash, entry.Value.EncounterCompleted ? 1L : 0L);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.Letters.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, (long)entry.Value.Direction);
            hash = MixLong(hash, (long)entry.Value.Action);
            hash = MixString(hash, entry.Value.SenderCharacterOrActorId);
            hash = MixString(hash, entry.Value.RecipientCharacterOrActorId);
            hash = MixLong(hash, entry.Value.DraftedByCharacterId?.Value ?? -1L);
            hash = MixLong(hash, entry.Value.SentDate.TotalMonths);
            hash = MixLong(hash, entry.Value.TransitTimeMonths);
            hash = MixLong(hash, entry.Value.MonthsElapsed);
            hash = MixLong(hash, entry.Value.RedirectionDelayMonths);
            hash = MixLong(hash, entry.Value.ArrivalDate?.TotalMonths ?? -1L);
            hash = MixLong(hash, (long)entry.Value.CourierType);
            hash = MixLong(hash, entry.Value.CourierCharacterId?.Value ?? -1L);
            hash = MixLong(hash, (long)entry.Value.InterceptionRisk);
            hash = MixLong(hash, entry.Value.Intercepted ? 1L : 0L);
            hash = MixLong(hash, entry.Value.Forged ? 1L : 0L);
            hash = MixLong(hash, entry.Value.Redirected ? 1L : 0L);
            hash = MixLong(hash, entry.Value.OralTraditionPenaltyApplied ? 1L : 0L);
            hash = MixLong(hash, entry.Value.RequiresResponse ? 1L : 0L);
            hash = MixLong(hash, entry.Value.Responded ? 1L : 0L);
            hash = MixLong(hash, entry.Value.ResponseAction.HasValue ? (long)entry.Value.ResponseAction.Value : -1L);
            hash = MixLong(hash, (long)entry.Value.Status);
            hash = MixLong(hash, (long)entry.Value.Outcome);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.DistantHoldings.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.HouseholdId.Value);
            hash = MixString(hash, entry.Value.HomeRegionId.Value);
            hash = MixString(hash, entry.Value.HoldingRegionId.Value);
            hash = MixLong(hash, entry.Value.HoldingId.Value);
            hash = MixLong(hash, (long)entry.Value.DistanceTier);
            hash = MixLong(hash, entry.Value.ProcuratorCharacterId?.Value ?? -1L);
            hash = MixLong(hash, entry.Value.MismanagementRiskActive ? 1L : 0L);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.LanguageProficiencies.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.CharacterId.Value);
            hash = MixString(hash, entry.Value.LanguageId.Value);
            hash = MixLong(hash, (long)entry.Value.FluencyTier);
            hash = MixLong(hash, (long)entry.Value.AcquisitionMethod);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.LiteracyRecords.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.IsLiterate ? 1L : 0L);
            hash = MixLong(hash, (long)entry.Value.DerivedFrom);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.CharacterHealthConditions.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.CharacterId.Value);
            hash = MixString(hash, entry.Value.ConditionId.Value);
            hash = MixLong(hash, (long)entry.Value.Category);
            hash = MixLong(hash, entry.Value.HasCure ? 1L : 0L);
            hash = MixLong(hash, entry.Value.Severity);
            hash = MixLong(hash, entry.Value.OnsetDate.TotalMonths);
            hash = MixLong(hash, (long)entry.Value.Status);
            hash = MixLong(hash, entry.Value.TreatedByPhysician ? 1L : 0L);
            hash = MixLong(hash, entry.Value.GrantedImmunity ? 1L : 0L);
            hash = MixLong(hash, entry.Value.ResolvedDate?.TotalMonths ?? -1L);
            hash = MixLong(hash, entry.Value.Quarantined ? 1L : 0L);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.SettlementSanitationInvestments.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, (long)entry.Value.Tier);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.EpidemicOutbreaks.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.SettlementId.Value);
            hash = MixString(hash, entry.Value.ConditionId.Value);
            hash = MixLong(hash, entry.Value.StartDate.TotalMonths);
            hash = MixLong(hash, (long)entry.Value.Status);
            hash = MixLong(hash, entry.Value.SettlementQuarantineActive ? 1L : 0L);
            hash = MixLong(hash, entry.Value.ImperialScale ? 1L : 0L);
            hash = MixLong(hash, entry.Value.ResolvedDate?.TotalMonths ?? -1L);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.DisasterEvents.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.SettlementId.Value);
            hash = MixLong(hash, entry.Value.OccurredDate.TotalMonths);
            hash = MixLong(hash, (long)entry.Value.HazardType);
            hash = MixLong(hash, (long)entry.Value.Severity);
            hash = MixLong(hash, entry.Value.TriggeredByCompounding ? 1L : 0L);
            hash = MixLong(hash, entry.Value.BuildingsDamaged);
            hash = MixLong(hash, entry.Value.PopulationLost);
            hash = MixLong(hash, entry.Value.PerennialCropSetback ? 1L : 0L);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.DormantVolcanoes.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.SettlementId.Value);
            hash = MixLong(hash, entry.Value.HasErupted ? 1L : 0L);
            hash = MixLong(hash, entry.Value.PostEruptionFertilityBoostActive ? 1L : 0L);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.Wanderers.InAscendingOrder())
        {
            var wanderer = entry.Value;
            hash = MixLong(hash, entry.Key.Value);
            hash = MixString(hash, wanderer.Name.Praenomen);
            hash = MixString(hash, wanderer.Name.Nomen);
            hash = MixString(hash, wanderer.Name.Cognomen ?? string.Empty);
            hash = MixLong(hash, (long)wanderer.Sex);
            hash = MixLong(hash, wanderer.BirthDate.TotalMonths);
            hash = MixLong(hash, (long)wanderer.LegalStatus);
            hash = MixString(hash, wanderer.Culture.Value);
            hash = MixLong(hash, (long)wanderer.Type);
            hash = MixString(hash, wanderer.CurrentLocationId.Value);
            foreach (var stop in wanderer.Itinerary)
            {
                hash = MixString(hash, stop.LocationId.Value);
                hash = MixLong(hash, stop.ArrivalMonth);
            }

            hash = MixLong(hash, wanderer.Fame);
            hash = MixLong(hash, (long)wanderer.FameTrend);
            hash = MixLong(hash, wanderer.IsActivelyTracked ? 1L : 0L);
            hash = MixLong(hash, (long)wanderer.Status);
            hash = MixLong(hash, wanderer.MonthsSinceLastEngagement);
            foreach (var householdId in wanderer.InterestedHouseholdIds)
                hash = MixLong(hash, householdId.Value);
            hash = MixLong(hash, wanderer.CommittedHouseholdId?.Value ?? -1L);
            hash = MixLong(hash, wanderer.RecruitedCharacterId?.Value ?? -1L);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.WandererEngagements.InAscendingOrder())
        {
            var engagement = entry.Value;
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, engagement.WandererId.Value);
            hash = MixLong(hash, engagement.HouseholdId.Value);
            hash = MixLong(hash, (long)engagement.EngagementType);
            hash = MixLong(hash, engagement.OccurredDate.TotalMonths);
            hash = MixLong(hash, engagement.FeePaid.RawValue);
            hash = MixLong(hash, engagement.DignitasGained);
            hash = MixLong(hash, engagement.WandererFameGained);
            hash = MixLong(hash, engagement.HealthRestored);
            hash = MixLong(hash, engagement.BeneficiaryCharacterId?.Value ?? -1L);
            hash = MixLong(hash, engagement.ResultingCharacterId?.Value ?? -1L);
            hash = MixLong(hash, engagement.ResultingDutySlot is { } slot ? (long)slot : -1L);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.InterpresAppointments.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.CharacterId.Value);
            foreach (var languageId in entry.Value.LanguagesCovered)
                hash = MixString(hash, languageId.Value);
        }

        // Already ascending-RuntimeId order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.DivergenceRecords.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Key.Value);
            hash = MixLong(hash, entry.Value.OccurredDate.TotalMonths);
            hash = MixLong(hash, entry.Value.TriggeringHouseholdId.Value);
            hash = MixString(hash, entry.Value.TriggeringAction);
            foreach (var entryId in entry.Value.AffectedTimelineEntryIds)
                hash = MixString(hash, entryId.Value);
            hash = MixLong(hash, entry.Value.NewAlternateHistoryBranchActive ? 1L : 0L);
        }

        // Already ascending string-key order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.FiredHistoricalTimelineEntryIds.InAscendingOrder())
        {
            hash = MixString(hash, entry.Key);
            hash = MixLong(hash, entry.Value.TotalMonths);
        }

        return hash;
    }

    /// <summary>Folds one <see cref="Interactions.Scheme"/>'s full state, in field-declaration order
    /// (Phase 10 item 6).</summary>
    private static ulong MixScheme(ulong hash, Scheme scheme)
    {
        hash = MixLong(hash, scheme.SchemeId.Value);
        hash = MixLong(hash, scheme.InitiatorCharacterId.Value);
        hash = MixLong(hash, scheme.TargetCharacterId.Value);
        hash = MixLong(hash, (long)scheme.Type);
        hash = MixLong(hash, (long)scheme.Status);
        hash = MixLong(hash, scheme.Progress);
        hash = MixLong(hash, scheme.DiscoveryRisk);
        hash = MixLong(hash, scheme.InitiatedDate.TotalMonths);
        hash = MixLong(hash, scheme.LastProgressedDate.TotalMonths);
        return hash;
    }

    /// <summary>Folds one <see cref="StewardshipAssignment"/>'s full state, in field-declaration order
    /// (Phase 10 item 2).</summary>
    private static ulong MixStewardshipAssignment(ulong hash, StewardshipAssignment assignment)
    {
        hash = MixLong(hash, assignment.AssignmentId.Value);
        hash = MixLong(hash, assignment.HouseholdId.Value);
        hash = MixLong(hash, (long)assignment.Context);
        hash = MixLong(hash, (long)assignment.Mode);
        hash = MixLong(hash, assignment.AppointeeCharacterId?.Value ?? -1L);
        foreach (var member in assignment.CouncilMembers)
        {
            hash = MixLong(hash, (long)member.Domain);
            hash = MixLong(hash, member.CharacterId.Value);
        }

        hash = MixLong(hash, assignment.CouncilHeadCharacterId?.Value ?? -1L);
        hash = MixLong(hash, (long)assignment.AutonomyLevel);
        hash = MixLong(hash, assignment.StartDate.TotalMonths);
        hash = MixLong(hash, assignment.EndDate?.TotalMonths ?? -1L);
        return hash;
    }

    /// <summary>Folds one <see cref="LivingWorldActor"/>'s full state, in field-declaration order
    /// (Phase 10 item 3).</summary>
    private static ulong MixLivingWorldActor(ulong hash, LivingWorldActor actor)
    {
        hash = MixLong(hash, actor.ActorId.Value);
        hash = MixLong(hash, (long)actor.ActorType);
        hash = MixString(hash, actor.Name);
        hash = MixLong(hash, (long)actor.Tier);
        hash = MixLong(hash, (long)actor.StandingTrend);
        hash = MixLong(hash, (long)actor.OriginStory);
        hash = MixLong(hash, actor.ParentActorId?.Value ?? -1L);
        hash = MixLong(hash, actor.IdentityTags.Economic is null ? -1L : (long)actor.IdentityTags.Economic.Value);
        hash = MixLong(hash, actor.IdentityTags.Faction is null ? -1L : (long)actor.IdentityTags.Faction.Value);
        hash = MixLong(hash, actor.HeadCharacterId?.Value ?? -1L);
        hash = MixLong(hash, actor.Dignitas);
        hash = MixLong(hash, (long)actor.NetWorth.Band);
        hash = MixLong(hash, actor.NetWorth.Figure is null ? 0 : 1);
        hash = MixLong(hash, actor.NetWorth.Figure?.RawValue ?? 0);
        hash = MixLong(hash, (long)actor.MilitaryStrength.Band);
        hash = MixString(hash, actor.MilitaryStrength.ResolvedForceId ?? string.Empty);
        hash = MixLong(hash, actor.RegionId.Value);
        hash = MixLong(hash, actor.HomeSettlementId.Value);
        hash = MixLong(hash, actor.LastContactDate?.TotalMonths ?? -1);
        return hash;
    }

    /// <summary>Folds one <see cref="BuildingInstance"/>'s full state, in field-declaration order.</summary>
    private static ulong MixBuilding(ulong hash, BuildingInstance building)
    {
        hash = MixLong(hash, building.Id.Value);
        hash = MixLong(hash, building.PlotId.Value);
        hash = MixBuildingDefinition(hash, building.Definition);
        hash = MixLong(hash, (long)building.Condition);
        // Already ordinal slot-ID order (ADR 0004) via BuildingInstance.Staff's SortedDictionary.
        foreach (var (slotId, workers) in building.Staff)
        {
            hash = MixString(hash, slotId);
            foreach (var workerId in workers)
                hash = MixString(hash, workerId);
        }

        return hash;
    }

    /// <summary>Folds one content-authored <see cref="BuildingDefinition"/> — embedded inline on every
    /// <see cref="BuildingInstance"/>/<see cref="Buildings.ConstructionProject"/> rather than
    /// referenced by ID, matching <see cref="Saves.WorldStateMapper"/>'s identical reasoning.</summary>
    private static ulong MixBuildingDefinition(ulong hash, BuildingDefinition definition)
    {
        hash = MixString(hash, definition.Id.Value);
        hash = MixLong(hash, (long)definition.Tier);
        hash = MixLong(hash, definition.ConstructionMonths);
        hash = MixLong(hash, definition.PlotCapacity);
        foreach (var prerequisite in definition.Prerequisites)
            hash = MixString(hash, prerequisite.Value);
        foreach (var terrain in definition.AllowedTerrain)
            hash = MixLong(hash, (long)terrain);
        hash = MixLong(hash, (long)definition.RequiredFeatures);
        foreach (var line in definition.Upkeep)
        {
            hash = MixString(hash, line.GoodId.Value);
            hash = MixLong(hash, line.Quantity);
        }

        foreach (var slot in definition.StaffingSlots)
        {
            hash = MixString(hash, slot.Id);
            hash = MixLong(hash, slot.Capacity);
            hash = MixLong(hash, slot.RequiredForProduction ? 1 : 0);
        }

        hash = MixLong(hash, definition.Recipe is null ? 0 : 1);
        if (definition.Recipe is not null)
        {
            foreach (var line in definition.Recipe.Inputs)
            {
                hash = MixString(hash, line.GoodId.Value);
                hash = MixLong(hash, line.Quantity);
            }

            foreach (var line in definition.Recipe.Outputs)
            {
                hash = MixString(hash, line.GoodId.Value);
                hash = MixLong(hash, line.Quantity);
            }
        }

        return hash;
    }

    /// <summary>Folds one Holding's <see cref="Stockpile"/> — lots in original list order (equal-age
    /// tie-break order per that type's own doc comment), reservations in ordinal ID order.</summary>
    private static ulong MixStockpile(ulong hash, Stockpile stockpile)
    {
        hash = MixLong(hash, stockpile.Capacity);
        foreach (var lot in stockpile.Lots)
        {
            hash = MixString(hash, lot.Good.Id.Value);
            hash = MixLong(hash, (long)lot.Good.Perishability);
            hash = MixLong(hash, lot.Good.QualityEligible ? 1 : 0);
            hash = MixLong(hash, lot.Good.ConditionTracked ? 1 : 0);
            hash = MixLong(hash, lot.Good.ShelfLifeTicks ?? -1);
            hash = MixLong(hash, lot.Quantity);
            hash = MixLong(hash, lot.Quality is null ? -1L : (long)lot.Quality.Value);
            hash = MixLong(hash, lot.Condition?.Value ?? -1);
            hash = MixLong(hash, lot.AgeInTicks);
            hash = MixString(hash, lot.Provenance?.SourceId ?? string.Empty);
            hash = MixString(hash, lot.Provenance?.EventId ?? string.Empty);
            hash = MixString(hash, lot.Provenance?.ExceptionalObjectId ?? string.Empty);
        }

        foreach (var reservation in stockpile.Reservations)
        {
            hash = MixString(hash, reservation.ReservationId);
            hash = MixString(hash, reservation.GoodId.Value);
            hash = MixLong(hash, reservation.Quality is null ? -1L : (long)reservation.Quality.Value);
            hash = MixLong(hash, reservation.Quantity);
        }

        return hash;
    }

    /// <summary>Folds every <see cref="Character"/> field (Phase 5 items 1-2) into the hash, in the
    /// record's declared field order, so a divergent Character anywhere flips the campaign hash.</summary>
    private static ulong MixCharacter(ulong hash, Character character)
    {
        hash = MixLong(hash, character.Id.Value);
        hash = MixString(hash, character.Praenomen);
        hash = MixString(hash, character.Nomen);
        hash = MixString(hash, character.Cognomen ?? string.Empty);
        hash = MixLong(hash, (long)character.Sex);
        hash = MixLong(hash, character.BirthDate.TotalMonths);
        hash = MixLong(hash, (long)character.VisualProfile.Height);
        hash = MixLong(hash, (long)character.VisualProfile.Build);
        hash = MixLong(hash, (long)character.VisualProfile.FacialStructure);
        hash = MixLong(hash, (long)character.VisualProfile.Complexion);
        hash = MixLong(hash, (long)character.VisualProfile.HairColor);
        hash = MixLong(hash, (long)character.VisualProfile.HairStyle);
        hash = MixLong(hash, (long)character.VisualProfile.EyeColor);
        foreach (var feature in character.VisualProfile.NotableFeatures)
            hash = MixLong(hash, (long)feature);
        foreach (var layer in character.VisualProfile.Portrait.Layers)
            hash = MixString(hash, layer);
        hash = MixLong(hash, (long)character.LegalStatus);
        hash = MixLong(hash, character.SocialClass is null ? -1L : (long)character.SocialClass.Value);
        hash = MixString(hash, character.Culture.Value);
        hash = MixLong(hash, character.Location.Value);
        hash = MixLong(hash, character.Household is null ? -1L : character.Household.Value.Value);
        hash = MixLong(hash, character.Attributes.Diplomacy);
        hash = MixLong(hash, character.Attributes.Martial);
        hash = MixLong(hash, character.Attributes.Stewardship);
        hash = MixLong(hash, character.Attributes.Intrigue);
        hash = MixLong(hash, character.Attributes.Learning);
        hash = MixLong(hash, character.Skills.Fieldwork);
        hash = MixLong(hash, character.Skills.DomesticService);
        hash = MixLong(hash, character.Skills.Craft);
        hash = MixLong(hash, character.Skills.Culinary);
        hash = MixLong(hash, character.Skills.Medicine);
        hash = MixLong(hash, character.Condition.Health);
        hash = MixLong(hash, character.Condition.Fatigue);
        hash = MixLong(hash, character.Condition.Loyalty);
        hash = MixLong(hash, character.Condition.Ambition);
        hash = MixLong(hash, character.Condition.Fertility);
        hash = MixLong(hash, (long)character.Source);
        hash = MixLong(hash, character.InstantiatedAtMonth);
        hash = MixLong(hash, character.BackfilledHistory ? 1L : 0L);
        hash = MixLong(hash, character.MotherId is null ? -1L : character.MotherId.Value.Value);
        hash = MixLong(hash, character.FatherId is null ? -1L : character.FatherId.Value.Value);
        hash = MixLong(hash, (long)character.Legitimacy);
        foreach (var marriage in character.MaritalHistory)
        {
            hash = MixLong(hash, marriage.SpouseId.Value);
            hash = MixLong(hash, marriage.StartDate.TotalMonths);
            hash = MixLong(hash, marriage.EndDate is null ? -1L : marriage.EndDate.Value.TotalMonths);
            hash = MixLong(hash, marriage.EndReason is null ? -1L : (long)marriage.EndReason.Value);
        }

        foreach (var injury in character.PermanentInjuries)
        {
            hash = MixLong(hash, (long)injury.Target);
            hash = MixLong(hash, injury.Magnitude);
            hash = MixString(hash, injury.Cause);
            hash = MixLong(hash, injury.InflictedDate.TotalMonths);
        }

        foreach (var trait in character.Traits)
            hash = MixString(hash, trait.Value);

        hash = MixLong(hash, character.DeathRecord is null ? -1L : character.DeathRecord.Value.Date.TotalMonths);
        hash = MixLong(hash, character.DeathRecord is null ? -1L : (long)character.DeathRecord.Value.Cause);
        hash = MixLong(hash, character.DeathRecord is null ? -1L : character.DeathRecord.Value.AgeAtDeath);
        hash = MixString(hash, character.DeathRecord?.ConditionId?.Value ?? string.Empty);
        hash = MixLong(hash, character.Duty is null ? -1L : character.Duty.Value.HouseholdId.Value);
        hash = MixLong(hash, character.Duty is null ? -1L : (long)character.Duty.Value.Slot);
        hash = MixLong(hash, character.Duty is null ? -1L : character.Duty.Value.AssignedDate.TotalMonths);

        hash = MixLong(hash, character.Regimen is null ? -1L : (long)character.Regimen.Value.Diet);
        hash = MixLong(hash, character.Regimen is null ? -1L : (long)character.Regimen.Value.Accommodation);
        hash = MixLong(hash, character.Regimen is null ? -1L : (long)character.Regimen.Value.Freedoms);
        hash = MixLong(hash, character.Regimen is null ? -1L : (long)character.Regimen.Value.Discipline);

        hash = MixLong(hash, character.Flight is null ? -1L : character.Flight.Value.FledDate.TotalMonths);
        hash = MixLong(hash, character.Flight is null ? -1L : character.Flight.Value.FormerHousehold.Value);
        hash = MixLong(hash, character.Flight?.LastKnownLocation is { } lastKnownLocation ? lastKnownLocation.Value : -1L);

        hash = MixLong(hash, character.Pursuit is null ? -1L : character.Pursuit.Value.MonthsRemaining);
        hash = MixLong(hash, character.Pursuit?.PursuerId is { } pursuerId ? pursuerId.Value : -1L);

        hash = MixLong(hash, character.ManumissionPlan is null ? -1L : character.ManumissionPlan.Value.GrantorId.Value);
        hash = MixLong(hash, character.ManumissionPlan is null ? -1L : (long)character.ManumissionPlan.Value.Type);

        hash = MixTravelLocation(hash, character.CurrentTravelLocation);

        return hash;
    }

    private static ulong MixTravelLocation(ulong hash, TravelLocation? location)
    {
        if (location is not { } value)
            return MixLong(hash, -1L);

        hash = MixLong(hash, (long)value.Kind);
        hash = MixString(hash, value.RegionId?.Value ?? string.Empty);
        hash = MixLong(hash, value.SettlementId?.Value ?? -1L);
        hash = MixLong(hash, value.ActorId?.Value ?? -1L);
        return hash;
    }

    private static ulong MixLong(ulong hash, long value)
    {
        foreach (var b in BitConverter.GetBytes(value))
            hash = unchecked((hash ^ b) * Prime);
        return hash;
    }

    private static ulong MixString(ulong hash, string value)
    {
        foreach (var b in Encoding.UTF8.GetBytes(value))
            hash = unchecked((hash ^ b) * Prime);

        // A length/terminator mix so ("ab","c") and ("a","bc") fold to different hashes.
        return unchecked((hash ^ 0xFF) * Prime);
    }
}
