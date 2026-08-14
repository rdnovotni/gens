using Gens.Simulation.Identity;

namespace Gens.Simulation.Goods;

/// <summary>The storage-relevant part of a content-authored good definition.</summary>
public sealed record GoodDefinition
{
    public GoodDefinition(
        DefinitionId<Good> id,
        Perishability perishability,
        bool qualityEligible = false,
        bool conditionTracked = false,
        int? shelfLifeTicks = null)
    {
        if (perishability == Perishability.NonPerishable && shelfLifeTicks is not null)
            throw new ArgumentException("A non-perishable good cannot have a shelf life.", nameof(shelfLifeTicks));
        if (perishability != Perishability.NonPerishable && shelfLifeTicks is <= 0)
            throw new ArgumentOutOfRangeException(nameof(shelfLifeTicks), shelfLifeTicks, "A perishable good needs a positive shelf life.");

        Id = id;
        Perishability = perishability;
        QualityEligible = qualityEligible;
        ConditionTracked = conditionTracked;
        ShelfLifeTicks = shelfLifeTicks;
    }

    public DefinitionId<Good> Id { get; }
    public Perishability Perishability { get; }
    public bool QualityEligible { get; }
    public bool ConditionTracked { get; }
    public int? ShelfLifeTicks { get; }
}
