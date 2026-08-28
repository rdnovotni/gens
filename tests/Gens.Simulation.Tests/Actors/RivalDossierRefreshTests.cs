using Gens.Simulation.Actors;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Chronicle;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;
using CharacterFixtures = Gens.Simulation.Tests.Characters.CharacterTestFixtures;

namespace Gens.Simulation.Tests.Actors;

/// <summary>Phase 10 package 14 coverage for <see cref="RivalDossierRefresh"/> and <see
/// cref="RivalDossierStaleness"/>: refresh only on genuine contact, never from ambient background
/// drift, and <see cref="RivalDossier.LastUpdatedDate"/> never regresses.</summary>
public sealed class RivalDossierRefreshTests
{
    private static (WorldState State, LivingWorldActor A, LivingWorldActor B) TwoActors()
    {
        var state = new WorldState(new GameDate(0));
        var netWorth = new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null);
        var military = new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest);
        var a = RivalHouseCreationService.CreateAncientSeed(
            state, "Aemilia", LivingWorldActorStandingTrend.Established, LivingWorldActorIdentity.None,
            0, netWorth, military, state.RegionIds.Issue(), state.SettlementIds.Issue());
        var b = RivalHouseCreationService.CreateAncientSeed(
            state, "Cornelia", LivingWorldActorStandingTrend.Established, LivingWorldActorIdentity.None,
            0, netWorth, military, state.RegionIds.Issue(), state.SettlementIds.Issue());
        return (state, a, b);
    }

    [Test]
    public void RefreshCreatesADossierOnFirstContact()
    {
        var (state, a, _) = TwoActors();

        RivalDossierRefresh.Refresh(state, a.ActorId, new GameDate(5), "First contact.");

        Assert.That(state.RivalDossiers.TryGet(a.ActorId, out var dossier), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(dossier!.Summary, Is.EqualTo("First contact."));
            Assert.That(dossier.LastUpdatedDate, Is.EqualTo(new GameDate(5)));
        });
    }

    [Test]
    public void RefreshUpdatesSummaryAndLastUpdatedDateOnALaterContact()
    {
        var (state, a, _) = TwoActors();
        RivalDossierRefresh.Refresh(state, a.ActorId, new GameDate(5), "First contact.");

        RivalDossierRefresh.Refresh(state, a.ActorId, new GameDate(10), "Second contact.");

        Assert.That(state.RivalDossiers.TryGet(a.ActorId, out var dossier), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(dossier!.Summary, Is.EqualTo("Second contact."));
            Assert.That(dossier.LastUpdatedDate, Is.EqualTo(new GameDate(10)));
        });
    }

    [Test]
    public void RefreshNeverRegressesLastUpdatedDate()
    {
        var (state, a, _) = TwoActors();
        RivalDossierRefresh.Refresh(state, a.ActorId, new GameDate(10), "Later contact.");

        RivalDossierRefresh.Refresh(state, a.ActorId, new GameDate(3), "An out-of-order earlier event.");

        Assert.That(state.RivalDossiers.TryGet(a.ActorId, out var dossier), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(dossier!.Summary, Is.EqualTo("Later contact."));
            Assert.That(dossier.LastUpdatedDate, Is.EqualTo(new GameDate(10)));
        });
    }

    [Test]
    public void RefreshAppendsAndTrimsRecentChronicleEntries()
    {
        var (state, a, _) = TwoActors();
        var entryIds = new List<RuntimeId<ChronicleEntry>>();
        for (var month = 0; month < RivalDossierCatalog.MaxRecentChronicleEntries + 3; month++)
        {
            var entryId = state.ChronicleEntryIds.Issue();
            entryIds.Add(entryId);
            RivalDossierRefresh.Refresh(state, a.ActorId, new GameDate(month), $"Month {month}.", entryId);
        }

        state.RivalDossiers.TryGet(a.ActorId, out var dossier);
        Assert.That(dossier!.RecentChronicleEntries, Has.Count.EqualTo(RivalDossierCatalog.MaxRecentChronicleEntries));
        Assert.That(dossier.RecentChronicleEntries[^1], Is.EqualTo(entryIds[^1]));
    }

    [Test]
    public void RefreshForCharacterResolvesTheActorThatHeadsIt()
    {
        var (state, a, _) = TwoActors();
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterFixtures.Minimal(headId));
        state.Actors.Remove(a.ActorId);
        state.Actors.Add(a.ActorId, a with { HeadCharacterId = headId });

        RivalDossierRefresh.RefreshForCharacter(state, headId, new GameDate(7), "A Scheme touched this house's head.");

        Assert.That(state.RivalDossiers.TryGet(a.ActorId, out var dossier), Is.True);
        Assert.That(dossier!.Summary, Is.EqualTo("A Scheme touched this house's head."));
    }

    [Test]
    public void RefreshForCharacterIsANoOpWhenTheCharacterHeadsNoActor()
    {
        var (state, _, _) = TwoActors();
        var playerCharacterId = state.CharacterIds.Issue();
        state.Characters.Add(playerCharacterId, CharacterFixtures.Minimal(playerCharacterId));

        RivalDossierRefresh.RefreshForCharacter(state, playerCharacterId, new GameDate(7), "Irrelevant.");

        Assert.That(state.RivalDossiers.Count, Is.EqualTo(0));
    }

    [Test]
    public void AdjustHouseStandingRefreshesBothSidesDossiers()
    {
        var (state, a, b) = TwoActors();
        var command = new AdjustHouseStandingCommand(
            state.CommandIds.Issue(), a.ActorId.ToTaggedString(), new GameDate(4), null, a.ActorId, b.ActorId,
            HouseStandingAdjustmentDirection.TowardRivalry);

        var result = AdjustHouseStandingCommands.Pipeline.Execute(state, command);

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(state.RivalDossiers.TryGet(a.ActorId, out var dossierA), Is.True);
            Assert.That(dossierA!.LastUpdatedDate, Is.EqualTo(new GameDate(4)));
            Assert.That(state.RivalDossiers.TryGet(b.ActorId, out var dossierB), Is.True);
            Assert.That(dossierB!.LastUpdatedDate, Is.EqualTo(new GameDate(4)));
        });
    }

    [Test]
    public void BackgroundHouseDriftNeverTouchesADossier()
    {
        var (state, a, _) = TwoActors();

        var system = new BackgroundHouseDriftSystem();
        var streams = new RandomStreamSet();
        streams.AddDerived(CampaignBootstrapper.BackgroundHouseDriftStreamName, 1UL);
        for (var month = 0; month < 24; month++)
            system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));

        Assert.That(state.RivalDossiers.TryGet(a.ActorId, out _), Is.False);
    }

    [Test]
    public void StalenessDescribesMonthsSinceTheLastUpdate()
    {
        var dossier = new RivalDossier(default(RuntimeId<Actor>), "Summary.", null, new GameDate(10), Array.Empty<RuntimeId<ChronicleEntry>>());

        Assert.Multiple(() =>
        {
            Assert.That(RivalDossierStaleness.MonthsSinceUpdate(dossier, new GameDate(10)), Is.EqualTo(0));
            Assert.That(RivalDossierStaleness.MonthsSinceUpdate(dossier, new GameDate(17)), Is.EqualTo(7));
            Assert.That(RivalDossierStaleness.Describe(dossier, new GameDate(10)), Is.EqualTo("as of this month"));
            Assert.That(RivalDossierStaleness.Describe(dossier, new GameDate(11)), Is.EqualTo("as of 1 month ago"));
            Assert.That(RivalDossierStaleness.Describe(dossier, new GameDate(17)), Is.EqualTo("as of 7 months ago"));
        });
    }
}
