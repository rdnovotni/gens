using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Clientela;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Collegia;

/// <summary>
/// §4's Patron Relationship: a household sponsors a Collegium, gaining real Dignitas and Influence —
/// "an efficient alternative path to political capital." Full parity with §4's own headline "an entire
/// bloc of grateful clients acquired in one relationship" is a deliberate, named cut: that would need
/// this command to resolve every one of the collegium's own <see
/// cref="CollegiumDetails.MemberHouseholdIds"/> down to an already-generated head Character and run each
/// through <see cref="RecruitClientCommand"/> in bulk, the same "no principled way to supply a lazily-
/// generated head on someone else's behalf" gap <see cref="Clientela.ClientPoachingSystem"/>'s own doc
/// comment already names for a rival Actor's head. What this command builds instead is the one real,
/// concrete tie §4 actually guarantees regardless of membership depth: once the collegium has a resolved
/// Magister (<see cref="CollegiumResolver.MagisterCharacterId"/>), the patron's own household head forms
/// a real Patron/Client <see cref="BondTag"/> pair with that one Character directly — "a genuine, mutual,
/// and visible arrangement," reusing the same bond vocabulary Clientela's own patron-client tie already
/// established rather than inventing a parallel one.
/// </summary>
public sealed record SponsorCollegiumCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Actor> CollegiumId,
    RuntimeId<Household> PatronHouseholdId) : ICommand;

/// <summary>Emitted whenever a <see cref="SponsorCollegiumCommand"/> is accepted. Public — per §4, "a
/// collegium patron's own name is displayed with real pride by the collegium itself," a genuinely
/// visible arrangement rather than a private favor.</summary>
public sealed record CollegiumSponsoredEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Actor> CollegiumId,
    RuntimeId<Household> PatronHouseholdId,
    string? CausationId) : IDomainEvent
{
    public string Type => "collegia.sponsored";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CollegiumId.ToTaggedString(), PatronHouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="SponsorCollegiumCommand"/> (ADR 0006).</summary>
public static class SponsorCollegiumCommands
{
    public static readonly ValidationErrorCode CollegiumNotFound = new("collegia.sponsor.collegiumNotFound");
    public static readonly ValidationErrorCode PatronHasNoHead = new("collegia.sponsor.patronHasNoHead");
    public static readonly ValidationErrorCode AlreadySponsored = new("collegia.sponsor.alreadySponsored");

    public static readonly CommandPipeline<WorldState, SponsorCollegiumCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, SponsorCollegiumCommand command)
    {
        if (!state.Collegia.TryGet(command.CollegiumId, out var details))
            return CollegiumNotFound;
        if (!state.HouseholdHeadships.TryGet(command.PatronHouseholdId, out _))
            return PatronHasNoHead;
        if (details!.PatronHouseholdId is not null)
            return AlreadySponsored;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, SponsorCollegiumCommand command)
    {
        state.Collegia.TryGet(command.CollegiumId, out var details);
        state.Collegia.Remove(command.CollegiumId);
        state.Collegia.Add(command.CollegiumId, details! with { PatronHouseholdId = command.PatronHouseholdId });

        var events = new List<IDomainEvent>();

        events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
            state, new AdjustDignitasCommand(
                state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                command.PatronHouseholdId, CollegiumCatalog.SponsorshipDignitasGrant,
                $"sponsored collegium {command.CollegiumId.ToTaggedString()}")).Events);

        InfluenceResolver.Apply(state, command.PatronHouseholdId, CollegiumCatalog.SponsorshipInfluenceGrant);

        if (CollegiumResolver.MagisterCharacterId(state, command.CollegiumId) is { } magisterId)
        {
            state.HouseholdHeadships.TryGet(command.PatronHouseholdId, out var headship);
            var patronHeadId = headship!.HeadCharacterId;
            EstablishPatronBond(state, patronHeadId, magisterId, command.SubmittedDate);
        }

        events.Add(new CollegiumSponsoredEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.CollegiumId, command.PatronHouseholdId,
            command.CommandId.ToTaggedString()));

        return events.ToArray();
    }

    /// <summary>Writes the Patron/Client <see cref="BondTag"/> pair directly, mirroring <see
    /// cref="Clientela.RecruitClientCommand"/>'s own internal <c>ClientelaBondHelper</c> shape rather
    /// than calling through it — this is a Collegia-owned bond (patron head to Magister), not a Clientela
    /// roster entry, so it writes the relationship-web tags without also adding a <see
    /// cref="Clientela.ClientelaEntry"/>.</summary>
    private static void EstablishPatronBond(WorldState state, RuntimeId<Character> patronHeadId, RuntimeId<Character> magisterId, GameDate date)
    {
        if (patronHeadId == magisterId)
            return;

        AddBond(state, patronHeadId, magisterId, BondTag.Client, date);
        AddBond(state, magisterId, patronHeadId, BondTag.Patron, date);
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
}
