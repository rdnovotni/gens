using System.Globalization;
using System.Text;

namespace Gens.Scene2D;

public readonly record struct SceneRenderMetrics(
    int NodeCount,
    int VisibleNodeCount,
    int RenderedNodeCount,
    int CulledNodeCount,
    int SpriteCount);

public static class SceneInspector
{
    public static string Describe(Scene2D scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        SceneRenderMetrics m = scene.LastRenderMetrics;
        return string.Create(CultureInfo.InvariantCulture,
            $"scene nodes={m.NodeCount} visible={m.VisibleNodeCount} rendered={m.RenderedNodeCount} culled={m.CulledNodeCount} sprites={m.SpriteCount} camera=({scene.Camera.Position.X:F1},{scene.Camera.Position.Y:F1}) zoom={scene.Camera.Zoom:F2} activeAnimations={scene.HasActiveAnimations}");
    }

    public static string Tree(Scene2D scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        var text = new StringBuilder("Scene");
        AppendChildren(text, scene.Root, string.Empty);
        return text.ToString();
    }

    private static void AppendChildren(StringBuilder text, SceneNode2D node, string prefix)
    {
        for (int i = 0; i < node.Children.Count; i++)
        {
            SceneNode2D child = node.Children[i];
            bool last = i == node.Children.Count - 1;
            text.AppendLine().Append(prefix).Append(last ? "└─ " : "├─ ")
                .Append(child.Name ?? child.GetType().Name).Append(" [z=").Append(child.ZOrder).Append(']');
            AppendChildren(text, child, prefix + (last ? "   " : "│  "));
        }
    }
}
