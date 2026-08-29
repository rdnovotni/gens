using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Magistracies;

/// <summary>
/// Seats a Character as Decurion at one settlement's Curia (Phase 12 item 2; §5.1) — the ladder's base
/// entry point, granted directly rather than through <see cref="HoldContestedElectionCommand"/>: §5.5
/// scopes contested elections to "any office above Decurion," so becoming a Decurion in the first place
/// is a straightforward qualification check (citizenship, a Dignitas threshold), not a contest.
///
/// <b>Scope note:</b> §5.1 also requires "the building" (the Curia, Buildings §4.10) — no Curia (or any
/// other) building type exists anywhere in <c>Gens.Simulation.Buildings</c> at the time this item was
/// built, so that half of the gate is not checked here; the Dignitas/citizenship half is. A future
/// Buildings pass adding a real Curia type should add a "settlement has a completed Curia" check to
/// this command's own <c>Validate</c>, following whatever gate convention that pass establishes
/// elsewhere (this codebase has no existing "requires this building" pattern yet to match either).
/// </summary>
public sealed record AppointDecurionCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    RuntimeId<Settlement> SettlementId) : ICommand;

/// <summary>Emitted whenever an <see cref="AppointDecurionCommand"/> (or any other office-assuming
/// command in this namespace) is accepted. <see cref="Visibility"/> is <see
/// cref="Commands.Visibility.Public"/> — per §5.6, fellow Decurions and the wider Curia are assumed to
/// simply know who holds a seat, the same "legible to the political class by definition" reasoning
/// <see cref="Reputation.DignitasChangedEvent"/>'s own doc comment already gives for Dignitas.</summary>
public sealed record MagistracyAssumedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<MagistracyRecord> RecordId,
    RuntimeId<Character> HolderId,
    MagistracyOffice Office,
    RuntimeId<Settlement> SettlementId,
    string? CausationId) : IDomainEvent
{
    public string Type => "magistracies.magistracyAssumed";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HolderId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="AppointDecurionCommand"/> (ADR 0006).</summary>
public static class AppointDecurionCommands
{
    public static readonly ValidationErrorCode CharacterNotFound = new("magistracies.appointDecurion.characterNotFound");
    public static readonly ValidationErrorCode CharacterDeceased = new("magistracies.appointDecurion.characterDeceased");
    public static readonly ValidationErrorCode IneligibleLegalStatus = new("magistracies.appointDecurion.ineligibleLegalStatus");
    public static readonly ValidationErrorCode NoHousehold = new("magistracies.appointDecurion.noHousehold");
    public static readonly ValidationErrorCode InsufficientDignitas = new("magistracies.appointDecurion.insufficientDignitas");
    public static readonly ValidationErrorCode AlreadyHoldsSeat = new("magistracies.appointDecurion.alreadyHoldsSeat");
    public static readonly ValidationErrorCode CuriaFull = new("magistracies.appointDecurion.curiaFull");

    public static readonly CommandPipeline<WorldState, AppointDecurionCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, AppointDecurionCommand command)
    {
        if (!state.Characters.TryGet(command.CharacterId, out var character))
            return CharacterNotFound;
        if (!character!.IsAlive)
            return CharacterDeceased;
        // §5.1: "Peregrine and Freedman statuses are excluded from formal office."
        if (character.LegalStatus is not (LegalStatus.RomanCitizen or LegalStatus.LatinRights))
            return IneligibleLegalStatus;
        if (character.Household is not { } householdId)
            return NoHousehold;
        if (DignitasResolver.Current(state, householdId) < MagistracyCatalog.DecurionDignitasThreshold)
            return InsufficientDignitas;
        if (MagistracyResolver.ActiveRecord(state, command.SettlementId, MagistracyOffice.Decurion, command.CharacterId) is not null)
            return AlreadyHoldsSeat;
        if (MagistracyResolver.ActiveSeatCount(state, command.SettlementId, MagistracyOffice.Decurion) >= MagistracyCatalog.DecurionCuriaSeatCount)
            return CuriaFull;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, AppointDecurionCommand command)
    {
        var recordId = state.MagistracyRecordIds.Issue();
        state.MagistracyRecords.Add(
            recordId,
            new MagistracyRecord(recordId, command.CharacterId, MagistracyOffice.Decurion, command.SettlementId, command.SubmittedDate));

        return new IDomainEvent[]
        {
            new MagistracyAssumedEvent(
                state.EventIds.Issue(), command.SubmittedDate, recordId, command.CharacterId, MagistracyOffice.Decurion,
                command.SettlementId, command.CommandId.ToTaggedString()),
        };
    }
}
