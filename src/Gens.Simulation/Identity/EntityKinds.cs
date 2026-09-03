using Gens.Simulation.BusinessCompetition;
using Gens.Simulation.Characters;
using Gens.Simulation.Chronicle;
using Gens.Simulation.Correspondence;
using Gens.Simulation.Crime;
using Gens.Simulation.Economy;
using Gens.Simulation.Edicts;
using Gens.Simulation.Epithets;
using Gens.Simulation.Events;
using Gens.Simulation.Funerary;
using Gens.Simulation.Hazards;
using Gens.Simulation.Health;
using Gens.Simulation.History;
using Gens.Simulation.Interactions;
using Gens.Simulation.Land;
using Gens.Simulation.Languages;
using Gens.Simulation.Ledger;
using Gens.Simulation.Legal;
using Gens.Simulation.Magistracies;
using Gens.Simulation.NotableBusinesses;
using Gens.Simulation.PrivateInfrastructure;
using Gens.Simulation.PublicContracts;
using Gens.Simulation.RealEstate;
using Gens.Simulation.Religion;
using Gens.Simulation.Reputation;
using Gens.Simulation.Scandal;
using Gens.Simulation.Societates;
using Gens.Simulation.Stewardship;
using Gens.Simulation.Succession;
using Gens.Simulation.Travel;
using Gens.Simulation.Wanderers;

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
        // Phase 13 item 2 — Gens.Simulation.Travel.TravelTrip, same "real record as its own tag"
        // convention as EdictRecord above.
        [typeof(TravelTrip)] = "traveltrip",
        // Phase 13 item 3 — Gens.Simulation.Correspondence.Letter, same "real record as its own tag"
        // convention as TravelTrip above.
        [typeof(Letter)] = "letter",
        // Phase 13 item 4 — Gens.Simulation.Languages.LanguageProficiency, same "real record as its own
        // tag" convention as Letter above. LiteracyRecord and InterpresAppointment need no entry: each
        // is keyed by RuntimeId<Character>/RuntimeId<Household>, not by its own RuntimeId, matching
        // DynasticEpithet's identical exemption.
        [typeof(LanguageProficiency)] = "langprof",
        // Phase 13 item 5 — Gens.Simulation.History.DivergenceRecord, same "real record as its own tag"
        // convention as LanguageProficiency above. HistoricalTimelineEntryDefinition and
        // NamedHistoricalFigureDefinition need no entry: both are content-authored (DefinitionId<T>,
        // never runtime-instantiated), matching Culture/Good/Trait's identical exemption at the top of
        // this file. FiredHistoricalTimelineEntryIds needs no entry either: it's keyed by a plain string
        // (the content entry's own DefinitionId value), not by its own RuntimeId.
        [typeof(DivergenceRecord)] = "divergence",
        // Phase 13 item 7 — Gens.Simulation.Land.DistantHolding, same "real record as its own tag"
        // convention as DivergenceRecord above.
        [typeof(DistantHolding)] = "distantholding",
        // Phase 14 item 1 — Gens.Simulation.Health.CharacterHealthCondition, same "real record as its
        // own tag" convention as DistantHolding above.
        [typeof(CharacterHealthCondition)] = "healthcond",
        // Phase 14 item 2 — Gens.Simulation.Health.EpidemicOutbreak, same "real record as its own tag"
        // convention as CharacterHealthCondition above.
        [typeof(EpidemicOutbreak)] = "epidemicoutbreak",
        // Phase 14 item 3 — Gens.Simulation.Hazards.DisasterEvent, same "real record as its own tag"
        // convention as EpidemicOutbreak above. DormantVolcano needs no entry here: it is keyed by its
        // own Plot's RuntimeId<Plot>, never instantiated with its own RuntimeId<DormantVolcano>, the
        // same exemption SettlementSanitationInvestment (keyed by RuntimeId<Settlement>) already has.
        [typeof(DisasterEvent)] = "disasterevent",
        // Phase 14 item 4 — Gens.Simulation.Wanderers.Wanderer and Gens.Simulation.Wanderers.
        // WandererEngagement, same "real record as its own tag" convention as DisasterEvent above.
        [typeof(Wanderer)] = "wanderer",
        [typeof(WandererEngagement)] = "wandererengagement",
        // Phase 15 item 1 — Gens.Simulation.RealEstate.District and PropertyRecord, same "real record
        // as its own tag" convention as Wanderer/WandererEngagement above.
        [typeof(District)] = "district",
        [typeof(PropertyRecord)] = "propertyrecord",
        // Phase 15 item 2 — Gens.Simulation.Societates.Societas, same "real record as its own tag"
        // convention as District/PropertyRecord above. ActioProSocioLink needs no entry: it is keyed
        // by RuntimeId<LegalCase> (the already-issued case ID), not by its own RuntimeId, matching
        // DynasticEpithet's identical exemption above.
        [typeof(Societas)] = "societas",
        // Phase 15 item 3 needs no entry at all: Gens.Simulation.MerchantFamilies.MerchantHouseArchetype
        // is keyed by a plain owner-tag string (the same exemption FiredHistoricalTimelineEntryIds
        // already has above), and Gens.Simulation.MerchantFamilies.SenateEntryInvestmentLog is keyed by
        // the already-registered RuntimeId<Household>, not by a RuntimeId of its own.
        // Phase 15 item 4 — Gens.Simulation.NotableBusinesses.NotableBusiness, same "real record as its
        // own tag" convention as Societas above. NotableBusinessRivalryLog and
        // NotableBusinessGovernmentContract need no entry: both are keyed by the already-registered
        // RuntimeId<NotableBusiness>, not by a RuntimeId of their own, matching SenateEntryInvestmentLog's
        // identical exemption.
        [typeof(NotableBusiness)] = "notablebusiness",
        // Phase 15 item 5 — Gens.Simulation.BusinessCompetition.CartelAgreement, same "real record as its
        // own tag" convention as NotableBusiness above. CompetitiveEscalation and GrainHoardingRecord need
        // no entry: both are keyed by the already-registered RuntimeId<NotableBusiness>, not by a
        // RuntimeId of their own, matching SenateEntryInvestmentLog's identical exemption.
        // MarketCapacityReading needs no entry either: it is keyed by MarketGoodKey, not by its own
        // RuntimeId, matching HouseholdDoctrineState's identical exemption.
        [typeof(CartelAgreement)] = "cartelagreement",
        // Phase 15 item 6 — Gens.Simulation.PublicContracts's own four new runtime-entity kinds, each a
        // "real record as its own tag" per that same convention: PublicContract, ContractBid,
        // LustrumEvent, and ContractFraudRecord. ContractFraudLegalLink needs no entry: it is keyed by
        // the already-registered RuntimeId<LegalCase>, not by a RuntimeId of its own, matching
        // ActioProSocioLink's identical exemption.
        [typeof(PublicContract)] = "publiccontract",
        [typeof(ContractBid)] = "contractbid",
        [typeof(LustrumEvent)] = "lustrumevent",
        [typeof(ContractFraudRecord)] = "contractfraudrecord",
        // Phase 15 item 7 — Gens.Simulation.PrivateInfrastructure's own two new runtime-entity kinds
        // needing a real identity of their own, same "real record as its own tag" convention as
        // PublicContract/ContractBid above: PavedRoadConnection and PrivateBridge. IrrigationCanal,
        // WellOrCistern, LandReclamationProject, and BoundaryInfrastructure need no entry: each is
        // keyed by the already-registered RuntimeId<Plot> it was built on, not by its own RuntimeId,
        // matching SenateEntryInvestmentLog's identical exemption above. InfrastructureCondition and
        // UnifiedEstateMilestones need no entry either: keyed by InfrastructureConditionKey and
        // RuntimeId<Household> respectively, matching HouseholdDoctrineState's identical exemption.
        [typeof(PavedRoadConnection)] = "pavedroad",
        [typeof(PrivateBridge)] = "privatebridge",
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
