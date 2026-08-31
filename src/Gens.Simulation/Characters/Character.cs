using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Time;
using Gens.Simulation.Travel;

namespace Gens.Simulation.Characters;

/// <summary>
/// The canonical Character record (Phase 5 item 1; <c>gens-characters-design.md</c> §14,
/// <c>gens-familia-design.md</c> §8): every named individual the game tracks — family, slave,
/// freedman, client, companion, rival, or encounter alike — gets this same full shape, immediately,
/// per <c>gens-characters-design.md</c> §2's "no more lightweight tier" decision. This item only
/// establishes the record's shape and validation; name/appearance generation (item 2), lifecycle
/// transitions and aging (item 3), traits (item 4), relationships (item 5), and household roles
/// (item 6) are separate Phase 5 work. Lazy instantiation/promotion (item 7) is <see
/// cref="PromoteToNamedCommand"/>, which creates records of exactly this shape from an aggregate <see
/// cref="PopGroup"/> entry.
/// </summary>
public sealed record Character
{
    private Character()
    {
    }

    public required RuntimeId<Character> Id { get; init; }

    // Identity (gens-familia-design.md §2.8).
    public required string Praenomen { get; init; }
    public required string Nomen { get; init; }
    public string? Cognomen { get; init; }
    public required Sex Sex { get; init; }
    public required GameDate BirthDate { get; init; }

    // Fixed-genetics appearance (gens-familia-design.md §2.4 / Core §7.11; item 2).
    public required CharacterVisualProfile VisualProfile { get; init; }

    // Legal status & social class (gens-familia-design.md §2.5).
    public required LegalStatus LegalStatus { get; init; }
    public SocialClass? SocialClass { get; init; }

    // Culture (content-authored; gens-familia-design.md §8's originCulture).
    public required DefinitionId<Culture> Culture { get; init; }

    // Location & household membership.
    public required RuntimeId<Settlement> Location { get; init; }
    public RuntimeId<Household>? Household { get; init; }

    // Attributes, skills, condition (gens-familia-design.md §2.1-2.3).
    public required CoreAttributes Attributes { get; init; }
    public required LaborSkills Skills { get; init; }
    public required Condition Condition { get; init; }

    // Provenance (gens-characters-design.md §14).
    public required CharacterSource Source { get; init; }
    public required int InstantiatedAtMonth { get; init; }

    /// <summary>Whether this Character's traits/axes/history were generated as an age-appropriate life
    /// history compressed into an instant, rather than accumulated in real time (§11) — true for
    /// nearly everyone <see cref="PromoteToNamedCommand"/> creates, false for a Familia-born child
    /// raised inside the simulation. Provenance metadata only: nothing downstream reads this to treat
    /// a backfilled Character as less "real" than one grown in play (§11's own framing).</summary>
    public bool BackfilledHistory { get; init; }

    // Parentage & legitimacy (gens-familia-design.md §5.2, §8; Phase 5 item 3).
    public RuntimeId<Character>? MotherId { get; init; }
    public RuntimeId<Character>? FatherId { get; init; }
    public Legitimacy Legitimacy { get; init; } = Legitimacy.Legitimate;

    // Marriage history (gens-familia-design.md §5, §5.1, §8).
    public IReadOnlyList<MarriageRecord> MaritalHistory { get; init; } = Array.Empty<MarriageRecord>();

    // Permanent injury (gens-familia-design.md §3.1, §8).
    public IReadOnlyList<PermanentInjury> PermanentInjuries { get; init; } = Array.Empty<PermanentInjury>();

    // Traits — definition references only, per rule 10 ("content is data, rules are code"); resolve
    // against a TraitCatalog for category, Axis nudge, opposed-pair, and tiered-spectrum data
    // (gens-traits-design.md; Phase 5 item 4). Exclusivity (opposed pairs, tiered spectrums) is
    // enforced by whatever grants a trait (TraitCatalog.CheckExclusivity), not by this record itself —
    // Character has no catalog to check against on its own.
    public IReadOnlyList<DefinitionId<Trait>> Traits { get; init; } = Array.Empty<DefinitionId<Trait>>();

    // Death (gens-familia-design.md §3). Non-null is itself the "dead" flag — see IsAlive below.
    public DeathRecord? DeathRecord { get; init; }

    // Household role (gens-familia-design.md §4, §8's role field; Phase 5 item 6). Labor duty slot
    // tier only — Court Positions belong to §6.20's own later design pass.
    public DutyAssignment? Duty { get; init; }

    // Labor & Slavery (gens-labor-slavery-design.md §5, §7, §8; Phase 6 item 6). Acquisition (§2),
    // flightRisk (derived — see FlightRiskCalculator, not stored per this record's own
    // GetLifecycleStage/GetEffectiveSkills convention), Sale (§9), and the vilicus tier (§4) are out
    // of this item's "basic" scope.
    public RegimenSettings? Regimen { get; init; }
    public FledRecord? Flight { get; init; }
    public PursuitRecord? Pursuit { get; init; }
    public ManumissionPlan? ManumissionPlan { get; init; }

    /// <summary>Phase 13 item 2 (<c>gens-travel-design.md</c> §5, §10) — where this Character actually
    /// is right now, tracked independently of <see cref="Household"/> membership the way CK3 tracks a
    /// court member's genuine whereabouts. Null means "at <see cref="Location"/>" (§10: "defaults to a
    /// 'home' Location") — the common case, and every Character's starting value; non-null only while
    /// a <see cref="TravelTrip"/> has them actually <see cref="TravelTripStatus.Arrived"/>, <see
    /// cref="TravelTripStatus.Returning"/>, or <see cref="TravelTripStatus.Recalled"/> somewhere else
    /// (<see cref="TravelProgressSystem"/> is the only system that ever sets or clears this).</summary>
    public TravelLocation? CurrentTravelLocation { get; init; }

    /// <summary>Hand-written for the same reason as <see cref="CharacterVisualProfile.Equals(CharacterVisualProfile?)"/>
    /// and <see cref="PortraitRecipe.Equals(PortraitRecipe?)"/>: the record-synthesized <c>Equals</c>
    /// would compare <see cref="MaritalHistory"/> and <see cref="PermanentInjuries"/> by list reference
    /// rather than by content, which breaks the moment a save round trip (or any other rebuild)
    /// produces an equal-but-distinct list instance.</summary>
    public bool Equals(Character? other) =>
        other is not null &&
        Id == other.Id &&
        Praenomen == other.Praenomen &&
        Nomen == other.Nomen &&
        Cognomen == other.Cognomen &&
        Sex == other.Sex &&
        BirthDate == other.BirthDate &&
        VisualProfile == other.VisualProfile &&
        LegalStatus == other.LegalStatus &&
        SocialClass == other.SocialClass &&
        Culture == other.Culture &&
        Location == other.Location &&
        Household == other.Household &&
        Attributes == other.Attributes &&
        Skills == other.Skills &&
        Condition == other.Condition &&
        Source == other.Source &&
        InstantiatedAtMonth == other.InstantiatedAtMonth &&
        BackfilledHistory == other.BackfilledHistory &&
        MotherId == other.MotherId &&
        FatherId == other.FatherId &&
        Legitimacy == other.Legitimacy &&
        MaritalHistory.SequenceEqual(other.MaritalHistory) &&
        PermanentInjuries.SequenceEqual(other.PermanentInjuries) &&
        Traits.SequenceEqual(other.Traits) &&
        DeathRecord == other.DeathRecord &&
        Duty == other.Duty &&
        Regimen == other.Regimen &&
        Flight == other.Flight &&
        Pursuit == other.Pursuit &&
        ManumissionPlan == other.ManumissionPlan &&
        CurrentTravelLocation == other.CurrentTravelLocation;

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Id);
        hash.Add(Praenomen);
        hash.Add(Nomen);
        hash.Add(Cognomen);
        hash.Add(Sex);
        hash.Add(BirthDate);
        hash.Add(VisualProfile);
        hash.Add(LegalStatus);
        hash.Add(SocialClass);
        hash.Add(Culture);
        hash.Add(Location);
        hash.Add(Household);
        hash.Add(Attributes);
        hash.Add(Skills);
        hash.Add(Condition);
        hash.Add(Source);
        hash.Add(InstantiatedAtMonth);
        hash.Add(BackfilledHistory);
        hash.Add(MotherId);
        hash.Add(FatherId);
        hash.Add(Legitimacy);
        foreach (var marriage in MaritalHistory)
            hash.Add(marriage);
        foreach (var injury in PermanentInjuries)
            hash.Add(injury);
        foreach (var trait in Traits)
            hash.Add(trait);
        hash.Add(DeathRecord);
        hash.Add(Duty);
        hash.Add(Regimen);
        hash.Add(Flight);
        hash.Add(Pursuit);
        hash.Add(ManumissionPlan);
        hash.Add(CurrentTravelLocation);
        return hash.ToHashCode();
    }

    /// <summary>The only supported way to construct a <see cref="Character"/> — validates the
    /// cross-field invariant an object initializer can't enforce on its own (<see cref="SocialClass"/>
    /// is citizen-only, <c>gens-familia-design.md</c> §2.5; a Character cannot be its own parent)
    /// before returning.</summary>
    public static Character Create(
        RuntimeId<Character> id,
        string praenomen,
        string nomen,
        string? cognomen,
        Sex sex,
        GameDate birthDate,
        CharacterVisualProfile visualProfile,
        LegalStatus status,
        SocialClass? socialClass,
        DefinitionId<Culture> culture,
        RuntimeId<Settlement> location,
        RuntimeId<Household>? household,
        CoreAttributes attributes,
        LaborSkills skills,
        Condition condition,
        CharacterSource source,
        int instantiatedAtMonth,
        bool backfilledHistory = false,
        RuntimeId<Character>? motherId = null,
        RuntimeId<Character>? fatherId = null,
        Legitimacy legitimacy = Legitimacy.Legitimate,
        IReadOnlyList<MarriageRecord>? maritalHistory = null,
        IReadOnlyList<PermanentInjury>? permanentInjuries = null,
        IReadOnlyList<DefinitionId<Trait>>? traits = null,
        DeathRecord? deathRecord = null,
        DutyAssignment? duty = null,
        RegimenSettings? regimen = null,
        FledRecord? flight = null,
        PursuitRecord? pursuit = null,
        ManumissionPlan? manumissionPlan = null,
        TravelLocation? currentTravelLocation = null)
    {
        if (string.IsNullOrWhiteSpace(praenomen))
            throw new ArgumentException("A Character requires a non-empty praenomen.", nameof(praenomen));
        if (string.IsNullOrWhiteSpace(nomen))
            throw new ArgumentException("A Character requires a non-empty nomen.", nameof(nomen));
        if (visualProfile is null)
            throw new ArgumentNullException(nameof(visualProfile));
        if (socialClass is not null && status != LegalStatus.RomanCitizen)
            throw new ArgumentException(
                $"'{nameof(socialClass)}' can only be set for a {LegalStatus.RomanCitizen} " +
                $"(gens-familia-design.md §2.5); got legal status '{status}'.",
                nameof(socialClass));
        if (motherId == id)
            throw new ArgumentException("A Character cannot be its own mother.", nameof(motherId));
        if (fatherId == id)
            throw new ArgumentException("A Character cannot be its own father.", nameof(fatherId));

        return new Character
        {
            Id = id,
            Praenomen = praenomen,
            Nomen = nomen,
            Cognomen = cognomen,
            Sex = sex,
            BirthDate = birthDate,
            VisualProfile = visualProfile,
            LegalStatus = status,
            SocialClass = socialClass,
            Culture = culture,
            Location = location,
            Household = household,
            Attributes = attributes,
            Skills = skills,
            Condition = condition,
            Source = source,
            InstantiatedAtMonth = instantiatedAtMonth,
            BackfilledHistory = backfilledHistory,
            MotherId = motherId,
            FatherId = fatherId,
            Legitimacy = legitimacy,
            MaritalHistory = maritalHistory ?? Array.Empty<MarriageRecord>(),
            PermanentInjuries = permanentInjuries ?? Array.Empty<PermanentInjury>(),
            Traits = traits ?? Array.Empty<DefinitionId<Trait>>(),
            DeathRecord = deathRecord,
            Duty = duty,
            Regimen = regimen,
            Flight = flight,
            Pursuit = pursuit,
            ManumissionPlan = manumissionPlan,
            CurrentTravelLocation = currentTravelLocation,
        };
    }

    /// <summary>Derives the lifecycle stage as of <paramref name="asOf"/> from <see cref="BirthDate"/>
    /// (<c>gens-familia-design.md</c> §3's age bands) — never stored, matching <see
    /// cref="GameDate"/>'s own "derived, never stored redundantly" convention.</summary>
    public LifecycleStage GetLifecycleStage(GameDate asOf)
    {
        // Checked directly in months, not via AgeInYears: integer division truncates toward zero, so
        // a BirthDate 1-11 months ahead of asOf would otherwise floor to an in-range 0 years old
        // instead of being caught as "before birth."
        if (asOf.TotalMonths < BirthDate.TotalMonths)
            throw new ArgumentOutOfRangeException(nameof(asOf), asOf, "A Character cannot have a negative age.");

        return AgeInYears(asOf) switch
        {
            <= 3 => LifecycleStage.Infant,
            <= 12 => LifecycleStage.Child,
            <= 17 => LifecycleStage.Adolescent,
            <= 59 => LifecycleStage.Adult,
            _ => LifecycleStage.Elderly,
        };
    }

    /// <summary>Whole years elapsed between <see cref="BirthDate"/> and <paramref name="asOf"/>,
    /// floor-divided from the project's sole month-granular time representation (ADR 0003).</summary>
    public int AgeInYears(GameDate asOf) => (asOf.TotalMonths - BirthDate.TotalMonths) / 12;

    /// <summary>Whether this Character is still living. Derived from <see cref="DeathRecord"/> rather
    /// than a separate stored flag — a non-null death record is itself the only source of truth.</summary>
    public bool IsAlive => DeathRecord is null;

    /// <summary>The spouse from the open (<see cref="MarriageRecord.EndDate"/> is <c>null</c>) entry
    /// in <see cref="MaritalHistory"/>, if any. Invariant: at most one entry should ever be open at a
    /// time (a Character cannot be married to two people simultaneously); this deliberately does not
    /// throw if that invariant is somehow violated — it just returns the first open entry found.</summary>
    public RuntimeId<Character>? CurrentSpouseId
    {
        get
        {
            foreach (var record in MaritalHistory)
                if (record.EndDate is null)
                    return record.SpouseId;
            return null;
        }
    }

    /// <summary>Core Attributes reduced by any <see cref="PermanentInjuries"/> targeting them
    /// (<c>gens-familia-design.md</c> §3.1's "standing penalty" mechanism), clamped back to the
    /// normal 0-100 range. <see cref="Attributes"/> itself is never mutated by an injury — this is the
    /// observable, injury-adjusted view other systems should read instead.</summary>
    public CoreAttributes GetEffectiveAttributes() => new(
        Math.Clamp(Attributes.Diplomacy - SumInjuryMagnitude(PermanentInjuryTarget.Diplomacy), 0, 100),
        Math.Clamp(Attributes.Martial - SumInjuryMagnitude(PermanentInjuryTarget.Martial), 0, 100),
        Math.Clamp(Attributes.Stewardship - SumInjuryMagnitude(PermanentInjuryTarget.Stewardship), 0, 100),
        Math.Clamp(Attributes.Intrigue - SumInjuryMagnitude(PermanentInjuryTarget.Intrigue), 0, 100),
        Math.Clamp(Attributes.Learning - SumInjuryMagnitude(PermanentInjuryTarget.Learning), 0, 100));

    /// <summary>Labor Skills reduced by any <see cref="PermanentInjuries"/> targeting them, clamped
    /// back to the normal 0-100 range. See <see cref="GetEffectiveAttributes"/> for the same mechanism
    /// applied to Core Attributes.</summary>
    public LaborSkills GetEffectiveSkills() => new(
        Math.Clamp(Skills.Fieldwork - SumInjuryMagnitude(PermanentInjuryTarget.Fieldwork), 0, 100),
        Math.Clamp(Skills.DomesticService - SumInjuryMagnitude(PermanentInjuryTarget.DomesticService), 0, 100),
        Math.Clamp(Skills.Craft - SumInjuryMagnitude(PermanentInjuryTarget.Craft), 0, 100),
        Math.Clamp(Skills.Culinary - SumInjuryMagnitude(PermanentInjuryTarget.Culinary), 0, 100),
        Math.Clamp(Skills.Medicine - SumInjuryMagnitude(PermanentInjuryTarget.Medicine), 0, 100));

    /// <summary><see cref="Condition.Fertility"/> reduced by any <see cref="PermanentInjuries"/>
    /// targeting it (e.g. "a difficult birth's lasting toll on the mother", §3.1), clamped back to the
    /// normal 0-100 range. <see cref="Condition.Health"/> is deliberately never adjusted this way —
    /// §3.1 excludes it from the permanent-injury mechanism entirely.</summary>
    public int GetEffectiveFertility() =>
        Math.Clamp(Condition.Fertility - SumInjuryMagnitude(PermanentInjuryTarget.Fertility), 0, 100);

    private int SumInjuryMagnitude(PermanentInjuryTarget target)
    {
        var total = 0;
        foreach (var injury in PermanentInjuries)
            if (injury.Target == target)
                total += injury.Magnitude;
        return total;
    }
}
