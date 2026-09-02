using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.RealEstate;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.MerchantFamilies;

/// <summary>
/// §4/§7/§9's real, deliberate act of a household actually taking on a Merchant House character (Phase
/// 15 item 3) — creating (or replacing, on a household's own later re-invention) its <see
/// cref="MerchantHouseArchetype"/>. Restricted to <see cref="PropertyOwnerKind.PlayerHousehold"/> and
/// <see cref="PropertyOwnerKind.RivalGens"/> — the two owner kinds §7's own Rival Houses extension and
/// §8's own Domus Mercatoria cross-integration actually name (see <see
/// cref="MerchantHouseArchetype"/>'s own doc comment); every other owner kind (an Individual Character,
/// a Temple, a Collegium, etc.) has no Background/Notable framework entry or player household doctrine
/// slot for this item to attach a merchant character to.
/// </summary>
public sealed record DesignateMerchantHouseCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    PropertyOwnerRef Owner,
    MerchantHouseType MerchantType,
    TradeScaleTier WholesaleOrRetailTier) : ICommand;

public sealed record MerchantHouseDesignatedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    PropertyOwnerRef Owner,
    MerchantHouseType MerchantType,
    TradeScaleTier WholesaleOrRetailTier,
    string? CausationId) : IDomainEvent
{
    public string Type => "merchantFamilies.merchantHouseDesignated";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { Owner.ToTaggedOwnerId() };
    public Visibility Visibility => Visibility.Public;
}

public static class DesignateMerchantHouseCommands
{
    public static readonly ValidationErrorCode InvalidOwnerKind = new("merchantFamilies.designate.invalidOwnerKind");
    public static readonly ValidationErrorCode OwnerNotFound = new("merchantFamilies.designate.ownerNotFound");

    public static readonly CommandPipeline<WorldState, DesignateMerchantHouseCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, DesignateMerchantHouseCommand command)
    {
        if (command.Owner.Kind is not (PropertyOwnerKind.PlayerHousehold or PropertyOwnerKind.RivalGens))
            return InvalidOwnerKind;

        if (command.Owner.Kind == PropertyOwnerKind.RivalGens
            && !state.Actors.TryGet(RuntimeId<Actor>.Parse(command.Owner.OwnerId!), out _))
            return OwnerNotFound;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, DesignateMerchantHouseCommand command)
    {
        var key = command.Owner.ToTaggedOwnerId();
        if (state.MerchantHouseArchetypes.TryGet(key, out _))
            state.MerchantHouseArchetypes.Remove(key);
        state.MerchantHouseArchetypes.Add(
            key, new MerchantHouseArchetype(command.Owner, command.MerchantType, command.WholesaleOrRetailTier));

        return new IDomainEvent[]
        {
            new MerchantHouseDesignatedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.Owner, command.MerchantType,
                command.WholesaleOrRetailTier, command.CommandId.ToTaggedString()),
        };
    }
}
