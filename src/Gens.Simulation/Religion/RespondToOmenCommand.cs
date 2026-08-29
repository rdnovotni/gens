using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Religion;

/// <summary>
/// Resolves a pending <see cref="OmenEvent"/> as heeded or ignored (Phase 12 item 3; §4.1: "every Omen
/// Event presents a real choice, never a forced outcome"). <see cref="RespondingCharacterId"/> is the
/// Character whose Piety tier and reaction this resolution reads — §4.1's "an Impious Character is
/// mechanically immune to the penalty for ignoring an omen... a Zealous one suffers a real Favor... cost
/// for ignoring one even when nothing bad follows" — checked directly against <see
/// cref="ReligionCatalog.ImpiousTraitId"/>/<see cref="ReligionCatalog.ZealousTraitId"/> on <see
/// cref="Character.Traits"/> (see that catalog's own doc comment for why this is a direct id check
/// rather than a compiled <see cref="TraitCatalog"/> lookup this domain has no access path to).
///
/// Heeding always averts (§4.1: "heed it... in exchange for averting whatever the omen warned of") — a
/// deterministic outcome, no roll needed. Ignoring needs a genuine "was the omen accurate" roll (§4.1's
/// "a real chance the omen was accurate and the warned-of consequence lands anyway"), which this
/// command performs through a named <see cref="RandomStreamSet"/> stream (rule 8) captured by <see
/// cref="RespondToOmenCommands.CreatePipeline"/> exactly the way <see
/// cref="Characters.PromoteToNamedCommand.CreatePipeline"/> already captures one for its own command —
/// a command's <c>Mutate</c> delegate closing over an injected <see cref="RandomStreamSet"/> is this
/// codebase's own established way for a command (not only a monthly <see
/// cref="IMonthlySystem{TState}"/>) to roll dice deterministically.
/// </summary>
public sealed record RespondToOmenCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<OmenEvent> OmenId,
    RuntimeId<Character> RespondingCharacterId,
    OmenChoice Choice) : ICommand;

/// <summary>Emitted whenever a <see cref="RespondToOmenCommand"/> is accepted. Public, matching every
/// other Favor-moving fact in this domain.</summary>
public sealed record OmenRespondedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<OmenEvent> OmenId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> RespondingCharacterId,
    OmenChoice Choice,
    OmenOutcome Outcome,
    int FavorDelta,
    string? CausationId) : IDomainEvent
{
    public string Type => "religion.omenResponded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), RespondingCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="RespondToOmenCommand"/> (ADR 0006).</summary>
public static class RespondToOmenCommands
{
    /// <summary>The named random stream (rule 8) reserved for the "did an ignored Omen's warning come
    /// true" roll — registered in <see cref="Campaign.CampaignBootstrapper"/>.</summary>
    public const string OmenIgnoredOutcomeStreamName = "religion.omenIgnoredOutcome";

    public static readonly ValidationErrorCode UnknownOmen = new("religion.respondToOmen.unknownOmen");
    public static readonly ValidationErrorCode AlreadyResolved = new("religion.respondToOmen.alreadyResolved");
    public static readonly ValidationErrorCode CharacterNotFound = new("religion.respondToOmen.characterNotFound");
    public static readonly ValidationErrorCode CharacterDeceased = new("religion.respondToOmen.characterDeceased");

    public static CommandPipeline<WorldState, RespondToOmenCommand> CreatePipeline(RandomStreamSet randomStreams)
    {
        if (randomStreams is null)
            throw new ArgumentNullException(nameof(randomStreams));

        return new CommandPipeline<WorldState, RespondToOmenCommand>(
            validate: Validate,
            mutate: (state, command) => Mutate(state, command, randomStreams),
            issueSequenceNumber: static state => state.IssueCommandSequenceNumber());
    }

    private static ValidationErrorCode? Validate(WorldState state, RespondToOmenCommand command)
    {
        if (!OmenResolver.TryGet(state, command.OmenId, out var omen))
            return UnknownOmen;
        if (omen.Outcome != OmenOutcome.Pending)
            return AlreadyResolved;
        if (!state.Characters.TryGet(command.RespondingCharacterId, out var character))
            return CharacterNotFound;
        if (!character!.IsAlive)
            return CharacterDeceased;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, RespondToOmenCommand command, RandomStreamSet randomStreams)
    {
        OmenResolver.TryGet(state, command.OmenId, out var omen);
        state.Characters.TryGet(command.RespondingCharacterId, out var character);

        var impious = character!.Traits.Contains(ReligionCatalog.ImpiousTraitId);
        var zealous = character.Traits.Contains(ReligionCatalog.ZealousTraitId);

        OmenOutcome outcome;
        int favorDelta;

        if (command.Choice == OmenChoice.Heeded)
        {
            outcome = OmenOutcome.Averted;
            favorDelta = ReligionCatalog.OmenHeededFavorGain;
        }
        else
        {
            var chance = (uint)(omen.Severity * ReligionCatalog.OmenIgnoredConsequenceChancePerSeverityPercent);
            var roll = randomStreams.NextUInt(OmenIgnoredOutcomeStreamName, 100);
            var landed = roll < chance;

            if (landed)
            {
                outcome = OmenOutcome.ConsequenceLanded;
                favorDelta = impious ? 0 : -ReligionCatalog.OmenIgnoredConsequenceFavorLoss;
            }
            else
            {
                outcome = OmenOutcome.NoConsequence;
                favorDelta = zealous ? -ReligionCatalog.ZealousIgnoredNoConsequencePenalty : 0;
            }
        }

        state.OmenEvents.Remove(command.OmenId);
        state.OmenEvents.Add(command.OmenId, omen with { PlayerChoice = command.Choice, Outcome = outcome });

        if (favorDelta != 0 && HouseholdReligionResolver.HasChosenPatron(state, omen.HouseholdId))
            HouseholdReligionResolver.ApplyFavorDelta(state, omen.HouseholdId, favorDelta);

        return new IDomainEvent[]
        {
            new OmenRespondedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.OmenId, omen.HouseholdId, command.RespondingCharacterId,
                command.Choice, outcome, favorDelta, command.CommandId.ToTaggedString()),
        };
    }
}
