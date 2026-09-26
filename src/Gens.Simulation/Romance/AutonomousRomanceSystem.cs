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

/// <summary>Emitted whenever <see cref="AutonomousRomanceSystem"/> rolls a successful spontaneous
/// advance for an eligible pair (<c>gens-romance-sexuality-lineage-design.md</c> §8, §8.1). Private to
/// both parties, matching every other Romance event's "interior state, not broadcast" reasoning — the
/// Monthly Report/Chronicle consuming this to surface it to the player is a later phase's job, per this
/// module's own established precedent of leaving that consumption to whichever system owns it.</summary>
public sealed record AutonomousRomanceTriggeredEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterAId,
    RuntimeId<Character> CharacterBId,
    RomanticBondType ResultingBondType) : IDomainEvent
{
    public string Type => "romance.autonomousRomanceTriggered";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterAId.ToTaggedString(), CharacterBId.ToTaggedString() };
    public string? CausationId => null;
    public Visibility Visibility => Visibility.Private(CharacterAId.ToTaggedString(), CharacterBId.ToTaggedString());
}

/// <summary>
/// The monthly "spontaneous romance" tick (<c>gens-romance-sexuality-lineage-design.md</c> §8, §8.1):
/// household life self-simulating rather than every <see cref="RomanticBond"/> requiring a player- or
/// AI-issued Courtship interaction to move at all.
///
/// <b>Scope cut, disclosed rather than an oversight:</b> §8 names four "opportunity" sources a pair can
/// meet through — sharing a household, a shared travel party, a hosted gathering, and (implicitly) any
/// other proximity this document leaves unspecified. This system implements only the first, cheapest,
/// most common case: two eligible Characters who already share a non-null <see
/// cref="Character.Household"/>. Travel-party and hosted-gathering opportunity sources are explicitly
/// NOT implemented this pass — the household case alone proves the mechanism end-to-end and already
/// covers §8.1's worked example's first two tiers (an unprompted spark between household members,
/// then that spark maturing into something the household itself has to reckon with). A later pass can
/// add the other two opportunity sources as their own additional pair-enumeration strategies feeding
/// the same roll-and-advance logic below, without disturbing this one. (The hosted-gathering source
/// now exists in a different shape: Phase 17 item 4's <see cref="Activities.ActivityPhaseRunner"/>
/// rolls a Flirtation incident between two attendees of an Activity's Phase, routed through the same
/// <see cref="RecordRomanticInteractionCommand"/> and gated by the same <see cref="RomanceEligibility.CheckPair"/>.)
///
/// Each tick, for every unordered pair of Characters sharing a household: <see
/// cref="RomanceEligibility.CheckPair"/> gates the pair first (Adult floor, power-imbalance exclusion),
/// then an eligible pair rolls <see cref="RomanceCatalog.AutonomousRomanceMonthlyChancePercent"/> on its
/// own named stream, in a fully deterministic per-tick pair order (households ordered by <see
/// cref="RuntimeId{T}"/>, members within a household ordered by <see cref="RuntimeId{T}"/>) so no
/// dictionary/hash-set iteration order ever reaches an RNG draw (ADR 0004). CRITICAL for determinism:
/// every eligible pair rolls independently, every tick it stays eligible — an earlier pair's success or
/// failure this same tick never causes a later pair's roll to be skipped.
///
/// On a successful roll: a pair with no existing <see cref="RomanticBond"/>, or one still short of <see
/// cref="RomanceCatalog.AutonomousRomanceMinimumScore"/> on either score, gets a small positive nudge to
/// both Affection and Attraction via <see cref="RecordRomanticInteractionCommand"/> — the "unprompted
/// spark" §8.1 describes. A pair whose EXISTING bond already clears that minimum on BOTH scores — a
/// repeat trigger on real, already-built mutual interest — instead advances the bond's <see
/// cref="RomanticBondType"/>: to <see cref="RomanticBondType.Affair"/> if either party is currently
/// married to someone other than their partner in this pair, or (re-)confirmed as <see
/// cref="RomanticBondType.Courtship"/> otherwise (a no-op for a bond already <see
/// cref="RomanticBondType.Courtship"/>, but a real correction for one that had cooled to <see
/// cref="RomanticBondType.PastRelationship"/>).
/// </summary>
public sealed class AutonomousRomanceSystem : IMonthlySystem<WorldState>
{
    /// <summary>The named random stream this system reserves for its monthly spontaneous-advance roll
    /// (rule 8), kept distinct from every other stream so a change here never perturbs another
    /// system's draws — <see cref="CampaignBootstrapper.RomanceAutonomousInitiationStreamName"/>.</summary>
    private const string StreamName = CampaignBootstrapper.RomanceAutonomousInitiationStreamName;

    public string Id => "romance.autonomousRomance";
    public TickPhase Phase => TickPhase.RelationshipsActors;

    // No separate "households" partition exists in this codebase to declare here: Character.Household
    // is a plain field on the Character record itself (State/WorldState.cs has no independent
    // households registry), so reading "characters" already covers everything this system needs to
    // know about household membership.
    public IReadOnlyCollection<string> Reads { get; } = new[] { "characters", "romanticBonds" };

    /// <summary>Includes <c>"commandIds"</c>/<c>"commandSequence"</c> alongside this system's own
    /// headline <see cref="State.WorldState.RomanticBonds"/> write: every successful roll submits <see
    /// cref="RecordRomanticInteractionCommand"/> through its own <see cref="Commands.CommandPipeline{TState,TCommand}"/>,
    /// which mints a fresh command id and sequence number each time, mirroring <see
    /// cref="Legal.LegalCaseAdvancementSystem"/>'s own identical "the write-set declared here must cover
    /// the counters those pipelines touch too" reasoning.</summary>
    public IReadOnlyCollection<string> Writes { get; } = new[] { "romanticBonds", "eventIds", "commandIds", "commandSequence" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // Materialize first, then group by household, matching RelationshipDecaySystem/
        // SchemeProgressSystem's identical "snapshot before mutating" guard — this system's own
        // mutation (RecordRomanticInteractionCommand) touches RomanticBonds, not Characters, but the
        // same discipline applies: never iterate a live registry while submitting commands against
        // the state it lives in. Iterating state.Characters.InAscendingOrder() and inserting into a
        // SortedDictionary keyed by RuntimeId<Household> (rather than a plain Dictionary) keeps every
        // later iteration fully deterministic — ascending household ID, then ascending member ID within
        // each household, with no reliance on unordered hash-based iteration anywhere (ADR 0004).
        var byHousehold = new SortedDictionary<RuntimeId<Household>, List<Character>>();
        foreach (var entry in state.Characters.InAscendingOrder())
        {
            var character = entry.Value;
            if (!character.IsAlive || character.Household is not { } householdId)
                continue;

            if (!byHousehold.TryGetValue(householdId, out var members))
            {
                members = new List<Character>();
                byHousehold[householdId] = members;
            }

            members.Add(character);
        }

        foreach (var (_, members) in byHousehold)
        {
            for (var i = 0; i < members.Count; i++)
            {
                for (var j = i + 1; j < members.Count; j++)
                {
                    var aId = members[i].Id;
                    var bId = members[j].Id;

                    if (RomanceEligibility.CheckPair(state, aId, bId, context.Date) is not null)
                        continue;

                    var rolled = context.RandomStreams.NextUInt(StreamName, 100)
                        < (uint)RomanceCatalog.AutonomousRomanceMonthlyChancePercent;
                    if (!rolled)
                        continue;

                    var key = RomanticBondKey.Create(aId, bId);
                    var hasBond = state.RomanticBonds.TryGet(key, out var existingBond);
                    var alreadyMature = hasBond
                        && existingBond.Affection >= RomanceCatalog.AutonomousRomanceMinimumScore
                        && existingBond.Attraction >= RomanceCatalog.AutonomousRomanceMinimumScore;

                    int affectionDelta;
                    int attractionDelta;
                    RomanticBondType? bondTypeOverride;

                    if (!alreadyMature)
                    {
                        affectionDelta = RomanceCatalog.AutonomousRomanceNudgeAffectionDelta;
                        attractionDelta = RomanceCatalog.AutonomousRomanceNudgeAttractionDelta;
                        bondTypeOverride = null;
                    }
                    else
                    {
                        // Repeat trigger on a pair with genuine, already-built mutual interest (§8.1):
                        // no further score nudge, just a bond-type correction reflecting the real
                        // marital situation.
                        affectionDelta = 0;
                        attractionDelta = 0;

                        state.Characters.TryGet(aId, out var aChar);
                        state.Characters.TryGet(bId, out var bChar);
                        var eitherMarriedElsewhere =
                            (aChar.CurrentSpouseId is { } aSpouse && aSpouse != bId) ||
                            (bChar.CurrentSpouseId is { } bSpouse && bSpouse != aId);
                        bondTypeOverride = eitherMarriedElsewhere ? RomanticBondType.Affair : RomanticBondType.Courtship;
                    }

                    var interactionResult = RecordRomanticInteractionCommands.Pipeline.Execute(
                        state,
                        new RecordRomanticInteractionCommand(
                            state.CommandIds.Issue(), "system", context.Date, null,
                            aId, bId, affectionDelta, attractionDelta, bondTypeOverride));
                    events.AddRange(interactionResult.Events);

                    var resultingBondType = state.RomanticBonds.TryGet(key, out var resultingBond)
                        ? resultingBond.BondType
                        : RomanticBondType.Courtship;

                    events.Add(new AutonomousRomanceTriggeredEvent(
                        state.EventIds.Issue(), context.Date, aId, bId, resultingBondType));
                }
            }
        }

        return events;
    }
}
