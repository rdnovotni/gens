using Gens.Graphics;
using Gens.UI;

namespace Gens.Accessibility.Windows.Uia;

/// <summary>
/// Wraps one node of the latest published <see cref="SemanticTreeSnapshot"/> as a UIA fragment provider.
/// Resolves against the *current* index/snapshot on every call (via the injected delegates) rather than
/// caching state, since UIA can hold a provider reference across several <c>Publish</c> calls.
/// </summary>
/// <remarks>
/// <see cref="Invoke"/> and <see cref="Toggle"/> are intentionally no-ops for now: routing a UIA-originated
/// action back into a live command against <c>UiRoot</c> would require broadening
/// <c>IAccessibilityBridge.Publish</c> beyond a read-only snapshot, which is out of scope for this pass (see
/// docs/engineering/accessibility.md). Role, name, state, tree navigation, bounds, and focus are fully real.
/// </remarks>
internal class UiaFragmentProvider : IRawElementProviderSimple, IRawElementProviderFragment, IInvokeProvider, IToggleProvider, IScrollProvider
{
    private readonly Func<SemanticFragmentIndex, SemanticNodeSnapshot?> selectNode;
    private readonly UiaFragmentRootProvider? root;

    internal UiaFragmentProvider(
        Func<SemanticFragmentIndex?> resolveIndex,
        Func<SemanticFragmentIndex, SemanticNodeSnapshot?> selectNode,
        Func<Rect, UiaRect> logicalToScreen,
        UiaFragmentRootProvider? root)
    {
        ResolveIndex = resolveIndex;
        this.selectNode = selectNode;
        LogicalToScreen = logicalToScreen;
        this.root = root;
    }

    protected UiaFragmentRootProvider RootProvider => root ?? (UiaFragmentRootProvider)this;
    protected Func<SemanticFragmentIndex?> ResolveIndex { get; }
    protected Func<Rect, UiaRect> LogicalToScreen { get; }

    /// <summary>The current index and the node this provider designates within it, resolved together so callers
    /// can derive the node's structural key (<see cref="SemanticFragmentIndex.KeyOf"/>) without a second,
    /// possibly-inconsistent resolution pass.</summary>
    private (SemanticFragmentIndex Index, SemanticNodeSnapshot Node)? Resolved
    {
        get
        {
            SemanticFragmentIndex? index = ResolveIndex();
            if (index is null) return null;
            SemanticNodeSnapshot? node = selectNode(index);
            return node is null ? null : (index, node);
        }
    }

    private SemanticNodeSnapshot? Node => Resolved?.Node;

    public UiaProviderOptions ProviderOptions => UiaProviderOptions.ServerSideProvider;

    public object? GetPatternProvider(int patternId)
    {
        SemanticNodeSnapshot? node = Node;
        if (node is null) return null;
        if (patternId == UiaConstants.InvokePatternId && UiaRoleMapping.SupportsInvoke(node.Role)) return this;
        if (patternId == UiaConstants.TogglePatternId && UiaRoleMapping.SupportsToggle(node.Role)) return this;
        if (patternId == UiaConstants.ScrollPatternId && UiaRoleMapping.SupportsScroll(node.Role)) return this;
        return null;
    }

    public object? GetPropertyValue(int propertyId)
    {
        (SemanticFragmentIndex Index, SemanticNodeSnapshot Node)? resolved = Resolved;
        if (resolved is null) return null;
        (SemanticFragmentIndex index, SemanticNodeSnapshot node) = resolved.Value;
        return propertyId switch
        {
            UiaConstants.NamePropertyId => (object?)(node.Name ?? node.Id),
            UiaConstants.ControlTypePropertyId => UiaRoleMapping.ControlTypeFor(node.Role),
            UiaConstants.IsEnabledPropertyId => node.IsEnabled,
            UiaConstants.HasKeyboardFocusPropertyId => node.IsFocused,
            UiaConstants.IsKeyboardFocusablePropertyId => node.IsFocused || UiaRoleMapping.SupportsInvoke(node.Role) || UiaRoleMapping.SupportsToggle(node.Role),
            UiaConstants.AutomationIdPropertyId => index.KeyOf(node),
            UiaConstants.IsContentElementPropertyId => true,
            UiaConstants.IsControlElementPropertyId => true,
            _ => null,
        };
    }

    public IRawElementProviderSimple? HostRawElementProvider => null;

    public IRawElementProviderFragment? Navigate(NavigateDirection direction)
    {
        (SemanticFragmentIndex Index, SemanticNodeSnapshot Node)? resolved = Resolved;
        if (resolved is null) return null;
        (SemanticFragmentIndex index, SemanticNodeSnapshot current) = resolved.Value;
        string currentKey = index.KeyOf(current);
        SemanticNodeSnapshot? target = direction switch
        {
            NavigateDirection.Parent => index.Parent(currentKey),
            NavigateDirection.NextSibling => index.NextSibling(currentKey),
            NavigateDirection.PreviousSibling => index.PreviousSibling(currentKey),
            NavigateDirection.FirstChild => index.FirstChild(currentKey),
            NavigateDirection.LastChild => index.LastChild(currentKey),
            _ => null,
        };
        if (target is null) return null;
        if (ReferenceEquals(target, index.Root)) return RootProvider;
        string targetKey = index.KeyOf(target);
        return new UiaFragmentProvider(ResolveIndex, idx => idx.Find(targetKey), LogicalToScreen, RootProvider);
    }

    // The leading "3" is the documented UiaAppendRuntimeId marker; the rest need only be stable and unique
    // enough for this-session equality checks. The structural key (not SemanticNodeSnapshot.Id, which is
    // routinely duplicated by controls that fall back to their type name) guarantees per-node uniqueness.
    public int[]? GetRuntimeId() => Resolved is { } resolved ? [3, resolved.Index.KeyOf(resolved.Node).GetHashCode(StringComparison.Ordinal)] : null;

    public UiaRect BoundingRectangle => Node is { } node ? LogicalToScreen(node.Bounds) : default;

    public object[]? GetEmbeddedFragmentRoots() => null;

    public void SetFocus() { }

    public IRawElementProviderFragmentRoot? FragmentRoot => RootProvider;

    public void Invoke() { }

    public void Toggle() { }
    public UiaToggleState ToggleState => Node?.IsChecked == true ? UiaToggleState.On : UiaToggleState.Off;

    // Gens.UI's ScrollView does not yet expose scroll offset/extent through the semantic snapshot, so this
    // reports a non-interactive, fully-visible view rather than fabricating numbers.
    public void Scroll(UiaScrollAmount horizontalAmount, UiaScrollAmount verticalAmount) { }
    public void SetScrollPercent(double horizontalPercent, double verticalPercent) { }
    public double HorizontalScrollPercent => 0;
    public double VerticalScrollPercent => 0;
    public double HorizontalViewSize => 100;
    public double VerticalViewSize => 100;
    public bool HorizontallyScrollable => false;
    public bool VerticallyScrollable => false;

    internal SemanticNodeSnapshot? NodeForTesting => Node;
}
