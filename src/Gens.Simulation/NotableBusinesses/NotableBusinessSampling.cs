using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.MerchantFamilies;
using Gens.Simulation.RealEstate;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.NotableBusinesses;

/// <summary>§3's single promotion door (Phase 15 item 4) — turns one member of the (unstored,
/// uncounted) ambient commerce pool into a real, individually-tracked <see cref="NotableBusiness"/>,
/// mirroring <c>Wanderers.InstantiateWandererCommand</c>'s own identical "§8's single promotion door"
/// role for that domain's own aggregate-to-named sampling. Every real trigger §3 names (see <see
/// cref="NotableBusinessTrigger"/>) is recorded via <see
/// cref="NotableBusinessTrigger"/>; whichever caller actually detects one (a Legal &amp; Court filing, a
/// Scandal, a Government Contract grant, a direct player transaction) submits this command with that
/// trigger named.</summary>
public sealed record PromoteNotableBusinessCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    string Name,
    PropertyOwnerRef Owner,
    NotableBusinessTrigger Trigger,
    DefinitionId<Good>? OutputGoodId = null,
    RuntimeId<PropertyRecord>? LinkedPropertyRecordId = null,
    RuntimeId<District>? DistrictId = null) : ICommand;

public sealed record NotableBusinessPromotedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<NotableBusiness> BusinessId,
    string Name,
    PropertyOwnerRef Owner,
    NotableBusinessTrigger Trigger,
    string? CausationId) : IDomainEvent
{
    public string Type => "notableBusinesses.businessPromoted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { BusinessId.ToTaggedString(), Owner.ToTaggedOwnerId() };
    public Visibility Visibility => Visibility.Public;
}

public static class PromoteNotableBusinessCommands
{
    public static readonly ValidationErrorCode EmptyName = new("notableBusinesses.promote.emptyName");
    public static readonly ValidationErrorCode InvalidOwnerKind = new("notableBusinesses.promote.invalidOwnerKind");
    public static readonly ValidationErrorCode OwnerNotFound = new("notableBusinesses.promote.ownerNotFound");
    public static readonly ValidationErrorCode PropertyRecordNotFound = new("notableBusinesses.promote.propertyRecordNotFound");
    public static readonly ValidationErrorCode DistrictNotFound = new("notableBusinesses.promote.districtNotFound");

    public static readonly CommandPipeline<WorldState, PromoteNotableBusinessCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, PromoteNotableBusinessCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            return EmptyName;

        if (command.Owner.Kind is not (PropertyOwnerKind.PlayerHousehold or PropertyOwnerKind.RivalGens or PropertyOwnerKind.IndividualCharacter))
            return InvalidOwnerKind;

        if (command.Owner.Kind == PropertyOwnerKind.RivalGens && !state.Actors.TryGet(RuntimeId<Actor>.Parse(command.Owner.OwnerId!), out _))
            return OwnerNotFound;

        if (command.Owner.Kind == PropertyOwnerKind.IndividualCharacter)
        {
            if (!state.Characters.TryGet(RuntimeId<Character>.Parse(command.Owner.OwnerId!), out var character) || !character!.IsAlive)
                return OwnerNotFound;
        }

        if (command.LinkedPropertyRecordId is { } propertyRecordId && !state.PropertyRecords.TryGet(propertyRecordId, out _))
            return PropertyRecordNotFound;

        if (command.DistrictId is { } districtId && !state.Districts.TryGet(districtId, out _))
            return DistrictNotFound;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, PromoteNotableBusinessCommand command)
    {
        var id = state.NotableBusinessIds.Issue();
        var business = NotableBusiness.Create(
            id, command.Name, command.Owner, command.Trigger, command.SubmittedDate,
            command.OutputGoodId, command.LinkedPropertyRecordId, command.DistrictId);
        state.NotableBusinesses.Add(id, business);

        return new IDomainEvent[]
        {
            new NotableBusinessPromotedEvent(
                state.EventIds.Issue(), command.SubmittedDate, id, command.Name, command.Owner, command.Trigger,
                command.CommandId.ToTaggedString()),
        };
    }
}

/// <summary>The shared Tracked↔Demoted promotion/demotion rule for an already-created <see
/// cref="NotableBusiness"/> (Phase 15 item 4), mirroring <see
/// cref="LivingWorldActorTieringService"/>'s identical shape — record real relevance the instant it
/// occurs, and freeze a quiet business back to <see cref="NotableBusinessStatus.Demoted"/> once it has
/// gone <see cref="NotableBusinessesCatalog.DemotionQuietPeriodMonths"/> without one. A plain static
/// utility, not an <see cref="ICommand"/>, matching that service's own identical "the underlying mover
/// other commands call" role rather than a redundant command envelope around a field write.</summary>
public static class NotableBusinessTieringService
{
    /// <summary>Records real relevance and, if <paramref name="businessId"/> is currently <see
    /// cref="NotableBusinessStatus.Demoted"/>, re-promotes it in the same step. Idempotent on an
    /// already-Tracked business: it simply refreshes <see
    /// cref="NotableBusiness.LastRelevantContactDate"/>, which is exactly what keeps a live thread from
    /// demoting under <see cref="DemoteIfQuiet"/>.</summary>
    public static NotableBusiness RecordContactAndPromote(WorldState state, RuntimeId<NotableBusiness> businessId, GameDate contactDate)
    {
        var business = GetOrThrow(state, businessId);
        var updated = business with { Status = NotableBusinessStatus.Tracked, LastRelevantContactDate = contactDate };
        Replace(state, businessId, updated);
        return updated;
    }

    /// <summary>Freezes <paramref name="businessId"/> back to <see cref="NotableBusinessStatus.Demoted"/>
    /// if it is currently <see cref="NotableBusinessStatus.Tracked"/> and has gone at least <see
    /// cref="NotableBusinessesCatalog.DemotionQuietPeriodMonths"/> since its last recorded relevant
    /// contact. A no-op, returning the business unchanged, otherwise.</summary>
    public static NotableBusiness DemoteIfQuiet(WorldState state, RuntimeId<NotableBusiness> businessId, GameDate currentDate)
    {
        var business = GetOrThrow(state, businessId);
        if (business.Status != NotableBusinessStatus.Tracked)
            return business;

        var monthsSinceContact = currentDate.TotalMonths - business.LastRelevantContactDate.TotalMonths;
        if (monthsSinceContact < NotableBusinessesCatalog.DemotionQuietPeriodMonths)
            return business;

        var updated = business with { Status = NotableBusinessStatus.Demoted };
        Replace(state, businessId, updated);
        return updated;
    }

    private static NotableBusiness GetOrThrow(WorldState state, RuntimeId<NotableBusiness> businessId) =>
        state.NotableBusinesses.TryGet(businessId, out var business)
            ? business!
            : throw new ArgumentException($"No Notable Business '{businessId}' is registered.", nameof(businessId));

    private static void Replace(WorldState state, RuntimeId<NotableBusiness> businessId, NotableBusiness updated)
    {
        state.NotableBusinesses.Remove(businessId);
        state.NotableBusinesses.Add(businessId, updated);
    }
}
