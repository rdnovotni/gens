using Gens.Simulation.Interactions;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Interactions;

/// <summary>Phase 10 item 6 coverage for <see cref="Scheme"/>.</summary>
public sealed class SchemeTests
{
    [Test]
    public void CreateStartsInProgressAtZeroProgressAndRisk()
    {
        var state = new WorldState(new GameDate(0));
        var initiatorId = state.CharacterIds.Issue();
        var targetId = state.CharacterIds.Issue();
        var schemeId = state.SchemeIds.Issue();

        var scheme = Scheme.Create(schemeId, initiatorId, targetId, SchemeType.Coercive, new GameDate(10));

        Assert.Multiple(() =>
        {
            Assert.That(scheme.Status, Is.EqualTo(SchemeStatus.InProgress));
            Assert.That(scheme.Progress, Is.EqualTo(0));
            Assert.That(scheme.DiscoveryRisk, Is.EqualTo(0));
            Assert.That(scheme.InitiatedDate, Is.EqualTo(new GameDate(10)));
            Assert.That(scheme.LastProgressedDate, Is.EqualTo(new GameDate(10)));
            Assert.That(scheme.IsResolved, Is.False);
        });
    }

    [Test]
    public void CreateRejectsASchemeTargetingItsOwnInitiator()
    {
        var state = new WorldState(new GameDate(0));
        var characterId = state.CharacterIds.Issue();
        var schemeId = state.SchemeIds.Issue();

        Assert.Throws<ArgumentException>(
            () => Scheme.Create(schemeId, characterId, characterId, SchemeType.Coercive, new GameDate(0)));
    }

    [Test]
    public void IsResolvedIsTrueForEveryNonInProgressStatus()
    {
        var state = new WorldState(new GameDate(0));
        var initiatorId = state.CharacterIds.Issue();
        var targetId = state.CharacterIds.Issue();
        var schemeId = state.SchemeIds.Issue();
        var scheme = Scheme.Create(schemeId, initiatorId, targetId, SchemeType.Coercive, new GameDate(0));

        Assert.That((scheme with { Status = SchemeStatus.Succeeded }).IsResolved, Is.True);
    }
}
