using Gens.Application.Campaign;
using Gens.Simulation.Campaign;
using Gens.Simulation.Policies;
using Gens.Simulation.Queries;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Application.Tests;

public sealed class CampaignSessionTests
{
    [Test]
    public void NewCampaignProvidesInitialProjectionAndHistory()
    {
        var session = Create(out var history);

        var clock = session.Query(new CampaignClockQuery(), "player");

        Assert.Multiple(() =>
        {
            Assert.That(history.Select(e => e.Type), Does.Contain("campaign.bootstrapped"));
            Assert.That(clock.MonthOfYear, Is.EqualTo(1));
            Assert.That(session.HouseholdId.ToTaggedString(), Is.EqualTo("household_0000000"));
        });
    }

    [Test]
    public void ValidCommandIsAcceptedAndInvalidCommandDoesNotMutateState()
    {
        var session = Create(out _);
        var accepted = session.Submit(ChangeRitesBudgetCommands.Pipeline, ChangeBudget(session, RitesBudgetTier.Lavish));
        var invalidCommand = ChangeBudget(session, RitesBudgetTier.Lavish);
        var hashBeforeRejection = StateHasher.Hash(session.State);

        var rejected = session.Submit(ChangeRitesBudgetCommands.Pipeline, invalidCommand);

        Assert.Multiple(() =>
        {
            Assert.That(accepted.Accepted, Is.True);
            Assert.That(rejected.Accepted, Is.False);
            Assert.That(StateHasher.Hash(session.State), Is.EqualTo(hashBeforeRejection));
        });
    }

    [Test]
    public void AdvanceMonthMovesTheCalendarExactlyOnce()
    {
        var session = Create(out _);
        var before = session.Query(new CampaignClockQuery(), "player");

        session.AdvanceMonth(Array.Empty<IMonthlySystem<WorldState>>());

        var after = session.Query(new CampaignClockQuery(), "player");
        Assert.That(after.MonthOfYear, Is.EqualTo(before.MonthOfYear + 1));
    }

    [Test]
    public void SaveLoadPreservesHashRandomStreamsAndFutureDeterminism()
    {
        var first = Create(out _);
        first.Submit(ChangeRitesBudgetCommands.Pipeline, ChangeBudget(first, RitesBudgetTier.Lavish));
        first.AdvanceMonth(Array.Empty<IMonthlySystem<WorldState>>());
        first.AdvanceMonth(Array.Empty<IMonthlySystem<WorldState>>());
        var path = TempPath();

        try
        {
            var expectedHash = StateHasher.Hash(first.State);
            var expectedStreams = first.RandomStreams.CaptureStates();
            first.Save(path, "0.0.0-test");
            var second = CampaignSession.Load(path, out _);

            Assert.Multiple(() =>
            {
                Assert.That(StateHasher.Hash(second.State), Is.EqualTo(expectedHash));
                Assert.That(second.RandomStreams.CaptureStates(), Is.EqualTo(expectedStreams));
            });

            first.AdvanceMonth(Array.Empty<IMonthlySystem<WorldState>>());
            second.AdvanceMonth(Array.Empty<IMonthlySystem<WorldState>>());
            Assert.That(StateHasher.Hash(second.State), Is.EqualTo(StateHasher.Hash(first.State)));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public void ReplayVerificationMatchesSavedState()
    {
        var session = Create(out _);
        var path = TempPath();
        try
        {
            var result = session.VerifyDeterministicReplay(path, "0.0.0-test");
            Assert.Multiple(() =>
            {
                Assert.That(result.Matches, Is.True);
                Assert.That(result.HashAfterReload, Is.EqualTo(result.HashBeforeSave));
            });
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static CampaignSession Create(out IReadOnlyList<Gens.Simulation.Commands.IDomainEvent> history) =>
        CampaignSession.CreateNew(new CampaignConfig
        {
            Seed = 4242,
            StartDate = new GameDate(0),
            RulesetId = "default",
            ContentPackHash = "test-content",
            RegionId = "latium",
            Difficulty = "standard",
        }, out history);

    private static ChangeRitesBudgetCommand ChangeBudget(CampaignSession session, RitesBudgetTier tier) =>
        new(session.State.CommandIds.Issue(), session.HouseholdId.ToTaggedString(), session.State.Date, null, session.HouseholdId, tier);

    private static string TempPath() => Path.Combine(Path.GetTempPath(), $"gens-application-{Guid.NewGuid():N}.gens");
}
