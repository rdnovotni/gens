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
/// a genuine Empire-wide event" flag — item 2 built it as an explicit, callerless hook exactly like
/// item 1's own <c>AfflictCharacterCommand</c>, and item 5's <see cref="AntoninePlagueEra"/> is the real
/// caller now: <see cref="EpidemicContagionSystem"/> stamps it true on any Pestilence outbreak it ignites
/// while <see cref="AntoninePlagueEra.IsActive"/> holds, and <see
/// cref="QuarantineEffectCalculator.SettlementSpreadMultiplier"/> honors it exactly as already
/// written.</summary>
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
        GameDate startDate,
        bool imperialScale = false) =>
        new()
        {
            Id = id,
            SettlementId = settlementId,
            ConditionId = conditionId,
            StartDate = startDate,
            Status = EpidemicOutbreakStatus.Active,
            ImperialScale = imperialScale,
        };
}
