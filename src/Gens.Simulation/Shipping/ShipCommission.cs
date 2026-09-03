using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.RealEstate;
using Gens.Simulation.Religion;
using Gens.Simulation.Reputation;
using Gens.Simulation.Societates;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Shipping;

public enum ShipCommissionStatus
{
    InProgress,
    Completed,
}

/// <summary>
/// §3/§11's <c>ShipCommission</c> — a real, multi-month construction project (Phase 15 item 8), matching
/// <see cref="PrivateInfrastructure.LandReclamationProject"/>'s own "real, slow, capital... investment"
/// shape rather than an instant purchase, per §3's own explicit "never simply pulled off an implicit
/// market shelf." §3's own "commissioned at the Shipyard/Navalia" is honestly narrowed: no Shipyard/
/// Navalia building exists anywhere in this codebase (<c>Buildings.BuildingDefinition</c>'s own roster,
/// confirmed unbuilt by direct search) — this item gates commissioning on the one real, checkable proxy
/// a Buildings entry would otherwise require, matching <see cref="PrivateInfrastructure.PrivateBridge"/>'s
/// and <see cref="PrivateInfrastructure.LandReclamationProject"/>'s own "the caller supplies an
/// already-resolved real fact this item can check directly" precedent: the settlement must actually have
/// a Coast- or River-terrain Plot (<see cref="CommissionShipCommands"/>'s own doc comment). §3.1's own
/// culture/region hull-class gating ("gated by the settlement's own region/culture the way Estate &amp;
/// Settlement already gates buildings by terrain") is the same honest gap — no per-Culture vessel-class
/// eligibility table exists anywhere in this codebase for this item to read, so every vessel class in
/// §2's registry is commissionable at any maritime-capable settlement regardless of culture.
/// </summary>
public sealed record ShipCommissionProject
{
    private ShipCommissionProject()
    {
    }

    public required RuntimeId<ShipCommissionProject> Id { get; init; }
    public required RuntimeId<Household> HouseholdId { get; init; }
    public required RuntimeId<Settlement> SettlementId { get; init; }
    public required string ShipName { get; init; }
    public required ShipVesselClass VesselClass { get; init; }
    public required GoodQuality BuildQuality { get; init; }

    /// <summary>§3.1's Decoration axis — "purely a Dignitas-and-flavor layer, mechanically light." A
    /// plain, free-form, unvalidated string, matching <see cref="Societates.Societas.DurationOrPurpose"/>'s
    /// own identical "free-form negotiated term... this item does not attempt to parse or enforce"
    /// precedent — nothing in this item reads this value back mechanically.</summary>
    public required string DecorationChoice { get; init; }

    public required bool ConsecratedLaunchRequested { get; init; }
    public required ShipOwnershipMode OwnershipMode { get; init; }
    public RuntimeId<Societas>? OwningSocietasId { get; init; }
    public PropertyOwnerRef? FrontingPersonOrSocietasId { get; init; }
    public required GameDate StartMonth { get; init; }
    public required int MonthsInvested { get; init; }
    public required ShipCommissionStatus Status { get; init; }
    public RuntimeId<MerchantShip>? ResultingShipId { get; init; }

    public static ShipCommissionProject Start(
        RuntimeId<ShipCommissionProject> id,
        RuntimeId<Household> householdId,
        RuntimeId<Settlement> settlementId,
        string shipName,
        ShipVesselClass vesselClass,
        GoodQuality buildQuality,
        string decorationChoice,
        bool consecratedLaunchRequested,
        ShipOwnershipMode ownershipMode,
        RuntimeId<Societas>? owningSocietasId,
        PropertyOwnerRef? frontingPersonOrSocietasId,
        GameDate startMonth) => new()
        {
            Id = id,
            HouseholdId = householdId,
            SettlementId = settlementId,
            ShipName = shipName,
            VesselClass = vesselClass,
            BuildQuality = buildQuality,
            DecorationChoice = decorationChoice,
            ConsecratedLaunchRequested = consecratedLaunchRequested,
            OwnershipMode = ownershipMode,
            OwningSocietasId = owningSocietasId,
            FrontingPersonOrSocietasId = frontingPersonOrSocietasId,
            StartMonth = startMonth,
            MonthsInvested = 0,
            Status = ShipCommissionStatus.InProgress,
            ResultingShipId = null,
        };

    /// <summary>Reconstructs a <see cref="ShipCommissionProject"/> from persisted save data (ADR 0010).</summary>
    public static ShipCommissionProject Restore(
        RuntimeId<ShipCommissionProject> id,
        RuntimeId<Household> householdId,
        RuntimeId<Settlement> settlementId,
        string shipName,
        ShipVesselClass vesselClass,
        GoodQuality buildQuality,
        string decorationChoice,
        bool consecratedLaunchRequested,
        ShipOwnershipMode ownershipMode,
        RuntimeId<Societas>? owningSocietasId,
        PropertyOwnerRef? frontingPersonOrSocietasId,
        GameDate startMonth,
        int monthsInvested,
        ShipCommissionStatus status,
        RuntimeId<MerchantShip>? resultingShipId) => new()
        {
            Id = id,
            HouseholdId = householdId,
            SettlementId = settlementId,
            ShipName = shipName,
            VesselClass = vesselClass,
            BuildQuality = buildQuality,
            DecorationChoice = decorationChoice,
            ConsecratedLaunchRequested = consecratedLaunchRequested,
            OwnershipMode = ownershipMode,
            OwningSocietasId = owningSocietasId,
            FrontingPersonOrSocietasId = frontingPersonOrSocietasId,
            StartMonth = startMonth,
            MonthsInvested = monthsInvested,
            Status = status,
            ResultingShipId = resultingShipId,
        };
}

public sealed record ShipCommissionStartedEvent(
    RuntimeId<DomainEventEntity> EventId, GameDate OccurredDate, RuntimeId<ShipCommissionProject> ProjectId,
    RuntimeId<Household> HouseholdId, ShipVesselClass VesselClass, string? CausationId) : IDomainEvent
{
    public string Type => "shipping.commissionStarted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { ProjectId.ToTaggedString(), HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public sealed record ShipCommissionCompletedEvent(
    RuntimeId<DomainEventEntity> EventId, GameDate OccurredDate, RuntimeId<ShipCommissionProject> ProjectId,
    RuntimeId<MerchantShip> ShipId, bool BlessedLaunch, string? CausationId) : IDomainEvent
{
    public string Type => "shipping.commissionCompleted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { ShipId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>§3's Custom Commissioning start (Phase 15 item 8). Ownership shape is chosen here, at
/// commissioning, rather than through a later conversion command — §5 describes the three shapes as how
/// a household "actually holds a Ship," not as a state a Ship transitions through, and no other Phase 15
/// item's own ownership record (<see cref="RealEstate.PropertyOwnerRef"/>, <see
/// cref="Societates.Societas"/>) offers a live re-titling command either.</summary>
public sealed record CommissionShipCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Settlement> SettlementId,
    string ShipName,
    ShipVesselClass VesselClass,
    GoodQuality BuildQuality,
    string DecorationChoice,
    bool ConsecratedLaunchRequested,
    ShipOwnershipMode OwnershipMode,
    RuntimeId<Societas>? OwningSocietasId = null,
    PropertyOwnerRef? FrontingPersonOrSocietasId = null) : ICommand;

public static class CommissionShipCommands
{
    public static readonly ValidationErrorCode EmptyShipName = new("shipping.commissionShip.emptyShipName");
    public static readonly ValidationErrorCode SettlementNotFound = new("shipping.commissionShip.settlementNotFound");

    /// <summary>§3's "commissioned at the Shipyard/Navalia" — since no such building exists anywhere in
    /// this codebase, the real, checkable proxy this item gates on instead: the settlement must actually
    /// have at least one Coast- or River-terrain Plot, matching <see
    /// cref="PrivateInfrastructure.BuildPrivateBridgeCommands"/>'s own identical "at least one Plot
    /// River-terrain or River-adjacent" gate applied to a settlement's own maritime access rather than
    /// one Plot's own riverbank.</summary>
    public static readonly ValidationErrorCode SettlementNotMaritime = new("shipping.commissionShip.settlementNotMaritime");

    public static readonly ValidationErrorCode SocietasRequired = new("shipping.commissionShip.societasRequired");
    public static readonly ValidationErrorCode SocietasNotFound = new("shipping.commissionShip.societasNotFound");
    public static readonly ValidationErrorCode SocietasNotActive = new("shipping.commissionShip.societasNotActive");
    public static readonly ValidationErrorCode HouseholdNotPartner = new("shipping.commissionShip.householdNotPartner");
    public static readonly ValidationErrorCode FrontingReferenceRequired = new("shipping.commissionShip.frontingReferenceRequired");
    public static readonly ValidationErrorCode FrontingKindNotSupported = new("shipping.commissionShip.frontingKindNotSupported");
    public static readonly ValidationErrorCode FrontingCharacterNotFound = new("shipping.commissionShip.frontingCharacterNotFound");
    public static readonly ValidationErrorCode FrontingSocietasNotFound = new("shipping.commissionShip.frontingSocietasNotFound");
    public static readonly ValidationErrorCode NoPatronDeityForConsecratedLaunch = new("shipping.commissionShip.noPatronDeityForConsecratedLaunch");

    public static readonly CommandPipeline<WorldState, CommissionShipCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, CommissionShipCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.ShipName))
            return EmptyShipName;
        if (!state.Settlements.TryGet(command.SettlementId, out _))
            return SettlementNotFound;
        var isMaritime = state.Plots.InAscendingOrder()
            .Any(entry => entry.Value.SettlementId == command.SettlementId
                && entry.Value.Terrain is TerrainType.Coast or TerrainType.River);
        if (!isMaritime)
            return SettlementNotMaritime;

        if (command.OwnershipMode == ShipOwnershipMode.Societas)
        {
            if (command.OwningSocietasId is not { } societasId)
                return SocietasRequired;
            if (!state.Societates.TryGet(societasId, out var societas))
                return SocietasNotFound;
            if (!societas!.IsActive)
                return SocietasNotActive;
            if (!SocietasResolver.IsPartner(societas!, PropertyOwnerRef.ForPlayerHousehold(command.HouseholdId)))
                return HouseholdNotPartner;
        }
        else if (command.OwnershipMode == ShipOwnershipMode.Fronted)
        {
            if (command.FrontingPersonOrSocietasId is not { } frontingRef)
                return FrontingReferenceRequired;

            // §5's own text names exactly two real fronts — "a freedman Operator or a Societas the
            // senator quietly controls" — so every other PropertyOwnerKind (RomanState, Municipal,
            // PlayerHousehold, a nonexistent individual, etc.) is rejected here rather than silently
            // accepted the way an unvalidated PropertyOwnerRef? would otherwise let through.
            if (frontingRef.Kind == PropertyOwnerKind.IndividualCharacter)
            {
                if (frontingRef.OwnerId is null || !state.Characters.TryGet(RuntimeId<Character>.Parse(frontingRef.OwnerId), out var character)
                    || !character!.IsAlive)
                {
                    return FrontingCharacterNotFound;
                }
            }
            else if (frontingRef.Kind == PropertyOwnerKind.Societas)
            {
                // §5's own Societas front is <see cref="PropertyOwnerRef.ForSocietasPlaceholder"/> — a
                // free-form, narrative-only display name (<see cref="FrontingArrangement"/>'s own doc
                // comment: "deliberately not... a real, resolvable Societas link"), not the real,
                // registry-backed Societas §7's ownership mode above checks. There is no real record to
                // resolve this kind against, so the one real thing left to validate is that a display
                // name was actually supplied.
                if (string.IsNullOrWhiteSpace(frontingRef.OwnerId))
                    return FrontingSocietasNotFound;
            }
            else
            {
                return FrontingKindNotSupported;
            }
        }

        if (command.ConsecratedLaunchRequested && !HouseholdReligionResolver.HasChosenPatron(state, command.HouseholdId))
            return NoPatronDeityForConsecratedLaunch;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, CommissionShipCommand command)
    {
        var id = state.ShipCommissionProjectIds.Issue();
        var project = ShipCommissionProject.Start(
            id, command.HouseholdId, command.SettlementId, command.ShipName, command.VesselClass, command.BuildQuality,
            command.DecorationChoice, command.ConsecratedLaunchRequested, command.OwnershipMode, command.OwningSocietasId,
            command.FrontingPersonOrSocietasId, command.SubmittedDate);
        state.ShipCommissionProjects.Add(id, project);

        return new IDomainEvent[]
        {
            new ShipCommissionStartedEvent(
                state.EventIds.Issue(), command.SubmittedDate, id, command.HouseholdId, command.VesselClass,
                command.CommandId.ToTaggedString()),
        };
    }
}

/// <summary>
/// §3's monthly commissioning resolution (Phase 15 item 8), matching <see
/// cref="PrivateInfrastructure.LandReclamationResolutionSystem"/>'s own unwired static-<c>Tick</c>
/// convention. For every in-progress project whose owning household actually pays this month's share of
/// <see cref="ShippingCatalog.TotalCommissionCost"/> (spread evenly across <see
/// cref="ShippingCatalog.CommissionDurationMonths"/>, paid from the commissioning household's own
/// account — an unpaid month simply does not advance <see cref="ShipCommissionProject.MonthsInvested"/>,
/// the same real, forgiving stall <see cref="PrivateInfrastructure.LandReclamationResolutionSystem"/>
/// already established rather than a hard failure), advances progress; once the full duration is
/// reached, creates the real <see cref="MerchantShip"/> record and, if §3.2's Consecrated Launch was
/// requested, attempts that Funded Action (a flat <see cref="ShippingCatalog.ConsecratedLaunchCost"/>
/// spend against the same household account — insufficient funds at completion time simply leaves <see
/// cref="MerchantShip.BlessedLaunch"/> false rather than blocking the Ship's own creation, since the
/// ceremony was always optional per §3.2's own "a Ship launched without this ceremony suffers no
/// penalty").
/// </summary>
public static class ShipCommissionResolutionSystem
{
    public static IReadOnlyList<IDomainEvent> Tick(WorldState state, GameDate date)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var entry in state.ShipCommissionProjects.InAscendingOrder().ToArray())
        {
            var project = entry.Value;
            if (project.Status != ShipCommissionStatus.InProgress)
                continue;

            var tier = ShippingCatalog.CapacityTierFor(project.VesselClass);
            var durationMonths = ShippingCatalog.CommissionDurationMonths(tier);
            var totalCost = ShippingCatalog.TotalCommissionCost(tier, project.BuildQuality);
            var monthlyCost = Money.FromMinorUnits(totalCost.RawValue / durationMonths);

            var paid = TryPay(state, date, project.HouseholdId, monthlyCost, LedgerTransactionCategory.Construction, "shipping.commission");
            var monthsInvested = paid ? project.MonthsInvested + 1 : project.MonthsInvested;

            if (monthsInvested < durationMonths)
            {
                state.ShipCommissionProjects.Remove(entry.Key);
                state.ShipCommissionProjects.Add(entry.Key, project with { MonthsInvested = monthsInvested });
                continue;
            }

            var blessedLaunch = project.ConsecratedLaunchRequested
                && TryPay(
                    state, date, project.HouseholdId, ShippingCatalog.ConsecratedLaunchCost, LedgerTransactionCategory.Gifts,
                    "shipping.consecratedLaunch");

            var shipId = state.MerchantShipIds.Issue();
            var ship = MerchantShip.Create(
                shipId, project.ShipName, project.VesselClass, project.BuildQuality, project.OwnershipMode, project.HouseholdId,
                project.SettlementId, project.OwningSocietasId, blessedLaunch);
            state.MerchantShips.Add(shipId, ship);

            if (project.OwnershipMode == ShipOwnershipMode.Fronted && project.FrontingPersonOrSocietasId is { } frontingRef)
            {
                state.ShipFrontingArrangements.Add(shipId, FrontingArrangement.Create(shipId, project.HouseholdId, frontingRef));
            }

            state.ShipCommissionProjects.Remove(entry.Key);
            state.ShipCommissionProjects.Add(entry.Key, project with
            {
                MonthsInvested = monthsInvested,
                Status = ShipCommissionStatus.Completed,
                ResultingShipId = shipId,
            });

            var completedEvent = new ShipCommissionCompletedEvent(
                state.EventIds.Issue(), date, project.Id, shipId, blessedLaunch, CausationId: null);
            events.Add(completedEvent);

            if (blessedLaunch)
            {
                HouseholdReligionResolver.ApplyFavorDelta(state, project.HouseholdId, ShippingCatalog.ConsecratedLaunchFavorGain);
                events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
                    state, new AdjustDignitasCommand(
                        state.CommandIds.Issue(), "system", date, completedEvent.EventId.ToTaggedString(), project.HouseholdId,
                        ShippingCatalog.ConsecratedLaunchDignitasGain, "consecrated a newly launched ship")).Events);
            }
        }

        return events;
    }

    private static bool TryPay(
        WorldState state, GameDate date, RuntimeId<Household> householdId, Money amount, LedgerTransactionCategory category, string reference)
    {
        var account = LedgerAccountKey.ForHousehold(householdId);
        var balance = state.LedgerAccounts.TryGet(account, out var ledgerAccount) ? ledgerAccount!.Balance : Money.Zero;
        if (balance.RawValue < amount.RawValue)
            return false;

        LedgerService.Post(
            state, date, category,
            new[]
            {
                new LedgerPosting(account, -amount),
                new LedgerPosting(LedgerAccountKey.Mint, amount),
            },
            reference: $"{reference}:{householdId.ToTaggedString()}");
        return true;
    }
}
