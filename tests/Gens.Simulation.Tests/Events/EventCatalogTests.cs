using Gens.Simulation.Commands;
using Gens.Simulation.Events;
using Gens.Simulation.Identity;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Events;

public sealed class EventCatalogTests
{
    private static readonly DefinitionId<EventDefinition> PersonalId = new("personal-test");
    private static readonly DefinitionId<EventDefinition> HouseholdId = new("household-test");

    [Test]
    public void GetReturnsARegisteredDefinition()
    {
        var catalog = new EventCatalog(new[] { Make(PersonalId, EventScope.Personal) });

        Assert.That(catalog.Get(PersonalId).Id, Is.EqualTo(PersonalId));
    }

    [Test]
    public void GetThrowsForAnUnregisteredId()
    {
        var catalog = new EventCatalog(Array.Empty<EventDefinition>());

        Assert.Throws<KeyNotFoundException>(() => catalog.Get(PersonalId));
    }

    [Test]
    public void ConstructorRejectsDuplicateIds()
    {
        var duplicate = Make(PersonalId, EventScope.Personal);

        Assert.Throws<ArgumentException>(() => new EventCatalog(new[] { duplicate, duplicate }));
    }

    [Test]
    public void ForScopeFiltersToOnlyThatScope()
    {
        var catalog = new EventCatalog(new[]
        {
            Make(PersonalId, EventScope.Personal),
            Make(HouseholdId, EventScope.Household),
        });

        var personalOnly = catalog.ForScope(EventScope.Personal).ToArray();

        Assert.That(personalOnly, Has.Length.EqualTo(1));
        Assert.That(personalOnly[0].Id, Is.EqualTo(PersonalId));
    }

    private static EventDefinition Make(DefinitionId<EventDefinition> id, EventScope scope)
    {
        var option = new EventOptionDefinition(
            new DefinitionId<EventOptionDefinition>("only-option"), "n", "d",
            eligibility: static (_, _) => null,
            scoreForAi: static (_, _) => 1.0,
            resolve: static (_, _, _, _) => Array.Empty<IDomainEvent>());
        var stage = new EventStageDefinition(0, "n", "d", new[] { option }, expiryMonths: 1);
        return new EventDefinition(
            id, scope, "n", "d", EventTriggerKind.Random,
            eligibility: static (_, _) => true,
            stages: new[] { stage },
            weight: static (_, _) => 1);
    }
}
