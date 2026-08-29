using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Magistracies;

/// <summary>
/// Links two independently-won <see cref="MagistracyOffice.Duumvir"/> seats into the paired
/// colleague relationship §5.4 describes: "the co-Duumvir is always a real Character... a genuine
/// relationship-web entry the player inherits the moment they win the office." <see
/// cref="HoldContestedElectionCommand"/> resolves each Duumvir seat independently (they're two
/// separate contests, per §5.4's own "held by two colleagues rather than one"); this command is the
/// explicit follow-up that wires the pairing once both are filled, writing <see
/// cref="BondTag.CoMagistrate"/> both ways in the relationship web.
///
/// <b>Scope note:</b> §5.4 also says Duumvir "satisfies the Mint/Moneta's political milestone gate" —
/// no Mint (or any other) building type exists anywhere in <c>Gens.Simulation.Buildings</c> at the time
/// this item was built, matching <see cref="AppointDecurionCommand"/>'s identical "no building system
/// to gate against yet" scope note for the Curia. A future Buildings pass adding a real Mint should
/// check <see cref="MagistracyResolver.ActiveRecord"/> for <see cref="MagistracyOffice.Duumvir"/>
/// directly.
/// </summary>
public sealed record PairDuumvirsCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Settlement> SettlementId,
    RuntimeId<Character> HolderAId,
    RuntimeId<Character> HolderBId) : ICommand;

/// <summary>Emitted whenever a <see cref="PairDuumvirsCommand"/> is accepted. Public, matching every
/// other office-holding fact in this namespace.</summary>
public sealed record DuumvirsPairedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Settlement> SettlementId,
    RuntimeId<Character> HolderAId,
    RuntimeId<Character> HolderBId,
    string? CausationId) : IDomainEvent
{
    public string Type => "magistracies.duumvirsPaired";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HolderAId.ToTaggedString(), HolderBId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="PairDuumvirsCommand"/> (ADR 0006).</summary>
public static class PairDuumvirsCommands
{
    public static readonly ValidationErrorCode SameCharacter = new("magistracies.pairDuumvirs.sameCharacter");
    public static readonly ValidationErrorCode HolderANotADuumvir = new("magistracies.pairDuumvirs.holderANotADuumvir");
    public static readonly ValidationErrorCode HolderBNotADuumvir = new("magistracies.pairDuumvirs.holderBNotADuumvir");
    public static readonly ValidationErrorCode AlreadyPaired = new("magistracies.pairDuumvirs.alreadyPaired");

    public static readonly CommandPipeline<WorldState, PairDuumvirsCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, PairDuumvirsCommand command)
    {
        if (command.HolderAId == command.HolderBId)
            return SameCharacter;

        var recordA = MagistracyResolver.ActiveRecord(state, command.SettlementId, MagistracyOffice.Duumvir, command.HolderAId);
        if (recordA is null)
            return HolderANotADuumvir;
        var recordB = MagistracyResolver.ActiveRecord(state, command.SettlementId, MagistracyOffice.Duumvir, command.HolderBId);
        if (recordB is null)
            return HolderBNotADuumvir;
        if (recordA.CoHolderId is not null || recordB.CoHolderId is not null)
            return AlreadyPaired;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, PairDuumvirsCommand command)
    {
        var recordA = MagistracyResolver.ActiveRecord(state, command.SettlementId, MagistracyOffice.Duumvir, command.HolderAId)!;
        var recordB = MagistracyResolver.ActiveRecord(state, command.SettlementId, MagistracyOffice.Duumvir, command.HolderBId)!;

        state.MagistracyRecords.Remove(recordA.RecordId);
        state.MagistracyRecords.Add(recordA.RecordId, recordA with { CoHolderId = command.HolderBId });
        state.MagistracyRecords.Remove(recordB.RecordId);
        state.MagistracyRecords.Add(recordB.RecordId, recordB with { CoHolderId = command.HolderAId });

        ApplyCoMagistrateBond(state, command.HolderAId, command.HolderBId, command.SubmittedDate);
        ApplyCoMagistrateBond(state, command.HolderBId, command.HolderAId, command.SubmittedDate);

        return new IDomainEvent[]
        {
            new DuumvirsPairedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.SettlementId, command.HolderAId, command.HolderBId,
                command.CommandId.ToTaggedString()),
        };
    }

    /// <summary>Writes the directed <see cref="BondTag.CoMagistrate"/> tag from <paramref
    /// name="fromId"/> toward <paramref name="toId"/>, mirroring <see
    /// cref="Clientela.ClientelaBondHelper"/>'s identical direct-write shape for its own Patron/Client
    /// pair rather than depending on that Clientela-specific helper for an unrelated bond tag.</summary>
    private static void ApplyCoMagistrateBond(WorldState state, RuntimeId<Character> fromId, RuntimeId<Character> toId, GameDate date)
    {
        var key = new RelationshipKey(fromId, toId);
        var exists = state.Relationships.TryGet(key, out var existing);
        var opinion = exists ? existing.Opinion : 0;
        var bonds = (exists ? existing.Bonds : BondTag.None) | BondTag.CoMagistrate;
        var formedDate = exists ? existing.FormedDate : date;

        if (exists)
            state.Relationships.Remove(key);
        state.Relationships.Add(key, new Relationship(opinion, bonds, RelationshipOrigin.Political, formedDate, date, provenanceEventId: null));
    }
}
