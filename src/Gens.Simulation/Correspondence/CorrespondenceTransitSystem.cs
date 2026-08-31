using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using Gens.Simulation.Travel;

namespace Gens.Simulation.Correspondence;

/// <summary>Emitted when a <see cref="Letter"/>'s transit finishes and <see cref="Letter.Outcome"/>
/// resolves — mirrors <see cref="Travel.TravelArrivedEvent"/>'s identical "the leg is over" shape,
/// though a letter's own Inbox visibility (whether the recipient can actually see it) is a UI-layer
/// concern this event doesn't gate.</summary>
public sealed record LetterDeliveredEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Letter> LetterId,
    string RecipientCharacterOrActorId,
    LetterOutcome Outcome,
    bool Redirected,
    string? CausationId) : IDomainEvent
{
    public string Type => "correspondence.delivered";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { RecipientCharacterOrActorId };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// Advances every <see cref="LetterStatus.InTransit"/> <see cref="Letter"/> by one month, matching
/// <see cref="Travel.TravelProgressSystem"/>'s identical "every entry in the registry advances
/// independently in the same tick" shape. On the month transit would otherwise finish:
///
/// <list type="number">
/// <item><b>Redirection</b> (§9) is checked exactly once, before delivery: if the recipient is a
/// tracked <see cref="Character"/> currently away from home (a non-null <see
/// cref="Character.CurrentTravelLocation"/> — Travel's own concurrent-location tracking, Phase 13 item
/// 2), the letter is marked <see cref="Letter.Redirected"/> and gains a further <see
/// cref="RedirectionDelayMonths"/>-month delay before delivery actually resolves. A recipient this
/// engine cannot resolve to a tracked Character (an Actor, or an untracked ID) is assumed reachable —
/// this item has no basis to assume otherwise for a party it cannot even look up.</item>
/// <item><b>Interception/forgery</b> (§9) resolves via one random draw against the route's own <see
/// cref="Letter.InterceptionRisk"/> (<see cref="RouteRiskChancePercent"/>) plus the chosen <see
/// cref="CourierCatalog"/> profile's own modifier; an intercepted letter draws again for whether it was
/// merely lost or actually forged and passed on.</item>
/// </list>
///
/// Every numeric constant here is this item's own invented, disclosed first pass, matching <see
/// cref="Interactions.SchemeProgressSystem"/>'s own "untuned" precedent for chance-based resolution.
/// </summary>
public sealed class CorrespondenceTransitSystem : IMonthlySystem<WorldState>
{
    public const string RiskStreamName = "correspondence.risk";

    /// <summary>This item's own invented extra delay once a letter is Redirected (§9) — unsized by
    /// §12's own open question ("Redirection's added delay").</summary>
    public const int RedirectionDelayMonths = 1;

    /// <summary>This item's own invented base interception chance per <see cref="Travel.RouteRiskLevel"/>
    /// tier, before the courier's own modifier (<see cref="CourierCatalog"/>) is added.</summary>
    private static int RouteRiskChancePercent(RouteRiskLevel risk) => risk switch
    {
        RouteRiskLevel.Secure => 5,
        RouteRiskLevel.Guarded => 15,
        RouteRiskLevel.Dangerous => 35,
        _ => throw new ArgumentOutOfRangeException(nameof(risk), risk, "Unknown route risk level."),
    };

    /// <summary>This item's own invented chance that an intercepted letter is forged and passed on
    /// rather than simply lost — §12's own "forgery detection mechanics... still unresolved" leaves no
    /// basis for anything more precise.</summary>
    private const int ForgeryGivenInterceptedChancePercent = 40;

    public string Id => "correspondence.transit";
    public TickPhase Phase => TickPhase.Lifecycle;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "letters", "characters" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "letters", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();
        var updates = new List<(RuntimeId<Letter> Id, Letter Letter)>();

        foreach (var entry in state.Letters.InAscendingOrder())
        {
            if (entry.Value.Status != LetterStatus.InTransit)
                continue;

            AdvanceOne(state, context, entry.Key, entry.Value, updates, events);
        }

        foreach (var (id, letter) in updates)
        {
            state.Letters.Remove(id);
            state.Letters.Add(id, letter);
        }

        return events;
    }

    private static void AdvanceOne(
        WorldState state, MonthlyTickContext context, RuntimeId<Letter> letterId, Letter letter,
        List<(RuntimeId<Letter> Id, Letter Letter)> updates, List<IDomainEvent> events)
    {
        var effectiveTransitMonths = letter.TransitTimeMonths + letter.RedirectionDelayMonths;
        var elapsed = letter.MonthsElapsed + 1;

        if (elapsed < effectiveTransitMonths)
        {
            updates.Add((letterId, letter with { MonthsElapsed = elapsed }));
            return;
        }

        // Redirection (§9) is checked exactly once, on the month transit would otherwise finish.
        if (!letter.Redirected && IsRecipientAwayFromHome(state, letter.RecipientCharacterOrActorId))
        {
            updates.Add((letterId, letter with { MonthsElapsed = elapsed, Redirected = true, RedirectionDelayMonths = RedirectionDelayMonths }));
            return;
        }

        var (outcome, intercepted, forged) = ResolveRisk(context, letter);
        var delivered = letter with
        {
            MonthsElapsed = elapsed,
            ArrivalDate = context.Date,
            Status = LetterStatus.Delivered,
            Outcome = outcome,
            Intercepted = intercepted,
            Forged = forged,
        };
        updates.Add((letterId, delivered));
        events.Add(new LetterDeliveredEvent(
            state.EventIds.Issue(), context.Date, letterId, letter.RecipientCharacterOrActorId,
            outcome, delivered.Redirected, CausationId: null));
    }

    private static bool IsRecipientAwayFromHome(WorldState state, string recipientCharacterOrActorId)
    {
        if (!recipientCharacterOrActorId.StartsWith("char_", StringComparison.Ordinal))
            return false;

        RuntimeId<Character> characterId;
        try
        {
            characterId = RuntimeId<Character>.Parse(recipientCharacterOrActorId);
        }
        catch (FormatException)
        {
            return false;
        }

        return state.Characters.TryGet(characterId, out var character) && character.CurrentTravelLocation is not null;
    }

    private static (LetterOutcome Outcome, bool Intercepted, bool Forged) ResolveRisk(MonthlyTickContext context, Letter letter)
    {
        var courierModifier = CourierCatalog.Resolve(letter.CourierType).InterceptionRiskModifierPercent;
        var interceptChance = Math.Clamp(RouteRiskChancePercent(letter.InterceptionRisk) + courierModifier, 0, 100);

        var intercepted = context.RandomStreams.NextUInt(RiskStreamName, 100) < (uint)interceptChance;
        if (!intercepted)
            return (LetterOutcome.DeliveredIntact, false, false);

        var forged = context.RandomStreams.NextUInt(RiskStreamName, 100) < ForgeryGivenInterceptedChancePercent;
        return forged ? (LetterOutcome.Forged, true, true) : (LetterOutcome.Intercepted, true, false);
    }
}
