using System.Globalization;

namespace Gens.Simulation.Identity;

/// <summary>
/// A campaign-scoped, monotonically increasing runtime identifier for entity kind
/// <typeparamref name="T"/> (a phantom type keeping, e.g., a Character ID from being passed where a
/// Plot ID is expected). Issued by <see cref="RuntimeIdCounter{T}"/>, never a GUID and never derived
/// from wall-clock time, so two runs of the same seed and command log issue identical IDs in
/// identical order (ADR 0001). Because the issuing counter is strictly increasing, ascending
/// <see cref="RuntimeId{T}"/> order is equivalent to creation order (ADR 0004).
/// </summary>
public readonly record struct RuntimeId<T> : IComparable<RuntimeId<T>>
{
    internal RuntimeId(long value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "A runtime ID cannot be negative.");
        Value = value;
    }

    public long Value { get; }

    public int CompareTo(RuntimeId<T> other) => Value.CompareTo(other.Value);

    public static bool operator <(RuntimeId<T> a, RuntimeId<T> b) => a.Value < b.Value;
    public static bool operator >(RuntimeId<T> a, RuntimeId<T> b) => a.Value > b.Value;
    public static bool operator <=(RuntimeId<T> a, RuntimeId<T> b) => a.Value <= b.Value;
    public static bool operator >=(RuntimeId<T> a, RuntimeId<T> b) => a.Value >= b.Value;

    /// <summary>The save-file cross-reference form, e.g. <c>char_0000042</c> (ADR 0001).</summary>
    public string ToTaggedString() =>
        $"{RuntimeIdTag<T>.Tag}_{Value.ToString("D7", CultureInfo.InvariantCulture)}";

    public override string ToString() => ToTaggedString();

    public static RuntimeId<T> Parse(string taggedString)
    {
        if (string.IsNullOrEmpty(taggedString))
            throw new ArgumentException("A tagged runtime ID string is required.", nameof(taggedString));

        var expectedPrefix = RuntimeIdTag<T>.Tag + "_";
        if (!taggedString.StartsWith(expectedPrefix, StringComparison.Ordinal))
            throw new FormatException($"'{taggedString}' is not a valid {RuntimeIdTag<T>.Tag} runtime ID.");

        var numericPart = taggedString.Substring(expectedPrefix.Length);
        if (!long.TryParse(numericPart, NumberStyles.None, CultureInfo.InvariantCulture, out var value))
            throw new FormatException($"'{taggedString}' is not a valid {RuntimeIdTag<T>.Tag} runtime ID.");

        return new RuntimeId<T>(value);
    }
}

/// <summary>Per-entity-kind counter. Itself campaign state: saved, restored, monotonic within a campaign.</summary>
public sealed class RuntimeIdCounter<T>
{
    private long _next;

    public RuntimeIdCounter()
        : this(0)
    {
    }

    private RuntimeIdCounter(long next) => _next = next;

    /// <summary>The next value this counter will issue. Persist and restore this for save compatibility.</summary>
    public long Peek => _next;

    public RuntimeId<T> Issue()
    {
        var id = new RuntimeId<T>(checked(_next));
        _next = checked(_next + 1);
        return id;
    }

    public static RuntimeIdCounter<T> Restore(long next)
    {
        if (next < 0)
            throw new ArgumentOutOfRangeException(nameof(next), "A restored counter cannot be negative.");
        return new RuntimeIdCounter<T>(next);
    }
}
