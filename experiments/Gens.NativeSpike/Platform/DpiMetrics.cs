namespace Gens.NativeSpike.Platform;

internal readonly record struct DpiMetrics(int LogicalWidth, int LogicalHeight, int PixelWidth, int PixelHeight)
{
    public float ScaleX => LogicalWidth == 0 ? 1 : (float)PixelWidth / LogicalWidth;
    public float ScaleY => LogicalHeight == 0 ? 1 : (float)PixelHeight / LogicalHeight;

    public (float X, float Y) LogicalToPixel(float x, float y) => (x * ScaleX, y * ScaleY);
    public (float X, float Y) PixelToLogical(float x, float y) => (x / ScaleX, y / ScaleY);
}
