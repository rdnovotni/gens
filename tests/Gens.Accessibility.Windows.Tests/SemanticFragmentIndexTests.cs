using Gens.Accessibility.Windows.Uia;
using Gens.Graphics;
using Gens.UI;
using NUnit.Framework;

namespace Gens.Accessibility.Windows.Tests;

[TestFixture]
public sealed class SemanticFragmentIndexTests
{
    // root
    //  |- a (no children)
    //  |- b
    //  |   |- b1
    //  |   |- b2
    //  |- c (single child)
    //      |- c1
    private static SemanticNodeSnapshot BuildNode(out SemanticNodeSnapshot a, out SemanticNodeSnapshot b, out SemanticNodeSnapshot b1, out SemanticNodeSnapshot b2, out SemanticNodeSnapshot c, out SemanticNodeSnapshot c1)
    {
        a = Node("a");
        b1 = Node("b1");
        b2 = Node("b2");
        b = Node("b", b1, b2);
        c1 = Node("c1");
        c = Node("c", c1);
        return Node("root", a, b, c);
    }

    private static SemanticTreeSnapshot BuildTree(out SemanticNodeSnapshot root, out SemanticNodeSnapshot a, out SemanticNodeSnapshot b, out SemanticNodeSnapshot b1, out SemanticNodeSnapshot b2, out SemanticNodeSnapshot c, out SemanticNodeSnapshot c1)
    {
        root = BuildNode(out a, out b, out b1, out b2, out c, out c1);
        return new(root, FocusedNode: null);
    }

    [Test]
    public void ParentReturnsTheContainingNodeAndNullForTheRoot()
    {
        SemanticTreeSnapshot snapshot = BuildTree(out SemanticNodeSnapshot root, out SemanticNodeSnapshot a, out SemanticNodeSnapshot b, out SemanticNodeSnapshot b1, out _, out _, out _);
        var index = new SemanticFragmentIndex(snapshot);
        Assert.Multiple(() =>
        {
            Assert.That(index.Parent(index.KeyOf(a)), Is.SameAs(root));
            Assert.That(index.Parent(index.KeyOf(b1)), Is.SameAs(b));
            Assert.That(index.Parent(index.KeyOf(root)), Is.Null);
        });
    }

    [Test]
    public void NextAndPreviousSiblingWalkInDeclarationOrder()
    {
        SemanticTreeSnapshot snapshot = BuildTree(out _, out SemanticNodeSnapshot a, out SemanticNodeSnapshot b, out SemanticNodeSnapshot b1, out SemanticNodeSnapshot b2, out SemanticNodeSnapshot c, out _);
        var index = new SemanticFragmentIndex(snapshot);
        Assert.Multiple(() =>
        {
            Assert.That(index.NextSibling(index.KeyOf(a)), Is.SameAs(b));
            Assert.That(index.NextSibling(index.KeyOf(c)), Is.Null); // last child of root
            Assert.That(index.PreviousSibling(index.KeyOf(a)), Is.Null); // first child of root
            Assert.That(index.PreviousSibling(index.KeyOf(c)), Is.SameAs(b));
            Assert.That(index.NextSibling(index.KeyOf(b1)), Is.SameAs(b2));
            Assert.That(index.PreviousSibling(index.KeyOf(b2)), Is.SameAs(b1));
        });
    }

    [Test]
    public void FirstAndLastChildMatchForANodeWithMultipleChildren()
    {
        SemanticTreeSnapshot snapshot = BuildTree(out SemanticNodeSnapshot root, out SemanticNodeSnapshot a, out SemanticNodeSnapshot b, out SemanticNodeSnapshot b1, out SemanticNodeSnapshot b2, out SemanticNodeSnapshot c, out _);
        var index = new SemanticFragmentIndex(snapshot);
        Assert.Multiple(() =>
        {
            Assert.That(index.FirstChild(index.KeyOf(root)), Is.SameAs(a));
            Assert.That(index.LastChild(index.KeyOf(root)), Is.SameAs(c));
            Assert.That(index.FirstChild(index.KeyOf(b)), Is.SameAs(b1));
            Assert.That(index.LastChild(index.KeyOf(b)), Is.SameAs(b2));
        });
    }

    [Test]
    public void SingleChildNodeHasNoSiblingsAndMatchingFirstLastChild()
    {
        SemanticTreeSnapshot snapshot = BuildTree(out _, out _, out _, out _, out _, out SemanticNodeSnapshot c, out SemanticNodeSnapshot c1);
        var index = new SemanticFragmentIndex(snapshot);
        Assert.Multiple(() =>
        {
            Assert.That(index.FirstChild(index.KeyOf(c)), Is.SameAs(c1));
            Assert.That(index.LastChild(index.KeyOf(c)), Is.SameAs(c1));
            Assert.That(index.NextSibling(index.KeyOf(c1)), Is.Null);
            Assert.That(index.PreviousSibling(index.KeyOf(c1)), Is.Null);
        });
    }

    [Test]
    public void NodeWithNoChildrenHasNullFirstAndLastChild()
    {
        SemanticTreeSnapshot snapshot = BuildTree(out _, out SemanticNodeSnapshot a, out _, out _, out _, out _, out _);
        var index = new SemanticFragmentIndex(snapshot);
        Assert.Multiple(() =>
        {
            Assert.That(index.FirstChild(index.KeyOf(a)), Is.Null);
            Assert.That(index.LastChild(index.KeyOf(a)), Is.Null);
        });
    }

    [Test]
    public void FindResolvesAnyNodeByItsKeyIncludingTheRoot()
    {
        SemanticTreeSnapshot snapshot = BuildTree(out SemanticNodeSnapshot root, out _, out _, out _, out SemanticNodeSnapshot b2, out _, out _);
        var index = new SemanticFragmentIndex(snapshot);
        Assert.Multiple(() =>
        {
            Assert.That(index.Find(index.KeyOf(root)), Is.SameAs(root));
            Assert.That(index.Find(index.KeyOf(b2)), Is.SameAs(b2));
            Assert.That(index.Find("no-such-key"), Is.Null);
        });
    }

    // Real Gens screens routinely contain many sibling controls that share a fallback Id, because
    // UiRoot.Snapshot falls back to the control's type name (e.g. "Button", "TextBlock") whenever no
    // explicit name is set. An id-keyed index would silently collapse these into one entry.
    [Test]
    public void DistinguishesSiblingsThatShareTheSameFallbackId()
    {
        SemanticNodeSnapshot firstButton = Node("Button");
        SemanticNodeSnapshot secondButton = Node("Button");
        SemanticNodeSnapshot thirdButton = Node("Button");
        SemanticNodeSnapshot root = Node("root", firstButton, secondButton, thirdButton);
        var index = new SemanticFragmentIndex(new(root, FocusedNode: null));

        Assert.Multiple(() =>
        {
            // Each colliding-Id node still round-trips through Find(KeyOf(node)) to itself, not to a sibling.
            Assert.That(index.Find(index.KeyOf(firstButton)), Is.SameAs(firstButton));
            Assert.That(index.Find(index.KeyOf(secondButton)), Is.SameAs(secondButton));
            Assert.That(index.Find(index.KeyOf(thirdButton)), Is.SameAs(thirdButton));

            // Navigation between the colliding siblings still resolves to the correct, distinct node.
            Assert.That(index.NextSibling(index.KeyOf(firstButton)), Is.SameAs(secondButton));
            Assert.That(index.NextSibling(index.KeyOf(secondButton)), Is.SameAs(thirdButton));
            Assert.That(index.PreviousSibling(index.KeyOf(thirdButton)), Is.SameAs(secondButton));

            // Every colliding node gets a distinct key.
            Assert.That(new[] { index.KeyOf(firstButton), index.KeyOf(secondButton), index.KeyOf(thirdButton) }.Distinct(), Has.Exactly(3).Items);
        });
    }

    private static SemanticNodeSnapshot Node(string id, params SemanticNodeSnapshot[] children) =>
        new(id, AccessibilityRole.Group, id, null, null, IsEnabled: true, IsFocused: false, IsChecked: false, new Rect(0, 0, 10, 10), children);
}
