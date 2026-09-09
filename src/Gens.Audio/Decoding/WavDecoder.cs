using System.Buffers.Binary;

namespace Gens.Audio.Decoding;

/// <summary>Decodes RIFF/WAVE PCM (8/16/24/32-bit integer, 32-bit float, and WAVE_FORMAT_EXTENSIBLE) to PCM16 little-endian.</summary>
public sealed class WavDecoder : IAudioFormatDecoder
{
    private const int FormatPcm = 1;
    private const int FormatIeeeFloat = 3;
    private const int FormatExtensible = unchecked((ushort)0xFFFE);

    public bool CanDecode(ReadOnlySpan<byte> header) =>
        header.Length >= 12 &&
        header[0] == (byte)'R' && header[1] == (byte)'I' && header[2] == (byte)'F' && header[3] == (byte)'F' &&
        header[8] == (byte)'W' && header[9] == (byte)'A' && header[10] == (byte)'V' && header[11] == (byte)'E';

    public (PcmFormat Format, TimeSpan Duration, Func<Stream> OpenPcmStream) Decode(Func<Stream> openEncodedStream)
    {
        WavHeader header;
        using (Stream probe = openEncodedStream()) header = ReadHeader(probe);

        var format = new PcmFormat(header.Channels, header.SampleRate);
        int sourceBytesPerSample = header.BitsPerSample / 8;
        long frameCount = header.DataLength / (header.Channels * sourceBytesPerSample);
        TimeSpan duration = header.SampleRate > 0 ? TimeSpan.FromSeconds((double)frameCount / header.SampleRate) : TimeSpan.Zero;

        Func<Stream> openPcmStream = () =>
        {
            Stream source = openEncodedStream();
            source.Seek(header.DataOffset, SeekOrigin.Begin);
            var bounded = new BoundedStream(source, header.DataLength, ownsInner: true);
            return header.AudioFormat == FormatPcm && header.BitsPerSample == 16
                ? bounded
                : new PcmConvertingStream(bounded, header.BitsPerSample, header.AudioFormat, ownsInner: true);
        };

        return (format, duration, openPcmStream);
    }

    private readonly record struct WavHeader(int AudioFormat, int Channels, int SampleRate, int BitsPerSample, long DataOffset, long DataLength);

    private static WavHeader ReadHeader(Stream stream)
    {
        Span<byte> tag = stackalloc byte[4];
        ReadExact(stream, tag);
        if (!Matches(tag, "RIFF")) throw new InvalidDataException("Not a RIFF stream.");
        Span<byte> four = stackalloc byte[4];
        ReadExact(stream, four); // RIFF chunk size, unused
        ReadExact(stream, tag);
        if (!Matches(tag, "WAVE")) throw new InvalidDataException("Not a WAVE stream.");

        int audioFormat = 0, channels = 0, sampleRate = 0, bitsPerSample = 0;
        long dataOffset = -1, dataLength = 0;
        long position = 12;
        Span<byte> chunkId = stackalloc byte[4];

        while (dataOffset < 0)
        {
            if (!TryReadExact(stream, chunkId)) throw new InvalidDataException("WAVE stream has no data chunk.");
            ReadExact(stream, four);
            uint chunkSize = BinaryPrimitives.ReadUInt32LittleEndian(four);
            position += 8;

            if (Matches(chunkId, "fmt "))
            {
                if (chunkSize < 16) throw new InvalidDataException("WAVE fmt chunk is too short.");
                byte[] fmt = new byte[chunkSize];
                ReadExact(stream, fmt);
                audioFormat = BinaryPrimitives.ReadUInt16LittleEndian(fmt.AsSpan(0, 2));
                channels = BinaryPrimitives.ReadUInt16LittleEndian(fmt.AsSpan(2, 2));
                sampleRate = (int)BinaryPrimitives.ReadUInt32LittleEndian(fmt.AsSpan(4, 4));
                bitsPerSample = BinaryPrimitives.ReadUInt16LittleEndian(fmt.AsSpan(14, 2));
                if (audioFormat == FormatExtensible && chunkSize >= 40)
                    audioFormat = BinaryPrimitives.ReadUInt16LittleEndian(fmt.AsSpan(24, 2));
                if (audioFormat != FormatPcm && audioFormat != FormatIeeeFloat)
                    throw new InvalidDataException($"Unsupported WAVE audio format code {audioFormat}.");
                if (bitsPerSample is not (8 or 16 or 24 or 32))
                    throw new InvalidDataException($"Unsupported WAVE bit depth {bitsPerSample}.");
                if (channels <= 0 || sampleRate <= 0) throw new InvalidDataException("WAVE fmt chunk has an invalid channel count or sample rate.");
                SkipPadding(stream, chunkSize);
            }
            else if (Matches(chunkId, "data"))
            {
                if (channels == 0) throw new InvalidDataException("WAVE data chunk appeared before fmt chunk.");
                // `position` already includes this chunk's 8-byte id+size header (advanced above), so the
                // payload starts exactly here.
                dataOffset = position;
                dataLength = chunkSize;
            }
            else
            {
                Skip(stream, chunkSize);
                SkipPadding(stream, chunkSize);
            }
            position += chunkSize + (chunkSize % 2);
        }

        return new WavHeader(audioFormat, channels, sampleRate, bitsPerSample, dataOffset, dataLength);
    }

    private static bool Matches(ReadOnlySpan<byte> tag, string ascii)
    {
        for (int i = 0; i < 4; i++) if (tag[i] != (byte)ascii[i]) return false;
        return true;
    }

    private static void SkipPadding(Stream stream, uint chunkSize) { if (chunkSize % 2 != 0) stream.Seek(1, SeekOrigin.Current); }
    private static void Skip(Stream stream, uint count) => stream.Seek(count, SeekOrigin.Current);

    private static void ReadExact(Stream stream, Span<byte> buffer)
    {
        if (!TryReadExact(stream, buffer)) throw new InvalidDataException("Unexpected end of WAVE stream.");
    }

    private static bool TryReadExact(Stream stream, Span<byte> buffer)
    {
        int total = 0;
        while (total < buffer.Length)
        {
            int read = stream.Read(buffer[total..]);
            if (read <= 0) return total == 0 ? false : throw new InvalidDataException("Unexpected end of WAVE stream.");
            total += read;
        }
        return true;
    }

    private static void ReadExact(Stream stream, byte[] buffer) => ReadExact(stream, buffer.AsSpan());
}
