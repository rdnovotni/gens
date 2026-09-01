using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Regions;
using Gens.Simulation.Time;

namespace Gens.Simulation.Wanderers;

/// <summary>
/// One individually-tracked itinerant specialist (<c>gens-wandering-populations-design.md</c> §10's
/// <c>Wanderer</c> record). A new <see cref="RuntimeId{T}"/>-keyed <c>WorldState</c> partition
/// (<c>Wanderers</c>), wired through <c>StateHasher</c>/<c>WorldSaveDto</c>/<c>WorldStateMapper</c>/
/// <c>EntityKinds</c> the same five-file way every prior runtime partition has been, and kept forever
/// once created — matching <c>Hazards.DisasterEvent</c>'s and <c>Health.EpidemicOutbreak</c>'s
/// identical "resolved or not, kept for the campaign's lifetime" convention. A Recruited Wanderer is
/// not removed: the record is the campaign's own history of who that person was before they joined.
///
/// <para><b>§8's sampling-and-promotion pattern, taken literally.</b> "The overwhelming majority of
/// Wanderers who could plausibly exist in the wider world stay purely ambient and unnamed — background
/// texture, never individually tracked." This codebase therefore stores <i>no</i> ambient pool at all:
/// an ambient Wanderer has no record, no ID, and costs nothing, exactly the way
/// <c>Characters.PopGroup</c> holds no per-unit rows and <c>Actors.LivingWorldActor</c>'s Background
/// tier holds no head Character until real contact promotes it (<see
/// cref="Actors.LivingWorldActorHeadGenerator"/>'s own "generated the moment the player household
/// actually interacts with them"). <see cref="InstantiateWandererCommand"/> is the single promotion
/// door, and every row in this partition has already come through it.</para>
///
/// <para><b>Why <see cref="IsActivelyTracked"/> still earns its place</b> despite that: §10 names the
/// field, and it carries real mechanics here rather than being a constant true. It is what <see
/// cref="WandererSystem"/> gates its monthly Itinerary/Fame advance on, and <see
/// cref="RecruitWandererCommands"/> sets it false the moment a Recruit succeeds — §6's "a successful
/// Recruit ends that Wanderer's own independent Itinerary entirely" made literal, without deleting the
/// historical record.</para>
///
/// <para><b>Identity fields (<see cref="Name"/>, <see cref="Sex"/>, <see cref="BirthDate"/>, <see
/// cref="Culture"/>, <see cref="LegalStatus"/>) are carried here rather than re-rolled at Recruit time</b>
/// so that the Character a Recruit produces is genuinely the same person the player has been reading
/// about: <see cref="InstantiateWandererCommand"/> generates them once through <see
/// cref="CharacterIdentityGenerator"/> — the exact same generator <see
/// cref="Characters.PromoteToNamedCommand"/> and <see cref="Actors.LivingWorldActorHeadGenerator"/>
/// already use — and <see cref="RecruitWandererCommands"/> replays them verbatim into <see
/// cref="Character.Create"/>. <see cref="SocialClass"/> is deliberately not carried: <see
/// cref="Character.Create"/> allows it only for a <see cref="LegalStatus.RomanCitizen"/>, and nothing
/// in the design document assigns a Wanderer an ordo, so a recruited Wanderer is always created with a
/// null social class.</para>
/// </summary>
public sealed record Wanderer
{
    public required RuntimeId<Wanderer> Id { get; init; }
    public required CharacterName Name { get; init; }
    public required Sex Sex { get; init; }
    public required GameDate BirthDate { get; init; }
    public required LegalStatus LegalStatus { get; init; }
    public required DefinitionId<Culture> Culture { get; init; }
    public required WandererType Type { get; init; }

    /// <summary>§10's <c>currentLocationId</c> — always the last entry of <see cref="Itinerary"/>; see
    /// <see cref="WandererItineraryStop"/> for why this is a Gazetteer entry rather than a
    /// Settlement.</summary>
    public required DefinitionId<GazetteerLocationDefinition> CurrentLocationId { get; init; }

    /// <summary>§3/§10's moving Itinerary, oldest stop first and the current location last. Capped at
    /// <see cref="WandererItineraryCalculator.MaxItineraryLength"/> stops by <see
    /// cref="WandererSystem"/>: this is a real, bounded travel history, not an unbounded log that grows
    /// for the campaign's lifetime.</summary>
    public required IReadOnlyList<WandererItineraryStop> Itinerary { get; init; }

    /// <summary>§4/§10's Fame — always in [0, 100], the identical range and clamp <see
    /// cref="Gens.Simulation.Fame.CharacterFame"/> already enforces for the universal Character-level field, since §4 is
    /// explicit that this is "not a Wanderer-specific mechanic... the same 0-100 score any Character
    /// carries." It is a parallel value here only because a Wanderer is not yet a Character at all (§8);
    /// the two are joined for real at Recruit time, where <see cref="RecruitWandererCommands"/> seeds
    /// the new Character's own <see cref="Gens.Simulation.Fame.CharacterFame"/> from this exact number through <see
    /// cref="Gens.Simulation.Fame.FameResolver.Apply"/>.</summary>
    public required int Fame { get; init; }

    public required WandererFameTrend FameTrend { get; init; }

    /// <summary>§10's <c>isActivelyTracked</c> — see this record's own doc comment for why it is real
    /// mechanics rather than a constant.</summary>
    public required bool IsActivelyTracked { get; init; }

    public required WandererStatus Status { get; init; }

    /// <summary>How many months have passed since this Wanderer's last successful engagement — the
    /// input <see cref="WandererFameCalculator.MonthlyObscurityDecay"/> reads for §4's "fades through
    /// sustained obscurity." Starts at zero on instantiation (a freshly-noticed Wanderer is, by
    /// definition, currently being noticed).</summary>
    public required int MonthsSinceLastEngagement { get; init; }

    /// <summary>§7/§10's <c>competingHouseholdIds</c>: every household that has registered real,
    /// declared interest in this Wanderer without yet committing, in registration order. Emptied the
    /// moment <see cref="CommittedHouseholdId"/> is set — §7's "resolved the instant either side
    /// actually commits rather than held open indefinitely."</summary>
    public required IReadOnlyList<RuntimeId<Household>> InterestedHouseholdIds { get; init; }

    /// <summary>§7/§10's <c>winningHouseholdId</c>: the first household to actually commit, by Host or
    /// by Recruit. Null while nobody has. Once set, no other household can engage this Wanderer — the
    /// whole mechanical content of §7's race.</summary>
    public RuntimeId<Household>? CommittedHouseholdId { get; init; }

    /// <summary>The real <see cref="Character"/> a successful Recruit produced (§10's
    /// <c>resultingCompanionOrPositionId</c>, on the Wanderer side). Null unless <see
    /// cref="Status"/> is <see cref="WandererStatus.Recruited"/>.</summary>
    public RuntimeId<Character>? RecruitedCharacterId { get; init; }

    /// <summary>The only supported way to construct a <see cref="Wanderer"/> — validates the
    /// cross-field invariants an object initializer cannot, matching <see cref="Character.Create"/>'s
    /// and <c>Hazards.DisasterEvent.Create</c>'s identical convention.</summary>
    public static Wanderer Create(
        RuntimeId<Wanderer> id,
        CharacterName name,
        Sex sex,
        GameDate birthDate,
        LegalStatus status,
        DefinitionId<Culture> culture,
        WandererType type,
        DefinitionId<GazetteerLocationDefinition> currentLocationId,
        int fame,
        GameDate arrivalDate,
        WandererFameTrend fameTrend = WandererFameTrend.Established)
    {
        if (name is null)
            throw new ArgumentNullException(nameof(name));
        if (fame is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(fame), fame, "Wanderer Fame must be in [0, 100].");

        return new Wanderer
        {
            Id = id,
            Name = name,
            Sex = sex,
            BirthDate = birthDate,
            LegalStatus = status,
            Culture = culture,
            Type = type,
            CurrentLocationId = currentLocationId,
            Itinerary = new[] { new WandererItineraryStop(currentLocationId, arrivalDate.TotalMonths) },
            Fame = fame,
            FameTrend = fameTrend,
            IsActivelyTracked = true,
            Status = WandererStatus.Wandering,
            MonthsSinceLastEngagement = 0,
            InterestedHouseholdIds = Array.Empty<RuntimeId<Household>>(),
            CommittedHouseholdId = null,
            RecruitedCharacterId = null,
        };
    }
}
