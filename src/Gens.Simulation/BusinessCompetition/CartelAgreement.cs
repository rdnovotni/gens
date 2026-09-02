using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.NotableBusinesses;
using Gens.Simulation.Scandal;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.BusinessCompetition;

/// <summary>§4's two named cartel shapes.</summary>
public enum CartelAgreementType
{
    PriceFixing,
    MarketSharingByDistrict,
}

/// <summary>
/// §4's/§9's <c>CartelAgreement</c> data model (Phase 15 item 5) — "the real, cooperative alternative to
/// §2's own competitive ladder." Unlike <see cref="CompetitiveEscalation"/> (naturally two-party, so it
/// reuses an already-registered <see cref="RuntimeId{NotableBusiness}"/> as its own key), a cartel is
/// genuinely N-ary (§4: "two or more Notable Businesses"), so this record needs a real identity of its
/// own — a fresh <see cref="RuntimeId{CartelAgreement}"/>, matching every other Phase 12+ "real record as
/// its own tag" entity (<see cref="RealEstate.District"/>, <see cref="Societates.Societas"/>, <see
/// cref="NotableBusiness"/>).
/// </summary>
public sealed record CartelAgreement(
    RuntimeId<CartelAgreement> CartelId,
    IReadOnlyList<RuntimeId<NotableBusiness>> ParticipantBusinessIds,
    CartelAgreementType AgreementType,
    bool IsDiscovered,
    RuntimeId<NotableBusiness>? BreakingParticipantId);

public static class CartelAgreementResolver
{
    public static bool TryGetCurrent(WorldState state, RuntimeId<CartelAgreement> cartelId, out CartelAgreement cartel) =>
        state.CartelAgreements.TryGet(cartelId, out cartel!);
}

/// <summary>§4's real, deliberate act of collusion: two or more already-Tracked Notable Businesses agree
/// to fix prices or divide a settlement's customer base by District. Deliberately does not require the
/// participants to already be Main Competitors or fellow Collegium members — §4's own text names both as
/// "often but not always" true, not a hard precondition.</summary>
public sealed record FormCartelCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    IReadOnlyList<RuntimeId<NotableBusiness>> ParticipantBusinessIds,
    CartelAgreementType AgreementType) : ICommand;

public sealed record CartelFormedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<CartelAgreement> CartelId,
    IReadOnlyList<RuntimeId<NotableBusiness>> ParticipantBusinessIds,
    CartelAgreementType AgreementType,
    string? CausationId) : IDomainEvent
{
    public string Type => "businessCompetition.cartelFormed";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => ParticipantBusinessIds.Select(id => id.ToTaggedString()).ToArray();
    public Visibility Visibility => Visibility.Public;
}

public static class FormCartelCommands
{
    public static readonly ValidationErrorCode TooFewParticipants = new("businessCompetition.formCartel.tooFewParticipants");
    public static readonly ValidationErrorCode DuplicateParticipant = new("businessCompetition.formCartel.duplicateParticipant");
    public static readonly ValidationErrorCode ParticipantNotFound = new("businessCompetition.formCartel.participantNotFound");
    public static readonly ValidationErrorCode ParticipantNotTracked = new("businessCompetition.formCartel.participantNotTracked");

    public static readonly CommandPipeline<WorldState, FormCartelCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, FormCartelCommand command)
    {
        if (command.ParticipantBusinessIds.Count < 2)
            return TooFewParticipants;
        if (command.ParticipantBusinessIds.Distinct().Count() != command.ParticipantBusinessIds.Count)
            return DuplicateParticipant;

        foreach (var businessId in command.ParticipantBusinessIds)
        {
            if (!state.NotableBusinesses.TryGet(businessId, out var business))
                return ParticipantNotFound;
            if (business!.Status != NotableBusinessStatus.Tracked)
                return ParticipantNotTracked;
        }

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, FormCartelCommand command)
    {
        var cartelId = state.CartelAgreementIds.Issue();
        var cartel = new CartelAgreement(cartelId, command.ParticipantBusinessIds, command.AgreementType, IsDiscovered: false, BreakingParticipantId: null);
        state.CartelAgreements.Add(cartelId, cartel);

        return new IDomainEvent[]
        {
            new CartelFormedEvent(
                state.EventIds.Issue(), command.SubmittedDate, cartelId, command.ParticipantBusinessIds, command.AgreementType,
                command.CommandId.ToTaggedString()),
        };
    }
}

/// <summary>
/// §4's own fragility: monthly, for every cartel not yet discovered and not yet broken, checks each
/// participant's own resolved owner Character against §4's own named Reactive-Trait temptation — Ambition
/// clearing <see cref="BusinessCompetitionCatalog.CartelDefectionAmbitionThreshold"/>, or carrying <see
/// cref="BusinessCompetitionCatalog.GreedyTraitId"/> — matching <see
/// cref="Societates.PartnerSkimmingRiskSystem"/>'s and <see
/// cref="Societates.PartnerDisputeRiskQuery"/>'s identical Ambition/Greed reasoning applied here to a
/// different betrayal shape. The first qualifying participant (ascending order, deterministic per ADR
/// 0004) actually defects: <see cref="CartelAgreement.BreakingParticipantId"/> is set once and never
/// reconsidered thereafter (a defection, once it happens, does not un-happen), and — §4's own "converting
/// a cartel into exactly the kind of betrayal §3 already describes, now aimed at co-conspirators" — this
/// system fires one real <see cref="NotableBusinesses.RecordBusinessRivalryActionCommand"/> (<see
/// cref="BusinessRivalryActionType.PriceUndercut"/>) from the defector against the next participant in
/// the roster, reusing §5's own real rivalry mechanism directly rather than inventing a second Reputation
/// mover for the identical shape of act. That reuse is best-effort exactly like <see
/// cref="NotableBusinesses.RecordBusinessRivalryActionCommands"/>'s own Sabotage/Damaging-Rumor Scheme
/// call: it requires the two businesses to already be each other's Main Competitor (§5's own gate), which
/// a cartel does not itself establish, so an ordinary defection often lands with no Main-Competitor
/// rivalry to attach to — <see cref="CartelAgreement.BreakingParticipantId"/> is still set either way, the
/// one always-reachable, guaranteed consequence.</summary>
public static class CartelDefectionRiskSystem
{
    public static IReadOnlyList<IDomainEvent> Tick(WorldState state, GameDate date)
    {
        var events = new List<IDomainEvent>();

        foreach (var entry in state.CartelAgreements.InAscendingOrder().ToArray())
        {
            var cartel = entry.Value;
            if (cartel.IsDiscovered || cartel.BreakingParticipantId is not null)
                continue;

            RuntimeId<NotableBusiness>? defector = null;
            foreach (var participantId in cartel.ParticipantBusinessIds)
            {
                if (!state.NotableBusinesses.TryGet(participantId, out var participant))
                    continue;
                if (!NotableBusinessOwnerResolver.TryResolveCharacter(state, participant!.Owner, out var characterId))
                    continue;
                if (!state.Characters.TryGet(characterId, out var character))
                    continue;

                var tempted = character!.Condition.Ambition >= BusinessCompetitionCatalog.CartelDefectionAmbitionThreshold
                    || character.Traits.Contains(BusinessCompetitionCatalog.GreedyTraitId);
                if (tempted)
                {
                    defector = participantId;
                    break;
                }
            }

            if (defector is not { } defectorId)
                continue;

            state.CartelAgreements.Remove(entry.Key);
            state.CartelAgreements.Add(entry.Key, cartel with { BreakingParticipantId = defectorId });

            var victim = cartel.ParticipantBusinessIds.Where(id => id != defectorId).Select(id => (RuntimeId<NotableBusiness>?)id).FirstOrDefault();
            if (victim is { } victimId && state.NotableBusinesses.TryGet(defectorId, out var defectorBusiness)
                && state.NotableBusinesses.TryGet(victimId, out var victimBusiness)
                && defectorBusiness!.MainCompetitorBusinessId == victimId && victimBusiness!.MainCompetitorBusinessId == defectorId)
            {
                var rivalryResult = RecordBusinessRivalryActionCommands.Pipeline.Execute(
                    state, new RecordBusinessRivalryActionCommand(
                        state.CommandIds.Issue(), "system", date, null, defectorId, victimId, BusinessRivalryActionType.PriceUndercut));
                if (rivalryResult.Accepted)
                    events.AddRange(rivalryResult.Events);
            }

            events.Add(new CartelDefectionDetectedEvent(state.EventIds.Issue(), date, entry.Key, defectorId, CausationId: null));
        }

        return events;
    }
}

public sealed record CartelDefectionDetectedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<CartelAgreement> CartelId,
    RuntimeId<NotableBusiness> BreakingParticipantId,
    string? CausationId) : IDomainEvent
{
    public string Type => "businessCompetition.cartelDefectionDetected";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CartelId.ToTaggedString(), BreakingParticipantId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// §10's own open question — "whether a cartel's own discovery should route through the Scandal system
/// directly... this document assumes yes but doesn't formally amend that list itself" — this item resolves
/// that question by actually amending it: <see cref="ScandalSourceType.CartelDiscovery"/> is a real,
/// genuine new source (purely additive, matching that enum's own <see
/// cref="ScandalSourceType.BusinessMisconduct"/>/<see cref="ScandalSourceType.EdictBacklash"/> precedent),
/// wired by this command directly. Unlike <see cref="NotableBusinesses.RecordBusinessScandalCommand"/>'s
/// own business-Reputation-only framing, a discovered price-fixing conspiracy is exactly the kind of
/// personal wrongdoing <see cref="RecordScandalCommand"/> already models well (a household's own head
/// conspiring against the market, not merely "adulterated goods") — so this command does <b>not</b>
/// suppress the ordinary personal Dignitas penalty/Trait grant the way a Business Misconduct Scandal does;
/// every participant takes both a real personal Scandal (where its own owner resolves to a household) and
/// a real Business Reputation hit.</summary>
public sealed record DiscoverCartelCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<CartelAgreement> CartelId) : ICommand;

public sealed record CartelDiscoveredEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<CartelAgreement> CartelId,
    string? CausationId) : IDomainEvent
{
    public string Type => "businessCompetition.cartelDiscovered";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CartelId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class DiscoverCartelCommands
{
    public static readonly ValidationErrorCode CartelNotFound = new("businessCompetition.discoverCartel.cartelNotFound");
    public static readonly ValidationErrorCode AlreadyDiscovered = new("businessCompetition.discoverCartel.alreadyDiscovered");

    public static readonly CommandPipeline<WorldState, DiscoverCartelCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, DiscoverCartelCommand command)
    {
        if (!state.CartelAgreements.TryGet(command.CartelId, out var cartel))
            return CartelNotFound;
        if (cartel!.IsDiscovered)
            return AlreadyDiscovered;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, DiscoverCartelCommand command)
    {
        var events = new List<IDomainEvent>();
        state.CartelAgreements.TryGet(command.CartelId, out var cartel);
        state.CartelAgreements.Remove(command.CartelId);
        state.CartelAgreements.Add(command.CartelId, cartel! with { IsDiscovered = true });

        foreach (var participantId in cartel.ParticipantBusinessIds)
        {
            if (!state.NotableBusinesses.TryGet(participantId, out var participant))
                continue;

            if (NotableBusinessOwnerResolver.TryResolveHousehold(participant!.Owner, out var householdId))
            {
                events.AddRange(RecordScandalCommands.Pipeline.Execute(
                    state, new RecordScandalCommand(
                        state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                        householdId, ScandalSourceType.CartelDiscovery, ScandalSeverity.PublicDisgrace,
                        ApplyOrdinaryDignitasPenalty: true, ApplyTraitGrant: true)).Events);
            }

            events.AddRange(AdjustBusinessReputationCommands.Pipeline.Execute(
                state, new AdjustBusinessReputationCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    participantId, -BusinessCompetitionCatalog.CartelDiscoveryReputationLoss, BusinessReputationChangeReason.BusinessScandal)).Events);
        }

        events.Add(new CartelDiscoveredEvent(state.EventIds.Issue(), command.SubmittedDate, command.CartelId, command.CommandId.ToTaggedString()));
        return events.ToArray();
    }
}
