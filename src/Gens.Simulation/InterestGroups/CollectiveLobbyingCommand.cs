using Gens.Simulation.Clientela;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.InterestGroups;

/// <summary>
/// §5's Collective Lobbying: "the group-scale version of... individual lobbying... pooling Influence
/// from every participating household." §5's own stated target — moving a live Edict's own Reception
/// (Policies &amp; Edicts §5.1) — does not exist to move: Policies &amp; Edicts is entirely unbuilt
/// (Phase 12 item 9), so there is no Reception value anywhere in this codebase for a pooled lobbying
/// effort to shift. What this command builds instead is the one real, reachable half of §5's own
/// mechanism it actually can move: <see cref="HouseholdInfluence"/> itself, pooled from every
/// contributing household and credited to one beneficiary household, who can then spend the combined
/// total through the one real Influence-spending consumer this codebase has, <see
/// cref="Magistracies.HoldContestedElectionCommand"/> — "pooling Influence... more sharply than any
/// single household's own lobbying could," reinterpreted as a direct transfer rather than an Edict-
/// Reception shift that has nothing to attach to yet. §5's second action, the Curia Faction Bloc, is a
/// deliberate, separate cut — see <see cref="InterestGroupResolver"/>'s own doc comment for why §3's
/// Provincial Patronage is likewise not built.
/// </summary>
public sealed record CollectiveLobbyingCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    IReadOnlyList<RuntimeId<Household>> ContributingHouseholdIds,
    RuntimeId<Household> BeneficiaryHouseholdId,
    int InfluencePerContributor) : ICommand;

/// <summary>Emitted whenever a <see cref="CollectiveLobbyingCommand"/> is accepted. Public — a bloc's
/// pooled show of political support is a real, visible coalition act, the same reasoning §6's collegium
/// endorsement mechanic already treats an organized bloc's activity as legible rather than a private
/// favor.</summary>
public sealed record CollectiveLobbyingPooledEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    IReadOnlyList<RuntimeId<Household>> ContributingHouseholdIds,
    RuntimeId<Household> BeneficiaryHouseholdId,
    int TotalInfluencePooled,
    string? CausationId) : IDomainEvent
{
    public string Type => "interestGroups.collectiveLobbyingPooled";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => ContributingHouseholdIds
        .Append(BeneficiaryHouseholdId)
        .Select(id => id.ToTaggedString())
        .Distinct(StringComparer.Ordinal)
        .ToArray();
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="CollectiveLobbyingCommand"/> (ADR 0006).</summary>
public static class CollectiveLobbyingCommands
{
    public static readonly ValidationErrorCode NoContributors = new("interestGroups.collectiveLobbying.noContributors");
    public static readonly ValidationErrorCode InfluencePerContributorMustBePositive =
        new("interestGroups.collectiveLobbying.influencePerContributorMustBePositive");
    public static readonly ValidationErrorCode BeneficiaryHasNoHead = new("interestGroups.collectiveLobbying.beneficiaryHasNoHead");
    public static readonly ValidationErrorCode ContributorHasNoHead = new("interestGroups.collectiveLobbying.contributorHasNoHead");
    public static readonly ValidationErrorCode BeneficiaryIsContributor = new("interestGroups.collectiveLobbying.beneficiaryIsContributor");
    public static readonly ValidationErrorCode InsufficientInfluence = new("interestGroups.collectiveLobbying.insufficientInfluence");

    public static readonly CommandPipeline<WorldState, CollectiveLobbyingCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, CollectiveLobbyingCommand command)
    {
        if (command.ContributingHouseholdIds.Count == 0)
            return NoContributors;
        if (command.InfluencePerContributor <= 0)
            return InfluencePerContributorMustBePositive;
        if (!state.HouseholdHeadships.TryGet(command.BeneficiaryHouseholdId, out _))
            return BeneficiaryHasNoHead;

        foreach (var contributorId in command.ContributingHouseholdIds)
        {
            if (contributorId == command.BeneficiaryHouseholdId)
                return BeneficiaryIsContributor;
            if (!state.HouseholdHeadships.TryGet(contributorId, out _))
                return ContributorHasNoHead;
            if (InfluenceResolver.Current(state, contributorId) < command.InfluencePerContributor)
                return InsufficientInfluence;
        }

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, CollectiveLobbyingCommand command)
    {
        foreach (var contributorId in command.ContributingHouseholdIds)
            InfluenceResolver.Apply(state, contributorId, -command.InfluencePerContributor);

        var totalPooled = command.ContributingHouseholdIds.Count * command.InfluencePerContributor;
        InfluenceResolver.Apply(state, command.BeneficiaryHouseholdId, totalPooled);

        return new IDomainEvent[]
        {
            new CollectiveLobbyingPooledEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.ContributingHouseholdIds,
                command.BeneficiaryHouseholdId, totalPooled, command.CommandId.ToTaggedString()),
        };
    }
}
