using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Wanderers;

/// <summary>§6's Host: "a one-time, lower-commitment engagement... without recruiting the Wanderer into
/// the household — they remain independent, and move on afterward per their own Itinerary
/// (§3)."</summary>
public sealed record HostWandererCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Wanderer> WandererId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character>? BeneficiaryCharacterId = null) : ICommand;

/// <summary>Emitted whenever a <see cref="HostWandererCommand"/> is accepted.</summary>
public sealed record WandererHostedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Wanderer> WandererId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<WandererEngagement> EngagementId,
    int DignitasGained,
    int WandererFameGained,
    int HealthRestored,
    string? CausationId) : IDomainEvent
{
    public string Type => "wanderers.hosted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { WandererId.ToTaggedString(), HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// The validate/mutate pipeline for <see cref="HostWandererCommand"/> (ADR 0006).
///
/// <para><b>Which of §6's five named Host benefits are real here, and which are deferred.</b> §6 lists
/// "a Cultural Prestige boost, a construction discount, a rare goods purchase, a Health recovery, a
/// Favor gain." Two are delivered through mechanisms that genuinely already exist:
/// <list type="bullet">
/// <item><b>Cultural Prestige boost → Dignitas.</b> No Cultural Prestige field exists anywhere in this
/// codebase. <see cref="HouseholdReputation"/>'s Dignitas is the real, already-built household-standing
/// primitive — its own doc comment describes it as "moved by nearly everything... Villa Grandeur,
/// Monuments, Funded Actions" — so a hosted lecture or performance moves it through <see
/// cref="DignitasResolver.Apply"/>, the same path <c>Reputation.AdjustDignitasCommand</c> uses. This is
/// a disclosed substitution, not a claim that Cultural Prestige was built.</item>
/// <item><b>Health recovery → real <see cref="Condition.Health"/>.</b> A <see
/// cref="WandererType.Physician"/> Host restores <see cref="PhysicianHealthRecovery"/> points of Health
/// to a named household member, clamped at 100 by <c>Characters.StatRange</c>'s own 0-100 range. This
/// is deliberately a Condition-stat recovery rather than a <c>Health.CharacterHealthCondition</c>
/// resolution: that namespace's own <c>CharacterHealthConditionSystem</c> owns case resolution end to
/// end (care-capacity allocation, then drain, then a recovery/fatality roll), and reaching in from
/// outside to resolve a case would bypass every one of those gates. Wiring an itinerant Physician into
/// that system's own care-capacity budget (<c>Health.CareCapacityCalculator</c>, which reads a
/// household member's <c>DutySlot.Physician</c> assignment) is a real, worthwhile follow-up and is
/// named here as deferred — a Hosted Physician is not a household member and holds no duty slot, so
/// that calculator has nothing to read them off yet.</item>
/// </list>
/// The other three are deferred and named rather than faked, the same discipline this phase's items 1-3
/// used throughout: a <b>construction discount</b> would need Buildings to expose a per-commission cost
/// hook, and <c>Buildings/</c> exposes no construction-cost or in-progress-commission concept at all; a
/// <b>rare goods purchase</b> would need Markets to expose a "buy this specific rare good at this
/// place" path, and <c>Markets/</c>'s real surface is per-settlement clearing prices, with no rare-goods
/// tier and (per <see cref="WandererItineraryStop"/>'s own disclosure) no link from a Gazetteer entry to
/// a settlement to buy at; and a <b>Favor gain</b> would need <c>Reputation.FavorObligation</c>, which
/// is real but is strictly Character-to-Character — a Wanderer is by construction not yet a Character
/// (§8), so there is no grantor to open the obligation against until a Recruit converts them.</para>
///
/// <para><b>No co-location check.</b> §5's whole premise is that the player must actually reach the
/// Wanderer, and this command cannot enforce that: a Wanderer's location is a Gazetteer entry and a
/// household's is a runtime Settlement, with no link between the two in this codebase (<see
/// cref="WandererItineraryStop"/>). Disclosed rather than approximated with a check that would be
/// wrong.</para>
///
/// <para><b>No funds gate.</b> The fee is posted unconditionally: <see cref="Money"/>'s own doc comment
/// records that a Treasury "can run negative," and <c>Health.SanitationInvestmentSystem</c> already
/// posts its recurring cost the same way rather than skipping a poor settlement.</para>
/// </summary>
public static class HostWandererCommands
{
    /// <summary>The Health points a <see cref="WandererType.Physician"/> Host restores. This
    /// implementation's own invented figure (§11's "All numeric sizing"), sized as a real but bounded
    /// course of treatment rather than a cure.</summary>
    public const int PhysicianHealthRecovery = 15;

    /// <summary>The named conservation boundary engagement fees flow out to, matching
    /// <c>Health.SanitationInvestmentSystem</c>'s identical System-account sink shape.</summary>
    public static readonly LedgerAccountKey EngagementSink = new(LedgerAccountKind.System, "wanderers:engagement");

    public static readonly ValidationErrorCode WandererNotFound = new("wanderers.host.wandererNotFound");
    public static readonly ValidationErrorCode WandererUnavailable = new("wanderers.host.wandererUnavailable");
    public static readonly ValidationErrorCode CommittedElsewhere = new("wanderers.host.committedElsewhere");
    public static readonly ValidationErrorCode BeneficiaryNotFound = new("wanderers.host.beneficiaryNotFound");
    public static readonly ValidationErrorCode BeneficiaryDeceased = new("wanderers.host.beneficiaryDeceased");
    public static readonly ValidationErrorCode BeneficiaryNotHouseholdMember = new("wanderers.host.beneficiaryNotHouseholdMember");
    public static readonly ValidationErrorCode BeneficiaryNotTreatable = new("wanderers.host.beneficiaryNotTreatable");

    public static CommandPipeline<WorldState, HostWandererCommand> CreatePipeline(WandererTypeCatalog typeCatalog)
    {
        if (typeCatalog is null)
            throw new ArgumentNullException(nameof(typeCatalog));

        return new CommandPipeline<WorldState, HostWandererCommand>(
            validate: (state, command) => Validate(state, command, typeCatalog),
            mutate: (state, command) => Mutate(state, command, typeCatalog),
            issueSequenceNumber: static state => state.IssueCommandSequenceNumber());
    }

    private static ValidationErrorCode? Validate(WorldState state, HostWandererCommand command, WandererTypeCatalog typeCatalog)
    {
        if (!state.Wanderers.TryGet(command.WandererId, out var wanderer))
            return WandererNotFound;
        if (!wanderer!.IsActivelyTracked || wanderer.Status != WandererStatus.Wandering)
            return WandererUnavailable;

        // §7, resolved the instant either side commits: once any household has committed, nobody else
        // gets in. The committing household itself may still Host again — §11 leaves Host
        // repeatability open, and refusing a household its own second engagement would be inventing an
        // answer that section explicitly declines to give.
        if (wanderer.CommittedHouseholdId is { } committed && committed != command.HouseholdId)
            return CommittedElsewhere;

        if (command.BeneficiaryCharacterId is { } beneficiaryId)
        {
            if (wanderer.Type != WandererType.Physician)
                return BeneficiaryNotTreatable;
            if (!state.Characters.TryGet(beneficiaryId, out var beneficiary))
                return BeneficiaryNotFound;
            if (!beneficiary!.IsAlive)
                return BeneficiaryDeceased;
            if (beneficiary.Household != command.HouseholdId)
                return BeneficiaryNotHouseholdMember;
        }

        // Guarantees Mutate's own Get cannot throw for an unauthored type.
        _ = typeCatalog.Get(wanderer.Type);
        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, HostWandererCommand command, WandererTypeCatalog typeCatalog)
    {
        state.Wanderers.TryGet(command.WandererId, out var wanderer);
        var profile = typeCatalog.Get(wanderer!.Type);
        var events = new List<IDomainEvent>();

        if (profile.HostFee != Money.Zero)
        {
            events.Add(LedgerService.Post(
                state, command.SubmittedDate, LedgerTransactionCategory.Wages,
                new[]
                {
                    new LedgerPosting(LedgerAccountKey.ForHousehold(command.HouseholdId), -profile.HostFee),
                    new LedgerPosting(EngagementSink, profile.HostFee),
                },
                reference: $"wanderers:host:{command.WandererId.ToTaggedString()}:{command.SubmittedDate.TotalMonths}"));
        }

        // §4's "a genuinely more valuable Host... target" made literal: the delivered benefit scales
        // with the Wanderer's own current Fame (see WandererFameCalculator.ScaleByFame's own doc
        // comment), not just the fixed per-type base amount.
        var dignitasGain = WandererFameCalculator.ScaleByFame(profile.HostDignitasGain, wanderer.Fame);
        DignitasResolver.Apply(state, command.HouseholdId, dignitasGain);

        var healthRestored = 0;
        if (command.BeneficiaryCharacterId is { } beneficiaryId)
        {
            state.Characters.TryGet(beneficiaryId, out var beneficiary);
            var condition = beneficiary!.Condition;
            var healthRecovery = WandererFameCalculator.ScaleByFame(PhysicianHealthRecovery, wanderer.Fame);
            var restoredHealth = Math.Min(100, condition.Health + healthRecovery);
            healthRestored = restoredHealth - condition.Health;
            state.Characters.Remove(beneficiaryId);
            state.Characters.Add(beneficiaryId, beneficiary with
            {
                Condition = new Condition(
                    restoredHealth, condition.Fatigue, condition.Loyalty, condition.Ambition, condition.Fertility),
            });
        }

        var previousFame = wanderer.Fame;
        var newFame = WandererFameCalculator.ApplyDelta(previousFame, profile.EngagementFameGain);
        state.Wanderers.Remove(command.WandererId);
        state.Wanderers.Add(command.WandererId, wanderer with
        {
            Fame = newFame,
            FameTrend = WandererFameCalculator.Trend(previousFame, newFame),
            MonthsSinceLastEngagement = 0,
            CommittedHouseholdId = command.HouseholdId,
            InterestedHouseholdIds = Array.Empty<RuntimeId<Household>>(),
        });

        var engagementId = state.WandererEngagementIds.Issue();
        state.WandererEngagements.Add(engagementId, WandererEngagement.Create(
            engagementId, command.WandererId, command.HouseholdId, WandererEngagementType.Host,
            command.SubmittedDate, profile.HostFee, dignitasGain, newFame - previousFame,
            healthRestored, command.BeneficiaryCharacterId));

        events.Add(new WandererHostedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.WandererId, command.HouseholdId,
            engagementId, dignitasGain, newFame - previousFame, healthRestored,
            command.CommandId.ToTaggedString()));

        return events.ToArray();
    }
}
