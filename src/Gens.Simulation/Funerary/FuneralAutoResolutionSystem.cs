using Gens.Simulation.Commands;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Funerary;

/// <summary>
/// Resolves any <see cref="FuneralStatus.Pending"/> <see cref="FuneralRecord"/> that has sat unchosen
/// for <see cref="FuneraryCatalog.FuneralAutoResolutionAfterMonths"/> months at <see
/// cref="FuneraryCatalog.AutoResolutionDefaultTier"/> (Phase 11 item 4) — the same "resolve
/// automatically after a window" shape <see cref="Succession.SuccessionDisputeResolutionSystem"/> uses
/// for a stale <see cref="Succession.SuccessionDispute"/>, applied here so a background/NPC household
/// (or a player who never gets around to it) still gets a real funeral rather than an indefinitely
/// Pending one blocking that household's Memoria from ever moving. Declares <see
/// cref="FuneralOpeningSystem"/> as a same-phase prerequisite purely so a funeral opened this same
/// month is never immediately eligible for the same-tick auto-resolution window (it never would be —
/// <see cref="FuneraryCatalog.FuneralAutoResolutionAfterMonths"/> is always at least one full month —
/// but the explicit ordering documents the intent rather than relying on that arithmetic alone).
/// </summary>
public sealed class FuneralAutoResolutionSystem : IMonthlySystem<WorldState>
{
    public string Id => "funerary.funeralAutoResolution";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "funeralRecords", "ledgerAccounts" };

    public IReadOnlyCollection<string> Writes { get; } =
        new[] { "funeralRecords", "ledgerAccounts", "ledgerTransactions", "ledgerTransactionIds", "memoriaStates", "eventIds" };

    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "funerary.funeralOpening" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var entry in state.FuneralRecords.InAscendingOrder().ToArray())
        {
            var funeral = entry.Value;
            if (funeral.Status != FuneralStatus.Pending)
                continue;

            var monthsPending = context.Date.TotalMonths - funeral.DeathDate.TotalMonths;
            if (monthsPending < FuneraryCatalog.FuneralAutoResolutionAfterMonths)
                continue;

            events.AddRange(FuneralResolution.Hold(
                state, funeral, FuneraryCatalog.AutoResolutionDefaultTier, context.Date, autoResolved: true, causationId: null));
        }

        return events;
    }
}
