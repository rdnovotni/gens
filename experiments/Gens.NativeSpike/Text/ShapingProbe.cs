using HarfBuzzSharp;
using SkiaSharp;

namespace Gens.NativeSpike.Text;

internal static class ShapingProbe
{
    internal static ShapingResult Shape(string text)
    {
        using SKTypeface typeface = SKTypeface.Default;
        using SKStreamAsset stream = typeface.OpenStream(out int faceIndex) ?? throw new InvalidOperationException("Default typeface data is unavailable.");
        byte[] bytes = new byte[checked((int)stream.Length)];
        _ = stream.Read(bytes, bytes.Length);
        using var blob = Blob.FromStream(new MemoryStream(bytes));
        using var face = new Face(blob, (uint)faceIndex);
        using var font = new HarfBuzzSharp.Font(face);
        font.SetFunctionsOpenType();
        font.SetScale(face.UnitsPerEm, face.UnitsPerEm);
        using var buffer = new HarfBuzzSharp.Buffer();
        buffer.AddUtf16(text);
        buffer.GuessSegmentProperties();
        font.Shape(buffer);
        GlyphInfo[] infos = buffer.GlyphInfos;
        GlyphPosition[] positions = buffer.GlyphPositions;
        return new ShapingResult(text, infos.Select(static x => x.Codepoint).ToArray(), positions.Sum(static x => x.XAdvance), buffer.Direction.ToString());
    }
}

internal sealed record ShapingResult(string Text, uint[] Glyphs, int Advance, string Direction);
