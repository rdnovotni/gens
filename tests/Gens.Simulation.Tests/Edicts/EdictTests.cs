using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Clientela;
using Gens.Simulation.Commands;
using Gens.Simulation.Economy;
using Gens.Simulation.Edicts;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Legal;
using Gens.Simulation.Magistracies;
using Gens.Simulation.Reputation;
using Gens.Simulation.Saves;
using Gens.Simulation.Scandal;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Edicts;

/// <summary>Phase 12 item 9 coverage: the three real, reachable Edicts (<see
/// cref="IssueManumissionEdictCommand"/>, <see cref="GrantCitizenshipEdictCommand"/>, <see
/// cref="IssueProscriptionCommand"/>), each costing real Influence/Dignitas to issue and each routing
/// its own Reception through Phase 12 item 7's Scandal engine.</summary>
public sealed class EdictTests
{
    private static (WorldState State, RuntimeId<Household> HouseholdId, RuntimeId<Character> HeadId, RuntimeId<Settlement> SettlementId)
        OneHousehold()
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));

        var householdId = state.HouseholdIds.Issue();
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, nomen: "Fabius", household: householdId, location: settlementId));
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, headId, new GameDate(0)));

        return (state, householdId, headId, settlementId);
    }

    private static void GiveInfluence(WorldState state, RuntimeId<Household> householdId, int amount) =>
        InfluenceResolver.Apply(state, householdId, amount);

    // ---- Manumission Edict --------------------------------------------------------------------

    [Test]
    public void ManumissionEdictRejectedWithNoEnslavedCharacters()
    {
        var (state, householdId, headId, _) = OneHousehold();
        GiveInfluence(state, householdId, 100);

        var result = IssueManumissionEdictCommands.Pipeline.Execute(
            state, new IssueManumissionEdictCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, headId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Error, Is.EqualTo(IssueManumissionEdictCommands.NoEnslavedCharacters));
        });
    }

    [Test]
    public void ManumissionEdictRejectedWithoutEnoughInfluence()
    {
        var (state, householdId, headId, settlementId) = OneHousehold();
        var enslavedId = state.CharacterIds.Issue();
        state.Characters.Add(enslavedId, CharacterTestFixtures.Minimal(
            enslavedId, nomen: "Servus", household: householdId, location: settlementId, status: LegalStatus.Enslaved));

        var result = IssueManumissionEdictCommands.Pipeline.Execute(
            state, new IssueManumissionEdictCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, headId));

        Assert.That(result.Error, Is.EqualTo(IssueManumissionEdictCommands.InsufficientInfluence));
    }

    [Test]
    public void ManumissionEdictFreesEveryEnslavedMemberAndGrantsDignitasAndRecordsScandal()
    {
        var (state, householdId, headId, settlementId) = OneHousehold();
        GiveInfluence(state, householdId, 100);
        var enslavedIds = new[]
        {
            state.CharacterIds.Issue(),
            state.CharacterIds.Issue(),
        };
        foreach (var id in enslavedIds)
        {
            state.Characters.Add(id, CharacterTestFixtures.Minimal(
                id, nomen: "Servus", household: householdId, location: settlementId, status: LegalStatus.Enslaved));
        }

        var dignitasBefore = DignitasResolver.Current(state, householdId);
        var result = IssueManumissionEdictCommands.Pipeline.Execute(
            state, new IssueManumissionEdictCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, headId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            foreach (var id in enslavedIds)
            {
                state.Characters.TryGet(id, out var freed);
                Assert.That(freed!.LegalStatus, Is.EqualTo(LegalStatus.Freedman));
            }

            // Net of the issuance cost, the Effect's own Dignitas gain, and Reception's own real
            // backlash penalty at EdictCatalog.ManumissionEdictReceptionSeverity (PublicDisgrace) — the
            // Reception is a genuine cost, not double-counted bookkeeping.
            Assert.That(
                DignitasResolver.Current(state, householdId),
                Is.EqualTo(
                    dignitasBefore - EdictCatalog.ManumissionEdictDignitasCost + EdictCatalog.ManumissionEdictDignitasGain
                    - ScandalCatalog.PublicDisgraceDignitasPenalty));
            Assert.That(InfluenceResolver.Current(state, householdId), Is.EqualTo(100 - EdictCatalog.ManumissionEdictInfluenceCost));
            Assert.That(state.EdictRecords.Count, Is.EqualTo(1));
            Assert.That(state.ScandalRecords.Count, Is.EqualTo(1));
            state.ScandalRecords.TryGet(state.ScandalRecords.InAscendingOrder().Single().Key, out var scandal);
            Assert.That(scandal!.SourceType, Is.EqualTo(ScandalSourceType.EdictBacklash));
        });
    }

    // ---- Citizenship Grant ----------------------------------------------------------------------

    [Test]
    public void GrantCitizenshipRejectedWhenTargetAlreadyCitizen()
    {
        var (state, householdId, headId, settlementId) = OneHousehold();
        GiveInfluence(state, householdId, 100);

        var result = GrantCitizenshipEdictCommands.Pipeline.Execute(
            state, new GrantCitizenshipEdictCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, headId, settlementId));

        Assert.That(result.Error, Is.EqualTo(GrantCitizenshipEdictCommands.AlreadyCitizen));
    }

    [Test]
    public void GrantCitizenshipChangesLegalStatusAndGrantsDignitasAndRecordsScandal()
    {
        var (state, householdId, _, settlementId) = OneHousehold();
        GiveInfluence(state, householdId, 100);
        var targetId = state.CharacterIds.Issue();
        state.Characters.Add(targetId, CharacterTestFixtures.Minimal(
            targetId, nomen: "Peregrinus", location: settlementId, status: LegalStatus.Peregrine));

        var dignitasBefore = DignitasResolver.Current(state, householdId);
        var result = GrantCitizenshipEdictCommands.Pipeline.Execute(
            state, new GrantCitizenshipEdictCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, targetId, settlementId));

        state.Characters.TryGet(targetId, out var granted);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(granted!.LegalStatus, Is.EqualTo(LegalStatus.RomanCitizen));
            // Net of issuance cost, Effect gain, and Reception's own real backlash penalty at
            // EdictCatalog.CitizenshipGrantReceptionSeverity (MinorEmbarrassment).
            Assert.That(
                DignitasResolver.Current(state, householdId),
                Is.EqualTo(
                    dignitasBefore - EdictCatalog.CitizenshipGrantDignitasCost + EdictCatalog.CitizenshipGrantDignitasGain
                    - ScandalCatalog.MinorEmbarrassmentDignitasPenalty));
            Assert.That(state.ScandalRecords.Count, Is.EqualTo(1));
            Assert.That(state.LegalCases.Count, Is.EqualTo(0));
        });
    }

    [Test]
    public void GrantCitizenshipWithAResolvedChallengerFilesARealLegalCase()
    {
        var (state, householdId, _, settlementId) = OneHousehold();
        GiveInfluence(state, householdId, 100);
        var targetId = state.CharacterIds.Issue();
        state.Characters.Add(targetId, CharacterTestFixtures.Minimal(
            targetId, nomen: "Peregrinus", location: settlementId, status: LegalStatus.Peregrine));

        var challengerHouseholdId = state.HouseholdIds.Issue();
        var challengerHeadId = state.CharacterIds.Issue();
        state.Characters.Add(challengerHeadId, CharacterTestFixtures.Minimal(
            challengerHeadId, nomen: "Rival", household: challengerHouseholdId, location: settlementId));
        state.HouseholdHeadships.Add(challengerHouseholdId, new HouseholdHeadship(challengerHouseholdId, challengerHeadId, new GameDate(0)));
        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Treasury,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(challengerHouseholdId), Money.FromDenarii(50)),
                new LedgerPosting(new LedgerAccountKey(LedgerAccountKind.System, "test:seed"), Money.FromDenarii(-50)),
            });

        var result = GrantCitizenshipEdictCommands.Pipeline.Execute(
            state, new GrantCitizenshipEdictCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, targetId, settlementId, challengerHouseholdId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(state.LegalCases.Count, Is.EqualTo(1));
            state.EdictRecords.TryGet(state.EdictRecords.InAscendingOrder().Single().Key, out var edict);
            Assert.That(edict!.LegalCaseId, Is.Not.Null);
        });
    }

    // ---- Proscription ---------------------------------------------------------------------------

    private static LivingWorldActor TargetRivalActor(WorldState state, RuntimeId<Region> regionId, RuntimeId<Settlement> settlementId)
    {
        var netWorth = new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null);
        var military = new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest);
        return RivalHouseCreationService.CreateAncientSeed(
            state, "Cornelia", LivingWorldActorStandingTrend.Established, LivingWorldActorIdentity.None, 0, netWorth, military,
            regionId, settlementId);
    }

    [Test]
    public void ProscriptionRejectedWithoutDuumvirSeat()
    {
        var (state, householdId, _, settlementId) = OneHousehold();
        GiveInfluence(state, householdId, 100);
        var regionId = state.Regions.InAscendingOrder().Single().Key;
        var target = TargetRivalActor(state, regionId, settlementId);

        var result = IssueProscriptionCommands.Pipeline.Execute(
            state, new IssueProscriptionCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, target.ActorId));

        Assert.That(result.Error, Is.EqualTo(IssueProscriptionCommands.IssuerLacksAuthority));
    }

    [Test]
    public void ProscriptionSeizesAssetsScarsRelationshipAndTriggersDemonstrationEffect()
    {
        var (state, householdId, headId, settlementId) = OneHousehold();
        GiveInfluence(state, householdId, 100);
        var regionId = state.Regions.InAscendingOrder().Single().Key;

        // Grant a real Duumvir seat directly on the partition (avoids depending on Magistracies' own
        // election machinery, which this test does not need to exercise).
        var recordId = state.MagistracyRecordIds.Issue();
        state.MagistracyRecords.Add(recordId, new MagistracyRecord(recordId, headId, MagistracyOffice.Duumvir, settlementId, new GameDate(0)));

        var target = TargetRivalActor(state, regionId, settlementId);
        var targetHeadId = state.CharacterIds.Issue();
        state.Characters.Add(targetHeadId, CharacterTestFixtures.Minimal(targetHeadId, nomen: "TargetHead", location: settlementId));
        state.Actors.Remove(target.ActorId);
        state.Actors.Add(target.ActorId, target with { HeadCharacterId = targetHeadId });

        // TargetRivalActor seeds a Modest NetWorth.Band — the real, only wealth figure a Gens actor
        // carries (its own LedgerAccountKey.ForActor is never funded in production), so seizure reads
        // and steps down that Band directly rather than a manually-funded Actor ledger account.
        var otherActor = TargetRivalActor(state, regionId, settlementId);

        var result = IssueProscriptionCommands.Pipeline.Execute(
            state, new IssueProscriptionCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, target.ActorId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(
                state.Actors.TryGet(target.ActorId, out var seizedTarget) ? seizedTarget!.NetWorth.Band : (HouseholdWealthBand?)null,
                Is.EqualTo(HouseholdWealthBand.Ruined));
            Assert.That(
                state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var householdAccount) ? householdAccount!.Balance : Money.Zero,
                Is.EqualTo(EdictCatalog.ProscriptionSeizureByBand[HouseholdWealthBand.Modest]));
            Assert.That(
                state.Relationships.TryGet(new RelationshipKey(headId, targetHeadId), out var relationship) ? relationship.Opinion : 0,
                Is.LessThan(0));
            Assert.That(
                HouseStandingResolver.GetEffectiveStanding(state, target.ActorId, otherActor.ActorId),
                Is.EqualTo(HouseStandingLevel.Rivalrous));
            state.EdictRecords.TryGet(state.EdictRecords.InAscendingOrder().Single().Key, out var edict);
            Assert.That(edict!.DemonstrationEffectTriggered, Is.True);
            state.ScandalRecords.TryGet(state.ScandalRecords.InAscendingOrder().Single().Key, out var scandal);
            Assert.That(scandal!.Severity, Is.EqualTo(ScandalSeverity.NotaCensoriaEligible));
        });
    }

    [Test]
    public void ProscriptionFeedsDomusDuraDoctrineSignal()
    {
        var (state, householdId, headId, settlementId) = OneHousehold();
        GiveInfluence(state, householdId, 100);
        var regionId = state.Regions.InAscendingOrder().Single().Key;
        var recordId = state.MagistracyRecordIds.Issue();
        state.MagistracyRecords.Add(recordId, new MagistracyRecord(recordId, headId, MagistracyOffice.Duumvir, settlementId, new GameDate(0)));
        var target = TargetRivalActor(state, regionId, settlementId);

        Assert.That(EdictResolver.HasIssuedProscription(state, householdId), Is.False);

        IssueProscriptionCommands.Pipeline.Execute(
            state, new IssueProscriptionCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, target.ActorId));

        Assert.That(EdictResolver.HasIssuedProscription(state, householdId), Is.True);
    }

    // ---- Save/load round trip ------------------------------------------------------------------------

    [Test]
    public void EdictStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, householdId, headId, settlementId) = OneHousehold();
        GiveInfluence(state, householdId, 100);
        var targetId = state.CharacterIds.Issue();
        state.Characters.Add(targetId, CharacterTestFixtures.Minimal(
            targetId, nomen: "Peregrinus", location: settlementId, status: LegalStatus.Peregrine));

        GrantCitizenshipEdictCommands.Pipeline.Execute(
            state, new GrantCitizenshipEdictCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, targetId, settlementId));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.EdictRecords.Count, Is.EqualTo(state.EdictRecords.Count));
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        });
    }
}
