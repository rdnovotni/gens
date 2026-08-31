using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Legal;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Doctrine;

/// <summary>Emitted whenever an <see cref="InvokeAncestralSanctionCommand"/> is accepted. Public,
/// matching <see cref="Legal.LegalCaseRuledEvent"/>'s own reasoning — overturning a standing verdict is
/// exactly as on-the-record public a fact as the original ruling was.</summary>
public sealed record AncestralSanctionInvokedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<LegalCase> CaseId,
    string? CausationId) : IDomainEvent
{
    public string Type => "doctrine.ancestralSanctionInvoked";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// Mos Maiorum's Defining capstone (§3.2: "once per generation, overturn a Legal &amp; Court ruling
/// against the household without the usual political cost"). Touches <see
/// cref="WorldState.LegalCases"/> directly — a new command reaching into that partition, not a
/// reopening of <see cref="LegalCaseRuling"/>'s own already-shipped, already-tested Apply pipeline —
/// matching <see cref="Scandal.DiscoverFabricationCommand"/>'s identical "a new command touching that
/// partition directly" precedent. Overturns the verdict to <see cref="LegalCaseVerdict.Dismissed"/> and
/// restores a real, partial share of the conviction's own Dignitas penalty (<see
/// cref="DoctrineCatalog.AncestralSanctionDignitasRestored"/>) through <see
/// cref="AdjustDignitasCommand"/> — "without the usual political cost" read here as a real, felt
/// mitigation rather than the case never having happened at all, since this item does not also reverse
/// whatever office loss or sentence the original ruling already carried out (Phase 12 items 2's <see
/// cref="Magistracies.EndMagistracyForConvictionCommand"/> and item 5's own sentence machinery are both
/// already-shipped, already-tested consequences this capstone does not attempt to unwind).
///
/// <b>"Once per generation" is a real, honest cut to "once per campaign":</b> see <see
/// cref="HouseholdDoctrineState.CapstoneUsedThisGeneration"/>'s own doc comment for why no succession-
/// event reset exists yet.
/// </summary>
public sealed record InvokeAncestralSanctionCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<LegalCase> CaseId) : ICommand;

/// <summary>The validate/mutate pipeline for <see cref="InvokeAncestralSanctionCommand"/> (ADR 0006).</summary>
public static class InvokeAncestralSanctionCommands
{
    public static readonly ValidationErrorCode DoctrineNotDefining = new("doctrine.invokeAncestralSanction.doctrineNotDefining");
    public static readonly ValidationErrorCode CapstoneAlreadyUsed = new("doctrine.invokeAncestralSanction.capstoneAlreadyUsed");
    public static readonly ValidationErrorCode CaseNotFound = new("doctrine.invokeAncestralSanction.caseNotFound");
    public static readonly ValidationErrorCode NotDefendant = new("doctrine.invokeAncestralSanction.notDefendant");
    public static readonly ValidationErrorCode NotConvicted = new("doctrine.invokeAncestralSanction.notConvicted");

    public static readonly CommandPipeline<WorldState, InvokeAncestralSanctionCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, InvokeAncestralSanctionCommand command)
    {
        var doctrine = HouseholdDoctrineResolver.Current(state, command.HouseholdId, HouseholdDoctrineType.MosMaiorum);
        // Gates on the persisted CapstoneUnlocked flag, not the current Tier: HouseholdDoctrineState's own
        // doc comment says a capstone once earned stays earned even if Affinity later decays back below
        // Defining — checking Tier directly here would silently contradict that and strand an earned
        // capstone the moment a quiet month ticks it down.
        if (!doctrine.CapstoneUnlocked)
            return DoctrineNotDefining;
        if (doctrine.CapstoneUsedThisGeneration)
            return CapstoneAlreadyUsed;

        if (!state.LegalCases.TryGet(command.CaseId, out var legalCase))
            return CaseNotFound;
        if (legalCase!.DefendantId != command.HouseholdId)
            return NotDefendant;
        if (legalCase.Verdict != LegalCaseVerdict.Convicted)
            return NotConvicted;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, InvokeAncestralSanctionCommand command)
    {
        state.LegalCases.TryGet(command.CaseId, out var legalCase);
        state.LegalCases.Remove(command.CaseId);
        state.LegalCases.Add(command.CaseId, legalCase! with { Verdict = LegalCaseVerdict.Dismissed });

        var doctrine = HouseholdDoctrineResolver.Current(state, command.HouseholdId, HouseholdDoctrineType.MosMaiorum);
        HouseholdDoctrineResolver.Set(state, doctrine with { CapstoneUsedThisGeneration = true });

        var events = new List<IDomainEvent>();
        events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
            state, new AdjustDignitasCommand(
                state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                command.HouseholdId, DoctrineCatalog.AncestralSanctionDignitasRestored, "Mos Maiorum: Ancestral Sanction")).Events);

        events.Add(new AncestralSanctionInvokedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, command.CaseId, command.CommandId.ToTaggedString()));

        return events.ToArray();
    }
}
