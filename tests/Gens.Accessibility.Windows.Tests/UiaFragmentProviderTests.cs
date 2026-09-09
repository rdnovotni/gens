using Gens.Accessibility.Windows.Uia;
using Gens.Graphics;
using Gens.UI;
using NUnit.Framework;

namespace Gens.Accessibility.Windows.Tests;

[TestFixture]
public sealed class UiaFragmentProviderTests
{
    private static SemanticNodeSnapshot Button(string id, string name, bool focused = false) =>
        new(id, AccessibilityRole.Button, name, null, null, IsEnabled: true, IsFocused: focused, IsChecked: false, new Rect(10, 20, 30, 40), []);

    private static SemanticNodeSnapshot Text(string id, string name) =>
        new(id, AccessibilityRole.Text, name, null, null, IsEnabled: true, IsFocused: false, IsChecked: false, new Rect(0, 0, 5, 5), []);

    private static Rect ExpectedScreenRect => new(110, 120, 30, 40);
    private static UiaRect FakeLogicalToScreen(Rect logical) => new() { Left = logical.X + 100, Top = logical.Y + 100, Width = logical.Width, Height = logical.Height };

    private static (SemanticFragmentIndex Index, UiaFragmentRootProvider Root, SemanticNodeSnapshot Button) BuildSingleButtonTree(bool focused = false)
    {
        SemanticNodeSnapshot button = Button("btn", "Save campaign", focused);
        var snapshot = new SemanticTreeSnapshot(new("root", AccessibilityRole.Group, "root", null, null, true, false, false, new Rect(0, 0, 100, 100), [button]), focused ? button : null);
        SemanticFragmentIndex? index = null;
        var root = new UiaFragmentRootProvider(() => index, FakeLogicalToScreen, static () => new IntPtr(1));
        index = new SemanticFragmentIndex(snapshot);
        return (index, root, button);
    }

    private static UiaFragmentProvider ButtonProvider(SemanticFragmentIndex index, UiaFragmentRootProvider root, SemanticNodeSnapshot button)
    {
        string key = index.KeyOf(button);
        return new UiaFragmentProvider(() => index, idx => idx.Find(key), FakeLogicalToScreen, root);
    }

    [Test]
    public void GetPropertyValueNameReturnsTheSnapshotName()
    {
        (SemanticFragmentIndex index, UiaFragmentRootProvider root, SemanticNodeSnapshot button) = BuildSingleButtonTree();
        UiaFragmentProvider provider = ButtonProvider(index, root, button);
        Assert.That(provider.GetPropertyValue(UiaConstants.NamePropertyId), Is.EqualTo("Save campaign"));
    }

    [Test]
    public void GetPatternProviderReturnsInvokeOnlyForButtonRole()
    {
        (SemanticFragmentIndex index, UiaFragmentRootProvider root, SemanticNodeSnapshot button) = BuildSingleButtonTree();
        UiaFragmentProvider buttonProvider = ButtonProvider(index, root, button);
        var textNode = Text("txt", "Label");
        var textProvider = new UiaFragmentProvider(() => index, _ => textNode, FakeLogicalToScreen, root);

        Assert.Multiple(() =>
        {
            Assert.That(buttonProvider.GetPatternProvider(UiaConstants.InvokePatternId), Is.Not.Null);
            Assert.That(textProvider.GetPatternProvider(UiaConstants.InvokePatternId), Is.Null);
            Assert.That(buttonProvider.GetPatternProvider(UiaConstants.TogglePatternId), Is.Null);
        });
    }

    [Test]
    public void BoundingRectangleUsesTheInjectedLogicalToScreenConversion()
    {
        (SemanticFragmentIndex index, UiaFragmentRootProvider root, SemanticNodeSnapshot button) = BuildSingleButtonTree();
        UiaFragmentProvider provider = ButtonProvider(index, root, button);
        UiaRect rect = provider.BoundingRectangle;
        Assert.Multiple(() =>
        {
            Assert.That(rect.Left, Is.EqualTo(ExpectedScreenRect.X));
            Assert.That(rect.Top, Is.EqualTo(ExpectedScreenRect.Y));
            Assert.That(rect.Width, Is.EqualTo(ExpectedScreenRect.Width));
            Assert.That(rect.Height, Is.EqualTo(ExpectedScreenRect.Height));
        });
    }

    [Test]
    public void NavigateFromChildToParentReturnsTheRootProvider()
    {
        (SemanticFragmentIndex index, UiaFragmentRootProvider root, SemanticNodeSnapshot button) = BuildSingleButtonTree();
        UiaFragmentProvider provider = ButtonProvider(index, root, button);
        IRawElementProviderFragment? parent = provider.Navigate(NavigateDirection.Parent);
        Assert.That(parent, Is.SameAs(root));
    }

    [Test]
    public void GetRuntimeIdIsStableForTheSameNodeAndDistinctAcrossSiblings()
    {
        SemanticNodeSnapshot first = Button("Button", "First");
        SemanticNodeSnapshot second = Button("Button", "Second"); // same fallback-style Id as `first`
        var snapshot = new SemanticTreeSnapshot(new("root", AccessibilityRole.Group, "root", null, null, true, false, false, new Rect(0, 0, 100, 100), [first, second]), FocusedNode: null);
        var index = new SemanticFragmentIndex(snapshot);
        var root = new UiaFragmentRootProvider(() => index, FakeLogicalToScreen, static () => new IntPtr(1));

        UiaFragmentProvider firstProviderA = new(() => index, idx => idx.Find(index.KeyOf(first)), FakeLogicalToScreen, root);
        UiaFragmentProvider firstProviderB = new(() => index, idx => idx.Find(index.KeyOf(first)), FakeLogicalToScreen, root);
        UiaFragmentProvider secondProvider = new(() => index, idx => idx.Find(index.KeyOf(second)), FakeLogicalToScreen, root);

        Assert.Multiple(() =>
        {
            Assert.That(firstProviderA.GetRuntimeId(), Is.EqualTo(firstProviderB.GetRuntimeId()));
            Assert.That(firstProviderA.GetRuntimeId(), Is.Not.EqualTo(secondProvider.GetRuntimeId()));
        });
    }

    [Test]
    public void RootGetFocusReturnsAProviderForTheFocusedNode()
    {
        (SemanticFragmentIndex _, UiaFragmentRootProvider root, SemanticNodeSnapshot _) = BuildSingleButtonTree(focused: true);
        IRawElementProviderFragment? focusProvider = root.GetFocus();
        Assert.That(focusProvider, Is.Not.Null);
        Assert.That(((UiaFragmentProvider)focusProvider!).GetPropertyValue(UiaConstants.NamePropertyId), Is.EqualTo("Save campaign"));
    }

    [Test]
    public void RootGetFocusReturnsNullWhenNothingIsFocused()
    {
        (SemanticFragmentIndex _, UiaFragmentRootProvider root, SemanticNodeSnapshot _) = BuildSingleButtonTree(focused: false);
        Assert.That(root.GetFocus(), Is.Null);
    }
}
