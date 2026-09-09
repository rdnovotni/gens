using Gens.Accessibility.Windows.Uia;
using Gens.UI;
using NUnit.Framework;

namespace Gens.Accessibility.Windows.Tests;

[TestFixture]
public sealed class UiaRoleMappingTests
{
    [TestCase(AccessibilityRole.None, UiaConstants.GroupControlTypeId)]
    [TestCase(AccessibilityRole.Group, UiaConstants.GroupControlTypeId)]
    [TestCase(AccessibilityRole.Text, UiaConstants.TextControlTypeId)]
    [TestCase(AccessibilityRole.Image, UiaConstants.ImageControlTypeId)]
    [TestCase(AccessibilityRole.Button, UiaConstants.ButtonControlTypeId)]
    [TestCase(AccessibilityRole.CheckBox, UiaConstants.CheckBoxControlTypeId)]
    [TestCase(AccessibilityRole.Toggle, UiaConstants.ButtonControlTypeId)]
    [TestCase(AccessibilityRole.Heading, UiaConstants.TextControlTypeId)]
    [TestCase(AccessibilityRole.List, UiaConstants.ListControlTypeId)]
    [TestCase(AccessibilityRole.ListItem, UiaConstants.ListItemControlTypeId)]
    [TestCase(AccessibilityRole.ScrollView, UiaConstants.PaneControlTypeId)]
    [TestCase(AccessibilityRole.Dialog, UiaConstants.PaneControlTypeId)]
    public void MapsEachRoleToItsControlType(AccessibilityRole role, int expectedControlType) => Assert.That(UiaRoleMapping.ControlTypeFor(role), Is.EqualTo(expectedControlType));

    [TestCase(AccessibilityRole.Button, true)]
    [TestCase(AccessibilityRole.Toggle, false)]
    [TestCase(AccessibilityRole.CheckBox, false)]
    [TestCase(AccessibilityRole.Text, false)]
    public void OnlyButtonSupportsInvoke(AccessibilityRole role, bool expected) => Assert.That(UiaRoleMapping.SupportsInvoke(role), Is.EqualTo(expected));

    [TestCase(AccessibilityRole.Toggle, true)]
    [TestCase(AccessibilityRole.CheckBox, true)]
    [TestCase(AccessibilityRole.Button, false)]
    [TestCase(AccessibilityRole.ScrollView, false)]
    public void ToggleAndCheckBoxSupportToggle(AccessibilityRole role, bool expected) => Assert.That(UiaRoleMapping.SupportsToggle(role), Is.EqualTo(expected));

    [TestCase(AccessibilityRole.ScrollView, true)]
    [TestCase(AccessibilityRole.List, false)]
    [TestCase(AccessibilityRole.Button, false)]
    public void OnlyScrollViewSupportsScroll(AccessibilityRole role, bool expected) => Assert.That(UiaRoleMapping.SupportsScroll(role), Is.EqualTo(expected));
}
