using System.Diagnostics;
using Gens.Graphics;
using Gens.Graphics.Skia;
using Gens.Platform;
using Gens.Portraits;
using Gens.Presentation.Visuals;
using Gens.Scene2D;
using Gens.Simulation.Characters;

using var graphics = new SkiaGraphicsBackend();
Console.WriteLine("Gens visual benchmark (software reference backend)");
foreach ((int width, int height) in new[] { (1920, 1080), (3840, 2160) })
    foreach (int count in new[] { 100, 500, 1000 }) MeasureScene(width, height, count);
CharacterVisualState visual = SampleVisual(); Gens.Presentation.Visuals.PortraitRecipe recipe = PortraitRecipeBuilder.Build(visual); var renderer = new ProceduralPortraitRenderer(graphics);
foreach (int size in new[] { 128, 256, 512 }) MeasurePortrait(size);
foreach (int count in new[] { 100, 1000 }) MeasureRecipeBatch(count);

void MeasureScene(int width, int height, int count)
{
    using IRenderSurface surface = graphics.CreateOffscreenSurface(new PixelSize(width, height));
    var scene = new Gens.Scene2D.Scene2D(); scene.Camera.Position = new(width / 2f, height / 2f);
    for (int i = 0; i < count; i++) scene.Root.AddChild(new RectangleNode2D { Rectangle = new((i * 47) % (width + 400) - 200, (i * 83) % (height + 400) - 200, 28, 28), Color = new((byte)(70 + i % 150), (byte)(50 + i % 100), 45), ZOrder = i % 7 });
    var players = new List<AnimationPlayer>();
    for (int i = 0; i < 12; i++) { var node = new RectangleNode2D(); var player = new AnimationPlayer { LoopMode = LoopMode.Loop }; player.Play(new("motion", new IAnimationTrack[] { AnimationPlayer.Scalar(new[] { new Keyframe<float>(TimeSpan.Zero, 0), new Keyframe<float>(TimeSpan.FromSeconds(1), 1) }, value => node.Opacity = value) })); scene.AddAnimation(player); players.Add(player); }
    for (int i = 0; i < 3; i++) Draw();
    long before = GC.GetAllocatedBytesForCurrentThread(); long updateStart = Stopwatch.GetTimestamp(); for (int i = 0; i < 30; i++) scene.Update(TimeSpan.FromMilliseconds(16)); double updateMs = Stopwatch.GetElapsedTime(updateStart).TotalMilliseconds / 30;
    long renderStart = Stopwatch.GetTimestamp(); for (int i = 0; i < 10; i++) Draw(); double renderMs = Stopwatch.GetElapsedTime(renderStart).TotalMilliseconds / 10; long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
    Console.WriteLine($"scene {width}x{height} nodes={count} update_ms={updateMs:F3} render_ms={renderMs:F3} allocated_bytes_per_iteration={allocated / 40}");
    void Draw() { using IRenderFrame frame = surface.BeginFrame(); frame.Canvas.Clear(new(25, 20, 17)); scene.Render(frame.Canvas, new(0, 0, width, height)); frame.Present(); }
}

void MeasurePortrait(int size)
{
    long before = GC.GetAllocatedBytesForCurrentThread(); long start = Stopwatch.GetTimestamp(); using IGraphicsImage image = renderer.Render(recipe, size).Image; double elapsed = Stopwatch.GetElapsedTime(start).TotalMilliseconds; long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
    string cachePath = Path.Combine(Path.GetTempPath(), "gens-visual-benchmark", Guid.NewGuid().ToString("N"));
    double memoryMs;
    using (var service = new PortraitService(graphics, cachePath)) { service.Resolve(visual, size); long cacheStart = Stopwatch.GetTimestamp(); ResolvedPortrait cached = service.Resolve(visual, size); memoryMs = Stopwatch.GetElapsedTime(cacheStart).TotalMilliseconds; if (!cached.CacheHit) throw new InvalidOperationException("Expected a memory cache hit."); }
    double diskMs;
    using (var diskService = new PortraitService(graphics, cachePath)) { long diskStart = Stopwatch.GetTimestamp(); ResolvedPortrait cached = diskService.Resolve(visual, size); diskMs = Stopwatch.GetElapsedTime(diskStart).TotalMilliseconds; if (!cached.CacheHit) throw new InvalidOperationException("Expected a disk cache hit."); }
    Directory.Delete(cachePath, true);
    Console.WriteLine($"portrait size={size} cold_ms={elapsed:F3} memory_cache_ms={memoryMs:F3} disk_cache_ms={diskMs:F3} allocated_bytes={allocated}");
}

void MeasureRecipeBatch(int count)
{
    long before = GC.GetAllocatedBytesForCurrentThread(); long start = Stopwatch.GetTimestamp(); string last = string.Empty;
    for (int i = 0; i < count; i++)
    {
        CharacterVisualState state = visual with { CharacterId = $"benchmark:character:{i}", VisualSeed = CharacterVisualStateProjector.DeriveSeed($"benchmark:character:{i}") };
        last = PortraitRecipeBuilder.Build(state).CanonicalIdentity;
    }
    double elapsed = Stopwatch.GetElapsedTime(start).TotalMilliseconds; long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
    Console.WriteLine($"recipes count={count} total_ms={elapsed:F3} per_recipe_us={elapsed * 1000 / count:F3} allocated_bytes_per_recipe={allocated / count} guard={last.Length}");
}

static CharacterVisualState SampleVisual()
{
    var profile = new CharacterVisualProfile { Height = Height.Tall, Build = Build.Muscular, FacialStructure = FacialStructure.Square, Complexion = Complexion.Bronzed, HairColor = HairColor.Black, HairStyle = HairStyle.Cropped, EyeColor = EyeColor.Brown, NotableFeatures = new[] { NotableFeature.Scar, NotableFeature.BrokenNose }, Portrait = PortraitRecipeGenerator.Generate(Height.Tall, Build.Muscular, FacialStructure.Square, Complexion.Bronzed, HairColor.Black, HairStyle.Cropped, EyeColor.Brown, new[] { NotableFeature.Scar, NotableFeature.BrokenNose }) };
    return CharacterVisualStateProjector.Project("benchmark:character", Sex.Male, 48, profile, LegalStatus.RomanCitizen, SocialClass.Senatorial, DutySlot.FieldHand);
}
