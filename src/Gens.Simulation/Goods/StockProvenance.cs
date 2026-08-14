namespace Gens.Simulation.Goods;

/// <summary>
/// Optional origin metadata carried by a batch. Ordinary fungible goods may omit it; keeping it on
/// the lot boundary lets a future Exceptional batch be promoted to individually tracked objects
/// without inventing its history after the fact.
/// </summary>
public readonly record struct StockProvenance
{
    public StockProvenance(string sourceId, string? eventId = null, string? exceptionalObjectId = null)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
            throw new ArgumentException("A provenance source cannot be empty.", nameof(sourceId));
        if (eventId is not null && string.IsNullOrWhiteSpace(eventId))
            throw new ArgumentException("A provenance event ID cannot be empty.", nameof(eventId));
        if (exceptionalObjectId is not null && string.IsNullOrWhiteSpace(exceptionalObjectId))
            throw new ArgumentException("An exceptional object ID cannot be empty.", nameof(exceptionalObjectId));

        SourceId = sourceId;
        EventId = eventId;
        ExceptionalObjectId = exceptionalObjectId;
    }

    public string SourceId { get; }
    public string? EventId { get; }
    public string? ExceptionalObjectId { get; }
}
