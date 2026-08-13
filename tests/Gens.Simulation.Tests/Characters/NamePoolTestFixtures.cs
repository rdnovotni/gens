using Gens.Simulation.Characters;

namespace Gens.Simulation.Tests.Characters;

/// <summary>A small, deterministic sample <see cref="NamePool"/> shared across name-generation tests.</summary>
public static class NamePoolTestFixtures
{
    public static readonly NamePool Roman = new()
    {
        Praenomina = new[] { "Marcus", "Gaius", "Lucius" },
        Nomina = new[] { "Aurelius", "Fabius", "Julius" },
        Cognomina = new[] { "Maximus", "Rufus" },
        GivenNames = new[] { "Bato", "Dagan", "Vercingetorix" },
    };

    public static readonly NamePool NoCognomina = new()
    {
        Praenomina = new[] { "Marcus" },
        Nomina = new[] { "Aurelius" },
        Cognomina = Array.Empty<string>(),
        GivenNames = new[] { "Bato" },
    };
}
