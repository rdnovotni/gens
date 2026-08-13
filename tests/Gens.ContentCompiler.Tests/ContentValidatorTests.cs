using Gens.ContentCompiler.Content;
using NUnit.Framework;

namespace Gens.ContentCompiler.Tests;

/// <summary>
/// Proves Phase 3, item 5's validations are hard failures, not warnings, matching the exit gate's
/// "invalid references and schema violations fail before Unity starts."
/// </summary>
public sealed class ContentValidatorTests
{
    private static readonly string FixturesRoot = Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures");

    [Test]
    public void ValidPackHasNoErrors()
    {
        var errors = Validate("valid-pack");

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void DuplicateDefinitionIdWithinAFamilyIsAHardError()
    {
        var errors = Validate("duplicate-id-pack");

        Assert.That(errors, Has.Some.Matches<ContentValidationError>(e =>
            e.Family == "goods" && e.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase)));
    }

    [Test]
    public void DanglingCrossFamilyReferenceIsAHardError()
    {
        var errors = Validate("dangling-reference-pack");

        Assert.That(errors, Has.Some.Matches<ContentValidationError>(e =>
            e.Family == "buildings" && e.Message.Contains("bread", StringComparison.Ordinal)));
    }

    [Test]
    public void OneDirectionalTraitOppositionIsAHardError()
    {
        var errors = Validate("opposed-pair-pack");

        Assert.That(errors, Has.Some.Matches<ContentValidationError>(e =>
            e.Family == "traits" && e.Id == "one-sided" && e.Message.Contains("mutual", StringComparison.OrdinalIgnoreCase)));
    }

    [Test]
    public void DuplicateTierPositionWithinATraitSpectrumIsAHardError()
    {
        var errors = Validate("opposed-pair-pack");

        Assert.That(errors, Has.Some.Matches<ContentValidationError>(e =>
            e.Family == "traits" && e.Id == "tier-zero-b" && e.Message.Contains("tier position", StringComparison.OrdinalIgnoreCase)));
    }

    [Test]
    public void MissingLocalizationKeyIsAHardError()
    {
        var errors = Validate("missing-localization-pack");

        Assert.That(errors, Has.Some.Matches<ContentValidationError>(e =>
            e.Family == "goods" && e.Message.Contains("localization", StringComparison.OrdinalIgnoreCase)));
    }

    [Test]
    public void ValidPackCompilesWithGoodsBeforeBuildingsInDependencyOrder()
    {
        var pack = ContentPack.Load(Path.Combine(FixturesRoot, "valid-pack"));
        var compiled = ContentManifestBuilder.Build(pack);

        var goodsIndex = compiled.Manifest.DependencyOrder.ToList().IndexOf("goods");
        var buildingsIndex = compiled.Manifest.DependencyOrder.ToList().IndexOf("buildings");

        Assert.That(goodsIndex, Is.LessThan(buildingsIndex));
    }

    [Test]
    public void CompilingTheSamePackTwiceProducesByteIdenticalOutput()
    {
        var pack = ContentPack.Load(Path.Combine(FixturesRoot, "valid-pack"));
        var first = Gens.Simulation.Saves.CanonicalJson.SerializeToCanonicalBytes(ContentManifestBuilder.Build(pack));
        var second = Gens.Simulation.Saves.CanonicalJson.SerializeToCanonicalBytes(ContentManifestBuilder.Build(pack));

        Assert.That(second, Is.EqualTo(first));
    }

    private static IReadOnlyList<ContentValidationError> Validate(string fixtureName)
    {
        var root = Path.Combine(FixturesRoot, fixtureName);
        var pack = ContentPack.Load(root);
        var validator = new ContentValidator(Path.Combine(root, "schemas"));
        return validator.Validate(pack);
    }
}
