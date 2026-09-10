using System.Diagnostics;
using Gens.Graphics;
using Gens.Platform;

namespace Gens.UI;

public sealed class UiRoot : UiNode
{
    private UiNode? hovered, captured, pressed;
    private UiNode? modal, preModalFocus;
    private bool treeMutationGuard;
    private bool nodeCountValid;
    private float uiScale = 1;
    private Action? invalidate;
    public UiRoot(UiTheme theme) { Theme = theme; Focus = new(this); Name = "UiRoot"; Semantics.Role = AccessibilityRole.Group; }
    public UiTheme Theme { get; private set; }
    public FocusManager Focus { get; }
    public UiDiagnostics Diagnostics { get; } = new();
    public MotionPolicy MotionPolicy { get; set; } = new(MotionMode.Full);
    public float UiScale { get => uiScale; set { if (value <= 0 || !float.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value)); if (uiScale == value) return; uiScale = value; InvalidateMeasure(); } }
    public bool DrawLayoutOutlines { get; set; }
    public UiNode? HoveredNode => hovered;
    public UiNode? CapturedNode => captured;
    public UiNode? Modal => modal;
    public void AttachInvalidation(Action requestFrame) => invalidate = requestFrame;
    public void ApplyTheme(UiTheme theme) { Theme = theme ?? throw new ArgumentNullException(nameof(theme)); InvalidateMeasure(); }
    internal void RequestFrame() => invalidate?.Invoke();

    public void Layout(Size2 logicalSize)
    {
        Diagnostics.BeginFrame(); long started = Stopwatch.GetTimestamp(); treeMutationGuard = true;
        try { Size2 scaled = new(logicalSize.Width / UiScale, logicalSize.Height / UiScale); Measure(scaled); Arrange(new(0, 0, scaled.Width, scaled.Height)); if (!nodeCountValid) { Diagnostics.NodeCount = DescendantsAndSelf().Count(); nodeCountValid = true; } }
        finally { treeMutationGuard = false; Diagnostics.LayoutDuration = Stopwatch.GetElapsedTime(started); }
    }

    public void Render(ICanvas2D canvas)
    {
        long started = Stopwatch.GetTimestamp(); treeMutationGuard = true;
        try { using ICanvasState state = canvas.Save(); canvas.Scale(UiScale, UiScale); Paint(canvas); if (DrawLayoutOutlines) DrawOutlines(canvas, this); }
        finally { treeMutationGuard = false; Diagnostics.PaintDuration = Stopwatch.GetElapsedTime(started); }
    }

    public void HandleEvent(PlatformEvent evt)
    {
        switch (evt)
        {
            case PointerMovedEvent moved: MovePointer(new(moved.Position.X / UiScale, moved.Position.Y / UiScale)); break;
            case PointerButtonEvent button when button.Button == PointerButton.Primary && button.IsDown: PressPointer(new(button.Position.X / UiScale, button.Position.Y / UiScale)); break;
            case PointerButtonEvent button when button.Button == PointerButton.Primary: ReleasePointer(new(button.Position.X / UiScale, button.Position.Y / UiScale)); break;
            case WheelEvent wheel: Route(HitTarget(new(wheel.Position.X / UiScale, wheel.Position.Y / UiScale)), new(UiPointerEventType.Wheel, new(wheel.Position.X / UiScale, wheel.Position.Y / UiScale), delta: wheel.Delta)); break;
            case KeyboardEvent key when key.IsDown: HandleKey(new(key.Key, key.Modifiers, key.IsRepeat)); break;
            case TextInputEvent text: HandleTextInput(text.Text); break;
        }
    }

    public void MovePointer(Point2 position)
    {
        UiNode? hit = HitTarget(position);
        if (hovered != hit)
        {
            UiNode? old = hovered; hovered = hit;
            if (old is not null) { old.IsHovered = false; Route(old, new(UiPointerEventType.Exited, position)); old.InvalidatePaint(); }
            if (hit is not null) { hit.IsHovered = true; Route(hit, new(UiPointerEventType.Entered, position)); hit.InvalidatePaint(); }
        }
        Route(captured ?? hit, new(UiPointerEventType.Moved, position));
    }

    public void PressPointer(Point2 position)
    {
        UiNode? target = HitTarget(position); pressed = target;
        if (target is not null) { target.IsPressed = true; if (target.IsFocusable) Focus.RequestFocus(target); Route(target, new(UiPointerEventType.Pressed, position, PointerButton.Primary)); target.InvalidatePaint(); }
    }

    public void ReleasePointer(Point2 position)
    {
        UiNode? releaseTarget = captured ?? HitTarget(position); UiNode? originalPress = pressed;
        if (releaseTarget is not null) Route(releaseTarget, new(UiPointerEventType.Released, position, PointerButton.Primary));
        if (originalPress is not null) { originalPress.IsPressed = false; originalPress.InvalidatePaint(); if (HitTarget(position) == originalPress) Route(originalPress, new(UiPointerEventType.Clicked, position, PointerButton.Primary)); }
        pressed = null; captured = null;
    }

    public void HandleKey(UiKeyEvent evt)
    {
        const uint tabScanCode = 43;
        if (evt.Key.ScanCode == tabScanCode) { evt.Handled = Focus.Move((evt.Modifiers & (KeyModifiers.LeftShift | KeyModifiers.RightShift)) != 0); return; }
        if (Focus.FocusedNode is UiNode focused)
        {
            try { focused.RaiseKey(evt); }
            catch (Exception ex) { throw new UiInputException($"Keyboard handler failed for {focused.DebugPath}.", ex); }
        }
    }

    public void HandleTextInput(string text) { if (Focus.FocusedNode is UiNode focused) { try { focused.RaiseTextInput(text); } catch (Exception ex) { throw new UiInputException($"Text input handler failed for {focused.DebugPath}.", ex); } } }
    public void CapturePointer(UiNode node) { if (node.Root != this) throw new InvalidOperationException("Pointer capture target must belong to this root."); captured = node; }
    public void ReleasePointerCapture(UiNode node) { if (captured == node) captured = null; }
    public void ShowModal(UiNode node)
    {
        if (modal is not null) throw new InvalidOperationException("Only one modal is supported by this foundation.");
        preModalFocus = Focus.FocusedNode; modal = node; node.ZIndex = int.MaxValue; AddChild(node); Focus.Clear(); Focus.Move(false);
    }
    public void CloseModal()
    {
        if (modal is null) return; UiNode closing = modal; modal = null; RemoveChild(closing);
        if (!Focus.RequestFocus(preModalFocus)) Focus.Move(false); preModalFocus = null;
    }
    public SemanticTreeSnapshot CaptureSemantics()
    {
        UiNode scope = modal ?? this;
        SemanticNodeSnapshot root = Snapshot(scope);
        return new(root, FindFocused(root));
    }
    public IReadOnlyList<string> ValidateSemantics()
    {
        var errors = new List<string>();
        foreach (UiNode node in Traverse(modal ?? this))
        {
            if (node.Semantics.IsDecorative) continue;
            bool interactive = node.IsFocusable || node.Semantics.Role is AccessibilityRole.Button or AccessibilityRole.CheckBox or AccessibilityRole.Toggle;
            if (interactive && string.IsNullOrWhiteSpace(node.Semantics.Label)) errors.Add($"{node.DebugPath}: interactive node has no accessible name.");
            if (interactive && node.Semantics.Role == AccessibilityRole.None) errors.Add($"{node.DebugPath}: interactive node has no accessibility role.");
        }
        return errors;
    }
    internal void ValidateFocus() { if (Focus.FocusedNode is UiNode focused && (!focused.IsEnabled || focused.Visibility != UiVisibility.Visible || focused.Root != this || !IsInActiveFocusScope(focused))) Focus.Clear(); }
    internal bool IsInActiveFocusScope(UiNode node) => modal is null || node == modal || node.IsDescendantOf(modal);
    internal IEnumerable<UiNode> EnumerateActiveScope() => Traverse(modal ?? this);
    internal IEnumerable<UiNode> DescendantsAndSelf() => Traverse(this);
    internal void OnSubtreeDetached(UiNode node) { if (Focus.FocusedNode is UiNode focused && (focused == node || focused.IsDescendantOf(node))) Focus.Clear(); if (captured == node || captured?.IsDescendantOf(node) == true) captured = null; if (hovered == node || hovered?.IsDescendantOf(node) == true) hovered = null; }
    internal void OnTreeChanged() => nodeCountValid = false;
    internal void AssertTreeMutationAllowed() { if (treeMutationGuard) throw new InvalidOperationException("The UI tree cannot be mutated during measure, arrange, or paint."); }
    private UiNode? HitTarget(Point2 position) { UiNode scope = modal ?? this; return scope.HitTest(position); }
    private static IEnumerable<UiNode> Traverse(UiNode start) { var stack = new Stack<UiNode>(); stack.Push(start); while (stack.TryPop(out UiNode? node)) { yield return node; for (int i = node.Children.Count - 1; i >= 0; i--) stack.Push(node.Children[i]); } }
    private static SemanticNodeSnapshot Snapshot(UiNode node)
    {
        IReadOnlyList<SemanticNodeSnapshot> children = node.Children.Where(static child => !child.Semantics.IsDecorative && child.Visibility == UiVisibility.Visible).Select(Snapshot).ToArray();
        return new(node.Name ?? node.GetType().Name, node.Semantics.Role, node.Semantics.Label, node.Semantics.Description, node.Semantics.Value, node.IsEnabled, node.IsFocused, node.Semantics.IsChecked, node.Bounds, children);
    }
    private static SemanticNodeSnapshot? FindFocused(SemanticNodeSnapshot node) => node.IsFocused ? node : node.Children.Select(FindFocused).FirstOrDefault(static candidate => candidate is not null);
    private static void Route(UiNode? target, UiPointerEvent evt)
    {
        if (target is null) return; evt.OriginalTarget = target;
        foreach (UiNode node in target.AncestorsAndSelf()) { try { node.RaisePointer(evt); } catch (Exception ex) { throw new UiInputException($"{evt.Type} handler failed for {node.DebugPath} (target {target.DebugPath}).", ex); } if (evt.Handled) break; }
    }
    private static void DrawOutlines(ICanvas2D canvas, UiNode node)
    {
        DrawRect(canvas, node.Bounds, new(210, 65, 50));
        if (node.ContentBounds != node.Bounds) DrawRect(canvas, node.ContentBounds, new(50, 130, 210));
        if (node.ClipToBounds) DrawRect(canvas, node.Bounds, new(55, 175, 95));
        foreach (UiNode child in node.Children) DrawOutlines(canvas, child);
    }
    private static void DrawRect(ICanvas2D canvas, Rect rect, Color color) { canvas.DrawLine(new(rect.X, rect.Y), new(rect.X + rect.Width, rect.Y), new(color, 1)); canvas.DrawLine(new(rect.X + rect.Width, rect.Y), new(rect.X + rect.Width, rect.Y + rect.Height), new(color, 1)); canvas.DrawLine(new(rect.X + rect.Width, rect.Y + rect.Height), new(rect.X, rect.Y + rect.Height), new(color, 1)); canvas.DrawLine(new(rect.X, rect.Y + rect.Height), new(rect.X, rect.Y), new(color, 1)); }
}
