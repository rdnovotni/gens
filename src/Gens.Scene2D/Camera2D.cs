using System.Numerics;
using Gens.Graphics;

namespace Gens.Scene2D;

public sealed class Camera2D
{
    private float zoom = 1;
    private Point2 position;
    private float minZoom = 0.1f, maxZoom = 16;
    private Rect viewport;
    public Point2 Position { get => position; set { if (position == value) return; position = value; Changed?.Invoke(); } }
    public float Zoom { get => zoom; set { float next = Math.Clamp(value, MinZoom, MaxZoom); if (zoom == next) return; zoom = next; Changed?.Invoke(); } }
    public float MinZoom { get => minZoom; set { ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, 0); ArgumentOutOfRangeException.ThrowIfGreaterThan(value, maxZoom); minZoom = value; Zoom = zoom; } }
    public float MaxZoom { get => maxZoom; set { ArgumentOutOfRangeException.ThrowIfLessThan(value, minZoom); maxZoom = value; Zoom = zoom; } }
    public Rect Viewport { get => viewport; set { if (viewport == value) return; viewport = value; Changed?.Invoke(); } }
    public event Action? Changed;
    public Matrix3x2 ViewMatrix => Matrix3x2.CreateTranslation(-Position.X, -Position.Y) * Matrix3x2.CreateScale(Zoom) * Matrix3x2.CreateTranslation(Viewport.X + Viewport.Width / 2, Viewport.Y + Viewport.Height / 2);

    public Point2 WorldToScreen(Point2 point)
    {
        Vector2 value = Vector2.Transform(new(point.X, point.Y), ViewMatrix);
        return new(value.X, value.Y);
    }

    public Point2 ScreenToWorld(Point2 point)
    {
        if (!Matrix3x2.Invert(ViewMatrix, out Matrix3x2 inverse)) throw new InvalidOperationException("The camera transform is not invertible.");
        Vector2 value = Vector2.Transform(new(point.X, point.Y), inverse);
        return new(value.X, value.Y);
    }
}
