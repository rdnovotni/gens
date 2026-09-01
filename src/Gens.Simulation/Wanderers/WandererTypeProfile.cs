using Gens.Simulation.Characters;
using Gens.Simulation.Ledger;
using Gens.Simulation.Regions;

namespace Gens.Simulation.Wanderers;

/// <summary>The per-<see cref="WandererType"/> content row: which kinds of place that type's Itinerary
/// gravitates toward (§3), what a Host engagement costs and delivers (§6), and which real Familia duty
/// slot a successful Recruit lands in (§6/§9). A sealed record that validates in its constructor,
/// mirroring <c>Health.HealthConditionDefinition</c>/<c>Cultures.CultureDefinition</c>'s identical
/// "sealed record, constructor-validates, held in a duplicate-ID-rejecting catalog" content shape
/// exactly.</summary>
public sealed record WandererTypeProfile
{
    public WandererTypeProfile(
        WandererType type,
        IReadOnlyList<GazetteerRole> preferredRoles,
        bool prefersHighProminence,
        Money hostFee,
        Money recruitFee,
        int hostDignitasGain,
        int engagementFameGain,
        DutySlot? recruitedDutySlot)
    {
        if (preferredRoles is null || preferredRoles.Count == 0)
            throw new ArgumentException("A wanderer type profile requires at least one preferred gazetteer role.", nameof(preferredRoles));
        if (preferredRoles.Distinct().Count() != preferredRoles.Count)
            throw new ArgumentException("A wanderer type profile's preferred gazetteer roles must not repeat.", nameof(preferredRoles));
        if (hostFee.IsNegative)
            throw new ArgumentOutOfRangeException(nameof(hostFee), hostFee, "A host fee cannot be negative.");
        if (recruitFee.IsNegative)
            throw new ArgumentOutOfRangeException(nameof(recruitFee), recruitFee, "A recruit fee cannot be negative.");
        if (hostDignitasGain <= 0)
            throw new ArgumentOutOfRangeException(nameof(hostDignitasGain), hostDignitasGain, "A host Dignitas gain must be positive.");
        if (engagementFameGain <= 0)
            throw new ArgumentOutOfRangeException(nameof(engagementFameGain), engagementFameGain, "An engagement Fame gain must be positive.");

        Type = type;
        PreferredRoles = preferredRoles;
        PrefersHighProminence = prefersHighProminence;
        HostFee = hostFee;
        RecruitFee = recruitFee;
        HostDignitasGain = hostDignitasGain;
        EngagementFameGain = engagementFameGain;
        RecruitedDutySlot = recruitedDutySlot;
    }

    public WandererType Type { get; }

    /// <summary>The <see cref="GazetteerRole"/>s this type's Itinerary skews toward (§3), read by <see
    /// cref="WandererItineraryCalculator.MovementWeight"/>. See <see cref="WandererTypeCatalog"/>'s own
    /// doc comment for which of §3's named gravitational pulls these are a real read of and which are a
    /// disclosed proxy.</summary>
    public IReadOnlyList<GazetteerRole> PreferredRoles { get; }

    /// <summary>Whether this type additionally skews toward high-<see cref="ProminenceTier"/> places —
    /// §3's "a philosopher's itinerary skews toward Institutions of Renown and other high-Prominence
    /// Gazetteer locations," the one gravitational pull §3 names that this codebase can read directly
    /// off real content rather than through a proxy.</summary>
    public bool PrefersHighProminence { get; }

    /// <summary>What a §6 Host engagement draws from the hosting household's own Ledger
    /// (<see cref="LedgerAccountKey.ForHousehold"/>) — "a funded lecture, a building consultation, a
    /// hired performance."</summary>
    public Money HostFee { get; }

    /// <summary>What a §6 Recruit engagement draws from the recruiting household's own Ledger — a
    /// permanent offer, priced strictly above <see cref="HostFee"/> for every type.</summary>
    public Money RecruitFee { get; }

    /// <summary>The Dignitas a Host engagement delivers to the hosting household — this item's own
    /// honest realization of §6's "a Cultural Prestige boost." No Cultural Prestige field exists
    /// anywhere in this codebase; <see cref="Reputation.HouseholdReputation"/>'s Dignitas is the real,
    /// already-built household-standing primitive, and §6's own benefit list is explicitly a menu
    /// ("a Cultural Prestige boost, a construction discount, a rare goods purchase, a Health recovery,
    /// a Favor gain") rather than a per-type contract. See <see cref="HostWandererCommands"/> for
    /// which of the other four have a real hook this pass and which are disclosed as deferred.</summary>
    public int HostDignitasGain { get; }

    /// <summary>How far a successful engagement moves the Wanderer's own Fame (§4: "a Wanderer's own
    /// Fame rises after a successful engagement"), applied through <see
    /// cref="WandererFameCalculator.ApplyDelta"/>.</summary>
    public int EngagementFameGain { get; }

    /// <summary>The real Familia <see cref="DutySlot"/> a successful Recruit is placed into (§6: "a
    /// hosted physician becomes the household's own Court Physician"), or null when no existing duty
    /// slot honestly fits this type — see <see cref="WandererTypeCatalog"/>'s own doc comment for the
    /// per-type reasoning and the disclosure for the four types that get null.</summary>
    public DutySlot? RecruitedDutySlot { get; }
}
