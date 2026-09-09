using System.Runtime.InteropServices;

namespace Gens.Accessibility.Windows.Uia;

// COM interface declarations mirroring UIAutomationCore.h. GUIDs and member order must match the real SDK
// header exactly for a screen reader to bind correctly — verified against
// https://github.com/microsoft/win32metadata's recompiled UIAutomationCore.h. Confirm against a live NVDA or
// Windows Accessibility Insights session before relying on this in production (see docs/engineering/accessibility.md).

internal enum NavigateDirection { Parent = 0, NextSibling = 1, PreviousSibling = 2, FirstChild = 3, LastChild = 4 }

[Flags]
internal enum UiaProviderOptions
{
    ClientSideProvider = 0x1,
    ServerSideProvider = 0x2,
    NonClientAreaProvider = 0x4,
    OverrideProvider = 0x8,
    ProviderOwnsSetFocus = 0x10,
    UseComThreading = 0x20,
    RefuseNonClientSupport = 0x40,
    HasNativeIAccessible = 0x80,
    UseClientCoordinates = 0x100,
}

internal enum UiaToggleState { Off = 0, On = 1, Indeterminate = 2 }
internal enum UiaScrollAmount { LargeDecrement = 0, SmallDecrement = 1, NoAmount = 2, LargeIncrement = 3, SmallIncrement = 4 }

[StructLayout(LayoutKind.Sequential)]
internal struct UiaRect { internal double Left, Top, Width, Height; }

[ComImport, Guid("d6dd68d1-86fd-4332-8666-9abedea2d24c"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IRawElementProviderSimple
{
    UiaProviderOptions ProviderOptions { get; }
    [return: MarshalAs(UnmanagedType.IUnknown)] object? GetPatternProvider(int patternId);
    [return: MarshalAs(UnmanagedType.Struct)] object? GetPropertyValue(int propertyId);
    IRawElementProviderSimple? HostRawElementProvider { get; }
}

[ComImport, Guid("f7063da8-8359-439c-9297-bbc5299a7d87"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IRawElementProviderFragment
{
    IRawElementProviderFragment? Navigate(NavigateDirection direction);
    int[]? GetRuntimeId();
    UiaRect BoundingRectangle { get; }
    object[]? GetEmbeddedFragmentRoots();
    void SetFocus();
    IRawElementProviderFragmentRoot? FragmentRoot { get; }
}

[ComImport, Guid("620ce2a5-ab8f-40a9-86cb-de3c75599b58"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IRawElementProviderFragmentRoot
{
    IRawElementProviderFragment? ElementProviderFromPoint(double x, double y);
    IRawElementProviderFragment? GetFocus();
}

[ComImport, Guid("54fcb24b-e18e-47a2-b4d3-eccbe77599a2"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IInvokeProvider
{
    void Invoke();
}

[ComImport, Guid("bd5b5915-fcb8-475d-bea1-e6d3b1f5e236"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IToggleProvider
{
    void Toggle();
    UiaToggleState ToggleState { get; }
}

[ComImport, Guid("b17d6187-0907-464b-a168-0ef17a1572b1"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IScrollProvider
{
    void Scroll(UiaScrollAmount horizontalAmount, UiaScrollAmount verticalAmount);
    void SetScrollPercent(double horizontalPercent, double verticalPercent);
    double HorizontalScrollPercent { get; }
    double VerticalScrollPercent { get; }
    double HorizontalViewSize { get; }
    double VerticalViewSize { get; }
    bool HorizontallyScrollable { get; }
    bool VerticallyScrollable { get; }
}
