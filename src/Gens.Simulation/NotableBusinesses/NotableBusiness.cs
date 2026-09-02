using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.RealEstate;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Time;
using Gens.Simulation.Wanderers;

namespace Gens.Simulation.NotableBusinesses;

/// <summary>§3's "explicit background/notable sampling plus triggered promotion" — where a tracked
/// Notable Business currently stands relative to the (unstored, uncounted) aggregate commerce pool it
/// was sampled from, matching <see cref="Wanderers.WandererStatus"/>'s and <see
/// cref="Actors.LivingWorldActorTier"/>'s identical two-state shape. <see cref="Demoted"/> mirrors <see
/// cref="Actors.LivingWorldActorTieringService.DemoteIfQuiet"/>'s own "freeze back... nothing about a
/// frozen entry is deleted, only no longer given extra simulation fidelity going forward" — a Demoted
/// business's own record is kept (for Chronicle/audit purposes and to let <see
/// cref="NotableBusinessTieringService.RecordContactAndPromote"/> re-promote it later without losing
/// its history) but no longer advances through <see cref="AdjustBusinessReputationCommand"/>,
/// <see cref="SupplierDisruptionSystem"/>, or <see cref="GovernmentContractPaymentSystem"/>.</summary>
public enum NotableBusinessStatus
{
    Tracked,
    Demoted,
}

/// <summary>§3's/§9's <c>sampledOrTriggeredBy</c> — every real trigger §3 names for promoting one
/// member of the aggregate commerce pool into a real, individually-tracked <see
/// cref="NotableBusiness"/>.</summary>
public enum NotableBusinessTrigger
{
    /// <summary>§3's own catch-all ambient promotion, not tied to one specific named trigger below.</summary>
    AmbientSample,

    /// <summary>§3's "its owner is already a Notable Household of note" — this codebase's own real
    /// stand-in, since no <c>Notable Households</c> domain exists yet (confirmed by direct search; only
    /// its own design doc does), is an owner already carrying a real <see
    /// cref="MerchantFamilies.MerchantHouseArchetype"/> or a <see
    /// cref="Actors.LivingWorldActorTier.Noteworthy"/> Rival Gens — see <see
    /// cref="PromoteNotableBusinessCommand"/>'s own doc comment for how this item narrows §2's
    /// "Notable Household head" framing to the owner kinds this codebase can actually resolve.</summary>
    OwnerAlreadyNotable,

    GovernmentContract,
    LegalOrScandalCase,
    DirectPlayerTransaction,
}

/// <summary>§6's "a specific named household, a Wandering Merchant..., or a... Property Record
/// producing that exact good" — every real supplier shape this item can point at.</summary>
public enum NotableBusinessSupplierKind
{
    Household,
    Character,
    PropertyRecord,
    Wanderer,
}

/// <summary>
/// §6's Named Supplier pointer — a tagged reference, kept as a plain <see cref="Kind"/> + string pair
/// rather than a single narrow <c>RuntimeId&lt;T&gt;</c>, matching <see cref="PropertyOwnerRef"/>'s own
/// identical reasoning: a supplier can be a Household, a Character, a <see cref="PropertyRecord"/>, or
/// a <see cref="Wanderer"/>, and no single phantom type could name them all.
/// </summary>
public readonly record struct NotableBusinessSupplierRef(NotableBusinessSupplierKind Kind, string RefId)
{
    public static NotableBusinessSupplierRef ForHousehold(RuntimeId<Household> householdId) =>
        new(NotableBusinessSupplierKind.Household, householdId.ToTaggedString());

    public static NotableBusinessSupplierRef ForCharacter(RuntimeId<Character> characterId) =>
        new(NotableBusinessSupplierKind.Character, characterId.ToTaggedString());

    public static NotableBusinessSupplierRef ForPropertyRecord(RuntimeId<PropertyRecord> propertyRecordId) =>
        new(NotableBusinessSupplierKind.PropertyRecord, propertyRecordId.ToTaggedString());

    public static NotableBusinessSupplierRef ForWanderer(RuntimeId<Wanderer> wandererId) =>
        new(NotableBusinessSupplierKind.Wanderer, wandererId.ToTaggedString());
}

/// <summary>
/// §2's Notable Business Record and §10's <c>NotableBusiness</c> data model (Phase 15 item 4;
/// <c>gens-notable-businesses-design.md</c>). §2's own worked example ("Bakery of Marcus Livius") names
/// every field's real source; this item follows that mapping directly — see each field's own doc
/// comment for its citation. <see cref="DistrictId"/> is this item's own invented addition beyond §10's
/// bare sketch, needed to give §8's Move a real "where the business currently sits" pointer to update
/// (the sketch's own <c>newDistrictId</c> field on <c>BusinessLifecycleEvent</c> presumes one exists
/// somewhere to move <i>from</i>).
/// </summary>
public sealed record NotableBusiness
{
    private NotableBusiness()
    {
    }

    public required RuntimeId<NotableBusiness> Id { get; init; }
    public required string Name { get; init; }

    /// <summary>§2's "Owner: Marcus Livius (a Notable Household head or full Character)." No <c>Notable
    /// Households</c> domain exists anywhere in this codebase (confirmed by direct search — only its own
    /// design doc does), so this item reuses <see cref="PropertyOwnerRef"/> directly, matching <see
    /// cref="MerchantFamilies.MerchantHouseArchetype"/>'s own identical reuse, and restricts it to the
    /// three owner kinds actually meaningful for "a household head or full Character" — <see
    /// cref="PropertyOwnerKind.PlayerHousehold"/>, <see cref="PropertyOwnerKind.RivalGens"/> (a household
    /// head is resolved, where needed, through <see cref="HouseholdHeadship"/>/<see
    /// cref="LivingWorldActor.HeadCharacterId"/> — see <see cref="NotableBusinessOwnerResolver"/>), and
    /// <see cref="PropertyOwnerKind.IndividualCharacter"/> directly. See <see
    /// cref="PromoteNotableBusinessCommand"/> for the validation that enforces this.</summary>
    public required PropertyOwnerRef Owner { get; init; }

    public required NotableBusinessTrigger SampledOrTriggeredBy { get; init; }
    public required NotableBusinessStatus Status { get; init; }

    /// <summary>The last date this business was genuinely relevant — reset by <see
    /// cref="NotableBusinessTieringService.RecordContactAndPromote"/>, read by <see
    /// cref="NotableBusinessTieringService.DemoteIfQuiet"/>, mirroring <see
    /// cref="LivingWorldActor.LastContactDate"/>'s identical role.</summary>
    public required GameDate LastRelevantContactDate { get; init; }

    /// <summary>§4's Reputation — 0-100, distinct from the owner's own Fame/Dignitas.</summary>
    public required int Reputation { get; init; }

    /// <summary>§2's "Property: workshop-type Property Record, [District]" — non-null only for the
    /// subset of Notable Businesses actually linked to a tracked <see cref="RealEstate.PropertyRecord"/>
    /// (most ordinary businesses run out of an untracked Plot, per that record's own doc comment on why
    /// most Estate &amp; Settlement property never gets a <see cref="RealEstate.PropertyRecord"/> at
    /// all).</summary>
    public RuntimeId<PropertyRecord>? LinkedPropertyRecordId { get; init; }

    /// <summary>Where this business currently operates — this item's own invented field; see this
    /// record's own doc comment for why. Updated by <see cref="MoveNotableBusinessCommand"/>
    /// (§8).</summary>
    public RuntimeId<District>? DistrictId { get; init; }

    /// <summary>§2's "Output: bread (Resources &amp; Goods' existing production chain)."</summary>
    public DefinitionId<Good>? OutputGoodId { get; init; }

    /// <summary>§2's/§5's "Main Competitor: Bakery of Gaius." Set/cleared by <see
    /// cref="SetMainCompetitorCommand"/>.</summary>
    public RuntimeId<NotableBusiness>? MainCompetitorBusinessId { get; init; }

    /// <summary>§2's/§6's "Main Supplier: [named grain-trading household]." Set/cleared by <see
    /// cref="SetMainSupplierCommand"/>.</summary>
    public NotableBusinessSupplierRef? MainSupplier { get; init; }

    /// <summary>§6's "a supplier's own bad harvest, bankruptcy... genuinely disrupts the dependent
    /// business's own Output" — this item's own invented one-shot-per-disruption-bout guard so <see
    /// cref="SupplierDisruptionSystem"/> does not re-penalize the same ongoing Insolvency every single
    /// month. Reset to <c>false</c> whenever <see cref="SetMainSupplierCommand"/> changes <see
    /// cref="MainSupplier"/>.</summary>
    public bool SupplierDisruptionApplied { get; init; }

    /// <summary>§2's/§8's Specialize — "narrowing its own Output to a single high-quality good."</summary>
    public bool IsSpecialized { get; init; }

    public DefinitionId<Good>? SpecializedGoodId { get; init; }

    public static NotableBusiness Create(
        RuntimeId<NotableBusiness> id,
        string name,
        PropertyOwnerRef owner,
        NotableBusinessTrigger trigger,
        GameDate promotedDate,
        DefinitionId<Good>? outputGoodId,
        RuntimeId<PropertyRecord>? linkedPropertyRecordId,
        RuntimeId<District>? districtId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A Notable Business requires a non-empty name.", nameof(name));

        return new NotableBusiness
        {
            Id = id,
            Name = name,
            Owner = owner,
            SampledOrTriggeredBy = trigger,
            Status = NotableBusinessStatus.Tracked,
            LastRelevantContactDate = promotedDate,
            Reputation = NotableBusinessesCatalog.DefaultReputation,
            OutputGoodId = outputGoodId,
            LinkedPropertyRecordId = linkedPropertyRecordId,
            DistrictId = districtId,
            MainCompetitorBusinessId = null,
            MainSupplier = null,
            SupplierDisruptionApplied = false,
            IsSpecialized = false,
            SpecializedGoodId = null,
        };
    }

    /// <summary>Reconstructs a <see cref="NotableBusiness"/> from persisted save data (ADR 0010).</summary>
    public static NotableBusiness Restore(
        RuntimeId<NotableBusiness> id,
        string name,
        PropertyOwnerRef owner,
        NotableBusinessTrigger trigger,
        NotableBusinessStatus status,
        GameDate lastRelevantContactDate,
        int reputation,
        DefinitionId<Good>? outputGoodId,
        RuntimeId<PropertyRecord>? linkedPropertyRecordId,
        RuntimeId<District>? districtId,
        RuntimeId<NotableBusiness>? mainCompetitorBusinessId,
        NotableBusinessSupplierRef? mainSupplier,
        bool supplierDisruptionApplied,
        bool isSpecialized,
        DefinitionId<Good>? specializedGoodId) =>
        new()
        {
            Id = id,
            Name = name,
            Owner = owner,
            SampledOrTriggeredBy = trigger,
            Status = status,
            LastRelevantContactDate = lastRelevantContactDate,
            Reputation = reputation,
            OutputGoodId = outputGoodId,
            LinkedPropertyRecordId = linkedPropertyRecordId,
            DistrictId = districtId,
            MainCompetitorBusinessId = mainCompetitorBusinessId,
            MainSupplier = mainSupplier,
            SupplierDisruptionApplied = supplierDisruptionApplied,
            IsSpecialized = isSpecialized,
            SpecializedGoodId = specializedGoodId,
        };
}

/// <summary>Read-side lookup for a <see cref="NotableBusiness"/>, matching <see
/// cref="RealEstate.PropertyResolver"/>'s and <see cref="MerchantFamilies.MerchantHouseArchetypeResolver"/>'s
/// own identical "one shared resolver" convention.</summary>
public static class NotableBusinessResolver
{
    public static bool TryGetCurrent(WorldState state, RuntimeId<NotableBusiness> businessId, out NotableBusiness business) =>
        state.NotableBusinesses.TryGet(businessId, out business!);
}

/// <summary>Resolves a <see cref="NotableBusiness.Owner"/> down to a real <see cref="Character"/>,
/// where §5's Sabotage/Damaging-Rumor rivalry actions and §9's business-Scandal integration both need
/// one — a <see cref="PropertyOwnerKind.IndividualCharacter"/> resolves directly; a <see
/// cref="PropertyOwnerKind.PlayerHousehold"/> or <see cref="PropertyOwnerKind.RivalGens"/> resolves
/// through its own already-tracked head (<see cref="HouseholdHeadship.HeadCharacterId"/>/<see
/// cref="LivingWorldActor.HeadCharacterId"/>) — the real, existing stand-in for §2's "Notable Household
/// head" this codebase actually has, since no <c>Notable Households</c> domain exists to read one
/// from directly.</summary>
public static class NotableBusinessOwnerResolver
{
    public static bool TryResolveCharacter(WorldState state, PropertyOwnerRef owner, out RuntimeId<Character> characterId)
    {
        switch (owner.Kind)
        {
            case PropertyOwnerKind.IndividualCharacter:
                characterId = RuntimeId<Character>.Parse(owner.OwnerId!);
                return state.Characters.TryGet(characterId, out var character) && character!.IsAlive;

            case PropertyOwnerKind.PlayerHousehold:
                var householdId = RuntimeId<Household>.Parse(owner.OwnerId!);
                if (state.HouseholdHeadships.TryGet(householdId, out var headship))
                {
                    characterId = headship!.HeadCharacterId;
                    return state.Characters.TryGet(characterId, out var head) && head!.IsAlive;
                }

                break;

            case PropertyOwnerKind.RivalGens:
                var actorId = RuntimeId<Actor>.Parse(owner.OwnerId!);
                if (state.Actors.TryGet(actorId, out var actor) && actor!.HeadCharacterId is { } headCharacterId)
                {
                    characterId = headCharacterId;
                    return state.Characters.TryGet(characterId, out var actorHead) && actorHead!.IsAlive;
                }

                break;
        }

        characterId = default;
        return false;
    }

    /// <summary>Resolves a <see cref="PropertyOwnerKind.PlayerHousehold"/> owner down to its household
    /// ID, for the one caller (§9's business-Scandal integration) that needs a <see
    /// cref="RuntimeId{Household}"/> rather than a Character — <see cref="Scandal.RecordScandalCommand"/>
    /// is itself household-scoped, so this only ever resolves for that one owner kind (matching <see
    /// cref="MerchantFamilies.SenateEntryInvestmentLog"/>'s own identical player-household-only
    /// narrowing).</summary>
    public static bool TryResolveHousehold(PropertyOwnerRef owner, out RuntimeId<Household> householdId)
    {
        if (owner.Kind == PropertyOwnerKind.PlayerHousehold)
        {
            householdId = RuntimeId<Household>.Parse(owner.OwnerId!);
            return true;
        }

        householdId = default;
        return false;
    }
}
