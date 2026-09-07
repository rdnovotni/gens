using System.Numerics;
using Gens.Platform;
using Gens.Platform.Sdl;
using Gens.Platform.Sdl.Interop;
using NUnit.Framework;
using System.Runtime.InteropServices;

namespace Gens.Platform.Tests;

public sealed class PlatformContractTests
{
    [Test]
    public void LogicalAndPixelSizesRemainDistinct()
    {
        var logical = new LogicalSize(1920, 1080); var pixels = new PixelSize(2880, 1620); var scale = new DisplayScale(1.5f, 1.5f);
        Assert.Multiple(() => { Assert.That(pixels.Width / (float)logical.Width, Is.EqualTo(scale.X)); Assert.That(pixels.Height / (float)logical.Height, Is.EqualTo(scale.Y)); Assert.That(logical, Is.Not.EqualTo((object)pixels)); });
    }

    [Test]
    public void KeyboardTextCompositionAndPointerAreSeparateImmutableEvents()
    {
        var id = new WindowId(7);
        PlatformEvent[] events =
        [
            new KeyboardEvent(1, id, new PhysicalKey(4), KeyModifiers.LeftShift, true, false),
            new TextInputEvent(2, id, "é"),
            new TextCompositionEvent(3, id, "かな", 0, 2),
            new PointerMovedEvent(4, id, new(12.5f, 20.25f), new Vector2(1, -2)),
        ];
        Assert.That(events.Select(static x => x.GetType()), Is.Unique);
        Assert.That(((PointerMovedEvent)events[3]).Position, Is.EqualTo(new PointerPosition(12.5f, 20.25f)));
    }

    [Test]
    public void SdlKeyboardAndPointerValuesTranslateWithoutLeakingSdlTypes()
    {
        KeyboardEvent key = SdlEventTranslator.Keyboard(12, 9, 41, 0x0141, true, true);
        PointerMovedEvent pointer = SdlEventTranslator.PointerMoved(13, 9, 25, 30, 2, -3);
        WheelEvent wheel = SdlEventTranslator.Wheel(14, 9, 25, 30, 1, -2, flipped: true);
        Assert.Multiple(() =>
        {
            Assert.That(key.Key, Is.EqualTo(new PhysicalKey(41)));
            Assert.That(key.Modifiers, Is.EqualTo(KeyModifiers.LeftShift | KeyModifiers.LeftControl | KeyModifiers.LeftAlt));
            Assert.That(key.IsRepeat, Is.True);
            Assert.That(pointer.Position, Is.EqualTo(new PointerPosition(25, 30)));
            Assert.That(pointer.Delta, Is.EqualTo(new Vector2(2, -3)));
            Assert.That(wheel.Delta, Is.EqualTo(new Vector2(-1, 2)));
        });
    }

    [Test]
    public void SdlEventUnionMatchesPinnedHeaderLayout()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Marshal.SizeOf<SdlNative.Event>(), Is.EqualTo(128));
            Assert.That(Marshal.OffsetOf<SdlNative.Event>(nameof(SdlNative.Event.Key)).ToInt32(), Is.Zero);
            Assert.That(Marshal.OffsetOf<SdlNative.Event>(nameof(SdlNative.Event.Text)).ToInt32(), Is.Zero);
            Assert.That(Marshal.OffsetOf<SdlNative.Event>(nameof(SdlNative.Event.Wheel)).ToInt32(), Is.Zero);
        });
    }
}
