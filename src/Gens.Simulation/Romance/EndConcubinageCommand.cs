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

/// <summary>Ends an active Concubinage, no-fault (<c>gens-romance-sexuality-lineage-design.md</c> §6:
/// Concubinage is "never itself subject to Divorce... simply ends"). Either party may submit this
/// command — unlike ending a marriage, there is no wronged-party asymmetry, no property settlement, and
/// no Legal &amp; Court case type this routes through.</summary>
public sealed record EndConcubinageCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterAId,
    RuntimeId<Character> CharacterBId) : ICommand;

/// <summary>Emitted whenever an <see cref="EndConcubinageCommand"/> is accepted. Public, mirroring <see
/// cref="ConcubinageEstablishedEvent"/>'s own visibility — the relationship was publicly acknowledged
/// while active, and its ending is not hidden either.</summary>
public sealed record ConcubinageEndedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterAId,
    RuntimeId<Character> CharacterBId,
    string? CausationId) : IDomainEvent
{
    public string Type => "romance.concubinageEnded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterAId.ToTaggedString(), CharacterBId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="EndConcubinageCommand"/> (ADR 0006).</summary>
public static class EndConcubinageCommands
{
    public static readonly ValidationErrorCode NoActiveConcubinage = new("romance.endConcubinage.noActiveConcubinage");

    public static readonly CommandPipeline<WorldState, EndConcubinageCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, EndConcubinageCommand command)
    {
        var eligibilityError = RomanceEligibility.CheckPair(state, command.CharacterAId, command.CharacterBId, command.SubmittedDate);
        if (eligibilityError is { } code)
            return code;

        var key = RomanticBondKey.Create(command.CharacterAId, command.CharacterBId);
        if (!state.RomanticBonds.TryGet(key, out var bond) || bond.BondType != RomanticBondType.Concubinage)
            return NoActiveConcubinage;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, EndConcubinageCommand command)
    {
        var key = RomanticBondKey.Create(command.CharacterAId, command.CharacterBId);
        state.RomanticBonds.TryGet(key, out var bond);

        // Direct read-remove-re-add, matching RecordRomanticInteractionCommand's own idiom exactly
        // (§6: retag rather than delete, so an ended Concubinage remains distinguishable on record from a
        // pair that never had one, the same reasoning RomanticBondDecaySystem already applies to a cooled
        // Courtship or Affair).
        var ended = new RomanticBond(
            RomanticBondType.PastRelationship, bond.Affection, bond.Attraction, bond.IsKnownPublicly, bond.DiscoveryRisk,
            bond.FormedDate, command.SubmittedDate, bond.ProvenanceEventId);
        state.RomanticBonds.Remove(key);
        state.RomanticBonds.Add(key, ended);

        // No BondTag.Concubine revocation: EstablishConcubinageCommand never grants that tag in the first
        // place (see its own doc comment for why), so there is nothing here to remove.

        return new IDomainEvent[]
        {
            new ConcubinageEndedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CharacterAId, command.CharacterBId,
                command.CommandId.ToTaggedString()),
        };
    }
}
