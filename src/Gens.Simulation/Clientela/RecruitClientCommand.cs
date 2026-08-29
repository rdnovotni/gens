using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Time;

namespace Gens.Simulation.Clientela;

/// <summary>
/// Recruits a Character directly onto a household's Clientela roster (Phase 12 item 2; §4.1). §4.1
/// narratively sources a roster from "Travel encounters, Events, direct recruitment of a promoted
/// Curiales Character, or a former debtor bonded... into clientage" — this item deliberately builds
/// only the last, simplest path: one direct command that a Travel encounter, an Event's chosen option,
/// or a player action against an already-promoted Character can all submit alike (rule 2), rather than
/// each of those richer sourcing moments getting its own bespoke recruitment command. A debtor's
/// clientage-instead-of-bondage resolution (§4.1's own aside on Economy &amp; Finance §6.4) is flagged
/// there as "worth flagging as an additional resolution option" for that document's own debt-bondage
/// command to add, not something this command reaches into <see cref="Economy.DebtRecord"/> to trigger
/// itself.
/// </summary>
public sealed record RecruitClientCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> PatronHouseholdId,
    RuntimeId<Character> ClientId,
    ClientSpecialty Specialty) : ICommand;

/// <summary>Emitted whenever a <see cref="RecruitClientCommand"/> is accepted. Private to the patron's
/// head and the new client, matching <see cref="Reputation.FavorGrantedEvent"/>'s identical "a fact
/// between two named individuals, not a standing public figure" reasoning — unlike holding a
/// magistracy (<see cref="Magistracies.MagistracyAssumedEvent"/>), which office Characters are, a
/// specific patron's specific client list is not assumed common knowledge.</summary>
public sealed record ClientRecruitedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> PatronHouseholdId,
    RuntimeId<Character> PatronHeadId,
    RuntimeId<Character> ClientId,
    ClientSpecialty Specialty,
    string? CausationId) : IDomainEvent
{
    public string Type => "clientela.clientRecruited";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { PatronHeadId.ToTaggedString(), ClientId.ToTaggedString() };
    public Visibility Visibility => Visibility.Private(PatronHeadId.ToTaggedString(), ClientId.ToTaggedString());
}

/// <summary>The validate/mutate pipeline for <see cref="RecruitClientCommand"/> (ADR 0006).</summary>
public static class RecruitClientCommands
{
    public static readonly ValidationErrorCode PatronHasNoHead = new("clientela.recruitClient.patronHasNoHead");
    public static readonly ValidationErrorCode ClientNotFound = new("clientela.recruitClient.clientNotFound");
    public static readonly ValidationErrorCode ClientDeceased = new("clientela.recruitClient.clientDeceased");
    public static readonly ValidationErrorCode SelfPatronage = new("clientela.recruitClient.selfPatronage");
    public static readonly ValidationErrorCode AlreadyAClient = new("clientela.recruitClient.alreadyAClient");

    public static readonly CommandPipeline<WorldState, RecruitClientCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, RecruitClientCommand command)
    {
        if (!state.HouseholdHeadships.TryGet(command.PatronHouseholdId, out var headship))
            return PatronHasNoHead;
        if (!state.Characters.TryGet(command.ClientId, out var client))
            return ClientNotFound;
        if (!client!.IsAlive)
            return ClientDeceased;
        if (headship!.HeadCharacterId == command.ClientId)
            return SelfPatronage;
        if (state.ClientelaEntries.TryGet(command.ClientId, out _))
            return AlreadyAClient;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, RecruitClientCommand command)
    {
        state.HouseholdHeadships.TryGet(command.PatronHouseholdId, out var headship);
        var patronHeadId = headship!.HeadCharacterId;

        state.ClientelaEntries.Add(
            command.ClientId,
            new ClientelaEntry(command.ClientId, command.PatronHouseholdId, command.Specialty, command.SubmittedDate));

        ClientelaBondHelper.EstablishBond(state, patronHeadId, command.ClientId, command.SubmittedDate);

        return new IDomainEvent[]
        {
            new ClientRecruitedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.PatronHouseholdId, patronHeadId, command.ClientId,
                command.Specialty, command.CommandId.ToTaggedString()),
        };
    }
}

/// <summary>Shared relationship-web plumbing for forming or breaking the Patron/Client <see
/// cref="BondTag"/> pair between a patron's head Character and a client (Phase 12 item 2). Writes both
/// directed <see cref="Relationship"/> records directly — mirroring, rather than calling through, <see
/// cref="RecordInteractionCommand"/>'s own mutate logic, the same judgment call <see
/// cref="RelationshipDecaySystem"/> already makes for its own direct writes: this is plumbing internal
/// to the Clientela commands/systems that own the bond's lifecycle, not a player-facing "interaction"
/// in Characters §9's Interaction Catalog sense.</summary>
internal static class ClientelaBondHelper
{
    public static void EstablishBond(WorldState state, RuntimeId<Character> patronHeadId, RuntimeId<Character> clientId, GameDate date)
    {
        AddBond(state, patronHeadId, clientId, BondTag.Client, date);
        AddBond(state, clientId, patronHeadId, BondTag.Patron, date);
    }

    public static void BreakBond(WorldState state, RuntimeId<Character> patronHeadId, RuntimeId<Character> clientId, GameDate date)
    {
        RemoveBond(state, patronHeadId, clientId, BondTag.Client, date);
        RemoveBond(state, clientId, patronHeadId, BondTag.Patron, date);
    }

    /// <summary>Applies a signed opinion delta to the directed relationship from <paramref
    /// name="fromId"/> toward <paramref name="toId"/>, creating the record on first contact if needed —
    /// the opinion-only half of <see cref="AddBond"/>/<see cref="RemoveBond"/>'s shared shape, used by
    /// <see cref="CallInClientFavorCommand"/> for §4.2's overdrawn-favor opinion cost.</summary>
    public static int AdjustOpinion(WorldState state, RuntimeId<Character> fromId, RuntimeId<Character> toId, int delta, GameDate date)
    {
        var key = new RelationshipKey(fromId, toId);
        var exists = state.Relationships.TryGet(key, out var existing);
        var before = exists ? existing.Opinion : 0;
        var after = Math.Clamp(before + delta, Relationship.MinOpinion, Relationship.MaxOpinion);
        var bonds = exists ? existing.Bonds : BondTag.None;
        var origin = exists ? existing.Origin : RelationshipOrigin.Political;
        var formedDate = exists ? existing.FormedDate : date;

        if (exists)
            state.Relationships.Remove(key);
        state.Relationships.Add(key, new Relationship(after, bonds, origin, formedDate, date, provenanceEventId: null));
        return after;
    }

    private static void AddBond(WorldState state, RuntimeId<Character> fromId, RuntimeId<Character> toId, BondTag bond, GameDate date)
    {
        var key = new RelationshipKey(fromId, toId);
        var exists = state.Relationships.TryGet(key, out var existing);
        var opinion = exists ? existing.Opinion : 0;
        var bonds = (exists ? existing.Bonds : BondTag.None) | bond;
        var formedDate = exists ? existing.FormedDate : date;

        if (exists)
            state.Relationships.Remove(key);
        state.Relationships.Add(key, new Relationship(opinion, bonds, RelationshipOrigin.Political, formedDate, date, provenanceEventId: null));
    }

    private static void RemoveBond(WorldState state, RuntimeId<Character> fromId, RuntimeId<Character> toId, BondTag bond, GameDate date)
    {
        var key = new RelationshipKey(fromId, toId);
        if (!state.Relationships.TryGet(key, out var existing))
            return;

        var bonds = existing.Bonds & ~bond;
        state.Relationships.Remove(key);
        var updated = new Relationship(existing.Opinion, bonds, existing.Origin, existing.FormedDate, date, existing.ProvenanceEventId);
        if (!updated.IsEmpty)
            state.Relationships.Add(key, updated);
    }
}
