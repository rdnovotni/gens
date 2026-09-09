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
    private static SemanticTreeSnapshot BuildTree()
    {
        SemanticNodeSnapshot a = Node("a");
        SemanticNodeSnapshot b1 = Node("b1");
        SemanticNodeSnapshot b2 = Node("b2");
        SemanticNodeSnapshot b = Node("b", b1, b2);
        SemanticNodeSnapshot c1 = Node("c1");
        SemanticNodeSnapshot c = Node("c", c1);
        SemanticNodeSnapshot root = Node("root", a, b, c);
        return new(root, FocusedNode: null);
    }

    [Test]
    public void ParentReturnsTheContainingNodeAndNullForTheRoot()
    {
        var index = new SemanticFragmentIndex(BuildTree());
        Assert.Multiple(() =>
        {
            Assert.That(index.Parent("a")?.Id, Is.EqualTo("root"));
            Assert.That(index.Parent("b1")?.Id, Is.EqualTo("b"));
            Assert.That(index.Parent("root"), Is.Null);
        });
    }

    [Test]
    public void NextAndPreviousSiblingWalkInDeclarationOrder()
    {
        var index = new SemanticFragmentIndex(BuildTree());
        Assert.Multiple(() =>
        {
            Assert.That(index.NextSibling("a")?.Id, Is.EqualTo("b"));
            Assert.That(index.NextSibling("c"), Is.Null); // last child of root
            Assert.That(index.PreviousSibling("a"), Is.Null); // first child of root
            Assert.That(index.PreviousSibling("c")?.Id, Is.EqualTo("b"));
            Assert.That(index.NextSibling("b1")?.Id, Is.EqualTo("b2"));
            Assert.That(index.PreviousSibling("b2")?.Id, Is.EqualTo("b1"));
        });
    }

    [Test]
    public void FirstAndLastChildMatchForANodeWithMultipleChildren()
    {
        var index = new SemanticFragmentIndex(BuildTree());
        Assert.Multiple(() =>
        {
            Assert.That(index.FirstChild("root")?.Id, Is.EqualTo("a"));
            Assert.That(index.LastChild("root")?.Id, Is.EqualTo("c"));
            Assert.That(index.FirstChild("b")?.Id, Is.EqualTo("b1"));
            Assert.That(index.LastChild("b")?.Id, Is.EqualTo("b2"));
        });
    }

    [Test]
    public void SingleChildNodeHasNoSiblingsAndMatchingFirstLastChild()
    {
        var index = new SemanticFragmentIndex(BuildTree());
        Assert.Multiple(() =>
        {
            Assert.That(index.FirstChild("c")?.Id, Is.EqualTo("c1"));
            Assert.That(index.LastChild("c")?.Id, Is.EqualTo("c1"));
            Assert.That(index.NextSibling("c1"), Is.Null);
            Assert.That(index.PreviousSibling("c1"), Is.Null);
        });
    }

    [Test]
    public void NodeWithNoChildrenHasNullFirstAndLastChild()
    {
        var index = new SemanticFragmentIndex(BuildTree());
        Assert.Multiple(() =>
        {
            Assert.That(index.FirstChild("a"), Is.Null);
            Assert.That(index.LastChild("a"), Is.Null);
        });
    }

    [Test]
    public void FindResolvesAnyNodeByIdIncludingTheRoot()
    {
        var index = new SemanticFragmentIndex(BuildTree());
        Assert.Multiple(() =>
        {
            Assert.That(index.Find("root")?.Id, Is.EqualTo("root"));
            Assert.That(index.Find("b2")?.Id, Is.EqualTo("b2"));
            Assert.That(index.Find("missing"), Is.Null);
        });
    }

    private static SemanticNodeSnapshot Node(string id, params SemanticNodeSnapshot[] children) =>
        new(id, AccessibilityRole.Group, id, null, null, IsEnabled: true, IsFocused: false, IsChecked: false, new Rect(0, 0, 10, 10), children);
}
