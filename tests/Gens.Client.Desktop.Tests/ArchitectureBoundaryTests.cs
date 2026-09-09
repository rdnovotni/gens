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

    // Gate 6 of the Unity retirement audit: DesktopApplicationController.CurrentCampaign used to
    // publicly re-expose CampaignSession, which itself carries a public WorldState escape hatch
    // (CampaignSession.State) required by the legacy Unity client. This asserts the controller's
    // public surface never leaks either type again, one level of indirection included.
    [Test]
    public void DesktopControllerDoesNotExposeCampaignSessionOrWorldState()
    {
        Type controller = typeof(Gens.Client.Desktop.App.DesktopApplicationController);
        string[] forbidden = { "Gens.Application.Campaign.CampaignSession", "Gens.Simulation.State.WorldState" };

        IEnumerable<Type> publicMemberTypes = controller.GetProperties().Select(static p => p.PropertyType)
            .Concat(controller.GetMethods().Where(static m => m.IsPublic).Select(static m => m.ReturnType));

        foreach (Type type in publicMemberTypes)
        {
            Type effective = Nullable.GetUnderlyingType(type) ?? type;
            Assert.That(forbidden, Does.Not.Contain(effective.FullName), $"{effective.FullName} must not be a public member type on DesktopApplicationController.");
            if (effective.IsGenericType)
                foreach (Type argument in effective.GetGenericArguments())
                    Assert.That(forbidden, Does.Not.Contain(argument.FullName), $"{argument.FullName} must not appear as a generic argument on a public member of DesktopApplicationController.");
        }
    }
}
