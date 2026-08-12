namespace Gens.ContentCompiler.Content;

public sealed record ContentValidationError(string Family, string? Id, string Message)
{
    public override string ToString() => Id is null ? $"[{Family}] {Message}" : $"[{Family}:{Id}] {Message}";
}
