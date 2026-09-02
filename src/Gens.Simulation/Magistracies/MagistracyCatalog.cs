namespace Gens.Simulation.Magistracies;

/// <summary>Versioned constants for Phase 12 item 2's Local Magistracies mechanics (§5), matching <see
/// cref="Clientela.ClientelaCatalog"/>'s identical "unsized against real playtest data, but named in
/// one place" convention — §12's Open Questions explicitly leaves every figure below unsized.</summary>
public static class MagistracyCatalog
{
    /// <summary>Every local office's term length (§5.7: "every local office runs on an annual term").</summary>
    public const int TermLengthMonths = 12;

    /// <summary>The household Dignitas floor <see cref="AppointDecurionCommand"/> requires (§5.1:
    /// "requires the building, a Dignitas/Core Attribute threshold").</summary>
    public const int DecurionDignitasThreshold = 20;

    /// <summary>The fixed number of Curia seats a settlement's Decurion body holds. §5.6 describes this
    /// as "a fixed-per-settlement-size number" — no Buildings/Settlement Demographics sizing formula
    /// exists to derive that from, so this item uses one flat constant across every settlement instead
    /// of inventing a scaling rule the design doc never specifies.</summary>
    public const int DecurionCuriaSeatCount = 10;

    /// <summary>Monthly passive Dignitas trickle per active office, applied by <see
    /// cref="MagistracyTermSystem"/> (§5.1: "a modest, passive Dignitas trickle"; §5.4: "the ladder's
    /// largest passive Dignitas bonus"). Deliberately ranked Decurion &lt; Aedile ≈ QuaestorLocal &lt;
    /// Duumvir, per §5's own relative framing, rather than named absolute figures the design doc never
    /// gives.</summary>
    public const int DecurionMonthlyDignitas = 1;
    public const int AedileMonthlyDignitas = 2;
    public const int QuaestorLocalMonthlyDignitas = 2;
    public const int DuumvirMonthlyDignitas = 4;

    /// <summary>Phase 15 item 6's own reading of §2's "sits above Duumvir in real historical
    /// prestige" — the capstone office's monthly trickle exceeds <see cref="DuumvirMonthlyDignitas"/>,
    /// this item's own invented figure per every other value in this table's identical unsized
    /// convention.</summary>
    public const int CensorMonthlyDignitas = 6;

    /// <summary>Dignitas swing from <see cref="AedileFundingChoice"/> (§5.2: "a real choice... and a
    /// real consequence — a Dignitas/Contentment boost if funded well, a modest Dignitas cost if
    /// skipped"). The Contentment half of that sentence is out of this item's scope — see <see
    /// cref="FundAedileWorksCommand"/>'s own doc comment.</summary>
    public const int AedileFundGenerouslyDignitasGain = 6;
    public const int AedileFundMinimallyDignitasGain = 2;
    public const int AedileLetItPassDignitasCost = 3;

    /// <summary>The extra Dignitas penalty §5.7 says an Insolvency- or conviction-driven loss of office
    /// carries "on top of the office itself" — deliberately more severe than an ordinary lost
    /// re-election, which carries none.</summary>
    public const int EarlyLossDignitasPenalty = 10;

    /// <summary>The Core Attribute an election's "relevant Core Attribute" (§5.5) resolves against.
    /// §5.5 never names one — Diplomacy is this implementation's own pick, as the attribute most
    /// obviously mapping to winning over a Curia electorate, matching <see
    /// cref="Reputation.AdjustDignitasCommand"/>'s own precedent of picking a concrete default where
    /// the design doc leaves an open slot.</summary>
    public const int FactionAlignmentBonus = 10;
}

/// <summary>The Aedile's "real choice" (§5.2) each time the office's occasional funding duty comes up.
/// See <see cref="FundAedileWorksCommand"/>.</summary>
public enum AedileFundingChoice
{
    FundGenerously,
    FundMinimally,
    LetItPass,
}
