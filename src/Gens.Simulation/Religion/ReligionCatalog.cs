using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;

namespace Gens.Simulation.Religion;

/// <summary>Versioned constants for Phase 12 item 3's Religion mechanics (<c>gens-religion-design.md</c>),
/// matching <see cref="Magistracies.MagistracyCatalog"/>'s identical "unsized against real playtest
/// data, but named in one place" convention — §11's Open Questions explicitly leaves every numeric
/// figure below unsized ("Favor gain/loss magnitudes, the Divine Displeasure threshold, Omen
/// frequency/severity curves, Auspices reliability deltas... all unsized").</summary>
public static class ReligionCatalog
{
    /// <summary>§2.3's low-Favor threshold below which a household enters Divine Displeasure.</summary>
    public const int DivineDispleasureThreshold = -15;

    /// <summary>§2.1: Reconsecration "is a Funded Action — a real ceremony, not a menu toggle." A
    /// fixed cost rather than a caller-chosen amount (unlike <see
    /// cref="Policies.FundFestivalCommand"/>'s open spend) — a Reconsecration is a single, ritually
    /// fixed rite, not a scalable, "the more you spend the more you get" event the way a Festival is
    /// (§5's "a real Favor and Dignitas payoff sized to the spend" applies only to §5's own funded
    /// observance, not to §2.1's ceremony).</summary>
    public static readonly Money ReconsecrationCeremonyCost = Money.FromDenarii(30);

    /// <summary>§4.1: "heed it (a concrete, usually modest cost paid now)... in exchange for averting."
    /// The Favor half of that concrete cost/benefit — heeding always averts, so this is framed as a
    /// gain rather than a net cost, matching §2.2's own "a correctly-heeded Omen" Favor-gain bullet.</summary>
    public const int OmenHeededFavorGain = 2;

    /// <summary>§4.1: ignoring an omen carries "a real chance the omen was accurate and the warned-of
    /// consequence lands anyway" — this is that consequence's Favor cost.</summary>
    public const int OmenIgnoredConsequenceFavorLoss = 6;

    /// <summary>§8/§4.1: "a Zealous [Character] suffers a real Favor and morale cost for ignoring [an
    /// omen] even when nothing bad follows." Only the Favor half is wired — Settlement Demographics'
    /// Contentment (the closest existing "morale"-shaped meter) has no per-Character write path this
    /// item's own household-scoped Favor primitive can route a single Character's morale cost through,
    /// matching <see cref="Magistracies.FundAedileWorksCommand"/>'s own "only the Dignitas half is
    /// wired" precedent for an identical two-part design-doc consequence.</summary>
    public const int ZealousIgnoredNoConsequencePenalty = 2;

    /// <summary>Percent chance, per point of <see cref="Religion.OmenEvent.Severity"/>, that an ignored
    /// Omen's warned-of consequence actually lands (§4.1's "a real chance the omen was accurate") —
    /// e.g. a severity-2 Omen lands 40% of the time. Unsized against any real curve (§11).</summary>
    public const int OmenIgnoredConsequenceChancePerSeverityPercent = 20;

    /// <summary>§4.1's Severity range this domain accepts on <see cref="RaiseOmenCommand"/> — a small,
    /// closed 1-3 scale rather than an open int, so the chance-per-severity formula above never exceeds
    /// 100%.</summary>
    public const int MinOmenSeverity = 1;
    public const int MaxOmenSeverity = 3;

    /// <summary>§6.2's Piety-tier gate for the state Priesthood track ("gated by the Piety trait tier
    /// (Devout or Zealous, Traits §3.5)") — the content-authored trait ids <c>
    /// content/source/traits/piety.json</c> actually defines for that spectrum, read directly off <see
    /// cref="Character.Traits"/> the same way <see cref="Reputation.AdjustDignitasCommand"/>'s own
    /// <c>Reason</c> parameter references a plain string rather than a closed enum — here the ids are
    /// concrete and known (the piety spectrum is real, shipped content, unlike Fame's genuinely
    /// nonexistent field), so a direct <see cref="DefinitionId{T}"/> membership check is a real,
    /// non-parallel read of the trait system rather than an invented stand-in for it. No compiled <see
    /// cref="TraitCatalog"/> is available to any command or <see cref="Time.IMonthlySystem{TState}"/>
    /// in this codebase (<see cref="Time.MonthlyTickContext"/> carries only <see cref="Time.GameDate"/>
    /// and the random stream registry) — checking for these specific, already-known ids is the
    /// principled alternative to a catalog-driven spectrum-tier lookup this item has no access path
    /// to.</summary>
    public static readonly DefinitionId<Trait> ImpiousTraitId = new("impious");
    public static readonly DefinitionId<Trait> DevoutTraitId = new("devout");
    public static readonly DefinitionId<Trait> ZealousTraitId = new("zealous");

    /// <summary>§6.2's Learning threshold for the Augur/Flamen priesthood gate ("gated by... Learning
    /// rather than by Politics &amp; Patronage's Dignitas/citizenship gate alone").</summary>
    public const int PriesthoodLearningThreshold = 20;

    /// <summary>§2.2: "Holding a state Priesthood... [is a Favor gain]" — the one-time gain on
    /// assumption, alongside the equivalent Dignitas gain applied through <see
    /// cref="Reputation.AdjustDignitasCommand"/>.</summary>
    public const int PriesthoodAssumedFavorGain = 5;
    public const int PriesthoodAssumedDignitasGain = 5;

    /// <summary>Monthly passive Favor/Dignitas trickle per held office (<see
    /// cref="PriesthoodTrickleSystem"/>), ranked Augur &lt; Flamen &lt; Pontifex per §6.2/§6.3's own
    /// relative framing ("the single strongest available multiplier" for Flamen; Pontifex "real
    /// Dignitas and Favor weight" as the capstone), matching <see
    /// cref="Magistracies.MagistracyCatalog"/>'s identical "ranked, not absolute" precedent for its own
    /// unsized office trickle.</summary>
    public const int AugurMonthlyFavor = 1;
    public const int AugurMonthlyDignitas = 2;
    public const int FlamenMonthlyFavor = 3;
    public const int FlamenMonthlyDignitas = 2;
    public const int PontifexMonthlyFavor = 4;
    public const int PontifexMonthlyDignitas = 5;

    /// <summary>§4.2's Auspices fee — a flat denarii cost standing in for "consumes Incense (Resources
    /// &amp; Goods)": no <c>incense</c> Good is defined anywhere in <c>content/source/goods/</c> at the
    /// time this item was built (only metals/staples/textiles exist), so this item prices the action in
    /// Money through the existing Ledger rather than inventing an unbacked Good reference — matching
    /// <see cref="Magistracies.AppointDecurionCommand"/>'s own "no building exists, so that half of the
    /// gate is not checked" precedent applied to a resource instead of a building.</summary>
    public static readonly Money AuspicesFee = Money.FromDenarii(8);

    /// <summary>§4.2/§2.2's "a well-executed Auspices reading" Favor gain, at the two reliability tiers
    /// this item actually builds (household default vs. an Augur officeholder — see <see
    /// cref="AuspicesReliabilityTier"/>). The itinerant-Haruspex middle tier §6.2 also describes is not
    /// built — see <see cref="CommissionAuspicesCommand"/>'s own doc comment.</summary>
    public const int AuspicesDefaultFavorGain = 1;
    public const int AuspicesAugurFavorGain = 3;

    /// <summary>§5's passive feast-day observance — "a small automatic Favor tick and nothing more."</summary>
    public const int PassiveFeastDayFavorGain = 1;

    /// <summary>§5's funded observance — "a real Favor and Dignitas payoff sized to the spend" — read as
    /// one Favor point per this many denarii spent (floored at 1 for any positive spend), and
    /// separately for Dignitas at a coarser rate, matching <see
    /// cref="Magistracies.FundAedileWorksCommand"/>'s own placeholder-sized "real, if placeholder-sized,
    /// Dignitas consequences" precedent for an identically unsized design-doc payoff.</summary>
    public const int FestivalFavorPerDenarii = 10;
    public const int FestivalDignitasPerDenarii = 20;
}
