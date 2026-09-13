using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Education;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Random;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Education;

/// <summary>Phase 17 item 2 coverage, slice 3: Educational Tracks (§3) and Culture drift (§2).</summary>
public sealed class PedagogyTests
{
    private static readonly GameDate AdolescentDate = new(180);

    private static (WorldState State, RuntimeId<Household> HouseholdId, RuntimeId<Character> CharacterId) AdolescentInAHousehold()
    {
        var state = new WorldState(AdolescentDate);
        var householdId = state.HouseholdIds.Issue();
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId, nomen: "Cornelius", household: householdId));
        return (state, householdId, characterId);
    }

    private static void Fund(WorldState state, RuntimeId<Household> householdId, Money amount)
    {
        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Treasury,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), amount),
                new LedgerPosting(new LedgerAccountKey(LedgerAccountKind.System, "test:seed"), -amount),
            });
    }

    // ---- StartEducationalTrackCommand ---------------------------------------------------------

    [Test]
    public void StartEducationalTrackCommandEnrollsAnAdolescent()
    {
        var (state, _, characterId) = AdolescentInAHousehold();

        var result = StartEducationalTrackCommands.Pipeline.Execute(
            state,
            new StartEducationalTrackCommand(
                state.CommandIds.Issue(), "player", AdolescentDate, null, characterId, KnownEducationTracks.Rhetoric));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(EducationalTrackEnrollmentResolver.TryGet(state, characterId, out var enrollment), Is.True);
            Assert.That(enrollment.TrackId, Is.EqualTo(KnownEducationTracks.Rhetoric));
        });
    }

    [Test]
    public void StartEducationalTrackCommandRejectsANonAdolescentAnAlreadyEnrolledCharacterOrAnUnknownTrack()
    {
        var (state, _, characterId) = AdolescentInAHousehold();

        var adultId = state.CharacterIds.Issue();
        state.Characters.Add(
            adultId, CharacterTestFixtures.Minimal(adultId, nomen: "Adult", household: null, birthDate: new GameDate(-360)));
        Assert.That(
            StartEducationalTrackCommands.Pipeline.Execute(
                state,
                new StartEducationalTrackCommand(state.CommandIds.Issue(), "player", AdolescentDate, null, adultId, KnownEducationTracks.Rhetoric))
                .Error,
            Is.EqualTo(StartEducationalTrackCommands.NotAdolescent));

        Assert.That(
            StartEducationalTrackCommands.Pipeline.Execute(
                state,
                new StartEducationalTrackCommand(
                    state.CommandIds.Issue(), "player", AdolescentDate, null, characterId, new DefinitionId<EducationTrack>("bogus")))
                .Error,
            Is.EqualTo(StartEducationalTrackCommands.UnknownTrack));

        StartEducationalTrackCommands.Pipeline.Execute(
            state,
            new StartEducationalTrackCommand(
                state.CommandIds.Issue(), "player", AdolescentDate, null, characterId, KnownEducationTracks.Rhetoric));
        Assert.That(
            StartEducationalTrackCommands.Pipeline.Execute(
                state,
                new StartEducationalTrackCommand(
                    state.CommandIds.Issue(), "player", AdolescentDate, null, characterId, KnownEducationTracks.Philosophy))
                .Error,
            Is.EqualTo(StartEducationalTrackCommands.AlreadyEnrolled));
    }

    // ---- EducationalTrackProgressSystem --------------------------------------------------------

    [Test]
    public void EducationalTrackProgressSystemAppliesClampedAttributeGainsAndPrestigeForPhilosophy()
    {
        var (state, householdId, characterId) = AdolescentInAHousehold();
        StartEducationalTrackCommands.Pipeline.Execute(
            state,
            new StartEducationalTrackCommand(
                state.CommandIds.Issue(), "player", AdolescentDate, null, characterId, KnownEducationTracks.Philosophy));
        state.Characters.TryGet(characterId, out var before);
        var learningBefore = before!.Attributes.Learning;

        new EducationalTrackProgressSystem().Tick(state, new MonthlyTickContext(new GameDate(AdolescentDate.TotalMonths + 1), new RandomStreamSet()));

        state.Characters.TryGet(characterId, out var after);
        Assert.Multiple(() =>
        {
            Assert.That(after!.Attributes.Learning, Is.EqualTo(learningBefore + KnownEducationTracks.Catalog.Get(KnownEducationTracks.Philosophy).MonthlyAttributeGain));
            Assert.That(
                CulturalPrestigeResolver.Current(state, householdId),
                Is.EqualTo(KnownEducationTracks.Catalog.Get(KnownEducationTracks.Philosophy).MonthlyPrestigeGain));
        });
    }

    [Test]
    public void EducationalTrackProgressSystemClampsAtOneHundredAndCompletesAfterCompletionMonths()
    {
        var (state, _, characterId) = AdolescentInAHousehold();
        state.Characters.TryGet(characterId, out var character);
        state.Characters.Remove(characterId);
        state.Characters.Add(characterId, character! with { Attributes = new CoreAttributes(100, 10, 10, 10, 10) });

        StartEducationalTrackCommands.Pipeline.Execute(
            state,
            new StartEducationalTrackCommand(
                state.CommandIds.Issue(), "player", AdolescentDate, null, characterId, KnownEducationTracks.Rhetoric));

        var track = KnownEducationTracks.Catalog.Get(KnownEducationTracks.Rhetoric);
        var system = new EducationalTrackProgressSystem();
        for (var i = 1; i <= track.CompletionMonths; i++)
            system.Tick(state, new MonthlyTickContext(new GameDate(AdolescentDate.TotalMonths + i), new RandomStreamSet()));

        state.Characters.TryGet(characterId, out var completedCharacter);
        Assert.Multiple(() =>
        {
            Assert.That(completedCharacter!.Attributes.Diplomacy, Is.EqualTo(100));
            Assert.That(EducationalTrackEnrollmentResolver.TryGet(state, characterId, out var enrollment), Is.True);
            Assert.That(enrollment.CompletedDate, Is.Not.Null);
            Assert.That(EducationalTrackEnrollmentResolver.HasCompleted(state, characterId, KnownEducationTracks.Rhetoric), Is.True);
        });
    }

    // ---- PurchaseDistinguishedTutorCommand ------------------------------------------------------

    [Test]
    public void PurchaseDistinguishedTutorCommandUpgradesTheActiveEnrollmentAndDrawsTheLedger()
    {
        var (state, householdId, characterId) = AdolescentInAHousehold();
        Fund(state, householdId, Money.FromDenarii(100));
        StartEducationalTrackCommands.Pipeline.Execute(
            state,
            new StartEducationalTrackCommand(
                state.CommandIds.Issue(), "player", AdolescentDate, null, characterId, KnownEducationTracks.Rhetoric));

        var result = PurchaseDistinguishedTutorCommands.Pipeline.Execute(
            state, new PurchaseDistinguishedTutorCommand(state.CommandIds.Issue(), "player", AdolescentDate, null, householdId, characterId));

        var track = KnownEducationTracks.Catalog.Get(KnownEducationTracks.Rhetoric);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            EducationalTrackEnrollmentResolver.TryGet(state, characterId, out var enrollment);
            Assert.That(enrollment.DistinguishedTierActive, Is.True);
            Assert.That(state.DistinguishedEducationInvestments.Count, Is.EqualTo(1));
            var account = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var acc) ? acc!.Balance : Money.Zero;
            Assert.That(account, Is.EqualTo(Money.FromDenarii(100) - track.DistinguishedTierCostPerMonth));
        });
    }

    [Test]
    public void PurchaseDistinguishedTutorCommandRejectsWithNoActiveEnrollmentAlreadyDistinguishedOrInsufficientTreasury()
    {
        var (state, householdId, characterId) = AdolescentInAHousehold();

        Assert.That(
            PurchaseDistinguishedTutorCommands.Pipeline.Execute(
                state, new PurchaseDistinguishedTutorCommand(state.CommandIds.Issue(), "player", AdolescentDate, null, householdId, characterId))
                .Error,
            Is.EqualTo(PurchaseDistinguishedTutorCommands.NoActiveEnrollment));

        StartEducationalTrackCommands.Pipeline.Execute(
            state,
            new StartEducationalTrackCommand(
                state.CommandIds.Issue(), "player", AdolescentDate, null, characterId, KnownEducationTracks.Rhetoric));
        Assert.That(
            PurchaseDistinguishedTutorCommands.Pipeline.Execute(
                state, new PurchaseDistinguishedTutorCommand(state.CommandIds.Issue(), "player", AdolescentDate, null, householdId, characterId))
                .Error,
            Is.EqualTo(PurchaseDistinguishedTutorCommands.InsufficientTreasury));

        Fund(state, householdId, Money.FromDenarii(100));
        PurchaseDistinguishedTutorCommands.Pipeline.Execute(
            state, new PurchaseDistinguishedTutorCommand(state.CommandIds.Issue(), "player", AdolescentDate, null, householdId, characterId));
        Assert.That(
            PurchaseDistinguishedTutorCommands.Pipeline.Execute(
                state, new PurchaseDistinguishedTutorCommand(state.CommandIds.Issue(), "player", AdolescentDate, null, householdId, characterId))
                .Error,
            Is.EqualTo(PurchaseDistinguishedTutorCommands.AlreadyDistinguished));
    }

    // ---- AssignEducationRoleCommand -------------------------------------------------------------

    [Test]
    public void AssignEducationRoleCommandAssignsAForeignTutorEvenOutsideTheHousehold()
    {
        var (state, householdId, _) = AdolescentInAHousehold();
        var tutorId = state.CharacterIds.Issue();
        state.Characters.Add(tutorId, CharacterTestFixtures.Minimal(tutorId, nomen: "Xenon", household: null));

        var result = AssignEducationRoleCommands.Pipeline.Execute(
            state, new AssignEducationRoleCommand(state.CommandIds.Issue(), "player", AdolescentDate, null, householdId, tutorId, EducationRole.ForeignTutor));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(state.EducationRoleAssignments.TryGet(householdId, out var assignment), Is.True);
            Assert.That(assignment!.Role, Is.EqualTo(EducationRole.ForeignTutor));
        });
    }

    [Test]
    public void AssignEducationRoleCommandRejectsADeceasedTutor()
    {
        var (state, householdId, _) = AdolescentInAHousehold();
        var tutorId = state.CharacterIds.Issue();
        state.Characters.Add(
            tutorId,
            CharacterTestFixtures.Minimal(tutorId, nomen: "Dead", household: null, deathRecord: new DeathRecord(AdolescentDate, DeathCause.OldAge, 60)));

        var result = AssignEducationRoleCommands.Pipeline.Execute(
            state, new AssignEducationRoleCommand(state.CommandIds.Issue(), "player", AdolescentDate, null, householdId, tutorId, EducationRole.PrivateTutor));

        Assert.That(result.Error, Is.EqualTo(AssignEducationRoleCommands.TutorDeceased));
    }

    // ---- Culture drift ---------------------------------------------------------------------------

    [Test]
    public void SetCulturalDriftTargetCommandStartsProgressAtZero()
    {
        var (state, _, characterId) = AdolescentInAHousehold();
        var targetCulture = new DefinitionId<Culture>("gallic");

        var result = SetCulturalDriftTargetCommands.Pipeline.Execute(
            state, new SetCulturalDriftTargetCommand(state.CommandIds.Issue(), "player", AdolescentDate, null, characterId, targetCulture));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(CulturalDriftResolver.TryGet(state, characterId, out var drift), Is.True);
            Assert.That(drift.ProgressMonths, Is.EqualTo(0));
            Assert.That(drift.TargetCultureId, Is.EqualTo(targetCulture));
        });
    }

    [Test]
    public void SetCulturalDriftTargetCommandRejectsTheCharactersOwnCurrentCulture()
    {
        var (state, _, characterId) = AdolescentInAHousehold();
        state.Characters.TryGet(characterId, out var character);

        var result = SetCulturalDriftTargetCommands.Pipeline.Execute(
            state, new SetCulturalDriftTargetCommand(state.CommandIds.Issue(), "player", AdolescentDate, null, characterId, character!.Culture));

        Assert.That(result.Error, Is.EqualTo(SetCulturalDriftTargetCommands.AlreadyThatCulture));
    }

    [Test]
    public void CulturalDriftSystemCrossesTheThresholdAndReassignsCultureFastForAnAdolescent()
    {
        var (state, _, characterId) = AdolescentInAHousehold();
        var targetCulture = new DefinitionId<Culture>("gallic");
        SetCulturalDriftTargetCommands.Pipeline.Execute(
            state, new SetCulturalDriftTargetCommand(state.CommandIds.Issue(), "player", AdolescentDate, null, characterId, targetCulture));

        var system = new CulturalDriftSystem();
        var monthsNeeded = (EducationCulturalDriftCatalog.DriftThresholdMonths / EducationCulturalDriftCatalog.FastDriftMonthsPerMonth) + 1;
        var events = new List<IDomainEvent>();
        for (var i = 1; i <= monthsNeeded; i++)
            events.AddRange(system.Tick(state, new MonthlyTickContext(new GameDate(AdolescentDate.TotalMonths + i), new RandomStreamSet())));

        state.Characters.TryGet(characterId, out var character);
        Assert.Multiple(() =>
        {
            Assert.That(character!.Culture, Is.EqualTo(targetCulture));
            Assert.That(CulturalDriftResolver.TryGet(state, characterId, out _), Is.False);
            Assert.That(events, Has.Some.InstanceOf<CultureDriftedEvent>());
        });
    }

    [Test]
    public void CulturalDriftSystemAcceleratesWithAMatchingActiveForeignTutor()
    {
        var (state, householdId, characterId) = AdolescentInAHousehold();
        var targetCulture = new DefinitionId<Culture>("gallic");
        var tutorId = state.CharacterIds.Issue();
        state.Characters.Add(tutorId, CharacterTestFixtures.Minimal(tutorId, nomen: "Divico", household: null) with { Culture = targetCulture });
        AssignEducationRoleCommands.Pipeline.Execute(
            state, new AssignEducationRoleCommand(state.CommandIds.Issue(), "player", AdolescentDate, null, householdId, tutorId, EducationRole.ForeignTutor));
        SetCulturalDriftTargetCommands.Pipeline.Execute(
            state, new SetCulturalDriftTargetCommand(state.CommandIds.Issue(), "player", AdolescentDate, null, characterId, targetCulture));

        new CulturalDriftSystem().Tick(state, new MonthlyTickContext(new GameDate(AdolescentDate.TotalMonths + 1), new RandomStreamSet()));

        CulturalDriftResolver.TryGet(state, characterId, out var drift);
        Assert.That(
            drift.ProgressMonths,
            Is.EqualTo(EducationCulturalDriftCatalog.FastDriftMonthsPerMonth * EducationCulturalDriftCatalog.ForeignTutorAccelerationMultiplier));
    }

    // ---- Save round trip & determinism --------------------------------------------------------

    [Test]
    public void PedagogyStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, householdId, characterId) = AdolescentInAHousehold();
        Fund(state, householdId, Money.FromDenarii(200));
        StartEducationalTrackCommands.Pipeline.Execute(
            state,
            new StartEducationalTrackCommand(
                state.CommandIds.Issue(), "player", AdolescentDate, null, characterId, KnownEducationTracks.Philosophy));
        PurchaseDistinguishedTutorCommands.Pipeline.Execute(
            state, new PurchaseDistinguishedTutorCommand(state.CommandIds.Issue(), "player", AdolescentDate, null, householdId, characterId));
        new EducationalTrackProgressSystem().Tick(state, new MonthlyTickContext(new GameDate(AdolescentDate.TotalMonths + 1), new RandomStreamSet()));

        var tutorId = state.CharacterIds.Issue();
        state.Characters.Add(tutorId, CharacterTestFixtures.Minimal(tutorId, nomen: "Xenon", household: null));
        AssignEducationRoleCommands.Pipeline.Execute(
            state, new AssignEducationRoleCommand(state.CommandIds.Issue(), "player", AdolescentDate, null, householdId, tutorId, EducationRole.ForeignTutor));

        var targetCulture = new DefinitionId<Culture>("gallic");
        SetCulturalDriftTargetCommands.Pipeline.Execute(
            state, new SetCulturalDriftTargetCommand(state.CommandIds.Issue(), "player", AdolescentDate, null, characterId, targetCulture));
        new CulturalDriftSystem().Tick(state, new MonthlyTickContext(new GameDate(AdolescentDate.TotalMonths + 1), new RandomStreamSet()));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.EducationalTrackEnrollments.Count, Is.EqualTo(1));
            Assert.That(restored.DistinguishedEducationInvestments.Count, Is.EqualTo(1));
            Assert.That(restored.EducationRoleAssignments.Count, Is.EqualTo(1));
            Assert.That(restored.CulturalDriftStates.Count, Is.EqualTo(1));
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        });
    }
}
