using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Actors;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Time;

namespace Gens.Simulation.Diplomacy;

/// <summary>The three treaty shapes this slice implements (<c>gens-diplomacy-non-roman-peoples-design.md</c>
/// §6). Marriage Alliance, Auxiliary Levy, and Foederati Pact are explicitly deferred — see the build
/// roadmap's Phase 16 item 5 progress note.</summary>
public enum FrontierTreatyType
{
    NonAggression,
    Tribute,
    Trade,
}

/// <summary>Which side pays under a <see cref="FrontierTreatyType.Tribute"/> treaty. <see
/// cref="None"/> for every other treaty type.</summary>
public enum TributeDirection
{
    None,
    HouseholdPaysPeople,
    PeoplePayHousehold,
}

/// <summary>How a <see cref="FrontierTreaty"/> currently stands.</summary>
public enum FrontierTreatyStatus
{
    Active,
    Expired,
    Abrogated,
}

/// <summary>One concluded Frontier treaty between a household and a Foreign People (Phase 16 item 5
/// slice 1; §6, §13's <c>FrontierTreaty</c> data-model sketch). Immutable like every other <c>WorldState</c>
/// record — <see cref="FrontierTreatySystem"/> and <see cref="AbrogateFrontierTreatyCommand"/> both
/// replace the entry rather than mutating one in place.</summary>
public sealed record FrontierTreaty(
    RuntimeId<FrontierTreaty> TreatyId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Actor> ForeignPeopleActorId,
    FrontierTreatyType Type,
    TributeDirection TributeDirection,
    Money MonthlyTribute,
    GameDate ConcludedDate,
    GameDate ExpiresDate,
    FrontierTreatyStatus Status,
    GameDate? EndedDate)
{
    /// <summary>The only supported way to construct a <see cref="FrontierTreaty"/>. Enforces: tribute
    /// direction/amount are set if and only if <paramref name="type"/> is <see
    /// cref="FrontierTreatyType.Tribute"/>; the term must run forward; <paramref name="endedDate"/> is
    /// set if and only if <paramref name="status"/> is terminal.</summary>
    public static FrontierTreaty Create(
        RuntimeId<FrontierTreaty> treatyId,
        RuntimeId<Household> householdId,
        RuntimeId<Actor> foreignPeopleActorId,
        FrontierTreatyType type,
        TributeDirection tributeDirection,
        Money monthlyTribute,
        GameDate concludedDate,
        GameDate expiresDate,
        FrontierTreatyStatus status = FrontierTreatyStatus.Active,
        GameDate? endedDate = null)
    {
        var tributeFieldsSet = tributeDirection != TributeDirection.None || monthlyTribute != Money.Zero;
        if (type == FrontierTreatyType.Tribute && tributeDirection == TributeDirection.None)
            throw new ArgumentException("A Tribute treaty requires a TributeDirection.", nameof(tributeDirection));
        if (type != FrontierTreatyType.Tribute && tributeFieldsSet)
            throw new ArgumentException("Only a Tribute treaty may set tribute terms.", nameof(type));
        if (expiresDate.TotalMonths <= concludedDate.TotalMonths)
            throw new ArgumentException("A treaty's term must run forward from its conclusion date.", nameof(expiresDate));

        var isTerminal = status != FrontierTreatyStatus.Active;
        if (isTerminal != endedDate.HasValue)
        {
            throw new ArgumentException(
                "EndedDate must be set if and only if the treaty's status is terminal.", nameof(endedDate));
        }

        return new FrontierTreaty(
            treatyId, householdId, foreignPeopleActorId, type, tributeDirection, monthlyTribute,
            concludedDate, expiresDate, status, endedDate);
    }
}
