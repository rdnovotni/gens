namespace Gens.Audio.Decoding;

/// <summary>Interleaved PCM16 little-endian format description produced by every <see cref="IAudioFormatDecoder"/>.</summary>
public readonly record struct PcmFormat(int Channels, int SampleRate);

/// <summary>Decodes one encoded audio container into interleaved PCM16 little-endian data.</summary>
public interface IAudioFormatDecoder
{
    /// <summary>Sniffs the leading bytes of a stream to decide whether this decoder understands the container.</summary>
    bool CanDecode(ReadOnlySpan<byte> header);

    /// <summary>
    /// Decodes the stream produced by <paramref name="openEncodedStream"/>. The returned <c>OpenPcmStream</c>
    /// factory re-runs decoding from the start of the source each time it is invoked, so it is safe to use for
    /// both buffered (decode once) and looping/streaming (decode repeatedly) playback.
    /// </summary>
    (PcmFormat Format, TimeSpan Duration, Func<Stream> OpenPcmStream) Decode(Func<Stream> openEncodedStream);
}
