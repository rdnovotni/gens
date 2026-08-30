using Gens.Simulation.Commands;
using Gens.Simulation.Crime;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Scandal;

/// <summary>Emitted whenever a <see cref="DiscoverFabricationCommand"/> is accepted. Public, matching
/// <see cref="PunishableOffenseRecordedEvent"/>'s own reasoning for the underlying offense.</summary>
public sealed record FabricationDiscoveredEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<PunishableOffense> OffenseId,
    RuntimeId<Characters.Character> CharacterId,
    string? CausationId) : IDomainEvent
{
    public string Type => "scandal.fabricationDiscovered";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// Finally consumes the hook Phase 12 item 5 left deliberately dormant: <see
/// cref="PunishableOffense.FabricationDiscovered"/>'s own doc comment names it "a real, present hook...
/// specifically so that future Scheme type has somewhere to land," and states plainly that "no caller in
/// this codebase ever sets [it] true." This item is not the Scheme type that doc comment was actually
/// waiting for (Crime &amp; Punishment §9's own Fabricating-Justification-as-a-Scheme-type loop is still
/// unbuilt, per that item's own explicit cut) — but §4's own "a discovered Fabrication... retroactively
/// the single worst-case scandal source this project has built" only needs a real, standalone way for
/// <em>something</em> to flip that flag, not specifically a Scheme. This command is that real, narrow,
/// separate primitive: it flips <see cref="PunishableOffense.FabricationDiscovered"/> on an already-real
/// <see cref="PunishableOffense"/> (one <see cref="RecordPunishableOffenseCommand"/> already recorded
/// with <see cref="PunishableOffense.IsFabricated"/> true) directly on <see
/// cref="WorldState.PunishableOffenses"/> — a new command touching that partition, not a reopening of
/// <see cref="RecordPunishableOffenseCommand"/>'s own already-tested pipeline — and, in the same
/// mutation, records a real <see cref="ScandalRecord"/> at <see
/// cref="ScandalSeverity.NotaCensoriaEligible"/>, this item's one real path to that severity tier (see
/// that enum's own doc comment for why the formal Nota Censoria consequence itself still never fires).
/// </summary>
public sealed record DiscoverFabricationCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<PunishableOffense> OffenseId) : ICommand;

/// <summary>The validate/mutate pipeline for <see cref="DiscoverFabricationCommand"/> (ADR 0006).</summary>
public static class DiscoverFabricationCommands
{
    public static readonly ValidationErrorCode OffenseNotFound = new("scandal.discoverFabrication.offenseNotFound");
    public static readonly ValidationErrorCode NotFabricated = new("scandal.discoverFabrication.notFabricated");
    public static readonly ValidationErrorCode AlreadyDiscovered = new("scandal.discoverFabrication.alreadyDiscovered");

    public static readonly CommandPipeline<WorldState, DiscoverFabricationCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, DiscoverFabricationCommand command)
    {
        if (!state.PunishableOffenses.TryGet(command.OffenseId, out var offense))
            return OffenseNotFound;
        if (!offense!.IsFabricated)
            return NotFabricated;
        if (offense.FabricationDiscovered)
            return AlreadyDiscovered;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, DiscoverFabricationCommand command)
    {
        state.PunishableOffenses.TryGet(command.OffenseId, out var offense);
        state.PunishableOffenses.Remove(command.OffenseId);
        state.PunishableOffenses.Add(command.OffenseId, offense! with { FabricationDiscovered = true });

        var events = new List<IDomainEvent>
        {
            new FabricationDiscoveredEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.OffenseId, offense.CharacterId,
                command.CommandId.ToTaggedString()),
        };

        if (state.Characters.TryGet(offense.CharacterId, out var character) && character!.Household is { } householdId)
        {
            events.AddRange(RecordScandalCommands.Pipeline.Execute(
                state, new RecordScandalCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    householdId, ScandalSourceType.DiscoveredFabrication, ScandalSeverity.NotaCensoriaEligible,
                    TraitGrantCharacterId: offense.CharacterId)).Events);
        }

        return events.ToArray();
    }
}
