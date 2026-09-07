using System.Diagnostics;

namespace Gens.UI;

public sealed class UiDiagnostics
{
    public int NodeCount { get; internal set; }
    public int MeasureCount { get; private set; }
    public int ArrangeCount { get; private set; }
    public int PaintNodeCount { get; private set; }
    public int HitTestCount { get; private set; }
    public TimeSpan LayoutDuration { get; internal set; }
    public TimeSpan PaintDuration { get; internal set; }
    internal void BeginFrame() { MeasureCount = ArrangeCount = PaintNodeCount = HitTestCount = 0; LayoutDuration = PaintDuration = default; }
    internal void IncrementMeasure() => MeasureCount++;
    internal void IncrementArrange() => ArrangeCount++;
    internal void IncrementPaint() => PaintNodeCount++;
    internal void IncrementHitTest() => HitTestCount++;
}

public sealed record UiInspection(
    string Type, string? Name, Gens.Graphics.Rect Bounds, Gens.Graphics.Rect ContentBounds, Gens.Graphics.Rect? ClipBounds, Gens.Graphics.Size2 DesiredSize,
    UiVisibility Visibility, bool Enabled, bool Focused, bool Hovered, bool Pressed,
    bool MeasureValid, bool ArrangeValid, bool PaintValid, int ChildrenCount);

public static class UiInspector
{
    public static UiInspection Inspect(UiNode node) => new(node.GetType().Name, node.Name, node.Bounds, node.ContentBounds, node.ClipToBounds ? node.Bounds : null, node.DesiredSize, node.Visibility, node.IsEnabled, node.IsFocused, node.IsHovered, node.IsPressed, node.IsMeasureValid, node.IsArrangeValid, node.IsPaintValid, node.Children.Count);
    public static string Tree(UiNode root) { var writer = new StringWriter(); Append(root, string.Empty, true); return writer.ToString(); void Append(UiNode node, string indent, bool last) { writer.Write(indent); writer.Write(last ? "└─ " : "├─ "); string label = node.Name is null ? node.GetType().Name : $"{node.GetType().Name} ({node.Name})"; writer.WriteLine($"{label} [{node.Bounds.X:F0},{node.Bounds.Y:F0} {node.Bounds.Width:F0}×{node.Bounds.Height:F0}]"); for (int i = 0; i < node.Children.Count; i++) Append(node.Children[i], indent + (last ? "   " : "│  "), i == node.Children.Count - 1); } }
}
