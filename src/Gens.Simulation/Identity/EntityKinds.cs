using Gens.Simulation.Characters;
using Gens.Simulation.Chronicle;
using Gens.Simulation.Crime;
using Gens.Simulation.Economy;
using Gens.Simulation.Edicts;
using Gens.Simulation.Epithets;
using Gens.Simulation.Events;
using Gens.Simulation.Funerary;
using Gens.Simulation.Interactions;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Legal;
using Gens.Simulation.Magistracies;
using Gens.Simulation.Religion;
using Gens.Simulation.Reputation;
using Gens.Simulation.Scandal;
using Gens.Simulation.Stewardship;
using Gens.Simulation.Succession;

namespace Gens.Simulation.Identity;

// Phantom marker types for RuntimeId<T> and DefinitionId<T>. Never instantiated — only used as a
// compile-time tag so, e.g., a RuntimeId<Character> can never be passed where a RuntimeId<Plot> is
// expected (ADR 0001). Runtime-instantiated kinds (created during a campaign) get RuntimeId<T>;
// content-authored kinds (defined ahead of time, never runtime-generated) get DefinitionId<T>.
//
// Character (Phase 5 item 1) is the first entity kind whose real record now exists
// (Characters.Character) — RuntimeId<T>'s tag parameter is unconstrained, so that real record serves
// directly as its own RuntimeId/DefinitionId tag rather than needing a separate, never-instantiated
// marker class here. Region, Settlement, Plot, and Holding (Phase 6 item 1) use the same pattern:
// their real records in Gens.Simulation.Land now serve as the type parameters directly. Every other
// kind below still uses a local phantom marker until its own real record lands.

public sealed class Campaign
{
    private Campaign()
    {
    }
}

public sealed class Household
{
    private Household()
    {
    }
}

public sealed class Actor
{
    private Actor()
    {
    }
}

/// <summary>Phantom type for content-authored culture definitions (Phase 5 item 1). Content-authored
/// only — never runtime-instantiated, so it uses <see cref="DefinitionId{T}"/> rather than <see
/// cref="RuntimeId{T}"/> and needs no <see cref="RuntimeIdTagRegistry"/> entry.</summary>
public sealed class Culture
{
    private Culture()
    {
    }
}

public sealed class Good
{
    private Good()
    {
    }
}

/// <summary>Phantom type for content-authored trait definitions (Phase 5 item 4;
/// <c>gens-traits-design.md</c>). Content-authored only, like <see cref="Culture"/> — a Character
/// holds a trait as a bare <see cref="DefinitionId{T}"/> reference rather than an embedded copy of
/// the trait's data, per rule 10 ("content is data, rules are code").</summary>
public sealed class Trait
{
    private Trait()
    {
    }
}

public sealed class Building
{
    private Building()
    {
    }
}

public sealed class Contract
{
    private Contract()
    {
    }
}

public sealed class Activity
{
    private Activity()
    {
    }
}

/// <summary>Phantom type for command IDs (ADR 0006). Not one of the roadmap's listed content/runtime kinds.</summary>
public sealed class Command
{
    private Command()
    {
    }
}

/// <summary>Phantom type for domain event IDs (ADR 0007). Not one of the roadmap's listed content/runtime kinds.</summary>
public sealed class DomainEventEntity
{
    private DomainEventEntity()
    {
    }
}

/// <summary>Phantom type for scheduled-action IDs (Phase 4 item 4: "scheduled actions and a calendar
/// queue for future-dated work"). Not one of the roadmap's listed content/runtime kinds.</summary>
public sealed class ScheduledAction
{
    private ScheduledAction()
    {
    }
}

/// <summary>Maps each entity-kind phantom type to its short save-file tag, e.g. <c>Character</c> → <c>char</c>.</summary>
internal static class RuntimeIdTagRegistry
{
    private static readonly Dictionary<Type, string> Tags = new()
    {
        [typeof(Campaign)] = "campaign",
        [typeof(Region)] = "region",
        [typeof(Settlement)] = "settlement",
        [typeof(Plot)] = "plot",
        [typeof(Household)] = "household",
        [typeof(Actor)] = "actor",
        [typeof(Character)] = "char",
        [typeof(Holding)] = "holding",
        [typeof(Building)] = "building",
        [typeof(Contract)] = "contract",
        [typeof(Activity)] = "activity",
        [typeof(Command)] = "cmd",
        [typeof(DomainEventEntity)] = "event",
        [typeof(ScheduledAction)] = "action",
        // Phase 8 item 1 — the real Gens.Simulation.Ledger.LedgerTransaction record (defined once
        // its own file lands) serves directly as its own RuntimeId tag, matching Character/Region/
        // Settlement/Plot/Holding's identical "real record replaces the phantom marker" convention
        // (this file's own top-of-file doc comment).
        [typeof(LedgerTransaction)] = "ledgertxn",
        // Phase 8 item 6 — Gens.Simulation.Economy.DebtRecord, same "real record as its own tag"
        // convention as LedgerTransaction above.
        [typeof(DebtRecord)] = "debt",
        // Phase 8 item 7 — Gens.Simulation.Economy.StandingContract, same convention.
        [typeof(StandingContract)] = "stcontract",
        // Phase 9 item 3 — Gens.Simulation.Events.EventInstance, same "real record as its own tag"
        // convention as LedgerTransaction/DebtRecord/StandingContract above.
        [typeof(EventInstance)] = "eventinst",
        // Phase 10 item 2 — Gens.Simulation.Stewardship.StewardshipAssignment, same convention.
        [typeof(StewardshipAssignment)] = "stewardship",
        // Phase 10 item 10 — Gens.Simulation.Stewardship.AutonomousDecisionLog, same convention.
        [typeof(AutonomousDecisionLog)] = "stewarddecision",
        // Phase 10 item 6 — Gens.Simulation.Interactions.Scheme, same "real record as its own tag"
        // convention as StewardshipAssignment/AutonomousDecisionLog above.
        [typeof(Scheme)] = "scheme",
        // Phase 10 package 13 — Gens.Simulation.Stewardship.ReturnReport, same "real record as its
        // own tag" convention as StewardshipAssignment/AutonomousDecisionLog/Scheme above.
        [typeof(ReturnReport)] = "returnreport",
        // Phase 11 item 1 — Gens.Simulation.Succession.SuccessionDispute, same "real record as its
        // own tag" convention as ReturnReport/Scheme above.
        [typeof(SuccessionDispute)] = "succdispute",
        // Phase 11 item 3 — Gens.Simulation.Chronicle.ChronicleEntry, same convention.
        [typeof(ChronicleEntry)] = "chronentry",
        // Phase 11 item 4 — Gens.Simulation.Funerary.FuneralRecord, same "real record as its own tag"
        // convention as ChronicleEntry/SuccessionDispute above.
        [typeof(FuneralRecord)] = "funeral",
        // Phase 11 item 5 — Gens.Simulation.Epithets.Agnomen, same "real record as its own tag"
        // convention as FuneralRecord/ChronicleEntry/SuccessionDispute above.
        [typeof(Agnomen)] = "agnomen",
        // Phase 11 item 5 — Gens.Simulation.Epithets.InheritedCognomenDecision, same convention.
        // DynasticEpithet needs no entry: it's keyed by RuntimeId<Household>, not RuntimeId<DynasticEpithet>.
        [typeof(InheritedCognomenDecision)] = "inheritedcognomen",
        // Phase 12 item 1 — Gens.Simulation.Reputation.FavorObligation, same "real record as its own
        // tag" convention as InheritedCognomenDecision/Agnomen/FuneralRecord above. HouseholdReputation
        // needs no entry: it's keyed by RuntimeId<Household>, not RuntimeId<HouseholdReputation>,
        // matching DynasticEpithet's identical exemption above.
        [typeof(FavorObligation)] = "favor",
        // Phase 12 item 2 — Gens.Simulation.Magistracies.MagistracyRecord, same "real record as its
        // own tag" convention as FavorObligation above. ClientelaEntry/HouseholdInfluence/
        // CharacterFactionAlignment need no entry: each is keyed by RuntimeId<Character> or
        // RuntimeId<Household>, not by its own RuntimeId, matching DynasticEpithet's identical exemption.
        [typeof(MagistracyRecord)] = "magistracy",
        // Phase 12 item 3 — Gens.Simulation.Religion.OmenEvent and Gens.Simulation.Religion.
        // PriesthoodRecord, same "real record as its own tag" convention as MagistracyRecord above.
        // HouseholdReligion needs no entry: it's keyed by RuntimeId<Household>, not by its own
        // RuntimeId, matching DynasticEpithet's identical exemption.
        [typeof(OmenEvent)] = "omen",
        [typeof(PriesthoodRecord)] = "priesthood",
        // Phase 12 item 4 — Gens.Simulation.Legal.LegalCase, same "real record as its own tag"
        // convention as OmenEvent/PriesthoodRecord/MagistracyRecord above.
        [typeof(LegalCase)] = "legalcase",
        // Phase 12 item 5 — Gens.Simulation.Crime.PunishableOffense, DetentionRecord, SentenceRecord,
        // and RansomNegotiation, same "real record as its own tag" convention as LegalCase above.
        [typeof(PunishableOffense)] = "punishableoffense",
        [typeof(DetentionRecord)] = "detention",
        [typeof(SentenceRecord)] = "sentence",
        [typeof(RansomNegotiation)] = "ransom",
        // Phase 12 item 7 — Gens.Simulation.Scandal.ScandalRecord, same "real record as its own tag"
        // convention as LegalCase/PunishableOffense above.
        [typeof(ScandalRecord)] = "scandal",
        // Phase 12 item 9 — Gens.Simulation.Edicts.EdictRecord, same "real record as its own tag"
        // convention as ScandalRecord above. HouseholdDoctrineState needs no entry: it's keyed by
        // HouseholdDoctrineKey, not by its own RuntimeId, matching DynasticEpithet's identical exemption.
        [typeof(EdictRecord)] = "edict",
    };

    public static string Resolve(Type type) =>
        Tags.TryGetValue(type, out var tag)
            ? tag
            : throw new InvalidOperationException($"No RuntimeId tag is registered for entity kind '{type.Name}'.");
}

/// <summary>Caches the resolved tag for entity kind <typeparamref name="T"/> once per closed generic type.</summary>
internal static class RuntimeIdTag<T>
{
    public static readonly string Tag = RuntimeIdTagRegistry.Resolve(typeof(T));
}
