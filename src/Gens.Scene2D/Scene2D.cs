using Gens.Graphics;

namespace Gens.Scene2D;

public sealed class Scene2D
{
    private readonly List<AnimationPlayer> players = [];
    public Scene2D() { Root.Changed += () => Invalidated?.Invoke(); Camera.Changed += () => Invalidated?.Invoke(); }
    public GroupNode2D Root { get; } = new() { Name = "Root" };
    public Camera2D Camera { get; } = new();
    public SceneRenderMetrics LastRenderMetrics { get; private set; }
    public bool HasActiveAnimations => players.Any(static player => player.IsPlaying) || Nodes(Root).OfType<AnimatedSprite2D>().Any(static sprite => sprite.IsPlaying);
    public event Action? Invalidated;
    public event Action<bool>? AnimationStateChanged;

    public Layer2D AddLayer(string name, int zOrder)
    {
        var layer = new Layer2D(name) { Name = name, ZOrder = zOrder };
        Root.AddChild(layer);
        return layer;
    }

    public void AddAnimation(AnimationPlayer player)
    {
        ArgumentNullException.ThrowIfNull(player);
        if (players.Contains(player)) return;
        players.Add(player);
        player.StateChanged += OnAnimationStateChanged;
    }

    public void Update(TimeSpan delta)
    {
        foreach (AnimationPlayer player in players) if (player.IsPlaying) player.Update(delta);
        foreach (AnimatedSprite2D sprite in Nodes(Root).OfType<AnimatedSprite2D>()) if (sprite.IsPlaying) sprite.Update(delta);
        if (HasActiveAnimations) Invalidated?.Invoke();
    }

    public void Render(ICanvas2D canvas, Rect viewport)
    {
        Camera.Viewport = viewport;
        using ICanvasState state = canvas.Save();
        canvas.ClipRect(viewport);
        canvas.Concat(Camera.ViewMatrix);
        Rect worldViewport = WorldViewport(viewport);
        var metrics = new SceneRenderAccumulator();
        Root.Render(canvas, 1, worldViewport, metrics);
        LastRenderMetrics = metrics.Snapshot();
    }

    private Rect WorldViewport(Rect viewport)
    {
        Point2 a = Camera.ScreenToWorld(new(viewport.X, viewport.Y));
        Point2 b = Camera.ScreenToWorld(new(viewport.X + viewport.Width, viewport.Y + viewport.Height));
        return new(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Abs(b.X - a.X), Math.Abs(b.Y - a.Y));
    }

    private void OnAnimationStateChanged() { AnimationStateChanged?.Invoke(HasActiveAnimations); Invalidated?.Invoke(); }
    private static IEnumerable<SceneNode2D> Nodes(SceneNode2D node) { yield return node; foreach (SceneNode2D child in node.Children) foreach (SceneNode2D descendant in Nodes(child)) yield return descendant; }
}
