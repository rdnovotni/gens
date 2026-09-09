using System.Runtime.InteropServices;
using Gens.Accessibility.Windows.Uia;
using Gens.Graphics;
using Gens.UI;

namespace Gens.Accessibility.Windows;

/// <summary>
/// Publishes semantic snapshots to Windows assistive technology through both the legacy MSAA focus-event path
/// (<c>NotifyWinEvent</c>) and a real UI Automation fragment-provider tree (<see cref="Uia.UiaFragmentProvider"/>).
/// See docs/engineering/accessibility.md for what is and is not yet certified against a real screen reader.
/// </summary>
public sealed class WindowsAccessibilityBridge : Gens.Accessibility.IAccessibilityBridge
{
    private const uint EventObjectFocus = 0x8005;
    private const int ObjIdClient = -4;

    private readonly Func<IntPtr> hwndAccessor;
    private SemanticFragmentIndex? latestIndex;
    private UiaFragmentRootProvider? rootProvider;

    /// <param name="hwndAccessor">Returns the native HWND to host UIA on, or <see cref="IntPtr.Zero"/> before a window exists.</param>
    public WindowsAccessibilityBridge(Func<IntPtr>? hwndAccessor = null) => this.hwndAccessor = hwndAccessor ?? (static () => IntPtr.Zero);

    public string Name => "Windows accessibility bridge (MSAA focus events + UI Automation fragment provider)";
    public bool IsAvailable => OperatingSystem.IsWindows();
    public SemanticTreeSnapshot? LatestSnapshot { get; private set; }

    public void Publish(SemanticTreeSnapshot snapshot)
    {
        LatestSnapshot = snapshot;
        latestIndex = new SemanticFragmentIndex(snapshot);
        rootProvider ??= new UiaFragmentRootProvider(() => latestIndex, LogicalToScreen, hwndAccessor);
    }

    public void FocusChanged(SemanticNodeSnapshot? node)
    {
        if (!IsAvailable) return;
        NotifyWinEvent(EventObjectFocus, IntPtr.Zero, ObjIdClient, 0);
        if (node is null || rootProvider is null || latestIndex is null) return;
        string nodeKey = latestIndex.KeyOf(node);
        var provider = new UiaFragmentProvider(() => latestIndex, index => index.Find(nodeKey), LogicalToScreen, rootProvider);
        _ = UiaNative.UiaRaiseAutomationEvent(provider, UiaConstants.AutomationFocusChangedEventId);
    }

    /// <summary>
    /// Call from the platform's Windows-message hook (see <c>Gens.Platform.Sdl.SdlWindowsMessageHook</c>) for
    /// every message on the game window. Responds to <c>WM_GETOBJECT</c> for the client area by handing UIA the
    /// root fragment provider. The exact mechanism SDL's message hook uses to deliver a custom WndProc return
    /// value for WM_GETOBJECT has not been confirmed against a live SDL window — verify this end-to-end with a
    /// real UIA client (NVDA, Narrator, or Accessibility Insights) before relying on it; see
    /// docs/engineering/accessibility.md.
    /// </summary>
    public bool HandleWindowsMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam)
    {
        // WM_GETOBJECT carries the requested object id in lParam (OBJID_CLIENT for a raw-element provider);
        // wParam is an unrelated child-window handle/zero and must simply be forwarded, not checked.
        if (!IsAvailable || message != UiaConstants.WM_GETOBJECT || lParam.ToInt64() != UiaConstants.OBJID_CLIENT || rootProvider is null) return true;
        UiaNative.UiaReturnRawElementProvider(hwnd, wParam, lParam, rootProvider);
        return false;
    }

    public void Dispose()
    {
        if (rootProvider is not null) _ = UiaNative.UiaDisconnectProvider(rootProvider);
        rootProvider = null;
    }

    private UiaRect LogicalToScreen(Rect logical)
    {
        IntPtr hwnd = hwndAccessor();
        if (hwnd == IntPtr.Zero) return new() { Left = logical.X, Top = logical.Y, Width = logical.Width, Height = logical.Height };

        var topLeft = new UiaNative.Point { X = 0, Y = 0 };
        UiaNative.ClientToScreen(hwnd, ref topLeft);
        uint dpi = UiaNative.GetDpiForWindow(hwnd);
        float scale = dpi > 0 ? dpi / 96f : 1f;
        return new()
        {
            Left = topLeft.X + logical.X * scale,
            Top = topLeft.Y + logical.Y * scale,
            Width = logical.Width * scale,
            Height = logical.Height * scale,
        };
    }

    [DllImport("user32.dll")] private static extern void NotifyWinEvent(uint eventId, IntPtr hwnd, int idObject, int idChild);
}
