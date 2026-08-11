using Gens.Simulation.Identity;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Identity;

public sealed class DefinitionIdTests
{
    [Test]
    public void ValidKebabCaseValueIsAccepted()
    {
        var id = new DefinitionId<Good>("iron-ore");
        Assert.That(id.Value, Is.EqualTo("iron-ore"));
        Assert.That(id.ToString(), Is.EqualTo("iron-ore"));
    }

    [TestCase("")]
    [TestCase("Grain")]
    [TestCase("iron_ore")]
    [TestCase("-grain")]
    [TestCase("grain-")]
    [TestCase("grain--ore")]
    public void InvalidValuesAreRejected(string value)
    {
        Assert.That(() => new DefinitionId<Good>(value), Throws.ArgumentException);
    }
}
