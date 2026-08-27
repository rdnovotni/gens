using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Time;

namespace Gens.Simulation.Campaign;

/// <summary>
/// Constructs a fresh <see cref="WorldState"/> and its <see cref="RandomStreamSet"/> from a validated
/// <see cref="CampaignConfig"/> (Phase 4 item 2). The exit gate calls this an "empty campaign" for a
/// reason: no real Region/Settlement/Household record types exist yet (those land in Phase 5/6), so
/// bootstrap only issues the IDs those later phases will attach real data to, plus one
/// <see cref="CampaignBootstrappedEvent"/> recording that this is where the campaign's history
/// starts.
/// </summary>
public static class CampaignBootstrapper
{
    /// <summary>The named random stream every non-system-specific campaign-level draw uses.</summary>
    public const string CampaignStreamName = "campaign";

    /// <summary>The named random stream <see cref="ScheduledActionSystem"/> reserves for itself, so a
    /// scheduled-action-ordering change can never perturb the campaign stream's draws (rule 8).</summary>
    public const string ScheduledActionStreamName = "scheduled-actions";

    /// <summary>The named random stream <see cref="Characters.CharacterLifecycleSystem"/> reserves for
    /// its monthly mortality roll, so a change to how any other system draws never perturbs the
    /// mortality curve's draws, and vice versa (rule 8).</summary>
    public const string CharacterMortalityStreamName = "characters.mortality";

    /// <summary>The named random stream Character generation (<see cref="Characters.BirthCharacterCommand"/>'s
    /// name/appearance/baseline-Condition rolls) reserves for itself, kept distinct from <see
    /// cref="CharacterMortalityStreamName"/> for the same reason (rule 8).</summary>
    public const string CharacterGenerationStreamName = "characters.generation";

    /// <summary>The named random stream <see cref="Characters.PromoteToNamedCommand"/>'s age/identity/
    /// attribute backfill rolls reserve for themselves, kept distinct from <see
    /// cref="CharacterGenerationStreamName"/> for the same rule-8 reason: promotion and birth are
    /// different systems whose draw sequences shouldn't perturb each other.</summary>
    public const string CharacterPromotionStreamName = "characters.promotion";

    /// <summary>The named random stream <see cref="Characters.LaborFlightSystem"/> reserves for its
    /// monthly flight-opportunity roll (Phase 6 item 6), kept distinct from every other stream here for
    /// the same rule-8 reason.</summary>
    public const string CharacterLaborFlightStreamName = "characters.laborFlight";

    /// <summary>The named random stream <see cref="Characters.LaborFlightSystem"/> reserves for its
    /// pursuit-outcome roll (Phase 6 item 6), kept distinct from <see cref="CharacterLaborFlightStreamName"/>
    /// for the same rule-8 reason.</summary>
    public const string CharacterPursuitOutcomeStreamName = "characters.pursuitOutcome";

    /// <summary>The named random stream <see cref="Characters.GrowthMortalitySystem"/> reserves for its
    /// monthly rounding roll (Phase 7 item 4), kept distinct from every other stream here for the same
    /// rule-8 reason.</summary>
    public const string PopGroupGrowthMortalityStreamName = "characters.popGroupGrowthMortality";

    /// <summary>The named random stream <see cref="Characters.MigrationSystem"/> reserves for its
    /// monthly emigration rounding roll (Phase 7 item 4), kept distinct from every other stream here
    /// for the same rule-8 reason.</summary>
    public const string PopGroupEmigrationStreamName = "characters.popGroupEmigration";

    /// <summary>The named random stream <see cref="Characters.MigrationSystem"/> reserves for its
    /// monthly immigration rounding roll (Phase 7 item 4), kept distinct from <see
    /// cref="PopGroupEmigrationStreamName"/> for the same rule-8 reason.</summary>
    public const string PopGroupImmigrationStreamName = "characters.popGroupImmigration";

    /// <summary>The named random stream <see cref="Events.EventPoolSystem"/> reserves for its monthly
    /// weighted-pool draws (Phase 9 item 3), kept distinct from every other stream here for the same
    /// rule-8 reason.</summary>
    public const string EventPoolStreamName = "events.pool";

    /// <summary>The named random stream <see cref="Actors.LivingWorldActorHeadGenerator"/> reserves for
    /// lazily generating a rival house's head Character (Phase 10 item 4), kept distinct from <see
    /// cref="CharacterGenerationStreamName"/> for the same rule-8 reason even though both draw a full
    /// Character identity — a rival house's generation must not perturb, or be perturbed by, the
    /// player household's own.</summary>
    public const string RivalHouseHeadGenerationStreamName = "actors.rivalHouseHeadGeneration";

    /// <summary>The named random stream <see cref="Actors.BackgroundHouseDriftSystem"/> reserves for
    /// its monthly Background-tier fortune/standing-trend rolls (Phase 10 item 3), kept distinct from
    /// <see cref="RivalHouseHeadGenerationStreamName"/> for the same rule-8 reason.</summary>
    public const string BackgroundHouseDriftStreamName = "actors.backgroundHouseDrift";

    /// <summary>The named random stream <see cref="Actors.RivalAmbitionSystem"/> reserves for its
    /// monthly Noteworthy-tier act-chance roll (Phase 10 item 4), kept distinct from every other stream
    /// here for the same rule-8 reason.</summary>
    public const string RivalAmbitionStreamName = "actors.rivalAmbition";

    /// <summary>The named random stream <see cref="Actors.LivingWorldActorExtinctionSystem"/> reserves
    /// for its monthly Background-tier extinction roll (Phase 10 item 4), kept distinct from every
    /// other stream here for the same rule-8 reason — the Noteworthy-tier half of that system's check is
    /// a deterministic genealogy lookup and draws no random numbers at all.</summary>
    public const string ActorExtinctionStreamName = "actors.extinction";

    /// <summary>The named random stream <see cref="Interactions.SchemeProgressSystem"/> reserves for
    /// its monthly resolution rolls (counter-play foil-vs-escalate, clean success-vs-quiet-failure —
    /// Phase 10 item 6), kept distinct from every other stream here for the same rule-8 reason. Progress
    /// and discovery-risk advancement themselves are deterministic formulas and draw no random numbers.</summary>
    public const string SchemeProgressStreamName = "interactions.schemeProgress";

    /// <summary>The named random stream <see cref="Stewardship.StewardAutonomousDecisionSystem"/>
    /// reserves for its monthly steward/Council Loyalty-risk and incident-type rolls (Phase 10 package
    /// 13; <c>gens-steward-council-auto-management-design.md</c> §6), kept distinct from every other
    /// stream here for the same rule-8 reason — competence itself is a deterministic stat readout and
    /// draws no random numbers.</summary>
    public const string StewardLoyaltyRiskStreamName = "stewardship.loyaltyRisk";

    /// <summary>The named random stream <see cref="SuccessionHandoffSystem"/> reserves for its monthly
    /// succession-drama trigger roll (Phase 11 item 1), kept distinct from every other stream here for
    /// the same rule-8 reason.</summary>
    public const string SuccessionDisputeTriggerStreamName = SuccessionHandoffSystem.DisputeTriggerStreamName;

    /// <summary>The named random stream <see cref="SuccessionDisputeResolutionSystem"/> reserves for
    /// its per-claimant scoring tiebreak (Phase 11 item 1), kept distinct from every other stream here
    /// for the same rule-8 reason.</summary>
    public const string SuccessionDisputeScoringStreamName = SuccessionDisputeResolutionSystem.ScoringStreamName;

    /// <summary>The named random stream <see cref="SuccessionDisputeResolutionSystem"/> reserves for
    /// its runner-up splinter-house roll (Phase 11 item 1), kept distinct from <see
    /// cref="SuccessionDisputeScoringStreamName"/> for the same rule-8 reason.</summary>
    public const string SuccessionDisputeSplinterStreamName = SuccessionDisputeResolutionSystem.SplinterStreamName;

    public static BootstrappedCampaign Bootstrap(CampaignConfig config)
    {
        if (config is null)
            throw new ArgumentNullException(nameof(config));
        config.Validate();

        var state = new WorldState(config.StartDate);

        var streams = new RandomStreamSet();
        streams.AddDerived(CampaignStreamName, config.Seed);
        streams.AddDerived(ScheduledActionStreamName, config.Seed);
        streams.AddDerived(CharacterMortalityStreamName, config.Seed);
        streams.AddDerived(CharacterGenerationStreamName, config.Seed);
        streams.AddDerived(CharacterPromotionStreamName, config.Seed);
        streams.AddDerived(CharacterLaborFlightStreamName, config.Seed);
        streams.AddDerived(CharacterPursuitOutcomeStreamName, config.Seed);
        streams.AddDerived(PopGroupGrowthMortalityStreamName, config.Seed);
        streams.AddDerived(PopGroupEmigrationStreamName, config.Seed);
        streams.AddDerived(PopGroupImmigrationStreamName, config.Seed);
        streams.AddDerived(EventPoolStreamName, config.Seed);
        streams.AddDerived(RivalHouseHeadGenerationStreamName, config.Seed);
        streams.AddDerived(BackgroundHouseDriftStreamName, config.Seed);
        streams.AddDerived(RivalAmbitionStreamName, config.Seed);
        streams.AddDerived(ActorExtinctionStreamName, config.Seed);
        streams.AddDerived(SchemeProgressStreamName, config.Seed);
        streams.AddDerived(StewardLoyaltyRiskStreamName, config.Seed);
        streams.AddDerived(SuccessionDisputeTriggerStreamName, config.Seed);
        streams.AddDerived(SuccessionDisputeScoringStreamName, config.Seed);
        streams.AddDerived(SuccessionDisputeSplinterStreamName, config.Seed);

        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        var householdId = state.HouseholdIds.Issue();

        var bootstrapped = new CampaignBootstrappedEvent(
            state.EventIds.Issue(),
            config.StartDate,
            regionId,
            settlementId,
            householdId);

        return new BootstrappedCampaign(state, streams, new IDomainEvent[] { bootstrapped }, regionId, settlementId, householdId);
    }
}

/// <summary>The result of bootstrapping a new campaign: the constructed <see cref="State.WorldState"/>
/// and its <see cref="Random.RandomStreamSet"/>, ready to be saved via <see
/// cref="Saves.SaveWriter"/>, plus the "initial history" (Phase 4 item 2) the console runner should
/// surface to the player before the first tick ever runs.</summary>
public sealed record BootstrappedCampaign(
    WorldState State,
    RandomStreamSet RandomStreams,
    IReadOnlyList<IDomainEvent> InitialHistory,
    RuntimeId<Region> RegionId,
    RuntimeId<Settlement> SettlementId,
    RuntimeId<Household> HouseholdId);

/// <summary>Marks the tick a campaign was bootstrapped at and which region/settlement/household IDs
/// it started with. Every later "who was here from the beginning" question reads this event rather
/// than a bespoke campaign-metadata field.</summary>
public sealed record CampaignBootstrappedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Region> RegionId,
    RuntimeId<Settlement> SettlementId,
    RuntimeId<Household> HouseholdId) : IDomainEvent
{
    public string Type => "campaign.bootstrapped";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { RegionId.ToTaggedString(), SettlementId.ToTaggedString(), HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}
