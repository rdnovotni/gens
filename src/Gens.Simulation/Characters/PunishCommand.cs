using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>The blunt one-off punishment ladder (<c>gens-labor-slavery-design.md</c> §6), distinct from
/// the standing Regimen (§5). Deliberately deterministic rather than rolled: the design corpus leaves
/// every severity number untuned (§12), and unlike Mild/Moderate/Severe's stat penalties, "near-guaranteed"
/// injury and certain death read most simply as guaranteed outcomes at this system's "basic" scope —
/// Dignitas, Chronicle, and Legal &amp; Court consequences are deferred to whichever future item builds
/// those systems.</summary>
public enum PunishmentTier
{
    Mild,
    Moderate,
    Severe,
    Lethal,
}

/// <summary>Applies one rung of the punishment ladder to a living enslaved Character. <see
/// cref="InjuryTarget"/> is only consulted for <see cref="PunishmentTier.Severe"/> — the caller names
/// which Labor Skill/Core Attribute/Fertility the maiming lands on (<see
/// cref="PermanentInjuryTarget"/> excludes Health itself, per <c>gens-familia-design.md</c> §3.1).</summary>
public sealed record PunishCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    PunishmentTier Tier,
    PermanentInjuryTarget InjuryTarget = PermanentInjuryTarget.Fieldwork) : ICommand;

/// <summary>Emitted whenever a <see cref="PunishCommand"/> is accepted.</summary>
public sealed record CharacterPunishedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    PunishmentTier Tier,
    string? CausationId) : IDomainEvent
{
    public string Type => "characters.punished";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="PunishCommand"/> (ADR 0006).</summary>
public static class PunishCommands
{
    /// <summary>Invented baseline (§12's severity numbers are untuned) — the fixed Fatigue/Loyalty/Health
    /// penalty each tier applies, plus the fixed injury magnitude Severe applies via the same <see
    /// cref="PermanentInjury"/> primitive <see cref="ApplyPermanentInjuryCommand"/> uses.</summary>
    private const int MildFatiguePenalty = 10;
    private const int MildLoyaltyPenalty = 5;
    private const int ModerateFatiguePenalty = 20;
    private const int ModerateLoyaltyPenalty = 15;
    private const int ModerateHealthPenalty = 10;
    private const int SevereLoyaltyPenalty = 30;
    private const int SevereInjuryMagnitude = 30;
    private const string SeverePunishmentInjuryCause = "Severe punishment";

    public static readonly ValidationErrorCode CharacterNotFound = new("characters.punish.characterNotFound");
    public static readonly ValidationErrorCode CharacterDeceased = new("characters.punish.characterDeceased");
    public static readonly ValidationErrorCode NotEnslaved = new("characters.punish.notEnslaved");

    public static readonly CommandPipeline<WorldState, PunishCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, PunishCommand command)
    {
        if (!state.Characters.TryGet(command.CharacterId, out var character))
            return CharacterNotFound;
        if (!character.IsAlive)
            return CharacterDeceased;
        if (character.LegalStatus != LegalStatus.Enslaved)
            return NotEnslaved;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, PunishCommand command)
    {
        state.Characters.TryGet(command.CharacterId, out var character);
        var updated = command.Tier switch
        {
            PunishmentTier.Mild => character with
            {
                Condition = new Condition(
                    character.Condition.Health,
                    Math.Clamp(character.Condition.Fatigue + MildFatiguePenalty, 0, 100),
                    Math.Clamp(character.Condition.Loyalty - MildLoyaltyPenalty, 0, 100),
                    character.Condition.Ambition, character.Condition.Fertility),
            },
            PunishmentTier.Moderate => character with
            {
                Condition = new Condition(
                    Math.Clamp(character.Condition.Health - ModerateHealthPenalty, 0, 100),
                    Math.Clamp(character.Condition.Fatigue + ModerateFatiguePenalty, 0, 100),
                    Math.Clamp(character.Condition.Loyalty - ModerateLoyaltyPenalty, 0, 100),
                    character.Condition.Ambition, character.Condition.Fertility),
            },
            PunishmentTier.Severe => character with
            {
                Condition = new Condition(
                    character.Condition.Health, character.Condition.Fatigue,
                    Math.Clamp(character.Condition.Loyalty - SevereLoyaltyPenalty, 0, 100),
                    character.Condition.Ambition, character.Condition.Fertility),
                PermanentInjuries = character.PermanentInjuries
                    .Append(new PermanentInjury(command.InjuryTarget, SevereInjuryMagnitude, SeverePunishmentInjuryCause, command.SubmittedDate))
                    .ToArray(),
            },
            PunishmentTier.Lethal => character with
            {
                DeathRecord = new DeathRecord(command.SubmittedDate, DeathCause.Violence, character.AgeInYears(command.SubmittedDate)),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(command), command.Tier, "Unknown punishment tier."),
        };

        state.Characters.Remove(command.CharacterId);
        state.Characters.Add(command.CharacterId, updated);

        return new IDomainEvent[]
        {
            new CharacterPunishedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CharacterId, command.Tier,
                command.CommandId.ToTaggedString()),
        };
    }
}
