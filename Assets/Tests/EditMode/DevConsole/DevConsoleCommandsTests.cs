#nullable enable

using Gens.Presentation.Shell;
using Gens.Presentation.Shell.DevConsole;
using Gens.Presentation.Shell.DevConsole.Commands;
using Gens.Presentation.Tests.Support;
using Gens.Simulation.Queries;
using NUnit.Framework;
using UnityEngine;
using static Gens.Presentation.Tests.Support.PrivateFieldAccess;

namespace Gens.Presentation.Tests.EditMode.DevConsole;

/// <summary>Covers each built-in <see cref="IDevConsoleCommand"/> against a real
/// <see cref="CampaignShellBehaviour"/>/<see cref="CampaignShell"/> pair — the same "real shell, real
/// pipeline" convention <c>CampaignShellTests</c> uses — proving every command reaches the campaign only
/// through <see cref="CampaignShell.Query{TProjection}"/>/<see cref="CampaignShell.Submit{TCommand}"/>
/// (ADR 0013), never a bespoke <c>WorldState</c> traversal.</summary>
public sealed class DevConsoleCommandsTests
{
    private GameObject? _root;

    [TearDown]
    public void TearDown()
    {
        if (_root != null)
            Object.DestroyImmediate(_root);
    }

    private DevConsoleContext BootstrapContext()
    {
        _root = new GameObject("dev-console-commands-test");
        _root.SetActive(false);

        var shellBehaviour = _root.AddComponent<CampaignShellBehaviour>();
        Set(shellBehaviour, "seed", CampaignTestFixtures.DefaultSeed);
        Set(shellBehaviour, "startMonths", CampaignTestFixtures.DefaultStartMonths);
        Set(shellBehaviour, "rulesetId", "classic");
        Set(shellBehaviour, "contentPackHash", "content-hash-placeholder");
        Set(shellBehaviour, "regionId", "latium");
        Set(shellBehaviour, "difficulty", "standard");

        _root.SetActive(true);

        return new DevConsoleContext(shellBehaviour, clearOutput: () => { });
    }

    /// <summary>A <see cref="CampaignShellBehaviour"/> left inactive, so <c>Awake</c> never fires and
    /// <see cref="CampaignShellBehaviour.Shell"/> stays null — the "console opened before any campaign
    /// started" case every command must handle without throwing.</summary>
    private DevConsoleContext ContextWithNoCampaign()
    {
        _root = new GameObject("dev-console-no-campaign-test");
        _root.SetActive(false);
        var shellBehaviour = _root.AddComponent<CampaignShellBehaviour>();
        return new DevConsoleContext(shellBehaviour, clearOutput: () => { });
    }

    [Test]
    public void HelpCommandListsEveryRegisteredCommandByName()
    {
        var commands = DevConsoleCommandRegistry.BuildDefault();
        var help = commands[0];

        var output = help.Execute(BootstrapContext(), args: System.Array.Empty<string>());

        Assert.That(output, Does.Contain("state"));
        Assert.That(output, Does.Contain("hash"));
        Assert.That(output, Does.Contain("query"));
        Assert.That(output, Does.Contain("submit"));
        Assert.That(output, Does.Contain("clear"));
    }

    [Test]
    public void StateCommandPrintsTheDebugSnapshot()
    {
        var context = BootstrapContext();

        var output = new StateCommand().Execute(context, System.Array.Empty<string>());

        Assert.That(output, Does.Contain("StateHash"));
        Assert.That(output, Does.Contain("NextCommandSequenceNumber"));
    }

    [Test]
    public void HashCommandMatchesTheDebugQuerysStateHash()
    {
        var context = BootstrapContext();
        var expected = context.Shell!.Query(new CampaignDebugQuery(), "dev-console").StateHash;

        var output = new HashCommand().Execute(context, System.Array.Empty<string>());

        Assert.That(output, Is.EqualTo(expected.ToString()));
    }

    [Test]
    public void QueryCommandRunsANamedQueryByName()
    {
        var context = BootstrapContext();

        var output = new QueryCommand().Execute(context, new[] { "inkBar" });

        Assert.That(output, Does.Contain("InkBarProjection"));
    }

    [Test]
    public void QueryCommandReportsUnknownQueryNames()
    {
        var context = BootstrapContext();

        var output = new QueryCommand().Execute(context, new[] { "notARealQuery" });

        Assert.That(output, Does.Contain("Unknown query"));
    }

    [Test]
    public void QueryCommandRequiresAnArgument()
    {
        var context = BootstrapContext();

        var output = new QueryCommand().Execute(context, System.Array.Empty<string>());

        Assert.That(output, Does.StartWith("Usage:"));
    }

    [Test]
    public void SubmitVerbCommandRunsAGenericCommandThroughTheRealPipeline()
    {
        var context = BootstrapContext();
        var hashBefore = context.Shell!.Query(new CampaignDebugQuery(), "dev-console").StateHash;

        var output = new SubmitVerbCommand().Execute(context, new[] { "dev.test", "{}" });

        Assert.That(output, Does.StartWith("Executed"));
        var hashAfter = context.Shell!.Query(new CampaignDebugQuery(), "dev-console").StateHash;
        Assert.That(hashAfter, Is.Not.EqualTo(hashBefore));
    }

    [Test]
    public void SubmitVerbCommandRequiresTypeAndPayload()
    {
        var context = BootstrapContext();

        var output = new SubmitVerbCommand().Execute(context, new[] { "dev.test" });

        Assert.That(output, Does.StartWith("Usage:"));
    }

    [Test]
    public void StateCommandReportsNoActiveCampaign()
    {
        var output = new StateCommand().Execute(ContextWithNoCampaign(), System.Array.Empty<string>());

        Assert.That(output, Is.EqualTo("No active campaign."));
    }

    [Test]
    public void QueryCommandReportsNoActiveCampaign()
    {
        var output = new QueryCommand().Execute(ContextWithNoCampaign(), new[] { "inkBar" });

        Assert.That(output, Is.EqualTo("No active campaign."));
    }

    [Test]
    public void SubmitVerbCommandReportsNoActiveCampaign()
    {
        var output = new SubmitVerbCommand().Execute(ContextWithNoCampaign(), new[] { "dev.test", "{}" });

        Assert.That(output, Is.EqualTo("No active campaign."));
    }

    [Test]
    public void ClearCommandInvokesTheContextsClearCallback()
    {
        var cleared = false;
        _root = new GameObject("dev-console-clear-test");
        var shellBehaviour = _root.AddComponent<CampaignShellBehaviour>();
        var context = new DevConsoleContext(shellBehaviour, clearOutput: () => cleared = true);

        new ClearCommand().Execute(context, System.Array.Empty<string>());

        Assert.That(cleared, Is.True);
    }
}
