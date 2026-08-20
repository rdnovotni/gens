#nullable enable

using System.Collections.Generic;
using Gens.Presentation.Adapters;
using Gens.Presentation.Tests.Support;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Queries;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Presentation.Tests.EditMode.ExitGate;

/// <summary>
/// Proves the Phase 9 exit gate, verbatim from the roadmap: "a player can complete the loop for 24
/// months without debug tools and can explain every important change from the UI and monthly
/// report." Runs the same <see cref="CampaignShell"/>/<see cref="GensUIController"/> APIs the Unity
/// shell itself uses — <see cref="CampaignShell.Submit{TCommand}"/> for the golden-path commands,
/// <see cref="CampaignShell.AdvanceMonth"/> with the exact system list the console runner's own
/// <c>advance</c> verb and <c>GensUIController</c> both run — for 24 consecutive months, and asserts a
/// <see cref="MonthlyReportAdapter"/>-shaped view model is producible for every single one of them. No
/// debug/cheat tooling (e.g. <see cref="CampaignDebugQuery"/>) is used anywhere in this run.
/// </summary>
public sealed class TwentyFourMonthCampaignSoakTests
{
    private static readonly IReadOnlyList<IMonthlySystem<WorldState>> MonthlySystems =
        new IMonthlySystem<WorldState>[] { new ScheduledActionSystem() };

    [Test]
    public void CampaignCompletesTwentyFourMonthsWithAReportableMonthlyReportEveryMonth()
    {
        var shell = CampaignTestFixtures.Bootstrap();
        var characterId = CampaignTestFixtures.AddAdultHouseholdMember(shell);
        var startDate = shell.State.Date;

        AssertAccepted(shell.Submit(AssignDutyCommands.Pipeline, new AssignDutyCommand(
            shell.State.CommandIds.Issue(), shell.HouseholdId.ToTaggedString(), shell.State.Date, null,
            characterId, shell.HouseholdId, DutySlot.FieldHand)));

        for (var month = 0; month < 24; month++)
        {
            if (month == 6)
            {
                AssertAccepted(shell.Submit(Gens.Simulation.Policies.ChangeRitesBudgetCommands.Pipeline,
                    new Gens.Simulation.Policies.ChangeRitesBudgetCommand(
                        shell.State.CommandIds.Issue(), shell.HouseholdId.ToTaggedString(), shell.State.Date, null,
                        shell.HouseholdId, Gens.Simulation.Policies.RitesBudgetTier.Lavish)));
            }

            var events = shell.AdvanceMonth(MonthlySystems);

            var financials = shell.Query(new HouseholdFinancialsQuery(shell.HouseholdId), "player");
            var report = MonthlyReportProjector.Project(shell.State.Date, events);
            var viewModel = MonthlyReportAdapter.Adapt(financials, report);

            Assert.That(viewModel.IncomeLabel, Is.Not.Null.And.Not.Empty);
            Assert.That(viewModel.Headlines, Is.Not.Null);

            var inkBar = new InkBarAdapter().Adapt(shell.Query(new InkBarQuery(shell.HouseholdId), "player"));
            Assert.That(inkBar.DateLabel, Is.Not.Null.And.Not.Empty);

            var roster = new HouseholdRosterAdapter().Adapt(shell.Query(new HouseholdRosterQuery(shell.HouseholdId), "player"));
            Assert.That(roster.Rows, Has.Count.EqualTo(1));
        }

        Assert.That(shell.State.Date.TotalMonths, Is.EqualTo(startDate.TotalMonths + 24));
    }

    [Test]
    public void SameSeedReproducesIdenticalStateHashAcrossTwoIndependentTwentyFourMonthRuns()
    {
        Assert.That(RunAndHash(), Is.EqualTo(RunAndHash()));
    }

    private static ulong RunAndHash()
    {
        var shell = CampaignTestFixtures.Bootstrap();
        var characterId = CampaignTestFixtures.AddAdultHouseholdMember(shell);
        AssertAccepted(shell.Submit(AssignDutyCommands.Pipeline, new AssignDutyCommand(
            shell.State.CommandIds.Issue(), shell.HouseholdId.ToTaggedString(), shell.State.Date, null,
            characterId, shell.HouseholdId, DutySlot.FieldHand)));

        for (var month = 0; month < 24; month++)
            shell.AdvanceMonth(MonthlySystems);

        return Gens.Simulation.State.StateHasher.Hash(shell.State);
    }

    private static void AssertAccepted(Gens.Simulation.Commands.CommandResult result) =>
        Assert.That(result.Accepted, Is.True, () => $"Expected command to be accepted; rejected with {result.Error}.");
}
