using Gens.Simulation.Characters;
using Gens.Simulation.Ledger;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using Gens.Simulation.Wanderers;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Wanderers;

public sealed class WanderersSaveRoundTripTests
{
    [Test]
    public void WanderersAndEngagementsRoundTripThroughTheDtoAndDeterministicHashStaysStable()
    {
        var state = new WorldState(new GameDate(40));
        var playerHouseholdId = state.HouseholdIds.Issue();
        var rivalHouseholdId = state.HouseholdIds.Issue();
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, Tests.Characters.CharacterTestFixtures.Minimal(
            characterId, household: playerHouseholdId));

        // A contested, still-wandering philosopher with a real multi-stop itinerary.
        var contestedId = state.WandererIds.Issue();
        var contested = Wanderer.Create(
            contestedId,
            new CharacterName("Gaius", "Fabius", "Rhetor"),
            Sex.Male,
            new GameDate(-300),
            LegalStatus.Peregrine,
            WandererTestFixtures.Culture,
            WandererType.PhilosopherRhetorician,
            WandererTestFixtures.Seat,
            fame: 72,
            arrivalDate: new GameDate(30));
        state.Wanderers.Add(contestedId, contested with
        {
            Itinerary = new[]
            {
                new WandererItineraryStop(WandererTestFixtures.Port, 27),
                new WandererItineraryStop(WandererTestFixtures.Seat, 30),
            },
            FameTrend = WandererFameTrend.Rising,
            MonthsSinceLastEngagement = 6,
            InterestedHouseholdIds = new[] { playerHouseholdId, rivalHouseholdId },
        });

        // A recruited physician, cognomen-less, with the itinerary ended and a resulting Character.
        var recruitedId = state.WandererIds.Issue();
        var recruited = Wanderer.Create(
            recruitedId,
            new CharacterName("Lucius", "Julius", null),
            Sex.Female,
            new GameDate(-280),
            LegalStatus.RomanCitizen,
            WandererTestFixtures.Culture,
            WandererType.Physician,
            WandererTestFixtures.Shrine,
            fame: 44,
            arrivalDate: new GameDate(20));
        state.Wanderers.Add(recruitedId, recruited with
        {
            Status = WandererStatus.Recruited,
            IsActivelyTracked = false,
            FameTrend = WandererFameTrend.Declining,
            Itinerary = Array.Empty<WandererItineraryStop>(),
            CommittedHouseholdId = playerHouseholdId,
            RecruitedCharacterId = characterId,
        });

        var hostEngagementId = state.WandererEngagementIds.Issue();
        state.WandererEngagements.Add(hostEngagementId, WandererEngagement.Create(
            hostEngagementId, contestedId, rivalHouseholdId, WandererEngagementType.Host,
            new GameDate(35), Money.FromDenarii(120), dignitasGained: 6, wandererFameGained: 5,
            healthRestored: 0));

        var treatmentEngagementId = state.WandererEngagementIds.Issue();
        state.WandererEngagements.Add(treatmentEngagementId, WandererEngagement.Create(
            treatmentEngagementId, recruitedId, playerHouseholdId, WandererEngagementType.Host,
            new GameDate(36), Money.FromDenarii(100), dignitasGained: 2, wandererFameGained: 3,
            healthRestored: 15, beneficiaryCharacterId: characterId));

        var recruitEngagementId = state.WandererEngagementIds.Issue();
        state.WandererEngagements.Add(recruitEngagementId, WandererEngagement.Create(
            recruitEngagementId, recruitedId, playerHouseholdId, WandererEngagementType.Recruit,
            new GameDate(37), Money.FromDenarii(450), dignitasGained: 0, wandererFameGained: 0,
            healthRestored: 0, beneficiaryCharacterId: null, resultingCharacterId: characterId,
            resultingDutySlot: DutySlot.Physician));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.WandererIds.Peek, Is.EqualTo(state.WandererIds.Peek));
            Assert.That(restored.WandererEngagementIds.Peek, Is.EqualTo(state.WandererEngagementIds.Peek));
            Assert.That(restored.Wanderers.Count, Is.EqualTo(2));
            Assert.That(restored.WandererEngagements.Count, Is.EqualTo(3));

            restored.Wanderers.TryGet(contestedId, out var restoredContested);
            Assert.That(restoredContested!.Name, Is.EqualTo(new CharacterName("Gaius", "Fabius", "Rhetor")));
            Assert.That(restoredContested.Sex, Is.EqualTo(Sex.Male));
            Assert.That(restoredContested.BirthDate, Is.EqualTo(new GameDate(-300)));
            Assert.That(restoredContested.LegalStatus, Is.EqualTo(LegalStatus.Peregrine));
            Assert.That(restoredContested.Culture, Is.EqualTo(WandererTestFixtures.Culture));
            Assert.That(restoredContested.Type, Is.EqualTo(WandererType.PhilosopherRhetorician));
            Assert.That(restoredContested.Fame, Is.EqualTo(72));
            Assert.That(restoredContested.FameTrend, Is.EqualTo(WandererFameTrend.Rising));
            Assert.That(restoredContested.MonthsSinceLastEngagement, Is.EqualTo(6));
            Assert.That(restoredContested.IsActivelyTracked, Is.True);
            Assert.That(restoredContested.Itinerary.Select(stop => stop.LocationId),
                Is.EqualTo(new[] { WandererTestFixtures.Port, WandererTestFixtures.Seat }));
            Assert.That(restoredContested.Itinerary[0].ArrivalMonth, Is.EqualTo(27));
            Assert.That(restoredContested.InterestedHouseholdIds,
                Is.EqualTo(new[] { playerHouseholdId, rivalHouseholdId }));
            Assert.That(restoredContested.CommittedHouseholdId, Is.Null);
            Assert.That(restoredContested.RecruitedCharacterId, Is.Null);

            restored.Wanderers.TryGet(recruitedId, out var restoredRecruited);
            Assert.That(restoredRecruited!.Name.Cognomen, Is.Null);
            Assert.That(restoredRecruited.Sex, Is.EqualTo(Sex.Female));
            Assert.That(restoredRecruited.Status, Is.EqualTo(WandererStatus.Recruited));
            Assert.That(restoredRecruited.IsActivelyTracked, Is.False);
            Assert.That(restoredRecruited.Itinerary, Is.Empty);
            Assert.That(restoredRecruited.CommittedHouseholdId, Is.EqualTo(playerHouseholdId));
            Assert.That(restoredRecruited.RecruitedCharacterId, Is.EqualTo(characterId));

            restored.WandererEngagements.TryGet(treatmentEngagementId, out var restoredTreatment);
            Assert.That(restoredTreatment!.HealthRestored, Is.EqualTo(15));
            Assert.That(restoredTreatment.BeneficiaryCharacterId, Is.EqualTo(characterId));
            Assert.That(restoredTreatment.FeePaid, Is.EqualTo(Money.FromDenarii(100)));
            Assert.That(restoredTreatment.ResultingDutySlot, Is.Null);

            restored.WandererEngagements.TryGet(recruitEngagementId, out var restoredRecruit);
            Assert.That(restoredRecruit!.EngagementType, Is.EqualTo(WandererEngagementType.Recruit));
            Assert.That(restoredRecruit.ResultingCharacterId, Is.EqualTo(characterId));
            Assert.That(restoredRecruit.ResultingDutySlot, Is.EqualTo(DutySlot.Physician));

            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        });
    }

    [Test]
    public void ACampaignWithNoWandererIsUnaffectedByTheNewPartitions()
    {
        var state = new WorldState(new GameDate(5));
        var beforeHash = StateHasher.Hash(state);

        var restored = WorldStateMapper.ToWorldState(WorldStateMapper.ToDto(state));

        Assert.Multiple(() =>
        {
            Assert.That(restored.Wanderers.Count, Is.Zero);
            Assert.That(restored.WandererEngagements.Count, Is.Zero);
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        });
    }
}
