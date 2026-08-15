using System.Text;
using System.Text.Json;
using Gens.Simulation.Characters;
using Gens.Simulation.Land;

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
