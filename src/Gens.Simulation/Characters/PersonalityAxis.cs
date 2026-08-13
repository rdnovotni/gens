namespace Gens.Simulation.Characters;

/// <summary>The seven hidden behavioral axes <c>gens-characters-design.md</c> §5 defines: "seven
/// numeric axes (-100 to 100, 0 neutral), hidden from ordinary UI... that every trait nudges by a
/// small weighted amount and that all mechanical resolution reads uniformly." Fixed and code-defined
/// rather than content-authored — the axis set itself is a rules concept; only which traits nudge
/// which axis, and by how much, is content (<see cref="TraitDefinition"/>). Each member is named for
/// the axis itself (§5's left-hand column, e.g. <c>Greed</c>, <c>Zealotry</c>), not either of its two
/// named poles; a trait's positive nudge moves toward that axis's first-listed pole, a negative nudge
/// toward its second (e.g. Honor: Honorable ↔ Treacherous, so a positive Honor nudge is more
/// honorable, a negative one more treacherous).</summary>
public enum PersonalityAxis
{
    Honor,
    Compassion,
    Greed,
    Zealotry,
    Vengefulness,
    Boldness,
    Rationality,
}

/// <summary>Parses the lowercase, content-authored spelling of a <see cref="PersonalityAxis"/> (e.g.
/// <c>traits.schema.json</c>'s <c>"axis"</c> enum values) into the code-defined enum member.</summary>
public static class PersonalityAxes
{
    public static PersonalityAxis Parse(string value) => value switch
    {
        "honor" => PersonalityAxis.Honor,
        "compassion" => PersonalityAxis.Compassion,
        "greed" => PersonalityAxis.Greed,
        "zealotry" => PersonalityAxis.Zealotry,
        "vengefulness" => PersonalityAxis.Vengefulness,
        "boldness" => PersonalityAxis.Boldness,
        "rationality" => PersonalityAxis.Rationality,
        _ => throw new ArgumentException($"'{value}' is not a recognized personality axis.", nameof(value)),
    };

    /// <summary>The valid range for a Character's accumulated score on any one axis
    /// (<c>gens-characters-design.md</c> §5: "-100 to 100, 0 neutral").</summary>
    public const int MinScore = -100;
    public const int MaxScore = 100;
}
