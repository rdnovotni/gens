using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;
using Gens.Simulation.Travel;

namespace Gens.Simulation.Correspondence;

/// <summary>§11's <c>Letter{}</c> shape: one committed piece of correspondence, from being sent to
/// (eventually) resolving what actually reached its recipient. A real <see cref="State.WorldState"/>
/// partition, matching <see cref="TravelTrip"/>'s identical "genuine campaign state, not pure content"
/// reasoning — a letter's own transit progress changes tick to tick.</summary>
public sealed record Letter
{
    private Letter()
    {
    }

    public required RuntimeId<Letter> Id { get; init; }
    public required LetterDirection Direction { get; init; }
    public required LetterAction Action { get; init; }

    /// <summary>Either a Character's or an Actor's own tagged ID string (§11's literal
    /// <c>senderCharacterOrActorId</c>/<c>recipientCharacterOrActorId</c> naming) — a letter's
    /// counterparty is not always a named Character with a full record (a Rival House, a foreign
    /// people under treaty).</summary>
    public required string SenderCharacterOrActorId { get; init; }

    public required string RecipientCharacterOrActorId { get; init; }

    /// <summary>Who actually put pen to wax tablet (§2's Illiterate-dictation privacy angle) — null
    /// when this item's own caller doesn't track it, which is always true for an <see
    /// cref="OriginateInboundLetterCommand"/>-created letter (this engine has no reason to know which
    /// Character on another Actor's side drafted their own outgoing mail).</summary>
    public RuntimeId<Character>? DraftedByCharacterId { get; init; }

    public required GameDate SentDate { get; init; }
    public required int TransitTimeMonths { get; init; }

    /// <summary>Months elapsed on the current leg — mirrors <see cref="TravelTrip.MonthsElapsed"/>'s
    /// identical shape. Counts against <see cref="TransitTimeMonths"/> plus <see
    /// cref="RedirectionDelayMonths"/> once that's been applied.</summary>
    public int MonthsElapsed { get; init; }

    /// <summary>§9's Redirection: an added delay applied at most once per letter, when the recipient
    /// turns out to be away from home at the moment transit would otherwise have finished (<see
    /// cref="CorrespondenceTransitSystem"/>). Zero until that happens; this item's own invented,
    /// disclosed extra delay amount (see <see cref="CorrespondenceTransitSystem"/>'s own doc comment)
    /// since §12 leaves "Redirection's added delay" an explicit open question.</summary>
    public int RedirectionDelayMonths { get; init; }

    public GameDate? ArrivalDate { get; init; }

    public required CourierType CourierType { get; init; }

    /// <summary>The Character actually carrying this letter, when it's a real household member (a
    /// Tabellarius) rather than an anonymous hired stranger or a bird — null for <see
    /// cref="CourierType.HiredCarrier"/>/<see cref="CourierType.Pigeon"/> unless the caller supplies
    /// one.</summary>
    public RuntimeId<Character>? CourierCharacterId { get; init; }

    public required RouteRiskLevel InterceptionRisk { get; init; }
    public bool Intercepted { get; init; }
    public bool Forged { get; init; }
    public bool Redirected { get; init; }
    public required bool OralTraditionPenaltyApplied { get; init; }

    /// <summary>Only meaningful for <see cref="LetterDirection.Inbound"/> letters (§11's own note) —
    /// always false for an outbound letter, since a response is something the *player's* own
    /// correspondent might send back, not something this engine tracks against the player's own
    /// outgoing mail.</summary>
    public required bool RequiresResponse { get; init; }

    public bool Responded { get; init; }
    public LetterAction? ResponseAction { get; init; }
    public LetterStatus Status { get; init; } = LetterStatus.InTransit;
    public LetterOutcome Outcome { get; init; } = LetterOutcome.Pending;

    /// <summary>Begins an outbound letter (<see cref="SendLetterCommand"/>) or an inbound one (<see
    /// cref="OriginateInboundLetterCommand"/>) — <paramref name="requiresResponse"/> is forced false for
    /// <see cref="LetterDirection.Outbound"/> regardless of what the caller passes, matching this
    /// record's own <see cref="RequiresResponse"/> doc comment.</summary>
    public static Letter Begin(
        RuntimeId<Letter> id,
        LetterDirection direction,
        LetterAction action,
        string senderCharacterOrActorId,
        string recipientCharacterOrActorId,
        RuntimeId<Character>? draftedByCharacterId,
        LetterRoute route,
        CourierType courierType,
        RuntimeId<Character>? courierCharacterId,
        GameDate sentDate,
        bool requiresResponse)
    {
        if (route is null)
            throw new ArgumentNullException(nameof(route));
        if (route.Blocked)
        {
            throw new ArgumentException(
                "A blocked LetterRoute (§7's Oral Tradition Problem) cannot begin a letter — the caller " +
                "must reject the command before reaching this factory.", nameof(route));
        }
        if (route.TransitTimeMonths <= 0)
            throw new ArgumentException("A letter route's transit time must be positive.", nameof(route));
        if (string.IsNullOrWhiteSpace(senderCharacterOrActorId))
            throw new ArgumentException("A sender ID is required.", nameof(senderCharacterOrActorId));
        if (string.IsNullOrWhiteSpace(recipientCharacterOrActorId))
            throw new ArgumentException("A recipient ID is required.", nameof(recipientCharacterOrActorId));

        return new Letter
        {
            Id = id,
            Direction = direction,
            Action = action,
            SenderCharacterOrActorId = senderCharacterOrActorId,
            RecipientCharacterOrActorId = recipientCharacterOrActorId,
            DraftedByCharacterId = draftedByCharacterId,
            SentDate = sentDate,
            TransitTimeMonths = route.TransitTimeMonths,
            MonthsElapsed = 0,
            RedirectionDelayMonths = 0,
            ArrivalDate = null,
            CourierType = courierType,
            CourierCharacterId = courierCharacterId,
            InterceptionRisk = route.InterceptionRisk,
            Intercepted = false,
            Forged = false,
            Redirected = false,
            OralTraditionPenaltyApplied = route.OralTraditionPenaltyApplied,
            RequiresResponse = direction == LetterDirection.Outbound ? false : requiresResponse,
            Responded = false,
            ResponseAction = null,
            Status = LetterStatus.InTransit,
            Outcome = LetterOutcome.Pending,
        };
    }

    /// <summary>Reconstructs a <see cref="Letter"/> from persisted save data (ADR 0010) — mirrors <see
    /// cref="TravelTrip.Restore"/>'s identical "the mapper's own restore path" shape.</summary>
    public static Letter Restore(
        RuntimeId<Letter> id, LetterDirection direction, LetterAction action,
        string senderCharacterOrActorId, string recipientCharacterOrActorId,
        RuntimeId<Character>? draftedByCharacterId, GameDate sentDate, int transitTimeMonths,
        int monthsElapsed, int redirectionDelayMonths, GameDate? arrivalDate, CourierType courierType,
        RuntimeId<Character>? courierCharacterId, RouteRiskLevel interceptionRisk, bool intercepted,
        bool forged, bool redirected, bool oralTraditionPenaltyApplied, bool requiresResponse,
        bool responded, LetterAction? responseAction, LetterStatus status, LetterOutcome outcome) =>
        new()
        {
            Id = id,
            Direction = direction,
            Action = action,
            SenderCharacterOrActorId = senderCharacterOrActorId,
            RecipientCharacterOrActorId = recipientCharacterOrActorId,
            DraftedByCharacterId = draftedByCharacterId,
            SentDate = sentDate,
            TransitTimeMonths = transitTimeMonths,
            MonthsElapsed = monthsElapsed,
            RedirectionDelayMonths = redirectionDelayMonths,
            ArrivalDate = arrivalDate,
            CourierType = courierType,
            CourierCharacterId = courierCharacterId,
            InterceptionRisk = interceptionRisk,
            Intercepted = intercepted,
            Forged = forged,
            Redirected = redirected,
            OralTraditionPenaltyApplied = oralTraditionPenaltyApplied,
            RequiresResponse = requiresResponse,
            Responded = responded,
            ResponseAction = responseAction,
            Status = status,
            Outcome = outcome,
        };
}
