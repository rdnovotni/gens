#nullable enable

using Gens.Presentation.Shell.DevConsole;
using NUnit.Framework;

namespace Gens.Presentation.Tests.EditMode.DevConsole;

/// <summary>Covers <see cref="DevConsoleLineParser"/> in isolation — the console's line-splitting logic
/// is Unity-free on purpose, so it needs no <c>MonoBehaviour</c> host to test.</summary>
public sealed class DevConsoleLineParserTests
{
    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    public void ParseReturnsNullForBlankInput(string? line)
    {
        Assert.That(DevConsoleLineParser.Parse(line!), Is.Null);
    }

    [Test]
    public void ParseSplitsTheVerbFromItsArguments()
    {
        var result = DevConsoleLineParser.Parse("query inkBar");

        Assert.That(result!.Value.Verb, Is.EqualTo("query"));
        Assert.That(result.Value.Args, Is.EqualTo(new[] { "inkBar" }));
    }

    [Test]
    public void ParseReturnsNoArgumentsForABareVerb()
    {
        var result = DevConsoleLineParser.Parse("help");

        Assert.That(result!.Value.Verb, Is.EqualTo("help"));
        Assert.That(result.Value.Args, Is.Empty);
    }

    [Test]
    public void ParseCollapsesRepeatedSpacesBetweenTokens()
    {
        var result = DevConsoleLineParser.Parse("  submit   command.type   {}  actor  ");

        Assert.That(result!.Value.Verb, Is.EqualTo("submit"));
        Assert.That(result.Value.Args, Is.EqualTo(new[] { "command.type", "{}", "actor" }));
    }
}
