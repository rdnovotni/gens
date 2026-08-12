namespace Gens.Simulation.Identity;

// Phantom marker types for RuntimeId<T> and DefinitionId<T>. Never instantiated — only used as a
// compile-time tag so, e.g., a RuntimeId<Character> can never be passed where a RuntimeId<Plot> is
// expected (ADR 0001). Runtime-instantiated kinds (created during a campaign) get RuntimeId<T>;
// content-authored kinds (defined ahead of time, never runtime-generated) get DefinitionId<T>.

public sealed class Campaign
{
    private Campaign()
    {
    }
}

public sealed class Region
{
    private Region()
    {
    }
}

public sealed class Settlement
{
    private Settlement()
    {
    }
}

public sealed class Plot
{
    private Plot()
    {
    }
}

public sealed class Household
{
    private Household()
    {
    }
}

public sealed class Actor
{
    private Actor()
    {
    }
}

public sealed class Character
{
    private Character()
    {
    }
}

public sealed class Good
{
    private Good()
    {
    }
}

public sealed class Building
{
    private Building()
    {
    }
}

public sealed class Contract
{
    private Contract()
    {
    }
}

public sealed class Activity
{
    private Activity()
    {
    }
}

/// <summary>Phantom type for command IDs (ADR 0006). Not one of the roadmap's listed content/runtime kinds.</summary>
public sealed class Command
{
    private Command()
    {
    }
}

/// <summary>Phantom type for domain event IDs (ADR 0007). Not one of the roadmap's listed content/runtime kinds.</summary>
public sealed class DomainEventEntity
{
    private DomainEventEntity()
    {
    }
}

/// <summary>Phantom type for scheduled-action IDs (Phase 4 item 4: "scheduled actions and a calendar
/// queue for future-dated work"). Not one of the roadmap's listed content/runtime kinds.</summary>
public sealed class ScheduledAction
{
    private ScheduledAction()
    {
    }
}

/// <summary>Maps each entity-kind phantom type to its short save-file tag, e.g. <c>Character</c> → <c>char</c>.</summary>
internal static class RuntimeIdTagRegistry
{
    private static readonly Dictionary<Type, string> Tags = new()
    {
        [typeof(Campaign)] = "campaign",
        [typeof(Region)] = "region",
        [typeof(Settlement)] = "settlement",
        [typeof(Plot)] = "plot",
        [typeof(Household)] = "household",
        [typeof(Actor)] = "actor",
        [typeof(Character)] = "char",
        [typeof(Building)] = "building",
        [typeof(Contract)] = "contract",
        [typeof(Activity)] = "activity",
        [typeof(Command)] = "cmd",
        [typeof(DomainEventEntity)] = "event",
        [typeof(ScheduledAction)] = "action",
    };

    public static string Resolve(Type type) =>
        Tags.TryGetValue(type, out var tag)
            ? tag
            : throw new InvalidOperationException($"No RuntimeId tag is registered for entity kind '{type.Name}'.");
}

/// <summary>Caches the resolved tag for entity kind <typeparamref name="T"/> once per closed generic type.</summary>
internal static class RuntimeIdTag<T>
{
    public static readonly string Tag = RuntimeIdTagRegistry.Resolve(typeof(T));
}
