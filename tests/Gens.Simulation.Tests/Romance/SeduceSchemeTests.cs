using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Interactions;
using Gens.Simulation.Random;
using Gens.Simulation.Romance;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;
using CharacterTestFixtures = Gens.Simulation.Tests.Characters.CharacterTestFixtures;

namespace Gens.Simulation.Tests.Romance;

/// <summary>Phase 17 item 3 slice 4 coverage: <see cref="SchemeType.Seduce"/>'s Attraction-weighted
/// success term (<c>gens-romance-sexuality-lineage-design.md</c> §7) and <see
/// cref="SeduceSchemeResolutionHook"/>'s success payoff.</summary>
public sealed class SeduceSchemeTests
{
    private static WorldState NewState() => new(new GameDate(0));

    private static RandomStreamSet Streams(ulong seed)
    {
        var streams = new RandomStreamSet();
        streams.Add("interactions.schemeProgress", seed, 1);
        return streams;
    }

    private static RuntimeId<Character> AddCharacter(WorldState state, int intrigue = 10)
    {
        var id = state.CharacterIds.Issue();
        state.Characters.Add(
            id,
            CharacterTestFixtures.Minimal(id, birthDate: new GameDate(-30 * 12), attributes: new CoreAttributes(10, 10, 10, intrigue, 10)));
        return id;
    }

    // ---- SeduceSchemeResolutionHook (deterministic, preferred over RNG-driven success rates) --------

    [Test]
    public void ApplyGrantsBlackmailLeverageAndAdvancesTheBondOnSuccess()
    {
        var state = NewState();
        var initiatorId = AddCharacter(state);
        var targetId = AddCharacter(state);
        var schemeId = state.SchemeIds.Issue();
        var scheme = Scheme.Create(schemeId, initiatorId, targetId, SchemeType.Seduce, new GameDate(0))
            with
        { Status = SchemeStatus.Succeeded };

        var events = SeduceSchemeResolutionHook.Apply(state, scheme, new GameDate(1));

        Assert.That(events, Is.Not.Empty);
        Assert.That(state.Relationships.TryGet(new RelationshipKey(initiatorId, targetId), out var relationship), Is.True);
        Assert.That(relationship.Bonds.HasFlag(BondTag.BlackmailLeverage), Is.True);

        Assert.That(state.RomanticBonds.TryGet(RomanticBondKey.Create(initiatorId, targetId), out var bond), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(bond.Affection, Is.EqualTo(RomanceCatalog.SeduceSuccessAffectionDelta));
            Assert.That(bond.Attraction, Is.EqualTo(RomanceCatalog.SeduceSuccessAttractionDelta));
        });
    }

    [Test]
    public void ApplyIsANoOpForACoerciveScheme()
    {
        var state = NewState();
        var initiatorId = AddCharacter(state);
        var targetId = AddCharacter(state);
        var schemeId = state.SchemeIds.Issue();
        var scheme = Scheme.Create(schemeId, initiatorId, targetId, SchemeType.Coercive, new GameDate(0))
            with
        { Status = SchemeStatus.Succeeded };

        var events = SeduceSchemeResolutionHook.Apply(state, scheme, new GameDate(1));

        Assert.That(events, Is.Empty);
        Assert.That(state.Relationships.TryGet(new RelationshipKey(initiatorId, targetId), out _), Is.False);
    }

    [Test]
    public void ApplyIsANoOpForASeduceSchemeThatDidNotSucceed()
    {
        var state = NewState();
        var initiatorId = AddCharacter(state);
        var targetId = AddCharacter(state);
        var schemeId = state.SchemeIds.Issue();
        var scheme = Scheme.Create(schemeId, initiatorId, targetId, SchemeType.Seduce, new GameDate(0))
            with
        { Status = SchemeStatus.FailedQuietly };

        var events = SeduceSchemeResolutionHook.Apply(state, scheme, new GameDate(1));

        Assert.That(events, Is.Empty);
    }

    // ---- Success-chance weighting -------------------------------------------------------------------

    [Test]
    public void ASeduceSchemeWithAMaximalPreExistingAttractionBondAlwaysSucceedsOnCompletion()
    {
        // Base success chance is 50% at zero Intrigue (SchemeProgressCatalog.BaseSuccessChancePercent);
        // a maximal (100) pre-existing Attraction adds SeduceAttractionSuccessWeightPercent (50) on top,
        // clamping the chance to 100 regardless of which roll the stream produces — deterministic without
        // needing to hand-pick a seed.
        var state = NewState();
        var initiatorId = AddCharacter(state, intrigue: 0);
        var targetId = AddCharacter(state, intrigue: 0);
        RecordRomanticInteractionCommands.Pipeline.Execute(
            state, new RecordRomanticInteractionCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, initiatorId, targetId, 0, 100, null));

        var schemeId = state.SchemeIds.Issue();
        // Progress starts one month's worth of gain short of completion so it resolves on the very next tick.
        var progressPerMonth = SchemeProgressCatalog.BaseProgressPerMonthPercent;
        state.Schemes.Add(
            schemeId,
            Scheme.Create(schemeId, initiatorId, targetId, SchemeType.Seduce, new GameDate(0))
                with
            { Progress = Scheme.MaxValue - progressPerMonth });

        new SchemeProgressSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams(seed: 7)));

        state.Schemes.TryGet(schemeId, out var resolved);
        Assert.That(resolved!.Status, Is.EqualTo(SchemeStatus.Succeeded));
    }

    [Test]
    public void DiscoveryRiskAdvancesIdenticallyForASeduceSchemeAsForACoerciveOne()
    {
        // §7 only touches the success-chance formula; discovery risk must stay byte-for-byte identical
        // for every SchemeType.
        var coerciveState = NewState();
        var coerciveInitiator = AddCharacter(coerciveState, intrigue: 40);
        var coerciveTarget = AddCharacter(coerciveState, intrigue: 60);
        var coerciveSchemeId = coerciveState.SchemeIds.Issue();
        coerciveState.Schemes.Add(
            coerciveSchemeId, Scheme.Create(coerciveSchemeId, coerciveInitiator, coerciveTarget, SchemeType.Coercive, new GameDate(0)));

        var seduceState = NewState();
        var seduceInitiator = AddCharacter(seduceState, intrigue: 40);
        var seduceTarget = AddCharacter(seduceState, intrigue: 60);
        var seduceSchemeId = seduceState.SchemeIds.Issue();
        seduceState.Schemes.Add(
            seduceSchemeId, Scheme.Create(seduceSchemeId, seduceInitiator, seduceTarget, SchemeType.Seduce, new GameDate(0)));

        new SchemeProgressSystem().Tick(coerciveState, new MonthlyTickContext(new GameDate(1), Streams(1)));
        new SchemeProgressSystem().Tick(seduceState, new MonthlyTickContext(new GameDate(1), Streams(1)));

        coerciveState.Schemes.TryGet(coerciveSchemeId, out var coerciveScheme);
        seduceState.Schemes.TryGet(seduceSchemeId, out var seduceScheme);

        Assert.Multiple(() =>
        {
            Assert.That(seduceScheme!.DiscoveryRisk, Is.EqualTo(coerciveScheme!.DiscoveryRisk));
            Assert.That(seduceScheme.Progress, Is.EqualTo(coerciveScheme.Progress));
        });
    }
}
