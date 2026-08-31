using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.History;

/// <summary>
/// One recorded Divergence (Phase 13 item 5; §6.7, §10's own <c>DivergenceRecord{}</c> sketch): a
/// sufficiently Prominent household's own upward-rippling action has genuinely branched the Historical
/// Timeline from this point forward. A real <see cref="Gens.Simulation.State.WorldState"/> partition —
/// unlike the pure-content <see cref="HistoricalTimelineEntryDefinition"/>/<see
/// cref="NamedHistoricalFigureDefinition"/> catalogs above, a Divergence is a genuine, campaign-specific
/// fact, mirroring <see cref="Gens.Simulation.Correspondence.Letter"/>/<see
/// cref="Gens.Simulation.Travel.TravelTrip"/>'s identical "real record, not content" reasoning. §6.7's
/// own frequency commitment (zero for the large majority of playthroughs, one for a genuinely Prominent
/// one, more only in exceptional cases) is a property of how rarely <see cref="RecordDivergenceCommand"/>
/// is actually submitted and accepted, not anything this record itself needs to enforce.
/// </summary>
/// <param name="Id">Own <see cref="RuntimeId{T}"/>, matching every other genuine runtime record in this
/// codebase.</param>
/// <param name="OccurredDate">When this Divergence was recorded (§10's <c>month</c>).</param>
/// <param name="TriggeringHouseholdId">The Prominent household whose action caused it.</param>
/// <param name="TriggeringAction">A human-readable disclosure of what caused it — this item builds no
/// real Prominence-severity-threshold system to compute a trigger from yet (§11's own open "Divergence's
/// exact severity threshold" question), so a caller (a future Prominence/Doctrine/civil-war-allegiance
/// system) supplies this as a plain description, matching how Phase 13 item 3 left interception
/// detection mechanics explicitly unresolved rather than fabricated.</param>
/// <param name="AffectedTimelineEntryIds">Every real Timeline entry this Divergence pulls off the real
/// historical roster (§6.7's "stops drawing on the real historical roster"), always <see
/// cref="HistoricalTimelineEntryDefinition.DivergenceEligible"/> ones dated on or after <see
/// cref="OccurredDate"/> (validated by <see cref="RecordDivergenceCommand"/> — "immutable history" from
/// the other direction: a real date already passed in this campaign cannot retroactively branch).</param>
/// <param name="NewAlternateHistoryBranchActive">Always <c>true</c> once recorded (§10's own field) —
/// there is no path back to the real historical roster once Diverged (§6.7).</param>
/// <remarks>Deliberately omits §10's own <c>chronicleEntryTier</c> field: §6.7 fixes it at "always
/// maximum tier" with no variation to actually store, so <see
/// cref="Chronicle.ChronicleProjector"/> emits the real <see
/// cref="Chronicle.ChronicleTier.Legendary"/> Dynasty Chronicle entry directly off <see
/// cref="DivergenceRecordedEvent"/> rather than persisting a constant.</remarks>
public sealed record DivergenceRecord(
    RuntimeId<DivergenceRecord> Id,
    GameDate OccurredDate,
    RuntimeId<Household> TriggeringHouseholdId,
    string TriggeringAction,
    IReadOnlyList<DefinitionId<HistoricalTimelineEntryDefinition>> AffectedTimelineEntryIds,
    bool NewAlternateHistoryBranchActive);
