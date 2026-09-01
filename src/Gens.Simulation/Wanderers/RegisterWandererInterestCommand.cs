using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Wanderers;

/// <summary>§7's declared-but-uncommitted interest: a household (the player's own, or a rival's) makes
/// it known that it wants this Wanderer, without yet paying for a Host or a Recruit.</summary>
public sealed record RegisterWandererInterestCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Wanderer> WandererId,
    RuntimeId<Household> HouseholdId) : ICommand;

/// <summary>Emitted whenever a <see cref="RegisterWandererInterestCommand"/> is accepted.</summary>
public sealed record WandererInterestRegisteredEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Wanderer> WandererId,
    RuntimeId<Household> HouseholdId,
    int CompetingHouseholdCount,
    string? CausationId) : IDomainEvent
{
    public string Type => "wanderers.interestRegistered";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { WandererId.ToTaggedString(), HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// The validate/mutate pipeline for <see cref="RegisterWandererInterestCommand"/> (ADR 0006), and the
/// whole of this item's §7 Competition mechanic.
///
/// <para><b>What is real here.</b> §7's load-bearing rule is that a contested Wanderer is "resolved the
/// instant either side actually commits rather than held open indefinitely." That is implemented
/// exactly, and symmetrically: this command records interest without resolving anything, and the first
/// <see cref="HostWandererCommand"/> or <see cref="RecruitWandererCommand"/> to land — from any
/// household, player's or rival's — stamps <see cref="Wanderer.CommittedHouseholdId"/>, empties the
/// interest list, and causes every other household's engagement to be rejected with that command's own
/// <c>CommittedElsewhere</c> error. Nothing is held open, nothing is queued, and there is no
/// tiebreak: first commit wins, deterministically, which is precisely §7.1's own "I should have left
/// sooner" outcome. §4's Fame gate is real too — <see
/// cref="WandererFameCalculator.CompetitionVisibilityThreshold"/> refuses to record interest in an
/// obscure Wanderer, §7's own "a <i>sufficiently high-Fame</i> Wanderer is a real, visible object of
/// interest to more than just the player."</para>
///
/// <para><b>What is deliberately not built, and why.</b> §7 imagines a Rival House deciding on its own
/// to enter the race. This codebase has no hook for that decision and none is invented here: <see
/// cref="Actors.RivalAmbitionSystem"/> is the only place a rival house autonomously wants anything, and
/// its wants come from a closed <see cref="Actors.RivalAmbitionCatalog"/> of house-scale ambitions with
/// no notion of an individual person as a target, no way to express "engage this specific entity," and
/// no spending path. Rather than fake a rival AI, this command is the neutral mechanism <i>either</i>
/// side uses, and a future Rival Houses pass can drive it for a rival by submitting it with that
/// house's own household ID — the same "hook now, caller later" discipline
/// <c>Health.AfflictCharacterCommand</c> and <c>Hazards.DesignateDormantVolcanoCommand</c> used in this
/// same phase. §10's own <c>wasPlayerAwareViaRumor</c> field is likewise not carried on any record
/// here: it asks whether Correspondence's early warning reached the player in time, and
/// <c>Correspondence/</c> has no rumor-delivery action to have reached them with (<see
/// cref="InstantiateWandererCommands"/>'s own disclosure).</para>
/// </summary>
public static class RegisterWandererInterestCommands
{
    public static readonly ValidationErrorCode WandererNotFound = new("wanderers.interest.wandererNotFound");
    public static readonly ValidationErrorCode WandererUnavailable = new("wanderers.interest.wandererUnavailable");
    public static readonly ValidationErrorCode AlreadyResolved = new("wanderers.interest.alreadyResolved");
    public static readonly ValidationErrorCode InsufficientFame = new("wanderers.interest.insufficientFame");
    public static readonly ValidationErrorCode AlreadyInterested = new("wanderers.interest.alreadyInterested");

    public static readonly CommandPipeline<WorldState, RegisterWandererInterestCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, RegisterWandererInterestCommand command)
    {
        if (!state.Wanderers.TryGet(command.WandererId, out var wanderer))
            return WandererNotFound;
        if (!wanderer!.IsActivelyTracked || wanderer.Status != WandererStatus.Wandering)
            return WandererUnavailable;
        if (wanderer.CommittedHouseholdId is not null)
            return AlreadyResolved;
        if (!WandererFameCalculator.IsCompetitionVisible(wanderer.Fame))
            return InsufficientFame;
        if (wanderer.InterestedHouseholdIds.Contains(command.HouseholdId))
            return AlreadyInterested;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, RegisterWandererInterestCommand command)
    {
        state.Wanderers.TryGet(command.WandererId, out var wanderer);
        var interested = wanderer!.InterestedHouseholdIds.Append(command.HouseholdId).ToArray();

        state.Wanderers.Remove(command.WandererId);
        state.Wanderers.Add(command.WandererId, wanderer with { InterestedHouseholdIds = interested });

        return new IDomainEvent[]
        {
            new WandererInterestRegisteredEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.WandererId, command.HouseholdId,
                interested.Length, command.CommandId.ToTaggedString()),
        };
    }
}
