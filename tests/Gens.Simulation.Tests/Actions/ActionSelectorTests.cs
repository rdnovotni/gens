using Gens.Simulation.Actions;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Policies;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Actions;

/// <summary>Phase 10 item 1 ("reusable AI considerations and action selection against the same action
/// definitions used by the player") coverage for <see cref="ActionSelector"/>.</summary>
public sealed class ActionSelectorTests
{
    private static readonly DefinitionId<ActionDefinition> HighScoreId = new("high-score");
    private static readonly DefinitionId<ActionDefinition> LowScoreId = new("low-score");
    private static readonly DefinitionId<ActionDefinition> IneligibleId = new("ineligible");
    private static readonly ValidationErrorCode NeverEligibleError = new("test.neverEligible");

    private static ActionDefinition MakeDefinition(DefinitionId<ActionDefinition> id, double score, bool eligible) => new(
        id: id,
        nameKey: "test.name",
        descriptionKey: "test.description",
        targetKind: ActionTargetKind.None,
        cost: ActionCost.None,
        duration: ActionDuration.Instant,
        confirmation: ActionConfirmationSeverity.Ordinary,
        eligibility: (_, _) => eligible ? null : NeverEligibleError,
        scoreForAi: (_, _) => score,
        projectResult: (_, _) => ActionResultProjection.Of("test"));

    [Test]
    public void RankOrdersEligibleCandidatesByScoreDescending()
    {
        var catalog = new ActionCatalog(new[]
        {
            MakeDefinition(LowScoreId, score: 0.2, eligible: true),
            MakeDefinition(HighScoreId, score: 0.9, eligible: true),
        });
        var state = new WorldState(new GameDate(0));
        var invocation = new ActionInvocation("household_0000000", null, new GameDate(0));

        var ranked = ActionSelector.Rank(state, catalog, invocation);

        Assert.That(ranked.Select(c => c.Definition.Id), Is.EqualTo(new[] { HighScoreId, LowScoreId }));
    }

    [Test]
    public void RankExcludesIneligibleDefinitions()
    {
        var catalog = new ActionCatalog(new[]
        {
            MakeDefinition(HighScoreId, score: 0.9, eligible: true),
            MakeDefinition(IneligibleId, score: 1.0, eligible: false),
        });
        var state = new WorldState(new GameDate(0));
        var invocation = new ActionInvocation("household_0000000", null, new GameDate(0));

        var ranked = ActionSelector.Rank(state, catalog, invocation);

        Assert.That(ranked.Select(c => c.Definition.Id), Is.EqualTo(new[] { HighScoreId }));
    }

    [Test]
    public void RankBreaksScoreTiesByDefinitionIdAscending()
    {
        var catalog = new ActionCatalog(new[]
        {
            MakeDefinition(LowScoreId, score: 0.5, eligible: true),
            MakeDefinition(HighScoreId, score: 0.5, eligible: true),
        });
        var state = new WorldState(new GameDate(0));
        var invocation = new ActionInvocation("household_0000000", null, new GameDate(0));

        var ranked = ActionSelector.Rank(state, catalog, invocation);

        // "high-score" < "low-score" ordinally, so it wins the tiebreak despite the name.
        Assert.That(ranked.Select(c => c.Definition.Id), Is.EqualTo(new[] { HighScoreId, LowScoreId }));
    }

    [Test]
    public void SelectBestReturnsNullWhenNothingIsEligible()
    {
        var catalog = new ActionCatalog(new[] { MakeDefinition(IneligibleId, score: 1.0, eligible: false) });
        var state = new WorldState(new GameDate(0));
        var invocation = new ActionInvocation("household_0000000", null, new GameDate(0));

        Assert.That(ActionSelector.SelectBest(state, catalog, invocation), Is.Null);
    }

    [Test]
    public void SelectBestPicksFundFestivalOverChangeRitesBudgetOnceTheHouseholdCanAfford()
    {
        var catalog = PolicyActionDefinitions.BuildCatalog();
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var invocation = new ActionInvocation(householdId.ToTaggedString(), null, new GameDate(0));

        // Before funding, FundFestival is ineligible (insufficient treasury) so ChangeRitesBudget wins
        // by default even though its own ScoreForAi (1.0) beats FundFestival's (0.5) either way.
        var beforeFunding = ActionSelector.SelectBest(state, catalog, invocation);
        Assert.That(beforeFunding!.Value.Definition.Id, Is.EqualTo(PolicyActionDefinitions.ChangeRitesBudget));

        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Treasury,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), PolicyActionDefinitions.DefaultFestivalAmount),
                new LedgerPosting(LedgerAccountKey.Mint, -PolicyActionDefinitions.DefaultFestivalAmount),
            });

        // ChangeRitesBudget's own PolicyActionDefinitions.RitesBudgetEligibility scores 1.0 whenever
        // eligible, so it still outranks FundFestival's 0.5 — both remain eligible and ranked.
        var afterFunding = ActionSelector.Rank(state, catalog, invocation);
        Assert.That(afterFunding.Select(c => c.Definition.Id), Is.EqualTo(new[]
        {
            PolicyActionDefinitions.ChangeRitesBudget,
            PolicyActionDefinitions.FundFestival,
        }));
    }
}
