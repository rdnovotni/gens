using Gens.Simulation.Campaign;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.ExitGate;

/// <summary>
/// Proves the Phase 4 exit gate, verbatim from the roadmap: "a seed-defined empty campaign advances
/// for 1,200 months, saves/loads repeatedly, and reproduces identical hashes without unbounded
/// allocations or history growth."
/// </summary>
public sealed class HeadlessCampaignSoakTests
{
    [Test]
    public void EmptyCampaignAdvancesTwelveHundredMonthsWithStableRepeatedSaveLoadHashes()
    {
        var config = BuildConfig(24680UL);
        var campaign = CampaignBootstrapper.Bootstrap(config);
        var state = campaign.State;
        var streams = campaign.RandomStreams;
        var simulation = NewSimulation();

        const int totalMonths = 1200;
        const int saveLoadEveryMonths = 100;

        for (var month = 0; month < totalMonths; month++)
        {
            simulation.Tick(state, state.Date, streams);
            state.AdvanceMonth();

            if ((month + 1) % saveLoadEveryMonths != 0)
                continue;

            var path = Path.Combine(Path.GetTempPath(), $"gens-soak-test-{Guid.NewGuid():N}.gens");
            try
            {
                var beforeHash = StateHasher.Hash(state);
                var beforeStreamStates = streams.CaptureStates();
                SaveWriter.Write(path, state, streams, "0.0.0-test", config.ContentPackHash);
                var loaded = SaveReader.Read(path);

                Assert.That(StateHasher.Hash(loaded.State), Is.EqualTo(beforeHash));
                Assert.That(loaded.RandomStreams.CaptureStates(), Is.EqualTo(beforeStreamStates));

                // Reload as the live state/streams going forward, proving repeated save->load->advance
                // cycles compose without drift, not just a single isolated round trip.
                state = loaded.State;
                streams = loaded.RandomStreams;
                simulation = NewSimulation();
            }
            finally
            {
                File.Delete(path);
            }
        }

        Assert.That(state.Date.TotalMonths, Is.EqualTo(totalMonths));

        // No unbounded growth: an empty campaign with no scheduled work never accumulates queue,
        // character, or knowledge entries across 1,200 months.
        Assert.That(state.ScheduledActions.Count, Is.EqualTo(0));
        Assert.That(state.Characters.Count, Is.EqualTo(0));
        Assert.That(state.Knowledge.All().Count(), Is.EqualTo(0));
    }

    [Test]
    public void SameSeedReproducesIdenticalHashAfterAFullSoakRun()
    {
        var config = BuildConfig(13579UL);

        Assert.That(RunSoak(config, 200), Is.EqualTo(RunSoak(config, 200)));
    }

    private static ulong RunSoak(CampaignConfig config, int months)
    {
        var campaign = CampaignBootstrapper.Bootstrap(config);
        var simulation = NewSimulation();

        for (var month = 0; month < months; month++)
        {
            simulation.Tick(campaign.State, campaign.State.Date, campaign.RandomStreams);
            campaign.State.AdvanceMonth();
        }

        return StateHasher.Hash(campaign.State);
    }

    private static WriteSetVerifyingSimulation NewSimulation() =>
        new(new IMonthlySystem<WorldState>[] { new ScheduledActionSystem() });

    private static CampaignConfig BuildConfig(ulong seed) => new()
    {
        Seed = seed,
        StartDate = new GameDate(0),
        RulesetId = "classic",
        ContentPackHash = "content-hash-placeholder",
        RegionId = "latium",
        Difficulty = "standard",
    };
}
