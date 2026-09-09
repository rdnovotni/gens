namespace Gens.Platform.Sdl;

/// <summary>
/// Retry policy used to recover an SDL audio stream after a default-output-device change or disconnect causes a
/// write failure. Extracted from <see cref="SdlAudioBackend"/> so the backoff/retry behavior is unit-testable
/// without a real SDL audio device.
/// </summary>
internal static class SdlAudioDeviceRecovery
{
    internal static readonly IReadOnlyList<TimeSpan> DefaultBackoff =
    [
        TimeSpan.FromMilliseconds(50),
        TimeSpan.FromMilliseconds(150),
        TimeSpan.FromMilliseconds(400),
    ];

    /// <summary>Calls <paramref name="openStream"/> after each backoff delay until it returns a non-null handle or the attempts are exhausted.</summary>
    internal static async Task<IntPtr> TryReopenAsync(Func<IntPtr> openStream, CancellationToken cancellationToken, IReadOnlyList<TimeSpan>? backoff = null)
    {
        foreach (TimeSpan delay in backoff ?? DefaultBackoff)
        {
            try { await Task.Delay(delay, cancellationToken).ConfigureAwait(false); }
            catch (OperationCanceledException) { return IntPtr.Zero; }
            IntPtr candidate = openStream();
            if (candidate != IntPtr.Zero) return candidate;
        }
        return IntPtr.Zero;
    }
}
