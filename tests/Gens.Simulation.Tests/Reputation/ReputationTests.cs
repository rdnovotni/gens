using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Queries;
using Gens.Simulation.Random;
using Gens.Simulation.Reputation;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Reputation;

/// <summary>Phase 12 item 1 coverage: household Dignitas, the generic Favor/Obligation ledger, its
/// monthly expiration, and the audience-scoped <see cref="Visibility"/> each surfaces at
/// (<c>gens-politics-patronage-design.md</c> §2, §4.2).</summary>
public sealed class ReputationTests
{
    private static (WorldState State, RuntimeId<Household> HouseholdId) HouseholdOnly()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        return (state, householdId);
    }

    private static (WorldState State, RuntimeId<Character> GrantorId, RuntimeId<Character> BeneficiaryId) TwoCharacters()
    {
        var state = new WorldState(new GameDate(0));
        var grantorId = state.CharacterIds.Issue();
        state.Characters.Add(grantorId, CharacterTestFixtures.Minimal(grantorId, nomen: "Cornelius"));
        var beneficiaryId = state.CharacterIds.Issue();
        state.Characters.Add(beneficiaryId, CharacterTestFixtures.Minimal(beneficiaryId, nomen: "Aurelius"));
        return (state, grantorId, beneficiaryId);
    }

    [Test]
    public void DignitasResolverDefaultsToZeroForAnUntouchedHousehold()
    {
        var (state, householdId) = HouseholdOnly();
        Assert.That(DignitasResolver.Current(state, householdId), Is.EqualTo(0));
    }

    [Test]
    public void AdjustDignitasCommandMovesTheTotalAndEmitsAPublicEvent()
    {
        var (state, householdId) = HouseholdOnly();

        var command = new AdjustDignitasCommand(
            state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, 15, "won a magistracy");
        var result = AdjustDignitasCommands.Pipeline.Execute(state, command);

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(DignitasResolver.Current(state, householdId), Is.EqualTo(15));

            var changed = (DignitasChangedEvent)result.Events[0];
            Assert.That(changed.PreviousDignitas, Is.EqualTo(0));
            Assert.That(changed.NewDignitas, Is.EqualTo(15));
            Assert.That(changed.Reason, Is.EqualTo("won a magistracy"));
            Assert.That(changed.Visibility, Is.EqualTo(Visibility.Public));
        });
    }

    [Test]
    public void DignitasCanGoNegativeAndAccumulatesAcrossMultipleAdjustments()
    {
        var (state, householdId) = HouseholdOnly();

        AdjustDignitasCommands.Pipeline.Execute(
            state, new AdjustDignitasCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, 10, "a"));
        AdjustDignitasCommands.Pipeline.Execute(
            state, new AdjustDignitasCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, householdId, -25, "a scandal"));

        Assert.That(DignitasResolver.Current(state, householdId), Is.EqualTo(-15));
    }

    [Test]
    public void AdjustDignitasCommandRejectsAZeroDelta()
    {
        var (state, householdId) = HouseholdOnly();

        var command = new AdjustDignitasCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, 0, "nothing");
        var result = AdjustDignitasCommands.Pipeline.Execute(state, command);

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Error, Is.EqualTo(AdjustDignitasCommands.ZeroDelta));
        });
    }

    [Test]
    public void InkBarQueryReflectsTheRealDignitasTotal()
    {
        var (state, householdId) = HouseholdOnly();
        AdjustDignitasCommands.Pipeline.Execute(
            state, new AdjustDignitasCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, 42, "a"));

        var projection = new InkBarQuery(householdId).Execute(state, "player");

        Assert.That(projection.Dignitas, Is.EqualTo(42));
    }

    [Test]
    public void GrantFavorCommandOpensAnOutstandingObligationVisibleOnlyToTheTwoParties()
    {
        var (state, grantorId, beneficiaryId) = TwoCharacters();

        var command = new GrantFavorCommand(
            state.CommandIds.Issue(), "player", new GameDate(1), null, grantorId, beneficiaryId, "vouched at the Curia");
        var result = GrantFavorCommands.Pipeline.Execute(state, command);

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(state.FavorObligations.Count, Is.EqualTo(1));

            var favor = state.FavorObligations.InAscendingOrder().First().Value;
            Assert.That(favor.Status, Is.EqualTo(FavorStatus.Outstanding));
            Assert.That(favor.GrantorId, Is.EqualTo(grantorId));
            Assert.That(favor.BeneficiaryId, Is.EqualTo(beneficiaryId));
            Assert.That(favor.Kind, Is.EqualTo("vouched at the Curia"));

            var granted = (FavorGrantedEvent)result.Events[0];
            Assert.That(granted.Visibility.ObserverIds, Is.EquivalentTo(new[] { grantorId.ToTaggedString(), beneficiaryId.ToTaggedString() }));
        });
    }

    [Test]
    public void GrantFavorCommandRejectsTheSameCharacterEmptyKindOrAnUnknownCharacter()
    {
        var (state, grantorId, beneficiaryId) = TwoCharacters();
        var strangerId = RuntimeId<Character>.Parse("char_9999999");

        Assert.Multiple(() =>
        {
            Assert.That(
                GrantFavorCommands.Pipeline.Execute(
                    state, new GrantFavorCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, grantorId, grantorId, "x")).Error,
                Is.EqualTo(GrantFavorCommands.SameCharacter));

            Assert.That(
                GrantFavorCommands.Pipeline.Execute(
                    state, new GrantFavorCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, grantorId, beneficiaryId, "  ")).Error,
                Is.EqualTo(GrantFavorCommands.EmptyKind));

            Assert.That(
                GrantFavorCommands.Pipeline.Execute(
                    state, new GrantFavorCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, grantorId, strangerId, "x")).Error,
                Is.EqualTo(GrantFavorCommands.UnknownCharacter));
        });
    }

    [Test]
    public void SettleFavorCommandResolvesAsRepaidOrForgiven()
    {
        var (state, grantorId, beneficiaryId) = TwoCharacters();
        GrantFavorCommands.Pipeline.Execute(
            state, new GrantFavorCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, grantorId, beneficiaryId, "a favor"));
        var favorId = state.FavorObligations.InAscendingOrder().First().Key;

        var result = SettleFavorCommands.Pipeline.Execute(
            state, new SettleFavorCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, favorId, FavorResolution.Repaid));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            state.FavorObligations.TryGet(favorId, out var favor);
            Assert.That(favor!.Status, Is.EqualTo(FavorStatus.Repaid));
            Assert.That(favor.ResolvedDate, Is.EqualTo(new GameDate(3)));

            var settled = (FavorSettledEvent)result.Events[0];
            Assert.That(settled.Resolution, Is.EqualTo(FavorResolution.Repaid));
            Assert.That(settled.Visibility.ObserverIds, Is.EquivalentTo(new[] { grantorId.ToTaggedString(), beneficiaryId.ToTaggedString() }));
        });
    }

    [Test]
    public void SettleFavorCommandRejectsAnUnknownOrAlreadyResolvedFavor()
    {
        var (state, grantorId, beneficiaryId) = TwoCharacters();
        GrantFavorCommands.Pipeline.Execute(
            state, new GrantFavorCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, grantorId, beneficiaryId, "a favor"));
        var favorId = state.FavorObligations.InAscendingOrder().First().Key;

        var unknownId = state.FavorObligationIds.Issue();
        Assert.That(
            SettleFavorCommands.Pipeline.Execute(
                state, new SettleFavorCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, unknownId, FavorResolution.Forgiven)).Error,
            Is.EqualTo(SettleFavorCommands.UnknownFavor));

        SettleFavorCommands.Pipeline.Execute(
            state, new SettleFavorCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, favorId, FavorResolution.Forgiven));

        Assert.That(
            SettleFavorCommands.Pipeline.Execute(
                state, new SettleFavorCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, favorId, FavorResolution.Repaid)).Error,
            Is.EqualTo(SettleFavorCommands.NotOutstanding));
    }

    [Test]
    public void FavorExpirationSystemDeclaresTheRelationshipsActorsPhaseAndReadWriteSet()
    {
        var system = new FavorExpirationSystem();
        Assert.Multiple(() =>
        {
            Assert.That(system.Phase, Is.EqualTo(TickPhase.RelationshipsActors));
            Assert.That(system.Reads, Is.EquivalentTo(new[] { "favorObligations" }));
            Assert.That(system.Writes, Is.EquivalentTo(new[] { "favorObligations", "eventIds" }));
        });
    }

    [Test]
    public void FavorExpirationSystemLapsesOnlyOutstandingFavorsPastTheThreshold()
    {
        var (state, grantorId, beneficiaryId) = TwoCharacters();

        // Old enough to expire.
        GrantFavorCommands.Pipeline.Execute(
            state, new GrantFavorCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, grantorId, beneficiaryId, "an old favor"));
        // Freshly granted — must survive the same tick.
        GrantFavorCommands.Pipeline.Execute(
            state,
            new GrantFavorCommand(
                state.CommandIds.Issue(), "player", new GameDate(ReputationCatalog.FavorExpirationAfterMonths - 10), null,
                grantorId, beneficiaryId, "a fresh favor"));

        var favors = state.FavorObligations.InAscendingOrder().Select(entry => entry.Key).ToArray();
        var oldFavorId = favors[0];
        var freshFavorId = favors[1];

        var expiryTickDate = new GameDate(ReputationCatalog.FavorExpirationAfterMonths);
        var tooEarlyEvents = new FavorExpirationSystem().Tick(state, new MonthlyTickContext(new GameDate(ReputationCatalog.FavorExpirationAfterMonths - 1), new RandomStreamSet()));
        Assert.That(tooEarlyEvents, Is.Empty);

        var events = new FavorExpirationSystem().Tick(state, new MonthlyTickContext(expiryTickDate, new RandomStreamSet()));

        Assert.Multiple(() =>
        {
            Assert.That(events.OfType<FavorExpiredEvent>().Count(), Is.EqualTo(1));
            state.FavorObligations.TryGet(oldFavorId, out var old);
            Assert.That(old!.Status, Is.EqualTo(FavorStatus.Expired));
            Assert.That(old.ResolvedDate, Is.EqualTo(expiryTickDate));

            state.FavorObligations.TryGet(freshFavorId, out var fresh);
            Assert.That(fresh!.Status, Is.EqualTo(FavorStatus.Outstanding));
        });
    }

    [Test]
    public void ReputationStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, grantorId, beneficiaryId) = TwoCharacters();
        var householdId = state.HouseholdIds.Issue();

        AdjustDignitasCommands.Pipeline.Execute(
            state, new AdjustDignitasCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, -8, "a defaulted debt"));

        GrantFavorCommands.Pipeline.Execute(
            state, new GrantFavorCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, grantorId, beneficiaryId, "repaid favor"));
        var repaidFavorId = state.FavorObligations.InAscendingOrder().First().Key;
        SettleFavorCommands.Pipeline.Execute(
            state, new SettleFavorCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, repaidFavorId, FavorResolution.Repaid));

        GrantFavorCommands.Pipeline.Execute(
            state, new GrantFavorCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, grantorId, beneficiaryId, "will lapse"));
        new FavorExpirationSystem().Tick(state, new MonthlyTickContext(new GameDate(ReputationCatalog.FavorExpirationAfterMonths), new RandomStreamSet()));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(DignitasResolver.Current(restored, householdId), Is.EqualTo(-8));

            Assert.That(restored.FavorObligations.Count, Is.EqualTo(2));
            restored.FavorObligations.TryGet(repaidFavorId, out var repaid);
            Assert.That(repaid!.Status, Is.EqualTo(FavorStatus.Repaid));

            var lapsed = restored.FavorObligations.InAscendingOrder().Last().Value;
            Assert.That(lapsed.Status, Is.EqualTo(FavorStatus.Expired));

            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        });
    }
}
