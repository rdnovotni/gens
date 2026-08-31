namespace Gens.Simulation.Regions;

/// <summary>A light, three-step read of how significant a <see cref="GazetteerLocationDefinition"/> is
/// (<c>gens-starting-regions-design.md</c> §8.2) — deliberately not a numeric population or wealth
/// figure, per this project's standing no-numeric-sizing convention for content.</summary>
public enum ProminenceTier
{
    Outpost,
    RegionalCenter,
    ProvincialSeat,
}
