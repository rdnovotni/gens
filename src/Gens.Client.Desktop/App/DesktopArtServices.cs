using Gens.Art.Cache;
using Gens.Art;
using Gens.Art.Portraits;
using Gens.Art.Providers;
using Gens.Art.Queue;
using Gens.Art.Diagnostics;
using Gens.Graphics;
using Gens.Portraits;

namespace Gens.Client.Desktop.App;

internal sealed class DesktopArtServices : IDisposable
{
    private readonly GeneratedArtCache cache;
    private readonly ArtGenerationQueue queue;
    private bool disposed;

    public DesktopArtServices(IGraphicsBackend graphics, DesktopApplicationController controller)
    {
        cache = new(Path.Combine(controller.PortraitCachePath, "generated"));
        IArtProvider provider = new MockArtProvider(new(TimeSpan.FromMilliseconds(750), MaximumConcurrentRequests: 2));
        queue = new(provider, cache, new(controller.Settings.Art.MaximumConcurrentRequests, controller.Settings.Art.MaximumAutomaticRetries));
        Coordinator = new(queue, cache);
        Portraits = new(graphics, controller.PortraitCachePath, Coordinator);
        Diagnostics = new(queue, cache, provider);
    }

    public GeneratedPortraitCoordinator Coordinator { get; }
    public PortraitService Portraits { get; }
    public ArtDiagnostics Diagnostics { get; }

    public void Dispose()
    {
        if (disposed) return;
        Portraits.Dispose();
        queue.DisposeAsync().AsTask().GetAwaiter().GetResult();
        cache.Dispose();
        disposed = true;
    }
}
