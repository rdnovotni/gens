using Gens.Simulation.Chronicle;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;

namespace Gens.Simulation.Funerary;

/// <summary>
/// Every numeric baseline <c>gens-ancestor-veneration-funerary-customs-design.md</c> §10 leaves as an
/// Open Question ("all numeric sizing deferred, per convention") — this implementation's own invented
/// values, matching <see cref="Succession.SuccessionCatalog"/>'s identical disclaimer for its own
/// untuned numbers. A plain C# static class, not content-authored JSON, for the same reason <see
/// cref="Succession.SuccessionCatalog"/> gives.
/// </summary>
public static class FuneraryCatalog
{
    /// <summary>How many months household <see cref="Luctus"/> mourning lasts after a death (§4.1) —
    /// this implementation's own invented baseline; the design doc leaves the exact duration
    /// unsized.</summary>
    public const int MourningDurationMonths = 3;

    /// <summary>How many months a <see cref="FuneralRecord"/> may sit <see
    /// cref="FuneralStatus.Pending"/> awaiting a player's <see cref="ChooseFuneralTierCommand"/> before
    /// <see cref="FuneralAutoResolutionSystem"/> resolves it at <see cref="AutoResolutionDefaultTier"/>
    /// on the household's behalf — a background/NPC household, or a player who simply never gets
    /// around to it, still gets a real funeral rather than an indefinitely Pending one, matching <see
    /// cref="Succession.SuccessionCatalog.DisputeResolutionMonths"/>'s identical "resolve automatically
    /// after N months" shape.</summary>
    public const int FuneralAutoResolutionAfterMonths = 2;

    /// <summary>The tier <see cref="FuneralAutoResolutionSystem"/> falls back to — the design doc's own
    /// §2.2 framing of Modest as "appropriate for a household... the family has real reason not to
    /// publicize" reads naturally as the safe unattended default.</summary>
    public const FuneralTier AutoResolutionDefaultTier = FuneralTier.Modest;

    /// <summary>The one-time Treasury cost of holding a funeral at this tier (§2.2's "trading a real,
    /// one-time Treasury cost against the funeral's Memoria and Dignitas yield" — no personal or
    /// household Dignitas stat is tracked yet, per <see cref="DeclareHeirCommand"/>'s own doc comment,
    /// so only the Memoria half of that trade is implemented here).</summary>
    public static Money TreasuryCost(FuneralTier tier) => tier switch
    {
        FuneralTier.Modest => Money.FromDenarii(15),
        FuneralTier.Proper => Money.FromDenarii(50),
        FuneralTier.Grand => Money.FromDenarii(150),
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unknown Funeral Tier."),
    };

    /// <summary>This tier's flat Memoria yield before any ancestral-achievement scaling (§2.2, §6.1).</summary>
    public static int BaseMemoriaGain(FuneralTier tier) => tier switch
    {
        FuneralTier.Modest => 3,
        FuneralTier.Proper => 8,
        FuneralTier.Grand => 20,
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unknown Funeral Tier."),
    };

    /// <summary>How much each existing Major/Legendary Dynasty Chronicle entry already on the
    /// household's record adds to a Grand funeral's Memoria yield (§2.2's "the tier's own Memoria...
    /// payoff scales with how much real ancestral achievement... the household actually has to
    /// display") — deliberately scoped to Grand tier only, matching the design text's own framing; a
    /// Modest or Proper funeral's yield is flat regardless of ancestry.</summary>
    public const int AncestralAchievementMemoriaPerEntry = 2;

    /// <summary>Caps <see cref="AncestralAchievementMemoriaPerEntry"/>'s contribution so a genuinely
    /// old family's Grand funeral yield stays bounded rather than growing without limit across a long
    /// campaign.</summary>
    public const int AncestralAchievementMemoriaCap = 20;

    /// <summary>Counts the household's own existing Major/Legendary Chronicle entries (§6.1's "real
    /// ancestral achievement already on record") and converts that into <see
    /// cref="AncestralAchievementMemoriaPerEntry"/>-scaled, <see
    /// cref="AncestralAchievementMemoriaCap"/>-capped bonus Memoria. Reads only entries already
    /// persisted at call time — the funeral's own eventual Chronicle entry (recorded afterward by <see
    /// cref="ChronicleGenerationSystem"/>, outside this same command/tick) never counts toward its own
    /// bonus.</summary>
    public static int AncestralAchievementBonus(WorldState state, RuntimeId<Household> householdId)
    {
        var count = 0;
        foreach (var entry in state.ChronicleEntries.InAscendingOrder())
        {
            if (entry.Value.HouseholdId == householdId &&
                entry.Value.Tier is ChronicleTier.Major or ChronicleTier.Legendary)
                count++;
        }

        return Math.Min(count * AncestralAchievementMemoriaPerEntry, AncestralAchievementMemoriaCap);
    }

    /// <summary>Whether a Grand funeral actually displays the household's <c>imagines</c> (§2.2: "a
    /// Grand funeral for a Character with few or no Chronicle-notable ancestors reads as hollow
    /// ambition rather than earned grandeur") — true only at Grand tier and only when the household has
    /// at least one Major/Legendary Chronicle entry to actually display.</summary>
    public static bool ImaginesDisplayed(FuneralTier tier, int ancestralAchievementBonus) =>
        tier == FuneralTier.Grand && ancestralAchievementBonus > 0;

    /// <summary>February — the historical <c>Parentalia</c> window (§5.1's "traditionally the
    /// 13th-21st"), collapsed to a single once-a-year credit on this month rather than modeling the
    /// nine-day festival's own internal structure.</summary>
    public const int ParentaliaMonthOfYear = 2;

    /// <summary>The small annual Treasury cost of a household's own tomb offering (§5.1's "wine, milk,
    /// and flowers") — small enough that only a household in genuine financial distress fails to afford
    /// it, matching §6.3's "a household in genuine financial crisis unable to afford even a modest
    /// offering" as the one thing that causes a skip.</summary>
    public static readonly Money ParentaliaOfferingCost = Money.FromDenarii(3);

    /// <summary>The flat Memoria gain from a successfully observed <c>Parentalia</c> (§5.1's "the
    /// single most reliable ongoing source" — structurally parallel to Religion's own household-worship
    /// Favor mechanic).</summary>
    public const int ParentaliaBaseMemoriaGain = 2;

    /// <summary>Per-Major/Legendary-Chronicle-entry trickle folded into the same annual credit (§6.1's
    /// "a Dynasty Chronicle entry for any ancestor... contributes a small, permanent Memoria trickle
    /// for as long as that entry remains part of the family record" — realized here as part of
    /// <c>Parentalia</c>'s own once-a-year credit rather than a separate monthly system, since nothing
    /// else in this pass needs a finer-grained trickle).</summary>
    public const int ChronicleEntryMemoriaTrickle = 1;

    /// <summary>Caps <see cref="ChronicleEntryMemoriaTrickle"/>'s contribution, matching <see
    /// cref="AncestralAchievementMemoriaCap"/>'s identical bounded-growth reasoning.</summary>
    public const int ChronicleTrickleCap = 15;

    /// <summary>The Memoria loss from a skipped <c>Parentalia</c> (§5.1's "a skipped one... is
    /// Memoria's most common source of quiet drift downward"; §6.3's "erodes from real neglect rather
    /// than active offense").</summary>
    public const int ParentaliaSkippedMemoriaLoss = 2;

    /// <summary>Folds a household's existing Major/Legendary Chronicle entries into the same
    /// per-entry-trickle shape <see cref="AncestralAchievementBonus"/> uses for funerals, but with
    /// <see cref="ChronicleEntryMemoriaTrickle"/>/<see cref="ChronicleTrickleCap"/> instead — kept as a
    /// separate method (rather than reusing <see cref="AncestralAchievementBonus"/> directly) because
    /// the two callers' per-entry rate and cap are independently invented baselines that happen to
    /// share the same counting logic, not the same numbers.</summary>
    public static int ChronicleTrickle(WorldState state, RuntimeId<Household> householdId)
    {
        var count = 0;
        foreach (var entry in state.ChronicleEntries.InAscendingOrder())
        {
            if (entry.Value.HouseholdId == householdId &&
                entry.Value.Tier is ChronicleTier.Major or ChronicleTier.Legendary)
                count++;
        }

        return Math.Min(count * ChronicleEntryMemoriaTrickle, ChronicleTrickleCap);
    }
}
