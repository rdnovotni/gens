using Gens.Simulation.Time;

namespace Gens.Simulation.Campaign;

/// <summary>
/// Everything <see cref="CampaignBootstrapper"/> needs to construct a new campaign (Phase 4 item 1):
/// the root random seed, the starting date, which ruleset and compiled content the campaign is
/// authored against, difficulty/content toggles, and which region/start profile the household begins
/// in. This is the config surface, not the running campaign — it is small enough to be passed on a
/// console runner's command line or embedded verbatim at the top of a save's provenance record.
/// </summary>
public sealed record CampaignConfig
{
    public required ulong Seed { get; init; }

    public required GameDate StartDate { get; init; }

    /// <summary>Which rules variant this campaign runs under (e.g. house-rule toggles bundled under a
    /// named ruleset). No ruleset content exists yet — this is a stable identifier future systems can
    /// key off, not a validated reference today.</summary>
    public required string RulesetId { get; init; }

    /// <summary>The compiled content pack this campaign is authored against (ADR 0012). A single hash
    /// today, not a list: the content compiler produces one compiled pack per invocation, so a
    /// campaign is currently bootstrapped from exactly one. Multi-pack/mod composition is deferred
    /// until a real use case demands it.</summary>
    public required string ContentPackHash { get; init; }

    /// <summary>The content-authored region the household starts in (a <c>regions</c> family
    /// definition ID). The bootstrap does not itself validate this against a compiled pack — the
    /// caller (e.g. the console runner, which already has the pack loaded) is expected to check it
    /// resolves before bootstrapping.</summary>
    public required string RegionId { get; init; }

    /// <summary>Reserved for a future "start profile" content family (household composition, starting
    /// wealth, etc.) that does not exist yet. <c>null</c> means "use the region's bare defaults."</summary>
    public string? StartProfileId { get; init; }

    public required string Difficulty { get; init; }

    /// <summary>Accessibility/content toggles (e.g. content-maturity filters), by stable ID. Empty by
    /// default.</summary>
    public IReadOnlyList<string> ContentToggles { get; init; } = Array.Empty<string>();

    /// <summary>Throws if any required identifier is missing or blank. Bootstrap-time validation only
    /// — this does not check that <see cref="RegionId"/>/<see cref="ContentPackHash"/> resolve against
    /// any real compiled content.</summary>
    public void Validate()
    {
        RequireNonBlank(RulesetId, nameof(RulesetId));
        RequireNonBlank(ContentPackHash, nameof(ContentPackHash));
        RequireNonBlank(RegionId, nameof(RegionId));
        RequireNonBlank(Difficulty, nameof(Difficulty));
    }

    private static void RequireNonBlank(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"CampaignConfig.{fieldName} is required.", fieldName);
    }
}
