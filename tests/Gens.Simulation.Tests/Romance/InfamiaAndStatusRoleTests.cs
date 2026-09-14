using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Fame;
using Gens.Simulation.Identity;
using Gens.Simulation.Queries;
using Gens.Simulation.Reputation;
using Gens.Simulation.Romance;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;
using CharacterTestFixtures = Gens.Simulation.Tests.Characters.CharacterTestFixtures;

namespace Gens.Simulation.Tests.Romance;

/// <summary>Phase 17 item 3 slice 9 coverage (<c>gens-romance-sexuality-lineage-design.md</c> §13):
/// <see cref="GrantInfamiaCommand"/>'s round trip, <see cref="StatusRoleDignitasModifier"/>'s pure
/// rank-gap calculation (sex-independent, per §13's own "applied identically whichever sexes are
/// involved" framing), and <see cref="FameDivergenceQuery"/> reading the real <see cref="InfamiaStatus"/>
/// primitive.</summary>
public sealed class InfamiaAndStatusRoleTests
{
    private static readonly GameDate Epoch = new(0);

    private static WorldState NewState() => new(Epoch);

    private static RuntimeId<Character> AddAdult(
        WorldState state, LegalStatus status = LegalStatus.RomanCitizen, Sex sex = Sex.Male, RuntimeId<Household>? household = null)
    {
        var id = state.CharacterIds.Issue();
        state.Characters.Add(
            id, CharacterTestFixtures.Minimal(id, sex: sex, birthDate: new GameDate(-30 * 12), status: status, household: household));
        return id;
    }

    // ---- GrantInfamiaCommand ------------------------------------------------------------------

    [Test]
    public void GrantInfamiaCommandCreatesARealInfamiaStatus()
    {
        var state = NewState();
        var characterId = AddAdult(state);

        var result = GrantInfamiaCommands.Pipeline.Execute(
            state, new GrantInfamiaCommand(state.CommandIds.Issue(), "system", Epoch, null, characterId, InfamiaSource.ConvictedAdultery));

        Assert.That(result.Accepted, Is.True, $"Rejected: {result.Error}");

        state.InfamiaStatuses.TryGet(characterId, out var status);
        Assert.Multiple(() =>
        {
            Assert.That(status, Is.Not.Null);
            Assert.That(status!.Source, Is.EqualTo(InfamiaSource.ConvictedAdultery));
            Assert.That(status.LegalProtectionsLost, Is.Not.Empty);
        });
    }

    [Test]
    public void GrantInfamiaCommandRejectsADoubleGrant()
    {
        var state = NewState();
        var characterId = AddAdult(state);
        GrantInfamiaCommands.Pipeline.Execute(
            state, new GrantInfamiaCommand(state.CommandIds.Issue(), "system", Epoch, null, characterId, InfamiaSource.ConvictedAdultery));

        var result = GrantInfamiaCommands.Pipeline.Execute(
            state, new GrantInfamiaCommand(state.CommandIds.Issue(), "system", Epoch, null, characterId, InfamiaSource.Prostitution));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(GrantInfamiaCommands.AlreadyMarked));
    }

    // ---- StatusRoleDignitasModifier (§13) -----------------------------------------------------

    [Test]
    public void StatusRoleDignitasModifierReturnsZeroForEqualStatus()
    {
        var state = NewState();
        var a = AddAdult(state, status: LegalStatus.RomanCitizen);
        var b = AddAdult(state, status: LegalStatus.RomanCitizen, sex: Sex.Female);

        Assert.That(StatusRoleDignitasModifier.Calculate(state, a, b), Is.EqualTo(0));
        Assert.That(StatusRoleDignitasModifier.DetermineHigherRankedParty(state, a, b), Is.Null);
    }

    [Test]
    public void StatusRoleDignitasModifierGrowsWithABiggerStatusGap()
    {
        var state = NewState();
        var citizen = AddAdult(state, status: LegalStatus.RomanCitizen);
        var freedman = AddAdult(state, status: LegalStatus.Freedman, sex: Sex.Female);
        var peregrine = AddAdult(state, status: LegalStatus.Peregrine, sex: Sex.Female);

        var smallGapMagnitude = -StatusRoleDignitasModifier.Calculate(state, citizen, freedman);
        var bigGapMagnitude = -StatusRoleDignitasModifier.Calculate(state, citizen, peregrine);

        Assert.Multiple(() =>
        {
            Assert.That(smallGapMagnitude, Is.GreaterThan(0));
            Assert.That(bigGapMagnitude, Is.GreaterThan(smallGapMagnitude));
            Assert.That(StatusRoleDignitasModifier.DetermineHigherRankedParty(state, citizen, peregrine), Is.EqualTo(citizen));
        });
    }

    [Test]
    public void StatusRoleDignitasModifierDoesNotVaryByWhichPartyIsWhichSex()
    {
        var state = NewState();
        var citizenMale = AddAdult(state, status: LegalStatus.RomanCitizen, sex: Sex.Male);
        var freedwomanFemale = AddAdult(state, status: LegalStatus.Freedman, sex: Sex.Female);

        var stateReversed = NewState();
        var citizenFemale = AddAdult(stateReversed, status: LegalStatus.RomanCitizen, sex: Sex.Female);
        var freedmanMale = AddAdult(stateReversed, status: LegalStatus.Freedman, sex: Sex.Male);

        var magnitude = StatusRoleDignitasModifier.Calculate(state, citizenMale, freedwomanFemale);
        var magnitudeReversed = StatusRoleDignitasModifier.Calculate(stateReversed, citizenFemale, freedmanMale);

        Assert.That(magnitude, Is.EqualTo(magnitudeReversed));
    }

    // ---- FameDivergenceQuery reading the real InfamiaStatus primitive (§2, §13) ---------------

    [Test]
    public void FameDivergenceQueryReadsFamousAndDisreputableFromARealInfamiaStatus()
    {
        var state = NewState();
        var householdId = state.HouseholdIds.Issue();
        var characterId = AddAdult(state, household: householdId);

        AdjustFameCommands.Pipeline.Execute(
            state, new AdjustFameCommand(state.CommandIds.Issue(), "player", Epoch, null, characterId, 60, FameSourceType.ArenaOrCircusOrTheatre));
        // High Dignitas — the OLD Dignitas-threshold proxy would have read this as FamousAndRespected;
        // a real InfamiaStatus must be what decides FamousAndDisreputable now.
        AdjustDignitasCommands.Pipeline.Execute(
            state, new AdjustDignitasCommand(state.CommandIds.Issue(), "player", Epoch, null, householdId, 200, "high standing"));
        state.InfamiaStatuses.Add(characterId, new InfamiaStatus(characterId, InfamiaSource.ConvictedAdultery, new[] { "barred from holding public office" }));

        var reading = new FameDivergenceQuery(characterId).Execute(state, observerId: "player");

        Assert.That(reading.DivergenceCategory, Is.EqualTo(FameDivergenceCategory.FamousAndDisreputable));
    }
}
