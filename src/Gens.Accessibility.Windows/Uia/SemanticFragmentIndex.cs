using Gens.UI;

namespace Gens.Accessibility.Windows.Uia;

/// <summary>
/// Adds parent/sibling navigation over an immutable <see cref="SemanticTreeSnapshot"/>, which only carries
/// child links. Rebuilt once per <c>Publish(snapshot)</c> call. No COM types — fully unit-testable.
/// </summary>
internal sealed class SemanticFragmentIndex
{
    private readonly Dictionary<string, SemanticNodeSnapshot> byId = [];
    private readonly Dictionary<string, SemanticNodeSnapshot?> parentOf = [];
    private readonly Dictionary<string, int> indexInParent = [];

    internal SemanticFragmentIndex(SemanticTreeSnapshot snapshot)
    {
        Root = snapshot.Root;
        Index(snapshot.Root, null);
    }

    internal SemanticNodeSnapshot Root { get; }

    internal SemanticNodeSnapshot? Find(string id) => byId.GetValueOrDefault(id);
    internal SemanticNodeSnapshot? Parent(string id) => parentOf.GetValueOrDefault(id);
    internal SemanticNodeSnapshot? FirstChild(string id) => byId.TryGetValue(id, out SemanticNodeSnapshot? node) && node.Children.Count > 0 ? node.Children[0] : null;
    internal SemanticNodeSnapshot? LastChild(string id) => byId.TryGetValue(id, out SemanticNodeSnapshot? node) && node.Children.Count > 0 ? node.Children[^1] : null;

    internal SemanticNodeSnapshot? NextSibling(string id)
    {
        SemanticNodeSnapshot? parent = Parent(id);
        if (parent is null || !indexInParent.TryGetValue(id, out int index)) return null;
        int next = index + 1;
        return next < parent.Children.Count ? parent.Children[next] : null;
    }

    internal SemanticNodeSnapshot? PreviousSibling(string id)
    {
        SemanticNodeSnapshot? parent = Parent(id);
        if (parent is null || !indexInParent.TryGetValue(id, out int index)) return null;
        int previous = index - 1;
        return previous >= 0 ? parent.Children[previous] : null;
    }

    private void Index(SemanticNodeSnapshot node, SemanticNodeSnapshot? parent)
    {
        byId[node.Id] = node;
        parentOf[node.Id] = parent;
        for (int i = 0; i < node.Children.Count; i++)
        {
            indexInParent[node.Children[i].Id] = i;
            Index(node.Children[i], node);
        }
    }
}
