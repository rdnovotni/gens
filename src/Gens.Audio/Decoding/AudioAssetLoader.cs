namespace Gens.Audio.Decoding;

/// <summary>Sniffs an encoded audio stream's container format and produces a ready-to-play <see cref="AudioClip"/>.</summary>
public sealed class AudioAssetLoader
{
    private readonly IReadOnlyList<IAudioFormatDecoder> decoders;

    public AudioAssetLoader(IReadOnlyList<IAudioFormatDecoder>? decoders = null) => this.decoders = decoders ?? [new WavDecoder(), new VorbisDecoder()];

    /// <summary>
    /// Decodes the container from <paramref name="openEncodedStream"/> and returns an <see cref="AudioClip"/> whose
    /// <c>OpenStream</c> yields raw interleaved PCM16 little-endian data. For <see cref="AudioStorage.Buffered"/>
    /// clips the PCM is decoded once into memory; for <see cref="AudioStorage.Streaming"/> clips each call to
    /// <c>OpenStream</c> re-runs decoding from the start of the source, matching how <c>SdlAudioBackend</c> replays
    /// looping voices.
    /// </summary>
    public AudioClip LoadClip(string id, Func<Stream> openEncodedStream, AudioStorage storage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(openEncodedStream);

        byte[] header = new byte[12];
        using (Stream probe = openEncodedStream())
        {
            int read = 0;
            while (read < header.Length)
            {
                int n = probe.Read(header, read, header.Length - read);
                if (n <= 0) break;
                read += n;
            }
            if (read < header.Length) Array.Resize(ref header, read);
        }

        IAudioFormatDecoder? decoder = decoders.FirstOrDefault(d => d.CanDecode(header));
        if (decoder is null) throw new InvalidDataException($"Audio asset '{id}' is not a recognized WAV or Ogg Vorbis container.");

        (PcmFormat format, TimeSpan duration, Func<Stream> openPcmStream) = decoder.Decode(openEncodedStream);

        if (storage == AudioStorage.Streaming) return new AudioClip(id, duration, format.Channels, format.SampleRate, storage, openPcmStream);

        byte[] pcm;
        using (Stream pcmStream = openPcmStream())
        using (var buffer = new MemoryStream())
        {
            pcmStream.CopyTo(buffer);
            pcm = buffer.ToArray();
        }
        return new AudioClip(id, duration, format.Channels, format.SampleRate, storage, () => new MemoryStream(pcm, writable: false));
    }
}
