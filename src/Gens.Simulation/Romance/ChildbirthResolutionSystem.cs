using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Companions;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Romance;

/// <summary>Emitted whenever <see cref="ChildbirthResolutionSystem"/> resolves a due <see
/// cref="PregnancyRecord"/>, regardless of outcome (child survives, child is lost, mother dies, or the
/// mother had already died beforehand of an unrelated cause). Private to both parents, matching this
/// module's own "interior state, not broadcast" visibility convention — the separate <see
/// cref="Characters.CharacterBornEvent"/> (public) and <see cref="Characters.CharacterDiedEvent"/>
/// (public) already carry the parts of this outcome the rest of the campaign needs to see.</summary>
public sealed record PregnancyResolvedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<PregnancyRecord> PregnancyId,
    RuntimeId<Character> MotherId,
    RuntimeId<Character> FatherId,
    bool MotherSurvived,
    RuntimeId<Character>? BornChildId) : IDomainEvent
{
    public string Type => "romance.pregnancyResolved";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { MotherId.ToTaggedString(), FatherId.ToTaggedString() };
    public string? CausationId => null;
    public Visibility Visibility => Visibility.Private(MotherId.ToTaggedString(), FatherId.ToTaggedString());
}

/// <summary>
/// The monthly childbirth-resolution tick (<c>gens-romance-sexuality-lineage-design.md</c> §9): every
/// unresolved <see cref="PregnancyRecord"/> whose <see cref="PregnancyRecord.DueDate"/> has arrived is
/// resolved exactly once, either delivering a live child via <see cref="Characters.BirthCharacterCommands"/>
/// (the newborn's <see cref="Characters.Legitimacy"/> is derived there, unchanged by this system — see
/// <see cref="Characters.BirthCharacterCommand"/>'s own doc comment), losing the infant, losing the
/// mother, or both, per two independent risk rolls (<see
/// cref="RomanceCatalog.ChildbirthBaseMaternalRiskPercent"/> et al.).
///
/// <b>A mother who died before term of an unrelated cause</b> (violence, disease, old age — anything
/// this system does not itself cause) simply resolves the pregnancy with no child and no further roll:
/// there is no live mother left to deliver, so both risk rolls are moot, not merely skipped.
///
/// <b>Order of operations matters</b>: the infant-survival roll and the (possible) <see
/// cref="Characters.BirthCharacterCommand"/> call always happen <i>before</i> the maternal-risk roll is
/// applied to the mother's own record — <see cref="Characters.BirthCharacterCommands"/>' own validation
/// requires the mother to still be alive at the moment of the call, and historically a mother dying in
/// childbirth while her child survives her is exactly the outcome this ordering has to support, not an
/// edge case to avoid.
///
/// <see cref="RomanceContentSettings.FertilityRiskAbstracted"/>, when <c>true</c>, skips both rolls
/// entirely and always resolves a live mother's due pregnancy as both mother and infant surviving.
/// </summary>
public sealed class ChildbirthResolutionSystem : IMonthlySystem<WorldState>
{
    /// <summary>The named random stream this system reserves for its maternal-death-risk roll (rule 8)
    /// — <see cref="CampaignBootstrapper.RomanceChildbirthMaternalRiskStreamName"/>.</summary>
    private const string MaternalRiskStreamName = CampaignBootstrapper.RomanceChildbirthMaternalRiskStreamName;

    /// <summary>The named random stream this system reserves for its infant-death-risk roll (rule 8),
    /// independent of <see cref="MaternalRiskStreamName"/> — <see
    /// cref="CampaignBootstrapper.RomanceChildbirthInfantRiskStreamName"/>.</summary>
    private const string InfantRiskStreamName = CampaignBootstrapper.RomanceChildbirthInfantRiskStreamName;

    public string Id => "romance.childbirthResolution";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "pregnancyRecords", "characters" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "pregnancyRecords", "characters", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // Materialize first, matching ConceptionSystem/RomanticBondDecaySystem's identical "snapshot
        // before mutating" guard — this loop mutates state.PregnancyRecords (and, for a surviving
        // child, state.Characters) while iterating a snapshot of the former, in fixed ascending
        // RuntimeId<PregnancyRecord> order so both RNG streams below draw in a stable order every tick
        // (ADR 0004).
        var duePregnancies = state.PregnancyRecords.InAscendingOrder()
            .Where(entry => !entry.Value.Resolved && entry.Value.DueDate.TotalMonths <= context.Date.TotalMonths)
            .ToArray();

        foreach (var (pregnancyId, pregnancy) in duePregnancies)
        {
            state.Characters.TryGet(pregnancy.MotherId, out var mother);

            // The mother died before term of some unrelated cause (violence, disease, old age, ...):
            // both risk rolls are moot, not merely skipped — there is no live mother left to deliver.
            if (mother is null || !mother.IsAlive)
            {
                ResolvePregnancy(state, pregnancyId, pregnancy, bornChildId: null);
                events.Add(new PregnancyResolvedEvent(
                    state.EventIds.Issue(), context.Date, pregnancyId, pregnancy.MotherId, pregnancy.FatherId,
                    MotherSurvived: false, BornChildId: null));
                continue;
            }

            bool infantSurvives;
            bool maternalRiskRolled;

            if (state.RomanceContentSettings.FertilityRiskAbstracted)
            {
                infantSurvives = true;
                maternalRiskRolled = false;
            }
            else
            {
                var infantLost = context.RandomStreams.NextUInt(InfantRiskStreamName, 100)
                    < (uint)RomanceCatalog.ChildbirthInfantRiskPercent;
                infantSurvives = !infantLost;

                maternalRiskRolled = context.RandomStreams.NextUInt(MaternalRiskStreamName, 100)
                    < (uint)ComputeMaternalRiskPercent(state, mother);
            }

            // The infant-survival roll (and the resulting birth, if any) is applied before the
            // maternal-risk roll is applied to the mother — see this type's own doc comment for why:
            // BirthCharacterCommands' own validation requires the mother to still be alive at the
            // moment of the call, and a mother dying in childbirth while her child survives her must
            // remain a reachable outcome.
            RuntimeId<Character>? bornChildId = null;
            if (infantSurvives)
            {
                var birthResult = BirthCharacterCommands.CreatePipeline(context.RandomStreams).Execute(
                    state,
                    new BirthCharacterCommand(
                        state.CommandIds.Issue(), "system", context.Date, null,
                        pregnancy.MotherId, pregnancy.FatherId, Sex: null,
                        mother.LegalStatus, mother.SocialClass, RomanceCatalog.PlaceholderNewbornNamePool,
                        CampaignBootstrapper.CharacterGenerationStreamName, mother.Household));

                if (birthResult.Accepted)
                {
                    events.AddRange(birthResult.Events);
                    var bornEvent = birthResult.Events.OfType<CharacterBornEvent>().FirstOrDefault();
                    bornChildId = bornEvent?.CharacterId;
                }
            }

            var motherSurvives = !maternalRiskRolled;
            if (!motherSurvives)
            {
                // Re-fetch: BirthCharacterCommands may have just added the newborn to state.Characters,
                // but the mother's own record is unaffected by that call — still safe to read fresh
                // here rather than reuse the pre-birth snapshot.
                state.Characters.TryGet(pregnancy.MotherId, out var motherBeforeDeath);
                if (motherBeforeDeath is { } livingMother)
                    ApplyMaternalDeath(state, livingMother, context.Date);
            }

            ResolvePregnancy(state, pregnancyId, pregnancy, bornChildId);

            events.Add(new PregnancyResolvedEvent(
                state.EventIds.Issue(), context.Date, pregnancyId, pregnancy.MotherId, pregnancy.FatherId,
                MotherSurvived: motherSurvives, BornChildId: bornChildId));
        }

        return events;
    }

    /// <summary>§9's risk formula: a base percent, reduced by the mother's own Health and (when
    /// present) a filled Court Physician, never dropping below a floor — see <see
    /// cref="RomanceCatalog"/>'s own doc comments on each constant for the reasoning behind each
    /// term.</summary>
    private static int ComputeMaternalRiskPercent(WorldState state, Character mother)
    {
        var physicianFilled = mother.Household is { } householdId
            && SeniorPositionResolver.IsCurrentlyFilled(state, householdId, SeniorPositionTitle.CourtPhysician);

        var risk = RomanceCatalog.ChildbirthBaseMaternalRiskPercent
            - (mother.Condition.Health * RomanceCatalog.ChildbirthMaternalRiskHealthWeightPercent / 100)
            - (physicianFilled ? RomanceCatalog.ChildbirthPhysicianRiskReductionPercent : 0);

        return Math.Max(RomanceCatalog.ChildbirthMaternalRiskFloorPercent, risk);
    }

    /// <summary>Records the mother's death by childbirth and closes any open marriage — mirrors <see
    /// cref="Characters.CharacterLifecycleSystem"/>'s own idiom for a death mid-tick exactly (small
    /// per-system idioms are restated rather than shared in this codebase; see e.g. how
    /// RomanticBondDecaySystem/SchemeProgressSystem/AutonomousRomanceSystem each separately restate
    /// "materialize before mutate").</summary>
    private static void ApplyMaternalDeath(WorldState state, Character mother, GameDate date)
    {
        var deathRecord = new DeathRecord(date, DeathCause.Childbirth, mother.AgeInYears(date));
        var updatedMother = mother with { DeathRecord = deathRecord };

        var spouseId = updatedMother.CurrentSpouseId;
        if (spouseId is { } spouse)
        {
            updatedMother = CloseOpenMarriage(updatedMother, spouse, date);
            if (state.Characters.TryGet(spouse, out var spouseCharacter) && spouseCharacter.IsAlive)
            {
                var updatedSpouse = CloseOpenMarriage(spouseCharacter, mother.Id, date);
                state.Characters.Remove(spouse);
                state.Characters.Add(spouse, updatedSpouse);
            }
        }

        state.Characters.Remove(mother.Id);
        state.Characters.Add(mother.Id, updatedMother);
    }

    private static Character CloseOpenMarriage(Character character, RuntimeId<Character> spouseId, GameDate endDate)
    {
        var history = character.MaritalHistory
            .Select(record => record.SpouseId == spouseId && record.EndDate is null
                ? new MarriageRecord(record.SpouseId, record.StartDate, endDate, MarriageEndReason.Death)
                : record)
            .ToArray();
        return character with { MaritalHistory = history };
    }

    private static void ResolvePregnancy(
        WorldState state, RuntimeId<PregnancyRecord> pregnancyId, PregnancyRecord pregnancy, RuntimeId<Character>? bornChildId)
    {
        state.PregnancyRecords.Remove(pregnancyId);
        state.PregnancyRecords.Add(pregnancyId, pregnancy with
        {
            Resolved = true,
            MaternalRiskResolved = true,
            InfantRiskResolved = true,
            BornChildId = bornChildId,
        });
    }
}
