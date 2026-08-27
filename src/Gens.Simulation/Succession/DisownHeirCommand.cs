using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Succession;

/// <summary>§2.3 Disownment: removes an eligible heir from the pool entirely. Clears the disowned
/// Character from <see cref="HeirDesignation.PreferredHeirId"/>/<see
/// cref="HeirDesignation.FormallyDeclaredHeirId"/> if they held either, and applies <see
/// cref="SuccessionCatalog.DisownedLoyaltyPenalty"/> to the disowned Character's own Condition — this
/// implementation's scoped stand-in for §2.3's fuller "damages opinion between the disowned and
/// everyone who stayed loyal" (see <see cref="SuccessionCatalog"/>'s own doc comment). Reconciliation
/// (undoing a disownment) is an Open Question §10 leaves unresolved — there is no command to reverse
/// this one.</summary>
public sealed record DisownHeirCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> CharacterId) : ICommand;

/// <summary>Emitted whenever a <see cref="DisownHeirCommand"/> is accepted.</summary>
public sealed record HeirDisownedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> CharacterId,
    string? CausationId) : IDomainEvent
{
    public string Type => "succession.heirDisowned";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="DisownHeirCommand"/> (ADR 0006).</summary>
public static class DisownHeirCommands
{
    public static readonly ValidationErrorCode HouseholdHasNoHead = new("succession.disownHeir.householdHasNoHead");
    public static readonly ValidationErrorCode NotAnEligibleHeir = new("succession.disownHeir.notAnEligibleHeir");
    public static readonly ValidationErrorCode AlreadyDisowned = new("succession.disownHeir.alreadyDisowned");

    public static readonly CommandPipeline<WorldState, DisownHeirCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, DisownHeirCommand command)
    {
        if (!state.HouseholdHeadships.TryGet(command.HouseholdId, out var headship))
            return HouseholdHasNoHead;

        state.HeirDesignations.TryGet(command.HouseholdId, out var existing);
        if (existing?.DisownedCharacterIds.Contains(command.CharacterId) == true)
            return AlreadyDisowned;

        var pool = HeirEligibilityService.EligibleHeirs(state, headship.HeadCharacterId, existing);
        if (!pool.Contains(command.CharacterId))
            return NotAnEligibleHeir;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, DisownHeirCommand command)
    {
        var designation = state.HeirDesignations.TryGet(command.HouseholdId, out var existing)
            ? existing
            : HeirDesignation.Empty(command.HouseholdId);

        var updated = designation with
        {
            DisownedCharacterIds = designation.DisownedCharacterIds.Append(command.CharacterId).ToArray(),
            PreferredHeirId = designation.PreferredHeirId == command.CharacterId ? null : designation.PreferredHeirId,
            FormallyDeclaredHeirId = designation.FormallyDeclaredHeirId == command.CharacterId ? null : designation.FormallyDeclaredHeirId,
            DeclaredDate = designation.FormallyDeclaredHeirId == command.CharacterId ? null : designation.DeclaredDate,
        };

        state.HeirDesignations.Remove(command.HouseholdId);
        state.HeirDesignations.Add(command.HouseholdId, updated);

        var events = new List<IDomainEvent>
        {
            new HeirDisownedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, command.CharacterId,
                command.CommandId.ToTaggedString()),
        };

        if (state.Characters.TryGet(command.CharacterId, out var disowned))
        {
            var penalized = disowned with
            {
                Condition = new Condition(
                    disowned.Condition.Health, disowned.Condition.Fatigue,
                    Math.Clamp(disowned.Condition.Loyalty - SuccessionCatalog.DisownedLoyaltyPenalty, 0, 100),
                    disowned.Condition.Ambition, disowned.Condition.Fertility),
            };
            state.Characters.Remove(command.CharacterId);
            state.Characters.Add(command.CharacterId, penalized);
        }

        return events.ToArray();
    }
}
