using System.Text.RegularExpressions;

namespace Gens.Simulation.Identity;

/// <summary>
/// A content-authored identifier for entity kind <typeparamref name="T"/> (a phantom type — never
/// instantiated, only used to keep, e.g., a Good's ID from being passed where a Building's ID is
/// expected). Values are validated, non-empty, ASCII kebab-case strings such as <c>"grain"</c> or
/// <c>"iron-ore"</c>, authored in content and never runtime-generated (ADR 0001).
/// </summary>
public readonly record struct DefinitionId<T>
{
    private static readonly Regex KebabCasePattern = new("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);

    public DefinitionId(string value)
    {
        if (string.IsNullOrEmpty(value) || !KebabCasePattern.IsMatch(value))
            throw new ArgumentException(
                $"'{value}' is not a valid definition ID; expected non-empty, lowercase, ASCII kebab-case.",
                nameof(value));

        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}
