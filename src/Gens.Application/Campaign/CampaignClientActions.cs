#nullable enable

using System;
using System.Collections.Generic;
using Gens.Simulation.Actions;
using Gens.Simulation.Characters;
using Gens.Simulation.Campaign;
using Gens.Simulation.Commands;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Policies;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Application.Campaign;

/// <summary>Client-neutral options for the supported playable campaign bootstrap.</summary>
public sealed record CampaignStartOptions(
    ulong Seed,
    string RegionId,
    string Difficulty,
    string RulesetId = "standard",
    string ContentPackHash = "development",
    string? StartProfileId = null,
    int StartMonths = 300)
{
    public CampaignConfig ToConfig() => new()
    {
        Seed = Seed,
        StartDate = new GameDate(StartMonths),
        RulesetId = RulesetId,
        ContentPackHash = ContentPackHash,
        RegionId = RegionId,
        StartProfileId = StartProfileId,
        Difficulty = Difficulty,
        ContentToggles = Array.Empty<string>(),
    };
}

/// <summary>The two household actions exposed by the current vertical slice.</summary>
public enum CampaignHouseholdAction { CycleRitesBudget, FundFestival }

/// <summary>Non-mutating command preview used by either client to render a confirmation.</summary>
public sealed record CampaignActionPreview(
    CampaignHouseholdAction Action,
    string NameKey,
    string Summary,
    bool RequiresWaxSeal,
    bool IsAvailable,
    string? ErrorCode);

public static class CampaignClientActions
{
    private static readonly ActionCatalog Catalog = PolicyActionDefinitions.BuildCatalog();
    private static readonly IReadOnlyList<IMonthlySystem<WorldState>> MonthlySystems =
        new IMonthlySystem<WorldState>[] { new ScheduledActionSystem() };

    public static CampaignSession CreateNew(CampaignStartOptions options, out IReadOnlyList<IDomainEvent> initialHistory)
    {
        if (options is null) throw new ArgumentNullException(nameof(options));
        CampaignSession session = CampaignSession.CreateNew(options.ToConfig(), out initialHistory);
        SeedPlayableVerticalSlice(session);
        return session;
    }

    internal static IReadOnlyList<IDomainEvent> AdvancePlayableMonth(this CampaignSession session) =>
        session.AdvanceMonth(MonthlySystems);

    internal static CampaignActionPreview Preview(this CampaignSession session, CampaignHouseholdAction action)
    {
        ActionDefinition definition = Definition(action);
        var invocation = new ActionInvocation(session.HouseholdId.ToTaggedString(), null, session.State.Date);
        ValidationErrorCode? error = definition.Eligibility(session.State, invocation);
        ActionResultProjection result = definition.ProjectResult(session.State, invocation);
        return new(action, definition.NameKey, result.Summary,
            definition.Confirmation == ActionConfirmationSeverity.WaxSeal, error is null, error?.Code);
    }

    internal static CommandResult Submit(this CampaignSession session, CampaignHouseholdAction action) => action switch
    {
        CampaignHouseholdAction.CycleRitesBudget => SubmitRitesBudget(session),
        CampaignHouseholdAction.FundFestival => SubmitFestival(session),
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
    };

    private static CommandResult SubmitRitesBudget(CampaignSession session)
    {
        RitesBudgetTier current = HouseholdPolicyResolver.GetEffectiveRitesBudget(session.State, session.HouseholdId);
        var command = new ChangeRitesBudgetCommand(
            session.State.CommandIds.Issue(), session.HouseholdId.ToTaggedString(), session.State.Date, null,
            session.HouseholdId, Next(current));
        return session.Submit(ChangeRitesBudgetCommands.Pipeline, command);
    }

    private static CommandResult SubmitFestival(CampaignSession session)
    {
        var command = new FundFestivalCommand(
            session.State.CommandIds.Issue(), session.HouseholdId.ToTaggedString(), session.State.Date, null,
            session.HouseholdId, session.SettlementId, PolicyActionDefinitions.DefaultFestivalAmount);
        return session.Submit(FundFestivalCommands.Pipeline, command);
    }

    private static RitesBudgetTier Next(RitesBudgetTier tier) => tier switch
    {
        RitesBudgetTier.Frugal => RitesBudgetTier.Standard,
        RitesBudgetTier.Standard => RitesBudgetTier.Lavish,
        RitesBudgetTier.Lavish => RitesBudgetTier.Frugal,
        _ => RitesBudgetTier.Standard,
    };

    private static ActionDefinition Definition(CampaignHouseholdAction action)
    {
        var id = action == CampaignHouseholdAction.CycleRitesBudget
            ? PolicyActionDefinitions.ChangeRitesBudget
            : PolicyActionDefinitions.FundFestival;
        return Catalog.TryGet(id, out ActionDefinition? definition)
            ? definition
            : throw new InvalidOperationException($"Policy action '{id}' is not registered.");
    }

    /// <summary>
    /// Materializes the small production start that the old Unity presentation fixtures supplied.
    /// This is campaign bootstrap, not a player mutation: all actions after creation still use commands.
    /// </summary>
    private static void SeedPlayableVerticalSlice(CampaignSession session)
    {
        WorldState state = session.State;
        RuntimeId<Region> regionId = RuntimeId<Region>.Parse("region_0000000");
        state.Settlements.Add(session.SettlementId, Settlement.Create(session.SettlementId, regionId));
        string household = session.HouseholdId.ToTaggedString();
        RuntimeId<Holding> holdingId = state.HoldingIds.Issue(); RuntimeId<Plot> plotId = state.PlotIds.Issue();
        state.Holdings.Add(holdingId, Holding.Create(holdingId, session.SettlementId, household, household, residentCapacity: 12));
        state.Plots.Add(plotId, Plot.Create(plotId, session.SettlementId, ownerId: household, occupyingHoldingId: holdingId, capacity: 4));
        state.Stockpiles.Add(holdingId, new Stockpile(1000));
        state.LedgerAccounts.Add(LedgerAccountKey.ForHousehold(session.HouseholdId), new(LedgerAccountKey.ForHousehold(session.HouseholdId), Money.FromDenarii(500)));
        AddCharacter(session, "Marcus", "Aurelius", Sex.Male, 0, new CoreAttributes(10, 12, 14, 8, 11));
        AddCharacter(session, "Livia", "Aurelia", Sex.Female, 36, new CoreAttributes(13, 7, 15, 11, 12));
        AddCharacter(session, "Titus", "Aurelius", Sex.Male, 180, new CoreAttributes(8, 9, 7, 10, 13));
    }

    private static void AddCharacter(CampaignSession session, string praenomen, string nomen, Sex sex, int birthMonth, CoreAttributes attributes)
    {
        RuntimeId<Character> id = session.State.CharacterIds.Issue();
        var profile = new CharacterVisualProfile
        {
            Height = Height.Average,
            Build = Build.Average,
            FacialStructure = FacialStructure.Oval,
            Complexion = Complexion.Olive,
            HairColor = HairColor.Brown,
            HairStyle = HairStyle.Cropped,
            EyeColor = EyeColor.Brown,
            NotableFeatures = Array.Empty<NotableFeature>(),
            Portrait = PortraitRecipeGenerator.Generate(Height.Average, Build.Average, FacialStructure.Oval, Complexion.Olive, HairColor.Brown, HairStyle.Cropped, EyeColor.Brown, Array.Empty<NotableFeature>()),
        };
        session.State.Characters.Add(id, Character.Create(id, praenomen, nomen, null, sex, new GameDate(birthMonth), profile,
            LegalStatus.RomanCitizen, SocialClass.Plebeian, new DefinitionId<Culture>("roman"), session.SettlementId,
            session.HouseholdId, attributes, new LaborSkills(35, 35, 35, 35, 35), new Condition(85, 0, 60, 20, 55),
            CharacterSource.Familia, session.State.Date.TotalMonths));
    }
}
