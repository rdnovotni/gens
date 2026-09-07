using Gens.Graphics;
using Gens.Platform;

namespace Gens.UI.Testing;

public sealed class UiTestHost(UiRoot root, Size2 size)
{
    public UiRoot Root { get; } = root;
    public Size2 Size { get; set; } = size;
    public TimeSpan PresentationTime { get; private set; }
    public UiNode Find(string name) => Root.DescendantsAndSelf().FirstOrDefault(n => string.Equals(n.Name, name, StringComparison.Ordinal)) ?? throw new KeyNotFoundException($"UI node '{name}' was not found.");
    public void MovePointer(float x, float y) => Root.MovePointer(new(x, y));
    public void Click(float x, float y) { Root.PressPointer(new(x, y)); Root.ReleasePointer(new(x, y)); }
    public void PressKey(uint scanCode, KeyModifiers modifiers = KeyModifiers.None) => Root.HandleKey(new(new(scanCode), modifiers, false));
    public void Advance(TimeSpan delta) => PresentationTime += delta;
    public void Layout() => Root.Layout(Size);
    public void RenderFrame(ICanvas2D canvas) { Root.Layout(Size); Root.Render(canvas); }
}
