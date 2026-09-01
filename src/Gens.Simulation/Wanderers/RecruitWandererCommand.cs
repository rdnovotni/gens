using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Fame;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Wanderers;

/// <summary>§6's Recruit: "a genuine, permanent offer to join the household outright, converting the
/// Wanderer into a full Familia record the instant it succeeds."</summary>
/// <param name="SettlementId">Where the new household member comes to live. Supplied by the caller
/// rather than derived: <see cref="Household"/> is a phantom entity kind in this codebase with no
/// record of its own (see <c>Identity/EntityKinds.cs</c>), so nothing anywhere can answer "which
/// settlement is this household's" — the same reason <c>Characters.PromoteToNamedCommand</c> takes its
/// own <c>SettlementId</c> explicitly.</param>
public sealed record RecruitWandererCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Wanderer> WandererId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Settlement> SettlementId,
    string RandomStreamName) : ICommand;

/// <summary>Emitted whenever a <see cref="RecruitWandererCommand"/> is accepted.</summary>
public sealed record WandererRecruitedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Wanderer> WandererId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> CharacterId,
    RuntimeId<WandererEngagement> EngagementId,
    DutySlot? DutySlot,
    string? CausationId) : IDomainEvent
{
    public string Type => "wanderers.recruited";
    public int SchemaVersion => 1;

    public IReadOnlyList<string> SubjectIds =>
        new[] { WandererId.ToTaggedString(), HouseholdId.ToTaggedString(), CharacterId.ToTaggedString() };

    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// The validate/mutate pipeline for <see cref="RecruitWandererCommand"/> (ADR 0006).
///
/// <para><b>The promotion is Familia's real mechanism, not a parallel one.</b> §6/§9 say a Recruit
/// converts the Wanderer "into a full Familia record... per Familia §7's own existing promotion rule."
/// This codebase's realization of that rule is <c>Characters.PromoteToNamedCommand</c>'s pipeline —
/// roll sex/birth date/identity/attributes/skills/condition from a named stream in a fixed draw order,
/// then <c>Character.Create</c> with a real <see cref="CharacterSource"/> — and this command performs
/// the identical sequence with the identical primitives (<see cref="CharacterBackfillGenerator"/>,
/// <see cref="CharacterVisualProfileGenerator"/>, <see cref="Character.Create"/>), tagged <see
/// cref="CharacterSource.CourtPosition"/>, the enum value <see cref="CharacterSource"/>'s own doc
/// comment already reserves for exactly this ("a Court Position is <c>CourtPosition</c>").
///
/// It deliberately does <i>not</i> submit <c>PromoteToNamedCommand</c> itself, for a mechanical reason
/// rather than a stylistic one: that command's entire contract is decrementing a source <see
/// cref="PopGroup"/> by one, and ADR 0009 makes population conservation "a mechanical property of this
/// single command." A Wanderer was never counted in any settlement's PopGroups — they are an itinerant
/// from elsewhere, arriving from outside the settlement's demographics entirely — so routing through
/// it would silently destroy one unit of a settlement's population to pay for someone who never came
/// from it. The identity fields are replayed verbatim off the <see cref="Wanderer"/> record rather than
/// re-rolled, so the Character genuinely is the person the player has been tracking.</para>
///
/// <para><b>The duty slot reuses Familia's real gates.</b> Where <see
/// cref="WandererTypeProfile.RecruitedDutySlot"/> names a slot (§6's "a hosted physician becomes the
/// household's own Court Physician"), the recruit is placed into it only if they clear the same
/// <c>DutySlotCatalog.MinimumCompetence</c> and <c>DutySlotCatalog.Capacity</c> gates
/// <c>Characters.AssignDutyCommand</c> enforces, read from the recruit's own freshly-rolled <see
/// cref="LaborSkills"/>. Failing either gate is not a rejection: the recruit still joins the household,
/// simply without the slot, and <see cref="WandererEngagement.ResultingDutySlot"/> records which
/// happened. The assignment is written inline as part of <c>Character.Create</c> rather than by
/// submitting <c>AssignDutyCommand</c> afterward, because a command's <c>mutate</c> cannot submit
/// another command's pipeline — the same constraint <c>PromoteToNamedCommand</c> already lives with
/// when it stamps a household onto a freshly-created Character.</para>
///
/// <para><b>§4's universal Fame field is joined here.</b> The new Character's own <see
/// cref="CharacterFame"/> is seeded from the Wanderer's own accumulated Fame through <see
/// cref="FameResolver.Apply"/> — the moment the parallel value stops being parallel and becomes the
/// shared 0-100 field §4 insists it always was. <c>Fame.FameSourceType.WandererRenown</c> already
/// exists as a reserved source for exactly this (its own doc comment names "Wandering Populations" as
/// the unbuilt system it waits on); it is not used as a command here only because <see
/// cref="AdjustFameCommand"/> validates that the Character already exists, and this Character is being
/// created in the same mutate — so the resolver is called directly, exactly as
/// <c>Fame.FameDecaySystem</c> also does.</para>
/// </summary>
public static class RecruitWandererCommands
{
    public static readonly ValidationErrorCode WandererNotFound = new("wanderers.recruit.wandererNotFound");
    public static readonly ValidationErrorCode WandererUnavailable = new("wanderers.recruit.wandererUnavailable");
    public static readonly ValidationErrorCode CommittedElsewhere = new("wanderers.recruit.committedElsewhere");
    public static readonly ValidationErrorCode UnknownSettlement = new("wanderers.recruit.unknownSettlement");

    public static CommandPipeline<WorldState, RecruitWandererCommand> CreatePipeline(
        RandomStreamSet randomStreams, WandererTypeCatalog typeCatalog)
    {
        if (randomStreams is null)
            throw new ArgumentNullException(nameof(randomStreams));
        if (typeCatalog is null)
            throw new ArgumentNullException(nameof(typeCatalog));

        return new CommandPipeline<WorldState, RecruitWandererCommand>(
            validate: (state, command) => Validate(state, command, typeCatalog),
            mutate: (state, command) => Mutate(state, command, randomStreams, typeCatalog),
            issueSequenceNumber: static state => state.IssueCommandSequenceNumber());
    }

    private static ValidationErrorCode? Validate(WorldState state, RecruitWandererCommand command, WandererTypeCatalog typeCatalog)
    {
        if (!state.Wanderers.TryGet(command.WandererId, out var wanderer))
            return WandererNotFound;
        if (!wanderer!.IsActivelyTracked || wanderer.Status != WandererStatus.Wandering)
            return WandererUnavailable;
        if (wanderer.CommittedHouseholdId is { } committed && committed != command.HouseholdId)
            return CommittedElsewhere;
        if (!state.Settlements.TryGet(command.SettlementId, out _))
            return UnknownSettlement;

        _ = typeCatalog.Get(wanderer.Type);
        return null;
    }

    private static IDomainEvent[] Mutate(
        WorldState state, RecruitWandererCommand command, RandomStreamSet randomStreams, WandererTypeCatalog typeCatalog)
    {
        state.Wanderers.TryGet(command.WandererId, out var wanderer);
        var profile = typeCatalog.Get(wanderer!.Type);
        var events = new List<IDomainEvent>();

        // Every fallible step — the random-stream draws below throw KeyNotFoundException for an unknown
        // RandomStreamName — runs before the Ledger is touched, so a bad stream name never leaves a
        // half-applied recruitment fee (debited from the household, credited to the sink, transaction
        // appended) with no Character or Engagement to show for it.
        //
        // Same fixed draw order PromoteToNamedCommands.Mutate uses for the fields it still needs to
        // roll — visual profile, then attributes/skills, then condition. Name/sex/birth date are not
        // re-rolled: they are replayed off the Wanderer record (see this type's own doc comment).
        var visual = CharacterVisualProfileGenerator.Generate(randomStreams, command.RandomStreamName);
        var (attributes, skills) = CharacterBackfillGenerator.RollAttributesAndSkills(randomStreams, command.RandomStreamName);
        var condition = CharacterBackfillGenerator.RollCondition(randomStreams, command.RandomStreamName);

        var dutySlot = ResolveDutySlot(state, command.HouseholdId, profile, skills);

        if (profile.RecruitFee != Money.Zero)
        {
            events.Add(LedgerService.Post(
                state, command.SubmittedDate, LedgerTransactionCategory.Wages,
                new[]
                {
                    new LedgerPosting(LedgerAccountKey.ForHousehold(command.HouseholdId), -profile.RecruitFee),
                    new LedgerPosting(HostWandererCommands.EngagementSink, profile.RecruitFee),
                },
                reference: $"wanderers:recruit:{command.WandererId.ToTaggedString()}:{command.SubmittedDate.TotalMonths}"));
        }

        var characterId = state.CharacterIds.Issue();
        var character = Character.Create(
            id: characterId,
            praenomen: wanderer.Name.Praenomen,
            nomen: wanderer.Name.Nomen,
            cognomen: wanderer.Name.Cognomen,
            sex: wanderer.Sex,
            birthDate: wanderer.BirthDate,
            visualProfile: visual,
            status: wanderer.LegalStatus,
            socialClass: null,
            culture: wanderer.Culture,
            location: command.SettlementId,
            household: command.HouseholdId,
            attributes: attributes,
            skills: skills,
            condition: condition,
            source: CharacterSource.CourtPosition,
            instantiatedAtMonth: command.SubmittedDate.TotalMonths,
            backfilledHistory: true,
            duty: dutySlot is { } slot ? new DutyAssignment(command.HouseholdId, slot, command.SubmittedDate) : null);
        state.Characters.Add(characterId, character);

        // §4's parallel Fame value becomes the shared universal field.
        FameResolver.Apply(state, characterId, wanderer.Fame);

        state.Wanderers.Remove(command.WandererId);
        state.Wanderers.Add(command.WandererId, wanderer with
        {
            Status = WandererStatus.Recruited,
            IsActivelyTracked = false,
            CommittedHouseholdId = command.HouseholdId,
            InterestedHouseholdIds = Array.Empty<RuntimeId<Household>>(),
            RecruitedCharacterId = characterId,
            MonthsSinceLastEngagement = 0,
            // §6: "A successful Recruit ends that Wanderer's own independent Itinerary entirely."
            Itinerary = Array.Empty<WandererItineraryStop>(),
        });

        var engagementId = state.WandererEngagementIds.Issue();
        state.WandererEngagements.Add(engagementId, WandererEngagement.Create(
            engagementId, command.WandererId, command.HouseholdId, WandererEngagementType.Recruit,
            command.SubmittedDate, profile.RecruitFee, dignitasGained: 0,
            wandererFameGained: 0, healthRestored: 0, beneficiaryCharacterId: null,
            resultingCharacterId: characterId, resultingDutySlot: dutySlot));

        events.Add(new WandererRecruitedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.WandererId, command.HouseholdId,
            characterId, engagementId, dutySlot, command.CommandId.ToTaggedString()));

        return events.ToArray();
    }

    /// <summary>Applies <c>Characters.AssignDutyCommand</c>'s own competence and capacity gates to the
    /// recruit's freshly-rolled skills — see this type's doc comment for why the assignment is written
    /// inline rather than by submitting that command.</summary>
    private static DutySlot? ResolveDutySlot(
        WorldState state, RuntimeId<Household> householdId, WandererTypeProfile profile, LaborSkills skills)
    {
        if (profile.RecruitedDutySlot is not { } slot)
            return null;

        if (DutySlotCatalog.RelevantSkillValue(skills, slot) < DutySlotCatalog.MinimumCompetence)
            return null;

        var slotHolders = 0;
        foreach (var entry in state.Characters.InAscendingOrder())
        {
            var member = entry.Value;
            if (member.Household != householdId || !member.IsAlive)
                continue;
            if (member.Duty is { } duty && duty.HouseholdId == householdId && duty.Slot == slot)
                slotHolders++;
        }

        return slotHolders >= DutySlotCatalog.Capacity(slot) ? null : slot;
    }
}
