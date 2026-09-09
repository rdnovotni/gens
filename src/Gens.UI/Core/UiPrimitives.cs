using Gens.Graphics;

namespace Gens.UI;

public enum UiVisibility { Visible, Hidden, Collapsed }
public enum HorizontalAlignment { Start, Center, End, Stretch }
public enum VerticalAlignment { Start, Center, End, Stretch }
public enum Orientation { Horizontal, Vertical }
public enum UiLengthKind { Auto, Fixed, Star }
public enum AccessibilityRole { None, Group, Text, Image, Button, CheckBox, Toggle, Heading, List, ListItem, ScrollView, Dialog }
public enum MotionMode { Full, Reduced, None }
public enum MotionCategory { Essential, Informational, Decorative }
public enum TextWrapping { NoWrap, Wrap }
public enum TextTrimming { None, Ellipsis }
public enum ImageStretch { None, Contain, Cover, Fill }

public readonly record struct Thickness(float Left, float Top, float Right, float Bottom)
{
    public Thickness(float uniform) : this(uniform, uniform, uniform, uniform) { }
    public Thickness(float horizontal, float vertical) : this(horizontal, vertical, horizontal, vertical) { }
    public static readonly Thickness Zero = new(0);
    public float Horizontal => Left + Right;
    public float Vertical => Top + Bottom;
}

public readonly record struct UiLength(float Value, UiLengthKind Kind)
{
    public static UiLength Auto => new(0, UiLengthKind.Auto);
    public static UiLength Fixed(float value) => new(value, UiLengthKind.Fixed);
    public static UiLength Star(float weight = 1) => new(weight, UiLengthKind.Star);
}

public readonly record struct CornerRadius(float TopLeft, float TopRight, float BottomRight, float BottomLeft)
{
    public CornerRadius(float uniform) : this(uniform, uniform, uniform, uniform) { }
}

public sealed class UiSemantics
{
    public AccessibilityRole Role { get; set; }
    public string? Label { get; set; }
    public string? Description { get; set; }
    public string? Value { get; set; }
    public bool IsChecked { get; set; }
    public bool IsDecorative { get; set; }
}

public readonly record struct MotionPolicy(MotionMode Mode)
{
    public bool Allows(MotionCategory category) => Mode switch
    {
        MotionMode.Full => true,
        MotionMode.Reduced => category != MotionCategory.Decorative,
        MotionMode.None => category == MotionCategory.Essential,
        _ => false,
    };

    public TimeSpan Adjust(TimeSpan duration, MotionCategory category) => Allows(category)
        ? Mode == MotionMode.Reduced && category == MotionCategory.Informational ? TimeSpan.FromTicks(Math.Min(duration.Ticks, TimeSpan.FromMilliseconds(120).Ticks)) : duration
        : TimeSpan.Zero;
}

/// <summary><c>Bounds</c> is in root-relative logical units (matching <c>UiNode.Bounds</c>), not screen pixels.</summary>
public sealed record SemanticNodeSnapshot(string Id, AccessibilityRole Role, string? Name, string? Description, string? Value, bool IsEnabled, bool IsFocused, bool IsChecked, Rect Bounds, IReadOnlyList<SemanticNodeSnapshot> Children);
public sealed record SemanticTreeSnapshot(SemanticNodeSnapshot Root, SemanticNodeSnapshot? FocusedNode);

internal static class UiGeometry
{
    public static bool Contains(this Rect rect, Point2 point) => point.X >= rect.X && point.Y >= rect.Y && point.X <= rect.X + rect.Width && point.Y <= rect.Y + rect.Height;
    public static Size2 Deflate(this Size2 size, Thickness value) => new(Math.Max(0, size.Width - value.Horizontal), Math.Max(0, size.Height - value.Vertical));
    public static Rect Deflate(this Rect rect, Thickness value) => new(rect.X + value.Left, rect.Y + value.Top, Math.Max(0, rect.Width - value.Horizontal), Math.Max(0, rect.Height - value.Vertical));
    public static Size2 Inflate(this Size2 size, Thickness value) => new(size.Width + value.Horizontal, size.Height + value.Vertical);
    public static Size2 Constrain(this Size2 size, float minWidth, float minHeight, float maxWidth, float maxHeight) => new(Math.Clamp(size.Width, minWidth, maxWidth), Math.Clamp(size.Height, minHeight, maxHeight));
}
