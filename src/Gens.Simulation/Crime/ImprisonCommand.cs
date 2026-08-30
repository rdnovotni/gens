using Gens.Simulation.Characters;
using Gens.Simulation.Clientela;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Legal;
using Gens.Simulation.Magistracies;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Crime;

/// <summary>§4's real authority bases — "anyone reachable, any time," gated only by which real tie the
/// actor actually has to the target. <see cref="PatriaPotestas"/> and <see
/// cref="ClientelaAuthority"/> are this item's own two directly reachable bases (Familia's household
/// headship and Phase 12 item 2's Clientela roster, both already shipped). <see
/// cref="MagisterialJurisdiction"/> is a third real, reachable one this item adds on its own initiative:
/// §4 names "a sitting magistrate act[ing] outside a formal Hearing" as real authority, and Phase 12
/// item 2's Local Magistracies already ships a real <see cref="MagistracyRecord"/> a holder can be
/// checked against. §4's own remaining two bases — a Military &amp; Combat/Piracy &amp; Banditry
/// captive — are deliberately omitted from the enum entirely rather than included-but-unreachable,
/// since neither domain exists anywhere in this codebase yet (both Phase 16), matching <see
/// cref="Legal.LegalCase.CaseType"/>'s own "omitted rather than included-but-unreachable" precedent for
/// an unbuilt caller.</summary>
public enum ImprisonAuthorityBasis
{
    PatriaPotestas,
    ClientelaAuthority,
    MagisterialJurisdiction,
}

/// <summary>
/// §4's Imprison action: broadly available, targetable at anyone the actor holds real authority over,
/// resolved through §3's Justified/Unjust lens rather than gated on a formal verdict. Always resolves
/// into a new active <see cref="DetentionRecord"/> — the further real, reachable outcomes §7/§8/§10 name
/// (a sentence carried out, a ransom negotiated, a mercy release) are separate commands a caller applies
/// afterward against that same Detention, rather than this command trying to pre-decide them.
/// </summary>
public sealed record ImprisonCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> ActorCharacterId,
    RuntimeId<Character> TargetCharacterId,
    ImprisonAuthorityBasis AuthorityBasis,
    DetentionLocationType LocationType,
    RuntimeId<LegalCase>? LinkedLegalCaseId = null) : ICommand;

/// <summary>Emitted whenever an <see cref="ImprisonCommand"/> is accepted. Public — §4's whole point is
/// that "everyone is watching how you use power," so an Imprison (justified or not) is never a private
/// fact, the same reasoning <see cref="Legal.LegalCaseRuledEvent"/> already established for a
/// verdict.</summary>
public sealed record CharacterImprisonedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> ActorCharacterId,
    RuntimeId<Character> TargetCharacterId,
    RuntimeId<DetentionRecord> DetentionId,
    ImprisonAuthorityBasis AuthorityBasis,
    bool Justified,
    string? CausationId) : IDomainEvent
{
    public string Type => "crime.characterImprisoned";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { ActorCharacterId.ToTaggedString(), TargetCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="ImprisonCommand"/> (ADR 0006).</summary>
public static class ImprisonCommands
{
    public static readonly ValidationErrorCode SameCharacter = new("crime.imprison.sameCharacter");
    public static readonly ValidationErrorCode ActorNotFound = new("crime.imprison.actorNotFound");
    public static readonly ValidationErrorCode TargetNotFound = new("crime.imprison.targetNotFound");
    public static readonly ValidationErrorCode ActorDeceased = new("crime.imprison.actorDeceased");
    public static readonly ValidationErrorCode TargetDeceased = new("crime.imprison.targetDeceased");
    public static readonly ValidationErrorCode AlreadyDetained = new("crime.imprison.alreadyDetained");
    public static readonly ValidationErrorCode NoRealAuthority = new("crime.imprison.noRealAuthority");

    public static readonly CommandPipeline<WorldState, ImprisonCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, ImprisonCommand command)
    {
        if (command.ActorCharacterId == command.TargetCharacterId)
            return SameCharacter;
        if (!state.Characters.TryGet(command.ActorCharacterId, out var actor))
            return ActorNotFound;
        if (!state.Characters.TryGet(command.TargetCharacterId, out var target))
            return TargetNotFound;
        if (!actor!.IsAlive)
            return ActorDeceased;
        if (!target!.IsAlive)
            return TargetDeceased;
        if (DetentionResolver.ActiveFor(state, command.TargetCharacterId) is not null)
            return AlreadyDetained;
        if (!HasRealAuthority(state, command, target))
            return NoRealAuthority;

        return null;
    }

    /// <summary>§4's authority check, one real tie per <see cref="ImprisonAuthorityBasis"/> value — see
    /// that enum's own doc comment for which bases this item actually reaches.</summary>
    private static bool HasRealAuthority(WorldState state, ImprisonCommand command, Character target) =>
        command.AuthorityBasis switch
        {
            ImprisonAuthorityBasis.PatriaPotestas =>
                target.Household is { } householdId &&
                state.HouseholdHeadships.TryGet(householdId, out var headship) &&
                headship!.HeadCharacterId == command.ActorCharacterId,

            ImprisonAuthorityBasis.ClientelaAuthority =>
                ClientelaResolver.TryGetClient(state, command.TargetCharacterId, out var entry) &&
                state.HouseholdHeadships.TryGet(entry.PatronHouseholdId, out var patronHeadship) &&
                patronHeadship!.HeadCharacterId == command.ActorCharacterId,

            ImprisonAuthorityBasis.MagisterialJurisdiction => HoldsActiveOfficeAtTargetSettlement(state, command),

            _ => false,
        };

    private static bool HoldsActiveOfficeAtTargetSettlement(WorldState state, ImprisonCommand command)
    {
        if (!state.Characters.TryGet(command.TargetCharacterId, out var target))
            return false;

        foreach (var entry in state.MagistracyRecords.InAscendingOrder())
        {
            var record = entry.Value;
            if (MagistracyResolver.IsActive(record) && record.HolderId == command.ActorCharacterId &&
                record.SettlementId == target!.Location)
                return true;
        }

        return false;
    }

    private static IDomainEvent[] Mutate(WorldState state, ImprisonCommand command)
    {
        var justified = PunishableOffenseResolver.HasActiveOffense(state, command.TargetCharacterId);

        var detentionId = state.DetentionRecordIds.Issue();
        state.DetentionRecords.Add(
            detentionId,
            new DetentionRecord(
                detentionId, command.TargetCharacterId, command.LocationType, command.SubmittedDate,
                justified, command.LinkedLegalCaseId));

        var events = new List<IDomainEvent>();

        if (justified)
        {
            events.AddRange(RecordInteractionCommands.Pipeline.Execute(
                state, new RecordInteractionCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    command.ActorCharacterId, command.TargetCharacterId, -CrimeCatalog.JustifiedImprisonOpinionPenalty,
                    BondTag.None, BondTag.None, RelationshipOrigin.Political)).Events);
        }
        else
        {
            if (state.Characters.TryGet(command.ActorCharacterId, out var actor) && actor!.Household is { } actorHouseholdId)
            {
                events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
                    state, new AdjustDignitasCommand(
                        state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                        actorHouseholdId, -CrimeCatalog.UnjustImprisonDignitasPenalty,
                        $"unjust imprisonment of {command.TargetCharacterId.ToTaggedString()}")).Events);
            }

            events.AddRange(RecordInteractionCommands.Pipeline.Execute(
                state, new RecordInteractionCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    command.ActorCharacterId, command.TargetCharacterId, -CrimeCatalog.UnjustImprisonOpinionPenalty,
                    BondTag.Rival, BondTag.None, RelationshipOrigin.Political)).Events);
        }

        events.Add(new CharacterImprisonedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.ActorCharacterId, command.TargetCharacterId,
            detentionId, command.AuthorityBasis, justified, command.CommandId.ToTaggedString()));

        return events.ToArray();
    }
}
