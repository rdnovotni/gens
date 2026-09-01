namespace Gens.Simulation.Hazards;

/// <summary>Turns a live <see cref="HazardExposureProfile.ExposureFor"/> reading into §3's own "warnings
/// where appropriate" exit-gate language (Phase 14's own exit gate line) — the real surfacing item 3's
/// own doc comment named as its own "forecast/knowledge" scope item's whole realization (a readable
/// number) still left undelivered as an actual player-facing signal. <see cref="IsElevated"/> is this
/// implementation's own invented threshold (the same "§9's All numeric sizing" open question every other
/// figure in this namespace already cites): chosen high enough that only a settlement genuinely likely to
/// suffer a Disaster Event soon crosses it, not merely any nonzero Exposure.</summary>
public static class HazardWarningCalculator
{
    private const int ElevatedExposureThreshold = 65;

    public static bool IsElevated(int exposureScore) => exposureScore >= ElevatedExposureThreshold;
}
