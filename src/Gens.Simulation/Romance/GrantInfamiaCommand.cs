using System.Linq;
#nullable enable
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Romance;

/// <summary>Grants a real, permanent <see cref="InfamiaStatus"/> mark (<c>gens-romance-sexuality-lineage-design.md</c>
/// §13). <see cref="AdulteryResolutionHook"/> is this item's own real, reachable caller, submitting this
/// with <see cref="InfamiaSource.ConvictedAdultery"/> on a §12 Adultery conviction — see <see
/// cref="InfamiaStatus"/>'s and <see cref="InfamiaSource"/>'s own doc comments for why <see
/// cref="InfamiaSource.Prostitution"/>/<see cref="InfamiaSource.Acting"/>/<see
/// cref="InfamiaSource.Gladiatorial"/> have no caller yet.</summary>
public sealed record GrantInfamiaCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    InfamiaSource Source) : ICommand;

/// <summary>Emitted whenever a <see cref="GrantInfamiaCommand"/> is accepted. Public, matching <see
/// cref="Reputation.AdjustDignitasCommand"/>'s own "a standing figure other actors are simply assumed to
/// know" reasoning — a real Infamia mark is exactly the kind of legible, publicly known fact a Roman
/// citizen's own social/legal standing turns on, not a private interior state.</summary>
public sealed record InfamiaGrantedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    InfamiaSource Source,
    string? CausationId) : IDomainEvent
{
    public string Type => "romance.infamiaGranted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="GrantInfamiaCommand"/> (ADR 0006).</summary>
public static class GrantInfamiaCommands
{
    public static readonly ValidationErrorCode CharacterNotFound = new("romance.grantInfamia.characterNotFound");
    public static readonly ValidationErrorCode CharacterDeceased = new("romance.grantInfamia.characterDeceased");
    public static readonly ValidationErrorCode AlreadyMarked = new("romance.grantInfamia.alreadyMarked");

    public static readonly CommandPipeline<WorldState, GrantInfamiaCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, GrantInfamiaCommand command)
    {
        if (!state.Characters.TryGet(command.CharacterId, out var character))
            return CharacterNotFound;
        if (!character!.IsAlive)
            return CharacterDeceased;
        if (state.InfamiaStatuses.TryGet(command.CharacterId, out _))
            return AlreadyMarked;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, GrantInfamiaCommand command)
    {
        var status = new InfamiaStatus(command.CharacterId, command.Source, LegalProtectionsLostFor(command.Source));
        state.InfamiaStatuses.Add(command.CharacterId, status);

        return new IDomainEvent[]
        {
            new InfamiaGrantedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CharacterId, command.Source,
                command.CommandId.ToTaggedString()),
        };
    }

    /// <summary>A concrete, but explicitly untuned, per-source placeholder (§17: every numeric/textual
    /// size in this module is an untuned placeholder) — no mechanical system yet reads this list to
    /// actually withhold anything; it exists so a future pass has a real, honestly-labeled starting
    /// point.</summary>
    private static string[] LegalProtectionsLostFor(InfamiaSource source) => source switch
    {
        InfamiaSource.ConvictedAdultery => new[]
        {
            "cannot testify in certain proceedings",
            "barred from holding public office",
        },
        _ => new[]
        {
            "cannot testify in certain proceedings",
            "barred from holding public office",
        },
    };
}
