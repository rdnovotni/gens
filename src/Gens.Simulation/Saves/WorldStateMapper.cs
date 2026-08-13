using System.Text.Json;
using Gens.Simulation.Characters;
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
                ScheduledActionIds = state.ScheduledActionIds.Peek,
            },
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            CharacterIds = state.Characters.InAscendingOrder().Select(entry => entry.Key.ToTaggedString()).ToArray(),
            Characters = state.Characters.InAscendingOrder().Select(entry => ToCharacterDto(entry.Value)).ToArray(),
            // Already deterministic key order (ADR 0004) via KnowledgeState.All.
            Knowledge = state.Knowledge.All().Select(ToKnowledgeDto).ToArray(),
            // Already ascending (due date, action ID) order (ADR 0004) via OrderedRegistry.InAscendingOrder.
            ScheduledActions = state.ScheduledActions.InAscendingOrder().Select(entry => ToScheduledActionDto(entry.Value)).ToArray(),
            // Already ascending (From, To) order (ADR 0004) via OrderedRegistry.InAscendingOrder.
            Relationships = state.Relationships.InAscendingOrder().Select(entry => ToRelationshipDto(entry.Key, entry.Value)).ToArray(),
            // Already ascending (settlement, group type) order (ADR 0004) via OrderedRegistry.InAscendingOrder.
            PopGroups = state.PopGroups.InAscendingOrder().Select(entry => ToPopGroupDto(entry.Value)).ToArray(),
        };
    }

    public static WorldState ToWorldState(WorldSaveDocument dto)
    {
        if (dto is null)
            throw new ArgumentNullException(nameof(dto));

        var characters = OrderedRegistry<RuntimeId<Character>, Character>.Restore(
            dto.Characters.Select(characterDto =>
            {
                var character = FromCharacterDto(characterDto);
                return new KeyValuePair<RuntimeId<Character>, Character>(character.Id, character);
            }));

        var knowledge = KnowledgeState.Restore(dto.Knowledge.Select(FromKnowledgeDto));

        var scheduledActions = OrderedRegistry<ScheduledActionKey, ScheduledActionEntry>.Restore(
            dto.ScheduledActions.Select(FromScheduledActionDto));

        var relationships = OrderedRegistry<RelationshipKey, Relationship>.Restore(
            dto.Relationships.Select(FromRelationshipDto));

        var popGroups = OrderedRegistry<PopGroupKey, PopGroup>.Restore(
            dto.PopGroups.Select(FromPopGroupDto));

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
            scheduledActionIds: RuntimeIdCounter<ScheduledAction>.Restore(dto.Counters.ScheduledActionIds),
            characters: characters,
            relationships: relationships,
            scheduledActions: scheduledActions,
            popGroups: popGroups,
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

    private static CharacterDto ToCharacterDto(Character character) => new()
    {
        Id = character.Id.ToTaggedString(),
        Praenomen = character.Praenomen,
        Nomen = character.Nomen,
        Cognomen = character.Cognomen,
        Sex = character.Sex.ToString(),
        BirthDateTotalMonths = character.BirthDate.TotalMonths,
        VisualProfile = ToVisualProfileDto(character.VisualProfile),
        LegalStatus = character.LegalStatus.ToString(),
        SocialClass = character.SocialClass?.ToString(),
        Culture = character.Culture.Value,
        Location = character.Location.ToTaggedString(),
        Household = character.Household?.ToTaggedString(),
        Attributes = new CoreAttributesDto
        {
            Diplomacy = character.Attributes.Diplomacy,
            Martial = character.Attributes.Martial,
            Stewardship = character.Attributes.Stewardship,
            Intrigue = character.Attributes.Intrigue,
            Learning = character.Attributes.Learning,
        },
        Skills = new LaborSkillsDto
        {
            Fieldwork = character.Skills.Fieldwork,
            DomesticService = character.Skills.DomesticService,
            Craft = character.Skills.Craft,
            Culinary = character.Skills.Culinary,
            Medicine = character.Skills.Medicine,
        },
        Condition = new ConditionDto
        {
            Health = character.Condition.Health,
            Fatigue = character.Condition.Fatigue,
            Loyalty = character.Condition.Loyalty,
            Ambition = character.Condition.Ambition,
            Fertility = character.Condition.Fertility,
        },
        Source = character.Source.ToString(),
        InstantiatedAtMonth = character.InstantiatedAtMonth,
        MotherId = character.MotherId?.ToTaggedString(),
        FatherId = character.FatherId?.ToTaggedString(),
        Legitimacy = character.Legitimacy.ToString(),
        MaritalHistory = character.MaritalHistory.Select(ToMarriageRecordDto).ToArray(),
        PermanentInjuries = character.PermanentInjuries.Select(ToPermanentInjuryDto).ToArray(),
        DeathRecord = character.DeathRecord is null ? null : ToDeathRecordDto(character.DeathRecord.Value),
        Traits = character.Traits.Select(static trait => trait.Value).ToArray(),
        Duty = character.Duty is null ? null : ToDutyAssignmentDto(character.Duty.Value),
        BackfilledHistory = character.BackfilledHistory,
    };

    private static DutyAssignmentDto ToDutyAssignmentDto(DutyAssignment duty) => new()
    {
        HouseholdId = duty.HouseholdId.ToTaggedString(),
        Slot = duty.Slot.ToString(),
        AssignedDateTotalMonths = duty.AssignedDate.TotalMonths,
    };

    private static DutyAssignment FromDutyAssignmentDto(DutyAssignmentDto dto) => new(
        RuntimeId<Household>.Parse(dto.HouseholdId),
        Enum.Parse<DutySlot>(dto.Slot),
        new GameDate(dto.AssignedDateTotalMonths));

    private static Character FromCharacterDto(CharacterDto dto) => Character.Create(
        id: RuntimeId<Character>.Parse(dto.Id),
        praenomen: dto.Praenomen,
        nomen: dto.Nomen,
        cognomen: dto.Cognomen,
        sex: Enum.Parse<Sex>(dto.Sex),
        birthDate: new GameDate(dto.BirthDateTotalMonths),
        visualProfile: FromVisualProfileDto(dto.VisualProfile),
        status: Enum.Parse<LegalStatus>(dto.LegalStatus),
        socialClass: dto.SocialClass is null ? null : Enum.Parse<SocialClass>(dto.SocialClass),
        culture: new DefinitionId<Culture>(dto.Culture),
        location: RuntimeId<Settlement>.Parse(dto.Location),
        household: dto.Household is null ? null : RuntimeId<Household>.Parse(dto.Household),
        attributes: new CoreAttributes(
            dto.Attributes.Diplomacy, dto.Attributes.Martial, dto.Attributes.Stewardship,
            dto.Attributes.Intrigue, dto.Attributes.Learning),
        skills: new LaborSkills(
            dto.Skills.Fieldwork, dto.Skills.DomesticService, dto.Skills.Craft,
            dto.Skills.Culinary, dto.Skills.Medicine),
        condition: new Condition(
            dto.Condition.Health, dto.Condition.Fatigue, dto.Condition.Loyalty,
            dto.Condition.Ambition, dto.Condition.Fertility),
        source: Enum.Parse<CharacterSource>(dto.Source),
        instantiatedAtMonth: dto.InstantiatedAtMonth,
        backfilledHistory: dto.BackfilledHistory,
        motherId: dto.MotherId is null ? null : RuntimeId<Character>.Parse(dto.MotherId),
        fatherId: dto.FatherId is null ? null : RuntimeId<Character>.Parse(dto.FatherId),
        legitimacy: Enum.Parse<Legitimacy>(dto.Legitimacy),
        maritalHistory: dto.MaritalHistory.Select(FromMarriageRecordDto).ToArray(),
        permanentInjuries: dto.PermanentInjuries.Select(FromPermanentInjuryDto).ToArray(),
        traits: dto.Traits.Select(static trait => new DefinitionId<Trait>(trait)).ToArray(),
        deathRecord: dto.DeathRecord is null ? null : FromDeathRecordDto(dto.DeathRecord),
        duty: dto.Duty is null ? null : FromDutyAssignmentDto(dto.Duty));

    private static MarriageRecordDto ToMarriageRecordDto(MarriageRecord record) => new()
    {
        SpouseId = record.SpouseId.ToTaggedString(),
        StartDateTotalMonths = record.StartDate.TotalMonths,
        EndDateTotalMonths = record.EndDate?.TotalMonths,
        EndReason = record.EndReason?.ToString(),
    };

    private static MarriageRecord FromMarriageRecordDto(MarriageRecordDto dto) => new(
        RuntimeId<Character>.Parse(dto.SpouseId),
        new GameDate(dto.StartDateTotalMonths),
        dto.EndDateTotalMonths is null ? null : new GameDate(dto.EndDateTotalMonths.Value),
        dto.EndReason is null ? null : Enum.Parse<MarriageEndReason>(dto.EndReason));

    private static PermanentInjuryDto ToPermanentInjuryDto(PermanentInjury injury) => new()
    {
        Target = injury.Target.ToString(),
        Magnitude = injury.Magnitude,
        Cause = injury.Cause,
        InflictedDateTotalMonths = injury.InflictedDate.TotalMonths,
    };

    private static PermanentInjury FromPermanentInjuryDto(PermanentInjuryDto dto) => new(
        Enum.Parse<PermanentInjuryTarget>(dto.Target),
        dto.Magnitude,
        dto.Cause,
        new GameDate(dto.InflictedDateTotalMonths));

    private static DeathRecordDto ToDeathRecordDto(DeathRecord record) => new()
    {
        DateTotalMonths = record.Date.TotalMonths,
        Cause = record.Cause.ToString(),
        AgeAtDeath = record.AgeAtDeath,
    };

    private static DeathRecord FromDeathRecordDto(DeathRecordDto dto) => new(
        new GameDate(dto.DateTotalMonths),
        Enum.Parse<DeathCause>(dto.Cause),
        dto.AgeAtDeath);

    private static CharacterVisualProfileDto ToVisualProfileDto(CharacterVisualProfile profile) => new()
    {
        Height = profile.Height.ToString(),
        Build = profile.Build.ToString(),
        FacialStructure = profile.FacialStructure.ToString(),
        Complexion = profile.Complexion.ToString(),
        HairColor = profile.HairColor.ToString(),
        HairStyle = profile.HairStyle.ToString(),
        EyeColor = profile.EyeColor.ToString(),
        NotableFeatures = profile.NotableFeatures.Select(static feature => feature.ToString()).ToArray(),
        Portrait = new PortraitRecipeDto { Layers = profile.Portrait.Layers.ToArray() },
    };

    private static CharacterVisualProfile FromVisualProfileDto(CharacterVisualProfileDto dto) => new()
    {
        Height = Enum.Parse<Height>(dto.Height),
        Build = Enum.Parse<Build>(dto.Build),
        FacialStructure = Enum.Parse<FacialStructure>(dto.FacialStructure),
        Complexion = Enum.Parse<Complexion>(dto.Complexion),
        HairColor = Enum.Parse<HairColor>(dto.HairColor),
        HairStyle = Enum.Parse<HairStyle>(dto.HairStyle),
        EyeColor = Enum.Parse<EyeColor>(dto.EyeColor),
        NotableFeatures = dto.NotableFeatures.Select(static feature => Enum.Parse<NotableFeature>(feature)).ToArray(),
        Portrait = new PortraitRecipe { Layers = dto.Portrait.Layers.ToArray() },
    };

    private static ScheduledActionEntryDto ToScheduledActionDto(ScheduledActionEntry entry) => new()
    {
        ActionId = entry.ActionId.ToTaggedString(),
        DueDateTotalMonths = entry.DueDate.TotalMonths,
        ActorId = entry.ActorId,
        ActionType = entry.ActionType,
        PayloadJson = entry.PayloadJson,
        CausationId = entry.CausationId,
    };

    private static KeyValuePair<ScheduledActionKey, ScheduledActionEntry> FromScheduledActionDto(ScheduledActionEntryDto dto)
    {
        var actionId = RuntimeId<ScheduledAction>.Parse(dto.ActionId);
        var dueDate = new GameDate(dto.DueDateTotalMonths);
        var entry = new ScheduledActionEntry(actionId, dueDate, dto.ActorId, dto.ActionType, dto.PayloadJson, dto.CausationId);
        return new KeyValuePair<ScheduledActionKey, ScheduledActionEntry>(new ScheduledActionKey(dueDate, actionId), entry);
    }

    /// <summary>Every <see cref="BondTag"/> flag, in a fixed, explicit, hand-listed order rather than
    /// <c>Enum.GetValues</c> — ADR 0004's "never rely on reflection discovery order" applies just as
    /// much to an enum's runtime value order as to any other collection (matching <see
    /// cref="CharacterVisualProfileGenerator"/>'s identical convention).</summary>
    private static readonly BondTag[] AllBondTags =
    {
        BondTag.Parent, BondTag.Child, BondTag.Sibling, BondTag.Spouse,
        BondTag.Friend, BondTag.Rival, BondTag.Lover, BondTag.Mentor, BondTag.Student,
        BondTag.Contubernium, BondTag.Nemesis, BondTag.Patron, BondTag.Client,
        BondTag.Debtor, BondTag.Creditor, BondTag.CoMagistrate, BondTag.BlackmailLeverage,
    };

    /// <summary>Every individual flag set in <paramref name="bonds"/>, by name — stored as a string
    /// list rather than the raw numeric bitmask so a save file stays human-readable and stable across
    /// a future <see cref="BondTag"/> reordering, matching how <see cref="Character.Traits"/> is
    /// stored as definition-ID strings rather than an opaque encoding.</summary>
    private static string[] ToBondTagDto(BondTag bonds) =>
        AllBondTags.Where(tag => bonds.HasFlag(tag)).Select(tag => tag.ToString()).ToArray();

    private static BondTag FromBondTagDto(IReadOnlyList<string> bonds)
    {
        var result = BondTag.None;
        foreach (var bond in bonds)
            result |= Enum.Parse<BondTag>(bond);
        return result;
    }

    private static RelationshipDto ToRelationshipDto(RelationshipKey key, Relationship relationship) => new()
    {
        From = key.From.ToTaggedString(),
        To = key.To.ToTaggedString(),
        Opinion = relationship.Opinion,
        Bonds = ToBondTagDto(relationship.Bonds),
        Origin = relationship.Origin.ToString(),
        FormedDateTotalMonths = relationship.FormedDate.TotalMonths,
        LastMeaningfulInteractionDateTotalMonths = relationship.LastMeaningfulInteractionDate.TotalMonths,
        ProvenanceEventId = relationship.ProvenanceEventId,
    };

    private static KeyValuePair<RelationshipKey, Relationship> FromRelationshipDto(RelationshipDto dto)
    {
        var key = new RelationshipKey(RuntimeId<Character>.Parse(dto.From), RuntimeId<Character>.Parse(dto.To));
        var relationship = new Relationship(
            dto.Opinion,
            FromBondTagDto(dto.Bonds),
            Enum.Parse<RelationshipOrigin>(dto.Origin),
            new GameDate(dto.FormedDateTotalMonths),
            new GameDate(dto.LastMeaningfulInteractionDateTotalMonths),
            dto.ProvenanceEventId);
        return new KeyValuePair<RelationshipKey, Relationship>(key, relationship);
    }

    private static PopGroupDto ToPopGroupDto(PopGroup popGroup) => new()
    {
        SettlementId = popGroup.SettlementId.ToTaggedString(),
        GroupType = popGroup.GroupType.ToString(),
        Size = popGroup.Size,
    };

    private static KeyValuePair<PopGroupKey, PopGroup> FromPopGroupDto(PopGroupDto dto)
    {
        var settlementId = RuntimeId<Settlement>.Parse(dto.SettlementId);
        var groupType = Enum.Parse<PopGroupType>(dto.GroupType);
        var popGroup = PopGroup.Create(settlementId, groupType, dto.Size);
        return new KeyValuePair<PopGroupKey, PopGroup>(new PopGroupKey(settlementId, groupType), popGroup);
    }
}
