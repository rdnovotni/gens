using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Actors;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Combat;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Military;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Interactions;

/// <summary>
/// Wires <see cref="RaidThreatSystem"/>'s own deferred Retaliation (Phase 16 item 2's §5, closed here
/// per item 4's own "the natural, now-unblocked follow-up" progress note) onto the shared Combat
/// Resolution Engine, mirroring <see cref="Military.MilitaryCommands.ResolveMilitaryDeploymentCommand"/>'s
/// exact shape: the retaliating household's own <see cref="EstateForce"/> Squads project into <see
/// cref="CombatantGroup"/>s through the same <see cref="Military.MilitaryCommands.ToCombatantType"/>/<see
/// cref="Military.MilitaryCommands.CommanderMultiplier"/> helpers Military itself uses, and the targeted
/// <see cref="BanditConfederation"/> (per <c>gens-piracy-banditry-design.md</c> §2's own actor framework)
/// projects into a single <see cref="CombatantType.Irregular"/> group — the type <see
/// cref="CombatModels"/> itself reserves for exactly bandits and pirates.
///
/// This resolves as an abstract, no-travel engagement — no <see cref="Military.MilitaryDeployment"/> is
/// created or required — because a Bandit Confederation has no real <see cref="Travel.TravelLocation"/>-
/// anchored base location in this codebase yet (confirmed directly: <see
/// cref="Travel.TravelLocation.RivalEstate"/> has zero live callers anywhere). This is a deliberate,
/// disclosed scope boundary matching item 2's own precedent, not an oversight — a future Travel pass
/// that gives Confederations a real base can upgrade this into a travel-anchored deployment without
/// changing this command's own outward shape.
/// </summary>
public sealed record RetaliateAgainstConfederationCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> SponsorId,
    RuntimeId<Settlement> ForceSettlementId,
    RuntimeId<Actor> ConfederationActorId,
    IReadOnlyList<RuntimeId<Squad>> SquadIds,
    CombatSituation AttackerSituation,
    string AftermathSummary) : ICommand;

/// <summary>Emitted whenever a <see cref="RetaliateAgainstConfederationCommand"/> resolves.</summary>
public sealed record ConfederationRetaliationResolvedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Actor> ConfederationActorId,
    CombatOutcome Outcome,
    string? CausationId) : IDomainEvent
{
    public string Type => "interactions.confederationRetaliationResolved";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), ConfederationActorId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="RetaliateAgainstConfederationCommand"/> (ADR 0006).
/// RNG-consuming (the Combat Resolution Engine's variance roll), so — like <see
/// cref="Military.MilitaryCommands.CreateResolveDeploymentPipeline"/> — it captures an injected <see
/// cref="RandomStreamSet"/> rather than being exposed as a static field.</summary>
public static class RetaliateAgainstConfederationCommands
{
    /// <summary>The named random stream this command draws from, reserved separately from <see
    /// cref="CampaignBootstrapper.MilitaryCombatResolutionStreamName"/> (rule 8, ADR 0004).</summary>
    public const string StreamName = CampaignBootstrapper.RaidRetaliationStreamName;

    public static readonly ValidationErrorCode ForceNotFound = new("interactions.retaliate.forceNotFound");
    public static readonly ValidationErrorCode SponsorInvalid = new("interactions.retaliate.sponsorInvalid");
    public static readonly ValidationErrorCode ConfederationInvalid = new("interactions.retaliate.confederationInvalid");
    public static readonly ValidationErrorCode SquadUnavailable = new("interactions.retaliate.squadUnavailable");
    public static readonly ValidationErrorCode SummaryRequired = new("interactions.retaliate.summaryRequired");

    public static CommandPipeline<WorldState, RetaliateAgainstConfederationCommand> CreatePipeline(RandomStreamSet randomStreams)
    {
        if (randomStreams is null)
            throw new ArgumentNullException(nameof(randomStreams));

        return new CommandPipeline<WorldState, RetaliateAgainstConfederationCommand>(
            validate: Validate,
            mutate: (state, command) => Mutate(state, command, randomStreams),
            issueSequenceNumber: static state => state.IssueCommandSequenceNumber());
    }

    private static ValidationErrorCode? Validate(WorldState state, RetaliateAgainstConfederationCommand command)
    {
        if (!state.EstateForces.TryGet(command.ForceSettlementId, out var force) || force!.HouseholdId != command.HouseholdId)
            return ForceNotFound;
        if (!MilitaryCommands.ValidSponsor(state, command.SponsorId, command.HouseholdId, command.ForceSettlementId))
            return SponsorInvalid;
        if (!state.Actors.TryGet(command.ConfederationActorId, out var confederation) ||
            confederation!.ActorType != LivingWorldActorType.BanditConfederation)
            return ConfederationInvalid;
        if (command.SquadIds is null || command.SquadIds.Count == 0 || command.SquadIds.Distinct().Count() != command.SquadIds.Count)
            return SquadUnavailable;
        foreach (var squadId in command.SquadIds)
            if (!state.Squads.TryGet(squadId, out var squad) || squad!.ForceSettlementId != command.ForceSettlementId ||
                squad.Status != SquadStatus.Ready || squad.Manpower <= 0)
                return SquadUnavailable;
        if (string.IsNullOrWhiteSpace(command.AftermathSummary))
            return SummaryRequired;
        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, RetaliateAgainstConfederationCommand command, RandomStreamSet randomStreams)
    {
        state.Actors.TryGet(command.ConfederationActorId, out var confederation);
        state.EstateForces.TryGet(command.ForceSettlementId, out var force);

        var groups = new List<CombatantGroup>();
        RuntimeId<Character>? commanderId = null;
        foreach (var squadId in command.SquadIds)
        {
            state.Squads.TryGet(squadId, out var squad);
            groups.Add(new CombatantGroup(MilitaryCommands.ToCombatantType(squad!.Type), squad.Manpower, squad.EquipmentTier, squad.Readiness, squad.Morale));
            commanderId ??= squad.CommanderId;
        }
        commanderId ??= force!.PraefectusId;

        CombatantCommander? commander = null;
        if (commanderId is { } id && state.Characters.TryGet(id, out var character) && character!.IsAlive)
            commander = new CombatantCommander(MilitaryCommands.CommanderMultiplier(character.GetEffectiveAttributes().Martial));

        var attacker = new CombatSide(groups, commander, command.AttackerSituation);
        var defender = new CombatSide(
            new[] { ProjectConfederation(confederation!.MilitaryStrength.Band) }, null,
            new CombatSituation(CombatTerrain.Open, Ambush: false, Fortified: false));

        var resolution = CombatResolutionEngine.Resolve(attacker, defender, randomStreams, StreamName);

        for (var i = 0; i < command.SquadIds.Count; i++)
        {
            state.Squads.TryGet(command.SquadIds[i], out var squad);
            var loss = resolution.AttackerLosses[i];
            var manpower = Math.Max(0, squad!.Manpower - loss.Casualties);
            MilitaryCommands.SetSquad(state, squad with
            {
                Manpower = manpower,
                Readiness = Math.Max(0, squad.Readiness - loss.ReadinessLoss),
                Morale = Math.Max(0, squad.Morale - loss.MoraleLoss),
                Status = manpower == 0 ? SquadStatus.Destroyed : SquadStatus.Ready,
            });
        }

        if (resolution.AttackerOutcome is CombatOutcome.DecisiveVictory or CombatOutcome.CostlyVictory)
        {
            var newTrend = RaidThreatSystem.StepTowardDeclining(confederation.StandingTrend);
            if (newTrend != confederation.StandingTrend)
            {
                state.Actors.Remove(command.ConfederationActorId);
                state.Actors.Add(command.ConfederationActorId, confederation with { StandingTrend = newTrend });
            }
        }

        return new IDomainEvent[]
        {
            new ConfederationRetaliationResolvedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, command.ConfederationActorId,
                resolution.AttackerOutcome, command.CommandId.ToTaggedString()),
        };
    }

    /// <summary>A coarse, disclosed-as-invented mapping from a Confederation's abstract <see
    /// cref="MilitaryStrengthBand"/> (<see cref="Actors.LivingWorldActorMilitaryStrength"/>'s own "no
    /// Force/Squad record exists" placeholder) to a concrete <see cref="CombatantGroup"/> the shared
    /// engine can actually score. No design doc sizes a Confederation's real manpower/equipment/
    /// readiness/morale anywhere — these numbers are this implementation's own first pass, matching
    /// <see cref="Military.MilitaryCommands.CommanderMultiplier"/>'s identical disclosure for its own
    /// invented figures.</summary>
    private static CombatantGroup ProjectConfederation(MilitaryStrengthBand band)
    {
        var (manpower, equipmentTier, readiness, morale) = band switch
        {
            MilitaryStrengthBand.Negligible => (15, 0, 30, 40),
            MilitaryStrengthBand.Modest => (30, 1, 40, 50),
            MilitaryStrengthBand.Notable => (60, 1, 50, 60),
            MilitaryStrengthBand.Formidable => (100, 2, 60, 70),
            _ => throw new ArgumentOutOfRangeException(nameof(band), band, "Unhandled military strength band."),
        };
        return new CombatantGroup(CombatantType.Irregular, manpower, equipmentTier, readiness, morale);
    }
}
