using Gens.Simulation.Health;
using Gens.Simulation.Identity;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Health;

public sealed class HealthConditionCatalogTests
{
    [Test]
    public void ConstructorRejectsAnEmptyName()
    {
        Assert.Throws<ArgumentException>(() => new HealthConditionDefinition(
            new DefinitionId<HealthConditionDefinition>("test-fever"), "   ", HealthConditionCategory.Acute, false));
    }

    [Test]
    public void CatalogRejectsDuplicateIds()
    {
        var id = new DefinitionId<HealthConditionDefinition>("test-fever");
        var definitions = new[]
        {
            new HealthConditionDefinition(id, "Test Fever", HealthConditionCategory.Acute, false),
            new HealthConditionDefinition(id, "Test Fever Again", HealthConditionCategory.Chronic, true),
        };

        Assert.Throws<ArgumentException>(() => new HealthConditionCatalog(definitions));
    }

    [Test]
    public void TryGetFindsARegisteredDefinition()
    {
        var id = new DefinitionId<HealthConditionDefinition>("test-fever");
        var catalog = new HealthConditionCatalog(new[]
        {
            new HealthConditionDefinition(id, "Test Fever", HealthConditionCategory.Acute, false),
        });

        Assert.That(catalog.TryGet(id, out var definition), Is.True);
        Assert.That(definition.Name, Is.EqualTo("Test Fever"));
    }

    [Test]
    public void TryGetFailsForAnUnregisteredId()
    {
        var catalog = new HealthConditionCatalog(Array.Empty<HealthConditionDefinition>());

        Assert.That(catalog.TryGet(new DefinitionId<HealthConditionDefinition>("unknown"), out _), Is.False);
    }

    [Test]
    public void GetThrowsForAnUnregisteredId()
    {
        var catalog = new HealthConditionCatalog(Array.Empty<HealthConditionDefinition>());

        Assert.Throws<KeyNotFoundException>(() => catalog.Get(new DefinitionId<HealthConditionDefinition>("unknown")));
    }
}
