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

/// <summary>Turns a genuine love match into a formal marriage
/// (<c>gens-romance-sexuality-lineage-design.md</c> §4: "Courtship... can culminate in marriage"). Requires
/// both an existing <see cref="RomanticBond"/> that clears <see cref="RomanceCatalog.LoveMatchThreshold"/>
/// on <i>both</i> Affection and Attraction, and that neither party is already married — this command does
/// not create a bond itself, it only recognizes one that has already earned the threshold through prior
/// <see cref="RecordCourtshipInteractionCommand"/>/autonomous-romance activity. Delegates the actual
/// marriage to the existing <see cref="Characters.RecordMarriageCommand"/> pipeline rather than
/// duplicating its own-spouse/liveness bookkeeping, then flips the bond's type to <see
/// cref="RomanticBondType.Marriage"/>.
///
/// The roadmap's own item summary line mentions "Elope" as part of this feature — the FINAL design
/// document's own body has no separate elopement mechanic, so this is a deliberate simplification:
/// "eloping" is flavor text for exactly this same command (a lower-ceremony marriage still routes through
/// the identical validated path), not a second command or a bypass of any of its checks.
///
/// <i>Manus vs. sine manu</i> (§4.1's "what actually made a marriage" distinction under real Roman law) is
/// explicit, out-of-scope future Familia-only work — this command has no field for it and does not derive
/// it.</summary>
public sealed record ProposeMarriageCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    RuntimeId<Character> SpouseId) : ICommand;

/// <summary>Emitted whenever a <see cref="ProposeMarriageCommand"/> is accepted, in addition to the
/// <see cref="Characters.CharactersMarriedEvent"/> and <see cref="RomanticInteractionRecordedEvent"/> the
/// two composed sub-commands themselves emit (merged into this command's own result, matching <see
/// cref="Scandal.RecordScandalCommand"/>'s "merge sub-events, then add my own" idiom) — the event a caller
/// interested specifically in "this marriage was a recognized Romance love match" should read, distinct
/// from an arranged marriage recorded directly through <see cref="Characters.RecordMarriageCommand"/> with
/// no preceding <see cref="RomanticBond"/> at all. Public, matching <see
/// cref="Characters.CharactersMarriedEvent"/>'s own visibility — a marriage is not a private fact.</summary>
public sealed record MarriageProposalAcceptedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    RuntimeId<Character> SpouseId,
    string? CausationId) : IDomainEvent
{
    public string Type => "romance.marriageProposalAccepted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString(), SpouseId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="ProposeMarriageCommand"/> (ADR 0006).</summary>
public static class ProposeMarriageCommands
{
    public static readonly ValidationErrorCode AlreadyMarried = new("romance.proposeMarriage.alreadyMarried");

    /// <summary>No existing <see cref="RomanticBond"/> between the pair, or one that exists but has not
    /// cleared <see cref="RomanceCatalog.LoveMatchThreshold"/> on both Affection and Attraction — §4's
    /// "a genuine love match, not merely a warm-enough one" (see <see
    /// cref="RomanceCatalog.LoveMatchThreshold"/>'s own doc comment).</summary>
    public static readonly ValidationErrorCode InsufficientBond = new("romance.proposeMarriage.insufficientBond");

    public static readonly CommandPipeline<WorldState, ProposeMarriageCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, ProposeMarriageCommand command)
    {
        var eligibilityError = RomanceEligibility.CheckPair(state, command.CharacterId, command.SpouseId, command.SubmittedDate);
        if (eligibilityError is { } code)
            return code;

        state.Characters.TryGet(command.CharacterId, out var character);
        state.Characters.TryGet(command.SpouseId, out var spouse);
        if (character.CurrentSpouseId is not null || spouse.CurrentSpouseId is not null)
            return AlreadyMarried;

        // Duplicated here, not just left to the composed RecordMarriageCommand's own Validate, so this
        // command's own CommandResult.Accepted reflects reality — CommandPipeline<TState,TCommand> has no
        // mechanism for an outer command to un-accept itself after a composed sub-command's Mutate-time
        // Validate silently rejects, so this precondition must already hold before Mutate ever calls it
        // (§5's exclusion; see RecordMarriageCommand's own doc comment for the substantive rule this
        // enforces).
        if (character.Sex == spouse.Sex)
            return RecordMarriageCommands.SameSexNotSupported;

        var key = RomanticBondKey.Create(command.CharacterId, command.SpouseId);
        if (!state.RomanticBonds.TryGet(key, out var bond)
            || bond.Affection < RomanceCatalog.LoveMatchThreshold
            || bond.Attraction < RomanceCatalog.LoveMatchThreshold)
            return InsufficientBond;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, ProposeMarriageCommand command)
    {
        var events = new List<IDomainEvent>(RecordMarriageCommands.Pipeline.Execute(
            state,
            new RecordMarriageCommand(
                state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                command.CharacterId, command.SpouseId)).Events);

        events.AddRange(RecordRomanticInteractionCommands.Pipeline.Execute(
            state,
            new RecordRomanticInteractionCommand(
                state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                command.CharacterId, command.SpouseId, AffectionDelta: 0, AttractionDelta: 0,
                BondTypeOverride: RomanticBondType.Marriage)).Events);

        events.Add(new MarriageProposalAcceptedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.CharacterId, command.SpouseId,
            command.CommandId.ToTaggedString()));

        return events.ToArray();
    }
}
