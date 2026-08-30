using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Fame;
using Gens.Simulation.Identity;
using Gens.Simulation.Magistracies;
using Gens.Simulation.Queries;
using Gens.Simulation.Random;
using Gens.Simulation.Reputation;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Fame;

/// <summary>Phase 12 item 8 coverage: the shared Fame primitive, its monthly decay, the Fame/Dignitas
/// Divergence reading, and the Fame-endorsement bonus wired into <see
/// cref="HoldContestedElectionCommand"/> (<c>gens-celebrities-influential-figures-design.md</c>).</summary>
public sealed class FameTests
{
    private static (WorldState State, RuntimeId<Character> CharacterId) OneCharacter()
    {
        var state = new WorldState(new GameDate(0));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId, nomen: "Cornelius"));
        return (state, characterId);
    }

    [Test]
    public void FameResolverDefaultsToZeroForAnUntouchedCharacter()
    {
        var (state, characterId) = OneCharacter();
        Assert.That(FameResolver.Current(state, characterId), Is.EqualTo(0));
    }

    [Test]
    public void AdjustFameCommandMovesTheScoreAndEmitsAPublicEvent()
    {
        var (state, characterId) = OneCharacter();

        var result = AdjustFameCommands.Pipeline.Execute(
            state,
            new AdjustFameCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, characterId, 30, FameSourceType.Oratory));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(FameResolver.Current(state, characterId), Is.EqualTo(30));

            var changed = (FameChangedEvent)result.Events[0];
            Assert.That(changed.PreviousFame, Is.EqualTo(0));
            Assert.That(changed.NewFame, Is.EqualTo(30));
            Assert.That(changed.SourceType, Is.EqualTo(FameSourceType.Oratory));
            Assert.That(changed.Visibility, Is.EqualTo(Visibility.Public));
        });
    }

    [Test]
    public void AdjustFameCommandClampsToTheZeroToOneHundredRange()
    {
        var (state, characterId) = OneCharacter();

        AdjustFameCommands.Pipeline.Execute(
            state, new AdjustFameCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, characterId, 90, FameSourceType.MilitaryValor));
        AdjustFameCommands.Pipeline.Execute(
            state, new AdjustFameCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, characterId, 90, FameSourceType.MilitaryValor));

        Assert.That(FameResolver.Current(state, characterId), Is.EqualTo(100));

        AdjustFameCommands.Pipeline.Execute(
            state, new AdjustFameCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, characterId, -500, FameSourceType.RomanceOrScandal));

        Assert.That(FameResolver.Current(state, characterId), Is.EqualTo(0));
    }

    [Test]
    public void AdjustFameCommandRejectsAZeroDelta()
    {
        var (state, characterId) = OneCharacter();

        var result = AdjustFameCommands.Pipeline.Execute(
            state, new AdjustFameCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, characterId, 0, FameSourceType.Athletics));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(AdjustFameCommands.ZeroDelta));
    }

    [Test]
    public void AdjustFameCommandRejectsAnUnknownCharacter()
    {
        var state = new WorldState(new GameDate(0));
        var unknownId = state.CharacterIds.Issue();

        var result = AdjustFameCommands.Pipeline.Execute(
            state, new AdjustFameCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, unknownId, 10, FameSourceType.WandererRenown));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(AdjustFameCommands.UnknownCharacter));
    }

    [Test]
    public void FameDecaySystemErodesEveryStoredBalanceMonthly()
    {
        var (state, characterId) = OneCharacter();
        AdjustFameCommands.Pipeline.Execute(
            state, new AdjustFameCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, characterId, 10, FameSourceType.LiteraryWork));

        new FameDecaySystem().Tick(state, new MonthlyTickContext(new GameDate(2), new RandomStreamSet()));

        Assert.That(FameResolver.Current(state, characterId), Is.EqualTo(10 - FameCatalog.DecayPerMonth));
    }

    [Test]
    public void FameDecaySystemNeverDecaysBelowZero()
    {
        var (state, characterId) = OneCharacter();
        AdjustFameCommands.Pipeline.Execute(
            state, new AdjustFameCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, characterId, 1, FameSourceType.ReligiousCharisma));

        new FameDecaySystem().Tick(state, new MonthlyTickContext(new GameDate(2), new RandomStreamSet()));

        Assert.That(FameResolver.Current(state, characterId), Is.EqualTo(0));
    }

    [Test]
    public void FameDivergenceQueryReadsFamousAndDisreputableForHighFameLowDignitas()
    {
        var (state, characterId) = OneCharacter();
        var householdId = state.HouseholdIds.Issue();
        state.Characters.TryGet(characterId, out var character);
        state.Characters.Remove(characterId);
        state.Characters.Add(characterId, character! with { Household = householdId });

        AdjustFameCommands.Pipeline.Execute(
            state, new AdjustFameCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, characterId, 60, FameSourceType.ArenaOrCircusOrTheatre));
        AdjustDignitasCommands.Pipeline.Execute(
            state, new AdjustDignitasCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, -10, "Infamia-adjacent"));

        var reading = new FameDivergenceQuery(characterId).Execute(state, observerId: "player");

        Assert.Multiple(() =>
        {
            Assert.That(reading.Fame, Is.EqualTo(60));
            Assert.That(reading.Dignitas, Is.EqualTo(-10));
            Assert.That(reading.DivergenceCategory, Is.EqualTo(FameDivergenceCategory.FamousAndDisreputable));
        });
    }

    [Test]
    public void FameDivergenceQueryReadsRespectedAndObscureForHighDignitasLowFame()
    {
        var (state, characterId) = OneCharacter();
        var householdId = state.HouseholdIds.Issue();
        state.Characters.TryGet(characterId, out var character);
        state.Characters.Remove(characterId);
        state.Characters.Add(characterId, character! with { Household = householdId });

        AdjustDignitasCommands.Pipeline.Execute(
            state, new AdjustDignitasCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, 30, "a won magistracy"));

        var reading = new FameDivergenceQuery(characterId).Execute(state, observerId: "player");

        Assert.That(reading.DivergenceCategory, Is.EqualTo(FameDivergenceCategory.RespectedAndObscure));
    }

    [Test]
    public void FameDivergenceQueryDefaultsToNeitherYetForAnUntouchedCharacter()
    {
        var (state, characterId) = OneCharacter();
        var reading = new FameDivergenceQuery(characterId).Execute(state, observerId: "player");
        Assert.That(reading.DivergenceCategory, Is.EqualTo(FameDivergenceCategory.NeitherYet));
    }

    [Test]
    public void HoldContestedElectionCommandAppliesAFamousEndorsementBonus()
    {
        var state = new WorldState(new GameDate(0));
        var settlementId = state.SettlementIds.Issue();

        var challengerHousehold = state.HouseholdIds.Issue();
        var challengerId = state.CharacterIds.Issue();
        state.Characters.Add(challengerId, CharacterTestFixtures.Minimal(challengerId, nomen: "Challenger", household: challengerHousehold));
        var decurionRecordId = state.MagistracyRecordIds.Issue();
        state.MagistracyRecords.Add(
            decurionRecordId, new MagistracyRecord(decurionRecordId, challengerId, MagistracyOffice.Decurion, settlementId, new GameDate(0)));

        var incumbentHousehold = state.HouseholdIds.Issue();
        var incumbentId = state.CharacterIds.Issue();
        state.Characters.Add(incumbentId, CharacterTestFixtures.Minimal(incumbentId, nomen: "Incumbent", household: incumbentHousehold));
        var incumbentDecurionId = state.MagistracyRecordIds.Issue();
        state.MagistracyRecords.Add(
            incumbentDecurionId, new MagistracyRecord(incumbentDecurionId, incumbentId, MagistracyOffice.Decurion, settlementId, new GameDate(0)));

        var celebrityId = state.CharacterIds.Issue();
        state.Characters.Add(celebrityId, CharacterTestFixtures.Minimal(celebrityId, nomen: "Celebrity"));
        AdjustFameCommands.Pipeline.Execute(
            state, new AdjustFameCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, celebrityId, 75, FameSourceType.ArenaOrCircusOrTheatre));

        var result = HoldContestedElectionCommands.Pipeline.Execute(
            state,
            new HoldContestedElectionCommand(
                state.CommandIds.Issue(), "player", new GameDate(2), null, MagistracyOffice.Aedile, settlementId,
                IncumbentCharacterId: null, challengerId, InfluenceSpentByChallenger: 0, InfluenceSpentByIncumbent: 0,
                EndorsingCelebrityForChallenger: celebrityId));

        var resolved = (ElectionResolvedEvent)result.Events[^1];
        state.Characters.TryGet(challengerId, out var challenger);
        var diplomacy = challenger!.GetEffectiveAttributes().Diplomacy;

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(resolved.WinnerScore, Is.EqualTo(diplomacy + FameCatalog.EndorsementScoreBonus));
        });
    }

    [Test]
    public void FameStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, characterId) = OneCharacter();
        AdjustFameCommands.Pipeline.Execute(
            state, new AdjustFameCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, characterId, 42, FameSourceType.Athletics));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(FameResolver.Current(restored, characterId), Is.EqualTo(42));
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        });
    }
}
