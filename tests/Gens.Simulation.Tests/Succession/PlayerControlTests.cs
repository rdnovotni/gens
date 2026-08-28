using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Stewardship;
using Gens.Simulation.Succession;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Succession;

/// <summary>Phase 11 item 2's player-character handoff: <see cref="EstablishPlayerControlCommand"/>,
/// <see cref="RegencySystem"/>'s non-family Regency establishment/end (the gap <see
/// cref="SuccessionHandoffSystem"/>'s minor-heir branch left open), and <see
/// cref="PlayerControlHandoffSystem"/>'s monthly recomputation of who — if anyone — the player
/// directly controls (§6.2).</summary>
public sealed class PlayerControlTests
{
    private static RandomStreamSet Streams(ulong seed)
    {
        var streams = new RandomStreamSet();
        streams.Add(SuccessionHandoffSystem.DisputeTriggerStreamName, seed, 1);
        streams.Add(SuccessionDisputeResolutionSystem.ScoringStreamName, seed, 1);
        streams.Add(SuccessionDisputeResolutionSystem.SplinterStreamName, seed, 1);
        return streams;
    }

    private static RuntimeId<Household> Establish(WorldState state, RuntimeId<Character> headId, GameDate since)
    {
        var householdId = state.HouseholdIds.Issue();
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, headId, since));
        return householdId;
    }

    private static void EstablishPlayerControl(WorldState state, RuntimeId<Household> householdId)
    {
        var command = new EstablishPlayerControlCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, householdId);
        var result = PlayerControlCommands.EstablishPipeline.Execute(state, command);
        Assert.That(result.Accepted, Is.True);
    }

    [Test]
    public void RegencySystemDeclaresTheRelationshipsActorsPhaseAndReadWriteSet()
    {
        var system = new RegencySystem();
        Assert.Multiple(() =>
        {
            Assert.That(system.Phase, Is.EqualTo(TickPhase.RelationshipsActors));
            Assert.That(system.Reads, Is.EquivalentTo(new[] { "householdHeadships", "characters", "stewardshipAssignments" }));
            Assert.That(system.Writes, Is.EquivalentTo(new[]
            {
                "householdHeadships", "stewardshipAssignments", "stewardshipAssignmentIds", "returnReports",
                "returnReportIds", "eventIds", "commandIds", "commandSequence",
            }));
            Assert.That(system.Prerequisites, Is.EquivalentTo(new[] { "succession.handoff", "succession.disputeResolution" }));
        });
    }

    [Test]
    public void PlayerControlHandoffSystemDeclaresTheRelationshipsActorsPhaseAndReadWriteSet()
    {
        var system = new PlayerControlHandoffSystem();
        Assert.Multiple(() =>
        {
            Assert.That(system.Phase, Is.EqualTo(TickPhase.RelationshipsActors));
            Assert.That(system.Reads, Is.EquivalentTo(new[] { "playerControls", "householdHeadships", "stewardshipAssignments" }));
            Assert.That(system.Writes, Is.EquivalentTo(new[] { "playerControls", "eventIds" }));
            Assert.That(system.Prerequisites, Is.EquivalentTo(new[] { "succession.handoff", "succession.disputeResolution", "succession.regency" }));
        });
    }

    [Test]
    public void EstablishingPlayerControlOnAnOrdinaryHouseholdYieldsDirectHead()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId));
        var householdId = Establish(state, headId, new GameDate(0));

        var command = new EstablishPlayerControlCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, householdId);
        var result = PlayerControlCommands.EstablishPipeline.Execute(state, command);

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(result.Events, Has.Count.EqualTo(1));
            Assert.That(result.Events[0], Is.InstanceOf<PlayerControlEstablishedEvent>());

            state.PlayerControls.TryGet(householdId, out var control);
            Assert.That(control.Mode, Is.EqualTo(PlayerControlMode.DirectHead));
            Assert.That(control.ControlledCharacterId, Is.EqualTo(headId));
        });
    }

    [Test]
    public void EstablishingPlayerControlFailsWithoutAHouseholdHeadship()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();

        var command = new EstablishPlayerControlCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, householdId);
        var result = PlayerControlCommands.EstablishPipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(PlayerControlCommands.HouseholdHasNoHead));
    }

    [Test]
    public void EstablishingPlayerControlTwiceFails()
    {
        var state = new WorldState(new GameDate(0));
        var firstHeadId = state.CharacterIds.Issue();
        state.Characters.Add(firstHeadId, CharacterTestFixtures.Minimal(firstHeadId));
        var firstHouseholdId = Establish(state, firstHeadId, new GameDate(0));
        EstablishPlayerControl(state, firstHouseholdId);

        var secondHeadId = state.CharacterIds.Issue();
        state.Characters.Add(secondHeadId, CharacterTestFixtures.Minimal(secondHeadId));
        var secondHouseholdId = Establish(state, secondHeadId, new GameDate(0));

        var command = new EstablishPlayerControlCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, secondHouseholdId);
        var result = PlayerControlCommands.EstablishPipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(PlayerControlCommands.AlreadyEstablished));
    }

    [Test]
    public void OrdinaryHandoffMovesPlayerControlToTheNewAdultHead()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(
            headId, deathRecord: new DeathRecord(new GameDate(1), DeathCause.OldAge, 70)));
        var childId = state.CharacterIds.Issue();
        state.Characters.Add(childId, CharacterTestFixtures.Minimal(childId, fatherId: headId, birthDate: new GameDate(-240)));
        var householdId = Establish(state, headId, new GameDate(0));
        EstablishPlayerControl(state, householdId);

        var context = new MonthlyTickContext(new GameDate(1), Streams(1));
        new SuccessionHandoffSystem().Tick(state, context);
        new RegencySystem().Tick(state, context);
        var events = new PlayerControlHandoffSystem().Tick(state, context);

        Assert.Multiple(() =>
        {
            Assert.That(events, Has.Count.EqualTo(1));
            Assert.That(events[0], Is.InstanceOf<PlayerControlChangedEvent>());
            state.PlayerControls.TryGet(householdId, out var control);
            Assert.That(control.Mode, Is.EqualTo(PlayerControlMode.DirectHead));
            Assert.That(control.ControlledCharacterId, Is.EqualTo(childId));
        });
    }

    [Test]
    public void MinorHeirWithASurvivingSpouseLeavesTheEstateInTrustAndPlayerControlsTheSpouse()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        var spouseId = state.CharacterIds.Issue();
        var childId = state.CharacterIds.Issue();

        state.Characters.Add(headId, CharacterTestFixtures.Minimal(
            headId,
            maritalHistory: new[] { new MarriageRecord(spouseId, new GameDate(-100), new GameDate(1), MarriageEndReason.Death) },
            deathRecord: new DeathRecord(new GameDate(1), DeathCause.OldAge, 70)));
        state.Characters.Add(spouseId, CharacterTestFixtures.Minimal(
            spouseId,
            maritalHistory: new[] { new MarriageRecord(headId, new GameDate(-100), new GameDate(1), MarriageEndReason.Death) }));
        // Minor at tick month 1: age = (1 - (-120)) / 12 = 10.
        state.Characters.Add(childId, CharacterTestFixtures.Minimal(childId, fatherId: headId, birthDate: new GameDate(-120)));

        var householdId = Establish(state, headId, new GameDate(0));
        EstablishPlayerControl(state, householdId);

        var context = new MonthlyTickContext(new GameDate(1), Streams(1));
        new SuccessionHandoffSystem().Tick(state, context);
        var regencyEvents = new RegencySystem().Tick(state, context);
        var controlEvents = new PlayerControlHandoffSystem().Tick(state, context);

        Assert.Multiple(() =>
        {
            state.HouseholdHeadships.TryGet(householdId, out var headship);
            Assert.That(headship.HeadCharacterId, Is.EqualTo(childId));
            Assert.That(headship.RegentCharacterId, Is.EqualTo(spouseId));

            // Spouse-in-trust never creates a StewardshipAssignment (item 1 behavior unchanged).
            Assert.That(regencyEvents, Is.Empty);
            Assert.That(state.StewardshipAssignments.InAscendingOrder(), Is.Empty);

            Assert.That(controlEvents, Has.Count.EqualTo(1));
            state.PlayerControls.TryGet(householdId, out var control);
            Assert.That(control.Mode, Is.EqualTo(PlayerControlMode.RegentInTrust));
            Assert.That(control.ControlledCharacterId, Is.EqualTo(spouseId));
        });
    }

    [Test]
    public void MinorHeirWithNoSpouseButAnEligibleAdultGetsANonFamilyRegent()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(
            headId, deathRecord: new DeathRecord(new GameDate(1), DeathCause.OldAge, 70)));
        // Minor at tick month 1: age = (1 - (-120)) / 12 = 10.
        var childId = state.CharacterIds.Issue();
        state.Characters.Add(childId, CharacterTestFixtures.Minimal(childId, fatherId: headId, birthDate: new GameDate(-120)));
        var householdId = Establish(state, headId, new GameDate(0));

        // Adult household member, high Stewardship — the only Regent candidate.
        var stewardCandidateId = state.CharacterIds.Issue();
        state.Characters.Add(stewardCandidateId, CharacterTestFixtures.Minimal(
            stewardCandidateId, birthDate: new GameDate(-300), household: householdId,
            attributes: new CoreAttributes(10, 10, 80, 10, 10)));

        EstablishPlayerControl(state, householdId);

        var context = new MonthlyTickContext(new GameDate(1), Streams(1));
        new SuccessionHandoffSystem().Tick(state, context);
        var regencyEvents = new RegencySystem().Tick(state, context);
        var controlEvents = new PlayerControlHandoffSystem().Tick(state, context);

        Assert.Multiple(() =>
        {
            state.HouseholdHeadships.TryGet(householdId, out var headship);
            Assert.That(headship.HeadCharacterId, Is.EqualTo(childId));
            Assert.That(headship.RegentCharacterId, Is.EqualTo(stewardCandidateId));

            Assert.That(regencyEvents.Any(e => e is NonFamilyRegencyEstablishedEvent), Is.True);
            var assignment = state.StewardshipAssignments.InAscendingOrder().Single().Value;
            Assert.That(assignment.Context, Is.EqualTo(StewardshipContext.Regency));
            Assert.That(assignment.AppointeeCharacterId, Is.EqualTo(stewardCandidateId));
            Assert.That(assignment.IsActive, Is.True);

            // No change event: EstablishPlayerControl already resolved AutoManaged at t=0, since the
            // fixture head's DeathRecord (needed for SuccessionHandoffSystem to act at all) makes
            // PlayerControlResolver treat it as already un-controllable even before the handoff tick
            // runs — the post-Regency target (AutoManaged, no controlled character) is identical.
            Assert.That(controlEvents, Is.Empty);
            state.PlayerControls.TryGet(householdId, out var control);
            Assert.That(control.Mode, Is.EqualTo(PlayerControlMode.AutoManaged));
            Assert.That(control.ControlledCharacterId, Is.Null);
        });
    }

    [Test]
    public void MinorHeirWithNoSpouseAndNoEligibleAdultLeavesTheHeadshipUngovernedWithoutCrashing()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(
            headId, deathRecord: new DeathRecord(new GameDate(1), DeathCause.OldAge, 70)));
        var childId = state.CharacterIds.Issue();
        state.Characters.Add(childId, CharacterTestFixtures.Minimal(childId, fatherId: headId, birthDate: new GameDate(-120)));
        var householdId = Establish(state, headId, new GameDate(0));
        EstablishPlayerControl(state, householdId);

        var context = new MonthlyTickContext(new GameDate(1), Streams(1));

        Assert.DoesNotThrow(() =>
        {
            new SuccessionHandoffSystem().Tick(state, context);
            new RegencySystem().Tick(state, context);
            new PlayerControlHandoffSystem().Tick(state, context);
        });

        state.HouseholdHeadships.TryGet(householdId, out var headship);
        Assert.That(headship.HeadCharacterId, Is.EqualTo(childId));
        Assert.That(headship.RegentCharacterId, Is.Null);

        state.PlayerControls.TryGet(householdId, out var control);
        Assert.That(control.Mode, Is.EqualTo(PlayerControlMode.DirectHead));
        Assert.That(control.ControlledCharacterId, Is.EqualTo(childId));
    }

    [Test]
    public void RegencyEndsOnceTheHeirComesOfAgeAndPlayerControlReturnsToTheHead()
    {
        var state = new WorldState(new GameDate(0));
        // Already an adult (18) at month 0, so this test isolates the "regency ends" behavior from
        // the earlier "minor becomes head" handoff mechanics exercised by the other tests above.
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, birthDate: new GameDate(-216)));
        var regentId = state.CharacterIds.Issue();
        state.Characters.Add(regentId, CharacterTestFixtures.Minimal(regentId, birthDate: new GameDate(-300)));

        var householdId = state.HouseholdIds.Issue();
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, headId, new GameDate(0), regentId));
        EstablishPlayerControl(state, householdId);

        var appoint = new AppointStewardshipCommand(
            state.CommandIds.Issue(), "system", new GameDate(0), null, householdId, StewardshipContext.Regency,
            StewardshipMode.SingleSteward, regentId, null, null, StewardshipAssignment.DefaultAutonomyLevel);
        var appointResult = StewardshipCommands.AppointPipeline.Execute(state, appoint);
        Assert.That(appointResult.Accepted, Is.True);

        // PlayerControls starts out mid-Regency, matching the state RegencySystem would have left it in.
        state.PlayerControls.Remove(householdId);
        state.PlayerControls.Add(householdId, new PlayerControlState(householdId, ControlledCharacterId: null, PlayerControlMode.AutoManaged));

        var context = new MonthlyTickContext(new GameDate(1), Streams(1));
        var regencyEvents = new RegencySystem().Tick(state, context);
        var controlEvents = new PlayerControlHandoffSystem().Tick(state, context);

        Assert.Multiple(() =>
        {
            Assert.That(regencyEvents.Any(e => e is RegencyEndedEvent), Is.True);
            state.HouseholdHeadships.TryGet(householdId, out var headship);
            Assert.That(headship.RegentCharacterId, Is.Null);

            state.StewardshipAssignments.TryGet(appointResult.Events.OfType<StewardshipAssignedEvent>().Single().AssignmentId, out var assignment);
            Assert.That(assignment.IsActive, Is.False);

            Assert.That(controlEvents.Any(e => e is PlayerControlChangedEvent), Is.True);
            state.PlayerControls.TryGet(householdId, out var control);
            Assert.That(control.Mode, Is.EqualTo(PlayerControlMode.DirectHead));
            Assert.That(control.ControlledCharacterId, Is.EqualTo(headId));
        });
    }

    [Test]
    public void ExtinctionIsReflectedAsExtinguishedWithNoControlledCharacter()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(
            headId, deathRecord: new DeathRecord(new GameDate(1), DeathCause.OldAge, 70)));
        var householdId = Establish(state, headId, new GameDate(0));
        EstablishPlayerControl(state, householdId);

        var context = new MonthlyTickContext(new GameDate(1), Streams(1));
        new SuccessionHandoffSystem().Tick(state, context);
        new RegencySystem().Tick(state, context);
        var events = new PlayerControlHandoffSystem().Tick(state, context);

        Assert.Multiple(() =>
        {
            Assert.That(state.HouseholdHeadships.TryGet(householdId, out _), Is.False);
            Assert.That(events, Has.Count.EqualTo(1));
            Assert.That(events[0], Is.InstanceOf<PlayerControlChangedEvent>());

            state.PlayerControls.TryGet(householdId, out var control);
            Assert.That(control.Mode, Is.EqualTo(PlayerControlMode.Extinguished));
            Assert.That(control.ControlledCharacterId, Is.Null);
        });
    }

    [Test]
    public void PlayerControlsRoundTripThroughTheDtoAndStateHash()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId));
        var householdId = Establish(state, headId, new GameDate(0));
        EstablishPlayerControl(state, householdId);

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.PlayerControls.TryGet(householdId, out var control), Is.True);
            Assert.That(control!.HouseholdId, Is.EqualTo(householdId));
            Assert.That(control.ControlledCharacterId, Is.EqualTo(headId));
            Assert.That(control.Mode, Is.EqualTo(PlayerControlMode.DirectHead));
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        });
    }

    [Test]
    public void DeadHeadPendingADisputeFallsBackToAutoManagedInsteadOfControllingTheDeceased()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId));
        var householdId = Establish(state, headId, new GameDate(0));
        EstablishPlayerControl(state, householdId);

        state.PlayerControls.TryGet(householdId, out var initial);
        Assert.That(initial!.Mode, Is.EqualTo(PlayerControlMode.DirectHead));

        // Simulate what SuccessionHandoffSystem deliberately leaves in place while a SuccessionDispute
        // is Pending: the head is dead, but headship.HeadCharacterId still points at them
        // (RegentCharacterId stays null) for the whole multi-month dispute window.
        state.Characters.Remove(headId);
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(
            headId, deathRecord: new DeathRecord(new GameDate(1), DeathCause.OldAge, 70)));

        var context = new MonthlyTickContext(new GameDate(1), Streams(1));
        var events = new PlayerControlHandoffSystem().Tick(state, context);

        Assert.Multiple(() =>
        {
            Assert.That(events, Has.Count.EqualTo(1));
            state.PlayerControls.TryGet(householdId, out var control);
            Assert.That(control!.Mode, Is.EqualTo(PlayerControlMode.AutoManaged));
            Assert.That(control.ControlledCharacterId, Is.Null);
        });
    }

    [Test]
    public void ARegentsDeathEndsTheRegencyEvenThoughTheHeirIsStillAMinor()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        // Still a minor at month 1: age = (1 - (-120)) / 12 = 10.
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, birthDate: new GameDate(-120)));
        var regentId = state.CharacterIds.Issue();
        state.Characters.Add(regentId, CharacterTestFixtures.Minimal(
            regentId, birthDate: new GameDate(-300),
            deathRecord: new DeathRecord(new GameDate(1), DeathCause.OldAge, 70)));

        var householdId = state.HouseholdIds.Issue();
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, headId, new GameDate(0), regentId));
        EstablishPlayerControl(state, householdId);

        var appoint = new AppointStewardshipCommand(
            state.CommandIds.Issue(), "system", new GameDate(0), null, householdId, StewardshipContext.Regency,
            StewardshipMode.SingleSteward, regentId, null, null, StewardshipAssignment.DefaultAutonomyLevel);
        var appointResult = StewardshipCommands.AppointPipeline.Execute(state, appoint);
        Assert.That(appointResult.Accepted, Is.True);

        state.PlayerControls.Remove(householdId);
        state.PlayerControls.Add(householdId, new PlayerControlState(householdId, ControlledCharacterId: null, PlayerControlMode.AutoManaged));

        var context = new MonthlyTickContext(new GameDate(1), Streams(1));
        var regencyEvents = new RegencySystem().Tick(state, context);

        Assert.Multiple(() =>
        {
            var ended = regencyEvents.OfType<RegencyEndedEvent>().Single();
            Assert.That(ended.Reason, Is.EqualTo(RegencyEndReason.RegentDied));

            state.HouseholdHeadships.TryGet(householdId, out var headship);
            Assert.That(headship.RegentCharacterId, Is.Null);
            Assert.That(headship.HeadCharacterId, Is.EqualTo(headId));

            state.StewardshipAssignments.TryGet(appointResult.Events.OfType<StewardshipAssignedEvent>().Single().AssignmentId, out var assignment);
            Assert.That(assignment!.IsActive, Is.False);
        });
    }

    [Test]
    public void ANewRegencySupersedesAnAlreadyActiveTravelAssignment()
    {
        var state = new WorldState(new GameDate(0));
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(
            headId, deathRecord: new DeathRecord(new GameDate(1), DeathCause.OldAge, 70)));
        // Minor at tick month 1: age = (1 - (-120)) / 12 = 10.
        var childId = state.CharacterIds.Issue();
        state.Characters.Add(childId, CharacterTestFixtures.Minimal(childId, fatherId: headId, birthDate: new GameDate(-120)));
        var householdId = Establish(state, headId, new GameDate(0));

        var stewardCandidateId = state.CharacterIds.Issue();
        state.Characters.Add(stewardCandidateId, CharacterTestFixtures.Minimal(
            stewardCandidateId, birthDate: new GameDate(-300), household: householdId,
            attributes: new CoreAttributes(10, 10, 80, 10, 10)));

        // The head was already away on Travel when they died — a real StewardshipAssignment already
        // active for this household before the Regency need arises.
        var travelAppointeeId = state.CharacterIds.Issue();
        state.Characters.Add(travelAppointeeId, CharacterTestFixtures.Minimal(travelAppointeeId, household: householdId));
        var travelAppoint = new AppointStewardshipCommand(
            state.CommandIds.Issue(), "system", new GameDate(0), null, householdId, StewardshipContext.Travel,
            StewardshipMode.SingleSteward, travelAppointeeId, null, null, StewardshipAssignment.DefaultAutonomyLevel);
        var travelResult = StewardshipCommands.AppointPipeline.Execute(state, travelAppoint);
        Assert.That(travelResult.Accepted, Is.True);
        var travelAssignmentId = travelResult.Events.OfType<StewardshipAssignedEvent>().Single().AssignmentId;

        var context = new MonthlyTickContext(new GameDate(1), Streams(1));
        new SuccessionHandoffSystem().Tick(state, context);
        var regencyEvents = new RegencySystem().Tick(state, context);

        Assert.Multiple(() =>
        {
            state.StewardshipAssignments.TryGet(travelAssignmentId, out var travelAssignment);
            Assert.That(travelAssignment!.IsActive, Is.False, "the superseded Travel assignment should be ended, not left dangling");

            Assert.That(regencyEvents.Any(e => e is NonFamilyRegencyEstablishedEvent), Is.True);
            state.HouseholdHeadships.TryGet(householdId, out var headship);
            Assert.That(headship.RegentCharacterId, Is.EqualTo(stewardCandidateId));

            var regencyAssignments = state.StewardshipAssignments.InAscendingOrder()
                .Where(entry => entry.Value.HouseholdId == householdId && entry.Value.Context == StewardshipContext.Regency)
                .ToArray();
            Assert.That(regencyAssignments, Has.Length.EqualTo(1));
            Assert.That(regencyAssignments[0].Value.IsActive, Is.True);
        });
    }

    [Test]
    public void AnOrphanedRegencyAssignmentIsEndedOnceItsHouseholdGoesExtinct()
    {
        var state = new WorldState(new GameDate(0));
        var minorHeadId = state.CharacterIds.Issue();
        // Still a minor at month 1, and dies this same month with no heir at all.
        state.Characters.Add(minorHeadId, CharacterTestFixtures.Minimal(
            minorHeadId, birthDate: new GameDate(-120), deathRecord: new DeathRecord(new GameDate(1), DeathCause.OldAge, 70)));
        var regentId = state.CharacterIds.Issue();
        state.Characters.Add(regentId, CharacterTestFixtures.Minimal(regentId, birthDate: new GameDate(-300)));

        var householdId = state.HouseholdIds.Issue();
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, minorHeadId, new GameDate(0), regentId));

        var appoint = new AppointStewardshipCommand(
            state.CommandIds.Issue(), "system", new GameDate(0), null, householdId, StewardshipContext.Regency,
            StewardshipMode.SingleSteward, regentId, null, null, StewardshipAssignment.DefaultAutonomyLevel);
        var appointResult = StewardshipCommands.AppointPipeline.Execute(state, appoint);
        Assert.That(appointResult.Accepted, Is.True);
        var regencyAssignmentId = appointResult.Events.OfType<StewardshipAssignedEvent>().Single().AssignmentId;

        var context = new MonthlyTickContext(new GameDate(1), Streams(1));
        // The minor head dying with no eligible heir and no surviving spouse extinguishes the
        // Household outright (item 1 behavior) — this system never sees it again in its own
        // headship-keyed loop, since the headship record is gone.
        new SuccessionHandoffSystem().Tick(state, context);
        Assert.That(state.HouseholdHeadships.TryGet(householdId, out _), Is.False);

        var regencyEvents = new RegencySystem().Tick(state, context);

        Assert.Multiple(() =>
        {
            state.StewardshipAssignments.TryGet(regencyAssignmentId, out var assignment);
            Assert.That(assignment!.IsActive, Is.False);
            Assert.That(regencyEvents.Any(e => e is StewardshipEndedEvent), Is.True);
        });
    }
}
