using Gens.Simulation.Identity;

namespace Gens.Simulation.Correspondence;

/// <summary>§7's Oral Tradition Problem, generalized into a three-value scale rather than a binary
/// "can/can't write" flag — matching §7's own explicit "not a hard, universal wall" framing (an
/// Interpreter-equivalent can partially close the gap short of full literacy).</summary>
public enum CorrespondenceReachability
{
    /// <summary>No penalty — Parthia, Egypt, the Hellenic world, and every other genuinely literate
    /// culture on Cultures' own roster (§7's closing paragraph). Also this catalog's own default for
    /// any culture with no authored entry — see <see cref="CorrespondenceReachabilityCatalog"/>'s own
    /// doc comment for why that is the honest default rather than a cautious one.</summary>
    FullyLiterate,

    /// <summary>Meaningfully reduced effectiveness for <see cref="LetterActions.IsSubstantive"/>
    /// content specifically (§7: Gallic/British/Germanic religious or druidic-adjacent leadership, and
    /// Cultures' own Thin-Record peoples by extension) — routine correspondence still works fine.</summary>
    OralTraditionPartial,

    /// <summary>The extreme case (§7: "some content simply cannot be transmitted this way at all") —
    /// a <see cref="LetterActions.IsSubstantive"/> action cannot be sent by letter at all when the
    /// counterparty resolves to this level; routine correspondence is still unaffected.</summary>
    OralTraditionBlocked,
}

/// <summary>One authored (culture, reachability) entry.</summary>
public sealed record CultureReachabilityEntry(
    DefinitionId<Culture> CultureId,
    CorrespondenceReachability Reachability);

/// <summary>The general lookup mechanism §7 needs, structurally ready for Cultures of the Known
/// World's real thirty-six-entry roster (Phase 13 item 4, not yet built) — mirrors <see
/// cref="Travel.DistanceTierCatalog"/>'s identical "the general mechanism that table hangs off, not
/// that table itself" shape. This item deliberately authors no real culture content: every actual
/// (culture, reachability) pairing — which of the thirty-six cultures are Gallic/British/Germanic or
/// Thin-Record, and Interpreter-equivalent mitigation (§7's own closing paragraph) — is explicitly
/// item 4's job, per §12's own open question ("whether every one of Cultures' own thirty-six entries
/// needs an explicit yes/no flag... isn't fully resolved").</summary>
public sealed class CorrespondenceReachabilityCatalog
{
    private readonly Dictionary<string, CorrespondenceReachability> _entries;

    public CorrespondenceReachabilityCatalog(IEnumerable<CultureReachabilityEntry> entries)
    {
        if (entries is null)
            throw new ArgumentNullException(nameof(entries));

        var map = new Dictionary<string, CorrespondenceReachability>(StringComparer.Ordinal);
        foreach (var entry in entries)
        {
            if (!map.TryAdd(entry.CultureId.Value, entry.Reachability))
                throw new ArgumentException($"Duplicate correspondence reachability entry for culture '{entry.CultureId.Value}'.", nameof(entries));
        }

        _entries = map;
    }

    /// <summary>A culture with no authored entry defaults to <see
    /// cref="CorrespondenceReachability.FullyLiterate"/> — §7's own closing line ("this constraint is
    /// specific and honest, not a blanket 'foreigners can't read' assumption") makes literate-by-default
    /// the honest reading, unlike <see cref="Travel.DistanceTierCatalog"/>'s own least-committal-middle-
    /// tier default for an unrelated question. A null <paramref name="cultureId"/> (a sender/recipient
    /// whose culture this caller doesn't know or care to check) resolves the same way.</summary>
    public CorrespondenceReachability Resolve(DefinitionId<Culture>? cultureId)
    {
        if (cultureId is null)
            return CorrespondenceReachability.FullyLiterate;

        return _entries.TryGetValue(cultureId.Value.Value, out var reachability)
            ? reachability
            : CorrespondenceReachability.FullyLiterate;
    }
}
