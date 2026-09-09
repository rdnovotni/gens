using Gens.Graphics;
using Gens.UI;

namespace Gens.Accessibility.Windows.Uia;

/// <summary>The fragment provider for the window root, additionally implementing <see cref="IRawElementProviderFragmentRoot"/> and owning the native window handle used for bounds conversion and UIA hosting.</summary>
internal sealed class UiaFragmentRootProvider : UiaFragmentProvider, IRawElementProviderFragmentRoot
{
    private readonly Func<IntPtr> hwndAccessor;

    internal UiaFragmentRootProvider(Func<SemanticFragmentIndex?> resolveIndex, Func<Rect, UiaRect> logicalToScreen, Func<IntPtr> hwndAccessor)
        : base(resolveIndex, static index => index.Root, logicalToScreen, root: null)
    {
        this.hwndAccessor = hwndAccessor;
    }

    internal IntPtr Hwnd => hwndAccessor();

    /// <summary>Point-based hit-testing is not implemented; UIA falls back to bounding-rectangle intersection for most clients. Tracked as a follow-up alongside full screen-reader certification.</summary>
    public IRawElementProviderFragment? ElementProviderFromPoint(double x, double y) => this;

    public IRawElementProviderFragment? GetFocus()
    {
        SemanticFragmentIndex? index = ResolveIndex();
        SemanticNodeSnapshot? focused = index?.Root is null ? null : FindFocused(index.Root);
        if (focused is null || index is null) return null;
        if (ReferenceEquals(focused, index.Root)) return this;
        string focusedKey = index.KeyOf(focused);
        return new UiaFragmentProvider(ResolveIndex, idx => idx.Find(focusedKey), LogicalToScreen, this);
    }

    private static SemanticNodeSnapshot? FindFocused(SemanticNodeSnapshot node)
    {
        if (node.IsFocused) return node;
        foreach (SemanticNodeSnapshot child in node.Children)
        {
            SemanticNodeSnapshot? found = FindFocused(child);
            if (found is not null) return found;
        }
        return null;
    }
}
