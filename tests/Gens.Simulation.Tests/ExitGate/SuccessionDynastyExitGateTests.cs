using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Chronicle;
using Gens.Simulation.Commands;
using Gens.Simulation.Economy;
using Gens.Simulation.Epithets;
using Gens.Simulation.Funerary;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.ExitGate;

/// <summary>
/// Proves the Phase 11 exit gate, verbatim from the roadmap: "the vertical-slice campaign can survive
/// at least three successions, including a contested case, while ledgers, property, relationships,
/// history, and saves remain consistent." Combines every package this phase shipped in one continuous
/// run: heirs/eligibility/disputed succession/asset-and-obligation transfer (item 1, <see
/// cref="SuccessionHandoffSystem"/>/<see cref="SuccessionDisputeResolutionSystem"/>), the player-control
/// handoff (item 2, <see cref="PlayerControlHandoffSystem"/>), the Dynasty Chronicle (item 3, <see
/// cref="ChronicleGenerationSystem"/>), funerals/mourning/Memoria (item 4, <see
/// cref="FuneralOpeningSystem"/>/<see cref="FuneralAutoResolutionSystem"/>), and rules-and-provenance
/// epithets (item 5, <see cref="EpithetGenerationSystem"/>) — item 6's own "add succession fixtures"
/// construction-order step at the combined-system, whole-run integration level, matching how <see
/// cref="RivalHousesAndStewardshipSoakTests"/> closed out Phase 10. The isolated, one-system-at-a-time
/// scenarios (ordinary/contested/adoption/debt/absent-heir/extinction) are already covered by <see
/// cref="Tests.Succession.SuccessionTests"/>; this test's job is proving they still hold up wired
/// together across three real generations of one continuously-running household.
///
/// Head deaths are driven directly (flipping <see cref="Character.DeathRecord"/>, the same technique
/// <see cref="Tests.Succession.SuccessionTests"/> already uses) rather than left to <see
/// cref="CharacterLifecycleSystem"/>'s own mortality roll — a real mortality roll cannot be scheduled to
/// land on three chosen generations inside a bounded run, and that system's own soak coverage already
/// lives in Phase 5's <see cref="FamiliaHouseholdSoakTests"/>.
/// </summary>
public sealed class SuccessionDynastyExitGateTests
{
    [Test]
    public void HouseholdSurvivesThreeSuccessionsIncludingAContestedCaseWithConsistentLedgersHistoryAndSaves()
    {
        // A contested first succession needs a seed under which SuccessionHandoffSystem's dispute-
        // trigger roll (20%, SuccessionCatalog.DisputeTriggerChancePercent) actually lands — as
        // RivalHousesAndStewardshipSoakTests does for its own probabilistic events, try a bounded range
        // of campaign seeds until one produces a dispute within the run.
        for (var seed = 1UL; seed < 200UL; seed++)
        {
            var outcome = RunScenario(seed);
            if (outcome.DisputeOpened)
            {
                AssertScenario(outcome);
                return;
            }
        }

        Assert.Fail("Expected at least one seed in the tried range to produce a contested first succession.");
    }

    private static void AssertScenario(ScenarioOutcome outcome)
    {
        Assert.Multiple(() =>
        {
            // Three successions actually happened: the contested Gen0→Gen1 handoff plus two ordinary
            // handoffs (Gen1→Gen2, Gen2→Gen3), and the household is still alive at the end — it never
            // extinguished.
            Assert.That(outcome.DisputeOpened, Is.True);
            Assert.That(outcome.DisputeResolvedToAWinner, Is.True, "The contested dispute must resolve to a living winner, not an empty pool.");
            Assert.That(outcome.OrdinaryHandoffCount, Is.EqualTo(2), "Gen1→Gen2 and Gen2→Gen3 must both be ordinary (single-heir) handoffs.");
            Assert.That(outcome.HouseholdExtinguished, Is.False);
            Assert.That(outcome.FinalState.HouseholdHeadships.TryGet(outcome.HouseholdId, out var finalHeadship), Is.True);
            Assert.That(finalHeadship!.HeadCharacterId, Is.EqualTo(outcome.Gen3HeadId));

            // Asset/obligation transfer (item 1): the original loan is still owed by the same
            // Household, principal untouched, across all three handoffs.
            outcome.FinalState.DebtRecords.TryGet(outcome.DebtId, out var debt);
            Assert.That(debt!.DebtorHouseholdId, Is.EqualTo(outcome.HouseholdId));
            Assert.That(debt.Principal, Is.EqualTo(Money.FromDenarii(500)));

            // History (item 3): the Dynasty Chronicle recorded every headship transition for this
            // Household — three transfers/resolutions plus (item 4) three held funerals.
            var entries = outcome.FinalState.ChronicleEntries.InAscendingOrder()
                .Select(e => e.Value)
                .Where(e => e.HouseholdId == outcome.HouseholdId)
                .ToArray();
            Assert.That(entries.Count(e => e.Category == ChronicleCategory.PoliticsAndOffice), Is.GreaterThanOrEqualTo(3),
                "Expected a Chronicle entry for each of the three headship transitions.");
            Assert.That(entries.Any(e => e.Category == ChronicleCategory.BirthsAndDeaths), Is.True,
                "Expected at least one funeral/death Chronicle entry across three generations of head deaths.");

            // Funerals and Memoria (item 4): all three heads got a Held funeral, and the household's
            // Memoria moved off its untouched starting state.
            Assert.That(outcome.HeldFuneralCount, Is.EqualTo(3));
            Assert.That(outcome.FinalState.MemoriaStates.TryGet(outcome.HouseholdId, out var memoria), Is.True);
            Assert.That(memoria!.Memoria, Is.Not.EqualTo(0));

            // Epithets (item 5): the contested dispute's winner earned the Felix agnomen (§5's
            // "prevailing in a resolved SuccessionDisputeResolvedEvent").
            var felixAgnomens = outcome.FinalState.Agnomens.InAscendingOrder()
                .Select(e => e.Value)
                .Where(a => a.CharacterId == outcome.Gen1HeadId && a.Name == AgnomenCatalog.SuccessionVictoryAgnomenName)
                .ToArray();
            Assert.That(felixAgnomens, Is.Not.Empty, "The contested succession's winner must be awarded the Felix agnomen.");

            // Player control (item 2) followed headship all the way to the current, third-generation head.
            Assert.That(outcome.FinalState.PlayerControls.TryGet(outcome.HouseholdId, out var control), Is.True);
            Assert.That(control!.ControlledCharacterId, Is.EqualTo(outcome.Gen3HeadId));

            // Relationship referential integrity survives three generations of deaths and handoffs.
            var violations = new RelationshipReferentialIntegrityCheck().Check(outcome.FinalState).ToArray();
            Assert.That(violations, Is.Empty);

            // Saves (item 6's own exit-gate clause): the mid-run and final state hashes both survived
            // a save/load round trip untouched.
            Assert.That(outcome.MidRunReloadedHash, Is.EqualTo(outcome.MidRunHashBeforeSave));
            Assert.That(outcome.FinalReloadedHash, Is.EqualTo(StateHasher.Hash(outcome.FinalState)));
        });
    }

    private sealed record ScenarioOutcome(
        WorldState FinalState,
        RuntimeId<Household> HouseholdId,
        RuntimeId<Character> Gen1HeadId,
        RuntimeId<Character> Gen3HeadId,
        RuntimeId<DebtRecord> DebtId,
        bool DisputeOpened,
        bool DisputeResolvedToAWinner,
        int OrdinaryHandoffCount,
        bool HouseholdExtinguished,
        int HeldFuneralCount,
        ulong MidRunHashBeforeSave,
        ulong MidRunReloadedHash,
        ulong FinalReloadedHash);

    private static ScenarioOutcome RunScenario(ulong seed)
    {
        var config = BuildConfig(seed);
        var campaign = CampaignBootstrapper.Bootstrap(config);
        var state = campaign.State;
        var streams = campaign.RandomStreams;
        var householdId = campaign.HouseholdId;

        // Gen0: the founding head, with two adult sons and no Formal Declaration — an ambiguous pool
        // that can trigger a dispute — and a grandchild already waiting under each son's own line, so
        // whichever son ultimately wins the dispute already has an adult heir ready for the next
        // ordinary handoff.
        var gen0HeadId = state.CharacterIds.Issue();
        state.Characters.Add(gen0HeadId, CharacterTestFixtures.Minimal(
            gen0HeadId, praenomen: "Gaius", nomen: "Aurelius", household: householdId,
            birthDate: new GameDate(-700), deathRecord: new DeathRecord(new GameDate(1), DeathCause.OldAge, 70)));

        var sonAId = MakeChild(state, "Aulus", gen0HeadId, birthMonth: -300, household: householdId);
        var sonBId = MakeChild(state, "Marcus", gen0HeadId, birthMonth: -260, household: householdId);
        MakeChild(state, "Sextus", sonAId, birthMonth: -20, household: householdId);
        MakeChild(state, "Titus", sonBId, birthMonth: -20, household: householdId);

        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, gen0HeadId, state.Date));

        var establishControl = PlayerControlCommands.EstablishPipeline.Execute(
            state, new EstablishPlayerControlCommand(state.CommandIds.Issue(), gen0HeadId.ToTaggedString(), state.Date, null, householdId));
        Assert.That(establishControl.Accepted, Is.True);

        // Fund the treasury so every funeral this run opens has money to hold.
        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Treasury,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), Money.FromDenarii(5000)),
                new LedgerPosting(LedgerAccountKey.Mint, -(Money.FromDenarii(5000))),
            });

        // Asset/obligation transfer (item 1): a standing loan that must ride along through every
        // handoff untouched.
        var debt = DebtService.IssueLoan(state, state.Date, campaign.SettlementId, householdId, Money.FromDenarii(500));

        var simulation = NewSimulation();
        var disputeOpened = false;
        var disputeResolvedToAWinner = false;
        var ordinaryHandoffCount = 0;
        var householdExtinguished = false;
        var heldFuneralCount = 0;
        RuntimeId<Character> gen1HeadId = default;
        RuntimeId<Character> gen2HeadId = default;
        RuntimeId<Character> gen3HeadId = default;

        var midRunHashBeforeSave = 0UL;
        var midRunReloadedHash = 0UL;

        const int totalMonths = 900;
        var gen1Killed = false;
        var gen2Killed = false;

        for (var month = 1; month <= totalMonths; month++)
        {
            var events = simulation.Tick(state, state.Date, streams);
            var chronicleEvents = ChronicleGenerationSystem.Generate(state, events);
            var combined = new List<IDomainEvent>(events);
            combined.AddRange(chronicleEvents);
            var epithetEvents = EpithetGenerationSystem.Generate(state, combined);
            combined.AddRange(epithetEvents);

            foreach (var evt in combined)
            {
                switch (evt)
                {
                    case SuccessionDisputeOpenedEvent:
                        disputeOpened = true;
                        break;
                    case SuccessionDisputeResolvedEvent resolved when resolved.WinnerCharacterId is { } winner:
                        disputeResolvedToAWinner = true;
                        gen1HeadId = winner;
                        break;
                    case HouseholdHeadTransferredEvent transferred:
                        ordinaryHandoffCount++;
                        if (gen1Killed && gen2HeadId == default)
                            gen2HeadId = transferred.ToCharacterId;
                        else if (gen2Killed && gen3HeadId == default)
                            gen3HeadId = transferred.ToCharacterId;
                        break;
                    case HouseholdExtinguishedEvent:
                        householdExtinguished = true;
                        break;
                    case FuneralHeldEvent:
                        heldFuneralCount++;
                        break;
                }
            }

            state.AdvanceMonth();

            // Once Gen0's dispute has resolved to a living winner, kill that winner off partway through
            // its own tenure — its own single adult child (already seeded above) then inherits ordinarily.
            if (!gen1Killed && disputeResolvedToAWinner && state.Date.TotalMonths == 300)
            {
                Kill(state, gen1HeadId, state.Date);
                gen1Killed = true;
            }

            // Once Gen1→Gen2 has actually happened, kill Gen2 off in turn — its own single adult child
            // (the great-grandchild seeded under whichever line won) inherits ordinarily too.
            if (gen1Killed && !gen2Killed && gen2HeadId != default && state.Date.TotalMonths == 600)
            {
                MakeChild(state, "Quintus", gen2HeadId, birthMonth: state.Date.TotalMonths - 300, household: householdId);
                Kill(state, gen2HeadId, state.Date);
                gen2Killed = true;
            }

            if (month == 450)
            {
                midRunHashBeforeSave = StateHasher.Hash(state);
                var path = Path.Combine(Path.GetTempPath(), $"gens-phase11-soak-{Guid.NewGuid():N}.gens");
                try
                {
                    SaveWriter.Write(path, state, streams, "0.0.0-test", config.ContentPackHash);
                    var loaded = SaveReader.Read(path);
                    midRunReloadedHash = StateHasher.Hash(loaded.State);
                }
                finally
                {
                    File.Delete(path);
                }
            }

            // Keep ticking past the third handoff itself until that generation's own funeral has
            // actually been held (FuneralAutoResolutionSystem resolves it a couple of months later,
            // FuneraryCatalog.FuneralAutoResolutionAfterMonths after it opened) — otherwise the run
            // would stop one funeral short of the three the assertions below expect.
            if (gen2Killed && gen3HeadId != default && heldFuneralCount >= 3)
                break;

            // Gen0's dispute-trigger roll only ever fires on the very first tick (the head is already
            // dead before the loop starts); a seed that did not land it this run never will, so there
            // is no point running the remaining months just to discard the result.
            if (month == 1 && !disputeOpened)
                return new ScenarioOutcome(
                    state, householdId, gen1HeadId, gen3HeadId, debt.Id, disputeOpened, disputeResolvedToAWinner,
                    ordinaryHandoffCount, householdExtinguished, heldFuneralCount, midRunHashBeforeSave, midRunReloadedHash,
                    FinalReloadedHash: 0);
        }

        var finalReloadPath = Path.Combine(Path.GetTempPath(), $"gens-phase11-soak-final-{Guid.NewGuid():N}.gens");
        ulong finalReloadedHash;
        try
        {
            SaveWriter.Write(finalReloadPath, state, streams, "0.0.0-test", config.ContentPackHash);
            finalReloadedHash = StateHasher.Hash(SaveReader.Read(finalReloadPath).State);
        }
        finally
        {
            File.Delete(finalReloadPath);
        }

        return new ScenarioOutcome(
            state, householdId, gen1HeadId, gen3HeadId, debt.Id, disputeOpened, disputeResolvedToAWinner,
            ordinaryHandoffCount, householdExtinguished, heldFuneralCount, midRunHashBeforeSave, midRunReloadedHash,
            finalReloadedHash);
    }

    /// <summary>Adds a new adult, legitimate child of <paramref name="fatherId"/> and returns its ID.</summary>
    private static RuntimeId<Character> MakeChild(
        WorldState state, string praenomen, RuntimeId<Character> fatherId, int birthMonth, RuntimeId<Household> household)
    {
        var id = state.CharacterIds.Issue();
        state.Characters.Add(id, CharacterTestFixtures.Minimal(
            id, praenomen: praenomen, nomen: "Aurelius", fatherId: fatherId, household: household,
            birthDate: new GameDate(birthMonth)));
        return id;
    }

    /// <summary>Flips <paramref name="characterId"/>'s <see cref="Character.DeathRecord"/> so the next
    /// <see cref="SuccessionHandoffSystem"/>/<see cref="Funerary.FuneralOpeningSystem"/> tick treats them
    /// as freshly dead — the same direct-mutation technique <see cref="Tests.Succession.SuccessionTests"/>
    /// already uses to force a specific handoff scenario deterministically.</summary>
    private static void Kill(WorldState state, RuntimeId<Character> characterId, GameDate date)
    {
        state.Characters.TryGet(characterId, out var character);
        var updated = character! with { DeathRecord = new DeathRecord(date, DeathCause.OldAge, 70) };
        state.Characters.Remove(characterId);
        state.Characters.Add(characterId, updated);
    }

    private static WriteSetVerifyingSimulation NewSimulation() =>
        new(new IMonthlySystem<WorldState>[]
        {
            new SuccessionHandoffSystem(),
            new SuccessionDisputeResolutionSystem(),
            new RegencySystem(),
            new PlayerControlHandoffSystem(),
            new FuneralOpeningSystem(),
            new FuneralAutoResolutionSystem(),
            new ManesObservanceSystem(),
        });

    private static CampaignConfig BuildConfig(ulong seed) => new()
    {
        Seed = seed,
        StartDate = new GameDate(0),
        RulesetId = "classic",
        ContentPackHash = "content-hash-placeholder",
        RegionId = "latium",
        Difficulty = "standard",
    };
}
