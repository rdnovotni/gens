using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Magistracies;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.PublicContracts;

/// <summary>
/// §2's Censorship, filling both paired seats at once (§2: "elected in a genuine pair, exactly like the
/// Duumvirate"). Unlike <see cref="HoldContestedElectionCommand"/> + <see cref="PairDuumvirsCommand"/>'s
/// own two-step contest-then-pair sequence for the Duumvirate, this item takes a real, deliberate scope
/// simplification: both Censor seats are filled in one atomic command rather than two independently
/// contested elections — §9's own "Rival House bid AI depth... consistent with Rival Houses' own still-
/// open AI-depth question" leaves candidate generation and contest resolution genuinely open work this
/// item does not invent a resolution for; a caller (player choice, or a future AI layer) supplies both
/// already-chosen candidates directly, the same "caller supplies an already-resolved candidate" shape
/// <see cref="HoldContestedElectionCommand"/>'s own doc comment already established for rival-candidate
/// sourcing.
///
/// §2's gate — "having already held Duumvir at least once" — is checked via <see
/// cref="MagistracyResolver.HasEverHeldOffice"/>, a lifetime check rather than <see
/// cref="MagistracyResolver.ActiveRecord"/>'s "currently holds" check: a former Duumvir who has since
/// moved on remains Censor-eligible, matching §2's own "at least once" phrasing precisely.
/// </summary>
public sealed record ElectCensorsCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Settlement> SettlementId,
    RuntimeId<Character> HolderAId,
    RuntimeId<Character> HolderBId) : ICommand;

public sealed record CensorsElectedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Settlement> SettlementId,
    RuntimeId<Character> HolderAId,
    RuntimeId<Character> HolderBId,
    string? CausationId) : IDomainEvent
{
    public string Type => "publicContracts.censorsElected";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HolderAId.ToTaggedString(), HolderBId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class ElectCensorsCommands
{
    public static readonly ValidationErrorCode SettlementNotFound = new("publicContracts.electCensors.settlementNotFound");
    public static readonly ValidationErrorCode SameCharacter = new("publicContracts.electCensors.sameCharacter");
    public static readonly ValidationErrorCode HolderANotFound = new("publicContracts.electCensors.holderANotFound");
    public static readonly ValidationErrorCode HolderADeceased = new("publicContracts.electCensors.holderADeceased");
    public static readonly ValidationErrorCode HolderANeverHeldDuumvir = new("publicContracts.electCensors.holderANeverHeldDuumvir");
    public static readonly ValidationErrorCode HolderBNotFound = new("publicContracts.electCensors.holderBNotFound");
    public static readonly ValidationErrorCode HolderBDeceased = new("publicContracts.electCensors.holderBDeceased");
    public static readonly ValidationErrorCode HolderBNeverHeldDuumvir = new("publicContracts.electCensors.holderBNeverHeldDuumvir");
    public static readonly ValidationErrorCode CensorshipAlreadyFilled = new("publicContracts.electCensors.censorshipAlreadyFilled");

    public static readonly CommandPipeline<WorldState, ElectCensorsCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, ElectCensorsCommand command)
    {
        if (!state.Settlements.TryGet(command.SettlementId, out _))
            return SettlementNotFound;
        if (command.HolderAId == command.HolderBId)
            return SameCharacter;

        if (!state.Characters.TryGet(command.HolderAId, out var holderA))
            return HolderANotFound;
        if (!holderA!.IsAlive)
            return HolderADeceased;
        if (!MagistracyResolver.HasEverHeldOffice(state, command.HolderAId, MagistracyOffice.Duumvir))
            return HolderANeverHeldDuumvir;

        if (!state.Characters.TryGet(command.HolderBId, out var holderB))
            return HolderBNotFound;
        if (!holderB!.IsAlive)
            return HolderBDeceased;
        if (!MagistracyResolver.HasEverHeldOffice(state, command.HolderBId, MagistracyOffice.Duumvir))
            return HolderBNeverHeldDuumvir;

        if (MagistracyResolver.ActiveSeatCount(state, command.SettlementId, MagistracyOffice.Censor) > 0)
            return CensorshipAlreadyFilled;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, ElectCensorsCommand command)
    {
        var recordAId = state.MagistracyRecordIds.Issue();
        var recordBId = state.MagistracyRecordIds.Issue();

        state.MagistracyRecords.Add(
            recordAId,
            new MagistracyRecord(recordAId, command.HolderAId, MagistracyOffice.Censor, command.SettlementId, command.SubmittedDate, CoHolderId: command.HolderBId));
        state.MagistracyRecords.Add(
            recordBId,
            new MagistracyRecord(recordBId, command.HolderBId, MagistracyOffice.Censor, command.SettlementId, command.SubmittedDate, CoHolderId: command.HolderAId));

        ApplyCoMagistrateBond(state, command.HolderAId, command.HolderBId, command.SubmittedDate);
        ApplyCoMagistrateBond(state, command.HolderBId, command.HolderAId, command.SubmittedDate);

        return new IDomainEvent[]
        {
            new CensorsElectedEvent(state.EventIds.Issue(), command.SubmittedDate, command.SettlementId, command.HolderAId, command.HolderBId, command.CommandId.ToTaggedString()),
        };
    }

    /// <summary>Mirrors <see cref="PairDuumvirsCommands"/>' own identical private helper — same directed
    /// <see cref="BondTag.CoMagistrate"/> tag, the same "no shared helper exists yet to call instead"
    /// judgment call that command's own doc comment already made.</summary>
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
