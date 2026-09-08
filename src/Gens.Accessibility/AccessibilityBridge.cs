using Gens.UI;

namespace Gens.Accessibility;

public interface IAccessibilityBridge : IDisposable
{
    string Name { get; }
    bool IsAvailable { get; }
    void Publish(SemanticTreeSnapshot snapshot);
    void FocusChanged(SemanticNodeSnapshot? node);
}

public sealed class NullAccessibilityBridge(string name = "No native accessibility bridge") : IAccessibilityBridge
{
    public string Name { get; } = name;
    public bool IsAvailable => false;
    public void Publish(SemanticTreeSnapshot snapshot) { }
    public void FocusChanged(SemanticNodeSnapshot? node) { }
    public void Dispose() { }
}

public sealed class AccessibilityCoordinator(IAccessibilityBridge bridge)
{
    private string? lastFocus;
    public void Synchronize(UiRoot root)
    {
        SemanticTreeSnapshot snapshot = root.CaptureSemantics(); bridge.Publish(snapshot);
        string? focus = snapshot.FocusedNode?.Id;
        if (!string.Equals(focus, lastFocus, StringComparison.Ordinal)) { bridge.FocusChanged(snapshot.FocusedNode); lastFocus = focus; }
    }
}
