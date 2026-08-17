using Gens.Simulation.Commands;

namespace Gens.Simulation.Events;

/// <summary>The mandatory <see cref="Visibility"/> every Events-namespace <see cref="IDomainEvent"/>
/// carries (ADR 0007, ADR 0008), computed once here so <see cref="FireEventCommands"/>, <see
/// cref="ResolveEventOptionCommands"/>, and <see cref="EventPoolSystem"/>'s own directly-emitted
/// expiry/stage events all agree. Personal-scope events are witnessed only by their named subject(s)
/// (§2's "A single named Character") — everything else defaults to <see cref="Visibility.Public"/>:
/// no per-member household roster query exists yet to scope a Household-scope event down to "known to
/// this household's own members only," so Household/Settlement/Imperial stay at the same
/// known-to-everyone default every other campaign-wide event in this codebase already uses (e.g.
/// <see cref="Policies.RitesBudgetChangedEvent"/>).</summary>
public static class EventVisibilityRules
{
    public static Visibility For(EventScope scope, IReadOnlyList<string> subjectIds) =>
        scope == EventScope.Personal ? Visibility.Private(subjectIds.ToArray()) : Visibility.Public;
}
