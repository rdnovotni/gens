using Gens.Platform.Sdl;
using NUnit.Framework;

namespace Gens.Platform.Sdl.Tests;

[TestFixture]
public sealed class SdlAudioDeviceRecoveryTests
{
    private static readonly IReadOnlyList<TimeSpan> FastBackoff = [TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(1)];

    [Test]
    public async Task ReturnsFirstNonZeroHandleFromOpenStream()
    {
        int attempts = 0;
        IntPtr result = await SdlAudioDeviceRecovery.TryReopenAsync(
            () => { attempts++; return attempts < 2 ? IntPtr.Zero : new IntPtr(42); },
            CancellationToken.None,
            FastBackoff);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(new IntPtr(42)));
            Assert.That(attempts, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task ReturnsZeroHandleWhenAllAttemptsFail()
    {
        int attempts = 0;
        IntPtr result = await SdlAudioDeviceRecovery.TryReopenAsync(() => { attempts++; return IntPtr.Zero; }, CancellationToken.None, FastBackoff);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(IntPtr.Zero));
            Assert.That(attempts, Is.EqualTo(FastBackoff.Count));
        });
    }

    [Test]
    public async Task StopsEarlyWhenCancelled()
    {
        using var cts = new CancellationTokenSource();
        int attempts = 0;
        IntPtr result = await SdlAudioDeviceRecovery.TryReopenAsync(
            () => { attempts++; cts.Cancel(); return IntPtr.Zero; },
            cts.Token,
            [TimeSpan.FromMilliseconds(1), TimeSpan.FromSeconds(30)]);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(IntPtr.Zero));
            Assert.That(attempts, Is.EqualTo(1));
        });
    }
}
