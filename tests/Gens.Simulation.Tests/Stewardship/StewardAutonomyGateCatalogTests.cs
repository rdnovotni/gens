using Gens.Simulation.Policies;
using Gens.Simulation.Stewardship;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Stewardship;

/// <summary>Phase 10 item 2 coverage for <see cref="StewardAutonomyGateCatalog"/>.</summary>
public sealed class StewardAutonomyGateCatalogTests
{
    [Test]
    public void ChangeRitesBudgetRequiresAtLeastStandard()
    {
        Assert.Multiple(() =>
        {
            Assert.That(StewardAutonomyGateCatalog.IsAllowed(PolicyActionDefinitions.ChangeRitesBudget, StewardAutonomyLevel.Conservative), Is.False);
            Assert.That(StewardAutonomyGateCatalog.IsAllowed(PolicyActionDefinitions.ChangeRitesBudget, StewardAutonomyLevel.Standard), Is.True);
            Assert.That(StewardAutonomyGateCatalog.IsAllowed(PolicyActionDefinitions.ChangeRitesBudget, StewardAutonomyLevel.FullAutonomy), Is.True);
        });
    }

    [Test]
    public void FundFestivalRequiresFullAutonomy()
    {
        Assert.Multiple(() =>
        {
            Assert.That(StewardAutonomyGateCatalog.IsAllowed(PolicyActionDefinitions.FundFestival, StewardAutonomyLevel.Standard), Is.False);
            Assert.That(StewardAutonomyGateCatalog.IsAllowed(PolicyActionDefinitions.FundFestival, StewardAutonomyLevel.FullAutonomy), Is.True);
        });
    }
}
