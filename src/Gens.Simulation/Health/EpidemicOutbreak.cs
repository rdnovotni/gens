using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Time;

namespace Gens.Simulation.Health;

/// <summary>One settlement's standing outbreak of one epidemic disease — §11's <c>EpidemicOutbreak</c>
/// data model, scoped to the fields this codebase can actually maintain: <see cref="SettlementId"/>/
/// <see cref="ConditionId"/>/<see cref="StartDate"/>/<see cref="Status"/> track the outbreak itself
/// (the per-Character <c>infectedCharacterIds</c>/<c>quarantinedCharacterIds</c> lists §11 sketches are
/// deliberately not duplicated here — <see cref="HealthQueries.ActiveConditionsFor"/> plus a settlement
/// scan already answers "who's infected right now" without a second, driftable copy of the same
/// membership <see cref="Health.CharacterHealthCondition"/> already owns). <see
/// cref="SettlementQuarantineActive"/> is §4.2's settlement-wide Quarantine toggle, folded into this
/// record rather than a separate partition since it is only ever meaningful against a specific standing
/// outbreak. <see cref="ImperialScale"/> is §4.3's "quarantine is meaningfully less effective... during
/// a genuine Empire-wide event" flag — an explicit, callerless hook exactly like item 1's own
/// <c>AfflictCharacterCommand</c>: nothing in this item ever sets it true (the Antonine Plague's own
/// Event Chain trigger is item 3/5's job per §9/§10), but <see
/// cref="QuarantineEffectCalculator.SettlementSpreadMultiplier"/> already honors it once something
/// does.</summary>
public sealed record EpidemicOutbreak
{
    public required RuntimeId<EpidemicOutbreak> Id { get; init; }
    public required RuntimeId<Settlement> SettlementId { get; init; }
    public required DefinitionId<HealthConditionDefinition> ConditionId { get; init; }
    public required GameDate StartDate { get; init; }
    public required EpidemicOutbreakStatus Status { get; init; }
    public bool SettlementQuarantineActive { get; init; }
    public bool ImperialScale { get; init; }
    public GameDate? ResolvedDate { get; init; }

    public static EpidemicOutbreak Create(
        RuntimeId<EpidemicOutbreak> id,
        RuntimeId<Settlement> settlementId,
        DefinitionId<HealthConditionDefinition> conditionId,
        GameDate startDate) =>
        new()
        {
            Id = id,
            SettlementId = settlementId,
            ConditionId = conditionId,
            StartDate = startDate,
            Status = EpidemicOutbreakStatus.Active,
        };
}
