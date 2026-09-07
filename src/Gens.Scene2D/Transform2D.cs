using System.Numerics;
using Gens.Graphics;

namespace Gens.Scene2D;

public readonly record struct Transform2D(Point2 Position, float Rotation, Point2 Scale, Point2 Pivot)
{
    public static Transform2D Identity { get; } = new(new(0, 0), 0, new(1, 1), new(0, 0));

    public Matrix3x2 Matrix =>
        Matrix3x2.CreateTranslation(-Pivot.X, -Pivot.Y) *
        Matrix3x2.CreateScale(Scale.X, Scale.Y) *
        Matrix3x2.CreateRotation(Rotation * MathF.PI / 180f) *
        Matrix3x2.CreateTranslation(Position.X, Position.Y);

    public Point2 TransformPoint(Point2 point)
    {
        Vector2 value = Vector2.Transform(new(point.X, point.Y), Matrix);
        return new(value.X, value.Y);
    }

    public bool TryInverse(out Matrix3x2 inverse) => Matrix3x2.Invert(Matrix, out inverse);
}
