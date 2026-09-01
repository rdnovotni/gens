using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Time;

namespace Gens.Simulation.Wanderers;

/// <summary>§6's two genuinely different, non-exclusive ways to use a Wanderer.</summary>
public enum WandererEngagementType
{
    /// <summary>"A one-time, lower-commitment engagement: a funded lecture, a building consultation, a
    /// hired performance, a course of treatment, a single consultation" (§6). The Wanderer stays
    /// independent and moves on afterward per their own Itinerary.</summary>
    Host,

    /// <summary>"A genuine, permanent offer to join the household outright, converting the Wanderer into
    /// a full Familia record the instant it succeeds" (§6).</summary>
    Recruit,
}

/// <summary>
/// One completed §6 engagement (§10's <c>WandererEngagement</c> record) — a second new <see
/// cref="RuntimeId{T}"/>-keyed <c>WorldState</c> partition, wired through the same five files
/// (<c>WorldState</c>/<c>StateHasher</c>/<c>WorldSaveDto</c>/<c>WorldStateMapper</c>/<c>EntityKinds</c>)
/// as <see cref="Wanderer"/> itself and kept forever once written, matching
/// <c>Hazards.DisasterEvent</c>'s identical "real campaign history, not scratch state" convention.
///
/// <para><b>§10's <c>benefitDelivered</c> is decomposed into the concrete fields this codebase can
/// actually maintain</b> rather than kept as one free-form label — the same "real record, honestly
/// narrowed" discipline <c>Hazards.DisasterEvent</c>'s own doc comment already established for §8's
/// <c>affectedPlotIds</c>. §6 names five possible benefits; see <see cref="HostWandererCommands"/> for
/// exactly which two have a real hook in this codebase (Dignitas standing in for Cultural Prestige, and
/// a Physician's real Health recovery) and which three are disclosed as deferred.</para>
/// </summary>
public sealed record WandererEngagement
{
    public required RuntimeId<WandererEngagement> Id { get; init; }
    public required RuntimeId<Wanderer> WandererId { get; init; }
    public required RuntimeId<Household> HouseholdId { get; init; }
    public required WandererEngagementType EngagementType { get; init; }
    public required GameDate OccurredDate { get; init; }

    /// <summary>What the engagement actually drew from the household's own Ledger account
    /// (<see cref="LedgerAccountKey.ForHousehold"/>).</summary>
    public required Money FeePaid { get; init; }

    /// <summary>The Dignitas the household gained — §6's "Cultural Prestige boost" through the real
    /// household-standing primitive that exists. Zero for a Recruit, which pays its benefit in a
    /// permanent household member instead.</summary>
    public required int DignitasGained { get; init; }

    /// <summary>The Fame the Wanderer themselves gained (§4).</summary>
    public required int WandererFameGained { get; init; }

    /// <summary>How many Health points a Physician Host restored to <see cref="BeneficiaryCharacterId"/>
    /// — §6's "a Health recovery," the one other named benefit with a real hook (<see
    /// cref="Condition.Health"/>). Zero for every other type and for a Recruit.</summary>
    public required int HealthRestored { get; init; }

    /// <summary>The Character a Physician Host treated, if any.</summary>
    public RuntimeId<Character>? BeneficiaryCharacterId { get; init; }

    /// <summary>§10's <c>resultingCompanionOrPositionId</c> — "set only when engagementType is
    /// recruit."</summary>
    public RuntimeId<Character>? ResultingCharacterId { get; init; }

    /// <summary>The real Familia duty slot a Recruit landed in, or null when the type has no honest
    /// slot (<see cref="WandererTypeCatalog"/>'s own disclosure) or the recruit's rolled skills fell
    /// short of <c>DutySlotCatalog.MinimumCompetence</c> for it.</summary>
    public DutySlot? ResultingDutySlot { get; init; }

    public static WandererEngagement Create(
        RuntimeId<WandererEngagement> id,
        RuntimeId<Wanderer> wandererId,
        RuntimeId<Household> householdId,
        WandererEngagementType engagementType,
        GameDate occurredDate,
        Money feePaid,
        int dignitasGained = 0,
        int wandererFameGained = 0,
        int healthRestored = 0,
        RuntimeId<Character>? beneficiaryCharacterId = null,
        RuntimeId<Character>? resultingCharacterId = null,
        DutySlot? resultingDutySlot = null)
    {
        if (feePaid.IsNegative)
            throw new ArgumentOutOfRangeException(nameof(feePaid), feePaid, "An engagement fee cannot be negative.");
        if (dignitasGained < 0)
            throw new ArgumentOutOfRangeException(nameof(dignitasGained), dignitasGained, "Dignitas gained cannot be negative.");
        if (wandererFameGained < 0)
            throw new ArgumentOutOfRangeException(nameof(wandererFameGained), wandererFameGained, "Fame gained cannot be negative.");
        if (healthRestored < 0)
            throw new ArgumentOutOfRangeException(nameof(healthRestored), healthRestored, "Health restored cannot be negative.");
        if (engagementType == WandererEngagementType.Recruit && resultingCharacterId is null)
            throw new ArgumentException("A Recruit engagement must name the Character it produced.", nameof(resultingCharacterId));
        if (engagementType == WandererEngagementType.Host && resultingCharacterId is not null)
            throw new ArgumentException("A Host engagement never produces a Character.", nameof(resultingCharacterId));

        return new WandererEngagement
        {
            Id = id,
            WandererId = wandererId,
            HouseholdId = householdId,
            EngagementType = engagementType,
            OccurredDate = occurredDate,
            FeePaid = feePaid,
            DignitasGained = dignitasGained,
            WandererFameGained = wandererFameGained,
            HealthRestored = healthRestored,
            BeneficiaryCharacterId = beneficiaryCharacterId,
            ResultingCharacterId = resultingCharacterId,
            ResultingDutySlot = resultingDutySlot,
        };
    }
}
