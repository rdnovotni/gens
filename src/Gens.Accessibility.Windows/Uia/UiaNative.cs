using System.Runtime.InteropServices;

namespace Gens.Accessibility.Windows.Uia;

internal static class UiaNative
{
    [DllImport("UIAutomationCore.dll")]
    internal static extern IntPtr UiaReturnRawElementProvider(IntPtr hwnd, IntPtr wParam, IntPtr lParam, [MarshalAs(UnmanagedType.IUnknown)] object? provider);

    [DllImport("UIAutomationCore.dll")]
    internal static extern int UiaHostProviderFromHwnd(IntPtr hwnd, [MarshalAs(UnmanagedType.IUnknown)] out object provider);

    [DllImport("UIAutomationCore.dll")]
    internal static extern int UiaRaiseAutomationEvent([MarshalAs(UnmanagedType.IUnknown)] object provider, int eventId);

    [DllImport("UIAutomationCore.dll")]
    internal static extern int UiaDisconnectProvider([MarshalAs(UnmanagedType.IUnknown)] object provider);

    [StructLayout(LayoutKind.Sequential)]
    internal struct Point { internal int X; internal int Y; }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WinRect { internal int Left; internal int Top; internal int Right; internal int Bottom; }

    [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)] internal static extern bool ClientToScreen(IntPtr hwnd, ref Point point);
    [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)] internal static extern bool GetWindowRect(IntPtr hwnd, out WinRect rect);
    [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)] internal static extern bool GetClientRect(IntPtr hwnd, out WinRect rect);
    [DllImport("user32.dll")] internal static extern uint GetDpiForWindow(IntPtr hwnd);
}
