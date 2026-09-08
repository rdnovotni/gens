using System.Runtime.InteropServices;
using Gens.UI;

namespace Gens.Accessibility.Windows;

/// <summary>Publishes semantic focus changes to Windows assistive technology. Full UIA provider hosting is tracked separately.</summary>
public sealed class WindowsAccessibilityBridge : Gens.Accessibility.IAccessibilityBridge
{
    private const uint EventObjectFocus = 0x8005;
    private const int ObjIdClient = -4;
    public string Name => "Windows accessibility event bridge";
    public bool IsAvailable => OperatingSystem.IsWindows();
    public SemanticTreeSnapshot? LatestSnapshot { get; private set; }
    public void Publish(SemanticTreeSnapshot snapshot) => LatestSnapshot = snapshot;
    public void FocusChanged(SemanticNodeSnapshot? node)
    {
        if (IsAvailable) NotifyWinEvent(EventObjectFocus, IntPtr.Zero, ObjIdClient, 0);
    }
    public void Dispose() { }
    [DllImport("user32.dll")] private static extern void NotifyWinEvent(uint eventId, IntPtr hwnd, int idObject, int idChild);
}
