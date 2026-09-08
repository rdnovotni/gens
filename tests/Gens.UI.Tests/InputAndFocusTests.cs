using Gens.Graphics;
using Gens.Platform;
using Gens.UI.Testing;
using NUnit.Framework;

namespace Gens.UI.Tests;

public sealed class InputAndFocusTests : UiTestFixture
{
    [Test]
    public void SemanticSnapshotTracksFocusAndRejectsUnnamedInteractiveControls()
    {
        UiRoot root = Root(); var named = new Button { Name = "save", Semantics = { Label = "Save campaign" } }; root.AddChild(named); root.Layout(new(200, 100)); root.Focus.RequestFocus(named);
        SemanticTreeSnapshot snapshot = root.CaptureSemantics();
        Assert.Multiple(() => { Assert.That(snapshot.FocusedNode?.Name, Is.EqualTo("Save campaign")); Assert.That(root.ValidateSemantics(), Is.Empty); });
        root.AddChild(new Button { Name = "unnamed" }); Assert.That(root.ValidateSemantics(), Has.Count.EqualTo(1));
    }

    [Test]
    public void MotionPolicyDisablesDecorativeAndShortensInformationalMotion()
    {
        var reduced = new MotionPolicy(MotionMode.Reduced); var none = new MotionPolicy(MotionMode.None);
        Assert.Multiple(() => { Assert.That(reduced.Allows(MotionCategory.Decorative), Is.False); Assert.That(reduced.Adjust(TimeSpan.FromSeconds(1), MotionCategory.Informational), Is.EqualTo(TimeSpan.FromMilliseconds(120))); Assert.That(none.Allows(MotionCategory.Informational), Is.False); Assert.That(none.Allows(MotionCategory.Essential), Is.True); });
    }

    [Test]
    public void HitTestingRespectsNestingOverlayVisibilityClippingAndDisabledPolicy()
    {
        UiRoot root = Root(); var clipped = new Panel { Width = 50, Height = 50, HorizontalAlignment = HorizontalAlignment.Start, VerticalAlignment = VerticalAlignment.Start, ClipToBounds = true }; var lower = new FixedNode(50, 50) { Name = "lower" }; var upper = new FixedNode(50, 50) { Name = "upper", ZIndex = 2 }; clipped.AddChild(lower); clipped.AddChild(upper); root.AddChild(clipped); root.Layout(new(100, 100));
        Assert.That(root.HitTest(new(10, 10)), Is.SameAs(upper)); upper.Visibility = UiVisibility.Hidden; root.Layout(new(100, 100)); Assert.That(root.HitTest(new(10, 10)), Is.SameAs(lower)); lower.IsEnabled = false; Assert.That(root.HitTest(new(10, 10)), Is.SameAs(clipped)); Assert.That(root.HitTest(new(70, 10)), Is.SameAs(root));
    }

    [Test]
    public void PointerRoutingCaptureAndButtonClickSemanticsAreDeterministic()
    {
        UiRoot root = Root(); int clicks = 0; var button = new Button { Name = "button", Width = 80, Height = 40, HorizontalAlignment = HorizontalAlignment.Start, VerticalAlignment = VerticalAlignment.Start, Clicked = () => clicks++ }; root.AddChild(button); var host = new UiTestHost(root, new(200, 100)); host.Layout();
        host.MovePointer(10, 10); Assert.That(button.IsHovered, Is.True); root.PressPointer(new(10, 10)); Assert.That(root.CapturedNode, Is.SameAs(button)); root.MovePointer(new(150, 80)); root.ReleasePointer(new(150, 80)); Assert.That(clicks, Is.Zero);
        root.PressPointer(new(10, 10)); root.MovePointer(new(150, 80)); root.MovePointer(new(10, 10)); root.ReleasePointer(new(10, 10)); Assert.That(clicks, Is.EqualTo(1));
    }

    [Test]
    public void FocusTraversesForwardBackwardAndRecoversFromRemovalDisableAndCollapse()
    {
        UiRoot root = Root(); var row = new Row(); var one = new Button { Name = "one", Width = 10, Height = 10 }; var two = new Button { Name = "two", Width = 10, Height = 10 }; row.AddChild(one); row.AddChild(two); root.AddChild(row); root.Layout(new(100, 20));
        Assert.That(root.Focus.RequestFocus(one), Is.True); root.HandleKey(new(new(43), KeyModifiers.None, false)); Assert.That(root.Focus.FocusedNode, Is.SameAs(two)); root.HandleKey(new(new(43), KeyModifiers.LeftShift, false)); Assert.That(root.Focus.FocusedNode, Is.SameAs(one)); one.IsEnabled = false; Assert.That(root.Focus.FocusedNode, Is.Null); root.Focus.RequestFocus(two); two.Visibility = UiVisibility.Collapsed; Assert.That(root.Focus.FocusedNode, Is.Null); two.Visibility = UiVisibility.Visible; root.Focus.RequestFocus(two); row.RemoveChild(two); Assert.That(root.Focus.FocusedNode, Is.Null);
    }

    [Test]
    public void ModalTrapsFocusBlocksBackgroundAndRestoresPriorFocus()
    {
        UiRoot root = Root(); var background = new Button { Width = 100, Height = 100, Name = "background" }; root.AddChild(background); root.Layout(new(100, 100)); root.Focus.RequestFocus(background); var modal = new Button { Width = 40, Height = 40, Name = "modal", Semantics = { Role = AccessibilityRole.Dialog } }; root.ShowModal(modal); root.Layout(new(100, 100)); Assert.That(root.Focus.FocusedNode, Is.SameAs(modal)); Assert.That(root.HitTest(new(5, 5)), Is.SameAs(modal)); root.CloseModal(); Assert.That(root.Focus.FocusedNode, Is.SameAs(background));
    }

    [Test]
    public void ButtonActivatesFromKeyboardAndDisabledButtonDoesNotActivate()
    {
        UiRoot root = Root(); int count = 0; var button = new Button { Width = 40, Height = 20, Clicked = () => count++ }; root.AddChild(button); root.Layout(new(40, 20)); root.Focus.RequestFocus(button); root.HandleKey(new(new(44), KeyModifiers.None, false)); root.HandleKey(new(new(40), KeyModifiers.None, false)); Assert.That(count, Is.EqualTo(2)); button.IsEnabled = false; root.HandleKey(new(new(40), KeyModifiers.None, false)); root.PressPointer(new(5, 5)); root.ReleasePointer(new(5, 5)); Assert.That(count, Is.EqualTo(2));
    }
}
