using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Romance;

/// <summary>Emitted whenever <see cref="ConceptionSystem"/> rolls a successful conception. Private to
/// both parents — this is intimate biological information, matching <see
/// cref="RecordRomanticInteractionCommand"/>'s own "interior state, not broadcast" visibility
/// reasoning for this module.</summary>
public sealed record ConceptionRecordedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<PregnancyRecord> PregnancyId,
    RuntimeId<Character> MotherId,
    RuntimeId<Character> FatherId,
    GameDate DueDate) : IDomainEvent
{
    public string Type => "romance.conceptionRecorded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { MotherId.ToTaggedString(), FatherId.ToTaggedString() };
    public string? CausationId => null;
    public Visibility Visibility => Visibility.Private(MotherId.ToTaggedString(), FatherId.ToTaggedString());
}

/// <summary>
/// The monthly conception roll (<c>gens-romance-sexuality-lineage-design.md</c> §9): every active <see
/// cref="RomanticBondType.Marriage"/>, <see cref="RomanticBondType.Concubinage"/>, or <see
/// cref="RomanticBondType.Affair"/> <see cref="RomanticBond"/> between two alive, Adult-or-above,
/// opposite-sex Characters with no pregnancy already in progress rolls the mother's own existing <see
/// cref="Character.GetEffectiveFertility"/> as a flat percent chance.
///
/// <b>Same-sex bonds never conceive</b> — a deliberate, explicit corollary of §5 rather than something
/// left implicit: pregnancy is strictly biological in this document's model, so a bond between two
/// Characters of the same <see cref="Sex"/> is skipped outright, every tick, regardless of <see
/// cref="RomanticBondType"/>.
///
/// This system deliberately does <b>not</b> re-call <see cref="RomanceEligibility.CheckPair"/> — that
/// gate is about eligibility to FORM a bond, not about whether an already-existing bond keeps
/// conceiving month to month. If an Enslaved party's bond somehow still exists (it never should, since
/// every mutation point upstream of this system already gates on that), this system's own Sex/lifecycle
/// checks below are an acceptable fail-safe, not a re-implementation of that gate — a defensive net,
/// not a second copy of the rule.
/// </summary>
public sealed class ConceptionSystem : IMonthlySystem<WorldState>
{
    /// <summary>The named random stream this system reserves for its monthly conception roll (rule 8)
    /// — <see cref="CampaignBootstrapper.RomanceConceptionChanceStreamName"/>.</summary>
    private const string StreamName = CampaignBootstrapper.RomanceConceptionChanceStreamName;

    public string Id => "romance.conception";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "romanticBonds", "characters", "pregnancyRecords" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "pregnancyRecords", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // Materialize first, matching RomanticBondDecaySystem/SchemeProgressSystem's identical
        // "snapshot before mutating" guard — this loop adds to state.PregnancyRecords mid-iteration
        // over state.RomanticBonds, a different partition, but the discipline is the same.
        var bonds = state.RomanticBonds.InAscendingOrder().ToArray();

        foreach (var (key, bond) in bonds)
        {
            if (bond.BondType is not (RomanticBondType.Marriage or RomanticBondType.Concubinage or RomanticBondType.Affair))
                continue;

            if (!state.Characters.TryGet(key.CharacterAId, out var characterA) ||
                !state.Characters.TryGet(key.CharacterBId, out var characterB))
                continue;

            if (!characterA.IsAlive || !characterB.IsAlive)
                continue;

            // Cheap safety net, not a re-implementation of RomanceEligibility — see this type's own
            // doc comment.
            if (characterA.GetLifecycleStage(context.Date) is not (LifecycleStage.Adult or LifecycleStage.Elderly) ||
                characterB.GetLifecycleStage(context.Date) is not (LifecycleStage.Adult or LifecycleStage.Elderly))
                continue;

            // Same-sex bonds never conceive — a corollary of §5, stated explicitly (see this type's
            // own doc comment).
            if (characterA.Sex == characterB.Sex)
                continue;

            var mother = characterA.Sex == Sex.Female ? characterA : characterB;
            var father = characterA.Sex == Sex.Female ? characterB : characterA;

            if (HasUnresolvedPregnancy(state, mother.Id))
                continue;

            // This implementation's own invented mapping, unsized per §17: the mother's fertility
            // score IS the flat percent conception chance, the simplest defensible untuned baseline.
            var conceived = context.RandomStreams.NextUInt(StreamName, 100) < (uint)mother.GetEffectiveFertility();
            if (!conceived)
                continue;

            var pregnancyId = state.PregnancyRecordIds.Issue();
            var record = PregnancyRecord.Create(pregnancyId, mother.Id, father.Id, bond.BondType, context.Date);
            state.PregnancyRecords.Add(pregnancyId, record);

            events.Add(new ConceptionRecordedEvent(
                state.EventIds.Issue(), context.Date, pregnancyId, mother.Id, father.Id, record.DueDate));
        }

        return events;
    }

    private static bool HasUnresolvedPregnancy(WorldState state, RuntimeId<Character> motherId)
    {
        foreach (var entry in state.PregnancyRecords.InAscendingOrder())
            if (!entry.Value.Resolved && entry.Value.MotherId == motherId)
                return true;

        return false;
    }
}
