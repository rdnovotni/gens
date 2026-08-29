using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Land;
using Gens.Simulation.Policies;
using Gens.Simulation.Random;
using Gens.Simulation.Religion;
using Gens.Simulation.Reputation;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Religion;

/// <summary>Phase 12 item 3 coverage: the household Favor meter and Patron Deity (§2), Reconsecration
/// (§2.1), the Rites Budget's monthly consumption (§3.1), Omens and Auspices (§4), the Priesthood track
/// (§6.2), and the Sacred Calendar's two observance tiers (§5).</summary>
public sealed class ReligionTests
{
    private static (WorldState State, RuntimeId<Household> HouseholdId) HouseholdOnly()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        return (state, householdId);
    }

    private static (WorldState State, RuntimeId<Household> HouseholdId, RuntimeId<Character> HeadId) HouseholdWithHead()
    {
        var (state, householdId) = HouseholdOnly();
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, nomen: "Cornelius", household: householdId));
        return (state, householdId, headId);
    }

    private static RuntimeId<Household> WithPatron(WorldState state, RuntimeId<Character> headId, PatronDeity deity = PatronDeity.Jupiter)
    {
        state.Characters.TryGet(headId, out var head);
        var householdId = head!.Household!.Value;
        SetPatronDeityCommands.Pipeline.Execute(
            state, new SetPatronDeityCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, deity, headId));
        return householdId;
    }

    private static CommandPipeline<WorldState, RespondToOmenCommand> RespondToOmenPipeline()
    {
        var streams = new RandomStreamSet();
        streams.AddDerived(RespondToOmenCommands.OmenIgnoredOutcomeStreamName, 12345UL);
        return RespondToOmenCommands.CreatePipeline(streams);
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

    // ---- Patron Deity & Favor -------------------------------------------------------------

    [Test]
    public void SetPatronDeityCommandFoundsTheHouseholdReligionAtZeroFavor()
    {
        var (state, householdId, headId) = HouseholdWithHead();

        var result = SetPatronDeityCommands.Pipeline.Execute(
            state, new SetPatronDeityCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, PatronDeity.Mars, headId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(HouseholdReligionResolver.HasChosenPatron(state, householdId), Is.True);
            Assert.That(HouseholdReligionResolver.CurrentFavor(state, householdId), Is.EqualTo(0));
            var evt = (PatronDeitySetEvent)result.Events[0];
            Assert.That(evt.Deity, Is.EqualTo(PatronDeity.Mars));
            Assert.That(evt.Visibility, Is.EqualTo(Visibility.Public));
        });
    }

    [Test]
    public void SetPatronDeityCommandRejectsASecondCallForTheSameHousehold()
    {
        var (state, householdId, headId) = HouseholdWithHead();
        WithPatron(state, headId);

        var result = SetPatronDeityCommands.Pipeline.Execute(
            state, new SetPatronDeityCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, PatronDeity.Venus, headId));

        Assert.That(result.Error, Is.EqualTo(SetPatronDeityCommands.AlreadyChosen));
    }

    [Test]
    public void AdjustFavorCommandMovesTheTotalAndEmitsAPublicEvent()
    {
        var (state, _, headId) = HouseholdWithHead();
        var householdId = WithPatron(state, headId);

        var result = AdjustFavorCommands.Pipeline.Execute(
            state, new AdjustFavorCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, 12, "a well-executed rite"));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(HouseholdReligionResolver.CurrentFavor(state, householdId), Is.EqualTo(12));
            var changed = (FavorChangedEvent)result.Events[0];
            Assert.That(changed.PreviousFavor, Is.EqualTo(0));
            Assert.That(changed.NewFavor, Is.EqualTo(12));
            Assert.That(changed.Visibility, Is.EqualTo(Visibility.Public));
        });
    }

    [Test]
    public void AdjustFavorCommandRejectsAZeroDeltaOrAHouseholdWithNoPatronYet()
    {
        var (state, householdId) = HouseholdOnly();

        Assert.Multiple(() =>
        {
            Assert.That(
                AdjustFavorCommands.Pipeline.Execute(
                    state, new AdjustFavorCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, 0, "x")).Error,
                Is.EqualTo(AdjustFavorCommands.ZeroDelta));

            Assert.That(
                AdjustFavorCommands.Pipeline.Execute(
                    state, new AdjustFavorCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, 5, "x")).Error,
                Is.EqualTo(AdjustFavorCommands.NoPatronDeityYet));
        });
    }

    [Test]
    public void FavorCanGoNegativeAndDivineDispleasureTracksTheThreshold()
    {
        var (state, _, headId) = HouseholdWithHead();
        var householdId = WithPatron(state, headId);

        AdjustFavorCommands.Pipeline.Execute(
            state, new AdjustFavorCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, householdId,
                ReligionCatalog.DivineDispleasureThreshold - 1, "an unmaintained shrine"));

        Assert.That(HouseholdReligionResolver.IsDivinelyDispleased(state, householdId), Is.True);
    }

    // ---- Reconsecration ---------------------------------------------------------------------

    [Test]
    public void ReconsecrateCommandRequiresAGenuineHeadshipChangeAndResetsFavor()
    {
        var (state, householdId, headId) = HouseholdWithHead();
        WithPatron(state, headId, PatronDeity.Jupiter);
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, headId, new GameDate(0)));
        Fund(state, householdId, Money.FromDenarii(100));
        AdjustFavorCommands.Pipeline.Execute(
            state, new AdjustFavorCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, 30, "seed favor"));

        // Same head still in charge — must be rejected.
        var sameHeadResult = ReconsecrateCommands.Pipeline.Execute(
            state, new ReconsecrateCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, householdId, PatronDeity.Venus));
        Assert.That(sameHeadResult.Error, Is.EqualTo(ReconsecrateCommands.NotANewHeadship));

        // A new head assumes headship — Reconsecration now opens.
        var newHeadId = state.CharacterIds.Issue();
        state.Characters.Add(newHeadId, CharacterTestFixtures.Minimal(newHeadId, nomen: "Aurelius", household: householdId));
        state.HouseholdHeadships.Remove(householdId);
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, newHeadId, new GameDate(3)));

        var result = ReconsecrateCommands.Pipeline.Execute(
            state, new ReconsecrateCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, householdId, PatronDeity.Venus));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(HouseholdReligionResolver.CurrentFavor(state, householdId), Is.EqualTo(0));
            state.HouseholdReligions.TryGet(householdId, out var religion);
            Assert.That(religion!.PatronDeity, Is.EqualTo(PatronDeity.Venus));
            Assert.That(religion.ConsecratedUnderHeadCharacterId, Is.EqualTo(newHeadId));
            Assert.That(result.Events.OfType<ReconsecrationEvent>().Count(), Is.EqualTo(1));
            Assert.That(result.Events.OfType<LedgerTransactionPostedEvent>().Count(), Is.EqualTo(1));

            var account = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var acc) ? acc!.Balance : Money.Zero;
            Assert.That(account, Is.EqualTo(Money.FromDenarii(100) - ReligionCatalog.ReconsecrationCeremonyCost));
        });
    }

    [Test]
    public void ReconsecrateCommandRejectsInsufficientTreasury()
    {
        var (state, householdId, headId) = HouseholdWithHead();
        WithPatron(state, headId);
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, headId, new GameDate(0)));

        var newHeadId = state.CharacterIds.Issue();
        state.Characters.Add(newHeadId, CharacterTestFixtures.Minimal(newHeadId, nomen: "Aurelius", household: householdId));
        state.HouseholdHeadships.Remove(householdId);
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, newHeadId, new GameDate(1)));

        var result = ReconsecrateCommands.Pipeline.Execute(
            state, new ReconsecrateCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, PatronDeity.Ceres));

        Assert.That(result.Error, Is.EqualTo(ReconsecrateCommands.InsufficientTreasury));
    }

    // ---- Rites Budget cycle -------------------------------------------------------------------

    [Test]
    public void FavorCycleSystemDeclaresThePhaseAndReadWriteSet()
    {
        var system = new FavorCycleSystem();
        Assert.Multiple(() =>
        {
            Assert.That(system.Phase, Is.EqualTo(TickPhase.RelationshipsActors));
            Assert.That(system.Reads, Is.EquivalentTo(new[] { "householdReligions", "householdPolicies", "ledgerAccounts" }));
        });
    }

    [Test]
    public void FavorCycleSystemDrawsTheTreasuryAndAppliesTheTiersStabilityModifier()
    {
        var (state, _, headId) = HouseholdWithHead();
        var householdId = WithPatron(state, headId);
        Fund(state, householdId, Money.FromDenarii(100));

        ChangeRitesBudgetCommands.Pipeline.Execute(
            state, new ChangeRitesBudgetCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, RitesBudgetTier.Lavish));

        var events = new FavorCycleSystem().Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));

        Assert.Multiple(() =>
        {
            Assert.That(events.OfType<LedgerTransactionPostedEvent>().Count(), Is.EqualTo(1));
            var account = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var acc) ? acc!.Balance : Money.Zero;
            Assert.That(account, Is.EqualTo(Money.FromDenarii(100) - RitesBudgetCatalog.TreasuryDrawPerMonth(RitesBudgetTier.Lavish)));
            Assert.That(
                HouseholdReligionResolver.CurrentFavor(state, householdId),
                Is.EqualTo(RitesBudgetCatalog.DivineFavorStabilityModifier(RitesBudgetTier.Lavish)));
        });
    }

    // ---- Omens ---------------------------------------------------------------------------------

    [Test]
    public void RaiseOmenCommandCreatesAPendingOmenThemedToThePatronDeity()
    {
        var (state, _, headId) = HouseholdWithHead();
        var householdId = WithPatron(state, headId, PatronDeity.Neptune);

        var result = RaiseOmenCommands.Pipeline.Execute(
            state, new RaiseOmenCommand(state.CommandIds.Issue(), "system", new GameDate(1), null, householdId, 2));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            var omen = state.OmenEvents.InAscendingOrder().First().Value;
            Assert.That(omen.ThemedDeity, Is.EqualTo(PatronDeity.Neptune));
            Assert.That(omen.Outcome, Is.EqualTo(OmenOutcome.Pending));
        });
    }

    [Test]
    public void RaiseOmenCommandRejectsSeverityOutOfRangeOrNoPatronDeity()
    {
        var (state, householdId) = HouseholdOnly();

        Assert.That(
            RaiseOmenCommands.Pipeline.Execute(
                state, new RaiseOmenCommand(state.CommandIds.Issue(), "system", new GameDate(1), null, householdId, 2)).Error,
            Is.EqualTo(RaiseOmenCommands.NoPatronDeityYet));

        var (state2, _, headId2) = HouseholdWithHead();
        var householdId2 = WithPatron(state2, headId2);
        Assert.That(
            RaiseOmenCommands.Pipeline.Execute(
                state2, new RaiseOmenCommand(state2.CommandIds.Issue(), "system", new GameDate(1), null, householdId2, 9)).Error,
            Is.EqualTo(RaiseOmenCommands.SeverityOutOfRange));
    }

    [Test]
    public void RespondToOmenCommandHeedingAlwaysAvertsAndGainsFavor()
    {
        var (state, _, headId) = HouseholdWithHead();
        var householdId = WithPatron(state, headId);
        RaiseOmenCommands.Pipeline.Execute(
            state, new RaiseOmenCommand(state.CommandIds.Issue(), "system", new GameDate(1), null, householdId, 3));
        var omenId = state.OmenEvents.InAscendingOrder().First().Key;

        var pipeline = RespondToOmenPipeline();
        var result = pipeline.Execute(
            state, new RespondToOmenCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, omenId, headId, OmenChoice.Heeded));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            state.OmenEvents.TryGet(omenId, out var omen);
            Assert.That(omen!.Outcome, Is.EqualTo(OmenOutcome.Averted));
            Assert.That(HouseholdReligionResolver.CurrentFavor(state, householdId), Is.EqualTo(ReligionCatalog.OmenHeededFavorGain));
        });
    }

    [Test]
    public void RespondToOmenCommandRejectsAnAlreadyResolvedOmen()
    {
        var (state, _, headId) = HouseholdWithHead();
        var householdId = WithPatron(state, headId);
        RaiseOmenCommands.Pipeline.Execute(
            state, new RaiseOmenCommand(state.CommandIds.Issue(), "system", new GameDate(1), null, householdId, 1));
        var omenId = state.OmenEvents.InAscendingOrder().First().Key;
        var pipeline = RespondToOmenPipeline();
        pipeline.Execute(state, new RespondToOmenCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, omenId, headId, OmenChoice.Heeded));

        var result = pipeline.Execute(
            state, new RespondToOmenCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, omenId, headId, OmenChoice.Ignored));

        Assert.That(result.Error, Is.EqualTo(RespondToOmenCommands.AlreadyResolved));
    }

    [Test]
    public void RespondToOmenCommandIgnoringResolvesToAConsequenceOrNoConsequenceAndFavorMatchesWhicheverLanded()
    {
        var (state, _, headId) = HouseholdWithHead();
        var householdId = WithPatron(state, headId);
        AdjustFavorCommands.Pipeline.Execute(
            state, new AdjustFavorCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, householdId, 50, "seed"));

        RaiseOmenCommands.Pipeline.Execute(
            state, new RaiseOmenCommand(state.CommandIds.Issue(), "system", new GameDate(1), null, householdId, ReligionCatalog.MaxOmenSeverity));
        var omenId = state.OmenEvents.InAscendingOrder().First().Key;

        // The severity-scaled roll (§4.1) is a real, seed-driven PCG32 draw — this suite reads the
        // resolver's own recorded Outcome rather than hand-computing a specific draw, matching
        // FavorExpirationSystem's own precedent of asserting on state after a real random draw instead
        // of mocking the stream: both possible outcomes are asserted to move Favor by exactly the
        // catalog-documented amount for that outcome, so the test is meaningful regardless of which one
        // this particular seed happens to produce.
        var pipeline = RespondToOmenPipeline();
        var result = pipeline.Execute(
            state, new RespondToOmenCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, omenId, headId, OmenChoice.Ignored));

        state.OmenEvents.TryGet(omenId, out var omen);
        var favorAfter = HouseholdReligionResolver.CurrentFavor(state, householdId);
        var expectedFavor = omen!.Outcome == OmenOutcome.ConsequenceLanded ? 50 - ReligionCatalog.OmenIgnoredConsequenceFavorLoss : 50;

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(omen.Outcome, Is.AnyOf(OmenOutcome.ConsequenceLanded, OmenOutcome.NoConsequence));
            Assert.That(favorAfter, Is.EqualTo(expectedFavor));
        });
    }

    [Test]
    public void RespondToOmenCommandImpiousCharacterIsImmuneToTheIgnoredConsequencePenaltyRegardlessOfOutcome()
    {
        var (state, householdId) = HouseholdOnly();
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(
            headId,
            CharacterTestFixtures.Minimal(
                headId, nomen: "Cornelius", household: householdId, traits: new[] { ReligionCatalog.ImpiousTraitId }));
        WithPatron(state, headId);
        AdjustFavorCommands.Pipeline.Execute(
            state, new AdjustFavorCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, householdId, 50, "seed"));

        RaiseOmenCommands.Pipeline.Execute(
            state, new RaiseOmenCommand(state.CommandIds.Issue(), "system", new GameDate(1), null, householdId, ReligionCatalog.MaxOmenSeverity));
        var omenId = state.OmenEvents.InAscendingOrder().First().Key;

        var pipeline = RespondToOmenPipeline();
        pipeline.Execute(
            state, new RespondToOmenCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, omenId, headId, OmenChoice.Ignored));

        // Impious is immune to the ConsequenceLanded penalty and is never Zealous, so Favor is
        // untouched by either outcome branch — the one case in this domain where the roll's own result
        // provably cannot matter, asserted directly rather than branching on it.
        Assert.That(HouseholdReligionResolver.CurrentFavor(state, householdId), Is.EqualTo(50));
    }

    // ---- Priesthoods -----------------------------------------------------------------------

    private static (WorldState State, RuntimeId<Settlement> SettlementId, RuntimeId<Household> HouseholdId, RuntimeId<Character> CharacterId)
        DevoutLearnedCitizen()
    {
        var state = new WorldState(new GameDate(0));
        var settlementId = state.SettlementIds.Issue();
        var householdId = state.HouseholdIds.Issue();
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(
            characterId,
            CharacterTestFixtures.Minimal(
                characterId, nomen: "Metellus", household: householdId, traits: new[] { ReligionCatalog.DevoutTraitId },
                attributes: new CoreAttributes(10, 10, 10, 10, ReligionCatalog.PriesthoodLearningThreshold)));
        return (state, settlementId, householdId, characterId);
    }

    [Test]
    public void AppointPriesthoodCommandSeatsAnAugurAndGrantsTheOneTimeFavorAndDignitasGain()
    {
        var (state, settlementId, householdId, characterId) = DevoutLearnedCitizen();
        WithPatron(state, characterId);

        var result = AppointPriesthoodCommands.Pipeline.Execute(
            state,
            new AppointPriesthoodCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, characterId, settlementId, PriesthoodOffice.Augur, null));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(PriesthoodResolver.ActiveRecord(state, settlementId, PriesthoodOffice.Augur, characterId), Is.Not.Null);
            Assert.That(HouseholdReligionResolver.CurrentFavor(state, householdId), Is.EqualTo(ReligionCatalog.PriesthoodAssumedFavorGain));
            Assert.That(DignitasResolver.Current(state, householdId), Is.EqualTo(ReligionCatalog.PriesthoodAssumedDignitasGain));
            Assert.That(result.Events.OfType<PriesthoodAssumedEvent>().First().Visibility, Is.EqualTo(Visibility.Public));
        });
    }

    [Test]
    public void AppointPriesthoodCommandRejectsInsufficientPietyOrLearningOrIneligibleStatus()
    {
        var state = new WorldState(new GameDate(0));
        var settlementId = state.SettlementIds.Issue();
        var householdId = state.HouseholdIds.Issue();

        var noTraitId = state.CharacterIds.Issue();
        state.Characters.Add(
            noTraitId,
            CharacterTestFixtures.Minimal(
                noTraitId, nomen: "Naevius", household: householdId,
                attributes: new CoreAttributes(10, 10, 10, 10, ReligionCatalog.PriesthoodLearningThreshold)));
        Assert.That(
            AppointPriesthoodCommands.Pipeline.Execute(
                state,
                new AppointPriesthoodCommand(
                    state.CommandIds.Issue(), "player", new GameDate(1), null, noTraitId, settlementId, PriesthoodOffice.Augur, null)).Error,
            Is.EqualTo(AppointPriesthoodCommands.InsufficientPiety));

        var unlearnedId = state.CharacterIds.Issue();
        state.Characters.Add(
            unlearnedId,
            CharacterTestFixtures.Minimal(
                unlearnedId, nomen: "Unlearned", household: householdId, traits: new[] { ReligionCatalog.DevoutTraitId },
                attributes: new CoreAttributes(10, 10, 10, 10, 1)));
        Assert.That(
            AppointPriesthoodCommands.Pipeline.Execute(
                state,
                new AppointPriesthoodCommand(
                    state.CommandIds.Issue(), "player", new GameDate(1), null, unlearnedId, settlementId, PriesthoodOffice.Augur, null)).Error,
            Is.EqualTo(AppointPriesthoodCommands.InsufficientLearning));

        var peregrineId = state.CharacterIds.Issue();
        state.Characters.Add(
            peregrineId,
            CharacterTestFixtures.Minimal(
                peregrineId, nomen: "Peregrinus", household: householdId, traits: new[] { ReligionCatalog.ZealousTraitId },
                attributes: new CoreAttributes(10, 10, 10, 10, ReligionCatalog.PriesthoodLearningThreshold),
                status: LegalStatus.Peregrine));
        Assert.That(
            AppointPriesthoodCommands.Pipeline.Execute(
                state,
                new AppointPriesthoodCommand(
                    state.CommandIds.Issue(), "player", new GameDate(1), null, peregrineId, settlementId, PriesthoodOffice.Augur, null)).Error,
            Is.EqualTo(AppointPriesthoodCommands.IneligibleLegalStatus));
    }

    [Test]
    public void AppointPriesthoodCommandGatesFlamenOnMatchingThePatronDeityAndPontifexOnAPriorOffice()
    {
        var (state, settlementId, householdId, characterId) = DevoutLearnedCitizen();
        WithPatron(state, characterId, PatronDeity.Mars);

        // Wrong Flamen deity is rejected.
        Assert.That(
            AppointPriesthoodCommands.Pipeline.Execute(
                state,
                new AppointPriesthoodCommand(
                    state.CommandIds.Issue(), "player", new GameDate(1), null, characterId, settlementId, PriesthoodOffice.Flamen,
                    PatronDeity.Venus)).Error,
            Is.EqualTo(AppointPriesthoodCommands.FlamenDeityMismatch));

        // Matching Flamen deity succeeds.
        var flamenResult = AppointPriesthoodCommands.Pipeline.Execute(
            state,
            new AppointPriesthoodCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, characterId, settlementId, PriesthoodOffice.Flamen, PatronDeity.Mars));
        Assert.That(flamenResult.Accepted, Is.True);

        // Pontifex without a prior office is rejected for a fresh Character.
        var otherId = state.CharacterIds.Issue();
        state.Characters.Add(
            otherId,
            CharacterTestFixtures.Minimal(
                otherId, nomen: "Fabius", household: householdId, traits: new[] { ReligionCatalog.ZealousTraitId },
                attributes: new CoreAttributes(10, 10, 10, 10, ReligionCatalog.PriesthoodLearningThreshold)));
        Assert.That(
            AppointPriesthoodCommands.Pipeline.Execute(
                state,
                new AppointPriesthoodCommand(
                    state.CommandIds.Issue(), "player", new GameDate(2), null, otherId, settlementId, PriesthoodOffice.Pontifex, null)).Error,
            Is.EqualTo(AppointPriesthoodCommands.PontifexRequiresPriorOffice));

        // The already-seated Augur/Flamen (characterId already holds Flamen above) can become Pontifex.
        var pontifexResult = AppointPriesthoodCommands.Pipeline.Execute(
            state,
            new AppointPriesthoodCommand(
                state.CommandIds.Issue(), "player", new GameDate(3), null, characterId, settlementId, PriesthoodOffice.Pontifex, null));
        Assert.That(pontifexResult.Accepted, Is.True);
    }

    [Test]
    public void PriesthoodTrickleSystemAppliesMonthlyFavorAndDignitasAndVacatesOnDeath()
    {
        var (state, settlementId, householdId, characterId) = DevoutLearnedCitizen();
        WithPatron(state, characterId);
        AppointPriesthoodCommands.Pipeline.Execute(
            state,
            new AppointPriesthoodCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, characterId, settlementId, PriesthoodOffice.Augur, null));

        var favorBefore = HouseholdReligionResolver.CurrentFavor(state, householdId);
        var dignitasBefore = DignitasResolver.Current(state, householdId);

        new PriesthoodTrickleSystem().Tick(state, new MonthlyTickContext(new GameDate(2), new RandomStreamSet()));

        Assert.Multiple(() =>
        {
            Assert.That(HouseholdReligionResolver.CurrentFavor(state, householdId), Is.EqualTo(favorBefore + ReligionCatalog.AugurMonthlyFavor));
            Assert.That(DignitasResolver.Current(state, householdId), Is.EqualTo(dignitasBefore + ReligionCatalog.AugurMonthlyDignitas));
        });

        // Holder dies — the seat is vacated, not re-trickled.
        state.Characters.TryGet(characterId, out var character);
        state.Characters.Remove(characterId);
        state.Characters.Add(characterId, character! with { DeathRecord = new DeathRecord(new GameDate(3), DeathCause.Disease, 40) });

        var events = new PriesthoodTrickleSystem().Tick(state, new MonthlyTickContext(new GameDate(4), new RandomStreamSet()));

        Assert.Multiple(() =>
        {
            Assert.That(events.OfType<PriesthoodVacatedEvent>().Count(), Is.EqualTo(1));
            var record = PriesthoodResolver.ActiveRecord(state, settlementId, PriesthoodOffice.Augur, characterId);
            Assert.That(record, Is.Null);
        });
    }

    // ---- Auspices --------------------------------------------------------------------------

    [Test]
    public void CommissionAuspicesCommandReadsSuperiorReliabilityForAnActiveAugur()
    {
        var (state, settlementId, householdId, characterId) = DevoutLearnedCitizen();
        WithPatron(state, characterId);
        Fund(state, householdId, Money.FromDenarii(50));
        AppointPriesthoodCommands.Pipeline.Execute(
            state,
            new AppointPriesthoodCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, characterId, settlementId, PriesthoodOffice.Augur, null));
        var favorBefore = HouseholdReligionResolver.CurrentFavor(state, householdId);

        var result = CommissionAuspicesCommands.Pipeline.Execute(
            state,
            new CommissionAuspicesCommand(
                state.CommandIds.Issue(), "player", new GameDate(2), null, householdId, settlementId, characterId, "militaryCampaign"));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            var commissioned = (AuspicesCommissionedEvent)result.Events.OfType<AuspicesCommissionedEvent>().First();
            Assert.That(commissioned.ReliabilityTier, Is.EqualTo(AuspicesReliabilityTier.AugurSuperior));
            Assert.That(
                HouseholdReligionResolver.CurrentFavor(state, householdId),
                Is.EqualTo(favorBefore + ReligionCatalog.AuspicesAugurFavorGain));
        });
    }

    [Test]
    public void CommissionAuspicesCommandReadsHouseholdDefaultWithNoPerformerAndRejectsInsufficientTreasury()
    {
        var (state, _, headId) = HouseholdWithHead();
        var householdId = WithPatron(state, headId);
        var settlementId = state.SettlementIds.Issue();

        Assert.That(
            CommissionAuspicesCommands.Pipeline.Execute(
                state,
                new CommissionAuspicesCommand(
                    state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, settlementId, null, "travel")).Error,
            Is.EqualTo(CommissionAuspicesCommands.InsufficientTreasury));

        Fund(state, householdId, Money.FromDenarii(50));
        var result = CommissionAuspicesCommands.Pipeline.Execute(
            state,
            new CommissionAuspicesCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, settlementId, null, "travel"));

        var commissioned = (AuspicesCommissionedEvent)result.Events.OfType<AuspicesCommissionedEvent>().First();
        Assert.That(commissioned.ReliabilityTier, Is.EqualTo(AuspicesReliabilityTier.HouseholdDefault));
    }

    // ---- Sacred Calendar ---------------------------------------------------------------------

    [Test]
    public void ObserveFeastDayCommandAppliesTheSmallPassiveFavorTick()
    {
        var (state, _, headId) = HouseholdWithHead();
        var householdId = WithPatron(state, headId);

        var result = ObserveFeastDayCommands.Pipeline.Execute(
            state, new ObserveFeastDayCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, "Vestalia"));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(HouseholdReligionResolver.CurrentFavor(state, householdId), Is.EqualTo(ReligionCatalog.PassiveFeastDayFavorGain));
        });
    }

    [Test]
    public void FundFestivalCelebrationCommandPaysAFavorAndDignitasBoostSizedToTheSpend()
    {
        var (state, _, headId) = HouseholdWithHead();
        var householdId = WithPatron(state, headId);
        var settlementId = state.SettlementIds.Issue();
        Fund(state, householdId, Money.FromDenarii(200));

        var result = FundFestivalCelebrationCommands.Pipeline.Execute(
            state,
            new FundFestivalCelebrationCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, settlementId, "Cerealia", Money.FromDenarii(100)));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(HouseholdReligionResolver.CurrentFavor(state, householdId), Is.EqualTo(100 / ReligionCatalog.FestivalFavorPerDenarii));
            Assert.That(DignitasResolver.Current(state, householdId), Is.EqualTo(100 / ReligionCatalog.FestivalDignitasPerDenarii));

            var account = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var acc) ? acc!.Balance : Money.Zero;
            Assert.That(account, Is.EqualTo(Money.FromDenarii(100)));
        });
    }

    [Test]
    public void FundFestivalCelebrationCommandRejectsInsufficientTreasuryOrNonPositiveAmount()
    {
        var (state, _, headId) = HouseholdWithHead();
        var householdId = WithPatron(state, headId);
        var settlementId = state.SettlementIds.Issue();

        Assert.That(
            FundFestivalCelebrationCommands.Pipeline.Execute(
                state,
                new FundFestivalCelebrationCommand(
                    state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, settlementId, "Saturnalia", Money.Zero)).Error,
            Is.EqualTo(FundFestivalCelebrationCommands.AmountMustBePositive));

        Assert.That(
            FundFestivalCelebrationCommands.Pipeline.Execute(
                state,
                new FundFestivalCelebrationCommand(
                    state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, settlementId, "Saturnalia",
                    Money.FromDenarii(10))).Error,
            Is.EqualTo(FundFestivalCelebrationCommands.InsufficientTreasury));
    }

    // ---- Save round trip & determinism --------------------------------------------------------

    [Test]
    public void ReligionStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, settlementId, householdId, characterId) = DevoutLearnedCitizen();
        WithPatron(state, characterId, PatronDeity.Minerva);
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, characterId, new GameDate(0)));
        Fund(state, householdId, Money.FromDenarii(300));

        AppointPriesthoodCommands.Pipeline.Execute(
            state,
            new AppointPriesthoodCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, characterId, settlementId, PriesthoodOffice.Augur, null));

        RaiseOmenCommands.Pipeline.Execute(
            state, new RaiseOmenCommand(state.CommandIds.Issue(), "system", new GameDate(1), null, householdId, 2));
        var omenId = state.OmenEvents.InAscendingOrder().First().Key;
        RespondToOmenPipeline().Execute(
            state, new RespondToOmenCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, omenId, characterId, OmenChoice.Heeded));

        FundFestivalCelebrationCommands.Pipeline.Execute(
            state,
            new FundFestivalCelebrationCommand(
                state.CommandIds.Issue(), "player", new GameDate(2), null, householdId, settlementId, "Cerealia", Money.FromDenarii(40)));

        new FavorCycleSystem().Tick(state, new MonthlyTickContext(new GameDate(3), new RandomStreamSet()));
        new PriesthoodTrickleSystem().Tick(state, new MonthlyTickContext(new GameDate(3), new RandomStreamSet()));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.HouseholdReligions.Count, Is.EqualTo(1));
            restored.HouseholdReligions.TryGet(householdId, out var religion);
            Assert.That(religion!.PatronDeity, Is.EqualTo(PatronDeity.Minerva));
            Assert.That(religion.Favor, Is.EqualTo(HouseholdReligionResolver.CurrentFavor(state, householdId)));

            Assert.That(restored.OmenEvents.Count, Is.EqualTo(1));
            restored.OmenEvents.TryGet(omenId, out var restoredOmen);
            Assert.That(restoredOmen!.Outcome, Is.EqualTo(OmenOutcome.Averted));

            Assert.That(restored.PriesthoodRecords.Count, Is.EqualTo(1));
            Assert.That(PriesthoodResolver.ActiveRecord(restored, settlementId, PriesthoodOffice.Augur, characterId), Is.Not.Null);

            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        });
    }
}
