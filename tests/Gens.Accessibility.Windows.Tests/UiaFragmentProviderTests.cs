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

    private static (SemanticFragmentIndex Index, UiaFragmentRootProvider Root) BuildSingleButtonTree(bool focused = false)
    {
        SemanticNodeSnapshot button = Button("btn", "Save campaign", focused);
        var snapshot = new SemanticTreeSnapshot(new("root", AccessibilityRole.Group, "root", null, null, true, false, false, new Rect(0, 0, 100, 100), [button]), focused ? button : null);
        SemanticFragmentIndex? index = null;
        var root = new UiaFragmentRootProvider(() => index, FakeLogicalToScreen, static () => new IntPtr(1));
        index = new SemanticFragmentIndex(snapshot);
        return (index, root);
    }

    [Test]
    public void GetPropertyValueNameReturnsTheSnapshotName()
    {
        (SemanticFragmentIndex index, UiaFragmentRootProvider root) = BuildSingleButtonTree();
        var provider = new UiaFragmentProvider(() => index, idx => idx.Find("btn"), FakeLogicalToScreen, root);
        Assert.That(provider.GetPropertyValue(UiaConstants.NamePropertyId), Is.EqualTo("Save campaign"));
    }

    [Test]
    public void GetPatternProviderReturnsInvokeOnlyForButtonRole()
    {
        (SemanticFragmentIndex index, UiaFragmentRootProvider root) = BuildSingleButtonTree();
        var buttonProvider = new UiaFragmentProvider(() => index, idx => idx.Find("btn"), FakeLogicalToScreen, root);
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
        (SemanticFragmentIndex index, UiaFragmentRootProvider root) = BuildSingleButtonTree();
        var provider = new UiaFragmentProvider(() => index, idx => idx.Find("btn"), FakeLogicalToScreen, root);
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
        (SemanticFragmentIndex index, UiaFragmentRootProvider root) = BuildSingleButtonTree();
        var provider = new UiaFragmentProvider(() => index, idx => idx.Find("btn"), FakeLogicalToScreen, root);
        IRawElementProviderFragment? parent = provider.Navigate(NavigateDirection.Parent);
        Assert.That(parent, Is.SameAs(root));
    }

    [Test]
    public void RootGetFocusReturnsAProviderForTheFocusedNode()
    {
        (SemanticFragmentIndex _, UiaFragmentRootProvider root) = BuildSingleButtonTree(focused: true);
        IRawElementProviderFragment? focusProvider = root.GetFocus();
        Assert.That(focusProvider, Is.Not.Null);
        Assert.That(((UiaFragmentProvider)focusProvider!).GetPropertyValue(UiaConstants.NamePropertyId), Is.EqualTo("Save campaign"));
    }

    [Test]
    public void RootGetFocusReturnsNullWhenNothingIsFocused()
    {
        (SemanticFragmentIndex _, UiaFragmentRootProvider root) = BuildSingleButtonTree(focused: false);
        Assert.That(root.GetFocus(), Is.Null);
    }
}
