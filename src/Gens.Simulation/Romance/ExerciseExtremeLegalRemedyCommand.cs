using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Romance;

/// <summary>Emitted whenever an <see cref="ExerciseExtremeLegalRemedyCommand"/> is accepted. Public —
/// about as public an act as exists (§12): a citizen killed a fellow citizen outside the courts, and no
/// campaign-facing system in this codebase treats a death this visible as anything but public knowledge
/// (matching <see cref="Characters.CharacterDiedEvent"/>'s own always-public reasoning).</summary>
public sealed record ExtremeLegalRemedyExercisedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<AffairRecord> AffairId,
    RuntimeId<Character> ActorId,
    RuntimeId<Character> ThirdPartyCharacterId,
    RuntimeId<Character>? OffenderCharacterId,
    string? CausationId) : IDomainEvent
{
    public string Type => "romance.extremeLegalRemedyExercised";
    public int SchemaVersion => 1;

    public IReadOnlyList<string> SubjectIds => OffenderCharacterId is { } offenderId
        ? new[] { ActorId.ToTaggedString(), ThirdPartyCharacterId.ToTaggedString(), offenderId.ToTaggedString() }
        : new[] { ActorId.ToTaggedString(), ThirdPartyCharacterId.ToTaggedString() };

    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// §12's real Roman <i>jus occidendi</i>-flavored remedy, mapped onto this codebase's own marriage-based
/// model rather than patria potestas: "a wronged husband... catching his wife and her lover... his own
/// daughter and her lover" (§12) becomes, here, the <see cref="AffairRecord.WrongedSpouseId"/> exercising
/// this narrow, manually-invoked-only command against the <see cref="AffairRecord.ThirdPartyCharacterId"/>
/// (the milder sub-case) and, when <see cref="AlsoOffender"/> is set, the <see
/// cref="AffairRecord.OffenderCharacterId"/> as well (§12's fuller historical scenario — the wronged
/// spouse's own unfaithful spouse, not only the third party). §17's own decisive answer for this command:
/// "a real, narrow, manually-invoked-only command, gated to an already-HighStakes/Escalated <see
/// cref="AffairRecord"/> and to the wronged spouse... as actor. No AI/autonomous action-selection ever
/// chooses it" — this command enforces that gate directly in <see cref="ExerciseExtremeLegalRemedyCommands.Validate"/>
/// rather than trusting a caller's own discretion.
///
/// <b>Always, regardless of legal justification (§12's own explicit framing):</b> the actor's own
/// household takes a severe, guaranteed Dignitas penalty (<see
/// cref="RomanceCatalog.ExtremeLegalRemedyActorHouseholdDignitasPenalty"/>) — this is not a risk the actor
/// can avoid by having "really" been wronged. A relationship-web hit lands too, between the actor and
/// their own surviving spouse (the offender) when that spouse is still alive to record one against — <see
/// cref="Characters.RecordInteractionCommand"/> requires both parties alive, so this hit is only
/// meaningful, and only ever submitted, when <see cref="AlsoOffender"/> is <c>false</c>; when <see
/// cref="AlsoOffender"/> is <c>true</c> the offender is already dead by the time this would apply, so only
/// the Dignitas penalty lands in that fuller sub-case.
///
/// <b>Death cause:</b> <see cref="Characters.DeathCause.Violence"/> — the most directly applicable
/// existing category (<c>gens-familia-design.md</c>'s own "violence... can all end a life" framing),
/// reused rather than inventing a narrow new cause for this one command, matching this module's own
/// established "reuse a reasonable existing general-purpose value" convention (e.g. <see
/// cref="Characters.MarriageEndReason.Divorce"/> being reused, unmodified, for <see
/// cref="ResolveAffairCommand"/>'s own <see cref="AffairResolution.Divorced"/> branch instead of a new
/// enum value).
/// </summary>
public sealed record ExerciseExtremeLegalRemedyCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<AffairRecord> AffairId,
    bool AlsoOffender) : ICommand;

/// <summary>The validate/mutate pipeline for <see cref="ExerciseExtremeLegalRemedyCommand"/> (ADR
/// 0006).</summary>
public static class ExerciseExtremeLegalRemedyCommands
{
    public static readonly ValidationErrorCode AffairRecordNotFound = new("romance.exerciseExtremeLegalRemedy.affairRecordNotFound");
    public static readonly ValidationErrorCode NotHighStakes = new("romance.exerciseExtremeLegalRemedy.notHighStakes");
    public static readonly ValidationErrorCode AlreadyResolved = new("romance.exerciseExtremeLegalRemedy.alreadyResolved");
    public static readonly ValidationErrorCode ActorNotWrongedSpouse = new("romance.exerciseExtremeLegalRemedy.actorNotWrongedSpouse");

    public static readonly CommandPipeline<WorldState, ExerciseExtremeLegalRemedyCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    /// <summary>§17's own "gated to... the wronged spouse... as actor" restriction — no existing command
    /// elsewhere in this codebase was found comparing a string <see cref="ICommand.ActorId"/> against a
    /// specific <c>RuntimeId&lt;Character&gt;</c> via a <c>RuntimeId&lt;T&gt;.Parse</c> call (confirmed by
    /// direct search), and that <c>Parse</c> itself throws on a non-matching string (e.g. the reserved
    /// <c>"system"</c> sentinel) rather than failing softly — which a command's own validation must never
    /// do. This compares against the wronged spouse's own <c>ToTaggedString()</c> form instead, which is
    /// exception-safe for any input.</summary>
    private static ValidationErrorCode? Validate(WorldState state, ExerciseExtremeLegalRemedyCommand command)
    {
        if (!state.AffairRecords.TryGet(command.AffairId, out var record))
            return AffairRecordNotFound;
        if (record!.StakesLevel != AffairStakesLevel.HighStakes)
            return NotHighStakes;
        if (record.Resolution is not null)
            return AlreadyResolved;
        if (command.ActorId != record.WrongedSpouseId.ToTaggedString())
            return ActorNotWrongedSpouse;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, ExerciseExtremeLegalRemedyCommand command)
    {
        state.AffairRecords.TryGet(command.AffairId, out var record);
        var affair = record!;
        var events = new List<IDomainEvent>();

        KillCharacter(state, affair.ThirdPartyCharacterId, command.SubmittedDate, events);
        if (command.AlsoOffender)
            KillCharacter(state, affair.OffenderCharacterId, command.SubmittedDate, events);

        // §12: the actor's own household always bears a severe, guaranteed Dignitas cost, regardless of
        // legal justification.
        if (state.Characters.TryGet(affair.WrongedSpouseId, out var actor) && actor!.Household is { } actorHouseholdId)
        {
            events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
                state, new AdjustDignitasCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    actorHouseholdId, -RomanceCatalog.ExtremeLegalRemedyActorHouseholdDignitasPenalty,
                    $"extreme legal remedy exercised for affair {command.AffairId.ToTaggedString()}")).Events);
        }

        // §12's relationship-web hit — only meaningful, and only submitted, when the offender (the
        // actor's own spouse) survives to record a scar against; see this type's own doc comment.
        if (!command.AlsoOffender)
        {
            events.AddRange(RecordInteractionCommands.Pipeline.Execute(
                state, new RecordInteractionCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    affair.WrongedSpouseId, affair.OffenderCharacterId,
                    RomanceCatalog.ExtremeLegalRemedyRelationshipScarOpinionDelta, BondTag.Nemesis, BondTag.None,
                    RelationshipOrigin.Family)).Events);
        }

        state.AffairRecords.Remove(command.AffairId);
        state.AffairRecords.Add(command.AffairId, affair with { Resolution = AffairResolution.ExtremeLegalRemedyExercised });

        events.Add(new ExtremeLegalRemedyExercisedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.AffairId, affair.WrongedSpouseId,
            affair.ThirdPartyCharacterId, command.AlsoOffender ? affair.OffenderCharacterId : null,
            command.CommandId.ToTaggedString()));

        return events.ToArray();
    }

    /// <summary>Kills <paramref name="characterId"/> by <see cref="DeathCause.Violence"/> and closes any
    /// open marriage the same way <see cref="Characters.CharacterLifecycleSystem"/>'s own death-with-open-
    /// marriage idiom does (<see cref="MarriageEndedEvent"/> included) — a no-op if the Character is
    /// already dead or unresolvable, matching this module's own established "never fabricate or crash on
    /// stale data" convention rather than a hard validation failure.</summary>
    private static void KillCharacter(WorldState state, RuntimeId<Character> characterId, GameDate date, List<IDomainEvent> events)
    {
        if (!state.Characters.TryGet(characterId, out var character) || character is null || !character.IsAlive)
            return;

        var deathRecord = new DeathRecord(date, DeathCause.Violence, character.AgeInYears(date));
        var updatedCharacter = character with { DeathRecord = deathRecord };

        var spouseId = updatedCharacter.CurrentSpouseId;
        if (spouseId is { } spouse)
        {
            updatedCharacter = CloseOpenMarriage(updatedCharacter, spouse, date);
            if (state.Characters.TryGet(spouse, out var spouseCharacter) && spouseCharacter!.IsAlive)
            {
                var updatedSpouse = CloseOpenMarriage(spouseCharacter, characterId, date);
                state.Characters.Remove(spouse);
                state.Characters.Add(spouse, updatedSpouse);
            }

            events.Add(new MarriageEndedEvent(
                state.EventIds.Issue(), date, characterId, spouse, MarriageEndReason.Death, CausationId: null));
        }

        state.Characters.Remove(characterId);
        state.Characters.Add(characterId, updatedCharacter);

        events.Add(new CharacterDiedEvent(state.EventIds.Issue(), date, characterId, spouseId, deathRecord));
    }

    private static Character CloseOpenMarriage(Character character, RuntimeId<Character> spouseId, GameDate endDate)
    {
        var history = character.MaritalHistory
            .Select(record => record.SpouseId == spouseId && record.EndDate is null
                ? new MarriageRecord(record.SpouseId, record.StartDate, endDate, MarriageEndReason.Death)
                : record)
            .ToArray();
        return character with { MaritalHistory = history };
    }
}
