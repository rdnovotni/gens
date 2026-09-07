using System.Diagnostics;
using System.Security.Cryptography;
using SkiaSharp;

namespace Gens.NativeSpike.Rendering;

internal sealed class SoftwareRenderer : IDisposable
{
    private SKBitmap bitmap;
    private SKCanvas canvas;

    internal SoftwareRenderer(int width, int height)
    {
        bitmap = NewBitmap(width, height);
        canvas = new SKCanvas(bitmap);
    }

    internal int Width => bitmap.Width;
    internal int Height => bitmap.Height;
    internal SKCanvas Canvas => canvas;
    internal IntPtr Pixels => bitmap.GetPixels();
    internal int RowBytes => bitmap.RowBytes;

    internal void Resize(int width, int height)
    {
        width = Math.Max(width, 1); height = Math.Max(height, 1);
        if (width == Width && height == Height) return;
        canvas.Dispose(); bitmap.Dispose();
        bitmap = NewBitmap(width, height); canvas = new SKCanvas(bitmap);
    }

    internal string SavePng(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
        using FileStream stream = File.Create(path);
        data.SaveTo(stream);
        return path;
    }

    internal string PixelHash()
    {
        using SKPixmap pixels = bitmap.PeekPixels();
        return Convert.ToHexString(SHA256.HashData(pixels.GetPixelSpan()));
    }

    internal static BenchmarkSample Measure(int width, int height, Action<SKCanvas, int, int, double> draw, int frames)
    {
        using var renderer = new SoftwareRenderer(width, height);
        draw(renderer.Canvas, width, height, 0);
        long allocated = GC.GetAllocatedBytesForCurrentThread();
        int[] collections = [GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2)];
        var samples = new double[frames];
        var clock = Stopwatch.StartNew();
        for (int i = 0; i < frames; i++)
        {
            long start = Stopwatch.GetTimestamp();
            draw(renderer.Canvas, width, height, i / 60d);
            renderer.Canvas.Flush();
            samples[i] = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        }
        clock.Stop(); Array.Sort(samples);
        return new BenchmarkSample(samples.Average(), samples[(int)(frames * .95) - 1], samples[(int)(frames * .99) - 1],
            (GC.GetAllocatedBytesForCurrentThread() - allocated) / frames,
            [GC.CollectionCount(0) - collections[0], GC.CollectionCount(1) - collections[1], GC.CollectionCount(2) - collections[2]],
            Environment.WorkingSet, clock.Elapsed.TotalMilliseconds);
    }

    private static SKBitmap NewBitmap(int width, int height) => new(new SKImageInfo(Math.Max(1, width), Math.Max(1, height), SKColorType.Bgra8888, SKAlphaType.Premul));
    public void Dispose() { canvas.Dispose(); bitmap.Dispose(); }
}

internal sealed record BenchmarkSample(double AverageMs, double P95Ms, double P99Ms, long AllocatedBytesPerFrame, int[] Collections, long WorkingSetBytes, double WallMilliseconds);
