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
        IntPtr stream = OpenDeviceStream(spec);
        if (stream == IntPtr.Zero) return null;
        var voice = new SdlAudioVoice(stream, spec, clip, shouldLoop, () => voices.RemoveAll(candidate => candidate.IsDisposed));
        voices.Add(voice); voice.Start(); return voice;
    }
    public void Dispose() { if (disposed) return; foreach (SdlAudioVoice voice in voices.ToArray()) voice.Dispose(); voices.Clear(); disposed = true; }

    private static IntPtr OpenDeviceStream(SdlNative.AudioSpec spec) => SdlNative.SDL_OpenAudioDeviceStream(SdlNative.DefaultPlaybackDevice, ref spec, IntPtr.Zero, IntPtr.Zero);

    private sealed class SdlAudioVoice(IntPtr stream, SdlNative.AudioSpec spec, AudioClip clip, bool shouldLoop, Action completed) : IBackendAudioVoice
    {
        private readonly object gate = new();
        private readonly CancellationTokenSource cancellation = new();
        private IntPtr stream = stream;
        private Task? feeder;
        private bool disposed;
        private float gain = 1f;
        public bool IsPlaying { get; private set; }
        public bool IsDisposed => disposed;
        public void Start() { IsPlaying = true; feeder = Task.Run(FeedAsync); SdlNative.SDL_ResumeAudioStreamDevice(stream); }
        public void SetGain(float value) { float clamped = Math.Clamp(value, 0, 1); lock (gate) { gain = clamped; if (!disposed) SdlNative.SDL_SetAudioStreamGain(stream, clamped); } }
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
                        if (!await WriteWithRecoveryAsync(buffer, read).ConfigureAwait(false)) return;
                    }
                } while (shouldLoop && !cancellation.IsCancellationRequested);
            }
            catch (OperationCanceledException) { }
            finally { if (!shouldLoop) lock (gate) if (!disposed) IsPlaying = false; }
        }

        /// <summary>
        /// Writes one decoded chunk to the device stream. If the write fails (e.g. the default output device was
        /// disconnected or changed), destroys the stale stream and retries reopening against the current default
        /// device with backoff before giving up and stopping the voice — this keeps a default-device change from
        /// silently hanging or crashing playback.
        /// </summary>
        private async Task<bool> WriteWithRecoveryAsync(byte[] buffer, int length)
        {
            if (TryWrite(buffer, length)) return true;

            lock (gate) { if (!disposed) { SdlNative.SDL_DestroyAudioStream(stream); stream = IntPtr.Zero; } }
            IntPtr reopened = await SdlAudioDeviceRecovery.TryReopenAsync(() => OpenDeviceStream(spec), cancellation.Token).ConfigureAwait(false);
            if (reopened == IntPtr.Zero) return false;

            lock (gate)
            {
                if (disposed) { SdlNative.SDL_DestroyAudioStream(reopened); return false; }
                stream = reopened;
                // The replacement stream starts at SDL's default gain (1.0); reapply whatever this voice was
                // last set to (master/bus/fade/per-voice volume) so recovery doesn't cause an audible jump.
                SdlNative.SDL_SetAudioStreamGain(stream, gain);
                SdlNative.SDL_ResumeAudioStreamDevice(stream);
            }
            return TryWrite(buffer, length);
        }

        private bool TryWrite(byte[] buffer, int length)
        {
            IntPtr native = Marshal.AllocHGlobal(length);
            try
            {
                Marshal.Copy(buffer, 0, native, length);
                lock (gate) return !disposed && SdlNative.SDL_PutAudioStreamData(stream, native, length);
            }
            finally { Marshal.FreeHGlobal(native); }
        }
    }
}
