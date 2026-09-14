using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Actors;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Magistracies;
using Gens.Simulation.Reputation;
using Gens.Simulation.Scandal;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Romance;

/// <summary>Emitted whenever <see cref="AffairDiscoverySystem"/> rolls a high-stakes escalation
/// (<c>gens-romance-sexuality-lineage-design.md</c> §11). Public — unlike every other Romance event in
/// this module (private by design, "interior state, not broadcast"), a high-stakes discovered affair is
/// specifically no longer private the instant it escalates: §11 itself frames escalation as the affair
/// becoming a real, talked-about fact, matching <see
/// cref="Scandal.ScandalRecordedEvent"/>'s identical public reasoning for the comparable "stops being a
/// private matter" moment. A minor-stakes escalation gets no event of its own here — <see
/// cref="Scandal.RecordScandalCommand"/>'s own <see cref="Scandal.ScandalRecordedEvent"/> (already
/// public) is the real, sufficient public signal for that quieter case.</summary>
public sealed record AffairEscalatedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<AffairRecord> AffairId,
    RuntimeId<Character> OffenderCharacterId,
    RuntimeId<Character> ThirdPartyCharacterId,
    RuntimeId<Character> WrongedSpouseId,
    bool InvolvesRivalHouse) : IDomainEvent
{
    public string Type => "romance.affairEscalated";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[]
    {
        OffenderCharacterId.ToTaggedString(), ThirdPartyCharacterId.ToTaggedString(), WrongedSpouseId.ToTaggedString(),
    };
    public string? CausationId => null;
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// The monthly Discovery/Counter-play/Resolution tick for every undiscovered <see
/// cref="RomanticBondType.Affair"/> <see cref="RomanticBond"/> (<c>gens-romance-sexuality-lineage-design.md</c>
/// §11) — the wiring point for <see cref="Scandal.ScandalSourceType.AffairDiscovery"/>, reserved and
/// unused in <c>Scandal/ScandalRecord.cs</c> since the day it was added.
///
/// Reuses <see cref="Interactions.SchemeProgressSystem"/>'s own progress/risk/threshold/roll <i>shape</i>
/// — a flat monthly gain plus an Intrigue-scaled term, a threshold that triggers an immediate resolution
/// roll, an Intrigue-difference-weighted chance — not that system's own <see cref="Interactions.Scheme"/>
/// partition itself: a persistent <see cref="RomanticBond"/> has no single "completion" a Scheme has, so
/// there is nothing here to literally reuse beyond the formula pattern.
///
/// <b>Each undiscovered <see cref="RomanticBondType.Affair"/> bond, every month:</b>
/// <list type="number">
/// <item><see cref="RomanticBond.DiscoveryRisk"/> rises by <see
/// cref="RomanceCatalog.AffairMonthlyDiscoveryRiskGain"/> plus a term scaled by the most vigilant real
/// wronged spouse's own <see cref="CoreAttributes.Intrigue"/>, clamped to 100 (see <see
/// cref="RomanceCatalog.AffairMonthlyDiscoveryRiskWrongedSpouseIntrigueWeightPercent"/>'s own doc
/// comment for the zero-wronged-spouse case).</item>
/// <item>Once risk clears <see cref="RomanceCatalog.AffairDiscoveryThresholdPercent"/>, an
/// Intrigue-weighted roll on this system's own named stream decides Foiled versus Escalated — see <see
/// cref="RomanceCatalog.AffairDiscoveryIntrigueDifferenceWeightPercent"/>'s own doc comment for exactly
/// how this formula's sign deliberately mirrors, rather than copies, <see
/// cref="Interactions.SchemeProgressSystem"/>'s own counter-play term.</item>
/// <item><b>Foiled:</b> risk falls back to <see
/// cref="RomanceCatalog.AffairDiscoveryFoiledRiskRetentionPercent"/> of its post-gain value, the bond
/// stays private, nothing else happens.</item>
/// <item><b>Escalated:</b> <see cref="RomanticBond.IsKnownPublicly"/> becomes <c>true</c> (DiscoveryRisk
/// resets to 0 — meaningless once known, per that field's own doc comment) and this system resolves
/// stakes (see below). <b>Minor stakes</b> auto-resolve right here: a <see cref="AffairRecord"/> is
/// created with <see cref="AffairResolution.QuietlyResolved"/>, and <see
/// cref="Scandal.RecordScandalCommand"/> is called with <see
/// cref="Scandal.ScandalSourceType.AffairDiscovery"/> — the long-reserved wiring point. <b>High
/// stakes</b> creates an unresolved <see cref="AffairRecord"/> (<c>Resolution: null</c>, awaiting a
/// later slice's own <c>ResolveAffairCommand</c>) and emits a public <see
/// cref="AffairEscalatedEvent"/>. Both cases grant this item's own new reactive traits directly:
/// <see cref="RomanceCatalog.AdulterousTraitId"/> to the offending bond participant, and <see
/// cref="RomanceCatalog.HeartbrokenTraitId"/>/<see cref="RomanceCatalog.GuardedTraitId"/> to every real
/// wronged spouse (there can be 0, 1, or 2 — see <see cref="AffairRecord"/>'s own doc comment for why
/// the record itself only ever names one).</item>
/// </list>
///
/// <b>"Tracked Rival House" (High Stakes trigger (a)):</b> resolved exactly the way <see
/// cref="Actors.RivalAmbitionSystem"/> already resolves "is this Character a real, promoted house
/// head" — a <see cref="LivingWorldActor"/> of <see cref="LivingWorldActorType.Gens"/> whose own <see
/// cref="LivingWorldActor.HeadCharacterId"/> matches. This only ever catches a tracked house's own
/// head, never a "notable member" short of that — this codebase has no membership roster for a
/// <see cref="LivingWorldActorTier.Background"/> house's non-head relatives to check against, so this
/// item's own stated, decisive reading of "head or notable member" (§17's own guidance) is: head only.
///
/// <b>"Politically important marriage" (High Stakes trigger (c)):</b> this item's own stated, decisive
/// reading (§17's own guidance to make a call rather than leave this open) is: a real wronged spouse's
/// own household currently holds at least one active Magistracy (<see
/// cref="MagistracyResolver.ActiveOfficeCountForHousehold"/>), OR that wronged spouse personally carries
/// a <see cref="BondTag.Patron"/> or <see cref="BondTag.Client"/> relationship-web tie to anyone at all
/// — both real, already-modeled facts this codebase already treats as "politically meaningful"
/// elsewhere, rather than a new stored field bolted onto <see cref="MarriageRecord"/> the design doc
/// never asks for.
///
/// <b>House Standing on Rival House involvement — deliberately skipped, not deferred to a forward
/// hook:</b> <see cref="Actors.AdjustHouseStandingCommand"/> moves standing between two
/// <see cref="RuntimeId{T}"/>-of-<see cref="Identity.Actor"/> pairs, and this codebase has no established
/// mapping from an ordinary wronged spouse's own household to a <see cref="LivingWorldActor"/> — only a
/// tracked house (Background or Noteworthy) has one at all. A player-household victim, the overwhelmingly
/// common case, simply has no Actor id on that side to adjust standing FROM, and inventing one here would
/// be exactly the kind of new mechanic this item is not scoped to build. This matches <see
/// cref="SeduceSchemeResolutionHook"/>'s own identical, already-precedented "stops short of an
/// AdjustHouseStandingCommand call" restraint.
///
/// <b>Zero-wronged-spouse defensive case:</b> per <see cref="AutonomousRomanceSystem"/>'s own promotion
/// logic, a bond only ever becomes <see cref="RomanticBondType.Affair"/> when at least one participant
/// already holds an open marriage to someone else — so a real wronged spouse should always be
/// resolvable once a bond reaches this system at all. If a malformed or directly-seeded bond somehow
/// violates that invariant, this system does not fabricate a wronged spouse for <see
/// cref="AffairRecord.WrongedSpouseId"/>'s non-nullable field: an Escalated roll with no resolvable
/// wronged spouse is treated exactly like a Foiled one (risk reduction, stays private, nothing recorded)
/// rather than ever writing bogus data.
/// </summary>
public sealed class AffairDiscoverySystem : IMonthlySystem<WorldState>
{
    /// <summary>The named random stream this system reserves for its monthly Foiled-vs-Escalated
    /// resolution roll (rule 8) — <see cref="CampaignBootstrapper.RomanceAffairDiscoveryStreamName"/>.
    /// DiscoveryRisk's own monthly advancement is a deterministic formula and draws no random
    /// numbers.</summary>
    private const string StreamName = CampaignBootstrapper.RomanceAffairDiscoveryStreamName;

    public string Id => "romance.affairDiscovery";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "romanticBonds", "characters", "affairRecords", "householdReputations" };

    /// <summary>Broader than this system's own <see cref="State.WorldState.RomanticBonds"/>/<see
    /// cref="State.WorldState.AffairRecords"/> writes because an Escalated roll's own further
    /// consequences — the reactive-trait grants this method applies directly to <see
    /// cref="State.WorldState.Characters"/>, and (for the Status/Role Dignitas modifier and, for a
    /// minor-stakes case, <see cref="Scandal.RecordScandalCommand"/>'s own composed pipeline, which can
    /// additionally touch <see cref="State.WorldState.Relationships"/> when a scar target is supplied)
    /// each mint their own command id and sequence number — all reach further than this system's two
    /// headline partitions, mirroring <see cref="Legal.LegalCaseAdvancementSystem"/>'s own identical
    /// "the write-set declared here must cover the counters those pipelines touch too" reasoning.</summary>
    public IReadOnlyCollection<string> Writes { get; } = new[]
    {
        "romanticBonds", "affairRecords", "householdReputations", "eventIds",
        "characters", "commandIds", "commandSequence", "scandalRecords", "scandalRecordIds", "relationships",
    };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // Materialize first: this loop replaces entries in state.RomanticBonds and adds to
        // state.AffairRecords mid-iteration, matching RomanticBondDecaySystem/ConceptionSystem's
        // identical "snapshot before mutating" guard.
        var affairBonds = state.RomanticBonds.InAscendingOrder()
            .Where(entry => entry.Value.BondType == RomanticBondType.Affair && !entry.Value.IsKnownPublicly)
            .ToArray();

        foreach (var (key, bond) in affairBonds)
        {
            var wrongedSpouseIds = ResolveWrongedSpouseIds(state, key.CharacterAId, key.CharacterBId);
            var wrongedSpouseIntrigue = wrongedSpouseIds.Count == 0
                ? 0
                : wrongedSpouseIds.Max(id => GetIntrigue(state, id));

            var riskGain = RomanceCatalog.AffairMonthlyDiscoveryRiskGain
                + wrongedSpouseIntrigue * RomanceCatalog.AffairMonthlyDiscoveryRiskWrongedSpouseIntrigueWeightPercent / 100;
            var newRisk = Math.Min(RomanticBond.MaxScore, bond.DiscoveryRisk + riskGain);

            if (newRisk < RomanceCatalog.AffairDiscoveryThresholdPercent)
            {
                ReplaceBond(state, key, WithDiscoveryState(bond, newRisk, bond.IsKnownPublicly));
                continue;
            }

            var pairIntrigueAverage = (GetIntrigue(state, key.CharacterAId) + GetIntrigue(state, key.CharacterBId)) / 2;
            var escalateChance = Math.Clamp(
                RomanceCatalog.AffairDiscoveryBaseEscalateChancePercent
                    + (wrongedSpouseIntrigue - pairIntrigueAverage) * RomanceCatalog.AffairDiscoveryIntrigueDifferenceWeightPercent / 100,
                0, 100);
            var escalated = context.RandomStreams.NextUInt(StreamName, 100) < (uint)escalateChance;

            var (offenderId, thirdPartyId, primaryWrongedSpouseId) = ResolveOffenderAndThirdParty(state, key);

            if (!escalated || primaryWrongedSpouseId is null)
            {
                // Foiled (or an Escalated roll this system refuses to act on — see this type's own
                // "zero-wronged-spouse defensive case" doc comment): the suspicion doesn't pan out,
                // risk falls back partway, the bond stays exactly as private as before.
                var reducedRisk = newRisk * RomanceCatalog.AffairDiscoveryFoiledRiskRetentionPercent / 100;
                ReplaceBond(state, key, WithDiscoveryState(bond, reducedRisk, isKnownPublicly: false));
                continue;
            }

            // Escalated: the affair is genuinely found out. DiscoveryRisk becomes meaningless the
            // moment IsKnownPublicly flips true (see RomanticBond.DiscoveryRisk's own doc comment).
            ReplaceBond(state, key, WithDiscoveryState(bond, discoveryRisk: 0, isKnownPublicly: true));

            var involvesRivalHouse = IsRivalHouseHead(state, key.CharacterAId) || IsRivalHouseHead(state, key.CharacterBId);
            var legitimacyContested = HasUnresolvedAffairConceivedPregnancy(state, key.CharacterAId, key.CharacterBId);
            var threatensPoliticalMarriage = wrongedSpouseIds.Any(id => IsPoliticallySignificant(state, id));
            var stakesLevel = involvesRivalHouse || legitimacyContested || threatensPoliticalMarriage
                ? AffairStakesLevel.HighStakes
                : AffairStakesLevel.Minor;

            // §13: the Status/Role Dignitas modifier is populated (and actually applied as a Dignitas
            // delta) at discovery time too, not only at conviction — see StatusRoleDignitasModifier's
            // own doc comment for why the penalty always lands on the higher-LegalStatus-ranked party's
            // own household.
            var statusRoleModifier = StatusRoleDignitasModifier.Calculate(state, offenderId, thirdPartyId);
            var statusRoleHigherRankedPartyId = StatusRoleDignitasModifier.DetermineHigherRankedParty(state, offenderId, thirdPartyId);

            var affairId = state.AffairRecordIds.Issue();
            var record = new AffairRecord(
                affairId, offenderId, thirdPartyId, primaryWrongedSpouseId.Value, stakesLevel,
                involvesRivalHouse, legitimacyContested, threatensPoliticalMarriage,
                Resolution: stakesLevel == AffairStakesLevel.Minor ? AffairResolution.QuietlyResolved : null,
                StatusRoleDignitasModifier: statusRoleModifier, DiscoveredDate: context.Date);
            state.AffairRecords.Add(affairId, record);

            if (statusRoleModifier != 0 && statusRoleHigherRankedPartyId is { } statusRoleHigherRankedId
                && state.Characters.TryGet(statusRoleHigherRankedId, out var statusRoleHigherRankedCharacter)
                && statusRoleHigherRankedCharacter!.Household is { } statusRoleHouseholdId)
            {
                events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
                    state, new AdjustDignitasCommand(
                        state.CommandIds.Issue(), "system", context.Date, null, statusRoleHouseholdId, statusRoleModifier,
                        $"affair {affairId.ToTaggedString()} discovered — status/role modifier")).Events);
            }

            // §11: discovery itself produces the reactive traits regardless of stakes level — only the
            // FORMAL resolution path (a later slice) differs by stakes.
            GrantTrait(state, offenderId, RomanceCatalog.AdulterousTraitId);
            foreach (var wrongedId in wrongedSpouseIds)
            {
                GrantTrait(state, wrongedId, RomanceCatalog.HeartbrokenTraitId);
                GrantTrait(state, wrongedId, RomanceCatalog.GuardedTraitId);
            }

            if (stakesLevel == AffairStakesLevel.Minor)
            {
                // The long-reserved wiring point: ScandalSourceType.AffairDiscovery's first real caller.
                if (state.Characters.TryGet(offenderId, out var offenderCharacter) && offenderCharacter!.Household is { } offenderHouseholdId)
                {
                    events.AddRange(RecordScandalCommands.Pipeline.Execute(
                        state,
                        new RecordScandalCommand(
                            state.CommandIds.Issue(), "system", context.Date, null, offenderHouseholdId,
                            ScandalSourceType.AffairDiscovery, ScandalSeverity.MinorEmbarrassment,
                            ApplyOrdinaryDignitasPenalty: true, ApplyTraitGrant: true,
                            TraitGrantCharacterId: offenderId, ScarredAgainstCharacterId: primaryWrongedSpouseId)).Events);
                }
            }
            else
            {
                // High stakes: no auto-resolution here — a later slice's ResolveAffairCommand consumes
                // this unresolved AffairRecord. Rival House standing is deliberately not adjusted (see
                // this type's own doc comment for why).
                events.Add(new AffairEscalatedEvent(
                    state.EventIds.Issue(), context.Date, affairId, offenderId, thirdPartyId,
                    primaryWrongedSpouseId.Value, involvesRivalHouse));
            }
        }

        return events;
    }

    private static void ReplaceBond(WorldState state, RomanticBondKey key, RomanticBond updated)
    {
        state.RomanticBonds.Remove(key);
        state.RomanticBonds.Add(key, updated);
    }

    /// <summary>A new <see cref="RomanticBond"/> record identical to <paramref name="bond"/> except for
    /// <see cref="RomanticBond.DiscoveryRisk"/>/<see cref="RomanticBond.IsKnownPublicly"/> — this type
    /// has no <c>with</c>-compatible init/set accessors (readonly-getter properties, matching <see
    /// cref="Characters.Relationship"/>'s identical validate-in-constructor shape), so every field must
    /// be threaded back through its own constructor explicitly, mirroring <see
    /// cref="RomanticBondDecaySystem"/>'s own identical reconstruction idiom.</summary>
    private static RomanticBond WithDiscoveryState(RomanticBond bond, int discoveryRisk, bool isKnownPublicly) =>
        new(
            bond.BondType, bond.Affection, bond.Attraction, isKnownPublicly, discoveryRisk,
            bond.FormedDate, bond.LastMeaningfulInteractionDate, bond.ProvenanceEventId);

    private static int GetIntrigue(WorldState state, RuntimeId<Character> characterId) =>
        state.Characters.TryGet(characterId, out var character) ? character!.Attributes.Intrigue : 0;

    /// <summary>Every real wronged spouse for this bond — 0, 1, or 2 entries: a bond participant
    /// contributes their own <see cref="Character.CurrentSpouseId"/> only when it names someone OTHER
    /// than the other bond participant (a participant married to their own bond partner would not be
    /// having an affair with them at all).</summary>
    private static List<RuntimeId<Character>> ResolveWrongedSpouseIds(
        WorldState state, RuntimeId<Character> aId, RuntimeId<Character> bId)
    {
        var result = new List<RuntimeId<Character>>();
        if (state.Characters.TryGet(aId, out var a) && a!.CurrentSpouseId is { } aSpouse && aSpouse != bId)
            result.Add(aSpouse);
        if (state.Characters.TryGet(bId, out var b) && b!.CurrentSpouseId is { } bSpouse && bSpouse != aId)
            result.Add(bSpouse);
        return result;
    }

    /// <summary>Picks the single offender/third-party/wronged-spouse triple <see cref="AffairRecord"/>'s
    /// own fixed shape can name — see that record's own doc comment for the "both participants married
    /// elsewhere" tie-break (canonically prefer <paramref name="key"/>'s own <see
    /// cref="RomanticBondKey.CharacterAId"/> side) and the "neither participant married elsewhere"
    /// defensive case (a <c>null</c> wronged spouse).</summary>
    private static (RuntimeId<Character> OffenderId, RuntimeId<Character> ThirdPartyId, RuntimeId<Character>? WrongedSpouseId)
        ResolveOffenderAndThirdParty(WorldState state, RomanticBondKey key)
    {
        if (state.Characters.TryGet(key.CharacterAId, out var a) && a!.CurrentSpouseId is { } aSpouse && aSpouse != key.CharacterBId)
            return (key.CharacterAId, key.CharacterBId, aSpouse);
        if (state.Characters.TryGet(key.CharacterBId, out var b) && b!.CurrentSpouseId is { } bSpouse && bSpouse != key.CharacterAId)
            return (key.CharacterBId, key.CharacterAId, bSpouse);
        return (key.CharacterAId, key.CharacterBId, null);
    }

    /// <summary>Whether either bond participant is a tracked Rival House's own head — see this type's
    /// own doc comment for why "head" is this item's own stated reading of "head or notable
    /// member."</summary>
    private static bool IsRivalHouseHead(WorldState state, RuntimeId<Character> characterId)
    {
        foreach (var entry in state.Actors.InAscendingOrder())
            if (entry.Value.ActorType == LivingWorldActorType.Gens && entry.Value.HeadCharacterId == characterId)
                return true;
        return false;
    }

    private static bool HasUnresolvedAffairConceivedPregnancy(
        WorldState state, RuntimeId<Character> aId, RuntimeId<Character> bId)
    {
        foreach (var entry in state.PregnancyRecords.InAscendingOrder())
        {
            var pregnancy = entry.Value;
            if (pregnancy.Resolved || pregnancy.ConceivedViaBondType != RomanticBondType.Affair)
                continue;
            if (pregnancy.MotherId == aId || pregnancy.FatherId == aId || pregnancy.MotherId == bId || pregnancy.FatherId == bId)
                return true;
        }

        return false;
    }

    /// <summary>This item's own stated, decisive reading of "a politically important marriage" — see
    /// this type's own doc comment for the two checks this performs.</summary>
    private static bool IsPoliticallySignificant(WorldState state, RuntimeId<Character> characterId)
    {
        if (state.Characters.TryGet(characterId, out var character) && character!.Household is { } householdId
            && MagistracyResolver.ActiveOfficeCountForHousehold(state, householdId) > 0)
            return true;

        foreach (var entry in state.Relationships.InAscendingOrder())
        {
            if (entry.Key.From != characterId && entry.Key.To != characterId)
                continue;
            if ((entry.Value.Bonds & (BondTag.Patron | BondTag.Client)) != BondTag.None)
                return true;
        }

        return false;
    }

    /// <summary>Grants <paramref name="traitId"/> directly on <paramref name="characterId"/>, matching
    /// <see cref="Scandal.RecordScandalCommand.ApplyScandalMarkedTrait"/>'s own remove-then-readd
    /// plumbing exactly — see <see cref="RomanceCatalog.AdulterousTraitId"/>'s own doc comment for why
    /// this direct-append idiom is safe for a trait ID content has not authored yet.</summary>
    private static void GrantTrait(WorldState state, RuntimeId<Character> characterId, DefinitionId<Trait> traitId)
    {
        if (!state.Characters.TryGet(characterId, out var character) || character is null || !character.IsAlive)
            return;
        if (character.Traits.Contains(traitId))
            return;

        var updatedTraits = character.Traits.Append(traitId).ToArray();
        state.Characters.Remove(characterId);
        state.Characters.Add(characterId, character with { Traits = updatedTraits });
    }
}
