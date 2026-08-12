namespace Gens.Simulation.Characters;

/// <summary>Shared 0–100 bounds check for <see cref="CoreAttributes"/>, <see cref="LaborSkills"/>, and
/// <see cref="Condition"/> components (<c>gens-familia-design.md</c> §2.1–2.3's common "numeric
/// 0–100" range).</summary>
internal static class StatRange
{
    public static int Validate(int value, string paramName)
    {
        if (value is < 0 or > 100)
            throw new ArgumentOutOfRangeException(paramName, value, $"'{paramName}' must be between 0 and 100.");
        return value;
    }
}
