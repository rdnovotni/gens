using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.History;

/// <summary>
/// The real, authored Named Historical Figures roster (Phase 13 item 5; <c>gens-events-historical-
/// timeline-content.md</c> §6), mirroring <see cref="Cultures.KnownWorldCultures"/>/<see
/// cref="Languages.KnownWorldLanguages"/>'s own "real content, not a fixture" precedent from item 4.
/// Every figure §6 names is registered: the ten Republic-era names, all twenty-four Emperors in
/// succession order, the eight other notable figures, and Jesus of Nazareth per Religions' own
/// careful-treatment note (§6's closing paragraph).
///
/// <see cref="NamedHistoricalFigureDefinition.RealAccessionOrStartYear"/>/<see
/// cref="NamedHistoricalFigureDefinition.RealDeathOrEndYear"/> are populated wherever §6's own event
/// table (§2-§5) or the Emperor succession table directly gives a real year for that figure. A handful
/// of well-documented dates the content doc's own tables never state outright (e.g. Marius's, Sulla's,
/// Pompey's, and Crassus's real death years; Lucius Verus's AD 169 death, which falls after his own
/// AD 161-166 Parthian War entry) are filled in as standard, uncontroversial Roman history rather than
/// left null — still "real, documented biographical facts" per §6.5, just sourced slightly beyond this
/// pass's own content doc. Everywhere a figure's own real tenure has no second clean year worth
/// recording (most of the non-Emperor Republic-era and "other notable" figures only have one real dated
/// moment at all), the unset field is left null rather than guessed — an honest gap, not a silent
/// fabrication.
/// </summary>
public static class KnownWorldHistoricalFigures
{
    // Republic-era.
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> TiberiusGracchus = new("tiberius-gracchus");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> GaiusGracchus = new("gaius-gracchus");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Marius = new("marius");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> MithridatesVI = new("mithridates-vi");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Sulla = new("sulla");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> JuliusCaesar = new("julius-caesar");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Vercingetorix = new("vercingetorix");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Pompey = new("pompey");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Crassus = new("crassus");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Cleopatra = new("cleopatra");

    // Emperors, in succession order.
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Augustus = new("augustus");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Tiberius = new("tiberius-emperor");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Caligula = new("caligula");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Claudius = new("claudius");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Nero = new("nero");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Galba = new("galba");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Otho = new("otho");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Vitellius = new("vitellius");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Vespasian = new("vespasian");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Titus = new("titus");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Domitian = new("domitian");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Nerva = new("nerva");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Trajan = new("trajan");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Hadrian = new("hadrian");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> AntoninusPius = new("antoninus-pius");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> MarcusAurelius = new("marcus-aurelius");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> LuciusVerus = new("lucius-verus");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Commodus = new("commodus");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> SeptimiusSeverus = new("septimius-severus");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Caracalla = new("caracalla");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Geta = new("geta");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Macrinus = new("macrinus");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Elagabalus = new("elagabalus");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> SeverusAlexander = new("severus-alexander");

    // Other real, notable figures.
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Arminius = new("arminius");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Boudicca = new("boudicca");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Decebalus = new("decebalus");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> AvidiusCassius = new("avidius-cassius");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> ClodiusAlbinus = new("clodius-albinus");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> PlinyTheElder = new("pliny-the-elder");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Josephus = new("josephus");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Galen = new("galen");

    // Religions' own careful-treatment case.
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> JesusOfNazareth = new("jesus-of-nazareth");

    public static NamedHistoricalFigureCatalog BuildCatalog() => new(new[]
    {
        Figure(TiberiusGracchus, "Tiberius Gracchus", HistoricalFigureRole.Senator, end: (133, true)),
        Figure(GaiusGracchus, "Gaius Gracchus", HistoricalFigureRole.Senator, end: (121, true)),
        Figure(Marius, "Gaius Marius", HistoricalFigureRole.General, end: (86, true)),
        Figure(MithridatesVI, "Mithridates VI of Pontus", HistoricalFigureRole.HeadOfState, end: (63, true)),
        Figure(Sulla, "Lucius Cornelius Sulla", HistoricalFigureRole.HeadOfState, end: (78, true)),
        Figure(JuliusCaesar, "Gaius Julius Caesar", HistoricalFigureRole.HeadOfState, start: (49, true), end: (44, true)),
        Figure(Vercingetorix, "Vercingetorix", HistoricalFigureRole.General, end: (46, true)),
        Figure(Pompey, "Gnaeus Pompeius Magnus", HistoricalFigureRole.General, end: (48, true)),
        Figure(Crassus, "Marcus Licinius Crassus", HistoricalFigureRole.General, end: (53, true)),
        Figure(Cleopatra, "Cleopatra VII", HistoricalFigureRole.HeadOfState, start: (51, true), end: (30, true)),

        Figure(Augustus, "Augustus", HistoricalFigureRole.HeadOfState, start: (27, true), end: (14, false)),
        Figure(Tiberius, "Tiberius", HistoricalFigureRole.HeadOfState, start: (14, false), end: (37, false)),
        Figure(Caligula, "Caligula", HistoricalFigureRole.HeadOfState, start: (37, false), end: (41, false)),
        Figure(Claudius, "Claudius", HistoricalFigureRole.HeadOfState, start: (41, false), end: (54, false)),
        Figure(Nero, "Nero", HistoricalFigureRole.HeadOfState, start: (54, false), end: (68, false)),
        Figure(Galba, "Galba", HistoricalFigureRole.HeadOfState, start: (68, false), end: (69, false)),
        Figure(Otho, "Otho", HistoricalFigureRole.HeadOfState, start: (69, false), end: (69, false)),
        Figure(Vitellius, "Vitellius", HistoricalFigureRole.HeadOfState, start: (69, false), end: (69, false)),
        Figure(Vespasian, "Vespasian", HistoricalFigureRole.HeadOfState, start: (69, false), end: (79, false)),
        Figure(Titus, "Titus", HistoricalFigureRole.HeadOfState, start: (79, false), end: (81, false)),
        Figure(Domitian, "Domitian", HistoricalFigureRole.HeadOfState, start: (81, false), end: (96, false)),
        Figure(Nerva, "Nerva", HistoricalFigureRole.HeadOfState, start: (96, false), end: (98, false)),
        Figure(Trajan, "Trajan", HistoricalFigureRole.HeadOfState, start: (98, false), end: (117, false)),
        Figure(Hadrian, "Hadrian", HistoricalFigureRole.HeadOfState, start: (117, false), end: (138, false)),
        Figure(AntoninusPius, "Antoninus Pius", HistoricalFigureRole.HeadOfState, start: (138, false), end: (161, false)),
        Figure(MarcusAurelius, "Marcus Aurelius", HistoricalFigureRole.HeadOfState, start: (161, false), end: (180, false)),
        Figure(LuciusVerus, "Lucius Verus", HistoricalFigureRole.HeadOfState, start: (161, false), end: (169, false)),
        Figure(Commodus, "Commodus", HistoricalFigureRole.HeadOfState, start: (180, false), end: (192, false)),
        Figure(SeptimiusSeverus, "Septimius Severus", HistoricalFigureRole.HeadOfState, start: (193, false), end: (211, false)),
        Figure(Caracalla, "Caracalla", HistoricalFigureRole.HeadOfState, start: (211, false), end: (217, false)),
        Figure(Geta, "Geta", HistoricalFigureRole.HeadOfState, start: (211, false), end: (212, false)),
        Figure(Macrinus, "Macrinus", HistoricalFigureRole.HeadOfState, start: (217, false), end: (218, false)),
        Figure(Elagabalus, "Elagabalus", HistoricalFigureRole.HeadOfState, start: (218, false), end: (222, false)),
        Figure(SeverusAlexander, "Severus Alexander", HistoricalFigureRole.HeadOfState, start: (222, false), end: (235, false)),

        Figure(Arminius, "Arminius", HistoricalFigureRole.General, end: (21, false)),
        Figure(Boudicca, "Boudicca", HistoricalFigureRole.HeadOfState, end: (61, false)),
        Figure(Decebalus, "Decebalus", HistoricalFigureRole.HeadOfState, end: (106, false)),
        Figure(AvidiusCassius, "Avidius Cassius", HistoricalFigureRole.General, end: (175, false)),
        Figure(ClodiusAlbinus, "Clodius Albinus", HistoricalFigureRole.General, end: (197, false)),
        Figure(PlinyTheElder, "Pliny the Elder", HistoricalFigureRole.Other, end: (79, false)),
        Figure(Josephus, "Flavius Josephus", HistoricalFigureRole.Other),
        Figure(Galen, "Galen", HistoricalFigureRole.Other),

        Figure(JesusOfNazareth, "Jesus of Nazareth", HistoricalFigureRole.Other),
    });

    private static NamedHistoricalFigureDefinition Figure(
        DefinitionId<NamedHistoricalFigureDefinition> id, string realName, HistoricalFigureRole role,
        (int Year, bool Bce)? start = null, (int Year, bool Bce)? end = null) =>
        new(
            id, realName, role,
            start is { } s ? HistoricalYear.ToGameDate(s.Year, s.Bce) : null,
            end is { } e ? HistoricalYear.ToGameDate(e.Year, e.Bce) : null);
}
