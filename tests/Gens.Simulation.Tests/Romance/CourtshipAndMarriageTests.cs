using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Romance;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;
using CharacterTestFixtures = Gens.Simulation.Tests.Characters.CharacterTestFixtures;

namespace Gens.Simulation.Tests.Romance;

/// <summary>Phase 17 item 3 slice 3 coverage: the four Courtship interactions
/// (<c>gens-romance-sexuality-lineage-design.md</c> §4), <see cref="ProposeMarriageCommand"/> (§4, §5),
/// and Concubinage's establish/end round-trip (§6).</summary>
public sealed class CourtshipAndMarriageTests
{
    private static WorldState NewState() => new(new GameDate(0));

    private static RuntimeId<Character> AddAdult(WorldState state, Sex sex = Sex.Male, DeathRecord? deathRecord = null)
    {
        var id = state.CharacterIds.Issue();
        state.Characters.Add(
            id, CharacterTestFixtures.Minimal(id, birthDate: new GameDate(-30 * 12), deathRecord: deathRecord, sex: sex));
        return id;
    }

    private static RuntimeId<Character> AddMarriedAdult(WorldState state, RuntimeId<Character> spouseId, Sex sex = Sex.Male)
    {
        var id = state.CharacterIds.Issue();
        var character = CharacterTestFixtures.Minimal(
            id, birthDate: new GameDate(-30 * 12), sex: sex,
            maritalHistory: new[] { new MarriageRecord(spouseId, new GameDate(-10), null, null) });
        state.Characters.Add(id, character);
        return id;
    }

    // ---- Courtship interactions -----------------------------------------------------------------

    [TestCase(CourtshipInteractionKind.Flirt, RomanceCatalog.FlirtAffectionDelta, RomanceCatalog.FlirtAttractionDelta)]
    [TestCase(CourtshipInteractionKind.CourtWoo, RomanceCatalog.CourtWooAffectionDelta, RomanceCatalog.CourtWooAttractionDelta)]
    [TestCase(CourtshipInteractionKind.ConfessFeelings, RomanceCatalog.ConfessFeelingsAffectionDelta, RomanceCatalog.ConfessFeelingsAttractionDelta)]
    public void PositiveCourtshipInteractionsMoveAffectionAndAttractionByTheCatalogDeltas(
        CourtshipInteractionKind kind, int expectedAffectionDelta, int expectedAttractionDelta)
    {
        var state = NewState();
        var a = AddAdult(state);
        var b = AddAdult(state);

        var command = new RecordCourtshipInteractionCommand(
            state.CommandIds.Issue(), "player", new GameDate(0), null, a, b, kind);
        var result = RecordCourtshipInteractionCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        state.RomanticBonds.TryGet(RomanticBondKey.Create(a, b), out var bond);
        Assert.Multiple(() =>
        {
            Assert.That(bond.Affection, Is.EqualTo(expectedAffectionDelta));
            Assert.That(bond.Attraction, Is.EqualTo(expectedAttractionDelta));
        });
    }

    [Test]
    public void RebukeMovesAffectionAndAttractionDownward()
    {
        var state = NewState();
        var a = AddAdult(state);
        var b = AddAdult(state);
        RecordCourtshipInteractionCommands.Pipeline.Execute(
            state, new RecordCourtshipInteractionCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, a, b, CourtshipInteractionKind.ConfessFeelings));

        RecordCourtshipInteractionCommands.Pipeline.Execute(
            state, new RecordCourtshipInteractionCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, a, b, CourtshipInteractionKind.Rebuke));

        state.RomanticBonds.TryGet(RomanticBondKey.Create(a, b), out var bond);
        Assert.Multiple(() =>
        {
            Assert.That(bond.Affection, Is.EqualTo(0));
            Assert.That(bond.Attraction, Is.EqualTo(0));
        });
    }

    [Test]
    public void RejectsAnIneligiblePairWithTheSharedEligibilityRejectionCode()
    {
        var state = NewState();
        var a = AddAdult(state);
        var b = AddAdult(state, deathRecord: new DeathRecord(new GameDate(-1), DeathCause.Disease, 29));

        var command = new RecordCourtshipInteractionCommand(
            state.CommandIds.Issue(), "player", new GameDate(0), null, a, b, CourtshipInteractionKind.Flirt);
        var result = RecordCourtshipInteractionCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(RomanceEligibility.CharacterBDeceased));
    }

    // ---- ProposeMarriageCommand -------------------------------------------------------------------

    [Test]
    public void RejectsAProposalBelowTheLoveMatchThreshold()
    {
        var state = NewState();
        var a = AddAdult(state, sex: Sex.Male);
        var b = AddAdult(state, sex: Sex.Female);
        RecordRomanticInteractionCommands.Pipeline.Execute(
            state, new RecordRomanticInteractionCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, a, b,
                RomanceCatalog.LoveMatchThreshold - 1, RomanceCatalog.LoveMatchThreshold - 1, null));

        var command = new ProposeMarriageCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, a, b);
        var result = ProposeMarriageCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(ProposeMarriageCommands.InsufficientBond));
    }

    [Test]
    public void RejectsAProposalWithNoExistingBondAtAll()
    {
        var state = NewState();
        var a = AddAdult(state, sex: Sex.Male);
        var b = AddAdult(state, sex: Sex.Female);

        var command = new ProposeMarriageCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, a, b);
        var result = ProposeMarriageCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(ProposeMarriageCommands.InsufficientBond));
    }

    [Test]
    public void RejectsAProposalWhenEitherSideIsAlreadyMarried()
    {
        var state = NewState();
        var thirdParty = AddAdult(state, sex: Sex.Female);
        var a = AddMarriedAdult(state, thirdParty, sex: Sex.Male);
        var b = AddAdult(state, sex: Sex.Female);
        RecordRomanticInteractionCommands.Pipeline.Execute(
            state, new RecordRomanticInteractionCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, a, b,
                RomanceCatalog.LoveMatchThreshold, RomanceCatalog.LoveMatchThreshold, null));

        var command = new ProposeMarriageCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, a, b);
        var result = ProposeMarriageCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(ProposeMarriageCommands.AlreadyMarried));
    }

    [Test]
    public void RejectsASameSexProposalViaRecordMarriageCommandsOwnValidation()
    {
        var state = NewState();
        var a = AddAdult(state, sex: Sex.Male);
        var b = AddAdult(state, sex: Sex.Male);
        RecordRomanticInteractionCommands.Pipeline.Execute(
            state, new RecordRomanticInteractionCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, a, b,
                RomanceCatalog.LoveMatchThreshold, RomanceCatalog.LoveMatchThreshold, null));

        var command = new ProposeMarriageCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, a, b);
        var result = ProposeMarriageCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(RecordMarriageCommands.SameSexNotSupported));
    }

    [Test]
    public void SucceedsAboveThresholdAndFlipsTheBondToMarriage()
    {
        var state = NewState();
        var a = AddAdult(state, sex: Sex.Male);
        var b = AddAdult(state, sex: Sex.Female);
        RecordRomanticInteractionCommands.Pipeline.Execute(
            state, new RecordRomanticInteractionCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, a, b,
                RomanceCatalog.LoveMatchThreshold, RomanceCatalog.LoveMatchThreshold, null));

        var command = new ProposeMarriageCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, a, b);
        var result = ProposeMarriageCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        state.Characters.TryGet(a, out var character);
        state.Characters.TryGet(b, out var spouse);
        state.RomanticBonds.TryGet(RomanticBondKey.Create(a, b), out var bond);
        Assert.Multiple(() =>
        {
            Assert.That(character.CurrentSpouseId, Is.EqualTo(b));
            Assert.That(spouse.CurrentSpouseId, Is.EqualTo(a));
            Assert.That(bond.BondType, Is.EqualTo(RomanticBondType.Marriage));
        });
    }

    // ---- Concubinage round-trip -------------------------------------------------------------------

    [Test]
    public void EstablishingConcubinageCreatesAPubliclyKnownBondWithNoDowryInvolved()
    {
        var state = NewState();
        var a = AddAdult(state);
        var b = AddAdult(state);

        var command = new EstablishConcubinageCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, a, b);
        var result = EstablishConcubinageCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        state.RomanticBonds.TryGet(RomanticBondKey.Create(a, b), out var bond);
        Assert.Multiple(() =>
        {
            Assert.That(bond.BondType, Is.EqualTo(RomanticBondType.Concubinage));
            Assert.That(bond.IsKnownPublicly, Is.True);
        });
    }

    [Test]
    public void EstablishingConcubinageIsAllowedWhenEitherPartyIsIndependentlyMarried()
    {
        var state = NewState();
        var spouseOfA = AddAdult(state, sex: Sex.Female);
        var a = AddMarriedAdult(state, spouseOfA, sex: Sex.Male);
        var b = AddAdult(state);

        var command = new EstablishConcubinageCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, a, b);
        var result = EstablishConcubinageCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
    }

    [Test]
    public void ConcubinageRoundTripsToPastRelationshipOnEnd()
    {
        var state = NewState();
        var a = AddAdult(state);
        var b = AddAdult(state);
        EstablishConcubinageCommands.Pipeline.Execute(
            state, new EstablishConcubinageCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, a, b));

        var endCommand = new EndConcubinageCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, a, b);
        var result = EndConcubinageCommands.Pipeline.Execute(state, endCommand);

        Assert.That(result.Accepted, Is.True);
        state.RomanticBonds.TryGet(RomanticBondKey.Create(a, b), out var bond);
        Assert.That(bond.BondType, Is.EqualTo(RomanticBondType.PastRelationship));
    }

    [Test]
    public void EndingConcubinageWithoutAnActiveOneIsRejected()
    {
        var state = NewState();
        var a = AddAdult(state);
        var b = AddAdult(state);

        var command = new EndConcubinageCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, a, b);
        var result = EndConcubinageCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(EndConcubinageCommands.NoActiveConcubinage));
    }

    [Test]
    public void EstablishingConcubinageASecondTimeIsRejectedAsADuplicate()
    {
        var state = NewState();
        var a = AddAdult(state);
        var b = AddAdult(state);
        EstablishConcubinageCommands.Pipeline.Execute(
            state, new EstablishConcubinageCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, a, b));

        var command = new EstablishConcubinageCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, a, b);
        var result = EstablishConcubinageCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(EstablishConcubinageCommands.AlreadyConcubines));
    }
}
