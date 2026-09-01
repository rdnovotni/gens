using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.RealEstate;

/// <summary>
/// §6's monthly Operator resolution (Phase 15 item 1): "The Operator's own Core Attributes and Loyalty
/// (the same stats this project already tracks for every Character) determine how the arrangement
/// actually plays out" — this system is that determination, run once a month against every property
/// (<see cref="Plot"/> or <see cref="PropertyRecord"/>) currently <see
/// cref="PropertyManagementStatus.LeasedOut"/> with a living, assigned Operator:
///
/// <list type="bullet">
/// <item><description><b>Skim state</b> (§6's "quietly under-reports income") — reused directly from
/// <see cref="Land.DistantHoldingMismanagementRiskSystem"/>'s own Loyalty-risk threshold rather than
/// inventing a second one: an Operator whose <see cref="Condition.Loyalty"/> has fallen below <see
/// cref="RealEstateCatalog.SkimmingLoyaltyThreshold"/> is skimming this month.</description></item>
/// <item><description><b>Remittance</b> — a real, monthly Ledger posting from the property's own owner
/// account to <see cref="LedgerAccountKey.Mint"/> is deliberately <i>not</i> how this reads: the
/// Operator's remitted share is income the owner receives, so this system posts <see
/// cref="LedgerAccountKey.Mint"/> → the owner's own account (only for an owner kind this item can
/// resolve a real ledger balance for), at <see cref="RealEstateCatalog.SteadyOperatorMonthlyYield"/> or
/// the lower <see cref="RealEstateCatalog.SkimmingOperatorMonthlyYield"/> of the property's own tracked
/// Value.</description></item>
/// <item><description><b>Tenure and buyout eligibility</b> (§6.1's worked example) — tenure increments
/// every month the Operator holds the same assignment; a real buyout offer fires only once, when the
/// Operator has never skimmed, has held the assignment at least <see
/// cref="RealEstateCatalog.BuyoutMinimumTenureMonths"/>, clears both the Ambition (Core Condition) and
/// Stewardship (Core Attribute) floors, and the property's own District Property Value has genuinely
/// climbed past <see cref="RealEstateCatalog.BuyoutDistrictPropertyValueThreshold"/> — tying together
/// exactly the two conditions §6.1's own worked example names ("the freedman proves genuinely
/// capable, the District's own Property Value keeps climbing"). A property with no District (a Ship,
/// or a Plot not yet drawn into a District) never reaches buyout eligibility through this
/// path.</description></item>
/// </list>
/// </summary>
public sealed class OperatorLifecycleSystem : IMonthlySystem<WorldState>
{
    public string Id => "realEstate.operatorLifecycle";
    public TickPhase Phase => TickPhase.MarketsLedger;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "plots", "plotPropertyExtensions", "propertyRecords", "characters", "districts" };
    public IReadOnlyCollection<string> Writes { get; } =
        new[] { "plots", "plotPropertyExtensions", "propertyRecords", "ledgerAccounts", "ledgerTransactions", "ledgerTransactionIds", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "realEstate.districtPropertyValue" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var plotEntry in state.Plots.InAscendingOrder().ToArray())
            Resolve(state, PropertySubjectRef.ForPlot(plotEntry.Key), context, events);

        foreach (var recordEntry in state.PropertyRecords.InAscendingOrder().ToArray())
            Resolve(state, PropertySubjectRef.ForPropertyRecord(recordEntry.Key), context, events);

        return events;
    }

    private static void Resolve(WorldState state, PropertySubjectRef subject, MonthlyTickContext context, List<IDomainEvent> events)
    {
        if (!PropertyResolver.TryResolve(state, subject, out var view))
            return;
        if (view.ManagementStatus != PropertyManagementStatus.LeasedOut || view.OperatorCharacterId is not { } operatorId)
            return;
        if (!state.Characters.TryGet(operatorId, out var operatorCharacter) || !operatorCharacter!.IsAlive)
            return;

        var isSkimming = operatorCharacter.Condition.Loyalty < RealEstateCatalog.SkimmingLoyaltyThreshold;
        var tenure = view.OperatorTenureMonths + 1;

        var districtPropertyValue = view.DistrictId is { } districtId && state.Districts.TryGet(districtId, out var district)
            ? district!.PropertyValue
            : RealEstateCatalog.BaselinePropertyValue;

        var buyoutOffered = view.OperatorBuyoutOffered || (
            !isSkimming &&
            tenure >= RealEstateCatalog.BuyoutMinimumTenureMonths &&
            operatorCharacter.Condition.Ambition >= RealEstateCatalog.BuyoutAmbitionThreshold &&
            operatorCharacter.GetEffectiveAttributes().Stewardship >= RealEstateCatalog.BuyoutStewardshipThreshold &&
            view.DistrictId is not null &&
            districtPropertyValue >= RealEstateCatalog.BuyoutDistrictPropertyValueThreshold);

        PropertyResolver.SetOperatorState(state, subject, isSkimming, tenure, buyoutOffered);

        if (buyoutOffered && !view.OperatorBuyoutOffered)
        {
            events.Add(new OperatorBuyoutOfferedEvent(
                state.EventIds.Issue(), context.Date, subject, operatorId));
        }

        RemitIncome(state, context, subject, view, isSkimming, events);
    }

    private static void RemitIncome(
        WorldState state, MonthlyTickContext context, PropertySubjectRef subject, PropertyView view, bool isSkimming,
        List<IDomainEvent> events)
    {
        if (view.Value == Money.Zero)
            return;
        if (!TryOwnerLedgerAccount(view.Owner, out var ownerAccount))
            return;

        var yield = isSkimming ? RealEstateCatalog.SkimmingOperatorMonthlyYield : RealEstateCatalog.SteadyOperatorMonthlyYield;
        var remittance = view.Value.Scale(yield);
        if (remittance == Money.Zero)
            return;

        var ledgerEvent = LedgerService.Post(
            state, context.Date, LedgerTransactionCategory.Sales,
            new[] { new LedgerPosting(LedgerAccountKey.Mint, -remittance), new LedgerPosting(ownerAccount, remittance) },
            reference: $"realEstate.operatorRemittance:{subject.SubjectId}");
        events.Add(ledgerEvent);
    }

    private static bool TryOwnerLedgerAccount(PropertyOwnerRef owner, out LedgerAccountKey key)
    {
        switch (owner.Kind)
        {
            case PropertyOwnerKind.PlayerHousehold:
                key = LedgerAccountKey.ForHousehold(RuntimeId<Household>.Parse(owner.OwnerId!));
                return true;
            case PropertyOwnerKind.RivalGens:
            case PropertyOwnerKind.Collegium:
                key = LedgerAccountKey.ForActor(RuntimeId<Actor>.Parse(owner.OwnerId!));
                return true;
            case PropertyOwnerKind.Municipal:
                key = LedgerAccountKey.ForSettlementTreasury(RuntimeId<Settlement>.Parse(owner.OwnerId!));
                return true;
            default:
                key = default;
                return false;
        }
    }
}

/// <summary>Emitted the one month a real buyout offer first fires for a given Operator assignment
/// (§6.1) — not repeated every month the flag stays set, matching <see
/// cref="Characters.MigrationAppliedEvent"/>'s identical "only emitted on real change" shape.</summary>
public sealed record OperatorBuyoutOfferedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    PropertySubjectRef Subject,
    RuntimeId<Character> OperatorCharacterId) : IDomainEvent
{
    public string Type => "realEstate.operatorBuyoutOffered";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { Subject.SubjectId, OperatorCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}
