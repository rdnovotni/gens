using System.Text;
using System.Text.Json;
using Gens.Simulation.Actors;
using Gens.Simulation.Buildings;
using Gens.Simulation.Characters;
using Gens.Simulation.Goods;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Markets;
using Gens.Simulation.Stewardship;

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
            foreach (var chronicleEntry in entry.Value.RecentChronicleEntries)
                hash = MixString(hash, chronicleEntry);
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
