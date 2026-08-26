using Gens.Simulation.Interactions;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Saves;

/// <summary>Phase 10 item 6 save round-trip coverage, mirroring <see cref="HouseStandingSaveRoundTripTests"/>'s
/// identical pattern.</summary>
public sealed class SchemeSaveRoundTripTests
{
    [Test]
    public void SchemeRoundTripsThroughTheDtoAndCanonicalJson()
    {
        var state = new WorldState(new GameDate(5));
        var initiatorId = state.CharacterIds.Issue();
        var targetId = state.CharacterIds.Issue();
        var schemeId = state.SchemeIds.Issue();

        var scheme = Scheme.Create(schemeId, initiatorId, targetId, SchemeType.Coercive, new GameDate(2)) with
        {
            Progress = 40,
            DiscoveryRisk = 15,
            LastProgressedDate = new GameDate(5),
        };
        state.Schemes.Add(schemeId, scheme);

        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.Schemes.TryGet(schemeId, out var restoredScheme), Is.True);
            Assert.That(restoredScheme, Is.EqualTo(scheme));
        });

        var bytesA = CanonicalJson.SerializeToCanonicalBytes(dto);
        var bytesB = CanonicalJson.SerializeToCanonicalBytes(WorldStateMapper.ToDto(restored));
        Assert.That(bytesB, Is.EqualTo(bytesA));
    }

    [Test]
    public void AnEmptyPreExistingSaveFixtureStillLoadsWithoutAnySchemeData()
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Saves", "Fixtures", "v1-empty-campaign.gens");
        var loaded = SaveReader.Read(path);

        Assert.That(loaded.State.Schemes.Count, Is.EqualTo(0));
    }
}
