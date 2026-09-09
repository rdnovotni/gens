namespace Gens.Audio.Decoding;

/// <summary>Decodes an Ogg Vorbis stream (via the MIT-licensed managed NVorbis decoder) to interleaved PCM16 little-endian.</summary>
public sealed class VorbisDecoder : IAudioFormatDecoder
{
    public bool CanDecode(ReadOnlySpan<byte> header) =>
        header.Length >= 4 && header[0] == (byte)'O' && header[1] == (byte)'g' && header[2] == (byte)'g' && header[3] == (byte)'S';

    public (PcmFormat Format, TimeSpan Duration, Func<Stream> OpenPcmStream) Decode(Func<Stream> openEncodedStream)
    {
        int channels, sampleRate;
        TimeSpan duration;
        try
        {
            using var reader = new NVorbis.VorbisReader(openEncodedStream(), closeOnDispose: true);
            channels = reader.Channels;
            sampleRate = reader.SampleRate;
            duration = reader.TotalTime;
        }
        catch (Exception ex) when (ex is not InvalidDataException)
        {
            throw new InvalidDataException("The stream is not a valid Ogg Vorbis container.", ex);
        }

        Func<Stream> openPcmStream = () => new VorbisPcmStream(new NVorbis.VorbisReader(openEncodedStream(), closeOnDispose: true));
        return (new PcmFormat(channels, sampleRate), duration, openPcmStream);
    }

    private sealed class VorbisPcmStream(NVorbis.VorbisReader reader) : Stream
    {
        private readonly float[] floatBuffer = new float[4096];

        public override int Read(byte[] buffer, int offset, int count)
        {
            int maxFloats = count / 2;
            if (maxFloats <= 0) return 0;
            int floatsToRead = Math.Min(maxFloats, floatBuffer.Length);
            int floatsRead = reader.ReadSamples(floatBuffer, 0, floatsToRead);
            for (int i = 0; i < floatsRead; i++)
            {
                short pcm16 = (short)Math.Round(Math.Clamp(floatBuffer[i], -1f, 1f) * short.MaxValue);
                buffer[offset + i * 2] = (byte)(pcm16 & 0xFF);
                buffer[offset + i * 2 + 1] = (byte)((pcm16 >> 8) & 0xFF);
            }
            return floatsRead * 2;
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
            if (disposing) reader.Dispose();
            base.Dispose(disposing);
        }
    }
}
