using System.Text;

namespace Gens.Simulation.State;

/// <summary>
/// Folds <see cref="WorldState"/>'s ordered partitions (ADR 0004) into a stable 64-bit hash — the
/// literal mechanism the Phase 2 exit gate depends on ("the same seed plus the same ordered
/// commands produces identical event logs and state hashes across repeated headless runs"). Every
/// input is already canonically ordered, so no separate "sort before hashing" step is needed. This
/// never calls <see cref="object.GetHashCode"/> on a string: that method is randomized per process
/// in modern .NET and would silently break reproducibility across separate runs while still passing
/// every single-process test. All hashing here is over raw UTF-8 bytes and integers instead.
/// </summary>
public static class StateHasher
{
    private const ulong OffsetBasis = 14695981039346656037UL;
    private const ulong Prime = 1099511628211UL;

    public static ulong Hash(WorldState state)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var hash = OffsetBasis;
        hash = MixLong(hash, state.Date.TotalMonths);
        hash = MixLong(hash, state.RegionIds.Peek);
        hash = MixLong(hash, state.SettlementIds.Peek);
        hash = MixLong(hash, state.PlotIds.Peek);
        hash = MixLong(hash, state.HouseholdIds.Peek);
        hash = MixLong(hash, state.ActorIds.Peek);
        hash = MixLong(hash, state.CharacterIds.Peek);
        hash = MixLong(hash, state.BuildingIds.Peek);
        hash = MixLong(hash, state.ContractIds.Peek);
        hash = MixLong(hash, state.ActivityIds.Peek);
        hash = MixLong(hash, state.CommandIds.Peek);
        hash = MixLong(hash, state.EventIds.Peek);
        hash = MixLong(hash, state.ScheduledActionIds.Peek);
        hash = MixLong(hash, state.NextCommandSequenceNumber);

        // Worked-example partition (WorldState.Characters is a typed placeholder — see its own
        // doc comment); only the ordered ID sequence is hashed until a real Character record
        // exists to fold content from.
        foreach (var entry in state.Characters.InAscendingOrder())
            hash = MixLong(hash, entry.Key.Value);

        // Already ascending (due date, action ID) order (ADR 0004) via OrderedRegistry.
        foreach (var entry in state.ScheduledActions.InAscendingOrder())
        {
            hash = MixLong(hash, entry.Value.ActionId.Value);
            hash = MixLong(hash, entry.Value.DueDate.TotalMonths);
            hash = MixString(hash, entry.Value.ActorId);
            hash = MixString(hash, entry.Value.ActionType);
            hash = MixString(hash, entry.Value.PayloadJson);
            hash = MixString(hash, entry.Value.CausationId ?? string.Empty);
        }

        foreach (var entry in state.Knowledge.All())
        {
            hash = MixString(hash, entry.Key.ObserverId);
            hash = MixString(hash, entry.Key.SubjectId);
            hash = MixString(hash, entry.Key.Topic);
            hash = MixLong(hash, (long)entry.Value.Confidence);
            hash = MixLong(hash, entry.Value.AsOfDate.TotalMonths);
            hash = MixString(hash, entry.Value.ProvenanceEventId ?? string.Empty);
        }

        return hash;
    }

    private static ulong MixLong(ulong hash, long value)
    {
        foreach (var b in BitConverter.GetBytes(value))
            hash = unchecked((hash ^ b) * Prime);
        return hash;
    }

    private static ulong MixString(ulong hash, string value)
    {
        foreach (var b in Encoding.UTF8.GetBytes(value))
            hash = unchecked((hash ^ b) * Prime);

        // A length/terminator mix so ("ab","c") and ("a","bc") fold to different hashes.
        return unchecked((hash ^ 0xFF) * Prime);
    }
}
