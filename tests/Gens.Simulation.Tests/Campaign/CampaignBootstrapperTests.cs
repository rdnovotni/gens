using Gens.Simulation.Campaign;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Campaign;

public sealed class CampaignBootstrapperTests
{
    [Test]
    public void BootstrapIssuesOneRegionSettlementAndHouseholdAndOneEvent()
    {
        var campaign = CampaignBootstrapper.Bootstrap(BuildConfig());

        Assert.That(campaign.State.RegionIds.Peek, Is.EqualTo(1));
        Assert.That(campaign.State.SettlementIds.Peek, Is.EqualTo(1));
        Assert.That(campaign.State.HouseholdIds.Peek, Is.EqualTo(1));
        Assert.That(campaign.InitialHistory, Has.Count.EqualTo(1));

        var bootstrapEvent = (CampaignBootstrappedEvent)campaign.InitialHistory[0];
        Assert.That(bootstrapEvent.RegionId, Is.EqualTo(campaign.RegionId));
        Assert.That(bootstrapEvent.SettlementId, Is.EqualTo(campaign.SettlementId));
        Assert.That(bootstrapEvent.HouseholdId, Is.EqualTo(campaign.HouseholdId));
        Assert.That(bootstrapEvent.SubjectIds, Is.EqualTo(new[]
        {
            campaign.RegionId.ToTaggedString(),
            campaign.SettlementId.ToTaggedString(),
            campaign.HouseholdId.ToTaggedString(),
        }));
    }

    [Test]
    public void BootstrapRegistersTheCampaignAndScheduledActionRandomStreams()
    {
        var campaign = CampaignBootstrapper.Bootstrap(BuildConfig());

        Assert.That(campaign.RandomStreams.Names, Does.Contain(CampaignBootstrapper.CampaignStreamName));
        Assert.That(campaign.RandomStreams.Names, Does.Contain(CampaignBootstrapper.ScheduledActionStreamName));
    }

    [Test]
    public void SameSeedProducesIdenticalInitialStateHash()
    {
        var config = BuildConfig();

        var campaignA = CampaignBootstrapper.Bootstrap(config);
        var campaignB = CampaignBootstrapper.Bootstrap(config);

        Assert.That(StateHasher.Hash(campaignA.State), Is.EqualTo(StateHasher.Hash(campaignB.State)));
    }

    [Test]
    public void BootstrapRejectsAnInvalidConfig()
    {
        var invalid = BuildConfig() with { RegionId = "" };

        Assert.That(() => CampaignBootstrapper.Bootstrap(invalid), Throws.ArgumentException);
    }

    private static CampaignConfig BuildConfig() => new()
    {
        Seed = 42UL,
        StartDate = new GameDate(0),
        RulesetId = "classic",
        ContentPackHash = "content-hash-placeholder",
        RegionId = "latium",
        Difficulty = "standard",
    };
}
