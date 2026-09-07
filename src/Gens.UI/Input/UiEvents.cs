using Gens.Graphics;
using Gens.Platform;
using System.Numerics;

namespace Gens.UI;

public enum UiPointerEventType { Entered, Exited, Moved, Pressed, Released, Clicked, Wheel }

public sealed class UiPointerEvent
{
    public UiPointerEvent(UiPointerEventType type, Point2 position, PointerButton button = PointerButton.Unknown, Vector2 delta = default)
    {
        Type = type; Position = position; Button = button; Delta = delta;
    }
    public UiPointerEventType Type { get; }
    public Point2 Position { get; }
    public PointerButton Button { get; }
    public Vector2 Delta { get; }
    public UiNode? OriginalTarget { get; internal set; }
    public UiNode? CurrentTarget { get; internal set; }
    public bool Handled { get; set; }
    public void CapturePointer() => CurrentTarget?.Root?.CapturePointer(CurrentTarget);
    public void ReleasePointerCapture() => CurrentTarget?.Root?.ReleasePointerCapture(CurrentTarget);
}

public sealed class UiKeyEvent(PhysicalKey key, KeyModifiers modifiers, bool isRepeat)
{
    public PhysicalKey Key { get; } = key;
    public KeyModifiers Modifiers { get; } = modifiers;
    public bool IsRepeat { get; } = isRepeat;
    public bool Handled { get; set; }
}

public sealed class UiInputException(string message, Exception innerException) : Exception(message, innerException);
