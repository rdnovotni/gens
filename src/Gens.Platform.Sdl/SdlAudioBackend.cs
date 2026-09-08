using System.Runtime.InteropServices;
using Gens.Audio;
using Gens.Platform.Sdl.Interop;

namespace Gens.Platform.Sdl;

/// <summary>SDL3 push-stream backend for interleaved signed 16-bit PCM supplied by AudioClip.OpenStream.</summary>
public sealed class SdlAudioBackend : IAudioBackend
{
    private readonly List<SdlAudioVoice> voices = [];
    private bool disposed;
    public SdlAudioBackend()
    {
        IsAvailable = SdlNative.SDL_InitSubSystem(SdlNative.InitAudio);
        if (!IsAvailable) UnavailableReason = SdlNative.Error;
    }
    public string Name => IsAvailable ? "SDL3 audio (PCM16)" : "SDL3 audio unavailable";
    public bool IsAvailable { get; }
    public string? UnavailableReason { get; }
    public IBackendAudioVoice? StartPlayback(AudioClip clip, bool shouldLoop)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (!IsAvailable) return null;
        var spec = new SdlNative.AudioSpec { Format = SdlNative.AudioS16LittleEndian, Channels = clip.Channels, Frequency = clip.SampleRate };
        IntPtr stream = SdlNative.SDL_OpenAudioDeviceStream(SdlNative.DefaultPlaybackDevice, ref spec, IntPtr.Zero, IntPtr.Zero);
        if (stream == IntPtr.Zero) return null;
        var voice = new SdlAudioVoice(stream, clip, shouldLoop, () => voices.RemoveAll(candidate => candidate.IsDisposed));
        voices.Add(voice); voice.Start(); return voice;
    }
    public void Dispose() { if (disposed) return; foreach (SdlAudioVoice voice in voices.ToArray()) voice.Dispose(); voices.Clear(); disposed = true; }

    private sealed class SdlAudioVoice(IntPtr stream, AudioClip clip, bool shouldLoop, Action completed) : IBackendAudioVoice
    {
        private readonly object gate = new();
        private readonly CancellationTokenSource cancellation = new();
        private Task? feeder;
        private bool disposed;
        public bool IsPlaying { get; private set; }
        public bool IsDisposed => disposed;
        public void Start() { IsPlaying = true; feeder = Task.Run(FeedAsync); SdlNative.SDL_ResumeAudioStreamDevice(stream); }
        public void SetGain(float gain) { lock (gate) if (!disposed) SdlNative.SDL_SetAudioStreamGain(stream, Math.Clamp(gain, 0, 1)); }
        public void Pause() { lock (gate) if (!disposed && SdlNative.SDL_PauseAudioStreamDevice(stream)) IsPlaying = false; }
        public void ResumePlayback() { lock (gate) if (!disposed && SdlNative.SDL_ResumeAudioStreamDevice(stream)) IsPlaying = true; }
        public void StopPlayback() => Dispose();
        public void Dispose()
        {
            lock (gate) { if (disposed) return; disposed = true; IsPlaying = false; cancellation.Cancel(); }
            try { feeder?.Wait(TimeSpan.FromSeconds(1)); } catch (AggregateException) { }
            lock (gate) SdlNative.SDL_DestroyAudioStream(stream); cancellation.Dispose(); completed();
        }
        private async Task FeedAsync()
        {
            byte[] buffer = new byte[32 * 1024];
            try
            {
                do
                {
                    using Stream source = clip.OpenStream();
                    int read;
                    while ((read = await source.ReadAsync(buffer.AsMemory(), cancellation.Token).ConfigureAwait(false)) > 0)
                    {
                        IntPtr native = Marshal.AllocHGlobal(read);
                        try { Marshal.Copy(buffer, 0, native, read); lock (gate) if (!disposed && !SdlNative.SDL_PutAudioStreamData(stream, native, read)) return; }
                        finally { Marshal.FreeHGlobal(native); }
                    }
                } while (shouldLoop && !cancellation.IsCancellationRequested);
            }
            catch (OperationCanceledException) { }
            finally { if (!shouldLoop) lock (gate) if (!disposed) IsPlaying = false; }
        }
    }
}
