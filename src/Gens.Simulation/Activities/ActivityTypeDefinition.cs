using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;

namespace Gens.Simulation.Activities;

/// <summary>One weighted Quality input an Activity Type calls for (§5.2): "an Activity's Quality reads
/// whatever inputs its specific Type actually calls for".</summary>
public readonly record struct ActivityQualityInputDefinition(string Key, int Weight);

/// <summary>
/// §2's pluggable Type slot: the configuration a specific Activity Type (Feast, Hunt, Wedding,
/// Symposium...) supplies to the shared engine — its duration mode (§3), its Phase sequence (§6, the
/// generic Reception → Main Event → Aftermath default or its own override), the Venue families it may
/// use (§2), and its Quality inputs (§5.2). The engine never switches on a specific Type; every Type is
/// "simply a specific configuration of the six real slots", per §1.
/// </summary>
public sealed record ActivityTypeDefinition
{
    public ActivityTypeDefinition(
        string key,
        ActivityDurationMode durationMode,
        IReadOnlyList<string> phaseKeys,
        IReadOnlyList<ActivityVenueKind> allowedVenueKinds,
        IReadOnlyList<ActivityQualityInputDefinition> qualityInputs,
        int minimumMonths = 1,
        int maximumMonths = 1)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("An Activity Type requires a key.", nameof(key));
        if (phaseKeys is null || phaseKeys.Count == 0)
            throw new ArgumentException("An Activity Type requires at least one Phase.", nameof(phaseKeys));
        if (phaseKeys.Distinct(StringComparer.Ordinal).Count() != phaseKeys.Count)
            throw new ArgumentException("Phase keys must be unique within a Type.", nameof(phaseKeys));
        if (allowedVenueKinds is null || allowedVenueKinds.Count == 0)
            throw new ArgumentException("An Activity Type requires at least one Venue kind.", nameof(allowedVenueKinds));
        if (qualityInputs is null || qualityInputs.Count == 0 || qualityInputs.Any(input => input.Weight <= 0))
            throw new ArgumentException("An Activity Type requires at least one positively-weighted Quality input.", nameof(qualityInputs));
        if (durationMode == ActivityDurationMode.Quick && (minimumMonths != 1 || maximumMonths != 1))
            throw new ArgumentException("A Quick Activity always occupies exactly one month.", nameof(minimumMonths));
        if (durationMode == ActivityDurationMode.Extended && (minimumMonths < 2 || maximumMonths < minimumMonths))
            throw new ArgumentException("An Extended Activity spans at least two months.", nameof(minimumMonths));

        Key = key;
        DurationMode = durationMode;
        PhaseKeys = phaseKeys;
        AllowedVenueKinds = allowedVenueKinds;
        QualityInputs = qualityInputs;
        MinimumMonths = minimumMonths;
        MaximumMonths = maximumMonths;
    }

    public string Key { get; }
    public ActivityDurationMode DurationMode { get; }
    public IReadOnlyList<string> PhaseKeys { get; }
    public IReadOnlyList<ActivityVenueKind> AllowedVenueKinds { get; }
    public IReadOnlyList<ActivityQualityInputDefinition> QualityInputs { get; }
    public int MinimumMonths { get; }
    public int MaximumMonths { get; }
}

/// <summary>§6's generic default Phase sequence, usable as-is by any Type.</summary>
public static class ActivityPhaseKeys
{
    public const string Reception = "reception";
    public const string MainEvent = "mainEvent";
    public const string Aftermath = "aftermath";

    public static readonly IReadOnlyList<string> Default = new[] { Reception, MainEvent, Aftermath };
}

/// <summary>
/// The code-defined Activity Type catalog. Per the design doc's own direction ("this pass builds the
/// engine only; the specific Activity Types are deliberately left to their own future design passes"),
/// this item ships only two generic, Type-agnostic configurations — one per §3 duration mode — which
/// exercise every engine slot end to end. Feasts (Phase 17 item 5), Games &amp; Spectacle (item 6),
/// and any later Type add their own entries here rather than a parallel gathering system.
/// </summary>
public static class ActivityTypeCatalog
{
    /// <summary>Generic Quality inputs: provisioning (food/drink/equipment), hospitality (service and
    /// comfort), and entertainment — deliberately Food Culture's three-input Banquet Quality shape (§5.2).</summary>
    public const string ProvisioningInput = "provisioning";
    public const string HospitalityInput = "hospitality";
    public const string EntertainmentInput = "entertainment";

    private static readonly ActivityQualityInputDefinition[] GenericInputs =
    {
        new(ProvisioningInput, 2),
        new(HospitalityInput, 1),
        new(EntertainmentInput, 1),
    };

    /// <summary>A generic Quick gathering — an evening's reception at a Villa room, civic space, or
    /// outdoors, with the default Reception → Main Event → Aftermath sequence.</summary>
    public static readonly ActivityTypeDefinition Gathering = new(
        "gathering",
        ActivityDurationMode.Quick,
        ActivityPhaseKeys.Default,
        new[] { ActivityVenueKind.VillaRoom, ActivityVenueKind.CivicSpace, ActivityVenueKind.Outdoor },
        GenericInputs);

    /// <summary>A generic Extended gathering (a multi-month celebration or expedition) spanning two to
    /// six months, the same default Phases spread across that real duration.</summary>
    public static readonly ActivityTypeDefinition ExtendedGathering = new(
        "extendedGathering",
        ActivityDurationMode.Extended,
        ActivityPhaseKeys.Default,
        new[] { ActivityVenueKind.VillaRoom, ActivityVenueKind.CivicSpace, ActivityVenueKind.Outdoor },
        GenericInputs,
        minimumMonths: 2,
        maximumMonths: 6);

    public static readonly IReadOnlyList<ActivityTypeDefinition> All = new[] { Gathering, ExtendedGathering };

    public static bool TryGet(string key, out ActivityTypeDefinition definition)
    {
        foreach (var candidate in All)
        {
            if (string.Equals(candidate.Key, key, StringComparison.Ordinal))
            {
                definition = candidate;
                return true;
            }
        }

        definition = null!;
        return false;
    }

    public static ActivityTypeDefinition Get(string key) =>
        TryGet(key, out var definition)
            ? definition
            : throw new InvalidOperationException($"Unknown Activity Type '{key}'.");
}
