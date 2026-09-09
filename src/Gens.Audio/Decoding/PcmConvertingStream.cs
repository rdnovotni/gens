namespace Gens.Audio.Decoding;

/// <summary>Converts an 8/16/24/32-bit integer or 32-bit float PCM source stream to interleaved signed 16-bit little-endian PCM.</summary>
internal sealed class PcmConvertingStream : Stream
{
    private const int WavFormatIeeeFloat = 3;
    private readonly Stream inner;
    private readonly int sourceBytesPerSample;
    private readonly int sourceFormat;
    private readonly bool ownsInner;
    private readonly byte[] sourceBuffer;
    private readonly byte[] outputBuffer = new byte[8192];
    private int outputOffset;
    private int outputLength;
    private bool sourceExhausted;

    internal PcmConvertingStream(Stream inner, int sourceBitsPerSample, int sourceFormat, bool ownsInner)
    {
        this.inner = inner;
        this.sourceFormat = sourceFormat;
        this.ownsInner = ownsInner;
        sourceBytesPerSample = sourceBitsPerSample / 8;
        sourceBuffer = new byte[sourceBytesPerSample * 2048];
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int written = 0;
        while (written < count)
        {
            if (outputOffset >= outputLength && !RefillOutputBuffer()) break;
            int toCopy = Math.Min(outputLength - outputOffset, count - written);
            Buffer.BlockCopy(outputBuffer, outputOffset, buffer, offset + written, toCopy);
            outputOffset += toCopy;
            written += toCopy;
        }
        return written;
    }

    private bool RefillOutputBuffer()
    {
        if (sourceExhausted) return false;
        int maxSamples = outputBuffer.Length / 2;
        int bytesToRead = Math.Min(sourceBuffer.Length, maxSamples * sourceBytesPerSample);
        bytesToRead -= bytesToRead % sourceBytesPerSample;
        int read = ReadFully(inner, sourceBuffer, bytesToRead);
        int usableBytes = read - (read % sourceBytesPerSample);
        if (usableBytes <= 0) { sourceExhausted = true; return false; }
        if (read < bytesToRead) sourceExhausted = true;

        int sampleCount = usableBytes / sourceBytesPerSample;
        outputOffset = 0;
        outputLength = sampleCount * 2;
        for (int i = 0; i < sampleCount; i++)
        {
            short pcm16 = ConvertSample(sourceBuffer, i * sourceBytesPerSample);
            outputBuffer[i * 2] = (byte)(pcm16 & 0xFF);
            outputBuffer[i * 2 + 1] = (byte)((pcm16 >> 8) & 0xFF);
        }
        return true;
    }

    private short ConvertSample(byte[] buf, int offset) => sourceBytesPerSample switch
    {
        1 => (short)((buf[offset] - 128) * 256),
        2 => (short)(buf[offset] | (buf[offset + 1] << 8)),
        3 => Convert24(buf, offset),
        4 => sourceFormat == WavFormatIeeeFloat ? ConvertFloat32(buf, offset) : Convert32Pcm(buf, offset),
        _ => 0,
    };

    private static short Convert24(byte[] buf, int offset)
    {
        int value = buf[offset] | (buf[offset + 1] << 8) | (buf[offset + 2] << 16);
        if ((value & 0x800000) != 0) value |= unchecked((int)0xFF000000);
        return (short)(value >> 8);
    }

    private static short Convert32Pcm(byte[] buf, int offset)
    {
        int value = buf[offset] | (buf[offset + 1] << 8) | (buf[offset + 2] << 16) | (buf[offset + 3] << 24);
        return (short)(value >> 16);
    }

    private static short ConvertFloat32(byte[] buf, int offset)
    {
        float sample = BitConverter.ToSingle(buf, offset);
        float clamped = Math.Clamp(sample, -1f, 1f);
        return (short)Math.Round(clamped * short.MaxValue);
    }

    private static int ReadFully(Stream stream, byte[] buffer, int count)
    {
        int total = 0;
        while (total < count)
        {
            int read = stream.Read(buffer, total, count - total);
            if (read <= 0) break;
            total += read;
        }
        return total;
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public override void Flush() { }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing && ownsInner) inner.Dispose();
        base.Dispose(disposing);
    }
}
