using Gens.UI;

namespace Gens.Accessibility.Windows.Uia;

/// <summary>Pure mapping from Gens's engine-neutral <see cref="AccessibilityRole"/> to UIA control types and pattern support. No COM types — fully unit-testable.</summary>
internal static class UiaRoleMapping
{
    internal static int ControlTypeFor(AccessibilityRole role) => role switch
    {
        AccessibilityRole.Button => UiaConstants.ButtonControlTypeId,
        AccessibilityRole.CheckBox => UiaConstants.CheckBoxControlTypeId,
        AccessibilityRole.Toggle => UiaConstants.ButtonControlTypeId,
        AccessibilityRole.Heading => UiaConstants.TextControlTypeId,
        AccessibilityRole.List => UiaConstants.ListControlTypeId,
        AccessibilityRole.ListItem => UiaConstants.ListItemControlTypeId,
        AccessibilityRole.ScrollView => UiaConstants.PaneControlTypeId,
        AccessibilityRole.Image => UiaConstants.ImageControlTypeId,
        AccessibilityRole.Dialog => UiaConstants.PaneControlTypeId,
        AccessibilityRole.Text => UiaConstants.TextControlTypeId,
        AccessibilityRole.Group or AccessibilityRole.None => UiaConstants.GroupControlTypeId,
        _ => UiaConstants.CustomControlTypeId,
    };

    internal static bool SupportsInvoke(AccessibilityRole role) => role == AccessibilityRole.Button;
    internal static bool SupportsToggle(AccessibilityRole role) => role is AccessibilityRole.Toggle or AccessibilityRole.CheckBox;
    internal static bool SupportsScroll(AccessibilityRole role) => role == AccessibilityRole.ScrollView;
}
