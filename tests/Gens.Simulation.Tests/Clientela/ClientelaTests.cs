using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Clientela;
using Gens.Simulation.Commands;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
using Gens.Simulation.Reputation;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Clientela;

/// <summary>Phase 12 item 2 coverage: Clientela roster membership, the favor call-in's §4.2 opinion
/// cost, Influence generation/decay (§4.4), the Salutatio (§4.3), poaching (§4.5), and Character
/// Faction (§3.1).</summary>
public sealed class ClientelaTests
{
    private static (WorldState State, RuntimeId<Household> PatronHouseholdId, RuntimeId<Character> PatronHeadId, RuntimeId<Character> ClientId) PatronAndClient()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, nomen: "Cornelius", household: householdId));
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, headId, new GameDate(0)));

        var clientId = state.CharacterIds.Issue();
        state.Characters.Add(clientId, CharacterTestFixtures.Minimal(clientId, nomen: "Aurelius"));

        return (state, householdId, headId, clientId);
    }

    private static void Recruit(WorldState state, RuntimeId<Household> patronHouseholdId, RuntimeId<Character> clientId, GameDate date) =>
        RecruitClientCommands.Pipeline.Execute(
            state,
            new RecruitClientCommand(state.CommandIds.Issue(), "player", date, null, patronHouseholdId, clientId, ClientSpecialty.Legal));

    /// <summary>Overwrites the opinion half of an already-existing directed <see cref="Relationship"/>
    /// (created by <see cref="RecruitClientCommand"/>'s own bond-forming write), keeping its bonds —
    /// test-only plumbing standing in for a real "opinion moved by an interaction" event this suite
    /// doesn't need to model.</summary>
    private static void SetOpinion(WorldState state, RuntimeId<Character> fromId, RuntimeId<Character> toId, int opinion)
    {
        var key = new RelationshipKey(fromId, toId);
        state.Relationships.TryGet(key, out var existing);
        state.Relationships.Remove(key);
        state.Relationships.Add(key, new Relationship(
            opinion, existing.Bonds, existing.Origin, existing.FormedDate, existing.LastMeaningfulInteractionDate, existing.ProvenanceEventId));
    }

    [Test]
    public void RecruitClientCommandCreatesTheRosterEntryAndBothRelationshipBonds()
    {
        var (state, patronHouseholdId, patronHeadId, clientId) = PatronAndClient();

        var result = RecruitClientCommands.Pipeline.Execute(
            state, new RecruitClientCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, patronHouseholdId, clientId, ClientSpecialty.Legal));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(ClientelaResolver.TryGetClient(state, clientId, out var entry), Is.True);
            Assert.That(entry!.Specialty, Is.EqualTo(ClientSpecialty.Legal));
            Assert.That(entry.PatronHouseholdId, Is.EqualTo(patronHouseholdId));

            state.Relationships.TryGet(new RelationshipKey(patronHeadId, clientId), out var fromPatron);
            Assert.That(fromPatron.HasBond(BondTag.Client), Is.True);
            state.Relationships.TryGet(new RelationshipKey(clientId, patronHeadId), out var fromClient);
            Assert.That(fromClient.HasBond(BondTag.Patron), Is.True);

            var recruited = (ClientRecruitedEvent)result.Events[0];
            Assert.That(recruited.Visibility.ObserverIds, Is.EquivalentTo(new[] { patronHeadId.ToTaggedString(), clientId.ToTaggedString() }));
        });
    }

    [Test]
    public void RecruitClientCommandRejectsAMissingHeadUnknownDeceasedSelfOrDuplicateClient()
    {
        var (state, patronHouseholdId, patronHeadId, clientId) = PatronAndClient();
        var strangerHouseholdId = state.HouseholdIds.Issue();

        Assert.Multiple(() =>
        {
            Assert.That(
                RecruitClientCommands.Pipeline.Execute(
                    state, new RecruitClientCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, strangerHouseholdId, clientId, ClientSpecialty.Legal)).Error,
                Is.EqualTo(RecruitClientCommands.PatronHasNoHead));

            var unknownClientId = state.CharacterIds.Issue();
            Assert.That(
                RecruitClientCommands.Pipeline.Execute(
                    state, new RecruitClientCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, patronHouseholdId, unknownClientId, ClientSpecialty.Legal)).Error,
                Is.EqualTo(RecruitClientCommands.ClientNotFound));

            Assert.That(
                RecruitClientCommands.Pipeline.Execute(
                    state, new RecruitClientCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, patronHouseholdId, patronHeadId, ClientSpecialty.Legal)).Error,
                Is.EqualTo(RecruitClientCommands.SelfPatronage));

            Recruit(state, patronHouseholdId, clientId, new GameDate(1));
            Assert.That(
                RecruitClientCommands.Pipeline.Execute(
                    state, new RecruitClientCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, patronHouseholdId, clientId, ClientSpecialty.Mercantile)).Error,
                Is.EqualTo(RecruitClientCommands.AlreadyAClient));
        });
    }

    [Test]
    public void DismissClientCommandRemovesTheEntryAndBreaksTheBonds()
    {
        var (state, patronHouseholdId, patronHeadId, clientId) = PatronAndClient();
        Recruit(state, patronHouseholdId, clientId, new GameDate(1));

        var result = DismissClientCommands.Pipeline.Execute(
            state, new DismissClientCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, clientId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(ClientelaResolver.TryGetClient(state, clientId, out _), Is.False);
            state.Relationships.TryGet(new RelationshipKey(patronHeadId, clientId), out var fromPatron);
            Assert.That(fromPatron.HasBond(BondTag.Client), Is.False);
        });
    }

    [Test]
    public void DismissClientCommandRejectsANonClient()
    {
        var (state, _, _, clientId) = PatronAndClient();
        Assert.That(
            DismissClientCommands.Pipeline.Execute(
                state, new DismissClientCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, clientId)).Error,
            Is.EqualTo(DismissClientCommands.NotAClient));
    }

    [Test]
    public void CallInClientFavorCommandOpensAndSettlesAFavorWithNoOpinionCostWhenSpacedOut()
    {
        var (state, patronHouseholdId, patronHeadId, clientId) = PatronAndClient();
        Recruit(state, patronHouseholdId, clientId, new GameDate(0));

        var result = CallInClientFavorCommands.Pipeline.Execute(
            state, new CallInClientFavorCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, patronHouseholdId, clientId, "vouched at the Curia"));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(state.FavorObligations.Count, Is.EqualTo(1));
            var favor = state.FavorObligations.InAscendingOrder().First().Value;
            Assert.That(favor.Status, Is.EqualTo(FavorStatus.Repaid));
            Assert.That(favor.GrantorId, Is.EqualTo(clientId));
            Assert.That(favor.BeneficiaryId, Is.EqualTo(patronHeadId));

            var calledIn = result.Events.OfType<ClientFavorCalledInEvent>().Single();
            Assert.That(calledIn.Overdrawn, Is.False);
            Assert.That(calledIn.OpinionDelta, Is.EqualTo(0));

            ClientelaResolver.TryGetClient(state, clientId, out var entry);
            Assert.That(entry!.LastFavorCalledDate, Is.EqualTo(new GameDate(1)));
        });
    }

    [Test]
    public void CallInClientFavorCommandCostsOpinionWhenCalledTooSoon()
    {
        var (state, patronHouseholdId, patronHeadId, clientId) = PatronAndClient();
        Recruit(state, patronHouseholdId, clientId, new GameDate(0));

        CallInClientFavorCommands.Pipeline.Execute(
            state, new CallInClientFavorCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, patronHouseholdId, clientId, "a favor"));

        var secondResult = CallInClientFavorCommands.Pipeline.Execute(
            state,
            new CallInClientFavorCommand(
                state.CommandIds.Issue(), "player", new GameDate(1 + ClientelaCatalog.FavorCooldownMonths - 1), null,
                patronHouseholdId, clientId, "another favor so soon"));

        Assert.Multiple(() =>
        {
            var calledIn = secondResult.Events.OfType<ClientFavorCalledInEvent>().Single();
            Assert.That(calledIn.Overdrawn, Is.True);
            Assert.That(calledIn.OpinionDelta, Is.EqualTo(ClientelaCatalog.OverdrawnOpinionPenalty));

            state.Relationships.TryGet(new RelationshipKey(clientId, patronHeadId), out var relationship);
            Assert.That(relationship.Opinion, Is.EqualTo(ClientelaCatalog.OverdrawnOpinionPenalty));
        });
    }

    [Test]
    public void CallInClientFavorCommandRejectsANonClientOrAnotherPatronsClient()
    {
        var (state, patronHouseholdId, _, clientId) = PatronAndClient();
        var unrelatedHouseholdId = state.HouseholdIds.Issue();

        Assert.That(
            CallInClientFavorCommands.Pipeline.Execute(
                state, new CallInClientFavorCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, patronHouseholdId, clientId, "x")).Error,
            Is.EqualTo(CallInClientFavorCommands.UnknownClient));

        Recruit(state, patronHouseholdId, clientId, new GameDate(0));
        Assert.That(
            CallInClientFavorCommands.Pipeline.Execute(
                state, new CallInClientFavorCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, unrelatedHouseholdId, clientId, "x")).Error,
            Is.EqualTo(CallInClientFavorCommands.NotYourClient));
    }

    [Test]
    public void InfluenceCycleSystemGeneratesFromRosterSizeQualityAndDecaysExistingBalance()
    {
        var (state, patronHouseholdId, patronHeadId, clientId) = PatronAndClient();
        Recruit(state, patronHouseholdId, clientId, new GameDate(0));
        // A high, quality-bonus-earning opinion — Recruit already created the client-to-patron
        // relationship (with the Patron bond), so replace it rather than adding a duplicate key.
        SetOpinion(state, clientId, patronHeadId, ClientelaCatalog.InfluenceQualityOpinionThreshold);

        InfluenceResolver.Apply(state, patronHouseholdId, 50);
        var events = new InfluenceCycleSystem().Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));

        var expected = 50 - ClientelaCatalog.InfluenceDecayPerMonth
            + ClientelaCatalog.InfluencePerClient + ClientelaCatalog.InfluenceQualityBonus;
        Assert.Multiple(() =>
        {
            Assert.That(events, Is.Empty);
            Assert.That(InfluenceResolver.Current(state, patronHouseholdId), Is.EqualTo(expected));
        });
    }

    [Test]
    public void InfluenceResolverFloorsAtZero()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        InfluenceResolver.Apply(state, householdId, 3);
        InfluenceResolver.Apply(state, householdId, -10);
        Assert.That(InfluenceResolver.Current(state, householdId), Is.EqualTo(0));
    }

    [Test]
    public void SalutatioSystemRewardsAWellAttendedRosterAndPenalizesANeglectedOne()
    {
        var (wellState, wellPatronId, wellHeadId, wellClientId) = PatronAndClient();
        Recruit(wellState, wellPatronId, wellClientId, new GameDate(0));
        for (var i = 0; i < ClientelaCatalog.SalutatioWellAttendedMinClients - 1; i++)
        {
            var extraClientId = wellState.CharacterIds.Issue();
            wellState.Characters.Add(extraClientId, CharacterTestFixtures.Minimal(extraClientId, nomen: $"Extra{i}"));
            Recruit(wellState, wellPatronId, extraClientId, new GameDate(0));
            SetOpinion(wellState, extraClientId, wellHeadId, 40);
        }
        SetOpinion(wellState, wellClientId, wellHeadId, 40);

        var wellEvents = new SalutatioSystem().Tick(wellState, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));
        var wellChanged = wellEvents.OfType<DignitasChangedEvent>().Single();

        Assert.Multiple(() =>
        {
            Assert.That(wellChanged.NewDignitas - wellChanged.PreviousDignitas, Is.EqualTo(ClientelaCatalog.SalutatioWellAttendedDignitasGain));
            Assert.That(InfluenceResolver.Current(wellState, wellPatronId), Is.EqualTo(ClientelaCatalog.SalutatioWellAttendedInfluenceGain));
        });

        var (neglectedState, neglectedPatronId, _, neglectedClientId) = PatronAndClient();
        Recruit(neglectedState, neglectedPatronId, neglectedClientId, new GameDate(0));
        var neglectedEvents = new SalutatioSystem().Tick(neglectedState, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));
        var neglectedChanged = neglectedEvents.OfType<DignitasChangedEvent>().Single();
        Assert.That(neglectedChanged.NewDignitas - neglectedChanged.PreviousDignitas, Is.EqualTo(-ClientelaCatalog.SalutatioNeglectedDignitasCost));
    }

    [Test]
    public void ClientPoachingSystemEventuallyFlipsAnOverdrawnHighAmbitionLowLoyaltyClientToARivalActor()
    {
        var (state, patronHouseholdId, patronHeadId, clientId) = PatronAndClient();
        // Overwrite with a high-Ambition, low-Loyalty condition so the client is poaching-eligible.
        state.Characters.TryGet(clientId, out var client);
        state.Characters.Remove(clientId);
        state.Characters.Add(clientId, client! with { Condition = new Condition(80, 0, 20, 80, 50) });
        Recruit(state, patronHouseholdId, clientId, new GameDate(0));

        var rivalActorId = state.ActorIds.Issue();
        var rivalHeadId = state.CharacterIds.Issue();
        state.Characters.Add(rivalHeadId, CharacterTestFixtures.Minimal(rivalHeadId, nomen: "Rivalus"));
        var rivalActor = LivingWorldActor.Create(
            rivalActorId,
            LivingWorldActorType.Gens,
            "Rivalus",
            LivingWorldActorTier.Noteworthy,
            LivingWorldActorStandingTrend.Established,
            LivingWorldActorOrigin.Ancient,
            parentActorId: null,
            new LivingWorldActorIdentity(EconomicIdentityTag.Agrarian, FactionTag.Traditionalist),
            dignitas: 10,
            new LivingWorldActorNetWorth(HouseholdWealthBand.Comfortable, Figure: null),
            new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest),
            state.RegionIds.Issue(),
            state.SettlementIds.Issue()) with
        { HeadCharacterId = rivalHeadId };
        state.Actors.Add(rivalActorId, rivalActor);

        var streams = new RandomStreamSet();
        streams.AddDerived(ClientPoachingSystem.PoachingRiskStreamName, 12345UL);

        var overdrawnMonth = ClientelaCatalog.PoachingOverdrawnAfterMonths;
        var system = new ClientPoachingSystem();
        var poached = false;
        for (var month = overdrawnMonth; month < overdrawnMonth + 300 && !poached; month++)
        {
            var events = system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));
            if (events.OfType<ClientPoachedEvent>().Any())
                poached = true;
        }

        Assert.Multiple(() =>
        {
            Assert.That(poached, Is.True);
            Assert.That(ClientelaResolver.TryGetClient(state, clientId, out _), Is.False);
            state.Relationships.TryGet(new RelationshipKey(rivalHeadId, clientId), out var fromRival);
            Assert.That(fromRival.HasBond(BondTag.Client), Is.True);
            state.Relationships.TryGet(new RelationshipKey(patronHeadId, clientId), out var fromOldPatron);
            Assert.That(fromOldPatron.HasBond(BondTag.Client), Is.False);
        });
    }

    [Test]
    public void SetCharacterFactionCommandSetsAndTheResolverDefaultsToNullOtherwise()
    {
        var (state, _, _, clientId) = PatronAndClient();
        Assert.That(CharacterFactionResolver.Current(state, clientId), Is.Null);

        var result = SetCharacterFactionCommands.Pipeline.Execute(
            state, new SetCharacterFactionCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, clientId, PoliticalFaction.Popularist));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(CharacterFactionResolver.Current(state, clientId), Is.EqualTo(PoliticalFaction.Popularist));
            Assert.That(result.Events[0].Visibility, Is.EqualTo(Visibility.Public));
        });
    }

    [Test]
    public void ClientelaStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, patronHouseholdId, patronHeadId, clientId) = PatronAndClient();
        Recruit(state, patronHouseholdId, clientId, new GameDate(0));
        CallInClientFavorCommands.Pipeline.Execute(
            state, new CallInClientFavorCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, patronHouseholdId, clientId, "a favor"));
        InfluenceResolver.Apply(state, patronHouseholdId, 12);
        SetCharacterFactionCommands.Pipeline.Execute(
            state, new SetCharacterFactionCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, patronHeadId, PoliticalFaction.Traditionalist));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(ClientelaResolver.TryGetClient(restored, clientId, out var entry), Is.True);
            Assert.That(entry!.Specialty, Is.EqualTo(ClientSpecialty.Legal));
            Assert.That(InfluenceResolver.Current(restored, patronHouseholdId), Is.EqualTo(12));
            Assert.That(CharacterFactionResolver.Current(restored, patronHeadId), Is.EqualTo(PoliticalFaction.Traditionalist));
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        });
    }
}
