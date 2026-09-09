using System.Globalization;
using Gens.UI;

namespace Gens.Accessibility.Windows.Uia;

/// <summary>
/// Adds parent/sibling navigation over an immutable <see cref="SemanticTreeSnapshot"/>, which only carries
/// child links. Rebuilt once per <c>Publish(snapshot)</c> call. No COM types — fully unit-testable.
/// </summary>
/// <remarks>
/// Nodes are indexed by a structural path key (child-index chain from the root), not by
/// <see cref="SemanticNodeSnapshot.Id"/>: <c>UiRoot.Snapshot</c> falls back to a control's type name (e.g.
/// "Button", "TextBlock") when no explicit name is set, so <c>Id</c> is routinely duplicated across sibling
/// controls in real Gens screens. Indexing by <c>Id</c> would silently drop all but the last node sharing an
/// id, corrupting navigation, bounds, and focus resolution for every node after the first collision.
/// </remarks>
internal sealed class SemanticFragmentIndex
{
    private const string RootKey = "$";

    private readonly Dictionary<string, SemanticNodeSnapshot> byKey = [];
    private readonly Dictionary<SemanticNodeSnapshot, string> keyOf = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<string, string?> parentKeyOf = [];
    private readonly Dictionary<string, int> indexInParent = [];

    internal SemanticFragmentIndex(SemanticTreeSnapshot snapshot)
    {
        Root = snapshot.Root;
        Index(snapshot.Root, parentKey: null, key: RootKey);
    }

    internal SemanticNodeSnapshot Root { get; }

    /// <summary>The stable navigation key for a node from *this* index's snapshot (reference identity, not <c>Id</c>).</summary>
    internal string KeyOf(SemanticNodeSnapshot node) => keyOf.TryGetValue(node, out string? key) ? key : RootKey;

    internal SemanticNodeSnapshot? Find(string key) => byKey.GetValueOrDefault(key);
    internal SemanticNodeSnapshot? Parent(string key) => parentKeyOf.TryGetValue(key, out string? parentKey) && parentKey is not null ? byKey.GetValueOrDefault(parentKey) : null;
    internal SemanticNodeSnapshot? FirstChild(string key) => byKey.TryGetValue(key, out SemanticNodeSnapshot? node) && node.Children.Count > 0 ? node.Children[0] : null;
    internal SemanticNodeSnapshot? LastChild(string key) => byKey.TryGetValue(key, out SemanticNodeSnapshot? node) && node.Children.Count > 0 ? node.Children[^1] : null;

    internal SemanticNodeSnapshot? NextSibling(string key)
    {
        SemanticNodeSnapshot? parent = Parent(key);
        if (parent is null || !indexInParent.TryGetValue(key, out int index)) return null;
        int next = index + 1;
        return next < parent.Children.Count ? parent.Children[next] : null;
    }

    internal SemanticNodeSnapshot? PreviousSibling(string key)
    {
        SemanticNodeSnapshot? parent = Parent(key);
        if (parent is null || !indexInParent.TryGetValue(key, out int index)) return null;
        int previous = index - 1;
        return previous >= 0 ? parent.Children[previous] : null;
    }

    private void Index(SemanticNodeSnapshot node, string? parentKey, string key)
    {
        byKey[key] = node;
        keyOf[node] = key;
        parentKeyOf[key] = parentKey;
        for (int i = 0; i < node.Children.Count; i++)
        {
            string childKey = key + "/" + i.ToString(CultureInfo.InvariantCulture);
            indexInParent[childKey] = i;
            Index(node.Children[i], key, childKey);
        }
    }
}
