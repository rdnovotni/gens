using Gens.Simulation.Clientela;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Numerics;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.RealEstate;

/// <summary>
/// §5's "acquiring property that's already built and already owned by someone else" and §9's "sold
/// back to the general market... or sold directly to a specific, named party" — one shared command
/// (rule 2's "one command path"), since both are the identical underlying mutation (an ownership
/// transfer for a price) read from opposite narrative directions: §5 is the buyer submitting "I am
/// acquiring this," §9 is the seller submitting "I am selling this," and this codebase has no
/// mechanical reason to give the same state change two separate pipelines. <see
/// cref="PropertyTransferMethod"/> carries which of §5's four acquisition flavors or §9's market-sale
/// flavor actually applies.
///
/// Pricing and the deeper legal/financial trigger behind a <see
/// cref="PropertyTransferMethod.ForcedSale"/> (an Insolvency ruling, a Legal &amp; Court judgment, a
/// confiscation) are upstream concerns this command does not re-validate, matching <see
/// cref="AcquirePlotCommand"/>'s own identical "atomic transfer" scoping — a caller is trusted to have
/// already established that the forced sale is legitimate before submitting this command.
/// </summary>
public sealed record TransferPropertyCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    PropertySubjectRef Subject,
    PropertyTransferMethod Method,
    /// <summary>The acquiring party for every method except <see
    /// cref="PropertyTransferMethod.MarketSale"/> (ignored there — the market has no named buyer, §9)
    /// — for <see cref="PropertyTransferMethod.AgerPublicusLease"/> this is the leasing household
    /// rather than a new owner (ownership stays <see cref="PropertyOwnerRef.RomanState"/>).</summary>
    PropertyOwnerRef? BuyerId,
    /// <summary>Overrides the price this transfer settles at. When <c>null</c>, the price is the
    /// property's own currently tracked <see cref="PropertyView.Value"/> (§9: "a price scaled by the
    /// District's own Property Value... and the asset's own condition and income history" — already
    /// folded into that tracked Value by <see cref="DistrictPropertyValueSystem"/>), minus <see
    /// cref="RealEstateCatalog.MarketSaleFriction"/> for a <see
    /// cref="PropertyTransferMethod.MarketSale"/> specifically (§9's "current Value minus a standard
    /// friction"). Ignored entirely for <see cref="PropertyTransferMethod.AgerPublicusLease"/>, which
    /// transfers no money (§14's own "lease duration... isn't specified here").</summary>
    Money? NegotiatedPrice = null,
    /// <summary>§5's Influence expenditure for <see cref="PropertyTransferMethod.Persuasion"/> only —
    /// spent from the buyer's own household Influence (Politics &amp; Patronage §4.4).</summary>
    int PersuasionInfluenceCost = 0) : ICommand;

public sealed record PropertyTransferredEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    PropertySubjectRef Subject,
    PropertyTransferMethod Method,
    PropertyOwnerRef PreviousOwner,
    PropertyOwnerRef? NewOwner,
    Money? Price,
    string? CausationId) : IDomainEvent
{
    public string Type => "realEstate.propertyTransferred";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { Subject.SubjectId };
    public Visibility Visibility => Visibility.Public;
}

public static class TransferPropertyCommands
{
    public static readonly ValidationErrorCode SubjectNotFound = new("realEstate.transfer.subjectNotFound");
    public static readonly ValidationErrorCode BuyerRequired = new("realEstate.transfer.buyerRequired");
    public static readonly ValidationErrorCode AlreadyOwnedByBuyer = new("realEstate.transfer.alreadyOwnedByBuyer");
    public static readonly ValidationErrorCode NotAgerPublicus = new("realEstate.transfer.notAgerPublicus");
    public static readonly ValidationErrorCode LesseeMustBePlayerHousehold = new("realEstate.transfer.lesseeMustBePlayerHousehold");
    public static readonly ValidationErrorCode NotPersuadable = new("realEstate.transfer.notPersuadable");
    public static readonly ValidationErrorCode PersuasionRequiresInfluenceCost = new("realEstate.transfer.persuasionRequiresInfluenceCost");
    public static readonly ValidationErrorCode PersuaderMustBePlayerHousehold = new("realEstate.transfer.persuaderMustBePlayerHousehold");
    public static readonly ValidationErrorCode InsufficientInfluence = new("realEstate.transfer.insufficientInfluence");
    public static readonly ValidationErrorCode NegativePrice = new("realEstate.transfer.negativePrice");

    public static readonly CommandPipeline<WorldState, TransferPropertyCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, TransferPropertyCommand command)
    {
        if (!PropertyResolver.TryResolve(state, command.Subject, out var view))
            return SubjectNotFound;
        if (command.NegotiatedPrice is { IsNegative: true })
            return NegativePrice;

        switch (command.Method)
        {
            case PropertyTransferMethod.MarketSale:
                return null;

            case PropertyTransferMethod.AgerPublicusLease:
                if (view.Owner.Kind != PropertyOwnerKind.RomanState)
                    return NotAgerPublicus;
                if (command.BuyerId is not { Kind: PropertyOwnerKind.PlayerHousehold })
                    return LesseeMustBePlayerHousehold;
                return null;

            case PropertyTransferMethod.Persuasion:
                if (view.Owner.Kind is not (PropertyOwnerKind.Temple or PropertyOwnerKind.Collegium))
                    return NotPersuadable;
                if (command.PersuasionInfluenceCost <= 0)
                    return PersuasionRequiresInfluenceCost;
                if (command.BuyerId is not { Kind: PropertyOwnerKind.PlayerHousehold } buyer)
                    return PersuaderMustBePlayerHousehold;
                if (InfluenceResolver.Current(state, RuntimeId<Household>.Parse(buyer.OwnerId!)) < command.PersuasionInfluenceCost)
                    return InsufficientInfluence;
                return null;

            case PropertyTransferMethod.VoluntarySale:
            case PropertyTransferMethod.ForcedSale:
            default:
                if (command.BuyerId is not { } namedBuyer)
                    return BuyerRequired;
                if (namedBuyer == view.Owner)
                    return AlreadyOwnedByBuyer;
                return null;
        }
    }

    private static IDomainEvent[] Mutate(WorldState state, TransferPropertyCommand command)
    {
        PropertyResolver.TryResolve(state, command.Subject, out var view);
        var events = new List<IDomainEvent>();

        switch (command.Method)
        {
            case PropertyTransferMethod.AgerPublicusLease:
                {
                    var lesseeHouseholdId = RuntimeId<Household>.Parse(command.BuyerId!.Value.OwnerId!);
                    PropertyResolver.SetOwner(state, command.Subject, view.Owner, lesseeHouseholdId);
                    events.Add(new PropertyTransferredEvent(
                        state.EventIds.Issue(), command.SubmittedDate, command.Subject, command.Method, view.Owner,
                        NewOwner: null, Price: null, command.CommandId.ToTaggedString()));
                    return events.ToArray();
                }

            case PropertyTransferMethod.MarketSale:
                {
                    var price = command.NegotiatedPrice ?? RealEstateCatalog.PriceFor(view.Value, Fixed64.One - RealEstateCatalog.MarketSaleFriction);
                    PostTransfer(state, command.SubmittedDate, view.Owner, buyer: null, price, $"realEstate.marketSale:{command.Subject.SubjectId}", events);
                    RemoveFromCirculation(state, command.Subject);
                    events.Add(new PropertyTransferredEvent(
                        state.EventIds.Issue(), command.SubmittedDate, command.Subject, command.Method, view.Owner,
                        NewOwner: null, price, command.CommandId.ToTaggedString()));
                    return events.ToArray();
                }

            default:
                {
                    var buyer = command.BuyerId!.Value;
                    var price = command.NegotiatedPrice ?? view.Value;
                    PostTransfer(state, command.SubmittedDate, view.Owner, buyer, price, $"realEstate.transfer:{command.Subject.SubjectId}", events);

                    if (command.Method == PropertyTransferMethod.Persuasion)
                    {
                        var buyerHouseholdId = RuntimeId<Household>.Parse(buyer.OwnerId!);
                        InfluenceResolver.Apply(state, buyerHouseholdId, -command.PersuasionInfluenceCost);
                    }

                    PropertyResolver.SetOwner(state, command.Subject, buyer, lesseeId: null);
                    events.Add(new PropertyTransferredEvent(
                        state.EventIds.Issue(), command.SubmittedDate, command.Subject, command.Method, view.Owner,
                        buyer, price, command.CommandId.ToTaggedString()));
                    return events.ToArray();
                }
        }
    }

    /// <summary>Posts the price through the Ledger between whichever of <paramref name="seller"/>/
    /// <paramref name="buyer"/> resolve to a real tracked account (<see
    /// cref="TryLedgerAccount"/>) — a side that doesn't (a Temple, the Roman state, a Societas
    /// placeholder, an Imperial grant) settles against <see cref="LedgerAccountKey.Mint"/>, matching
    /// that key's own "the explicit, named conservation boundary... for campaign bootstrap seeding and
    /// any future minting" role, extended here to any owner kind this item cannot yet track a real
    /// ledger balance for. Posts nothing at all for a zero price or when neither side is a real
    /// account (a transaction the player's own ledger has no stake in).</summary>
    private static void PostTransfer(
        WorldState state, GameDate date, PropertyOwnerRef seller, PropertyOwnerRef? buyer, Money price,
        string reference, List<IDomainEvent> events)
    {
        if (price == Money.Zero)
            return;

        var sellerKey = TryLedgerAccount(seller, out var resolvedSeller) ? resolvedSeller : LedgerAccountKey.Mint;
        var buyerKey = buyer is { } b && TryLedgerAccount(b, out var resolvedBuyer) ? resolvedBuyer : LedgerAccountKey.Mint;
        if (sellerKey == LedgerAccountKey.Mint && buyerKey == LedgerAccountKey.Mint)
            return;

        var ledgerEvent = LedgerService.Post(
            state, date, LedgerTransactionCategory.Purchases,
            new[] { new LedgerPosting(buyerKey, -price), new LedgerPosting(sellerKey, price) },
            reference);
        events.Add(ledgerEvent);
    }

    private static bool TryLedgerAccount(PropertyOwnerRef owner, out LedgerAccountKey key)
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

    /// <summary>§9's abstract market sale: the property leaves individually tracked ownership
    /// entirely. A Plot reverts to <c>OwnerId = null</c> — Estate &amp; Settlement's own existing
    /// "unowned" state, immediately re-acquirable through <see cref="AcquirePlotCommand"/> exactly like
    /// any other raw parcel — rather than this item inventing a fictitious "the market" owner kind. A
    /// <see cref="PropertyRecord"/> (which has no "unowned" state of its own) is removed from <see
    /// cref="WorldState.PropertyRecords"/> outright: it stops being individually tracked, matching a
    /// Ship or Named Holding genuinely dissolving back into the abstract economy.</summary>
    private static void RemoveFromCirculation(WorldState state, PropertySubjectRef subject)
    {
        switch (subject.Kind)
        {
            case PropertySubjectKind.Plot:
                {
                    var plotId = subject.AsPlotId();
                    state.Plots.TryGet(plotId, out var plot);
                    state.Plots.Remove(plotId);
                    state.Plots.Add(plotId, plot with { OwnerId = null, Acquisition = null });
                    PlotPropertyResolver.Set(state, PlotPropertyExtension.Default(plotId));
                    return;
                }

            case PropertySubjectKind.PropertyRecord:
                state.PropertyRecords.Remove(subject.AsPropertyRecordId());
                return;
        }
    }
}
