using Gens.Graphics;
using Gens.Scene2D;

namespace Gens.UI;

/// <summary>Hosts a presentation-only scene inside retained UI layout and clipping.</summary>
public sealed class SceneView : UiNode
{
    private Scene2D.Scene2D? scene;
    public SceneView() { ClipToBounds = true; Semantics.Role = AccessibilityRole.Image; }
    public Scene2D.Scene2D? Scene
    {
        get => scene;
        set
        {
            if (scene == value) return;
            if (scene is not null) scene.Invalidated -= OnSceneInvalidated;
            scene = value;
            if (scene is not null) scene.Invalidated += OnSceneInvalidated;
            InvalidatePaint();
        }
    }

    public bool HasActiveAnimations => Scene?.HasActiveAnimations == true;
    public Point2 PointerToWorld(Point2 screenPosition) => Scene?.Camera.ScreenToWorld(screenPosition) ?? screenPosition;
    public void Advance(TimeSpan delta) { Scene?.Update(delta); }
    protected override Size2 MeasureOverride(Size2 availableSize) => new(Width ?? availableSize.Width, Height ?? availableSize.Height);
    protected override void PaintOverride(ICanvas2D canvas) => Scene?.Render(canvas, Bounds);
    private void OnSceneInvalidated() => InvalidatePaint();
}
