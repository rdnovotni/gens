using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Crime;

/// <summary>§10's "Release without ransom — simple mercy" — always available, and, per that section's
/// own logic, "reads as a genuine, positive relationship-web and Dignitas event precisely because it
/// was never required." A releasing household's own Dignitas requires it to actually be one — see this
/// command's own <see cref="ReleaseFromDetentionCommands.Mutate"/> for why that Dignitas grant only
/// fires when the releaser's own household can be resolved.</summary>
public sealed record ReleaseFromDetentionCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    RuntimeId<Character> ReleasingCharacterId) : ICommand;

/// <summary>Emitted whenever a <see cref="ReleaseFromDetentionCommand"/> is accepted.</summary>
public sealed record CharacterReleasedFromDetentionEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    RuntimeId<Character> ReleasingCharacterId,
    RuntimeId<DetentionRecord> DetentionId,
    string? CausationId) : IDomainEvent
{
    public string Type => "crime.characterReleasedFromDetention";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString(), ReleasingCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="ReleaseFromDetentionCommand"/> (ADR 0006).</summary>
public static class ReleaseFromDetentionCommands
{
    public static readonly ValidationErrorCode CharacterNotFound = new("crime.releaseFromDetention.characterNotFound");
    public static readonly ValidationErrorCode ReleasingCharacterNotFound = new("crime.releaseFromDetention.releasingCharacterNotFound");
    public static readonly ValidationErrorCode NotDetained = new("crime.releaseFromDetention.notDetained");

    public static readonly CommandPipeline<WorldState, ReleaseFromDetentionCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, ReleaseFromDetentionCommand command)
    {
        if (!state.Characters.TryGet(command.CharacterId, out var character) || !character!.IsAlive)
            return CharacterNotFound;
        if (!state.Characters.TryGet(command.ReleasingCharacterId, out var releaser) || !releaser!.IsAlive)
            return ReleasingCharacterNotFound;
        if (DetentionResolver.ActiveFor(state, command.CharacterId) is null)
            return NotDetained;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, ReleaseFromDetentionCommand command)
    {
        var detention = DetentionResolver.ActiveFor(state, command.CharacterId)!;
        state.DetentionRecords.Remove(detention.DetentionId);
        state.DetentionRecords.Add(detention.DetentionId, detention with { EndDate = command.SubmittedDate });

        var events = new List<IDomainEvent>();
        events.AddRange(RecordInteractionCommands.Pipeline.Execute(
            state, new RecordInteractionCommand(
                state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                command.ReleasingCharacterId, command.CharacterId, CrimeCatalog.RansomPaidOrMercyOpinionGain,
                BondTag.None, BondTag.None, RelationshipOrigin.Political)).Events);

        if (state.Characters.TryGet(command.ReleasingCharacterId, out var releaser) && releaser!.Household is { } releasingHouseholdId)
        {
            events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
                state, new AdjustDignitasCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    releasingHouseholdId, CrimeCatalog.RansomPaidOrMercyDignitasGain,
                    $"mercy release of {command.CharacterId.ToTaggedString()}")).Events);
        }

        events.Add(new CharacterReleasedFromDetentionEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.CharacterId, command.ReleasingCharacterId,
            detention.DetentionId, command.CommandId.ToTaggedString()));

        return events.ToArray();
    }
}
