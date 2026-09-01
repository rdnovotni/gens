using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
using Gens.Simulation.Regions;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Wanderers;

/// <summary>Which of §5's three encounter paths made this Wanderer relevant enough to instantiate
/// (§8). Every value is real vocabulary from §5; see <see cref="InstantiateWandererCommands"/> for
/// which of the three has a real, already-callable trigger in this codebase and which two are recorded
/// but not yet driven by any caller.</summary>
public enum WandererInstantiationTrigger
{
    /// <summary>§5's "Direct Travel encounter" — the player's own Travel destination happens to host
    /// the Wanderer.</summary>
    TravelArrival,

    /// <summary>§5's "Ambient rumor" — the Monthly Report or Correspondence &amp; Letters carries word
    /// before the player ever travels there.</summary>
    CorrespondenceRumor,

    /// <summary>§5's "A direct approach" — a famous enough Wanderer seeks a sufficiently Prominent
    /// household out directly.</summary>
    ProminenceDirectApproach,
}

/// <summary>
/// §8's single promotion door: turns one member of the (unstored, uncounted — see <see
/// cref="Wanderer"/>'s own doc comment) ambient pool into a real, named, individually-tracked <see
/// cref="Wanderer"/>. This is the exact "explicit background/notable sampling plus triggered promotion"
/// pattern §8 says to mirror, and it mirrors it against the two real implementations this codebase
/// already has: <c>Characters.PromoteToNamedCommand</c>'s aggregate-to-named promotion (ADR 0009) and
/// <c>Actors.LivingWorldActorHeadGenerator</c>'s "generated the moment the player household actually
/// interacts with them" lazy head generation. Like both of those, it draws its identity from <see
/// cref="CharacterIdentityGenerator"/> on a caller-named stream (rule 8), in that generator's own fixed
/// draw order, so a campaign seed always reproduces the same Wanderer for the same draw sequence.
///
/// <para><b>What is and is not wired into a real caller.</b> No system in this codebase submits this
/// command yet, and that is deliberate and disclosed rather than hidden — the same "hook now, caller
/// later" discipline <c>Health.AfflictCharacterCommand</c> and
/// <c>Hazards.DesignateDormantVolcanoCommand</c> both used in this same phase. Concretely, for each of
/// §5's three paths: <see cref="WandererInstantiationTrigger.TravelArrival"/> would hang off Travel §7's
/// Arrival-Encounter framework, which does not exist in this codebase — <c>Travel/</c> ships
/// <c>TravelTrip</c>, <c>TravelRoute</c>, <c>TravelProgressSystem</c> and a
/// <c>RouteRiskLevel</c>, but no arrival-encounter concept of any kind, so there is no hook to hang
/// off; <see cref="WandererInstantiationTrigger.CorrespondenceRumor"/> would hang off Correspondence
/// &amp; Letters' rumor delivery, and <c>Correspondence/</c>'s real <c>LetterAction</c> roster carries
/// no rumor/news-of-a-person action this could ride; and <see
/// cref="WandererInstantiationTrigger.ProminenceDirectApproach"/> needs a Prominence-gated threshold
/// that §11 itself lists as unsized <i>and</i> that has no Prominence field in this codebase to gate
/// on. The trigger is therefore recorded honestly on the command and on the emitted event so that
/// whichever future item builds one of those three can wire it without reshaping this command.</para>
/// </summary>
public sealed record InstantiateWandererCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    WandererType Type,
    DefinitionId<GazetteerLocationDefinition> InitialLocationId,
    DefinitionId<Culture> Culture,
    LegalStatus LegalStatus,
    NamePool NamePool,
    string RandomStreamName,
    WandererInstantiationTrigger Trigger,
    Sex? Sex = null) : ICommand;

/// <summary>Emitted whenever an <see cref="InstantiateWandererCommand"/> is accepted. <see
/// cref="Visibility"/> is <see cref="Commands.Visibility.Public"/> for the same reason
/// <c>Fame.FameChangedEvent</c>'s is: an itinerant whose "reputation traveled ahead of them" (§1) is a
/// public fact by construction.</summary>
public sealed record WandererInstantiatedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Wanderer> WandererId,
    WandererType WandererType,
    DefinitionId<GazetteerLocationDefinition> LocationId,
    int Fame,
    WandererInstantiationTrigger Trigger,
    string? CausationId) : IDomainEvent
{
    public string Type => "wanderers.instantiated";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { WandererId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="InstantiateWandererCommand"/> (ADR 0006).
/// Exposed via a factory rather than a pre-built static pipeline for the same reason as
/// <c>Characters.PromoteToNamedCommands.CreatePipeline</c>: <c>mutate</c> draws from a <see
/// cref="RandomStreamSet"/> and <see cref="CommandPipeline{TState,TCommand}"/>'s <c>mutate</c> delegate
/// only receives <see cref="WorldState"/>.</summary>
public static class InstantiateWandererCommands
{
    public static readonly ValidationErrorCode UnknownLocation = new("wanderers.instantiate.unknownLocation");
    public static readonly ValidationErrorCode AlreadyTrackedHere = new("wanderers.instantiate.alreadyTrackedHere");

    public static CommandPipeline<WorldState, InstantiateWandererCommand> CreatePipeline(
        RandomStreamSet randomStreams, RegionProfileCatalog regionCatalog)
    {
        if (randomStreams is null)
            throw new ArgumentNullException(nameof(randomStreams));
        if (regionCatalog is null)
            throw new ArgumentNullException(nameof(regionCatalog));

        return new CommandPipeline<WorldState, InstantiateWandererCommand>(
            validate: (state, command) => Validate(state, command, regionCatalog),
            mutate: (state, command) => Mutate(state, command, randomStreams),
            issueSequenceNumber: static state => state.IssueCommandSequenceNumber());
    }

    private static ValidationErrorCode? Validate(
        WorldState state, InstantiateWandererCommand command, RegionProfileCatalog regionCatalog)
    {
        if (!TryFindLocation(regionCatalog, command.InitialLocationId, out _))
            return UnknownLocation;

        // §8's restraint made mechanical: one tracked Wanderer of a given type per location at a time.
        // Sampling exists so the world does not need to track "every conceivable itinerant specialist
        // across the entire map at all times" — instantiating a second philosopher into a town that
        // already has a tracked one is exactly the drift that restraint is meant to prevent.
        foreach (var wanderer in WandererQueries.At(state, command.InitialLocationId))
        {
            if (wanderer.Type == command.Type)
                return AlreadyTrackedHere;
        }

        return null;
    }

    internal static bool TryFindLocation(
        RegionProfileCatalog catalog,
        DefinitionId<GazetteerLocationDefinition> locationId,
        out GazetteerLocationDefinition location)
    {
        foreach (var region in catalog.All())
        {
            foreach (var candidate in region.Gazetteer)
            {
                if (candidate.Id.Equals(locationId))
                {
                    location = candidate;
                    return true;
                }
            }
        }

        location = null!;
        return false;
    }

    private static IDomainEvent[] Mutate(WorldState state, InstantiateWandererCommand command, RandomStreamSet randomStreams)
    {
        // Identical draw order to Characters.PromoteToNamedCommands.Mutate and
        // Actors.LivingWorldActorHeadGenerator: sex, birth date, then identity. The Fame roll is
        // appended last so adding it never shifts the identity draws out from under either of those.
        var sex = command.Sex ?? (randomStreams.NextUInt(command.RandomStreamName, 2) == 0 ? Characters.Sex.Male : Characters.Sex.Female);
        var birthDate = CharacterBackfillGenerator.RollAdultBirthDate(randomStreams, command.RandomStreamName, command.SubmittedDate);
        var identity = CharacterIdentityGenerator.Generate(
            randomStreams, command.RandomStreamName, sex, command.LegalStatus, command.NamePool);

        var fameSpread = (uint)(WandererFameCalculator.MaximumStartingFame - WandererFameCalculator.MinimumStartingFame + 1);
        var fame = WandererFameCalculator.MinimumStartingFame + (int)randomStreams.NextUInt(command.RandomStreamName, fameSpread);

        var wandererId = state.WandererIds.Issue();
        var wanderer = Wanderer.Create(
            id: wandererId,
            name: identity.Name,
            sex: sex,
            birthDate: birthDate,
            status: command.LegalStatus,
            culture: command.Culture,
            type: command.Type,
            currentLocationId: command.InitialLocationId,
            fame: fame,
            arrivalDate: command.SubmittedDate);
        state.Wanderers.Add(wandererId, wanderer);

        return new IDomainEvent[]
        {
            new WandererInstantiatedEvent(
                state.EventIds.Issue(), command.SubmittedDate, wandererId, command.Type,
                command.InitialLocationId, fame, command.Trigger, command.CommandId.ToTaggedString()),
        };
    }
}
