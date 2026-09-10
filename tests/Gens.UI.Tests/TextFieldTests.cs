using Gens.Platform;
using NUnit.Framework;

namespace Gens.UI.Tests;

public sealed class TextFieldTests : UiTestFixture
{
    [Test]
    public void FocusableAndAcceptsTypedTextThroughRootTextInputRouting()
    {
        UiRoot root = Root(); var field = new TextField { Width = 160, Semantics = { Label = "Save name" } }; root.AddChild(field); root.Layout(new(200, 60));
        Assert.That(root.Focus.RequestFocus(field), Is.True);
        root.HandleTextInput("Aemilii");
        Assert.That(field.Text, Is.EqualTo("Aemilii"));
    }

    [Test]
    public void TextInputIsIgnoredWhenFieldIsNotFocused()
    {
        UiRoot root = Root(); var field = new TextField { Width = 160 }; root.AddChild(field); root.Layout(new(200, 60));
        root.HandleTextInput("ignored");
        Assert.That(field.Text, Is.Empty);
    }

    [Test]
    public void RespectsMaxLength()
    {
        UiRoot root = Root(); var field = new TextField { Width = 160, MaxLength = 5 }; root.AddChild(field); root.Layout(new(200, 60)); root.Focus.RequestFocus(field);
        root.HandleTextInput("Aemilius");
        Assert.That(field.Text, Is.EqualTo("Aemil"));
        root.HandleTextInput("X");
        Assert.That(field.Text, Is.EqualTo("Aemil"));
    }

    [Test]
    public void BackspaceRemovesLastCharacter()
    {
        UiRoot root = Root(); var field = new TextField { Width = 160, Text = "Rome" }; root.AddChild(field); root.Layout(new(200, 60)); root.Focus.RequestFocus(field);
        root.HandleKey(new(new(42), KeyModifiers.None, false));
        Assert.That(field.Text, Is.EqualTo("Rom"));
    }

    [Test]
    public void EnterInvokesSubmitted()
    {
        UiRoot root = Root(); int submitted = 0; var field = new TextField { Width = 160, Submitted = () => submitted++ }; root.AddChild(field); root.Layout(new(200, 60)); root.Focus.RequestFocus(field);
        root.HandleKey(new(new(40), KeyModifiers.None, false));
        Assert.That(submitted, Is.EqualTo(1));
    }

    [Test]
    public void SemanticsValueReflectsCurrentText()
    {
        UiRoot root = Root(); var field = new TextField { Width = 160, Text = "Latium" }; root.AddChild(field); root.Layout(new(200, 60));
        Assert.That(field.Semantics.Value, Is.EqualTo("Latium"));
    }

    [Test]
    public void DisabledFieldCannotBeFocusedAndIgnoresInput()
    {
        UiRoot root = Root(); var field = new TextField { Width = 160, Text = "Locked", IsEnabled = false }; root.AddChild(field); root.Layout(new(200, 60));
        bool focused = root.Focus.RequestFocus(field);
        root.HandleTextInput("more");
        root.HandleKey(new(new(42), KeyModifiers.None, false));
        Assert.Multiple(() => { Assert.That(focused, Is.False); Assert.That(field.Text, Is.EqualTo("Locked")); });
    }
}
