namespace Gens.Accessibility.Windows.Uia;

/// <summary>
/// UI Automation control-type, property, pattern, and event IDs (from <c>UIAutomationClient.h</c>). These are
/// stable, documented Win32 constants — see
/// https://learn.microsoft.com/en-us/windows/win32/winauto/uiauto-controltype-ids (and the sibling
/// property/pattern/event-id pages) for the authoritative values.
/// </summary>
internal static class UiaConstants
{
    // Property IDs
    internal const int NamePropertyId = 30005;
    internal const int ControlTypePropertyId = 30003;
    internal const int IsEnabledPropertyId = 30010;
    internal const int HasKeyboardFocusPropertyId = 30008;
    internal const int IsKeyboardFocusablePropertyId = 30009;
    internal const int BoundingRectanglePropertyId = 30001;
    internal const int AutomationIdPropertyId = 30011;
    internal const int IsContentElementPropertyId = 30017;
    internal const int IsControlElementPropertyId = 30016;

    // Control type IDs
    internal const int ButtonControlTypeId = 50000;
    internal const int CheckBoxControlTypeId = 50002;
    internal const int ImageControlTypeId = 50006;
    internal const int ListItemControlTypeId = 50007;
    internal const int ListControlTypeId = 50008;
    internal const int PaneControlTypeId = 50033;
    internal const int TextControlTypeId = 50020;
    internal const int GroupControlTypeId = 50026;
    internal const int CustomControlTypeId = 50025;

    // Pattern IDs
    internal const int InvokePatternId = 10000;
    internal const int ScrollPatternId = 10004;
    internal const int TogglePatternId = 10015;

    // Event IDs
    internal const int AutomationFocusChangedEventId = 20005;
    internal const int StructureChangedEventId = 20002;

    // Window message
    internal const uint WM_GETOBJECT = 0x003D;
    internal const int OBJID_CLIENT = -4;
}
