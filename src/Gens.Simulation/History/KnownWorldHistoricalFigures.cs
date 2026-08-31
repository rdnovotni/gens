using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.History;

/// <summary>
/// The real, authored Named Historical Figures roster (Phase 13 item 5; <c>gens-events-historical-
/// timeline-content.md</c> §6), mirroring <see cref="Cultures.KnownWorldCultures"/>/<see
/// cref="Languages.KnownWorldLanguages"/>'s own "real content, not a fixture" precedent from item 4.
/// Every figure §6 names is registered, together with a curated wider-known-world roster drawn from
/// the historical-figure research reference. The wider roster deliberately represents travel,
/// scholarship, literature, medicine, law, religion, engineering, patronage, and revolt as well as
/// rulers and generals, so timeline flavor can reach the full range of Gens systems.
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

    // Late-antiquity timeline roster.
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Decius = new("decius");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Valerian = new("valerian");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> ShapurI = new("shapur-i");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Postumus = new("postumus");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Zenobia = new("zenobia");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Aurelian = new("aurelian");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Diocletian = new("diocletian");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Galerius = new("galerius");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> ConstantineI = new("constantine-i");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Julian = new("julian");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> TheodosiusI = new("theodosius-i");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> AlaricI = new("alaric-i");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Attila = new("attila");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Odoacer = new("odoacer");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> RomulusAugustulus = new("romulus-augustulus");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> TheodoricTheGreat = new("theodoric-the-great");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> ClovisI = new("clovis-i");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> JustinianI = new("justinian-i");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Belisarius = new("belisarius");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Narses = new("narses");

    // Wider known-world figures: social conflict, letters, scholarship, faith, travel, and craft.
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Spartacus = new("spartacus");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Cicero = new("cicero");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Fulvia = new("fulvia");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Virgil = new("virgil");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Strabo = new("strabo");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Livy = new("livy");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> PhiloOfAlexandria = new("philo-of-alexandria");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> SenecaTheYounger = new("seneca-the-younger");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> PaulTheApostle = new("paul-the-apostle");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Columella = new("columella");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Quintilian = new("quintilian");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> PedaniusDioscorides = new("pedanius-dioscorides");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Plutarch = new("plutarch");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Tacitus = new("tacitus");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> PlinyTheYounger = new("pliny-the-younger");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Arrian = new("arrian");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Ptolemy = new("ptolemy");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> HerodesAtticus = new("herodes-atticus");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> JuliaBalbilla = new("julia-balbilla");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> JudahHaNasi = new("judah-ha-nasi");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Papinian = new("papinian");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> CassiusDio = new("cassius-dio");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Ulpian = new("ulpian");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Athenaeus = new("athenaeus");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Perpetua = new("perpetua");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Felicity = new("felicity");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Mani = new("mani");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> AnthonyTheGreat = new("anthony-the-great");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> EusebiusOfCaesarea = new("eusebius-of-caesarea");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> EphremTheSyrian = new("ephrem-the-syrian");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Hypatia = new("hypatia");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> MesropMashtots = new("mesrop-mashtots");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Egeria = new("egeria");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> SidoniusApollinaris = new("sidonius-apollinaris");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Proclus = new("proclus");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> AeliaEudocia = new("aelia-eudocia");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Boethius = new("boethius");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Damascius = new("damascius");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Cassiodorus = new("cassiodorus");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> AnthemiusOfTralles = new("anthemius-of-tralles");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> IsidoreOfMiletus = new("isidore-of-miletus");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Procopius = new("procopius");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> Tribonian = new("tribonian");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> JohnPhiloponus = new("john-philoponus");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> SimpliciusOfCilicia = new("simplicius-of-cilicia");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> JohnLydus = new("john-lydus");
    public static readonly DefinitionId<NamedHistoricalFigureDefinition> CosmasIndicopleustes = new("cosmas-indicopleustes");

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

        Figure(Decius, "Decius", HistoricalFigureRole.HeadOfState, start: (249, false), end: (251, false)),
        Figure(Valerian, "Valerian", HistoricalFigureRole.HeadOfState, start: (253, false), end: (260, false)),
        Figure(ShapurI, "Shapur I", HistoricalFigureRole.HeadOfState, start: (240, false), end: (270, false)),
        Figure(Postumus, "Postumus", HistoricalFigureRole.HeadOfState, start: (260, false), end: (269, false)),
        Figure(Zenobia, "Zenobia", HistoricalFigureRole.HeadOfState, start: (267, false), end: (272, false)),
        Figure(Aurelian, "Aurelian", HistoricalFigureRole.HeadOfState, start: (270, false), end: (275, false)),
        Figure(Diocletian, "Diocletian", HistoricalFigureRole.HeadOfState, start: (284, false), end: (305, false)),
        Figure(Galerius, "Galerius", HistoricalFigureRole.HeadOfState, start: (293, false), end: (311, false)),
        Figure(ConstantineI, "Constantine I", HistoricalFigureRole.HeadOfState, start: (306, false), end: (337, false)),
        Figure(Julian, "Julian", HistoricalFigureRole.HeadOfState, start: (361, false), end: (363, false)),
        Figure(TheodosiusI, "Theodosius I", HistoricalFigureRole.HeadOfState, start: (379, false), end: (395, false)),
        Figure(AlaricI, "Alaric I", HistoricalFigureRole.HeadOfState, start: (395, false), end: (410, false)),
        Figure(Attila, "Attila", HistoricalFigureRole.HeadOfState, start: (434, false), end: (453, false)),
        Figure(Odoacer, "Odoacer", HistoricalFigureRole.HeadOfState, start: (476, false), end: (493, false)),
        Figure(RomulusAugustulus, "Romulus Augustulus", HistoricalFigureRole.HeadOfState,
            start: (475, false), end: (476, false)),
        Figure(TheodoricTheGreat, "Theodoric the Great", HistoricalFigureRole.HeadOfState,
            start: (493, false), end: (526, false)),
        Figure(ClovisI, "Clovis I", HistoricalFigureRole.HeadOfState, start: (481, false), end: (511, false)),
        Figure(JustinianI, "Justinian I", HistoricalFigureRole.HeadOfState, start: (527, false), end: (565, false)),
        Figure(Belisarius, "Belisarius", HistoricalFigureRole.General, end: (565, false)),
        Figure(Narses, "Narses", HistoricalFigureRole.General, end: (573, false)),

        Figure(Spartacus, "Spartacus", HistoricalFigureRole.RebelLeader, end: (71, true)),
        Figure(Cicero, "Marcus Tullius Cicero", HistoricalFigureRole.Orator, end: (43, true)),
        Figure(Fulvia, "Fulvia", HistoricalFigureRole.Other, end: (40, true)),
        Figure(Virgil, "Virgil", HistoricalFigureRole.WriterOrHistorian, end: (19, true)),
        Figure(Strabo, "Strabo", HistoricalFigureRole.PhilosopherOrScholar, end: (24, false)),
        Figure(Livy, "Livy", HistoricalFigureRole.WriterOrHistorian, end: (17, false)),
        Figure(PhiloOfAlexandria, "Philo of Alexandria", HistoricalFigureRole.PhilosopherOrScholar, end: (50, false)),
        Figure(SenecaTheYounger, "Seneca the Younger", HistoricalFigureRole.PhilosopherOrScholar, end: (65, false)),
        Figure(PaulTheApostle, "Paul the Apostle", HistoricalFigureRole.ReligiousFigure, end: (65, false)),
        Figure(Columella, "Columella", HistoricalFigureRole.WriterOrHistorian, end: (70, false)),
        Figure(Quintilian, "Quintilian", HistoricalFigureRole.Orator, end: (100, false)),
        Figure(PedaniusDioscorides, "Pedanius Dioscorides", HistoricalFigureRole.PhysicianOrNaturalist, end: (90, false)),
        Figure(Plutarch, "Plutarch", HistoricalFigureRole.WriterOrHistorian, end: (120, false)),
        Figure(Tacitus, "Tacitus", HistoricalFigureRole.WriterOrHistorian, end: (120, false)),
        Figure(PlinyTheYounger, "Pliny the Younger", HistoricalFigureRole.WriterOrHistorian, end: (113, false)),
        Figure(Arrian, "Arrian", HistoricalFigureRole.WriterOrHistorian, end: (160, false)),
        Figure(Ptolemy, "Claudius Ptolemy", HistoricalFigureRole.PhilosopherOrScholar, end: (170, false)),
        Figure(HerodesAtticus, "Herodes Atticus", HistoricalFigureRole.Patron, end: (177, false)),
        Figure(JuliaBalbilla, "Julia Balbilla", HistoricalFigureRole.WriterOrHistorian, end: (130, false)),
        Figure(JudahHaNasi, "Judah ha-Nasi", HistoricalFigureRole.ReligiousFigure, end: (217, false)),
        Figure(Papinian, "Papinian", HistoricalFigureRole.Jurist, end: (212, false)),
        Figure(CassiusDio, "Cassius Dio", HistoricalFigureRole.WriterOrHistorian, end: (235, false)),
        Figure(Ulpian, "Ulpian", HistoricalFigureRole.Jurist),
        Figure(Athenaeus, "Athenaeus", HistoricalFigureRole.WriterOrHistorian, end: (230, false)),
        Figure(Perpetua, "Vibia Perpetua", HistoricalFigureRole.ReligiousFigure, end: (203, false)),
        Figure(Felicity, "Felicity of Carthage", HistoricalFigureRole.ReligiousFigure, end: (203, false)),
        Figure(Mani, "Mani", HistoricalFigureRole.ReligiousFigure, end: (276, false)),
        Figure(AnthonyTheGreat, "Anthony the Great", HistoricalFigureRole.ReligiousFigure, end: (356, false)),
        Figure(EusebiusOfCaesarea, "Eusebius of Caesarea", HistoricalFigureRole.WriterOrHistorian, end: (339, false)),
        Figure(EphremTheSyrian, "Ephrem the Syrian", HistoricalFigureRole.ReligiousFigure, end: (373, false)),
        Figure(Hypatia, "Hypatia", HistoricalFigureRole.PhilosopherOrScholar, end: (415, false)),
        Figure(MesropMashtots, "Mesrop Mashtots", HistoricalFigureRole.PhilosopherOrScholar, end: (440, false)),
        Figure(Egeria, "Egeria", HistoricalFigureRole.ExplorerOrWanderer, start: (381, false), end: (384, false)),
        Figure(SidoniusApollinaris, "Sidonius Apollinaris", HistoricalFigureRole.WriterOrHistorian, end: (489, false)),
        Figure(Proclus, "Proclus", HistoricalFigureRole.PhilosopherOrScholar, end: (485, false)),
        Figure(AeliaEudocia, "Aelia Eudocia", HistoricalFigureRole.Patron, end: (460, false)),
        Figure(Boethius, "Boethius", HistoricalFigureRole.PhilosopherOrScholar, end: (524, false)),
        Figure(Damascius, "Damascius", HistoricalFigureRole.PhilosopherOrScholar, end: (538, false)),
        Figure(Cassiodorus, "Cassiodorus", HistoricalFigureRole.WriterOrHistorian, end: (585, false)),
        Figure(AnthemiusOfTralles, "Anthemius of Tralles", HistoricalFigureRole.ArchitectOrEngineer, end: (534, false)),
        Figure(IsidoreOfMiletus, "Isidore of Miletus", HistoricalFigureRole.ArchitectOrEngineer, end: (537, false)),
        Figure(Procopius, "Procopius of Caesarea", HistoricalFigureRole.WriterOrHistorian, end: (565, false)),
        Figure(Tribonian, "Tribonian", HistoricalFigureRole.Jurist, end: (547, false)),
        Figure(JohnPhiloponus, "John Philoponus", HistoricalFigureRole.PhilosopherOrScholar, end: (570, false)),
        Figure(SimpliciusOfCilicia, "Simplicius of Cilicia", HistoricalFigureRole.PhilosopherOrScholar, end: (560, false)),
        Figure(JohnLydus, "John Lydus", HistoricalFigureRole.WriterOrHistorian, end: (565, false)),
        Figure(CosmasIndicopleustes, "Cosmas Indicopleustes", HistoricalFigureRole.ExplorerOrWanderer,
            start: (530, false), end: (550, false)),
    });

    private static NamedHistoricalFigureDefinition Figure(
        DefinitionId<NamedHistoricalFigureDefinition> id, string realName, HistoricalFigureRole role,
        (int Year, bool Bce)? start = null, (int Year, bool Bce)? end = null) =>
        new(
            id, realName, role,
            start is { } s ? HistoricalYear.ToGameDate(s.Year, s.Bce) : null,
            end is { } e ? HistoricalYear.ToGameDate(e.Year, e.Bce) : null);
}
