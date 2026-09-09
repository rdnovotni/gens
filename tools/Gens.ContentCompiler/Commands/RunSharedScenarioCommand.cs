using System.Globalization;
using System.Linq;
using System.Text.Json;
using Gens.ContentCompiler.Content;
using Gens.Simulation.Campaign;
using Gens.Simulation.Chronicle;
using Gens.Simulation.Commands;
using Gens.Simulation.Epithets;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.ContentCompiler.Commands;

/// <summary>
/// <c>run-shared-scenario &lt;scenarioSpecPath&gt; --content &lt;compiledPackPath&gt;
/// --fixtures-out &lt;pathPrefix&gt; --transcript-out &lt;path&gt;</c>: the UR-01 shared-scenario
/// runner (ADR 0019 — <see cref="Gens.ContentCompiler.Content.SharedScenarioSpec"/>). Bootstraps a
/// campaign from the spec's fixed seed/region/ruleset/difficulty, executes its command sequence
/// immediately (mirroring <c>submit-command --due-in-months 0</c>), then advances month-by-month —
/// capturing a <see cref="StateHasher"/> checkpoint at bootstrap, after every command, at every month
/// boundary, and at the end. Writes a legacy-shaped fixture (state before any command runs) and the
/// current-native fixture (final state), plus the full hash transcript that, during the Unity
/// retirement migration, a human could compare against a manually reproduced Unity-side run.
/// </summary>
public static class RunSharedScenarioCommand
{
    private static readonly JsonSerializerOptions SpecOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static int Run(string specPath, string contentPackPath, string fixturesOutPrefix, string transcriptOutPath)
    {
        var spec = JsonSerializer.Deserialize<SharedScenarioSpec>(File.ReadAllBytes(specPath), SpecOptions) ??
            throw new InvalidOperationException($"Failed to parse scenario spec '{specPath}'.");

        var packBytes = File.ReadAllBytes(contentPackPath);
        var contentHash = CanonicalJson.Sha256Hex(packBytes);

        if (!RegionExists(packBytes, spec.Region))
        {
            Console.Error.WriteLine($"Region '{spec.Region}' was not found in compiled pack '{contentPackPath}'.");
            return 1;
        }

        var config = new CampaignConfig
        {
            Seed = spec.Seed,
            StartDate = new GameDate(spec.StartMonths),
            RulesetId = spec.Ruleset,
            ContentPackHash = contentHash,
            RegionId = spec.Region,
            StartProfileId = spec.StartProfileId,
            Difficulty = spec.Difficulty,
        };

        var campaign = CampaignBootstrapper.Bootstrap(config);
        var state = campaign.State;
        var streams = campaign.RandomStreams;

        var checkpoints = new List<HashCheckpointDto>();
        void Capture(string label) => checkpoints.Add(new HashCheckpointDto
        {
            Label = label,
            DateTotalMonths = state.Date.TotalMonths,
            StateHashHex = StateHasher.Hash(state).ToString("x16", CultureInfo.InvariantCulture),
        });

        Capture("initial");

        var legacyPath = $"{fixturesOutPrefix}-legacy.gens";
        SaveWriter.Write(legacyPath, state, streams, GameVersionInfo.Current, contentHash);

        foreach (var command in spec.Commands)
        {
            var pipeline = new CommandPipeline<Gens.Simulation.State.WorldState, GenericCommand>(
                validate: static (_, _) => null,
                mutate: (worldState, cmd) => new IDomainEvent[]
                {
                    new GenericCommandExecutedEvent(worldState.EventIds.Issue(), worldState.Date, cmd.ActorId, cmd.CommandType, cmd.PayloadJson, cmd.CausationId),
                },
                issueSequenceNumber: static worldState => worldState.IssueCommandSequenceNumber());

            var envelope = new GenericCommand(state.CommandIds.Issue(), command.ActorId, state.Date, null, command.Type, command.Payload);
            var result = pipeline.Execute(state, envelope);
            if (!result.Accepted)
            {
                Console.Error.WriteLine($"Shared scenario command '{command.Type}' was rejected: {result.Error}.");
                return 1;
            }

            Capture($"post-command:{command.Type}");
        }

        for (var i = 0; i < spec.Months; i++)
        {
            var simulation = new WriteSetVerifyingSimulation(new IMonthlySystem<Gens.Simulation.State.WorldState>[] { new ScheduledActionSystem() });
            var tickEvents = simulation.Tick(state, state.Date, streams);
            var chronicleEvents = ChronicleGenerationSystem.Generate(state, tickEvents);
            var combined = tickEvents.Concat(chronicleEvents).ToArray();
            EpithetGenerationSystem.Generate(state, combined);
            state.AdvanceMonth();
            Capture($"month:{i + 1}");
        }

        Capture("final");

        var currentNativePath = $"{fixturesOutPrefix}-current-native.gens";
        SaveWriter.Write(currentNativePath, state, streams, GameVersionInfo.Current, contentHash);

        var transcript = new HashTranscriptDto { ScenarioSpecPath = specPath, Checkpoints = checkpoints };
        File.WriteAllBytes(transcriptOutPath, CanonicalJson.SerializeToCanonicalBytes(transcript));

        Console.WriteLine($"Ran shared scenario '{specPath}': {checkpoints.Count} checkpoint(s) captured.");
        Console.WriteLine($"Fixtures: '{legacyPath}', '{currentNativePath}'.");
        Console.WriteLine($"Transcript: '{transcriptOutPath}'.");
        return 0;
    }

    /// <summary>Mirrors <see cref="NewCampaignCommand"/>'s own region-existence check against a
    /// compiled pack's raw JSON shape.</summary>
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
