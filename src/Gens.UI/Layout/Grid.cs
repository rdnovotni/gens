using Gens.Graphics;

namespace Gens.UI;

public sealed class GridDefinition(UiLength length)
{
    public UiLength Length { get; set; } = length;
}

public class Grid : Panel
{
    private readonly Dictionary<UiNode, GridPlacement> placements = [];
    private float[] measuredRows = [], measuredColumns = [];
    public IList<GridDefinition> Rows { get; } = new List<GridDefinition>();
    public IList<GridDefinition> Columns { get; } = new List<GridDefinition>();
    public void SetPlacement(UiNode child, int row, int column, int rowSpan = 1, int columnSpan = 1)
    {
        if (!Children.Contains(child)) throw new InvalidOperationException("Grid placement can only be set for an attached child.");
        placements[child] = new(row, column, Math.Max(1, rowSpan), Math.Max(1, columnSpan)); InvalidateMeasure();
    }
    public GridPlacement GetPlacement(UiNode child) => placements.TryGetValue(child, out GridPlacement value) ? value : new(0, 0, 1, 1);
    protected override Size2 MeasureOverride(Size2 availableSize)
    {
        int rowCount = Math.Max(1, Rows.Count), columnCount = Math.Max(1, Columns.Count);
        measuredRows = Initialize(Rows, rowCount); measuredColumns = Initialize(Columns, columnCount);
        Size2 inner = availableSize.Deflate(Padding);
        AllocateStars(measuredColumns, Columns, inner.Width); AllocateStars(measuredRows, Rows, inner.Height);
        foreach (UiNode child in Children.Where(static c => c.Visibility != UiVisibility.Collapsed))
        {
            GridPlacement p = GetPlacement(child);
            float childWidth = Constraint(measuredColumns, Columns, p.Column, p.ColumnSpan, inner.Width);
            float childHeight = Constraint(measuredRows, Rows, p.Row, p.RowSpan, inner.Height);
            child.Measure(new(childWidth, childHeight));
            DistributeAuto(measuredColumns, Columns, p.Column, p.ColumnSpan, child.DesiredSize.Width);
            DistributeAuto(measuredRows, Rows, p.Row, p.RowSpan, child.DesiredSize.Height);
        }
        AllocateStars(measuredColumns, Columns, inner.Width); AllocateStars(measuredRows, Rows, inner.Height);
        return new Size2(measuredColumns.Sum(), measuredRows.Sum()).Inflate(Padding);
    }
    protected override void ArrangeOverride(Rect finalRect)
    {
        ContentBounds = finalRect.Deflate(Padding); float[] widths = ResolveFinal(Columns, measuredColumns, ContentBounds.Width); float[] heights = ResolveFinal(Rows, measuredRows, ContentBounds.Height);
        foreach (UiNode child in Children.Where(static c => c.Visibility != UiVisibility.Collapsed))
        {
            GridPlacement p = GetPlacement(child); int row = Math.Clamp(p.Row, 0, heights.Length - 1), column = Math.Clamp(p.Column, 0, widths.Length - 1);
            int rowSpan = Math.Min(p.RowSpan, heights.Length - row), columnSpan = Math.Min(p.ColumnSpan, widths.Length - column);
            child.Arrange(new(ContentBounds.X + widths.Take(column).Sum(), ContentBounds.Y + heights.Take(row).Sum(), widths.Skip(column).Take(columnSpan).Sum(), heights.Skip(row).Take(rowSpan).Sum()));
        }
    }
    private static float[] Initialize(IList<GridDefinition> definitions, int count) { var result = new float[count]; for (int i = 0; i < count; i++) if (i < definitions.Count && definitions[i].Length.Kind == UiLengthKind.Fixed) result[i] = definitions[i].Length.Value; return result; }
    private static void DistributeAuto(float[] values, IList<GridDefinition> definitions, int start, int span, float desired)
    {
        if (start < 0 || start >= values.Length) return; int end = Math.Min(values.Length, start + span); float current = values.Skip(start).Take(end - start).Sum(); float missing = Math.Max(0, desired - current);
        int[] auto = Enumerable.Range(start, end - start).Where(i => i >= definitions.Count || definitions[i].Length.Kind == UiLengthKind.Auto).ToArray(); if (auto.Length == 0) return;
        foreach (int i in auto) values[i] += missing / auto.Length;
    }
    private static void AllocateStars(float[] values, IList<GridDefinition> definitions, float available)
    {
        if (float.IsInfinity(available)) return; int[] stars = Enumerable.Range(0, values.Length).Where(i => i < definitions.Count && definitions[i].Length.Kind == UiLengthKind.Star).ToArray(); float remaining = Math.Max(0, available - Enumerable.Range(0, values.Length).Where(i => !stars.Contains(i)).Sum(i => values[i])); float total = stars.Sum(i => Math.Max(0, definitions[i].Length.Value));
        if (total <= 0) return; for (int i = 0; i < values.Length; i++) if (i < definitions.Count && definitions[i].Length.Kind == UiLengthKind.Star) values[i] = remaining * definitions[i].Length.Value / total;
    }
    private static float Constraint(float[] values, IList<GridDefinition> definitions, int start, int span, float fallback)
    {
        int end = Math.Min(values.Length, Math.Max(0, start) + span); if (start < 0 || start >= end) return fallback;
        if (Enumerable.Range(start, end - start).Any(i => i >= definitions.Count || definitions[i].Length.Kind == UiLengthKind.Auto)) return fallback;
        float value = values.Skip(start).Take(end - start).Sum(); return value > 0 ? value : fallback;
    }
    private static float[] ResolveFinal(IList<GridDefinition> definitions, float[] measured, float available) { float[] result = (float[])measured.Clone(); AllocateStars(result, definitions, available); return result; }
}

public readonly record struct GridPlacement(int Row, int Column, int RowSpan, int ColumnSpan);
