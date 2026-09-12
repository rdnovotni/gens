using Gens.Simulation.Characters;
using Gens.Simulation.Education;
using Gens.Simulation.Identity;
using Gens.Simulation.Languages;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Education;

/// <summary>Phase 17 item 2 coverage, slice 2: the Literacy hard-gate read-side helpers (§3, §10).</summary>
public sealed class EducationGateResolverTests
{
    private static (WorldState State, RuntimeId<Character> CharacterId) UntrackedCharacter()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId, nomen: "Marcus", household: householdId));
        return (state, characterId);
    }

    [Test]
    public void GatesDefaultToPermissiveWhenNoExplicitLiteracyRecordExists()
    {
        var (state, characterId) = UntrackedCharacter();

        Assert.Multiple(() =>
        {
            Assert.That(EducationGateResolver.CanHoldLearningTierRole(state, characterId), Is.True);
            Assert.That(EducationGateResolver.CanUseCorrespondence(state, characterId), Is.True);
        });
    }

    [Test]
    public void GatesRejectOnlyAnExplicitIlliterateRecordAndAcceptAnExplicitLiterateOne()
    {
        var (state, characterId) = UntrackedCharacter();
        SetLiteracyCommands.Pipeline.Execute(
            state, new SetLiteracyCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, characterId, false, LiteracyDerivation.LearningAttribute));

        Assert.Multiple(() =>
        {
            Assert.That(EducationGateResolver.CanHoldLearningTierRole(state, characterId), Is.False);
            Assert.That(EducationGateResolver.CanUseCorrespondence(state, characterId), Is.False);
        });

        state.LiteracyRecords.Remove(characterId);
        SetLiteracyCommands.Pipeline.Execute(
            state, new SetLiteracyCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, characterId, true, LiteracyDerivation.LearningAttribute));

        Assert.Multiple(() =>
        {
            Assert.That(EducationGateResolver.CanHoldLearningTierRole(state, characterId), Is.True);
            Assert.That(EducationGateResolver.CanUseCorrespondence(state, characterId), Is.True);
        });
    }

    [Test]
    public void ClearsCulturalPrestigeMarriageThresholdMatchesTheResolverDirectly()
    {
        var (state, characterId) = UntrackedCharacter();
        state.Characters.TryGet(characterId, out var character);
        var householdId = character!.Household!.Value;

        Assert.That(EducationGateResolver.ClearsCulturalPrestigeMarriageThreshold(state, householdId), Is.False);

        CulturalPrestigeResolver.Apply(state, householdId, CulturalPrestigeCatalog.MarriageMarketPrestigeThreshold);

        Assert.That(EducationGateResolver.ClearsCulturalPrestigeMarriageThreshold(state, householdId), Is.True);
    }
}
