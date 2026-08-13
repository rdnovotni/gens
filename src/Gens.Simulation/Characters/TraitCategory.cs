namespace Gens.Simulation.Characters;

/// <summary>The three lifecycle-gated trait categories <c>gens-characters-design.md</c> §4 defines —
/// Congenital (rolled at birth), Formative (Childhood/Adolescence), Reactive (Adulthood) — plus
/// <see cref="Lifestyle"/>, <c>gens-traits-design.md</c> §5.3's deliberate timing exception acquired
/// through sustained adult practice rather than locked in by the end of Adolescence.</summary>
public enum TraitCategory
{
    Congenital,
    Formative,
    Reactive,
    Lifestyle,
}

/// <summary>Parses the lowercase, content-authored spelling of a <see cref="TraitCategory"/> (<c>
/// traits.schema.json</c>'s <c>"category"</c> enum values) into the code-defined enum member.</summary>
public static class TraitCategories
{
    public static TraitCategory Parse(string value) => value switch
    {
        "congenital" => TraitCategory.Congenital,
        "formative" => TraitCategory.Formative,
        "reactive" => TraitCategory.Reactive,
        "lifestyle" => TraitCategory.Lifestyle,
        _ => throw new ArgumentException($"'{value}' is not a recognized trait category.", nameof(value)),
    };
}
