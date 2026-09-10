#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Gens.Simulation.Characters;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Time;
using Gens.Simulation.Travel;

namespace Gens.Simulation.Military;

public enum ForceInfrastructureTier { Barracks, Garrison, Fortress }
public enum SquadType { Infantry, Cavalry, Siege, Militia }
public enum SquadRecruitmentSource { Citizens, MusteredVeterans, EnslavedMilitia, Mercenaries }
public enum SquadStatus { Ready, Deployed, Destroyed, Demobilized }
public enum EquipmentKind { Weapons, Armor, Horses, SiegeEngines }
public enum MilitaryDeploymentType { Defense, Suppression, OffenseCampaign, PrivateFeud, Naval, PiracySuppression }
public enum MilitaryDeploymentStatus { Active, Resolved }
public enum MilitaryOutcome { DecisiveVictory, CostlyVictory, RepulsedStalemate, Defeat, CatastrophicDefeat, NegotiatedSurrender, Sack }

public readonly record struct SquadEquipmentLot(
    EquipmentKind Kind,
    DefinitionId<Good> GoodId,
    GoodQuality? Quality,
    long Committed,
    long Lost)
{
    public long Available => Committed - Lost;
}

/// <summary>A household's persistent private force at one settlement (Phase 16 item 3).</summary>
public sealed record EstateForce(
    RuntimeId<Settlement> SettlementId,
    RuntimeId<Household> HouseholdId,
    ForceInfrastructureTier InfrastructureTier,
    RuntimeId<Character>? PraefectusId,
    GameDate EstablishedDate)
{
    public int SquadCap => MilitaryCatalog.SquadCap(InfrastructureTier);
}

/// <summary>A named, persistent unit. Manpower was removed from <see cref="WorldState.PopGroups"/>
/// when raised; equipment was removed from a real stockpile when committed.</summary>
public sealed record Squad
{
    private Squad() { }

    public required RuntimeId<Squad> Id { get; init; }
    public required RuntimeId<Settlement> ForceSettlementId { get; init; }
    public required string Name { get; init; }
    public required SquadType Type { get; init; }
    public required SquadRecruitmentSource RecruitmentSource { get; init; }
    public PopGroupType? SourcePopGroup { get; init; }
    public required int Manpower { get; init; }
    public required int InitialManpower { get; init; }
    public required int Readiness { get; init; }
    public required int Morale { get; init; }
    public required SquadStatus Status { get; init; }
    public RuntimeId<Character>? CommanderId { get; init; }
    public required TravelLocation Location { get; init; }
    public required IReadOnlyList<SquadEquipmentLot> Equipment { get; init; }

    public static Squad Create(
        RuntimeId<Squad> id, RuntimeId<Settlement> forceSettlementId, string name, SquadType type,
        SquadRecruitmentSource recruitmentSource, PopGroupType? sourcePopGroup, int manpower,
        int readiness, int morale) => new()
        {
            Id = id,
            ForceSettlementId = forceSettlementId,
            Name = name,
            Type = type,
            RecruitmentSource = recruitmentSource,
            SourcePopGroup = sourcePopGroup,
            Manpower = manpower,
            InitialManpower = manpower,
            Readiness = readiness,
            Morale = morale,
            Status = SquadStatus.Ready,
            CommanderId = null,
            Location = TravelLocation.Home(forceSettlementId),
            Equipment = Array.Empty<SquadEquipmentLot>(),
        };

    public static Squad Restore(RuntimeId<Squad> id, RuntimeId<Settlement> forceSettlementId, string name,
        SquadType type, SquadRecruitmentSource recruitmentSource, PopGroupType? sourcePopGroup,
        int manpower, int initialManpower, int readiness, int morale, SquadStatus status,
        RuntimeId<Character>? commanderId, TravelLocation location, IReadOnlyList<SquadEquipmentLot> equipment) => new()
        {
            Id = id,
            ForceSettlementId = forceSettlementId,
            Name = name,
            Type = type,
            RecruitmentSource = recruitmentSource,
            SourcePopGroup = sourcePopGroup,
            Manpower = manpower,
            InitialManpower = initialManpower,
            Readiness = readiness,
            Morale = morale,
            Status = status,
            CommanderId = commanderId,
            Location = location,
            Equipment = equipment,
        };

    public int EquipmentTier
    {
        get
        {
            if (Manpower <= 0)
                return 0;
            var weapons = Equipment.Where(e => e.Kind == EquipmentKind.Weapons).Sum(e => e.Available);
            var armor = Equipment.Where(e => e.Kind == EquipmentKind.Armor).Sum(e => e.Available);
            var requiredSpecial = Type switch
            {
                SquadType.Cavalry => Equipment.Where(e => e.Kind == EquipmentKind.Horses).Sum(e => e.Available),
                SquadType.Siege => Equipment.Where(e => e.Kind == EquipmentKind.SiegeEngines).Sum(e => e.Available) * MilitaryCatalog.ManpowerPerSiegeEngine,
                _ => Manpower,
            };
            var coverage = Math.Min(weapons, Math.Min(armor, requiredSpecial));
            return coverage >= Manpower ? 3 : coverage * 2 >= Manpower ? 2 : coverage > 0 ? 1 : 0;
        }
    }
}

public readonly record struct SquadLoss(RuntimeId<Squad> SquadId, int Casualties, int Desertions,
    int ReadinessLoss, int MoraleLoss, IReadOnlyList<EquipmentLoss> EquipmentLosses);
public readonly record struct EquipmentLoss(EquipmentKind Kind, long Quantity);

/// <summary>A deployment remains authoritative until a caller (the shared combat kernel in item 4)
/// submits a validated aftermath.</summary>
public sealed record MilitaryDeployment
{
    private MilitaryDeployment() { }
    public required RuntimeId<MilitaryDeployment> Id { get; init; }
    public required RuntimeId<Settlement> ForceSettlementId { get; init; }
    public required MilitaryDeploymentType Type { get; init; }
    public required TravelLocation Destination { get; init; }
    public required IReadOnlyList<RuntimeId<Squad>> SquadIds { get; init; }
    public required GameDate BeganDate { get; init; }
    public required MilitaryDeploymentStatus Status { get; init; }
    public MilitaryOutcome? Outcome { get; init; }
    public GameDate? ResolvedDate { get; init; }
    public IReadOnlyList<SquadLoss> Losses { get; init; } = Array.Empty<SquadLoss>();
    public int CaptivesTaken { get; init; }
    public RuntimeId<Settlement>? CaptiveSourceSettlementId { get; init; }
    public PopGroupType? CaptiveSourcePopGroup { get; init; }
    public IReadOnlyList<RuntimeId<Character>> CapturedCharacters { get; init; } = Array.Empty<RuntimeId<Character>>();
    public string? AftermathSummary { get; init; }

    public static MilitaryDeployment Begin(RuntimeId<MilitaryDeployment> id, RuntimeId<Settlement> forceSettlementId,
        MilitaryDeploymentType type, TravelLocation destination, IReadOnlyList<RuntimeId<Squad>> squadIds,
        GameDate beganDate) => new()
        {
            Id = id,
            ForceSettlementId = forceSettlementId,
            Type = type,
            Destination = destination,
            SquadIds = squadIds.OrderBy(value => value).ToArray(),
            BeganDate = beganDate,
            Status = MilitaryDeploymentStatus.Active,
        };
}

/// <summary>The latest named military-capture provenance for a Character. Current captive status is
/// owned by Crime's ordinary DetentionRecord; this record explains which deployment put them there.</summary>
public sealed record MilitaryCaptivity(
    RuntimeId<Character> CharacterId,
    RuntimeId<MilitaryDeployment> DeploymentId,
    RuntimeId<Household> CaptorHouseholdId,
    RuntimeId<Settlement> CaptorSettlementId,
    GameDate CapturedDate);

public static class MilitaryCatalog
{
    public const int MinSquadManpower = 5;
    public const int MaxSquadManpower = 200;
    public const int FreshReadiness = 25;
    public const int VeteranReadiness = 70;
    public const int MercenaryReadiness = 90;
    public const int StartingMorale = 60;
    public const int MercenaryStartingMorale = 70;
    public const int MonthlyReadinessRecovery = 8;
    public const int MonthlyMoraleRecovery = 3;
    public const int MaxCondition = 100;
    public const int ManpowerPerSiegeEngine = 20;
    public const int RegularWageDenariiPerSoldier = 2;
    public const int MercenaryWageDenariiPerSoldier = 6;
    public const int MercenaryHireDenariiPerSoldier = 20;

    public static int SquadCap(ForceInfrastructureTier tier) => tier switch
    {
        ForceInfrastructureTier.Barracks => 2,
        ForceInfrastructureTier.Garrison => 4,
        ForceInfrastructureTier.Fortress => 6,
        _ => throw new ArgumentOutOfRangeException(nameof(tier)),
    };
}
