using Gens.Simulation.Characters;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class AttributeValueObjectsTests
{
    [TestCase(-1)]
    [TestCase(101)]
    public void CoreAttributesRejectsOutOfRangeComponents(int outOfRange)
    {
        Assert.That(() => new CoreAttributes(outOfRange, 0, 0, 0, 0), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void CoreAttributesAcceptsBoundaryValues()
    {
        Assert.That(() => new CoreAttributes(0, 100, 0, 100, 0), Throws.Nothing);
    }

    [TestCase(-1)]
    [TestCase(101)]
    public void LaborSkillsRejectsOutOfRangeComponents(int outOfRange)
    {
        Assert.That(() => new LaborSkills(outOfRange, 0, 0, 0, 0), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [TestCase(-1)]
    [TestCase(101)]
    public void ConditionRejectsOutOfRangeComponents(int outOfRange)
    {
        Assert.That(() => new Condition(outOfRange, 0, 0, 0, 0), Throws.TypeOf<ArgumentOutOfRangeException>());
    }
}
