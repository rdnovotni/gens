using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Crime;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Markets;
using Gens.Simulation.NotableBusinesses;
using Gens.Simulation.RealEstate;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.BusinessCompetition;

/// <summary>
/// §5's/§9's <c>GrainHoardingRecord</c> data model (Phase 15 item 5) — "this project's own single most
/// severe form of economic misconduct," deliberately outside §2's own survivable rungs. Sparse, keyed by
/// the hoarding business's own already-registered <see cref="RuntimeId{NotableBusiness}"/>, matching every
/// other Phase 15 "present only once touched" sparse partition.
/// </summary>
public sealed record GrainHoardingRecord(
    RuntimeId<NotableBusiness> BusinessId,
    bool IsActivelyHoarding,
    bool DuringActiveShortage,
    bool MobViolenceTriggered,
    bool PunishableOffenseGenerated);

public static class GrainHoardingResolver
{
    public static bool TryGetCurrent(WorldState state, RuntimeId<NotableBusiness> businessId, out GrainHoardingRecord record) =>
        state.GrainHoardingRecords.TryGet(businessId, out record!);

    /// <summary>§5's own real, checkable trigger population: "a grain-trading Notable Business" — this
    /// item reads that literally as a business whose own <see cref="NotableBusiness.OutputGoodId"/> is
    /// the exact grain-equivalent good Settlement Demographics' own Cura Annonae anxiety and Grain Dole
    /// both already anchor on, <see cref="NeedsConsumptionCalculator.ConsumptionGood"/> — Resources &amp;
    /// Goods' own single modeled needs-basket good (that calculator's own doc comment), rather than this
    /// item inventing a second, parallel "is this a grain business" flag.</summary>
    public static bool IsGrainTrading(NotableBusiness business) => business.OutputGoodId == NeedsConsumptionCalculator.ConsumptionGood;
}

/// <summary>§5's own declaration of the act itself — a grain-trading business's owner deliberately
/// withholds stock rather than releasing it. Declaring hoarding carries no immediate consequence on its
/// own; §5's own real severity only actually lands once <see cref="GrainHoardingResolutionSystem"/>
/// later finds this coincides with a genuine shortage.</summary>
public sealed record DeclareGrainHoardingCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<NotableBusiness> BusinessId) : ICommand;

public sealed record EndGrainHoardingCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<NotableBusiness> BusinessId) : ICommand;

public sealed record GrainHoardingDeclarationChangedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<NotableBusiness> BusinessId,
    bool IsActivelyHoarding,
    string? CausationId) : IDomainEvent
{
    public string Type => "businessCompetition.grainHoardingDeclarationChanged";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { BusinessId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class GrainHoardingDeclarationCommands
{
    public static readonly ValidationErrorCode BusinessNotFound = new("businessCompetition.grainHoarding.businessNotFound");
    public static readonly ValidationErrorCode NotGrainTrading = new("businessCompetition.grainHoarding.notGrainTrading");
    public static readonly ValidationErrorCode AlreadyHoarding = new("businessCompetition.grainHoarding.alreadyHoarding");
    public static readonly ValidationErrorCode NotHoarding = new("businessCompetition.grainHoarding.notHoarding");

    public static readonly CommandPipeline<WorldState, DeclareGrainHoardingCommand> DeclarePipeline = new(
        validate: ValidateDeclare, mutate: MutateDeclare, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    public static readonly CommandPipeline<WorldState, EndGrainHoardingCommand> EndPipeline = new(
        validate: ValidateEnd, mutate: MutateEnd, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? ValidateDeclare(WorldState state, DeclareGrainHoardingCommand command)
    {
        if (!state.NotableBusinesses.TryGet(command.BusinessId, out var business))
            return BusinessNotFound;
        if (!GrainHoardingResolver.IsGrainTrading(business!))
            return NotGrainTrading;
        if (state.GrainHoardingRecords.TryGet(command.BusinessId, out var existing) && existing!.IsActivelyHoarding)
            return AlreadyHoarding;

        return null;
    }

    private static IDomainEvent[] MutateDeclare(WorldState state, DeclareGrainHoardingCommand command)
    {
        if (state.GrainHoardingRecords.TryGet(command.BusinessId, out _))
            state.GrainHoardingRecords.Remove(command.BusinessId);
        state.GrainHoardingRecords.Add(
            command.BusinessId,
            new GrainHoardingRecord(command.BusinessId, IsActivelyHoarding: true, DuringActiveShortage: false, MobViolenceTriggered: false, PunishableOffenseGenerated: false));

        return new IDomainEvent[]
        {
            new GrainHoardingDeclarationChangedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.BusinessId, IsActivelyHoarding: true, command.CommandId.ToTaggedString()),
        };
    }

    private static ValidationErrorCode? ValidateEnd(WorldState state, EndGrainHoardingCommand command)
    {
        if (!state.GrainHoardingRecords.TryGet(command.BusinessId, out var existing) || !existing!.IsActivelyHoarding)
            return NotHoarding;

        return null;
    }

    private static IDomainEvent[] MutateEnd(WorldState state, EndGrainHoardingCommand command)
    {
        state.GrainHoardingRecords.TryGet(command.BusinessId, out var existing);
        state.GrainHoardingRecords.Remove(command.BusinessId);
        state.GrainHoardingRecords.Add(command.BusinessId, existing! with { IsActivelyHoarding = false });

        return new IDomainEvent[]
        {
            new GrainHoardingDeclarationChangedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.BusinessId, IsActivelyHoarding: false, command.CommandId.ToTaggedString()),
        };
    }
}

/// <summary>
/// §5's own real severity resolution (Phase 15 item 5), matching <see
/// cref="NotableBusinesses.SupplierDisruptionSystem"/>'s established static <c>Tick(state, date)</c>
/// convention. For every business actively hoarding: reads <see
/// cref="GrainHoardingResolver.IsGrainTrading"/>'s own good against its resolved settlement's (via <see
/// cref="NotableBusiness.DistrictId"/>) cleared <see cref="SettlementMarket"/> — §5's own real, checkable
/// "genuine shortage" signal is <see cref="SettlementMarket.UnsatisfiedDemand"/> &gt; 0: demand this
/// settlement's own market clearing could not actually satisfy, the one real shortage indicator this
/// codebase tracks (no separate Grain Dole/Cura Annonae shortage flag exists anywhere — Settlement
/// Demographics' Grain Dole is confirmed unbuilt, matching item 4's own identical finding for the same
/// gap).
///
/// The first month hoarding coincides with a real shortage: marks <see
/// cref="GrainHoardingRecord.MobViolenceTriggered"/> (§5's "a real risk of mob violence directly against
/// the business and its owner" — realized as a real, one-time Ledger loss against the owner's own tracked
/// account, since no <c>Notable Households</c> domain exists to carry a dedicated household-consequence
/// field instead, matching item 4's own identical "Notable Household" narrowing), applies a real Business
/// Reputation penalty, and records a real, live <see cref="PunishableOffense"/> against the owner's own
/// resolved Character (§5's "a real, live Crime &amp; Punishment exposure... rather than an ordinary civil
/// Market Dynamics outcome") via <see cref="RecordPunishableOffenseCommand"/> with the new <see
/// cref="PunishableOffenseSource.GrainHoarding"/> source (purely additive, matching that enum's own
/// <see cref="PunishableOffenseSource.Fabricated"/> precedent) at <see cref="OffenseSeverity.Capital"/> —
/// §5's own "this project's own single most severe form of economic misconduct" read as this codebase's
/// own most severe offense tier. Fires exactly once per hoarding bout (mirroring <see
/// cref="NotableBusiness.SupplierDisruptionApplied"/>'s identical guard shape) — a fresh <see
/// cref="EndGrainHoardingCommand"/>/<see cref="DeclareGrainHoardingCommand"/> cycle resets it. §5's own
/// unresolved "how the mob violence itself actually resolves" (§10: "likely reading through Crime &amp;
/// Punishment's own Justice Spectrum... but not explicitly stated") is left exactly that unresolved — this
/// item builds the real, triggered fact and its real, immediate financial/legal consequences, not a
/// further extralegal-violence resolution mechanic §5 never specifies.
/// </summary>
public static class GrainHoardingResolutionSystem
{
    public static IReadOnlyList<IDomainEvent> Tick(WorldState state, GameDate date)
    {
        var events = new List<IDomainEvent>();

        foreach (var entry in state.GrainHoardingRecords.InAscendingOrder().ToArray())
        {
            var record = entry.Value;
            if (!record.IsActivelyHoarding || record.MobViolenceTriggered)
                continue;
            if (!state.NotableBusinesses.TryGet(entry.Key, out var business) || business!.Status != NotableBusinessStatus.Tracked)
                continue;
            if (!TryResolveSettlement(state, business, out var settlementId))
                continue;

            var key = new MarketGoodKey(settlementId, NeedsConsumptionCalculator.ConsumptionGood);
            var duringShortage = state.MarketPrices.TryGet(key, out var market) && market!.UnsatisfiedDemand > 0;

            state.GrainHoardingRecords.Remove(entry.Key);
            if (!duringShortage)
            {
                state.GrainHoardingRecords.Add(entry.Key, record with { DuringActiveShortage = false });
                continue;
            }

            var punishableOffenseGenerated = false;
            if (NotableBusinessOwnerResolver.TryResolveCharacter(state, business.Owner, out var characterId))
            {
                events.AddRange(RecordPunishableOffenseCommands.Pipeline.Execute(
                    state, new RecordPunishableOffenseCommand(
                        state.CommandIds.Issue(), "system", date, null, characterId,
                        PunishableOffenseSource.GrainHoarding, OffenseSeverity.Capital)).Events);
                punishableOffenseGenerated = true;
            }

            if (TryResolveOwnerAccount(business.Owner, out var ownerAccount))
            {
                events.Add(LedgerService.Post(
                    state, date, LedgerTransactionCategory.Transfers,
                    new[]
                    {
                        new LedgerPosting(ownerAccount, -BusinessCompetitionCatalog.MobViolencePropertyDamage),
                        new LedgerPosting(LedgerAccountKey.Mint, BusinessCompetitionCatalog.MobViolencePropertyDamage),
                    },
                    reference: $"businessCompetition.mobViolence:{entry.Key.ToTaggedString()}"));
            }

            events.AddRange(AdjustBusinessReputationCommands.Pipeline.Execute(
                state, new AdjustBusinessReputationCommand(
                    state.CommandIds.Issue(), "system", date, null, entry.Key,
                    -BusinessCompetitionCatalog.GrainHoardingReputationLoss, BusinessReputationChangeReason.PriceGouging)).Events);

            state.GrainHoardingRecords.Add(
                entry.Key, record with { DuringActiveShortage = true, MobViolenceTriggered = true, PunishableOffenseGenerated = punishableOffenseGenerated });

            events.Add(new GrainHoardingMobViolenceTriggeredEvent(state.EventIds.Issue(), date, entry.Key, punishableOffenseGenerated, CausationId: null));
        }

        return events;
    }

    private static bool TryResolveSettlement(WorldState state, NotableBusiness business, out RuntimeId<Settlement> settlementId)
    {
        if (business.DistrictId is { } districtId && state.Districts.TryGet(districtId, out var district))
        {
            settlementId = district!.SettlementId;
            return true;
        }

        settlementId = default;
        return false;
    }

    private static bool TryResolveOwnerAccount(PropertyOwnerRef owner, out LedgerAccountKey account)
    {
        switch (owner.Kind)
        {
            case PropertyOwnerKind.PlayerHousehold:
                account = LedgerAccountKey.ForHousehold(RuntimeId<Household>.Parse(owner.OwnerId!));
                return true;
            case PropertyOwnerKind.RivalGens:
                account = LedgerAccountKey.ForActor(RuntimeId<Actor>.Parse(owner.OwnerId!));
                return true;
            default:
                account = default;
                return false;
        }
    }
}

public sealed record GrainHoardingMobViolenceTriggeredEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<NotableBusiness> BusinessId,
    bool PunishableOffenseGenerated,
    string? CausationId) : IDomainEvent
{
    public string Type => "businessCompetition.grainHoardingMobViolenceTriggered";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { BusinessId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}
