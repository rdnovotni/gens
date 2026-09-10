#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Crime;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using Gens.Simulation.Travel;

namespace Gens.Simulation.Military;

public sealed record EstablishEstateForceCommand(RuntimeId<Command> CommandId, string ActorId, GameDate SubmittedDate,
    string? CausationId, RuntimeId<Household> HouseholdId, RuntimeId<Character> SponsorId,
    RuntimeId<Settlement> SettlementId, ForceInfrastructureTier InfrastructureTier) : ICommand;

public sealed record RaiseSquadCommand(RuntimeId<Command> CommandId, string ActorId, GameDate SubmittedDate,
    string? CausationId, RuntimeId<Settlement> ForceSettlementId, RuntimeId<Character> SponsorId,
    string Name, SquadType Type, SquadRecruitmentSource RecruitmentSource, PopGroupType? SourcePopGroup,
    int Manpower) : ICommand;

public sealed record CommitSquadEquipmentCommand(RuntimeId<Command> CommandId, string ActorId, GameDate SubmittedDate,
    string? CausationId, RuntimeId<Character> SponsorId, RuntimeId<Squad> SquadId, RuntimeId<Holding> StockpileHoldingId,
    EquipmentKind Kind, DefinitionId<Good> GoodId, GoodQuality? Quality, long Quantity) : ICommand;

public sealed record AssignForceCommanderCommand(RuntimeId<Command> CommandId, string ActorId, GameDate SubmittedDate,
    string? CausationId, RuntimeId<Character> SponsorId, RuntimeId<Settlement> ForceSettlementId, RuntimeId<Character> CommanderId,
    RuntimeId<Squad>? SquadId = null) : ICommand;

public sealed record BeginMilitaryDeploymentCommand(RuntimeId<Command> CommandId, string ActorId, GameDate SubmittedDate,
    string? CausationId, RuntimeId<Character> SponsorId, RuntimeId<Settlement> ForceSettlementId, MilitaryDeploymentType Type,
    TravelLocation Destination, IReadOnlyList<RuntimeId<Squad>> SquadIds) : ICommand;

public sealed record DemobilizeSquadCommand(RuntimeId<Command> CommandId, string ActorId, GameDate SubmittedDate,
    string? CausationId, RuntimeId<Character> SponsorId, RuntimeId<Squad> SquadId) : ICommand;

public sealed record ApplyMilitaryAftermathCommand(RuntimeId<Command> CommandId, string ActorId, GameDate SubmittedDate,
    string? CausationId, RuntimeId<MilitaryDeployment> DeploymentId, MilitaryOutcome Outcome,
    IReadOnlyList<SquadLoss> Losses, int CaptivesTaken, RuntimeId<Settlement>? CaptiveSourceSettlementId,
    PopGroupType? CaptiveSourcePopGroup, RuntimeId<Settlement>? CaptiveIntakeSettlementId,
    IReadOnlyList<RuntimeId<Character>> CapturedCharacters, RuntimeId<Household>? CaptorHouseholdId,
    string AftermathSummary) : ICommand;

public sealed record MilitaryStateChangedEvent(RuntimeId<DomainEventEntity> EventId, GameDate OccurredDate,
    string Change, IReadOnlyList<string> SubjectIds, string? CausationId) : IDomainEvent
{
    public string Type => $"military.{Change}";
    public int SchemaVersion => 1;
    public Visibility Visibility => Visibility.Public;
}

public static class MilitaryCommands
{
    public static readonly ValidationErrorCode SponsorInvalid = new("military.sponsorInvalid");
    public static readonly ValidationErrorCode ForceNotFound = new("military.forceNotFound");
    public static readonly ValidationErrorCode ForceAlreadyExists = new("military.forceAlreadyExists");
    public static readonly ValidationErrorCode SquadCapReached = new("military.squadCapReached");
    public static readonly ValidationErrorCode InvalidManpower = new("military.invalidManpower");
    public static readonly ValidationErrorCode InvalidRecruitmentSource = new("military.invalidRecruitmentSource");
    public static readonly ValidationErrorCode InsufficientPopulation = new("military.insufficientPopulation");
    public static readonly ValidationErrorCode InsufficientFunds = new("military.insufficientFunds");
    public static readonly ValidationErrorCode SquadNotFound = new("military.squadNotFound");
    public static readonly ValidationErrorCode SquadUnavailable = new("military.squadUnavailable");
    public static readonly ValidationErrorCode StockpileNotFound = new("military.stockpileNotFound");
    public static readonly ValidationErrorCode InsufficientEquipment = new("military.insufficientEquipment");
    public static readonly ValidationErrorCode CommanderInvalid = new("military.commanderInvalid");
    public static readonly ValidationErrorCode DeploymentInvalid = new("military.deploymentInvalid");
    public static readonly ValidationErrorCode AftermathInvalid = new("military.aftermathInvalid");
    public static readonly ValidationErrorCode CaptiveIntakeMissing = new("military.captiveIntakeMissing");
    public static readonly ValidationErrorCode InvalidInfrastructure = new("military.invalidInfrastructure");

    public static readonly CommandPipeline<WorldState, EstablishEstateForceCommand> EstablishForce = new(
        ValidateEstablish, MutateEstablish, static state => state.IssueCommandSequenceNumber());
    public static readonly CommandPipeline<WorldState, RaiseSquadCommand> RaiseSquad = new(
        ValidateRaise, MutateRaise, static state => state.IssueCommandSequenceNumber());
    public static readonly CommandPipeline<WorldState, CommitSquadEquipmentCommand> CommitEquipment = new(
        ValidateEquipment, MutateEquipment, static state => state.IssueCommandSequenceNumber());
    public static readonly CommandPipeline<WorldState, AssignForceCommanderCommand> AssignCommander = new(
        ValidateCommander, MutateCommander, static state => state.IssueCommandSequenceNumber());
    public static readonly CommandPipeline<WorldState, BeginMilitaryDeploymentCommand> BeginDeployment = new(
        ValidateDeployment, MutateDeployment, static state => state.IssueCommandSequenceNumber());
    public static readonly CommandPipeline<WorldState, DemobilizeSquadCommand> DemobilizeSquad = new(
        ValidateDemobilize, MutateDemobilize, static state => state.IssueCommandSequenceNumber());
    public static readonly CommandPipeline<WorldState, ApplyMilitaryAftermathCommand> ApplyAftermath = new(
        ValidateAftermath, MutateAftermath, static state => state.IssueCommandSequenceNumber());

    private static bool ValidSponsor(WorldState state, RuntimeId<Character> id, RuntimeId<Household> household,
        RuntimeId<Settlement> settlement) => state.Characters.TryGet(id, out var character) && character!.IsAlive
        && character.Household == household && character.Location == settlement;

    private static ValidationErrorCode? ValidateEstablish(WorldState state, EstablishEstateForceCommand command)
    {
        if (!ValidSponsor(state, command.SponsorId, command.HouseholdId, command.SettlementId)) return SponsorInvalid;
        if (!state.Settlements.TryGet(command.SettlementId, out _)) return ForceNotFound;
        if (state.EstateForces.TryGet(command.SettlementId, out _)) return ForceAlreadyExists;
        if (!Enum.IsDefined(typeof(ForceInfrastructureTier), command.InfrastructureTier)) return InvalidInfrastructure;
        return null;
    }

    private static IDomainEvent[] MutateEstablish(WorldState state, EstablishEstateForceCommand command)
    {
        state.EstateForces.Add(command.SettlementId, new EstateForce(command.SettlementId, command.HouseholdId,
            command.InfrastructureTier, null, command.SubmittedDate));
        return Event(state, command.SubmittedDate, "forceEstablished", command.CausationId, command.SettlementId.ToTaggedString());
    }

    private static ValidationErrorCode? ValidateRaise(WorldState state, RaiseSquadCommand command)
    {
        if (!state.EstateForces.TryGet(command.ForceSettlementId, out var force)) return ForceNotFound;
        if (!ValidSponsor(state, command.SponsorId, force!.HouseholdId, command.ForceSettlementId)) return SponsorInvalid;
        if (string.IsNullOrWhiteSpace(command.Name) || command.Manpower < MilitaryCatalog.MinSquadManpower || command.Manpower > MilitaryCatalog.MaxSquadManpower) return InvalidManpower;
        if (!Enum.IsDefined(typeof(SquadType), command.Type) || !Enum.IsDefined(typeof(SquadRecruitmentSource), command.RecruitmentSource) ||
            command.RecruitmentSource == SquadRecruitmentSource.EnslavedMilitia && command.Type != SquadType.Militia)
            return InvalidRecruitmentSource;
        if (state.Squads.InAscendingOrder().Count(e => e.Value.ForceSettlementId == command.ForceSettlementId && e.Value.Status is SquadStatus.Ready or SquadStatus.Deployed) >= force.SquadCap) return SquadCapReached;

        var expected = command.RecruitmentSource switch
        {
            SquadRecruitmentSource.MusteredVeterans => PopGroupType.Veterans,
            SquadRecruitmentSource.EnslavedMilitia => PopGroupType.NonHouseholdEnslaved,
            SquadRecruitmentSource.Citizens when command.SourcePopGroup is PopGroupType.Coloni or PopGroupType.Operarii => command.SourcePopGroup,
            SquadRecruitmentSource.Mercenaries => null,
            _ => (PopGroupType?)null,
        };
        if (command.RecruitmentSource != SquadRecruitmentSource.Mercenaries && expected is null || command.SourcePopGroup != expected)
            return InvalidRecruitmentSource;
        if (expected is { } groupType && (!state.PopGroups.TryGet(new PopGroupKey(command.ForceSettlementId, groupType), out var group) || group!.Size < command.Manpower))
            return InsufficientPopulation;
        if (command.RecruitmentSource == SquadRecruitmentSource.Mercenaries && Balance(state, force.HouseholdId) < Money.FromDenarii(command.Manpower * MilitaryCatalog.MercenaryHireDenariiPerSoldier))
            return InsufficientFunds;
        return null;
    }

    private static IDomainEvent[] MutateRaise(WorldState state, RaiseSquadCommand command)
    {
        state.EstateForces.TryGet(command.ForceSettlementId, out var force);
        var events = new List<IDomainEvent>();
        if (command.SourcePopGroup is { } groupType)
        {
            var key = new PopGroupKey(command.ForceSettlementId, groupType);
            state.PopGroups.TryGet(key, out var group);
            state.PopGroups.Remove(key);
            state.PopGroups.Add(key, group! with { Size = group.Size - command.Manpower });
        }
        else
        {
            var cost = Money.FromDenarii(command.Manpower * MilitaryCatalog.MercenaryHireDenariiPerSoldier);
            events.Add(LedgerService.Post(state, command.SubmittedDate, LedgerTransactionCategory.Wages,
                new[] { new LedgerPosting(LedgerAccountKey.ForHousehold(force!.HouseholdId), -cost), new LedgerPosting(LedgerAccountKey.Mint, cost) },
                $"military.hire:{command.CommandId.ToTaggedString()}"));
        }

        var readiness = command.RecruitmentSource switch
        {
            SquadRecruitmentSource.MusteredVeterans => MilitaryCatalog.VeteranReadiness,
            SquadRecruitmentSource.Mercenaries => MilitaryCatalog.MercenaryReadiness,
            _ => MilitaryCatalog.FreshReadiness,
        };
        var morale = command.RecruitmentSource == SquadRecruitmentSource.Mercenaries ? MilitaryCatalog.MercenaryStartingMorale : MilitaryCatalog.StartingMorale;
        var id = state.SquadIds.Issue();
        state.Squads.Add(id, Squad.Create(id, command.ForceSettlementId, command.Name.Trim(), command.Type,
            command.RecruitmentSource, command.SourcePopGroup, command.Manpower, readiness, morale));
        events.AddRange(Event(state, command.SubmittedDate, "squadRaised", command.CausationId, id.ToTaggedString(), command.ForceSettlementId.ToTaggedString()));
        return events.ToArray();
    }

    private static ValidationErrorCode? ValidateEquipment(WorldState state, CommitSquadEquipmentCommand command)
    {
        if (!state.Squads.TryGet(command.SquadId, out var squad)) return SquadNotFound;
        if (squad!.Status != SquadStatus.Ready || command.Quantity <= 0) return SquadUnavailable;
        if (!state.EstateForces.TryGet(squad.ForceSettlementId, out var force) || !ValidSponsor(state, command.SponsorId, force!.HouseholdId, squad.ForceSettlementId)) return SponsorInvalid;
        if (!state.Stockpiles.TryGet(command.StockpileHoldingId, out var stockpile)) return StockpileNotFound;
        if (!state.Holdings.TryGet(command.StockpileHoldingId, out var holding) ||
            holding!.SettlementId != squad.ForceSettlementId ||
            holding.OwnerId != force.HouseholdId.ToTaggedString() && holding.OccupantId != force.HouseholdId.ToTaggedString())
            return StockpileNotFound;
        return stockpile!.AvailableQuantityOf(command.GoodId, command.Quality) < command.Quantity ? InsufficientEquipment : null;
    }

    private static IDomainEvent[] MutateEquipment(WorldState state, CommitSquadEquipmentCommand command)
    {
        state.Squads.TryGet(command.SquadId, out var squad);
        state.Stockpiles.TryGet(command.StockpileHoldingId, out var stockpile);
        stockpile!.ConsumeAvailable(command.GoodId, command.Quantity, command.Quality);
        var lots = squad!.Equipment.ToList();
        var index = lots.FindIndex(e => e.Kind == command.Kind && e.GoodId == command.GoodId && e.Quality == command.Quality);
        if (index >= 0) lots[index] = lots[index] with { Committed = lots[index].Committed + command.Quantity };
        else lots.Add(new SquadEquipmentLot(command.Kind, command.GoodId, command.Quality, command.Quantity, 0));
        SetSquad(state, squad with { Equipment = lots.OrderBy(e => e.Kind).ThenBy(e => e.GoodId).ThenBy(e => e.Quality).ToArray() });
        return Event(state, command.SubmittedDate, "equipmentCommitted", command.CausationId, command.SquadId.ToTaggedString());
    }

    private static ValidationErrorCode? ValidateCommander(WorldState state, AssignForceCommanderCommand command)
    {
        if (!state.EstateForces.TryGet(command.ForceSettlementId, out var force)) return ForceNotFound;
        if (!ValidSponsor(state, command.SponsorId, force!.HouseholdId, command.ForceSettlementId)) return SponsorInvalid;
        if (!state.Characters.TryGet(command.CommanderId, out var commander) || !commander!.IsAlive || commander.Household != force!.HouseholdId) return CommanderInvalid;
        if (command.SquadId is { } squadId && (!state.Squads.TryGet(squadId, out var squad) || squad!.ForceSettlementId != command.ForceSettlementId)) return SquadNotFound;
        return null;
    }

    private static IDomainEvent[] MutateCommander(WorldState state, AssignForceCommanderCommand command)
    {
        if (command.SquadId is { } squadId)
        {
            state.Squads.TryGet(squadId, out var squad);
            SetSquad(state, squad! with { CommanderId = command.CommanderId });
        }
        else
        {
            state.EstateForces.TryGet(command.ForceSettlementId, out var force);
            state.EstateForces.Remove(command.ForceSettlementId);
            state.EstateForces.Add(command.ForceSettlementId, force! with { PraefectusId = command.CommanderId });
        }
        return Event(state, command.SubmittedDate, "commanderAssigned", command.CausationId, command.CommanderId.ToTaggedString());
    }

    private static ValidationErrorCode? ValidateDeployment(WorldState state, BeginMilitaryDeploymentCommand command)
    {
        if (!state.EstateForces.TryGet(command.ForceSettlementId, out var force)) return ForceNotFound;
        if (!ValidSponsor(state, command.SponsorId, force!.HouseholdId, command.ForceSettlementId)) return SponsorInvalid;
        if (!Enum.IsDefined(typeof(MilitaryDeploymentType), command.Type) || command.SquadIds is null ||
            command.SquadIds.Count == 0 || command.SquadIds.Distinct().Count() != command.SquadIds.Count)
            return DeploymentInvalid;
        foreach (var id in command.SquadIds)
            if (!state.Squads.TryGet(id, out var squad) || squad!.ForceSettlementId != command.ForceSettlementId ||
                squad.Status != SquadStatus.Ready || squad.Manpower <= 0 ||
                squad.RecruitmentSource == SquadRecruitmentSource.EnslavedMilitia && command.Type is not (MilitaryDeploymentType.Defense or MilitaryDeploymentType.Suppression))
                return SquadUnavailable;
        return null;
    }

    private static IDomainEvent[] MutateDeployment(WorldState state, BeginMilitaryDeploymentCommand command)
    {
        var id = state.MilitaryDeploymentIds.Issue();
        state.MilitaryDeployments.Add(id, MilitaryDeployment.Begin(id, command.ForceSettlementId, command.Type, command.Destination, command.SquadIds, command.SubmittedDate));
        foreach (var squadId in command.SquadIds)
        {
            state.Squads.TryGet(squadId, out var squad);
            SetSquad(state, squad! with { Status = SquadStatus.Deployed, Location = command.Destination });
        }
        return Event(state, command.SubmittedDate, "deploymentBegan", command.CausationId, id.ToTaggedString());
    }

    private static ValidationErrorCode? ValidateDemobilize(WorldState state, DemobilizeSquadCommand command)
    {
        if (!state.Squads.TryGet(command.SquadId, out var squad)) return SquadNotFound;
        if (squad!.Status != SquadStatus.Ready || squad.Manpower <= 0) return SquadUnavailable;
        if (!state.EstateForces.TryGet(squad.ForceSettlementId, out var force)) return ForceNotFound;
        return ValidSponsor(state, command.SponsorId, force!.HouseholdId, squad.ForceSettlementId) ? null : SponsorInvalid;
    }

    private static IDomainEvent[] MutateDemobilize(WorldState state, DemobilizeSquadCommand command)
    {
        state.Squads.TryGet(command.SquadId, out var squad);
        if (squad!.SourcePopGroup is { } source)
            AddPopulation(state, squad.ForceSettlementId, source, squad.Manpower);
        SetSquad(state, squad with
        {
            Manpower = 0,
            Status = SquadStatus.Demobilized,
            CommanderId = null,
            Location = TravelLocation.Home(squad.ForceSettlementId),
        });
        return Event(state, command.SubmittedDate, "squadDemobilized", command.CausationId, command.SquadId.ToTaggedString());
    }

    private static ValidationErrorCode? ValidateAftermath(WorldState state, ApplyMilitaryAftermathCommand command)
    {
        if (!state.MilitaryDeployments.TryGet(command.DeploymentId, out var deployment) || deployment!.Status != MilitaryDeploymentStatus.Active || command.Losses is null || command.CapturedCharacters is null || string.IsNullOrWhiteSpace(command.AftermathSummary)) return AftermathInvalid;
        if (!Enum.IsDefined(typeof(MilitaryOutcome), command.Outcome)) return AftermathInvalid;
        if (command.Losses.Select(l => l.SquadId).Distinct().Count() != command.Losses.Count) return AftermathInvalid;
        foreach (var loss in command.Losses)
        {
            if (!deployment.SquadIds.Contains(loss.SquadId) || !state.Squads.TryGet(loss.SquadId, out var squad) ||
                loss.Casualties < 0 || loss.Desertions < 0 || loss.Casualties + loss.Desertions > squad!.Manpower ||
                loss.ReadinessLoss < 0 || loss.ReadinessLoss > squad.Readiness || loss.MoraleLoss < 0 || loss.MoraleLoss > squad.Morale)
                return AftermathInvalid;
            foreach (var equipmentLosses in (loss.EquipmentLosses ?? Array.Empty<EquipmentLoss>()).GroupBy(value => value.Kind))
                if (equipmentLosses.Any(value => value.Quantity < 0) ||
                    equipmentLosses.Sum(value => value.Quantity) > squad.Equipment.Where(e => e.Kind == equipmentLosses.Key).Sum(e => e.Available))
                    return AftermathInvalid;
        }
        if (command.CaptivesTaken < 0 || command.CaptivesTaken > 0 &&
            (command.CaptiveSourceSettlementId is null || command.CaptiveSourcePopGroup is null || command.CaptiveIntakeSettlementId is null))
            return CaptiveIntakeMissing;
        if (command.CaptivesTaken > 0 &&
            (!state.PopGroups.TryGet(new PopGroupKey(command.CaptiveSourceSettlementId!.Value, command.CaptiveSourcePopGroup!.Value), out var sourceGroup) || sourceGroup!.Size < command.CaptivesTaken))
            return CaptiveIntakeMissing;
        if (command.CaptivesTaken > 0 && !state.PopGroups.TryGet(new PopGroupKey(command.CaptiveIntakeSettlementId!.Value, PopGroupType.NonHouseholdEnslaved), out _)) return CaptiveIntakeMissing;
        if (command.CapturedCharacters.Count > 0 && command.CaptorHouseholdId is null) return CaptiveIntakeMissing;
        if (command.CapturedCharacters.Distinct().Count() != command.CapturedCharacters.Count) return AftermathInvalid;
        if (command.CaptorHouseholdId is { } captor && (!state.EstateForces.TryGet(deployment.ForceSettlementId, out var force) || force!.HouseholdId != captor)) return AftermathInvalid;
        foreach (var characterId in command.CapturedCharacters)
            if (!state.Characters.TryGet(characterId, out var character) || !character!.IsAlive ||
                DetentionResolver.ActiveFor(state, characterId) is not null)
                return AftermathInvalid;
        return null;
    }

    private static IDomainEvent[] MutateAftermath(WorldState state, ApplyMilitaryAftermathCommand command)
    {
        state.MilitaryDeployments.TryGet(command.DeploymentId, out var deployment);
        var losses = command.Losses.ToDictionary(l => l.SquadId);
        foreach (var squadId in deployment!.SquadIds)
        {
            state.Squads.TryGet(squadId, out var squad);
            losses.TryGetValue(squadId, out var loss);
            var manpower = squad!.Manpower - loss.Casualties - loss.Desertions;
            var lots = ApplyEquipmentLosses(squad.Equipment, loss.EquipmentLosses ?? Array.Empty<EquipmentLoss>());
            if (loss.Desertions > 0 && squad.SourcePopGroup is { } source)
                AddPopulation(state, squad.ForceSettlementId, source, loss.Desertions);
            SetSquad(state, squad with
            {
                Manpower = manpower,
                Readiness = Math.Max(0, squad.Readiness - loss.ReadinessLoss),
                Morale = Math.Max(0, squad.Morale - loss.MoraleLoss),
                Equipment = lots,
                Status = manpower == 0 ? SquadStatus.Destroyed : SquadStatus.Ready,
                Location = TravelLocation.Home(squad.ForceSettlementId),
            });
        }

        if (command.CaptivesTaken > 0)
        {
            var sourceKey = new PopGroupKey(command.CaptiveSourceSettlementId!.Value, command.CaptiveSourcePopGroup!.Value);
            state.PopGroups.TryGet(sourceKey, out var sourceGroup);
            state.PopGroups.Remove(sourceKey);
            state.PopGroups.Add(sourceKey, sourceGroup! with { Size = sourceGroup.Size - command.CaptivesTaken });
            AddPopulation(state, command.CaptiveIntakeSettlementId!.Value, PopGroupType.NonHouseholdEnslaved, command.CaptivesTaken);
        }
        var captorSettlement = command.CaptiveIntakeSettlementId ?? deployment.ForceSettlementId;
        foreach (var characterId in command.CapturedCharacters.OrderBy(value => value))
        {
            state.Characters.TryGet(characterId, out var character);
            state.Characters.Remove(characterId);
            state.Characters.Add(characterId, character! with { Location = captorSettlement });
            state.MilitaryCaptivities.Remove(characterId);
            state.MilitaryCaptivities.Add(characterId, new MilitaryCaptivity(characterId, command.DeploymentId,
                command.CaptorHouseholdId!.Value, captorSettlement, command.SubmittedDate));
            var detentionId = state.DetentionRecordIds.Issue();
            state.DetentionRecords.Add(detentionId, new DetentionRecord(detentionId, characterId,
                DetentionLocationType.PrivateErgastulum, command.SubmittedDate, Justified: true));
        }

        state.MilitaryDeployments.Remove(command.DeploymentId);
        state.MilitaryDeployments.Add(command.DeploymentId, deployment with
        {
            Status = MilitaryDeploymentStatus.Resolved,
            Outcome = command.Outcome,
            ResolvedDate = command.SubmittedDate,
            Losses = command.Losses.OrderBy(l => l.SquadId)
                .Select(loss => loss with { EquipmentLosses = loss.EquipmentLosses ?? Array.Empty<EquipmentLoss>() }).ToArray(),
            CaptivesTaken = command.CaptivesTaken,
            CaptiveSourceSettlementId = command.CaptiveSourceSettlementId,
            CaptiveSourcePopGroup = command.CaptiveSourcePopGroup,
            CapturedCharacters = command.CapturedCharacters.OrderBy(value => value).ToArray(),
            AftermathSummary = command.AftermathSummary.Trim(),
        });
        return Event(state, command.SubmittedDate, "aftermathApplied", command.CausationId, command.DeploymentId.ToTaggedString());
    }

    private static SquadEquipmentLot[] ApplyEquipmentLosses(IReadOnlyList<SquadEquipmentLot> lots, IReadOnlyList<EquipmentLoss> losses)
    {
        var result = lots.ToArray();
        foreach (var loss in losses.OrderBy(l => l.Kind))
        {
            var remaining = loss.Quantity;
            for (var i = 0; i < result.Length && remaining > 0; i++)
            {
                if (result[i].Kind != loss.Kind) continue;
                var taken = Math.Min(result[i].Available, remaining);
                result[i] = result[i] with { Lost = result[i].Lost + taken };
                remaining -= taken;
            }
        }
        return result;
    }

    internal static void AddPopulation(WorldState state, RuntimeId<Settlement> settlement, PopGroupType type, int count)
    {
        var key = new PopGroupKey(settlement, type);
        state.PopGroups.TryGet(key, out var group);
        state.PopGroups.Remove(key);
        state.PopGroups.Add(key, group! with { Size = checked(group.Size + count) });
    }

    internal static void SetSquad(WorldState state, Squad squad)
    {
        state.Squads.Remove(squad.Id);
        state.Squads.Add(squad.Id, squad);
    }

    private static Money Balance(WorldState state, RuntimeId<Household> household) => state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(household), out var account) ? account!.Balance : Money.Zero;
    private static IDomainEvent[] Event(WorldState state, GameDate date, string change, string? causationId, params string[] subjects) =>
        new IDomainEvent[] { new MilitaryStateChangedEvent(state.EventIds.Issue(), date, change, subjects, causationId) };
}
