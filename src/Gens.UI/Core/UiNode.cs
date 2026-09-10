using Gens.Graphics;

namespace Gens.UI;

public abstract class UiNode
{
    private readonly List<UiNode> children = [];
    private List<UiNode>? orderedChildren;
    private UiVisibility visibility = UiVisibility.Visible;
    private bool isEnabled = true;
    private Thickness margin;
    private float? width, height;
    private float minWidth, minHeight;
    private float maxWidth = float.PositiveInfinity, maxHeight = float.PositiveInfinity;

    public string? Name { get; set; }
    public UiNode? Parent { get; private set; }
    public IReadOnlyList<UiNode> Children => children;
    public UiRoot? Root => this as UiRoot ?? Parent?.Root;
    public UiSemantics Semantics { get; } = new();
    public Size2 DesiredSize { get; private set; }
    public Rect Bounds { get; private set; }
    public Rect ContentBounds { get; protected set; }
    public UiVisibility Visibility { get => visibility; set { if (visibility == value) return; visibility = value; InvalidateMeasure(); Root?.ValidateFocus(); } }
    public bool IsEnabled { get => isEnabled; set { if (isEnabled == value) return; isEnabled = value; InvalidatePaint(); Root?.ValidateFocus(); } }
    public bool IsFocusable { get; set; }
    public bool IsHitTestVisible { get; set; } = true;
    public bool ClipToBounds { get; set; }
    public float Opacity { get; set; } = 1;
    private int zIndex;
    public int ZIndex { get => zIndex; set { if (zIndex == value) return; zIndex = value; Parent?.InvalidateChildOrder(); InvalidatePaint(); } }
    public Thickness Margin { get => margin; set { if (margin == value) return; margin = value; InvalidateMeasure(); } }
    public float? Width { get => width; set { if (width == value) return; width = value; InvalidateMeasure(); } }
    public float? Height { get => height; set { if (height == value) return; height = value; InvalidateMeasure(); } }
    public float MinWidth { get => minWidth; set { minWidth = value; InvalidateMeasure(); } }
    public float MinHeight { get => minHeight; set { minHeight = value; InvalidateMeasure(); } }
    public float MaxWidth { get => maxWidth; set { maxWidth = value; InvalidateMeasure(); } }
    public float MaxHeight { get => maxHeight; set { maxHeight = value; InvalidateMeasure(); } }
    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Stretch;
    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Stretch;
    public bool IsMeasureValid { get; private set; }
    public bool IsArrangeValid { get; private set; }
    public bool IsPaintValid { get; private set; }
    public bool IsHovered { get; internal set; }
    public bool IsPressed { get; internal set; }
    public bool IsFocused => Root?.Focus.FocusedNode == this;

    public event Action<UiPointerEvent>? PointerEvent;
    public event Action<UiKeyEvent>? KeyEvent;

    public void AddChild(UiNode child)
    {
        ArgumentNullException.ThrowIfNull(child);
        if (child == this || IsDescendantOf(child)) throw new InvalidOperationException("A UI node cannot contain itself or an ancestor.");
        if (child.Parent is not null) throw new InvalidOperationException("A UI node may have only one parent; remove it before reparenting.");
        Root?.AssertTreeMutationAllowed();
        child.Parent = this; children.Add(child); orderedChildren = null; Root?.OnTreeChanged(); InvalidateMeasure();
    }

    public bool RemoveChild(UiNode child)
    {
        Root?.AssertTreeMutationAllowed();
        if (!children.Remove(child)) return false;
        orderedChildren = null;
        UiRoot? root = Root; root?.OnSubtreeDetached(child); child.Parent = null; root?.OnTreeChanged(); InvalidateMeasure(); return true;
    }

    public void ClearChildren()
    {
        foreach (UiNode child in children.ToArray()) RemoveChild(child);
    }

    public void InvalidateMeasure()
    {
        if (!IsMeasureValid && !IsArrangeValid && !IsPaintValid) return;
        IsMeasureValid = IsArrangeValid = IsPaintValid = false;
        Parent?.InvalidateMeasure(); Root?.RequestFrame();
    }

    public void InvalidateArrange()
    {
        if (!IsArrangeValid && !IsPaintValid) return;
        IsArrangeValid = IsPaintValid = false; Parent?.InvalidateArrange(); Root?.RequestFrame();
    }

    public void InvalidatePaint()
    {
        if (!IsPaintValid) return;
        IsPaintValid = false; Root?.RequestFrame();
    }

    public void Measure(Size2 availableSize)
    {
        if (Visibility == UiVisibility.Collapsed) { DesiredSize = default; IsMeasureValid = true; return; }
        if (IsMeasureValid && availableSize == LastMeasureConstraint) return;
        Root?.Diagnostics.IncrementMeasure();
        LastMeasureConstraint = availableSize;
        Size2 inner = availableSize.Deflate(Margin);
        if (Width is float w) inner = inner with { Width = Math.Min(inner.Width, w) };
        if (Height is float h) inner = inner with { Height = Math.Min(inner.Height, h) };
        Size2 measured = MeasureOverride(inner);
        if (Width is float explicitWidth) measured = measured with { Width = explicitWidth };
        if (Height is float explicitHeight) measured = measured with { Height = explicitHeight };
        DesiredSize = measured.Constrain(MinWidth, MinHeight, MaxWidth, MaxHeight).Inflate(Margin);
        IsMeasureValid = true;
    }

    public void Arrange(Rect finalRect)
    {
        if (Visibility == UiVisibility.Collapsed) { Bounds = ContentBounds = default; IsArrangeValid = true; return; }
        if (IsArrangeValid && finalRect == LastArrangeRect) return;
        Root?.Diagnostics.IncrementArrange(); LastArrangeRect = finalRect;
        Rect slot = finalRect.Deflate(Margin);
        float desiredWidth = Math.Max(0, DesiredSize.Width - Margin.Horizontal), desiredHeight = Math.Max(0, DesiredSize.Height - Margin.Vertical);
        float actualWidth = Width is not null ? Math.Min(slot.Width, desiredWidth) : HorizontalAlignment == HorizontalAlignment.Stretch ? slot.Width : Math.Min(slot.Width, desiredWidth);
        float actualHeight = Height is not null ? Math.Min(slot.Height, desiredHeight) : VerticalAlignment == VerticalAlignment.Stretch ? slot.Height : Math.Min(slot.Height, desiredHeight);
        float x = HorizontalAlignment switch { HorizontalAlignment.Center => slot.X + (slot.Width - actualWidth) / 2, HorizontalAlignment.End => slot.X + slot.Width - actualWidth, _ => slot.X };
        float y = VerticalAlignment switch { VerticalAlignment.Center => slot.Y + (slot.Height - actualHeight) / 2, VerticalAlignment.End => slot.Y + slot.Height - actualHeight, _ => slot.Y };
        Bounds = new(x, y, actualWidth, actualHeight); ContentBounds = Bounds;
        ArrangeOverride(Bounds); IsArrangeValid = true; IsPaintValid = false;
    }

    public void Paint(ICanvas2D canvas)
    {
        if (Visibility != UiVisibility.Visible || Opacity <= 0) return;
        Root?.Diagnostics.IncrementPaint();
        if (ClipToBounds)
        {
            using ICanvasState state = canvas.Save();
            canvas.ClipRect(Bounds);
            PaintTree(canvas);
            return;
        }
        PaintTree(canvas);
    }

    private void PaintTree(ICanvas2D canvas)
    {
        PaintOverride(canvas);
        foreach (UiNode child in OrderedChildren) child.Paint(canvas);
        IsPaintValid = true;
    }

    public UiNode? HitTest(Point2 position)
    {
        Root?.Diagnostics.IncrementHitTest();
        if (Visibility != UiVisibility.Visible || !IsEnabled || !IsHitTestVisible || (ClipToBounds && !Bounds.Contains(position))) return null;
        IReadOnlyList<UiNode> ordered = OrderedChildren;
        for (int i = ordered.Count - 1; i >= 0; i--)
        {
            UiNode? hit = ordered[i].HitTest(position); if (hit is not null) return hit;
        }
        return Bounds.Contains(position) && IsEnabled ? this : null;
    }

    internal void RaisePointer(UiPointerEvent evt) { evt.CurrentTarget = this; OnPointerEvent(evt); PointerEvent?.Invoke(evt); }
    internal void RaiseKey(UiKeyEvent evt) { OnKeyEvent(evt); KeyEvent?.Invoke(evt); }
    internal void RaiseTextInput(string text) { if (IsEnabled) OnTextInput(text); }
    internal IEnumerable<UiNode> AncestorsAndSelf() { for (UiNode? current = this; current is not null; current = current.Parent) yield return current; }
    internal bool IsDescendantOf(UiNode possibleAncestor) { for (UiNode? current = Parent; current is not null; current = current.Parent) if (current == possibleAncestor) return true; return false; }
    internal string DebugPath => string.Join("/", AncestorsAndSelf().Reverse().Select(static n => n.Name ?? n.GetType().Name));

    protected virtual Size2 MeasureOverride(Size2 availableSize)
    {
        float width = 0, height = 0;
        foreach (UiNode child in children) { child.Measure(availableSize); width = Math.Max(width, child.DesiredSize.Width); height = Math.Max(height, child.DesiredSize.Height); }
        return new(width, height);
    }
    protected virtual void ArrangeOverride(Rect finalRect) { foreach (UiNode child in children) child.Arrange(finalRect); }
    protected virtual void PaintOverride(ICanvas2D canvas) { }
    protected virtual void OnPointerEvent(UiPointerEvent evt) { }
    protected virtual void OnKeyEvent(UiKeyEvent evt) { }
    protected virtual void OnTextInput(string text) { }
    private Size2 LastMeasureConstraint { get; set; }
    private Rect LastArrangeRect { get; set; }
    private IReadOnlyList<UiNode> OrderedChildren
    {
        get
        {
            if (orderedChildren is not null) return orderedChildren;
            orderedChildren = [.. children];
            orderedChildren.Sort(static (left, right) => left.ZIndex.CompareTo(right.ZIndex));
            return orderedChildren;
        }
    }
    private void InvalidateChildOrder() { orderedChildren = null; InvalidatePaint(); }
}
