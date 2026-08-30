using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Magistracies;

/// <summary>
/// Ends every active <see cref="MagistracyRecord"/> held by a member of <see cref="HouseholdId"/> with
/// <see cref="MagistracyLossReason.LegalConviction"/> (Phase 12 item 4; §5.7's "magistracy-loss-by-
/// conviction case," §10 of <c>gens-legal-court-design.md</c>). This is the future caller <see
/// cref="MagistracyLossReason.LegalConviction"/>'s own doc comment named as "genuinely unreachable in
/// this codebase today" when Phase 12 item 2 shipped it — <see cref="Legal.LegalCaseRuling"/> is that
/// caller, submitting this command directly from a different domain the exact way <see
/// cref="Magistracies.MagistracyTermSystem"/> itself already calls into <see
/// cref="Reputation.AdjustDignitasCommand"/> across a domain boundary (rule 2's shared command path,
/// applied here to office-ending instead of Dignitas). A household-level target, not a specific holder
/// id: a Political case's defendant is a household (see <see cref="Legal.LegalCase"/>'s own scope
/// decision), and a household in this codebase can hold more than one active office across its members —
/// every one of them ends, mirroring how a single <see cref="Legal.LegalCase"/> verdict is meant to
/// strip the household's entire political standing, not just one seat picked arbitrarily.
/// </summary>
public sealed record EndMagistracyForConvictionCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId) : ICommand;

/// <summary>The validate/mutate pipeline for <see cref="EndMagistracyForConvictionCommand"/> (ADR 0006).</summary>
public static class EndMagistracyForConvictionCommands
{
    public static readonly ValidationErrorCode NoActiveOffice = new("magistracies.endForConviction.noActiveOffice");

    public static readonly CommandPipeline<WorldState, EndMagistracyForConvictionCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, EndMagistracyForConvictionCommand command)
    {
        foreach (var entry in state.MagistracyRecords.InAscendingOrder())
        {
            var record = entry.Value;
            if (!MagistracyResolver.IsActive(record))
                continue;
            if (state.Characters.TryGet(record.HolderId, out var holder) && holder!.Household == command.HouseholdId)
                return null;
        }

        return NoActiveOffice;
    }

    private static IDomainEvent[] Mutate(WorldState state, EndMagistracyForConvictionCommand command)
    {
        var events = new List<IDomainEvent>();

        foreach (var entry in state.MagistracyRecords.InAscendingOrder().ToArray())
        {
            var record = entry.Value;
            if (!MagistracyResolver.IsActive(record))
                continue;
            if (!state.Characters.TryGet(record.HolderId, out var holder) || holder!.Household != command.HouseholdId)
                continue;

            state.MagistracyRecords.Remove(record.RecordId);
            state.MagistracyRecords.Add(record.RecordId, record with { TermEndDate = command.SubmittedDate, LossReason = MagistracyLossReason.LegalConviction });

            events.Add(new MagistracyLostEvent(
                state.EventIds.Issue(), command.SubmittedDate, record.RecordId, record.HolderId, record.Office,
                MagistracyLossReason.LegalConviction, command.CommandId.ToTaggedString()));
        }

        return events.ToArray();
    }
}
