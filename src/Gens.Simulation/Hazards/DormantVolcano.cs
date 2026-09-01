using Gens.Simulation.Identity;
using Gens.Simulation.Land;

namespace Gens.Simulation.Hazards;

/// <summary>§2.2/§8's <c>DormantVolcano</c> — a rare, fixed terrain feature, deliberately kept outside
/// the ordinary <see cref="HazardExposureCalculator"/>/<see cref="NaturalDisasterSystem"/> Exposure
/// roll entirely, per §2.2's own "closer in spirit to Silphium's own rarity treatment than to this
/// document's other eight hazards." §2.2 places it "at map generation," but no map-generation/plot-
/// flagging pass exists anywhere in this codebase yet — this item builds the real record and its own
/// designation command as an explicit, callerless hook instead, the exact "hook now, caller later"
/// discipline <c>Health.AfflictCharacterCommand</c> and <c>Health.EpidemicOutbreak.ImperialScale</c>
/// already established: nothing in this item ever calls <see
/// cref="DesignateDormantVolcanoCommand"/> itself, and no system ever rolls an eruption against a
/// designated plot — nothing north of "a plot can be marked, and the mark persists and round-trips"
/// is real yet. §2.2's own genuinely double-edged aftermath (immediate catastrophe, real long-term Soil
/// Fertility boost) has no real trigger to hang off until whichever future item builds one, and no Soil
/// Fertility track exists yet either way (this namespace's own top-level disclosure) — <see
/// cref="HasErupted"/> and <see cref="PostEruptionFertilityBoostActive"/> are carried here, per §8's
/// own data model, purely so the shape is already correct and additive-only (ADR 0011) once a real
/// eruption mechanic and a real Soil Fertility track both exist.</summary>
public sealed record DormantVolcano
{
    public required RuntimeId<Plot> PlotId { get; init; }
    public required RuntimeId<Settlement> SettlementId { get; init; }
    public bool HasErupted { get; init; }
    public bool PostEruptionFertilityBoostActive { get; init; }

    public static DormantVolcano Create(RuntimeId<Plot> plotId, RuntimeId<Settlement> settlementId) =>
        new() { PlotId = plotId, SettlementId = settlementId };
}
