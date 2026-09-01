using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Regions;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Wanderers;

/// <summary>
/// The monthly Wanderer tick (Phase 14 item 4): for every actively-tracked <see cref="Wanderer"/>, ages
/// their obscurity counter, applies §4's Fame decay and recomputes their <see cref="WandererFameTrend"/>,
/// and — once they have dwelled <see cref="WandererItineraryCalculator.MonthsPerStop"/> months at their
/// current stop — advances §3's Itinerary to a type-weighted next destination. This is the whole of
/// "routes" and "fame/visibility" as a live, running mechanism rather than a stored slider.
///
/// <para><b>Phase.</b> <see cref="TickPhase.RelationshipsActors"/>, the phase
/// <c>Actors.BackgroundHouseDriftSystem</c> (the living world drifting whether or not the player is
/// watching) and <c>Fame.FameDecaySystem</c> (the universal Fame field eroding through inactivity) both
/// already run in — this system is exactly those two things applied to an individual itinerant, and §9
/// names Rival Houses' own "living world" principle as its direct parent. Deliberately not
/// <see cref="TickPhase.Hazards"/>, where this phase's items 1-3 all sat: nothing here is a hazard.
/// It declares no <c>Prerequisites</c>: it reads and writes only its own partition, so ADR 0004/0005's
/// alphabetical same-phase tiebreak is a complete and stable ordering for it.</para>
///
/// <para><b>Determinism.</b> One <c>uint</c> draw per moving Wanderer, from a single caller-named stream
/// (rule 8), taken in ascending-<see cref="RuntimeId{T}"/> order and only for Wanderers that are
/// actually due to move — so a campaign seed reproduces the same tour exactly. The weighting itself is
/// pure and RNG-free (<see cref="WandererItineraryCalculator"/>); the stream only picks a point inside
/// the weights.</para>
///
/// <para><b>Deliberately not modeled here</b> (matching this namespace's own per-file disclosures):
/// nothing instantiates or retires a Wanderer on its own. §8's sampling is explicitly trigger-driven —
/// "only actually instantiated... when a player's own Travel destination, Correspondence rumor, or
/// Prominence-driven direct approach makes them genuinely relevant" — and none of those three triggers
/// exists in this codebase (<see cref="InstantiateWandererCommands"/>'s own disclosure), so this system
/// would have to invent a spawn rule the design document explicitly does not want. §11's own second open
/// question ("Wanderer count per region/era... isn't specified") is the same gap from the other side.
/// A Wanderer also never ages out, dies, or retires here: no lifecycle rule for an itinerant is written
/// anywhere in the design corpus, and <c>Characters.CharacterLifecycleSystem</c> operates on real
/// Characters, which a Wanderer is not until Recruited.</para>
/// </summary>
public sealed class WandererSystem : IMonthlySystem<WorldState>
{
    private readonly string _streamName;
    private readonly WandererTypeCatalog _typeCatalog;
    private readonly RegionProfileCatalog _regionCatalog;

    public WandererSystem(string streamName, WandererTypeCatalog typeCatalog, RegionProfileCatalog regionCatalog)
    {
        if (string.IsNullOrWhiteSpace(streamName))
            throw new ArgumentException("A wanderer random stream name is required.", nameof(streamName));

        _streamName = streamName;
        _typeCatalog = typeCatalog ?? throw new ArgumentNullException(nameof(typeCatalog));
        _regionCatalog = regionCatalog ?? throw new ArgumentNullException(nameof(regionCatalog));
    }

    /// <summary>The upper bound of the single per-move draw. Large enough that a weighting's own integer
    /// modulo bias is negligible, matching <c>Hazards.NaturalDisasterSystem.RollPrecision</c>'s identical
    /// "one fixed precision for every draw this system takes" convention.</summary>
    public const uint RollPrecision = 10_000;

    public string Id => "wanderers.itineraryAndFame";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "wanderers" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "wanderers", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // Materialize first: the loop body replaces entries in the same registry it is scanning, the
        // same precaution Fame.FameDecaySystem already takes for its own in-scan replacement.
        foreach (var wandererId in state.Wanderers.InAscendingOrder().Select(static entry => entry.Key).ToArray())
        {
            state.Wanderers.TryGet(wandererId, out var wanderer);
            if (!wanderer!.IsActivelyTracked || wanderer.Status != WandererStatus.Wandering)
                continue;

            var monthsSinceEngagement = wanderer.MonthsSinceLastEngagement + 1;
            var previousFame = wanderer.Fame;
            var newFame = WandererFameCalculator.ApplyDelta(
                previousFame, -WandererFameCalculator.MonthlyObscurityDecay(monthsSinceEngagement));

            var itinerary = wanderer.Itinerary;
            var currentLocationId = wanderer.CurrentLocationId;
            var arrivalMonth = itinerary.Count > 0 ? itinerary[^1].ArrivalMonth : context.Date.TotalMonths;

            if (WandererItineraryCalculator.IsDueToMove(arrivalMonth, context.Date.TotalMonths))
            {
                var profile = _typeCatalog.Get(wanderer.Type);
                var roll = context.RandomStreams.NextUInt(_streamName, RollPrecision);
                var destination = WandererItineraryCalculator.SelectNextDestination(
                    profile, _regionCatalog, currentLocationId, roll);

                if (destination is { } nextLocationId)
                {
                    itinerary = WandererItineraryCalculator.Append(
                        itinerary, new WandererItineraryStop(nextLocationId, context.Date.TotalMonths));
                    currentLocationId = nextLocationId;

                    events.Add(new WandererMovedEvent(
                        state.EventIds.Issue(), context.Date, wandererId, wanderer.CurrentLocationId,
                        nextLocationId, CausationId: null));
                }
            }

            state.Wanderers.Remove(wandererId);
            state.Wanderers.Add(wandererId, wanderer with
            {
                Fame = newFame,
                FameTrend = WandererFameCalculator.Trend(previousFame, newFame),
                MonthsSinceLastEngagement = monthsSinceEngagement,
                CurrentLocationId = currentLocationId,
                Itinerary = itinerary,
            });
        }

        return events;
    }
}

/// <summary>Emitted whenever <see cref="WandererSystem"/> advances a Wanderer's Itinerary to a new
/// stop (§3). Fame drift emits nothing, matching <c>Fame.FameDecaySystem</c>'s identical "a quiet
/// resource drift needs no per-tick event for a number that already reads directly off the record"
/// precedent.</summary>
public sealed record WandererMovedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Wanderer> WandererId,
    DefinitionId<GazetteerLocationDefinition> FromLocationId,
    DefinitionId<GazetteerLocationDefinition> ToLocationId,
    string? CausationId) : IDomainEvent
{
    public string Type => "wanderers.moved";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { WandererId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}
