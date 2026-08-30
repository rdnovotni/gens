using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Crime;

/// <summary>
/// §5's "a genuine escape attempt" for a Detained Character, reusing <see
/// cref="DetentionResolver.ComputeRiskScore"/>'s risk score and <see
/// cref="FlightRiskCalculator.MonthlyProbabilityThreshold"/>'s identical risk-to-probability curve
/// rather than inventing a parallel one. A deliberately narrower shape than <see
/// cref="LaborFlightSystem"/>'s own enslaved-specific flight/recapture engine: this is a single,
/// directly-submitted command (mirroring how <see cref="Religion.RespondToOmenCommand"/> and <see
/// cref="Characters.PromoteToNamedCommand"/> already roll dice inline inside a command rather than only
/// from a monthly system), not a recurring monthly opportunity roll with its own dispatched-pursuit
/// countdown — Detention's own duration is "genuinely open-ended" per §5, not a fixed labor Duty a
/// system needs to re-check every month, so a caller-driven attempt is the honest shape here. A failed
/// attempt still costs the detainee Loyalty (mirroring <see
/// cref="LaborFlightSystem"/>'s own <c>RecaptureLoyaltyPenalty</c> for "caught trying"), but this item
/// does not build a further dispatched-pursuit/harm-or-loss resolution the way that system's own
/// enslaved-specific engine does — a successful escape here simply ends the Detention outright.
/// </summary>
public sealed record AttemptDetentionEscapeCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId) : ICommand;

/// <summary>Emitted whenever an <see cref="AttemptDetentionEscapeCommand"/> is accepted, successful or
/// not.</summary>
public sealed record DetentionEscapeAttemptedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    RuntimeId<DetentionRecord> DetentionId,
    bool Succeeded,
    string? CausationId) : IDomainEvent
{
    public string Type => "crime.detentionEscapeAttempted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="AttemptDetentionEscapeCommand"/> (ADR 0006),
/// parameterized on the named random stream (rule 8) it draws its opportunity roll from.</summary>
public static class AttemptDetentionEscapeCommands
{
    /// <summary>The named random stream (rule 8) this command reserves for its own escape-opportunity
    /// roll, registered into <c>CampaignBootstrapper</c> alongside every other rule-8 stream.</summary>
    public const string EscapeAttemptStreamName = "crime.detentionEscapeAttempt";

    public static readonly ValidationErrorCode CharacterNotFound = new("crime.attemptDetentionEscape.characterNotFound");
    public static readonly ValidationErrorCode CharacterDeceased = new("crime.attemptDetentionEscape.characterDeceased");
    public static readonly ValidationErrorCode NotDetained = new("crime.attemptDetentionEscape.notDetained");

    public static CommandPipeline<WorldState, AttemptDetentionEscapeCommand> CreatePipeline(RandomStreamSet randomStreams)
    {
        if (randomStreams is null)
            throw new ArgumentNullException(nameof(randomStreams));

        return new CommandPipeline<WorldState, AttemptDetentionEscapeCommand>(
            validate: Validate,
            mutate: (state, command) => Mutate(state, command, randomStreams),
            issueSequenceNumber: static state => state.IssueCommandSequenceNumber());
    }

    private static ValidationErrorCode? Validate(WorldState state, AttemptDetentionEscapeCommand command)
    {
        if (!state.Characters.TryGet(command.CharacterId, out var character))
            return CharacterNotFound;
        if (!character!.IsAlive)
            return CharacterDeceased;
        if (DetentionResolver.ActiveFor(state, command.CharacterId) is null)
            return NotDetained;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, AttemptDetentionEscapeCommand command, RandomStreamSet randomStreams)
    {
        var detention = DetentionResolver.ActiveFor(state, command.CharacterId)!;

        var riskScore = DetentionResolver.ComputeRiskScore(state, command.CharacterId);
        var threshold = FlightRiskCalculator.MonthlyProbabilityThreshold(riskScore);
        var roll = randomStreams.NextUInt(EscapeAttemptStreamName, FlightRiskCalculator.RollPrecision);
        var succeeded = roll < threshold;

        var events = new List<IDomainEvent>();

        state.Characters.TryGet(command.CharacterId, out var character);

        if (succeeded)
        {
            state.DetentionRecords.Remove(detention.DetentionId);
            state.DetentionRecords.Add(detention.DetentionId, detention with { EndDate = command.SubmittedDate, Escaped = true });
        }
        else
        {
            state.Characters.Remove(command.CharacterId);
            state.Characters.Add(
                command.CharacterId,
                character! with
                {
                    Condition = new Condition(
                        character.Condition.Health, character.Condition.Fatigue,
                        Math.Max(0, character.Condition.Loyalty - CrimeCatalog.FailedEscapeAttemptLoyaltyPenalty),
                        character.Condition.Ambition, character.Condition.Fertility),
                });
        }

        events.Add(new DetentionEscapeAttemptedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.CharacterId, detention.DetentionId, succeeded,
            command.CommandId.ToTaggedString()));

        return events.ToArray();
    }
}
