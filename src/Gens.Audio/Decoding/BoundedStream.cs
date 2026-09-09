namespace Gens.Audio.Decoding;

/// <summary>A read-only forward view over a limited byte range of an inner stream, optionally owning it.</summary>
internal sealed class BoundedStream : Stream
{
    private readonly Stream inner;
    private readonly long totalLength;
    private readonly bool ownsInner;
    private long remaining;

    internal BoundedStream(Stream inner, long length, bool ownsInner)
    {
        this.inner = inner;
        totalLength = length;
        this.ownsInner = ownsInner;
        remaining = length;
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => totalLength;
    public override long Position { get => totalLength - remaining; set => throw new NotSupportedException(); }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (remaining <= 0) return 0;
        int toRead = (int)Math.Min(count, remaining);
        int read = inner.Read(buffer, offset, toRead);
        remaining -= read;
        return read;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (remaining <= 0) return 0;
        int toRead = (int)Math.Min(buffer.Length, remaining);
        int read = await inner.ReadAsync(buffer[..toRead], cancellationToken).ConfigureAwait(false);
        remaining -= read;
        return read;
    }

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
