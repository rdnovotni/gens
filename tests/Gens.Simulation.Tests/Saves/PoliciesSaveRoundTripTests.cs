using Gens.Simulation.Policies;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Saves;

/// <summary>Phase 9 item 2 save round-trip coverage, mirroring <see cref="EconomySaveRoundTripTests"/>'s
/// identical pattern.</summary>
public sealed class PoliciesSaveRoundTripTests
{
    [Test]
    public void HouseholdPoliciesRoundTripsThroughTheDtoAndCanonicalJson()
    {
        var state = new WorldState(new GameDate(5));
        var householdId = state.HouseholdIds.Issue();
        var policy = new HouseholdPolicyState(householdId, RitesBudgetTier.Lavish, new GameDate(5));
        state.HouseholdPolicies.Add(householdId, policy);

        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.HouseholdPolicies.TryGet(householdId, out var restoredPolicy), Is.True);
            Assert.That(restoredPolicy, Is.EqualTo(policy));
        });

        var bytesA = CanonicalJson.SerializeToCanonicalBytes(dto);
        var bytesB = CanonicalJson.SerializeToCanonicalBytes(WorldStateMapper.ToDto(restored));
        Assert.That(bytesB, Is.EqualTo(bytesA));
    }

    [Test]
    public void AnEmptyPreExistingSaveFixtureStillLoadsWithoutAnyPhase9Item2Data()
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Saves", "Fixtures", "v1-empty-campaign.gens");
        var loaded = SaveReader.Read(path);

        Assert.That(loaded.State.HouseholdPolicies.Count, Is.EqualTo(0));
    }
}
