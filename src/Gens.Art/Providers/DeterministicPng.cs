using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Gens.Art.Providers;

internal static class DeterministicPng
{
    private static readonly byte[] Signature = [137, 80, 78, 71, 13, 10, 26, 10];

    public static byte[] Create(int width, int height, string identity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);
        byte[] color = SHA256.HashData(Encoding.UTF8.GetBytes(identity));
        using var raw = new MemoryStream();
        for (int y = 0; y < height; y++)
        {
            raw.WriteByte(0);
            for (int x = 0; x < width; x++)
            {
                raw.WriteByte((byte)(color[0] ^ (x & 31)));
                raw.WriteByte((byte)(color[1] ^ (y & 31)));
                raw.WriteByte(color[2]);
                raw.WriteByte(255);
            }
        }
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Fastest, leaveOpen: true))
        {
            byte[] rawBytes = raw.ToArray();
            zlib.Write(rawBytes);
        }
        using var png = new MemoryStream();
        png.Write(Signature);
        Span<byte> dimensions = stackalloc byte[13];
        BinaryPrimitives.WriteInt32BigEndian(dimensions, width); BinaryPrimitives.WriteInt32BigEndian(dimensions[4..], height);
        dimensions[8] = 8; dimensions[9] = 6;
        WriteChunk(png, "IHDR", dimensions); WriteChunk(png, "IDAT", compressed.ToArray()); WriteChunk(png, "IEND", []);
        return png.ToArray();
    }

    private static void WriteChunk(Stream output, string type, ReadOnlySpan<byte> data)
    {
        Span<byte> length = stackalloc byte[4]; BinaryPrimitives.WriteInt32BigEndian(length, data.Length); output.Write(length);
        byte[] typeBytes = Encoding.ASCII.GetBytes(type); output.Write(typeBytes); output.Write(data);
        byte[] crcInput = new byte[typeBytes.Length + data.Length]; typeBytes.CopyTo(crcInput, 0); data.CopyTo(crcInput.AsSpan(typeBytes.Length));
        BinaryPrimitives.WriteUInt32BigEndian(length, Crc32(crcInput)); output.Write(length);
    }

    private static uint Crc32(ReadOnlySpan<byte> bytes)
    {
        uint crc = uint.MaxValue;
        foreach (byte value in bytes) { crc ^= value; for (int bit = 0; bit < 8; bit++) crc = (crc >> 1) ^ (0xedb88320u & (uint)-(int)(crc & 1)); }
        return ~crc;
    }
}
