using Gens.Simulation.Time;

namespace Gens.Simulation.Land;

/// <summary>The source and provenance retained when a plot changes from unowned to owned.</summary>
public readonly record struct LandAcquisition(AcquisitionMethod Method, GameDate AcquiredDate, string? SourceId);

public enum AcquisitionMethod
{
    Purchase,
    LandGrant,
    ConquestOrSeizure,
    DowryOrInheritance,
    VeteranGrant,
    ForeclosureOrLegalSeizure,
}
