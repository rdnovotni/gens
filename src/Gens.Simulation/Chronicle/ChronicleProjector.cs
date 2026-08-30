using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Crime;
using Gens.Simulation.Doctrine;
using Gens.Simulation.Economy;
using Gens.Simulation.Edicts;
using Gens.Simulation.Funerary;
using Gens.Simulation.Identity;
using Gens.Simulation.Interactions;
using Gens.Simulation.Legal;
using Gens.Simulation.Scandal;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Time;

namespace Gens.Simulation.Chronicle;

/// <summary>An unpersisted, ID-less candidate <see cref="ChronicleEntry"/> — <see
/// cref="ChronicleGenerationSystem"/> is the only thing that mints an <see
/// cref="RuntimeId{ChronicleEntry}"/> and actually appends one of these to <see
/// cref="WorldState.ChronicleEntries"/>, matching <see cref="Campaign.MonthlyReportProjector"/>'s own
/// pure-projection/stateful-application split.</summary>
public readonly record struct ChronicleEntryDraft(
    GameDate Month,
    ChronicleCategory Category,
    ChronicleTier Tier,
    string Prose,
    IReadOnlyList<RuntimeId<Character>> LinkedCharacterIds,
    string SourceSystem,
    string SourceEventId,
    RuntimeId<Household>? HouseholdId,
    RuntimeId<Actor>? RivalActorId = null);

/// <summary>
/// Projects a month's <see cref="IDomainEvent"/>s into candidate <see cref="ChronicleEntryDraft"/>s
/// (Phase 11 item 3; <c>gens-dynasty-chronicle-design.md</c> §6): "this document doesn't invent new
/// triggers — every system that already flagged something as Chronicle-worthy... is the actual
/// generation source; this document just defines the shared format and tier-assignment rule." Reads
/// <see cref="WorldState"/> only to resolve display names and a Character's owning household —
/// exactly like <see cref="Campaign.MonthlyReportProjector"/> never invents state of its own, this
/// never mutates it.
///
/// Only the event types below are recognized; every other event type (background population,
/// production, market clearing, ledger postings, needs/contentment, lifecycle-stage bookkeeping, and
/// so on) is deliberately not chronicled — §3's own examples name named-Character and household-level
/// facts as the material, not the aggregate systems underneath them. <see
/// cref="Buildings.ConstructionSystem"/>'s <c>buildings.constructionCompleted</c> (named directly by
/// §10 as "Wealth &amp; Building" material) is the one named exception left out of this pass: nothing
/// in this codebase yet resolves a <c>Holding</c> back to an owning <c>Household</c>, so there is no
/// clean <see cref="ChronicleEntry.HouseholdId"/> to assign it — a follow-on once that lookup exists.
/// </summary>
public static class ChronicleProjector
{
    public static IReadOnlyList<ChronicleEntryDraft> Project(WorldState state, IReadOnlyList<IDomainEvent> events)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (events is null)
            throw new ArgumentNullException(nameof(events));

        var playerControlledCharacterIds = CollectPlayerControlledCharacterIds(state, events);

        var drafts = new List<ChronicleEntryDraft>();
        foreach (var evt in events)
        {
            var draft = ProjectOne(state, evt, playerControlledCharacterIds);
            if (draft is { } value)
                drafts.Add(value);
        }

        return drafts;
    }

    /// <summary>Every Character player-controlled at any point relevant to this batch — both right now
    /// (<see cref="WorldState.PlayerControls"/>) and, per <see cref="PlayerControlChangedEvent.PreviousCharacterId"/>,
    /// right before this same batch's own handoff. Needed because <see cref="ChronicleGenerationSystem"/>
    /// runs strictly after the full monthly tick: by the time it reads <see
    /// cref="WorldState.PlayerControls"/>, <see cref="Succession.PlayerControlHandoffSystem"/> has
    /// already moved control to the successor, so a plain "is currently controlled" check would read a
    /// same-tick death of the player-controlled head as no longer player-controlled at all.</summary>
    private static HashSet<RuntimeId<Character>> CollectPlayerControlledCharacterIds(WorldState state, IReadOnlyList<IDomainEvent> events)
    {
        var ids = new HashSet<RuntimeId<Character>>();
        foreach (var entry in state.PlayerControls.InAscendingOrder())
        {
            if (entry.Value.ControlledCharacterId is { } controlled)
                ids.Add(controlled);
        }

        foreach (var evt in events)
        {
            if (evt is PlayerControlChangedEvent { PreviousCharacterId: { } previous })
                ids.Add(previous);
        }

        return ids;
    }

    private static ChronicleEntryDraft? ProjectOne(
        WorldState state, IDomainEvent evt, HashSet<RuntimeId<Character>> playerControlledCharacterIds) => evt switch
        {
            CharacterBornEvent born => new ChronicleEntryDraft(
                born.OccurredDate,
                ChronicleCategory.BirthsAndDeaths,
                ChronicleTier.Minor,
                born.FatherId is { } father
                    ? $"{Name(state, born.CharacterId)} was born to {Name(state, born.MotherId)} and {Name(state, father)}."
                    : $"{Name(state, born.CharacterId)} was born to {Name(state, born.MotherId)}.",
                born.FatherId is { } fatherId
                    ? new[] { born.CharacterId, born.MotherId, fatherId }
                    : new[] { born.CharacterId, born.MotherId },
                born.Type,
                born.EventId.ToTaggedString(),
                HouseholdOf(state, born.CharacterId) ?? HouseholdOf(state, born.MotherId)),

            CharacterDiedEvent died => new ChronicleEntryDraft(
                died.OccurredDate,
                ChronicleCategory.BirthsAndDeaths,
                playerControlledCharacterIds.Contains(died.CharacterId) ? ChronicleTier.Legendary : ChronicleTier.Major,
                $"{Name(state, died.CharacterId)} died at the age of {died.DeathRecord.AgeAtDeath}.",
                died.SpouseId is { } spouse ? new[] { died.CharacterId, spouse } : new[] { died.CharacterId },
                died.Type,
                died.EventId.ToTaggedString(),
                HouseholdOf(state, died.CharacterId)),

            CharactersMarriedEvent married => new ChronicleEntryDraft(
                married.OccurredDate,
                ChronicleCategory.MarriagesAndFamily,
                ChronicleTier.Notable,
                $"{Name(state, married.CharacterId)} wed {Name(state, married.SpouseId)}.",
                new[] { married.CharacterId, married.SpouseId },
                married.Type,
                married.EventId.ToTaggedString(),
                HouseholdOf(state, married.CharacterId) ?? HouseholdOf(state, married.SpouseId)),

            // A death-triggered marriage closure is already covered by the CharacterDiedEvent entry above.
            MarriageEndedEvent { Reason: MarriageEndReason.Death } => null,

            MarriageEndedEvent ended => new ChronicleEntryDraft(
                ended.OccurredDate,
                ChronicleCategory.MarriagesAndFamily,
                ChronicleTier.Minor,
                $"{Name(state, ended.CharacterId)} and {Name(state, ended.SpouseId)} ended their marriage.",
                new[] { ended.CharacterId, ended.SpouseId },
                ended.Type,
                ended.EventId.ToTaggedString(),
                HouseholdOf(state, ended.CharacterId) ?? HouseholdOf(state, ended.SpouseId)),

            HouseholdHeadEstablishedEvent established => new ChronicleEntryDraft(
                established.OccurredDate,
                ChronicleCategory.PoliticsAndOffice,
                ChronicleTier.Notable,
                $"{Name(state, established.HeadCharacterId)} became head of the household.",
                new[] { established.HeadCharacterId },
                established.Type,
                established.EventId.ToTaggedString(),
                established.HouseholdId),

            HouseholdHeadTransferredEvent transferred => new ChronicleEntryDraft(
                transferred.OccurredDate,
                ChronicleCategory.PoliticsAndOffice,
                ChronicleTier.Major,
                transferred.Trigger == HandoffTrigger.RegencyInTrust
                    ? $"{Name(state, transferred.ToCharacterId)} took up the headship of the household in trust, succeeding {Name(state, transferred.FromCharacterId)}."
                    : $"{Name(state, transferred.ToCharacterId)} inherited the headship of the household from {Name(state, transferred.FromCharacterId)}.",
                new[] { transferred.FromCharacterId, transferred.ToCharacterId },
                transferred.Type,
                transferred.EventId.ToTaggedString(),
                transferred.HouseholdId),

            HouseholdExtinguishedEvent extinguished => new ChronicleEntryDraft(
                extinguished.OccurredDate,
                ChronicleCategory.BirthsAndDeaths,
                ChronicleTier.Legendary,
                $"With the death of {Name(state, extinguished.LastHeadCharacterId)}, the line of this household came to an end.",
                new[] { extinguished.LastHeadCharacterId },
                extinguished.Type,
                extinguished.EventId.ToTaggedString(),
                extinguished.HouseholdId),

            SuccessionDisputeOpenedEvent opened => new ChronicleEntryDraft(
                opened.OccurredDate,
                ChronicleCategory.PoliticsAndOffice,
                ChronicleTier.Major,
                $"The succession of the household was contested among {opened.ClaimantIds.Count} claimant(s).",
                opened.ClaimantIds,
                opened.Type,
                opened.EventId.ToTaggedString(),
                opened.HouseholdId),

            SuccessionDisputeResolvedEvent resolved => new ChronicleEntryDraft(
                resolved.OccurredDate,
                ChronicleCategory.PoliticsAndOffice,
                ChronicleTier.Major,
                resolved.WinnerCharacterId is { } winner
                    ? $"{Name(state, winner)} prevailed in the contest for the household's headship."
                    : "The contested succession of the household ended without a clear victor.",
                resolved.WinnerCharacterId is { } winnerId ? new[] { winnerId } : Array.Empty<RuntimeId<Character>>(),
                resolved.Type,
                resolved.EventId.ToTaggedString(),
                resolved.HouseholdId),

            SplinterHouseholdFoundedEvent splinter => new ChronicleEntryDraft(
                splinter.OccurredDate,
                ChronicleCategory.PoliticsAndOffice,
                ChronicleTier.Legendary,
                $"{Name(state, splinter.FounderCharacterId)}, having lost the contest for headship, broke away to found an independent household.",
                new[] { splinter.FounderCharacterId },
                splinter.Type,
                splinter.EventId.ToTaggedString(),
                splinter.OriginHouseholdId),

            NonFamilyRegencyEstablishedEvent regency => new ChronicleEntryDraft(
                regency.OccurredDate,
                ChronicleCategory.PoliticsAndOffice,
                ChronicleTier.Notable,
                $"{Name(state, regency.RegentCharacterId)} was appointed regent for the household during {Name(state, regency.MinorHeadCharacterId)}'s minority.",
                new[] { regency.MinorHeadCharacterId, regency.RegentCharacterId },
                regency.Type,
                regency.EventId.ToTaggedString(),
                regency.HouseholdId),

            RegencyEndedEvent regencyEnded => new ChronicleEntryDraft(
                regencyEnded.OccurredDate,
                ChronicleCategory.PoliticsAndOffice,
                ChronicleTier.Notable,
                $"{Name(state, regencyEnded.FormerHeirNowHeadCharacterId)} came of age and took up the headship, ending {Name(state, regencyEnded.FormerRegentCharacterId)}'s regency.",
                new[] { regencyEnded.FormerHeirNowHeadCharacterId, regencyEnded.FormerRegentCharacterId },
                regencyEnded.Type,
                regencyEnded.EventId.ToTaggedString(),
                regencyEnded.HouseholdId),

            // Rival-house-only fact: no player household to attach it to (see ChronicleEntry.HouseholdId's
            // own doc comment) — ChronicleGenerationSystem cross-posts it straight to that actor's own
            // RivalDossier instead.
            LivingWorldActorExtinguishedEvent extinct => new ChronicleEntryDraft(
                extinct.OccurredDate,
                ChronicleCategory.Other,
                ChronicleTier.Legendary,
                $"The house of {extinct.ActorName} came to an end.",
                Array.Empty<RuntimeId<Character>>(),
                extinct.Type,
                extinct.EventId.ToTaggedString(),
                null,
                extinct.ActorId),

            // Only the terminal "Fall of the House" rung is Chronicle-worthy (§9 rung 5, named directly by
            // InsolvencySystem's own doc comment as the hook this item fills in) — an ordinary AtRisk/
            // Insolvent stage change is routine financial bookkeeping, not a household-defining moment.
            InsolvencyStageChangedEvent { Stage: InsolvencyStage.Ruined } ruined => new ChronicleEntryDraft(
                ruined.OccurredDate,
                ChronicleCategory.WealthAndBuilding,
                ChronicleTier.Major,
                "The household's fortunes collapsed entirely — the estate lies in ruin.",
                Array.Empty<RuntimeId<Character>>(),
                ruined.Type,
                ruined.EventId.ToTaggedString(),
                ruined.HouseholdId),

            // Only a discovered-and-escalated Scheme is Chronicle-worthy (§6: "a discovered-and-escalated
            // Scheme" is Major-default material) — a Scheme that succeeded quietly or was foiled privately
            // never becomes public record.
            SchemeResolvedEvent { Status: SchemeStatus.DiscoveredAndEscalated } scheme => new ChronicleEntryDraft(
                scheme.OccurredDate,
                ChronicleCategory.FaithAndScandal,
                ChronicleTier.Major,
                $"A scheme against {Name(state, scheme.TargetCharacterId)} by {Name(state, scheme.InitiatorCharacterId)} was discovered and escalated into open conflict.",
                new[] { scheme.InitiatorCharacterId, scheme.TargetCharacterId },
                scheme.Type,
                scheme.EventId.ToTaggedString(),
                HouseholdOf(state, scheme.InitiatorCharacterId) ?? HouseholdOf(state, scheme.TargetCharacterId)),

            // Phase 11 item 4: §8's "the laudatio funebris is a natural, real Chronicle-eligible moment
            // in its own right" — held funerals are chronicled; the laudatio itself is not (it needs
            // Rhetoric/orator mechanics, Politics & Patronage, not yet built). Tier follows FuneralTier
            // directly rather than dynamically from MemoriaGained/ImaginesDisplayed, matching every
            // other static tier mapping in this method.
            FuneralHeldEvent held => new ChronicleEntryDraft(
                held.OccurredDate,
                ChronicleCategory.BirthsAndDeaths,
                held.Tier switch
                {
                    FuneralTier.Grand => ChronicleTier.Major,
                    FuneralTier.Proper => ChronicleTier.Notable,
                    _ => ChronicleTier.Minor,
                },
                $"{Name(state, held.DeceasedCharacterId)} was laid to rest with a {held.Tier.ToString().ToLowerInvariant()} funeral.",
                new[] { held.DeceasedCharacterId },
                held.Type,
                held.EventId.ToTaggedString(),
                held.HouseholdId),

            // §8's "an early-broken mourning period... is a real, new Scandal source" — Scandal (Phase
            // 12, not yet built) has nothing to fire into yet, so this Chronicle entry is the one real
            // consequence this pass can actually deliver for it.
            MourningBrokenEarlyEvent broken => new ChronicleEntryDraft(
                broken.OccurredDate,
                ChronicleCategory.FaithAndScandal,
                ChronicleTier.Notable,
                $"The household's mourning for {Name(state, broken.TriggeringDeathCharacterId)} was broken before its time.",
                new[] { broken.TriggeringDeathCharacterId },
                broken.Type,
                broken.EventId.ToTaggedString(),
                broken.HouseholdId),

            // Phase 12 item 4 (§9 of gens-legal-court-design.md): "every real verdict... a real entry,
            // tiered by the case's own severity." Only the two scandal-shaped outcomes are actually
            // Chronicle-worthy here, matching InsolvencyStageChangedEvent's own "only the terminal rung"
            // precedent above — an ordinary Plaintiff/Defendant/SplitCompromise/Acquitted verdict is
            // routine civil bookkeeping, not a household-defining moment. A conviction is a real public
            // scandal (Political convictions doubly so, since §5.7's loss-of-office rides alongside it);
            // a Patria Potestas case is Chronicled specifically because §6 frames it as "a real, usable
            // political attack" regardless of its guaranteed Dismissal.
            LegalCaseRuledEvent { Verdict: LegalCaseVerdict.Convicted } convicted => new ChronicleEntryDraft(
                convicted.OccurredDate,
                convicted.CaseType == LegalCaseType.Political ? ChronicleCategory.PoliticsAndOffice : ChronicleCategory.FaithAndScandal,
                ChronicleTier.Major,
                $"The household stood convicted before the court on a {convicted.CaseType.ToString().ToLowerInvariant()} charge.",
                Array.Empty<RuntimeId<Character>>(),
                convicted.Type,
                convicted.EventId.ToTaggedString(),
                convicted.DefendantId),

            LegalCaseRuledEvent { IsPatriaPotestasCase: true } patriaPotestas => new ChronicleEntryDraft(
                patriaPotestas.OccurredDate,
                ChronicleCategory.FaithAndScandal,
                ChronicleTier.Notable,
                "A case brought over the household head's exercise of patria potestas was aired publicly, and dismissed.",
                Array.Empty<RuntimeId<Character>>(),
                patriaPotestas.Type,
                patriaPotestas.EventId.ToTaggedString(),
                patriaPotestas.DefendantId),

            // Phase 12 item 5 (§8 of gens-crime-punishment-imprisonment-design.md): "every execution...
            // is real material, tiered by this document's own Justified/Unjust distinction" — matching
            // InsolvencyStageChangedEvent's own "only the terminal rung is Chronicle-worthy" precedent
            // above, an ordinary Fine/Relegatio/Deportatio sentence is routine bookkeeping; only a
            // sentence that actually ends the Character's life is Chronicle-worthy, and an Unjust one
            // carries the document's own "a lasting mark... regardless" weight as Legendary rather than
            // Major.
            SentenceAppliedEvent { ResultedInDeath: true } sentenced => new ChronicleEntryDraft(
                sentenced.OccurredDate,
                ChronicleCategory.BirthsAndDeaths,
                sentenced.WasJustified ? ChronicleTier.Major : ChronicleTier.Legendary,
                sentenced.SentenceType == SentenceType.HonorableExit
                    ? $"{Name(state, sentenced.CharacterId)} took an honorable exit rather than face the executioner."
                    : $"{Name(state, sentenced.CharacterId)} was put to death by {sentenced.SentenceType.ToString().ToLowerInvariant()}.",
                new[] { sentenced.CharacterId },
                sentenced.Type,
                sentenced.EventId.ToTaggedString(),
                HouseholdOf(state, sentenced.CharacterId)),

            // §10: "every ransom resolution is real material." Only a genuine resolution (paid,
            // bargained down, or refused) is Chronicle-worthy — an open, unresolved negotiation is not.
            RansomNegotiationResolvedEvent { Resolution: RansomResolution.Refused } refused => new ChronicleEntryDraft(
                refused.OccurredDate,
                ChronicleCategory.Other,
                ChronicleTier.Notable,
                $"A ransom demand for {Name(state, refused.CaptiveCharacterId)} was refused.",
                new[] { refused.CaptiveCharacterId },
                refused.Type,
                refused.EventId.ToTaggedString(),
                HouseholdOf(state, refused.CaptiveCharacterId)),

            RansomNegotiationResolvedEvent ransomed => new ChronicleEntryDraft(
                ransomed.OccurredDate,
                ChronicleCategory.Other,
                ChronicleTier.Notable,
                $"{Name(state, ransomed.CaptiveCharacterId)} was ransomed and returned home.",
                new[] { ransomed.CaptiveCharacterId },
                ransomed.Type,
                ransomed.EventId.ToTaggedString(),
                HouseholdOf(state, ransomed.CaptiveCharacterId)),

            // Phase 12 item 7 (§9 of gens-scandal-design.md): "a sufficiently severe Scandal... leaves a
            // Faith & Scandal category Dynasty Chronicle entry permanently" — matching every other
            // "only the severe/terminal rung is Chronicle-worthy" precedent above, only a Scandal severe
            // enough to have actually landed the Scandal-Marked Trait is chronicled; a lesser,
            // trait-less Scandal is routine, forgettable social friction.
            ScandalRecordedEvent { ScandalMarkedTraitApplied: true } scandalRecorded => new ChronicleEntryDraft(
                scandalRecorded.OccurredDate,
                ChronicleCategory.FaithAndScandal,
                scandalRecorded.Severity == ScandalSeverity.NotaCensoriaEligible ? ChronicleTier.Legendary : ChronicleTier.Major,
                "The household was marked by a public scandal it could not live down quietly.",
                Array.Empty<RuntimeId<Character>>(),
                scandalRecorded.Type,
                scandalRecorded.EventId.ToTaggedString(),
                scandalRecorded.PrimaryHouseholdId),

            // §8/§9: "genuine, earned redemption" — real Chronicle-worthy material in its own right,
            // the counterpart entry to the Scandal-Marked moment above.
            CharacterRehabilitatedEvent rehabilitated => new ChronicleEntryDraft(
                rehabilitated.OccurredDate,
                ChronicleCategory.FaithAndScandal,
                ChronicleTier.Notable,
                $"{Name(state, rehabilitated.CharacterId)} lived down the household's old disgrace through years of sustained good conduct.",
                new[] { rehabilitated.CharacterId },
                rehabilitated.Type,
                rehabilitated.EventId.ToTaggedString(),
                rehabilitated.HouseholdId),

            // §3.1/§7: "regional recognition as a real exemplar... a genuine Chronicle-worthy Dignitas
            // event" — only the Defining threshold itself is chronicled, matching <see
            // cref="ScandalRecordedEvent"/>'s own "only the severe/notable rung is Chronicle-worthy"
            // precedent (an Emerging tier is real, visible flavor, but not yet exemplar-grade).
            DoctrineTierChangedEvent { NewTier: DoctrineTier.Defining } tierChanged => new ChronicleEntryDraft(
                tierChanged.OccurredDate,
                ChronicleCategory.PoliticsAndOffice,
                ChronicleTier.Major,
                $"The household became a real, recognized exemplar of {tierChanged.DoctrineType}.",
                Array.Empty<RuntimeId<Character>>(),
                tierChanged.Type,
                tierChanged.EventId.ToTaggedString(),
                tierChanged.HouseholdId),

            // §8: "every Edict Declaration... is guaranteed Chronicle material."
            ManumissionEdictIssuedEvent manumissionEdict => new ChronicleEntryDraft(
                manumissionEdict.OccurredDate,
                ChronicleCategory.PoliticsAndOffice,
                ChronicleTier.Major,
                $"The household proclaimed a Manumission Edict, freeing {manumissionEdict.CharactersFreed} enslaved workers in one stroke.",
                Array.Empty<RuntimeId<Character>>(),
                manumissionEdict.Type,
                manumissionEdict.EventId.ToTaggedString(),
                manumissionEdict.IssuingHouseholdId),

            CitizenshipEdictGrantedEvent citizenshipEdict => new ChronicleEntryDraft(
                citizenshipEdict.OccurredDate,
                ChronicleCategory.PoliticsAndOffice,
                ChronicleTier.Notable,
                $"{Name(state, citizenshipEdict.TargetCharacterId)} was granted Roman citizenship by Edict.",
                new[] { citizenshipEdict.TargetCharacterId },
                citizenshipEdict.Type,
                citizenshipEdict.EventId.ToTaggedString(),
                citizenshipEdict.IssuingHouseholdId),

            ProscriptionIssuedEvent proscription => new ChronicleEntryDraft(
                proscription.OccurredDate,
                ChronicleCategory.PoliticsAndOffice,
                ChronicleTier.Legendary,
                "The household proclaimed a Proscription, declaring a rival house outlaw and seizing its assets in one stroke.",
                Array.Empty<RuntimeId<Character>>(),
                proscription.Type,
                proscription.EventId.ToTaggedString(),
                proscription.IssuingHouseholdId),

            _ => null,
        };

    internal static string Name(WorldState state, RuntimeId<Character> characterId)
    {
        if (!state.Characters.TryGet(characterId, out var character))
            return characterId.ToTaggedString();

        return character.Cognomen is null
            ? $"{character.Praenomen} {character.Nomen}"
            : $"{character.Praenomen} {character.Nomen} {character.Cognomen}";
    }

    private static RuntimeId<Household>? HouseholdOf(WorldState state, RuntimeId<Character> characterId) =>
        state.Characters.TryGet(characterId, out var character) ? character.Household : null;
}
