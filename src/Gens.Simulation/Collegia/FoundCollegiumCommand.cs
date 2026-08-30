using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Religion;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Collegia;

/// <summary>
/// Founds a new Collegium at a settlement (Phase 12 item 6; §2-§3) — the one entry point every
/// collegium comes into being through, mirroring <see cref="RivalHouseCreationService"/>'s own
/// actor-creation shape but built as a real, validated <see cref="ICommand"/> rather than a bootstrap
/// service method, since founding a collegium is a genuine in-campaign occurrence (a player action, an
/// AI-selected one) rather than only campaign-seed data. Always starts Background-tier with no head
/// Character yet (§3's Magister is a real, later achievement — see <see cref="ElectMagisterCommand"/>)
/// and an <see cref="LivingWorldActorStandingTrend.Established"/> trend that this domain never itself
/// changes, so <see cref="BackgroundHouseDriftSystem"/>'s own Gens-only filter and the extinction
/// system's identical "only a Declining actor rolls" gate both leave a founded Collegium alone
/// (§12's own open question: Collegia founding/dissolution is this domain's job, not Rival Houses'
/// background fortune drift).
/// </summary>
public sealed record FoundCollegiumCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    string Name,
    RuntimeId<Settlement> SettlementId,
    CollegiumType CollegiumType,
    PopGroupType? LinkedPopGroupType = null,
    PatronDeity? LinkedPatronDeity = null) : ICommand;

/// <summary>Emitted whenever a <see cref="FoundCollegiumCommand"/> is accepted. Public — a collegium's
/// existence is a real, visible institutional fact, the same reasoning <see
/// cref="Magistracies.MagistracyAssumedEvent"/> already gives for an office being assumed.</summary>
public sealed record CollegiumFoundedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Actor> CollegiumId,
    string Name,
    RuntimeId<Settlement> SettlementId,
    CollegiumType CollegiumType,
    string? CausationId) : IDomainEvent
{
    public string Type => "collegia.collegiumFounded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CollegiumId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="FoundCollegiumCommand"/> (ADR 0006).</summary>
public static class FoundCollegiumCommands
{
    public static readonly ValidationErrorCode NameRequired = new("collegia.foundCollegium.nameRequired");
    public static readonly ValidationErrorCode SettlementNotFound = new("collegia.foundCollegium.settlementNotFound");
    public static readonly ValidationErrorCode PopGroupTypeRequired = new("collegia.foundCollegium.popGroupTypeRequired");
    public static readonly ValidationErrorCode PopGroupTypeNotTradeEligible = new("collegia.foundCollegium.popGroupTypeNotTradeEligible");
    public static readonly ValidationErrorCode PopGroupTypeMustBeUnset = new("collegia.foundCollegium.popGroupTypeMustBeUnset");
    public static readonly ValidationErrorCode PatronDeityRequired = new("collegia.foundCollegium.patronDeityRequired");
    public static readonly ValidationErrorCode PatronDeityMustBeUnset = new("collegia.foundCollegium.patronDeityMustBeUnset");

    public static readonly CommandPipeline<WorldState, FoundCollegiumCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, FoundCollegiumCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            return NameRequired;
        if (!state.Settlements.TryGet(command.SettlementId, out _))
            return SettlementNotFound;

        if (command.CollegiumType == CollegiumType.Opificum)
        {
            if (command.LinkedPopGroupType is not { } popGroupType)
                return PopGroupTypeRequired;
            if (popGroupType is not (PopGroupType.Opifices or PopGroupType.Negotiatores))
                return PopGroupTypeNotTradeEligible;
        }
        else if (command.LinkedPopGroupType is not null)
        {
            return PopGroupTypeMustBeUnset;
        }

        if (command.CollegiumType == CollegiumType.CultSpecific)
        {
            if (command.LinkedPatronDeity is null)
                return PatronDeityRequired;
        }
        else if (command.LinkedPatronDeity is not null)
        {
            return PatronDeityMustBeUnset;
        }

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, FoundCollegiumCommand command)
    {
        state.Settlements.TryGet(command.SettlementId, out var settlement);

        // LivingWorldActorOrigin's three values (Ancient/NovusHomo/CadetBranch) are all Rival-Houses-
        // specific framing (§2.2 of that document) with no real equivalent for how a Collegium comes to
        // exist; Ancient is picked here purely because it carries no further invariant (unlike
        // CadetBranch's required parent) — Origin is never read anywhere in this domain.
        var actor = LivingWorldActor.Create(
            state.ActorIds.Issue(),
            LivingWorldActorType.Collegium,
            command.Name,
            LivingWorldActorTier.Background,
            LivingWorldActorStandingTrend.Established,
            LivingWorldActorOrigin.Ancient,
            parentActorId: null,
            LivingWorldActorIdentity.None,
            dignitas: 0,
            new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null),
            new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Negligible),
            settlement!.RegionId,
            command.SettlementId);
        state.Actors.Add(actor.ActorId, actor);

        state.Collegia.Add(
            actor.ActorId,
            new CollegiumDetails(
                actor.ActorId, command.CollegiumType, CollegiumLegalStatus.Licitum, command.LinkedPopGroupType,
                command.LinkedPatronDeity, ScholaPropertyId: null, PatronHouseholdId: null,
                QuinquennalisCharacterId: null, MemberHouseholdIds: Array.Empty<RuntimeId<Household>>()));

        return new IDomainEvent[]
        {
            new CollegiumFoundedEvent(
                state.EventIds.Issue(), command.SubmittedDate, actor.ActorId, command.Name, command.SettlementId,
                command.CollegiumType, command.CommandId.ToTaggedString()),
        };
    }
}
