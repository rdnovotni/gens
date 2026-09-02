using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.RealEstate;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Societates;

/// <summary>§7's audit action for a suspected partner, mirroring <see
/// cref="RealEstate.AuditPropertyOperatorCommand"/>'s own identical shape almost exactly (Phase 15
/// item 2): <see cref="PartnerSkimmingRiskSystem"/> already resolves the ground truth monthly, so this
/// command reveals it rather than rolling anything, and applies the one real consequence §7 (via its
/// own Operator-skimming parallel) names for the honest-partner branch — a Loyalty penalty for a false
/// accusation. Only meaningful for a partner this item can actually resolve a Character for (<see
/// cref="PartnerSkimmingRiskSystem"/>'s own doc comment) — auditing a partner kind with no resolvable
/// Character (a Rival Gens, Temple, Collegium, etc.) is rejected outright rather than silently
/// no-opping, since there is no ground truth to reveal at all.</summary>
public sealed record AuditPartnerCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Societas> SocietasId,
    PropertyOwnerRef PartnerOwner) : ICommand;

public sealed record PartnerAuditedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Societas> SocietasId,
    PropertyOwnerRef PartnerOwner,
    bool WasSkimming,
    string? CausationId) : IDomainEvent
{
    public string Type => "societates.partnerAudited";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { SocietasId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class AuditPartnerCommands
{
    public static readonly ValidationErrorCode SocietasNotFound = new("societates.audit.societasNotFound");
    public static readonly ValidationErrorCode PartnerNotFound = new("societates.audit.partnerNotFound");
    public static readonly ValidationErrorCode PartnerNotAuditable = new("societates.audit.partnerNotAuditable");

    public static readonly CommandPipeline<WorldState, AuditPartnerCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, AuditPartnerCommand command)
    {
        if (!state.Societates.TryGet(command.SocietasId, out var societas) || !societas!.IsActive)
            return SocietasNotFound;
        if (!SocietasResolver.TryGetPartner(societas, command.PartnerOwner, out _))
            return PartnerNotFound;
        if (command.PartnerOwner.Kind is not (PropertyOwnerKind.PlayerHousehold or PropertyOwnerKind.IndividualCharacter))
            return PartnerNotAuditable;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, AuditPartnerCommand command)
    {
        state.Societates.TryGet(command.SocietasId, out var societas);
        SocietasResolver.TryGetPartner(societas!, command.PartnerOwner, out var partner);

        if (!partner.IsSuspectedSkimming)
        {
            var characterId = ResolveCharacterId(state, command.PartnerOwner);
            if (characterId is { } id && state.Characters.TryGet(id, out var character))
            {
                var condition = character!.Condition;
                var loyalty = Math.Max(0, condition.Loyalty - SocietatesCatalog.FalseAuditAccusationLoyaltyPenalty);
                state.Characters.Remove(id);
                state.Characters.Add(id, character with
                {
                    Condition = new Condition(condition.Health, condition.Fatigue, loyalty, condition.Ambition, condition.Fertility),
                });
            }
        }

        return new IDomainEvent[]
        {
            new PartnerAuditedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.SocietasId, command.PartnerOwner,
                partner.IsSuspectedSkimming, command.CommandId.ToTaggedString()),
        };
    }

    private static RuntimeId<Character>? ResolveCharacterId(WorldState state, PropertyOwnerRef owner)
    {
        switch (owner.Kind)
        {
            case PropertyOwnerKind.IndividualCharacter:
                return RuntimeId<Character>.Parse(owner.OwnerId!);
            case PropertyOwnerKind.PlayerHousehold:
                return state.HouseholdHeadships.TryGet(RuntimeId<Household>.Parse(owner.OwnerId!), out var headship)
                    ? headship!.HeadCharacterId
                    : null;
            default:
                return null;
        }
    }
}
