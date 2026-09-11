using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Cultures;
using Gens.Simulation.Diplomacy;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Languages;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Diplomacy;

public sealed class SendDiplomaticGiftCommandTests
{
    private static readonly GameDate StartDate = new(0);

    private static (WorldState State, RuntimeId<Household> HouseholdId, RuntimeId<Character> EnvoyId, RuntimeId<Actor> PeopleActorId)
        Setup(long fundingDenarii = 10_000)
    {
        var state = new WorldState(StartDate);
        var householdId = state.HouseholdIds.Issue();
        var envoyId = state.CharacterIds.Issue();
        state.Characters.Add(envoyId, CharacterTestFixtures.Minimal(envoyId, household: householdId));
        state.LedgerAccounts.Add(
            LedgerAccountKey.ForHousehold(householdId),
            new LedgerAccount(LedgerAccountKey.ForHousehold(householdId), Money.FromDenarii(fundingDenarii)));

        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        var actor = ForeignPeopleCreationService.CreateAncientSeed(
            state, KnownWorldCultures.BuildCatalog(), "Suebi", KnownWorldCultures.Germanic,
            LivingWorldActorStandingTrend.Established, new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest),
            regionId, settlementId, StartDate);

        return (state, householdId, envoyId, actor.ActorId);
    }

    private static Simulation.Commands.CommandPipeline<WorldState, SendDiplomaticGiftCommand> Pipeline() =>
        SendDiplomaticGiftCommands.CreatePipeline(CultureLanguageMap.BuildKnownWorldMap());

    [Test]
    public void AcceptsAGiftAndImprovesStanding()
    {
        var (state, householdId, envoyId, actorId) = Setup();

        var result = Pipeline().Execute(state, new SendDiplomaticGiftCommand(
            state.CommandIds.Issue(), "player", StartDate, null, householdId, envoyId, actorId, Money.FromDenarii(200)));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            var standing = PerPeopleStandingResolver.GetEffective(state, householdId, actorId);
            Assert.That(standing.Goodwill, Is.GreaterThan(0));
        });
    }

    [Test]
    public void DebitsTheHouseholdLedgerForTheGiftValue()
    {
        var (state, householdId, envoyId, actorId) = Setup(fundingDenarii: 10_000);

        Pipeline().Execute(state, new SendDiplomaticGiftCommand(
            state.CommandIds.Issue(), "player", StartDate, null, householdId, envoyId, actorId, Money.FromDenarii(200)));

        var balance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var account) ? account!.Balance : Money.Zero;
        Assert.That(balance, Is.EqualTo(Money.FromDenarii(9_800)));
    }

    [Test]
    public void PromotesTheForeignPeopleToNoteworthyOnContact()
    {
        var (state, householdId, envoyId, actorId) = Setup();

        Pipeline().Execute(state, new SendDiplomaticGiftCommand(
            state.CommandIds.Issue(), "player", StartDate, null, householdId, envoyId, actorId, Money.FromDenarii(200)));

        state.Actors.TryGet(actorId, out var actor);
        Assert.That(actor!.Tier, Is.EqualTo(LivingWorldActorTier.Noteworthy));
    }

    [Test]
    public void WorksWithNoInterpreterAtAll()
    {
        var (state, householdId, envoyId, actorId) = Setup();

        var result = Pipeline().Execute(state, new SendDiplomaticGiftCommand(
            state.CommandIds.Issue(), "player", StartDate, null, householdId, envoyId, actorId, Money.FromDenarii(200)));

        Assert.That(result.Accepted, Is.True);
    }

    [Test]
    public void RejectsAnUnknownEnvoy()
    {
        var (state, householdId, _, actorId) = Setup();

        var result = Pipeline().Execute(state, new SendDiplomaticGiftCommand(
            state.CommandIds.Issue(), "player", StartDate, null, householdId, state.CharacterIds.Issue(), actorId, Money.FromDenarii(200)));

        Assert.That(result.Error, Is.EqualTo(SendDiplomaticGiftCommands.EnvoyNotFound));
    }

    [Test]
    public void RejectsAnEnvoyNotOfTheHousehold()
    {
        var (state, householdId, _, actorId) = Setup();
        var otherHouseholdId = state.HouseholdIds.Issue();
        var outsiderId = state.CharacterIds.Issue();
        state.Characters.Add(outsiderId, CharacterTestFixtures.Minimal(outsiderId, nomen: "Outsider", household: otherHouseholdId));

        var result = Pipeline().Execute(state, new SendDiplomaticGiftCommand(
            state.CommandIds.Issue(), "player", StartDate, null, householdId, outsiderId, actorId, Money.FromDenarii(200)));

        Assert.That(result.Error, Is.EqualTo(SendDiplomaticGiftCommands.EnvoyNotOfHousehold));
    }

    [Test]
    public void RejectsAnUnknownForeignPeople()
    {
        var (state, householdId, envoyId, _) = Setup();

        var result = Pipeline().Execute(state, new SendDiplomaticGiftCommand(
            state.CommandIds.Issue(), "player", StartDate, null, householdId, envoyId, state.ActorIds.Issue(), Money.FromDenarii(200)));

        Assert.That(result.Error, Is.EqualTo(SendDiplomaticGiftCommands.UnknownForeignPeople));
    }

    [Test]
    public void RejectsAGiftBelowTheMinimumValue()
    {
        var (state, householdId, envoyId, actorId) = Setup();

        var result = Pipeline().Execute(state, new SendDiplomaticGiftCommand(
            state.CommandIds.Issue(), "player", StartDate, null, householdId, envoyId, actorId, Money.FromDenarii(1)));

        Assert.That(result.Error, Is.EqualTo(SendDiplomaticGiftCommands.GiftValueOutOfRange));
    }

    [Test]
    public void RejectsAGiftWithInsufficientFunds()
    {
        var (state, householdId, envoyId, actorId) = Setup(fundingDenarii: 10);

        var result = Pipeline().Execute(state, new SendDiplomaticGiftCommand(
            state.CommandIds.Issue(), "player", StartDate, null, householdId, envoyId, actorId, Money.FromDenarii(200)));

        Assert.That(result.Error, Is.EqualTo(SendDiplomaticGiftCommands.InsufficientFunds));
    }
}
