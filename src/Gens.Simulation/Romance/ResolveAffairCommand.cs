using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Legal;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Romance;

/// <summary>Emitted whenever a <see cref="ResolveAffairCommand"/> is accepted, regardless of which <see
/// cref="AffairResolution"/> it resolved to. Public, matching <see cref="AffairEscalatedEvent"/>'s own
/// reasoning: resolving an already-publicly-known, high-stakes affair is itself a public act, not a
/// private fact between the two bond participants.</summary>
public sealed record AffairResolvedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<AffairRecord> AffairId,
    AffairResolution Resolution,
    string? CausationId) : IDomainEvent
{
    public string Type => "romance.affairResolved";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { AffairId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// Formally resolves an already-discovered, high-stakes <see cref="AffairRecord"/> — the consumer of
/// every <see cref="AffairRecord"/> <see cref="AffairDiscoverySystem"/> creates with <see
/// cref="AffairRecord.Resolution"/> left <c>null</c> (<c>gens-romance-sexuality-lineage-design.md</c>
/// §11). A minor-stakes affair never reaches this command at all — <see cref="AffairDiscoverySystem"/>
/// already auto-resolved it <see cref="AffairResolution.QuietlyResolved"/> the moment it was created.
///
/// <b>Composition with <see cref="FileAdulteryCaseCommand"/> (§12):</b> the <see
/// cref="AffairResolution.ProsecutedAdultery"/> branch derives that command's own two extra parameters
/// (a settlement from the wronged spouse's own <see cref="Character.Location"/>, and the wronged spouse
/// themselves as the filing Character) and delegates to it entirely — <see cref="FileAdulteryCaseCommand"/>,
/// not this command, is the one place <see cref="AffairRecord.Resolution"/> and <see
/// cref="AffairRecord.LegalCaseId"/> are actually written for that path (see that command's own doc
/// comment). Every OTHER branch (<see cref="AffairResolution.Forgiven"/>, <see
/// cref="AffairResolution.Divorced"/>, <see cref="AffairResolution.Challenged"/>) writes <see
/// cref="AffairRecord.Resolution"/> directly, here, via the same remove-then-readd idiom this module
/// already uses throughout. This keeps exactly one write path per resolution value rather than two
/// commands racing to set the same field.
///
/// <b><see cref="AffairResolution.Challenged"/></b> is accepted and recorded, but is a deliberate no-op
/// beyond that: Characters' own "Duel" mechanic (§9.6, cited by §11's Challenge option) does not exist
/// anywhere in this codebase, and building one is explicitly out of scope for this item (the approved
/// plan's own scope-defining decision) — matching this codebase's own repeated "modeled in the enum,
/// left genuinely unreached, until a later pass deliberately wires it" precedent (e.g. <see
/// cref="Legal.LegalSentence.DebtBondage"/>/<see cref="Legal.LegalSentence.Execution"/>).
///
/// <b><see cref="AffairResolution.QuietlyResolved"/></b> is rejected here — it is system-only, produced
/// exclusively by <see cref="AffairDiscoverySystem"/> for a minor-stakes escalation, never a value a
/// caller submits through this command. <b><see cref="AffairResolution.ExtremeLegalRemedyExercised"/></b>
/// is also rejected here, directing the caller to the separate, far more narrowly-gated <see
/// cref="ExerciseExtremeLegalRemedyCommand"/> instead (§12, §17) — that command's own gate (only the
/// wronged spouse may invoke it) is stricter than anything this command enforces, so it is never treated
/// as merely one more resolution value this command can also produce.
/// </summary>
public sealed record ResolveAffairCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<AffairRecord> AffairId,
    AffairResolution Resolution) : ICommand;

/// <summary>The validate/mutate pipeline for <see cref="ResolveAffairCommand"/> (ADR 0006). A factory
/// over <see cref="RandomStreamSet"/>, not a static <c>Pipeline</c>, matching <see
/// cref="Characters.BirthCharacterCommands.CreatePipeline"/>'s own identical shape — the <see
/// cref="AffairResolution.ProsecutedAdultery"/> branch's own nested <see
/// cref="FileAdulteryCaseCommands.CreatePipeline"/> call needs a real stream set threaded through, the
/// same reason that command needs one at all.</summary>
public static class ResolveAffairCommands
{
    public static readonly ValidationErrorCode AffairRecordNotFound = new("romance.resolveAffair.affairRecordNotFound");
    public static readonly ValidationErrorCode NotHighStakes = new("romance.resolveAffair.notHighStakes");
    public static readonly ValidationErrorCode AlreadyResolved = new("romance.resolveAffair.alreadyResolved");
    public static readonly ValidationErrorCode QuietlyResolvedIsSystemOnly = new("romance.resolveAffair.quietlyResolvedIsSystemOnly");
    public static readonly ValidationErrorCode UseExerciseExtremeLegalRemedyCommand = new("romance.resolveAffair.useExerciseExtremeLegalRemedyCommand");

    public static CommandPipeline<WorldState, ResolveAffairCommand> CreatePipeline(RandomStreamSet randomStreams)
    {
        if (randomStreams is null)
            throw new ArgumentNullException(nameof(randomStreams));

        return new CommandPipeline<WorldState, ResolveAffairCommand>(
            validate: Validate,
            mutate: (state, command) => Mutate(state, command, randomStreams),
            issueSequenceNumber: static state => state.IssueCommandSequenceNumber());
    }

    private static ValidationErrorCode? Validate(WorldState state, ResolveAffairCommand command)
    {
        if (!state.AffairRecords.TryGet(command.AffairId, out var record))
            return AffairRecordNotFound;
        if (record!.StakesLevel != AffairStakesLevel.HighStakes)
            return NotHighStakes;
        if (record.Resolution is not null)
            return AlreadyResolved;

        if (command.Resolution == AffairResolution.QuietlyResolved)
            return QuietlyResolvedIsSystemOnly;
        if (command.Resolution == AffairResolution.ExtremeLegalRemedyExercised)
            return UseExerciseExtremeLegalRemedyCommand;

        // The ProsecutedAdultery branch delegates entirely to FileAdulteryCaseCommand (this type's own
        // doc comment) — its Mutate call must be guaranteed to succeed, since a rejected nested command
        // whose CommandResult is silently discarded would otherwise leave this command "accepted" with
        // no legal case actually filed and AffairRecord.Resolution still null, contradicting the
        // AffairResolvedEvent(ProsecutedAdultery) already emitted. So every precondition
        // FileAdulteryCaseCommands.Validate itself checks is re-verified here, up front, reusing that
        // command's own ValidationErrorCode values rather than declaring a parallel set.
        if (command.Resolution == AffairResolution.ProsecutedAdultery)
        {
            if (!state.Characters.TryGet(record.WrongedSpouseId, out var wrongedSpouse) ||
                wrongedSpouse!.Household is not { } accusingHouseholdId)
                return FileAdulteryCaseCommands.WrongedSpouseHasNoHousehold;
            if (!state.Characters.TryGet(record.OffenderCharacterId, out var offender) ||
                offender!.Household is not { } defendantHouseholdId)
                return FileAdulteryCaseCommands.OffenderHasNoHousehold;
            if (accusingHouseholdId == defendantHouseholdId)
                return FileAdulteryCaseCommands.SameHousehold;
            if (!wrongedSpouse.IsAlive)
                return FileAdulteryCaseCommands.FilingCharacterDeceased;
            if (!state.Settlements.TryGet(wrongedSpouse.Location, out _))
                return FileAdulteryCaseCommands.SettlementNotFound;

            var balance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(accusingHouseholdId), out var account)
                ? account!.Balance
                : Money.Zero;
            if (balance < LegalCatalog.MajorFilingCost)
                return FileAdulteryCaseCommands.InsufficientTreasury;
        }

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, ResolveAffairCommand command, RandomStreamSet randomStreams)
    {
        state.AffairRecords.TryGet(command.AffairId, out var record);
        var affair = record!;
        var events = new List<IDomainEvent>();

        switch (command.Resolution)
        {
            case AffairResolution.Forgiven:
                events.AddRange(RecordRomanticInteractionCommands.Pipeline.Execute(
                    state, new RecordRomanticInteractionCommand(
                        state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                        affair.WrongedSpouseId, affair.OffenderCharacterId,
                        RomanceCatalog.AffairForgivenessAffectionDelta, 0, BondTypeOverride: null)).Events);
                GrantTrait(state, affair.WrongedSpouseId, RomanceCatalog.RehabilitatedTraitId);
                ReplaceResolution(state, command.AffairId, affair, command.Resolution);
                break;

            case AffairResolution.Divorced:
                events.AddRange(EndMarriageCommands.Pipeline.Execute(
                    state, new EndMarriageCommand(
                        state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                        affair.WrongedSpouseId, MarriageEndReason.Divorce)).Events);
                ReplaceResolution(state, command.AffairId, affair, command.Resolution);
                break;

            case AffairResolution.Challenged:
                // Deliberate no-op beyond recording the resolution — see this type's own doc comment.
                ReplaceResolution(state, command.AffairId, affair, command.Resolution);
                break;

            case AffairResolution.ProsecutedAdultery:
                state.Characters.TryGet(affair.WrongedSpouseId, out var wrongedSpouse);
                events.AddRange(FileAdulteryCaseCommands.CreatePipeline(randomStreams).Execute(
                    state, new FileAdulteryCaseCommand(
                        state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                        command.AffairId, wrongedSpouse!.Location, affair.WrongedSpouseId)).Events);
                // FileAdulteryCaseCommand itself already writes AffairRecord.Resolution (and LegalCaseId)
                // for this path — see this type's own doc comment for why this branch does not also
                // write it, avoiding a double write of the same field.
                break;
        }

        events.Add(new AffairResolvedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.AffairId, command.Resolution,
            command.CommandId.ToTaggedString()));

        return events.ToArray();
    }

    private static void ReplaceResolution(
        WorldState state, RuntimeId<AffairRecord> affairId, AffairRecord affair, AffairResolution resolution)
    {
        state.AffairRecords.Remove(affairId);
        state.AffairRecords.Add(affairId, affair with { Resolution = resolution });
    }

    /// <summary>Grants <paramref name="traitId"/> directly on <paramref name="characterId"/>, matching
    /// <see cref="Scandal.RecordScandalCommand.ApplyScandalMarkedTrait"/>'s and <see
    /// cref="AffairDiscoverySystem.GrantTrait"/>'s own identical remove-then-readd plumbing.</summary>
    private static void GrantTrait(WorldState state, RuntimeId<Character> characterId, DefinitionId<Trait> traitId)
    {
        if (!state.Characters.TryGet(characterId, out var character) || character is null || !character.IsAlive)
            return;
        if (character.Traits.Contains(traitId))
            return;

        var updatedTraits = character.Traits.Append(traitId).ToArray();
        state.Characters.Remove(characterId);
        state.Characters.Add(characterId, character with { Traits = updatedTraits });
    }
}
