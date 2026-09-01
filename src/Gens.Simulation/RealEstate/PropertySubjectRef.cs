using Gens.Simulation.Identity;
using Gens.Simulation.Land;

namespace Gens.Simulation.RealEstate;

/// <summary>Which of the two shapes a real property takes in this codebase (Phase 15 item 1): a
/// player-owned, Estate &amp; Settlement-tracked <see cref="Plot"/> (extended by <see
/// cref="PlotPropertyExtension"/>), or a genuinely new <see cref="PropertyRecord"/> (a Ship or Named
/// Holding, §3). <see cref="PropertySubjectRef"/> lets §5/§6/§9's commands and systems (acquisition,
/// leasing, audit, buyout, sale, Administrative Burden) operate over either shape through one shared
/// surface (<see cref="PropertyResolver"/>) rather than duplicating each command once per shape.</summary>
public enum PropertySubjectKind
{
    Plot,
    PropertyRecord,
}

/// <summary>A tagged reference to either a <see cref="Plot"/> or a <see cref="PropertyRecord"/> —
/// same "kind + tagged-ID-string" shape as <see cref="PropertyOwnerRef"/>, for the same reason: no
/// single <c>RuntimeId&lt;T&gt;</c> phantom type could name both.</summary>
public readonly record struct PropertySubjectRef(PropertySubjectKind Kind, string SubjectId)
{
    public static PropertySubjectRef ForPlot(RuntimeId<Plot> plotId) =>
        new(PropertySubjectKind.Plot, plotId.ToTaggedString());

    public static PropertySubjectRef ForPropertyRecord(RuntimeId<PropertyRecord> propertyRecordId) =>
        new(PropertySubjectKind.PropertyRecord, propertyRecordId.ToTaggedString());

    public RuntimeId<Plot> AsPlotId() => RuntimeId<Plot>.Parse(SubjectId);

    public RuntimeId<PropertyRecord> AsPropertyRecordId() => RuntimeId<PropertyRecord>.Parse(SubjectId);
}
