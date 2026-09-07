using Gens.Graphics;

namespace Gens.Scene2D;

public enum Easing { Linear, EaseIn, EaseOut, EaseInOut }
public enum LoopMode { Once, Loop, PingPong }
public readonly record struct Keyframe<T>(TimeSpan Time, T Value, Easing Easing = Easing.Linear);

public interface IAnimationTrack
{
    TimeSpan Duration { get; }
    void Apply(TimeSpan time);
}

public sealed class AnimationTrack<T>(IReadOnlyList<Keyframe<T>> keyframes, Action<T> setter, Func<T, T, float, T> interpolate) : IAnimationTrack
{
    private readonly Keyframe<T>[] frames = keyframes.Count > 0 ? keyframes.OrderBy(static key => key.Time).ToArray() : throw new ArgumentException("A track requires at least one keyframe.", nameof(keyframes));
    public TimeSpan Duration => frames[^1].Time;
    public void Apply(TimeSpan time)
    {
        if (time <= frames[0].Time) { setter(frames[0].Value); return; }
        for (int i = 1; i < frames.Length; i++)
        {
            if (time > frames[i].Time) continue;
            Keyframe<T> left = frames[i - 1], right = frames[i];
            double span = (right.Time - left.Time).TotalSeconds;
            float amount = span <= 0 ? 1 : (float)((time - left.Time).TotalSeconds / span);
            setter(interpolate(left.Value, right.Value, Ease(amount, right.Easing)));
            return;
        }
        setter(frames[^1].Value);
    }

    private static float Ease(float value, Easing easing) => easing switch
    {
        Easing.EaseIn => value * value,
        Easing.EaseOut => 1 - (1 - value) * (1 - value),
        Easing.EaseInOut => value < .5f ? 2 * value * value : 1 - MathF.Pow(-2 * value + 2, 2) / 2,
        _ => value,
    };
}

public sealed record AnimationClip(string Name, IReadOnlyList<IAnimationTrack> Tracks)
{
    public TimeSpan Duration => Tracks.Count == 0 ? TimeSpan.Zero : Tracks.Max(static track => track.Duration);
}

public sealed class AnimationPlayer
{
    private TimeSpan elapsed;
    public AnimationClip? Clip { get; private set; }
    public LoopMode LoopMode { get; set; }
    public bool IsPlaying { get; private set; }
    public event Action? Completed;
    public event Action? StateChanged;

    public void Play(AnimationClip clip, bool restart = true)
    {
        ArgumentNullException.ThrowIfNull(clip);
        Clip = clip;
        if (restart) elapsed = TimeSpan.Zero;
        IsPlaying = true;
        Apply();
        StateChanged?.Invoke();
    }

    public void Stop()
    {
        if (!IsPlaying) return;
        IsPlaying = false;
        StateChanged?.Invoke();
    }

    public void Update(TimeSpan delta)
    {
        if (!IsPlaying || Clip is null) return;
        ArgumentOutOfRangeException.ThrowIfLessThan(delta, TimeSpan.Zero);
        TimeSpan duration = Clip.Duration;
        if (duration <= TimeSpan.Zero) { Apply(); Complete(); return; }
        elapsed += delta;
        if (LoopMode == LoopMode.Once && elapsed >= duration) { elapsed = duration; Apply(); Complete(); return; }
        Apply();
    }

    private void Apply()
    {
        if (Clip is null) return;
        TimeSpan duration = Clip.Duration;
        TimeSpan evaluation = elapsed;
        if (duration > TimeSpan.Zero && LoopMode == LoopMode.Loop) evaluation = TimeSpan.FromTicks(elapsed.Ticks % duration.Ticks);
        else if (duration > TimeSpan.Zero && LoopMode == LoopMode.PingPong)
        {
            long phase = elapsed.Ticks % (duration.Ticks * 2);
            evaluation = TimeSpan.FromTicks(phase <= duration.Ticks ? phase : duration.Ticks * 2 - phase);
        }
        foreach (IAnimationTrack track in Clip.Tracks) track.Apply(evaluation);
    }
    private void Complete() { IsPlaying = false; Completed?.Invoke(); StateChanged?.Invoke(); }

    public static AnimationTrack<float> Scalar(IReadOnlyList<Keyframe<float>> frames, Action<float> setter) => new(frames, setter, static (a, b, t) => a + (b - a) * t);
    public static AnimationTrack<Point2> Point(IReadOnlyList<Keyframe<Point2>> frames, Action<Point2> setter) => new(frames, setter, static (a, b, t) => new(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t));
    public static AnimationTrack<Color> Color(IReadOnlyList<Keyframe<Color>> frames, Action<Color> setter) => new(frames, setter, static (a, b, t) => new(Lerp(a.R, b.R, t), Lerp(a.G, b.G, t), Lerp(a.B, b.B, t), Lerp(a.A, b.A, t)));
    private static byte Lerp(byte a, byte b, float t) => (byte)Math.Clamp((int)MathF.Round(a + (b - a) * t), 0, 255);
}

public sealed class AnimatedSprite2D : SceneNode2D
{
    private TimeSpan elapsed;
    private int frameIndex;
    private bool reverse;
    private IReadOnlyList<SpriteFrame> frames = [];
    public IReadOnlyList<SpriteFrame> Frames
    {
        get => frames;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value.Any(static frame => frame.Duration <= TimeSpan.Zero)) throw new ArgumentException("Sprite frame durations must be positive.", nameof(value));
            frames = value; frameIndex = 0; elapsed = TimeSpan.Zero; reverse = false; Invalidate();
        }
    }
    public LoopMode LoopMode { get; set; } = LoopMode.Loop;
    public bool IsPlaying { get; private set; }
    public Size2 Size { get; set; }
    public Point2 Anchor { get; set; } = new(.5f, .5f);
    public void Play() { IsPlaying = true; Invalidate(); }
    public void Stop() { IsPlaying = false; }
    public void Update(TimeSpan delta)
    {
        if (!IsPlaying || Frames.Count == 0) return;
        ArgumentOutOfRangeException.ThrowIfLessThan(delta, TimeSpan.Zero);
        elapsed += delta;
        while (elapsed >= Frames[frameIndex].Duration)
        {
            elapsed -= Frames[frameIndex].Duration;
            frameIndex += reverse ? -1 : 1;
            if (frameIndex >= Frames.Count)
            {
                if (LoopMode == LoopMode.Once) { frameIndex = Frames.Count - 1; IsPlaying = false; break; }
                if (LoopMode == LoopMode.PingPong && Frames.Count > 1) { frameIndex = Frames.Count - 2; reverse = true; }
                else frameIndex = 0;
            }
            else if (frameIndex < 0) { frameIndex = Frames.Count > 1 ? 1 : 0; reverse = false; }
        }
        Invalidate();
    }
    protected override void RenderSelf(ICanvas2D canvas, float opacity)
    {
        if (Frames.Count == 0) return;
        IGraphicsImage image = Frames[frameIndex].Image;
        float width = Size.Width > 0 ? Size.Width : image.Width, height = Size.Height > 0 ? Size.Height : image.Height;
        canvas.DrawImage(image, new(-Anchor.X * width, -Anchor.Y * height, width, height), opacity);
    }
}

public readonly record struct SpriteFrame(IGraphicsImage Image, TimeSpan Duration);
