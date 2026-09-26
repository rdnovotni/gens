using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Romance;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Romance;

/// <summary>Exhaustive coverage of Phase 17 item 3's shared Two-Hard-Exclusions gate
/// (<c>gens-romance-sexuality-lineage-design.md</c> §2) — every later Romance command/system is
/// expected to just assert the shared rejection code, so this file is where each individual rule
/// actually gets proven.</summary>
public sealed class RomanceEligibilityTests
{
    private static readonly GameDate Date0 = new(0);

    private static WorldState NewState() => new(Date0);

    private static RuntimeId<Character> AddCharacter(
        WorldState state,
        LifecycleStage stage = LifecycleStage.Adult,
        LegalStatus status = LegalStatus.RomanCitizen,
        DeathRecord? deathRecord = null)
    {
        var id = state.CharacterIds.Issue();
        var birthDate = stage switch
        {
            LifecycleStage.Infant => new GameDate(0),
            LifecycleStage.Child => new GameDate(-8 * 12),
            LifecycleStage.Adolescent => new GameDate(-15 * 12),
            LifecycleStage.Adult => new GameDate(-30 * 12),
            LifecycleStage.Elderly => new GameDate(-70 * 12),
            _ => new GameDate(-30 * 12),
        };
        state.Characters.Add(
            id,
            CharacterTestFixtures.Minimal(id, birthDate: birthDate, status: status, deathRecord: deathRecord));
        return id;
    }

    [Test]
    public void TwoAdultsPass()
    {
        var state = NewState();
        var a = AddCharacter(state, LifecycleStage.Adult);
        var b = AddCharacter(state, LifecycleStage.Adult);

        Assert.That(RomanceEligibility.CheckPair(state, a, b, Date0), Is.Null);
    }

    [Test]
    public void TwoEldersPass()
    {
        var state = NewState();
        var a = AddCharacter(state, LifecycleStage.Elderly);
        var b = AddCharacter(state, LifecycleStage.Elderly);

        Assert.That(RomanceEligibility.CheckPair(state, a, b, Date0), Is.Null);
    }

    [Test]
    public void AnAdultAndAnElderlyCharacterPass()
    {
        var state = NewState();
        var a = AddCharacter(state, LifecycleStage.Adult);
        var b = AddCharacter(state, LifecycleStage.Elderly);

        Assert.That(RomanceEligibility.CheckPair(state, a, b, Date0), Is.Null);
    }

    [Test]
    public void AnAdolescentCharacterAIsRejected()
    {
        var state = NewState();
        var a = AddCharacter(state, LifecycleStage.Adolescent);
        var b = AddCharacter(state, LifecycleStage.Adult);

        Assert.That(
            RomanceEligibility.CheckPair(state, a, b, Date0),
            Is.EqualTo(RomanceEligibility.CharacterBelowAdultLifecycleStage));
    }

    [Test]
    public void AnAdolescentCharacterBIsRejected()
    {
        var state = NewState();
        var a = AddCharacter(state, LifecycleStage.Adult);
        var b = AddCharacter(state, LifecycleStage.Adolescent);

        Assert.That(
            RomanceEligibility.CheckPair(state, a, b, Date0),
            Is.EqualTo(RomanceEligibility.CharacterBelowAdultLifecycleStage));
    }

    [Test]
    public void AChildCharacterIsRejected()
    {
        var state = NewState();
        var a = AddCharacter(state, LifecycleStage.Child);
        var b = AddCharacter(state, LifecycleStage.Adult);

        Assert.That(
            RomanceEligibility.CheckPair(state, a, b, Date0),
            Is.EqualTo(RomanceEligibility.CharacterBelowAdultLifecycleStage));
    }

    [Test]
    public void AnInfantCharacterIsRejected()
    {
        var state = NewState();
        var a = AddCharacter(state, LifecycleStage.Infant);
        var b = AddCharacter(state, LifecycleStage.Adult);

        Assert.That(
            RomanceEligibility.CheckPair(state, a, b, Date0),
            Is.EqualTo(RomanceEligibility.CharacterBelowAdultLifecycleStage));
    }

    [Test]
    public void AnEnslavedCharacterAIsRejected()
    {
        var state = NewState();
        var a = AddCharacter(state, status: LegalStatus.Enslaved);
        var b = AddCharacter(state, status: LegalStatus.RomanCitizen);

        Assert.That(
            RomanceEligibility.CheckPair(state, a, b, Date0),
            Is.EqualTo(RomanceEligibility.PowerImbalancedPairing));
    }

    [Test]
    public void AnEnslavedCharacterBIsRejected()
    {
        var state = NewState();
        var a = AddCharacter(state, status: LegalStatus.RomanCitizen);
        var b = AddCharacter(state, status: LegalStatus.Enslaved);

        Assert.That(
            RomanceEligibility.CheckPair(state, a, b, Date0),
            Is.EqualTo(RomanceEligibility.PowerImbalancedPairing));
    }

    [Test]
    public void TwoEnslavedCharactersAreRejected()
    {
        var state = NewState();
        var a = AddCharacter(state, status: LegalStatus.Enslaved);
        var b = AddCharacter(state, status: LegalStatus.Enslaved);

        Assert.That(
            RomanceEligibility.CheckPair(state, a, b, Date0),
            Is.EqualTo(RomanceEligibility.PowerImbalancedPairing));
    }

    [Test]
    public void ADeceasedCharacterAIsRejected()
    {
        var state = NewState();
        var a = AddCharacter(state, deathRecord: new DeathRecord(Date0, DeathCause.Unspecified, 30));
        var b = AddCharacter(state);

        Assert.That(
            RomanceEligibility.CheckPair(state, a, b, Date0),
            Is.EqualTo(RomanceEligibility.CharacterADeceased));
    }

    [Test]
    public void ADeceasedCharacterBIsRejected()
    {
        var state = NewState();
        var a = AddCharacter(state);
        var b = AddCharacter(state, deathRecord: new DeathRecord(Date0, DeathCause.Unspecified, 30));

        Assert.That(
            RomanceEligibility.CheckPair(state, a, b, Date0),
            Is.EqualTo(RomanceEligibility.CharacterBDeceased));
    }

    [Test]
    public void ASelfPairIsRejected()
    {
        var state = NewState();
        var a = AddCharacter(state);

        Assert.That(RomanceEligibility.CheckPair(state, a, a, Date0), Is.EqualTo(RomanceEligibility.SelfPair));
    }

    [Test]
    public void AMissingCharacterAIsRejected()
    {
        var state = NewState();
        var missing = state.CharacterIds.Issue();
        var b = AddCharacter(state);

        Assert.That(
            RomanceEligibility.CheckPair(state, missing, b, Date0),
            Is.EqualTo(RomanceEligibility.CharacterANotFound));
    }

    [Test]
    public void AMissingCharacterBIsRejected()
    {
        var state = NewState();
        var a = AddCharacter(state);
        var missing = state.CharacterIds.Issue();

        Assert.That(
            RomanceEligibility.CheckPair(state, a, missing, Date0),
            Is.EqualTo(RomanceEligibility.CharacterBNotFound));
    }
}
