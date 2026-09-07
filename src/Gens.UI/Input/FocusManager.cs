namespace Gens.UI;

public sealed class FocusManager(UiRoot root)
{
    public UiNode? FocusedNode { get; private set; }
    public bool RequestFocus(UiNode? node)
    {
        if (node is not null && (node.Root != root || !node.IsFocusable || !node.IsEnabled || node.Visibility != UiVisibility.Visible || !root.IsInActiveFocusScope(node))) return false;
        if (FocusedNode == node) return true;
        UiNode? previous = FocusedNode; FocusedNode = node; previous?.InvalidatePaint(); node?.InvalidatePaint(); return true;
    }
    public void Clear() => RequestFocus(null);
    public bool Move(bool reverse)
    {
        List<UiNode> candidates = root.EnumerateActiveScope().Where(static n => n.IsFocusable && n.IsEnabled && n.Visibility == UiVisibility.Visible).ToList();
        if (candidates.Count == 0) return false;
        int current = FocusedNode is null ? -1 : candidates.IndexOf(FocusedNode);
        int next = reverse ? (current <= 0 ? candidates.Count - 1 : current - 1) : (current + 1) % candidates.Count;
        return RequestFocus(candidates[next]);
    }
}
