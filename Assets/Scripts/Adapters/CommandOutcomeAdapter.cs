#nullable enable

using Gens.Simulation.Commands;

namespace Gens.Presentation.Adapters;

/// <summary>Display-ready outcome of one command submission (ADR 0013's command path): UI code never
/// branches on <see cref="CommandResult"/>'s domain shape directly, it reads this instead.</summary>
public readonly record struct CommandOutcomeViewModel(bool Accepted, string? ErrorCode);

/// <summary>Worked example translating a <see cref="CommandResult"/> into presentation-ready shape.
/// Deliberately exposes only the stable <see cref="ValidationErrorCode"/> string, never the emitted
/// <see cref="IDomainEvent"/> list — screens read the effects of an accepted command back out through
/// a query, not by inspecting the events a command happened to return.</summary>
public sealed class CommandOutcomeAdapter : IProjectionAdapter<CommandResult, CommandOutcomeViewModel>
{
    public CommandOutcomeViewModel Adapt(CommandResult projection) =>
        new(projection.Accepted, projection.Error?.ToString());
}
