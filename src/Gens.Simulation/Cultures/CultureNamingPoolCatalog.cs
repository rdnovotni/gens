using Gens.Simulation.Characters;
using Gens.Simulation.Identity;

namespace Gens.Simulation.Cultures;

/// <summary>
/// §13's "real patterns, not exhaustive lists" naming-convention content, turned into real, drawable
/// <see cref="NamePool"/> entries for <see cref="CharacterNameGenerator.Generate"/> — the concrete
/// integration point <see cref="NamePool"/>'s own doc comment names as future work this item closes for
/// every culture §13 itself actually calls out by name (Gallic/British/Hibernian's <c>-rix</c> suffix,
/// Germanic compounds, Egyptian theophoric forms, Judaean patronymic/theophoric Hebrew forms,
/// Parthian/Armenian Persian-derived elements, and Etruscan's own distinct non-Indo-European structure).
/// Per §13's own title, this catalog is deliberately not exhaustive across all thirty-six cultures — only
/// the seven §13 itself names get a real, authored pool; every other culture is free for a future pass to
/// add without this item inventing naming content §13 never actually specifies.
/// </summary>
public static class CultureNamingPoolCatalog
{
    public static readonly NamePool Roman = new()
    {
        Praenomina = new[] { "Marcus", "Gaius", "Lucius", "Quintus", "Titus" },
        Nomina = new[] { "Aurelius", "Cornelius", "Julius", "Claudius", "Fabius" },
        Cognomina = new[] { "Rufus", "Longus", "Maximus", "Severus" },
        GivenNames = new[] { "Felix", "Fortunata" },
    };

    /// <summary>Gallic/British/Hibernian's shared real, attested Celtic <c>-rix</c> ("king") suffix
    /// (§13, e.g. the historical Vercingetorix).</summary>
    public static readonly NamePool GallicBritishHibernian = new()
    {
        Praenomina = Array.Empty<string>(),
        Nomina = Array.Empty<string>(),
        Cognomina = Array.Empty<string>(),
        GivenNames = new[] { "Vercingetorix", "Dumnorix", "Cunobelinus", "Cassivellaunus", "Boudicca" },
    };

    /// <summary>Real two-element Germanic compounds (§13).</summary>
    public static readonly NamePool Germanic = new()
    {
        Praenomina = Array.Empty<string>(),
        Nomina = Array.Empty<string>(),
        Cognomina = Array.Empty<string>(),
        GivenNames = new[] { "Arminius", "Segestes", "Thusnelda", "Hariomannus" },
    };

    /// <summary>Real theophoric Egyptian constructions (§13).</summary>
    public static readonly NamePool Egyptian = new()
    {
        Praenomina = Array.Empty<string>(),
        Nomina = Array.Empty<string>(),
        Cognomina = Array.Empty<string>(),
        GivenNames = new[] { "Horemheb", "Thutmose", "Isidora", "Harpocration" },
    };

    /// <summary>Real patronymic or theophoric Hebrew forms (§13).</summary>
    public static readonly NamePool Judaean = new()
    {
        Praenomina = Array.Empty<string>(),
        Nomina = Array.Empty<string>(),
        Cognomina = Array.Empty<string>(),
        GivenNames = new[] { "Yehoshua", "Yohanan", "Miriam", "Shimon" },
    };

    /// <summary>Real Persian-derived elements shared by Parthian and Armenian naming (§13).</summary>
    public static readonly NamePool ParthianArmenian = new()
    {
        Praenomina = Array.Empty<string>(),
        Nomina = Array.Empty<string>(),
        Cognomina = Array.Empty<string>(),
        GivenNames = new[] { "Tiridates", "Vologases", "Artavasdes", "Tigranes" },
    };

    /// <summary>Etruscan's own real, distinct, famously non-Indo-European naming structure (§13) —
    /// still visible in a handful of Roman family names that were themselves originally Etruscan.</summary>
    public static readonly NamePool Etruscan = new()
    {
        Praenomina = Array.Empty<string>(),
        Nomina = new[] { "Vibenna", "Porsenna", "Spurinna" },
        Cognomina = Array.Empty<string>(),
        GivenNames = new[] { "Larth", "Thefarie" },
    };

    /// <summary>Every culture §13 names a real convention for. Deliberately incomplete across the full
    /// thirty-six-culture roster — see this class's own doc comment.</summary>
    public static IReadOnlyDictionary<DefinitionId<Identity.Culture>, NamePool> BuildMap() =>
        new Dictionary<DefinitionId<Identity.Culture>, NamePool>
        {
            [KnownWorldCultures.Roman] = Roman,
            [KnownWorldCultures.Gallic] = GallicBritishHibernian,
            [KnownWorldCultures.British] = GallicBritishHibernian,
            [KnownWorldCultures.Hibernian] = GallicBritishHibernian,
            [KnownWorldCultures.Germanic] = Germanic,
            [KnownWorldCultures.Egyptian] = Egyptian,
            [KnownWorldCultures.Judaean] = Judaean,
            [KnownWorldCultures.Parthian] = ParthianArmenian,
            [KnownWorldCultures.Armenian] = ParthianArmenian,
            [KnownWorldCultures.Etruscan] = Etruscan,
        };
}
