using System.Numerics;

namespace Gens.Platform.Sdl;

internal static class SdlEventTranslator
{
    internal static KeyboardEvent Keyboard(ulong timestamp, uint windowId, uint scanCode, ushort modifiers, bool isDown, bool isRepeat) =>
        new(timestamp, new(windowId), new(scanCode), TranslateModifiers(modifiers), isDown, isRepeat);

    internal static PointerMovedEvent PointerMoved(ulong timestamp, uint windowId, float x, float y, float deltaX, float deltaY) =>
        new(timestamp, new(windowId), new(x, y), new Vector2(deltaX, deltaY));

    internal static PointerButtonEvent PointerButton(ulong timestamp, uint windowId, float x, float y, byte button, bool isDown, byte clicks) =>
        new(timestamp, new(windowId), new(x, y), TranslateButton(button), isDown, clicks);

    internal static WheelEvent Wheel(ulong timestamp, uint windowId, float pointerX, float pointerY, float deltaX, float deltaY, bool flipped)
    {
        float direction = flipped ? -1 : 1;
        return new(timestamp, new(windowId), new(pointerX, pointerY), new Vector2(deltaX * direction, deltaY * direction));
    }

    internal static KeyModifiers TranslateModifiers(ushort value)
    {
        KeyModifiers result = KeyModifiers.None;
        if ((value & 0x0001) != 0) result |= KeyModifiers.LeftShift;
        if ((value & 0x0002) != 0) result |= KeyModifiers.RightShift;
        if ((value & 0x0040) != 0) result |= KeyModifiers.LeftControl;
        if ((value & 0x0080) != 0) result |= KeyModifiers.RightControl;
        if ((value & 0x0100) != 0) result |= KeyModifiers.LeftAlt;
        if ((value & 0x0200) != 0) result |= KeyModifiers.RightAlt;
        if ((value & 0x0400) != 0) result |= KeyModifiers.LeftGui;
        if ((value & 0x0800) != 0) result |= KeyModifiers.RightGui;
        if ((value & 0x1000) != 0) result |= KeyModifiers.NumLock;
        if ((value & 0x2000) != 0) result |= KeyModifiers.CapsLock;
        if ((value & 0x4000) != 0) result |= KeyModifiers.AltGr;
        return result;
    }

    private static PointerButton TranslateButton(byte value) => value switch
    {
        1 => Platform.PointerButton.Primary,
        2 => Platform.PointerButton.Middle,
        3 => Platform.PointerButton.Secondary,
        4 => Platform.PointerButton.X1,
        5 => Platform.PointerButton.X2,
        _ => Platform.PointerButton.Unknown,
    };
}
