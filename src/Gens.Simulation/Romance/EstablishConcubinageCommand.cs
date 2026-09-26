using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Romance;

/// <summary>Establishes Concubinage between two eligible Characters
/// (<c>gens-romance-sexuality-lineage-design.md</c> §6: "sits between marriage and an affair — a real,
/// legally-recognized relationship... but without the legal weight of marriage"). Deliberately carries no
/// dowry/Money parameter at all — §17's Open Questions settles the "Concubinage-dowry interaction"
/// question as "No dowry exchange implemented," and this type's absence of a Money field is itself that
/// decision made structurally impossible to violate by accident, not merely undocumented.
///
/// Either party may already be independently married to someone else — §6's own motivating case (a
/// widower, or a marriage of political convenience alongside a separately chosen Concubinage partner)
/// assumes exactly this is allowed. The only duplicate this command rejects is an already-existing <see
/// cref="RomanticBondType.Concubinage"/> bond between this exact pair.</summary>
public sealed record EstablishConcubinageCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterAId,
    RuntimeId<Character> CharacterBId) : ICommand;

/// <summary>Emitted whenever an <see cref="EstablishConcubinageCommand"/> is accepted. Public — unlike an
/// undiscovered Affair, Concubinage is §6's "publicly acknowledged" relationship from the moment it
/// begins, so there is no private phase for this event to respect.</summary>
public sealed record ConcubinageEstablishedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterAId,
    RuntimeId<Character> CharacterBId,
    string? CausationId) : IDomainEvent
{
    public string Type => "romance.concubinageEstablished";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterAId.ToTaggedString(), CharacterBId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="EstablishConcubinageCommand"/> (ADR 0006).</summary>
public static class EstablishConcubinageCommands
{
    public static readonly ValidationErrorCode AlreadyConcubines = new("romance.establishConcubinage.alreadyConcubines");

    public static readonly CommandPipeline<WorldState, EstablishConcubinageCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, EstablishConcubinageCommand command)
    {
        var eligibilityError = RomanceEligibility.CheckPair(state, command.CharacterAId, command.CharacterBId, command.SubmittedDate);
        if (eligibilityError is { } code)
            return code;

        var key = RomanticBondKey.Create(command.CharacterAId, command.CharacterBId);
        if (state.RomanticBonds.TryGet(key, out var existing) && existing.BondType == RomanticBondType.Concubinage)
            return AlreadyConcubines;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, EstablishConcubinageCommand command)
    {
        var events = new List<IDomainEvent>(RecordRomanticInteractionCommands.Pipeline.Execute(
            state,
            new RecordRomanticInteractionCommand(
                state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                command.CharacterAId, command.CharacterBId, AffectionDelta: 0, AttractionDelta: 0,
                BondTypeOverride: RomanticBondType.Concubinage)).Events);

        // RecordRomanticInteractionCommand has no IsKnownPublicly parameter of its own (it always
        // preserves whatever the bond already had, defaulting false for a newly-created one) — §6's
        // "publicly acknowledged... unlike an affair" is a Concubinage-specific fact this command applies
        // directly, immediately after the sub-command call, via its own read-remove-re-add on the bond
        // it just wrote.
        var key = RomanticBondKey.Create(command.CharacterAId, command.CharacterBId);
        state.RomanticBonds.TryGet(key, out var bond);
        var publiclyKnown = new RomanticBond(
            bond.BondType, bond.Affection, bond.Attraction, isKnownPublicly: true, bond.DiscoveryRisk,
            bond.FormedDate, bond.LastMeaningfulInteractionDate, bond.ProvenanceEventId);
        state.RomanticBonds.Remove(key);
        state.RomanticBonds.Add(key, publiclyKnown);

        // No BondTag.Concubine grant here: RecordMarriageCommand — the structural precedent this command
        // follows for "does the formal-relationship command also touch the ordinary relationship-web bond
        // tag" — does not itself grant BondTag.Spouse anywhere (Character.CurrentSpouseId, derived from
        // MaritalHistory, is that fact's real source of truth). Matching that precedent, this command
        // creates the real RomanticBond and stops there rather than inventing a BondTag.Concubine grant
        // this codebase's own analogous command does not make for its own tag.
        events.Add(new ConcubinageEstablishedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.CharacterAId, command.CharacterBId,
            command.CommandId.ToTaggedString()));

        return events.ToArray();
    }
}
