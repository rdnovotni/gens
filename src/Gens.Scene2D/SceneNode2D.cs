using System.Numerics;
using Gens.Graphics;

namespace Gens.Scene2D;

public abstract class SceneNode2D
{
    private readonly List<SceneNode2D> children = [];
    private List<SceneNode2D>? orderedChildren;
    private Transform2D transform = Transform2D.Identity;
    private bool visible = true;
    private float opacity = 1;
    private int zOrder;

    public string? Name { get; set; }
    public SceneNode2D? Parent { get; private set; }
    public IReadOnlyList<SceneNode2D> Children => children;
    public Transform2D Transform { get => transform; set { if (transform == value) return; transform = value; Invalidate(); } }
    public bool Visible { get => visible; set { if (visible == value) return; visible = value; Invalidate(); } }
    public float Opacity { get => opacity; set { float next = Math.Clamp(value, 0, 1); if (opacity == next) return; opacity = next; Invalidate(); } }
    public int ZOrder { get => zOrder; set { if (zOrder == value) return; zOrder = value; Parent?.InvalidateOrder(); Invalidate(); } }
    public Matrix3x2 WorldTransform => Parent is null ? Transform.Matrix : Transform.Matrix * Parent.WorldTransform;

    public event Action? Changed;

    public void AddChild(SceneNode2D child)
    {
        ArgumentNullException.ThrowIfNull(child);
        if (child == this || IsDescendantOf(child)) throw new InvalidOperationException("A scene node cannot contain itself or an ancestor.");
        if (child.Parent is not null) throw new InvalidOperationException("A scene node may have only one parent.");
        child.Parent = this;
        child.Changed += ChildChanged;
        children.Add(child);
        orderedChildren = null;
        Invalidate();
    }

    public bool RemoveChild(SceneNode2D child)
    {
        if (!children.Remove(child)) return false;
        child.Changed -= ChildChanged;
        child.Parent = null;
        orderedChildren = null;
        Invalidate();
        return true;
    }

    public Point2 LocalToWorld(Point2 point)
    {
        Vector2 value = Vector2.Transform(new(point.X, point.Y), WorldTransform);
        return new(value.X, value.Y);
    }

    public Point2 WorldToLocal(Point2 point)
    {
        if (!Matrix3x2.Invert(WorldTransform, out Matrix3x2 inverse)) throw new InvalidOperationException("The world transform is not invertible.");
        Vector2 value = Vector2.Transform(new(point.X, point.Y), inverse);
        return new(value.X, value.Y);
    }

    internal void Render(ICanvas2D canvas, float inheritedOpacity, Rect viewport, SceneRenderAccumulator metrics)
    {
        metrics.NodeCount++;
        if (this is Sprite2D or AnimatedSprite2D) metrics.SpriteCount++;
        if (!Visible || Opacity <= 0) return;
        metrics.VisibleNodeCount++;
        float combinedOpacity = inheritedOpacity * Opacity;
        using ICanvasState state = canvas.Save();
        canvas.Concat(Transform.Matrix);
        if (LocalBounds is not Rect bounds || IntersectsViewport(bounds, viewport, WorldTransform))
        {
            RenderSelf(canvas, combinedOpacity);
            metrics.RenderedNodeCount++;
        }
        else metrics.CulledNodeCount++;
        foreach (SceneNode2D child in OrderedChildren) child.Render(canvas, combinedOpacity, viewport, metrics);
    }

    protected virtual Rect? LocalBounds => null;
    protected virtual void RenderSelf(ICanvas2D canvas, float opacity) { }
    protected void Invalidate() => Changed?.Invoke();

    private IReadOnlyList<SceneNode2D> OrderedChildren
    {
        get
        {
            if (orderedChildren is not null) return orderedChildren;
            orderedChildren = [.. children];
            orderedChildren.Sort(static (left, right) => left.ZOrder.CompareTo(right.ZOrder));
            return orderedChildren;
        }
    }

    private void InvalidateOrder() { orderedChildren = null; Invalidate(); }
    private void ChildChanged() => Invalidate();
    private bool IsDescendantOf(SceneNode2D possibleAncestor) { for (SceneNode2D? current = Parent; current is not null; current = current.Parent) if (current == possibleAncestor) return true; return false; }
    private static bool IntersectsViewport(Rect local, Rect viewport, Matrix3x2 world)
    {
        Vector2 a = Vector2.Transform(new(local.X, local.Y), world);
        Vector2 b = Vector2.Transform(new(local.X + local.Width, local.Y), world);
        Vector2 c = Vector2.Transform(new(local.X, local.Y + local.Height), world);
        Vector2 d = Vector2.Transform(new(local.X + local.Width, local.Y + local.Height), world);
        float minX = MathF.Min(MathF.Min(a.X, b.X), MathF.Min(c.X, d.X));
        float maxX = MathF.Max(MathF.Max(a.X, b.X), MathF.Max(c.X, d.X));
        float minY = MathF.Min(MathF.Min(a.Y, b.Y), MathF.Min(c.Y, d.Y));
        float maxY = MathF.Max(MathF.Max(a.Y, b.Y), MathF.Max(c.Y, d.Y));
        return maxX >= viewport.X && minX <= viewport.X + viewport.Width && maxY >= viewport.Y && minY <= viewport.Y + viewport.Height;
    }
}

internal sealed class SceneRenderAccumulator
{
    public int NodeCount;
    public int VisibleNodeCount;
    public int RenderedNodeCount;
    public int CulledNodeCount;
    public int SpriteCount;
    public SceneRenderMetrics Snapshot() => new(NodeCount, VisibleNodeCount, RenderedNodeCount, CulledNodeCount, SpriteCount);
}

public sealed class GroupNode2D : SceneNode2D { }

public sealed class Layer2D(string layerName) : SceneNode2D
{
    public string LayerName { get; } = string.IsNullOrWhiteSpace(layerName) ? throw new ArgumentException("A layer requires a name.", nameof(layerName)) : layerName;
}
