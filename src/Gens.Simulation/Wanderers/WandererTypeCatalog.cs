using Gens.Simulation.Characters;
using Gens.Simulation.Ledger;
using Gens.Simulation.Regions;

namespace Gens.Simulation.Wanderers;

/// <summary>
/// Content catalog of every authored <see cref="WandererTypeProfile"/>, mirroring
/// <c>Health.HealthConditionCatalog</c>/<c>Cultures.CultureCatalog</c>'s identical "reject duplicate
/// IDs at construction" shape. Not part of <c>WorldState</c>: like those catalogs, this is content a
/// caller loads once and consults by key, not campaign state.
///
/// <para><b>Every number here is this implementation's own invented figure</b>, disclosed the same way
/// <c>Health.SanitationInvestmentCalculator</c> and <c>Hazards.DisasterSeverityCalculator</c> already
/// disclose theirs: §11's own first open question is "All numeric sizing... Fame growth/decay rates,
/// itinerary movement frequency, and the Prominence threshold for a direct approach are all unsized."
/// The figures are chosen only so a Recruit costs strictly more than a Host for every type, a type
/// whose engagement is a bigger public spectacle (Philosopher, Entertainer) delivers strictly more
/// Dignitas and Fame than a private one (Physician, Architect), and no type is free.</para>
///
/// <para><b>Which of §2/§3's named gravitational pulls are real reads and which are disclosed
/// proxies.</b> §3 names five: Institutions of Renown/high Prominence (Philosopher), active
/// construction (Architect), Market Dynamics margins (Merchant), funded Games and Symposium demand
/// (Entertainer), and outbreaks/Court-Physician absence (Physician), plus §2's foreign-cult
/// opportunity (Holy Man). Exactly one of those — high <see cref="ProminenceTier"/> — is real,
/// queryable content in this codebase (<see cref="GazetteerLocationDefinition.ProminenceTier"/>) and is
/// read directly. The rest are not, and are not faked:
/// <list type="bullet">
/// <item>Education &amp; Culture's <b>Institutions of Renown</b> do not exist as any record, field, or
/// tag anywhere in this codebase, so the Philosopher's pull is <see
/// cref="WandererTypeProfile.PrefersHighProminence"/> plus the <see cref="GazetteerRole.ProvincialSeat"/>/
/// <see cref="GazetteerRole.Capital"/> roles — §3's own "and other high-Prominence Gazetteer locations"
/// clause taken as the whole rule rather than half of it.</item>
/// <item><b>Active construction</b> is not readable per Gazetteer entry: <see
/// cref="Buildings.BuildingInstance"/> is anchored to a runtime <see cref="Land.Plot"/>, and nothing
/// links a runtime Settlement/Plot back to a content Gazetteer entry (<see
/// cref="WandererItineraryStop"/>'s own disclosure). The Architect's pull is the honest structural
/// proxy — the roles where a major commission plausibly sits.</item>
/// <item><b>Market Dynamics margins</b> (Merchant): <c>Markets</c> prices are per-good and per-
/// settlement, again with no Gazetteer link, so the pull is the two genuinely commercial roles.</item>
/// <item><b>Funded Games and Symposium demand</b> (Entertainer): Games &amp; Spectacle is unbuilt
/// (<see cref="Fame.FameSourceType"/>'s own doc comment says so outright) and <c>Villas</c>
/// exposes no Symposium-demand reading, so the pull is the two most public roles.</item>
/// <item><b>Outbreaks and Court-Physician absence</b> (Physician): <see cref="Health.EpidemicOutbreak"/>
/// is real and live, but keyed by <see cref="Land.Settlement"/>, so it cannot steer movement across a
/// Gazetteer roster; the pull is the two roles with the densest standing populations.</item>
/// <item><b>Foreign-cult encounter opportunity</b> (Holy Man): <c>Religion</c> exposes no
/// per-place cult-opportunity reading, but <see cref="GazetteerRole.Sanctuary"/> is a real, authored
/// role meaning exactly "pilgrimage and favor-seeking anchor here" — the closest thing to a genuine
/// read in this list.</item>
/// </list>
/// This is the same "honest, disclosed proxy rather than a faked read" discipline
/// <c>Health.DiseaseCatalog</c> set for Saturnism's mining driver and <c>Hazards.HazardExposureCalculator</c>
/// set for Forest Cover.</para>
///
/// <para><b>Recruited duty slots.</b> §6's own worked example is a physician becoming the household's
/// Court Physician, and <see cref="DutySlot.Physician"/> is a real, already-built slot reading real
/// <see cref="LaborSkills.Medicine"/> — so that one maps directly. <see cref="DutySlot.Craftsman"/>
/// (reading <see cref="LaborSkills.Craft"/>) is the honest fit for an Architect/Engineer. The other
/// four types get null: <see cref="DutySlot"/>'s roster is one slot per Labor Skill axis, and none of
/// Fieldwork/DomesticService/Culinary is a defensible home for a philosopher, a merchant, an
/// entertainer, or a holy man. A Recruit of those four types still converts into a full, real <see
/// cref="Characters.Character"/> in the household (§6's actual load-bearing promise) — they simply
/// hold no duty slot, because Companions &amp; Court Positions' own non-Labor position roster does not
/// exist in this codebase yet.</para>
/// </summary>
public sealed class WandererTypeCatalog
{
    private readonly Dictionary<WandererType, WandererTypeProfile> _entries;

    public WandererTypeCatalog(IEnumerable<WandererTypeProfile> profiles)
    {
        if (profiles is null)
            throw new ArgumentNullException(nameof(profiles));

        var map = new Dictionary<WandererType, WandererTypeProfile>();
        foreach (var profile in profiles)
        {
            if (!map.TryAdd(profile.Type, profile))
                throw new ArgumentException($"Duplicate wanderer type '{profile.Type}'.", nameof(profiles));
        }

        _entries = map;
    }

    public int Count => _entries.Count;

    public bool TryGet(WandererType type, out WandererTypeProfile profile) =>
        _entries.TryGetValue(type, out profile!);

    public WandererTypeProfile Get(WandererType type) =>
        TryGet(type, out var profile)
            ? profile
            : throw new KeyNotFoundException($"No wanderer type profile is registered for '{type}'.");

    public IEnumerable<WandererTypeProfile> All() => _entries.Values;

    /// <summary>The authored roster covering all six §2 types. Unlike
    /// <c>Health.HealthConditionCatalog</c>, which shipped empty in its own item because its content
    /// belonged to the next item, this catalog is authored in full here: §2's six types <i>are</i> this
    /// item's content, and every one of them is exercised by <see cref="WandererSystem"/> and the
    /// engagement commands.</summary>
    public static WandererTypeCatalog BuildDefault() => new(new[]
    {
        new WandererTypeProfile(
            WandererType.PhilosopherRhetorician,
            preferredRoles: new[] { GazetteerRole.Capital, GazetteerRole.ProvincialSeat },
            prefersHighProminence: true,
            hostFee: Money.FromDenarii(120),
            recruitFee: Money.FromDenarii(600),
            hostDignitasGain: 6,
            engagementFameGain: 5,
            recruitedDutySlot: null),
        new WandererTypeProfile(
            WandererType.ArchitectEngineer,
            preferredRoles: new[] { GazetteerRole.ProvincialSeat, GazetteerRole.MajorPort },
            prefersHighProminence: true,
            hostFee: Money.FromDenarii(200),
            recruitFee: Money.FromDenarii(900),
            hostDignitasGain: 3,
            engagementFameGain: 3,
            recruitedDutySlot: DutySlot.Craftsman),
        new WandererTypeProfile(
            WandererType.MerchantPeddler,
            preferredRoles: new[] { GazetteerRole.MajorPort, GazetteerRole.MarketHub },
            prefersHighProminence: false,
            hostFee: Money.FromDenarii(80),
            recruitFee: Money.FromDenarii(400),
            hostDignitasGain: 2,
            engagementFameGain: 2,
            recruitedDutySlot: null),
        new WandererTypeProfile(
            WandererType.Entertainer,
            preferredRoles: new[] { GazetteerRole.Capital, GazetteerRole.ProvincialSeat },
            prefersHighProminence: true,
            hostFee: Money.FromDenarii(150),
            recruitFee: Money.FromDenarii(500),
            hostDignitasGain: 5,
            engagementFameGain: 5,
            recruitedDutySlot: null),
        new WandererTypeProfile(
            WandererType.Physician,
            preferredRoles: new[] { GazetteerRole.ProvincialSeat, GazetteerRole.LegionaryBase },
            prefersHighProminence: false,
            hostFee: Money.FromDenarii(100),
            recruitFee: Money.FromDenarii(450),
            hostDignitasGain: 2,
            engagementFameGain: 3,
            recruitedDutySlot: DutySlot.Physician),
        new WandererTypeProfile(
            WandererType.HolyManAstrologer,
            preferredRoles: new[] { GazetteerRole.Sanctuary, GazetteerRole.FrontierOutpost },
            prefersHighProminence: false,
            hostFee: Money.FromDenarii(60),
            recruitFee: Money.FromDenarii(350),
            hostDignitasGain: 3,
            engagementFameGain: 4,
            recruitedDutySlot: null),
    });
}
