using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Religion;

/// <summary>
/// Seats a Character in a <see cref="PriesthoodOffice"/> (Phase 12 item 3; §6.2). §6.2's own gate —
/// "gated by the Piety trait tier (Devout or Zealous, Traits §3.5) and Learning rather than by Politics
/// &amp; Patronage's Dignitas/citizenship gate alone, though citizenship still applies per Familia
/// §2.5's own restriction" — is checked directly: <see cref="AppointDecurionCommand"/>'s own
/// citizenship check (Roman Citizen or Latin Rights) plus a Piety-tier and Learning threshold in place
/// of that command's Dignitas floor.
/// </summary>
public sealed record AppointPriesthoodCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    RuntimeId<Settlement> SettlementId,
    PriesthoodOffice Office,
    PatronDeity? FlamenDeity) : ICommand;

/// <summary>Emitted whenever an <see cref="AppointPriesthoodCommand"/> is accepted. Public, matching
/// <see cref="Magistracies.MagistracyAssumedEvent"/>'s own "legible to the political class by
/// definition" reasoning — a public Priesthood is, if anything, more publicly known than a Curia seat.</summary>
public sealed record PriesthoodAssumedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<PriesthoodRecord> RecordId,
    RuntimeId<Character> HolderId,
    PriesthoodOffice Office,
    RuntimeId<Settlement> SettlementId,
    string? CausationId) : IDomainEvent
{
    public string Type => "religion.priesthoodAssumed";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HolderId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="AppointPriesthoodCommand"/> (ADR 0006).</summary>
public static class AppointPriesthoodCommands
{
    public static readonly ValidationErrorCode CharacterNotFound = new("religion.appointPriesthood.characterNotFound");
    public static readonly ValidationErrorCode CharacterDeceased = new("religion.appointPriesthood.characterDeceased");
    public static readonly ValidationErrorCode IneligibleLegalStatus = new("religion.appointPriesthood.ineligibleLegalStatus");
    public static readonly ValidationErrorCode InsufficientPiety = new("religion.appointPriesthood.insufficientPiety");
    public static readonly ValidationErrorCode InsufficientLearning = new("religion.appointPriesthood.insufficientLearning");
    public static readonly ValidationErrorCode AlreadyHoldsOffice = new("religion.appointPriesthood.alreadyHoldsOffice");
    public static readonly ValidationErrorCode PontifexRequiresPriorOffice = new("religion.appointPriesthood.pontifexRequiresPriorOffice");
    public static readonly ValidationErrorCode FlamenDeityRequired = new("religion.appointPriesthood.flamenDeityRequired");
    public static readonly ValidationErrorCode FlamenDeityMismatch = new("religion.appointPriesthood.flamenDeityMismatch");
    public static readonly ValidationErrorCode UnexpectedFlamenDeity = new("religion.appointPriesthood.unexpectedFlamenDeity");

    public static readonly CommandPipeline<WorldState, AppointPriesthoodCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, AppointPriesthoodCommand command)
    {
        if (!state.Characters.TryGet(command.CharacterId, out var character))
            return CharacterNotFound;
        if (!character!.IsAlive)
            return CharacterDeceased;
        // §6.2: "citizenship still applies per Familia §2.5's own restriction," matching
        // AppointDecurionCommand's identical gate.
        if (character.LegalStatus is not (LegalStatus.RomanCitizen or LegalStatus.LatinRights))
            return IneligibleLegalStatus;
        if (!character.Traits.Contains(ReligionCatalog.DevoutTraitId) && !character.Traits.Contains(ReligionCatalog.ZealousTraitId))
            return InsufficientPiety;
        if (character.GetEffectiveAttributes().Learning < ReligionCatalog.PriesthoodLearningThreshold)
            return InsufficientLearning;
        if (PriesthoodResolver.ActiveRecord(state, command.SettlementId, command.Office, command.CharacterId) is not null)
            return AlreadyHoldsOffice;

        if (command.Office == PriesthoodOffice.Pontifex && PriesthoodResolver.AnyActiveRecordFor(state, command.CharacterId) is null)
            return PontifexRequiresPriorOffice;

        if (command.Office == PriesthoodOffice.Flamen)
        {
            if (command.FlamenDeity is not { } deity)
                return FlamenDeityRequired;
            if (character.Household is not { } householdId ||
                !state.HouseholdReligions.TryGet(householdId, out var religion) || religion!.PatronDeity != deity)
                return FlamenDeityMismatch;
        }
        else if (command.FlamenDeity is not null)
        {
            return UnexpectedFlamenDeity;
        }

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, AppointPriesthoodCommand command)
    {
        state.Characters.TryGet(command.CharacterId, out var character);

        var recordId = state.PriesthoodRecordIds.Issue();
        state.PriesthoodRecords.Add(
            recordId,
            new PriesthoodRecord(
                recordId, command.CharacterId, command.Office, command.SettlementId, command.SubmittedDate, command.FlamenDeity));

        var events = new List<IDomainEvent>
        {
            new PriesthoodAssumedEvent(
                state.EventIds.Issue(), command.SubmittedDate, recordId, command.CharacterId, command.Office,
                command.SettlementId, command.CommandId.ToTaggedString()),
        };

        if (character!.Household is { } holderHouseholdId)
        {
            var dignitasCommand = new AdjustDignitasCommand(
                state.CommandIds.Issue(), "system", command.SubmittedDate, command.CommandId.ToTaggedString(), holderHouseholdId,
                ReligionCatalog.PriesthoodAssumedDignitasGain, $"assumed the {command.Office} priesthood");
            events.AddRange(AdjustDignitasCommands.Pipeline.Execute(state, dignitasCommand).Events);

            if (HouseholdReligionResolver.HasChosenPatron(state, holderHouseholdId))
            {
                var favorCommand = new AdjustFavorCommand(
                    state.CommandIds.Issue(), "system", command.SubmittedDate, command.CommandId.ToTaggedString(), holderHouseholdId,
                    ReligionCatalog.PriesthoodAssumedFavorGain, $"assumed the {command.Office} priesthood");
                events.AddRange(AdjustFavorCommands.Pipeline.Execute(state, favorCommand).Events);
            }
        }

        return events.ToArray();
    }
}
