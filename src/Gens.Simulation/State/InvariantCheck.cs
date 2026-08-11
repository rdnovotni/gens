namespace Gens.Simulation.State;

/// <summary>One invariant failure: which invariant, a human-readable message, and the subjects involved.</summary>
public readonly record struct InvariantViolation(string InvariantId, string Message, IReadOnlyList<string> SubjectIds);

/// <summary>A pure, read-only consistency check run against <see cref="WorldState"/>, typically in
/// the <see cref="Time.TickPhase.InvariantChecks"/> phase (ADR 0005).</summary>
public interface IInvariantCheck
{
    string Id { get; }

    /// <summary>If true, any violation this check reports aborts the tick via
    /// <see cref="InvariantViolationException"/> instead of only being recorded.</summary>
    bool IsFatal { get; }

    /// <summary>Violations found, in a fixed, deterministic (ADR 0004) order.</summary>
    IEnumerable<InvariantViolation> Check(WorldState state);
}

/// <summary>Runs every registered <see cref="IInvariantCheck"/> in ordinal-ID order (ADR 0004), so
/// the same <see cref="WorldState"/> always produces the same ordered violation list regardless of
/// registration order.</summary>
public sealed class InvariantCheckRunner
{
    private readonly IReadOnlyList<IInvariantCheck> _checks;

    public InvariantCheckRunner(IEnumerable<IInvariantCheck> checks) =>
        _checks = checks.OrderBy(static check => check.Id, StringComparer.Ordinal).ToArray();

    public IReadOnlyList<InvariantViolation> RunAll(WorldState state)
    {
        var violations = new List<InvariantViolation>();
        foreach (var check in _checks)
        {
            var checkViolations = check.Check(state).ToArray();
            violations.AddRange(checkViolations);
            if (check.IsFatal && checkViolations.Length > 0)
                throw new InvariantViolationException(check.Id, checkViolations);
        }

        return violations;
    }
}

public sealed class InvariantViolationException : Exception
{
    public InvariantViolationException(string invariantId, IReadOnlyList<InvariantViolation> violations)
        : base($"Invariant '{invariantId}' was violated: {string.Join("; ", violations.Select(static v => v.Message))}")
    {
        InvariantId = invariantId;
        Violations = violations;
    }

    public string InvariantId { get; }

    public IReadOnlyList<InvariantViolation> Violations { get; }
}
