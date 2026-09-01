namespace Gens.Simulation.Hazards;

/// <summary>§5.1's four Disaster Event severity tiers, in ascending order of consequence — <see
/// cref="DisasterSeverityCalculator.RollSeverity"/> weights a low-<see
/// cref="HazardExposureCalculator"/>-Exposure settlement away from the top two tiers, per §5.1's own
/// "a low-Exposure household is both less likely to suffer an Event at all and weighted away from the
/// worst outcomes when one does land."</summary>
public enum DisasterSeverity
{
    Minor,
    Moderate,
    Severe,
    Catastrophic,
}
