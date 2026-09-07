using NUnit.Framework;

namespace Gens.Client.Desktop.Tests;

[TestFixture]
public sealed class ArchitectureBoundaryTests
{
    [Test]
    public void PresentationIsEngineNeutralAndUiDoesNotReferenceSimulation()
    {
        string[] presentation = typeof(Gens.Presentation.CampaignPresentation).Assembly.GetReferencedAssemblies().Select(static x => x.Name!).ToArray();
        string[] ui = typeof(Gens.UI.UiRoot).Assembly.GetReferencedAssemblies().Select(static x => x.Name!).ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(presentation, Does.Not.Contain("Gens.Platform.Sdl")); Assert.That(presentation, Does.Not.Contain("Gens.Graphics.Skia")); Assert.That(presentation, Does.Not.Contain("UnityEngine"));
            Assert.That(ui, Does.Not.Contain("Gens.Simulation")); Assert.That(ui, Does.Not.Contain("Gens.Application"));
        });
    }

    [Test]
    public void DesktopControllerDoesNotExposeWorldState()
    {
        Type controller = typeof(Gens.Client.Desktop.App.DesktopApplicationController);
        Assert.That(controller.GetProperties().Select(static p => p.PropertyType.FullName), Does.Not.Contain("Gens.Simulation.State.WorldState"));
    }
}
