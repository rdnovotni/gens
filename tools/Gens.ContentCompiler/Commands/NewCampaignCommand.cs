using System.Text.Json;
using Gens.Simulation.Campaign;
using Gens.Simulation.Saves;
using Gens.Simulation.Time;

namespace Gens.ContentCompiler.Commands;

/// <summary>
/// <c>new-campaign --seed &lt;n&gt; [--start-months &lt;n&gt;] --region &lt;id&gt; --content
/// &lt;compiledPackPath&gt; --out &lt;path&gt; [--start-profile &lt;id&gt;] [--ruleset &lt;id&gt;]
/// [--difficulty &lt;id&gt;]</c>: bootstraps a new campaign (Phase 4 item 2) against an
/// already-compiled content pack (the output of <c>compile</c>), validates the requested region
/// resolves against that pack, saves the result, and prints the campaign's initial history.
/// <c>--start-months</c> is <see cref="GameDate.TotalMonths"/> directly (0 = the epoch, 754 BCE);
/// it defaults to 0.
/// </summary>
public static class NewCampaignCommand
{
    public static int Run(
        ulong seed,
        int startMonths,
        string regionId,
        string contentPackPath,
        string outPath,
        string? startProfileId,
        string ruleset,
        string difficulty)
    {
        var packBytes = File.ReadAllBytes(contentPackPath);
        var contentHash = CanonicalJson.Sha256Hex(packBytes);

        if (!RegionExists(packBytes, regionId))
        {
            Console.Error.WriteLine($"Region '{regionId}' was not found in compiled pack '{contentPackPath}'.");
            return 1;
        }

        var config = new CampaignConfig
        {
            Seed = seed,
            StartDate = new GameDate(startMonths),
            RulesetId = ruleset,
            ContentPackHash = contentHash,
            RegionId = regionId,
            StartProfileId = startProfileId,
            Difficulty = difficulty,
        };

        var campaign = CampaignBootstrapper.Bootstrap(config);
        SaveWriter.Write(outPath, campaign.State, campaign.RandomStreams, GameVersionInfo.Current, contentHash);

        Console.WriteLine($"Bootstrapped a new campaign: date={campaign.State.Date.ToDisplayYearLabel()}, " +
            $"region={campaign.RegionId}, settlement={campaign.SettlementId}, household={campaign.HouseholdId}.");
        foreach (var evt in campaign.InitialHistory)
            Console.WriteLine($"  {evt.Type}: {string.Join(", ", evt.SubjectIds)}");
        Console.WriteLine($"Saved to '{outPath}'.");
        return 0;
    }

    /// <summary>Parsed as a raw <see cref="JsonDocument"/> rather than deserialized into <see
    /// cref="ContentCompiler.Content.CompiledContentPackDto"/> — mirroring <see cref="DiffCommand"/>'s
    /// own approach of reading a compiled pack's shape directly, since that shape (<c>families[].name</c>,
    /// <c>families[].definitions[].id</c>) is the compiled-output contract, not this DTO's in-memory
    /// round trip.</summary>
    private static bool RegionExists(byte[] packBytes, string regionId)
    {
        using var document = JsonDocument.Parse(packBytes);
        foreach (var family in document.RootElement.GetProperty("families").EnumerateArray())
        {
            if (family.GetProperty("name").GetString() != "regions")
                continue;

            foreach (var definition in family.GetProperty("definitions").EnumerateArray())
            {
                if (definition.TryGetProperty("id", out var idProperty) && idProperty.GetString() == regionId)
                    return true;
            }
        }

        return false;
    }
}
