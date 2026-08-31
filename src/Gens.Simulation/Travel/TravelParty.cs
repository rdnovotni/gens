using Gens.Simulation.Characters;
using Gens.Simulation.Identity;

namespace Gens.Simulation.Travel;

/// <summary>§10's <c>travelerCharacterId</c> plus <c>retinueCharacterIds</c>: who is actually making
/// the trip. The Retinue itself remains Companions &amp; Court Positions §7's mechanic, "recap, not
/// redesign" (§6) — this record carries only the IDs; what each member contributes and how recruitment
/// works is that document's own, not-yet-built territory.</summary>
public sealed record TravelParty
{
    private TravelParty(RuntimeId<Character> travelerId, IReadOnlyList<RuntimeId<Character>> retinueIds)
    {
        TravelerId = travelerId;
        RetinueIds = retinueIds;
    }

    public RuntimeId<Character> TravelerId { get; }
    public IReadOnlyList<RuntimeId<Character>> RetinueIds { get; }

    /// <summary>Every Character this trip reserves (§5) — the traveler plus every retinue member.</summary>
    public IEnumerable<RuntimeId<Character>> AllMembers => RetinueIds.Prepend(TravelerId);

    public static TravelParty Create(RuntimeId<Character> travelerId, IReadOnlyList<RuntimeId<Character>>? retinueIds = null)
    {
        var retinue = retinueIds ?? Array.Empty<RuntimeId<Character>>();
        if (retinue.Contains(travelerId))
            throw new ArgumentException("The traveler cannot also be their own retinue member.", nameof(retinueIds));
        if (retinue.Distinct().Count() != retinue.Count)
            throw new ArgumentException("A travel party's retinue members must not repeat.", nameof(retinueIds));

        return new TravelParty(travelerId, retinue);
    }

    public bool Equals(TravelParty? other) =>
        other is not null && TravelerId == other.TravelerId && RetinueIds.SequenceEqual(other.RetinueIds);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(TravelerId);
        foreach (var id in RetinueIds)
            hash.Add(id);
        return hash.ToHashCode();
    }
}
