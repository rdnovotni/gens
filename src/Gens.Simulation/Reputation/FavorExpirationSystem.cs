using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Reputation;

/// <summary>Emitted when <see cref="FavorExpirationSystem"/> lapses an <see
/// cref="FavorStatus.Outstanding"/> <see cref="FavorObligation"/> that sat uncollected past <see
/// cref="ReputationCatalog.FavorExpirationAfterMonths"/>. Same private, two-party <see
/// cref="Visibility"/> as the favor's own lifecycle events.</summary>
public sealed record FavorExpiredEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<FavorObligation> FavorId,
    RuntimeId<Characters.Character> GrantorId,
    RuntimeId<Characters.Character> BeneficiaryId,
    string? CausationId) : IDomainEvent
{
    public string Type => "reputation.favorExpired";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { GrantorId.ToTaggedString(), BeneficiaryId.ToTaggedString() };
    public Visibility Visibility => Visibility.Private(GrantorId.ToTaggedString(), BeneficiaryId.ToTaggedString());
}

/// <summary>
/// The monthly system that lapses a favor nobody ever called in (Phase 12 item 1) — time itself as the
/// third way a <see cref="FavorObligation"/> resolves, alongside the two explicit commands (<see
/// cref="GrantFavorCommand"/>/<see cref="SettleFavorCommand"/>). Runs in <see
/// cref="TickPhase.RelationshipsActors"/>, the same phase every other actor-standing/relationship
/// system in this codebase runs in (e.g. <see cref="Actors.AncestralGrudgeDecaySystem"/>, <see
/// cref="Funerary.ManesObservanceSystem"/>).
/// </summary>
public sealed class FavorExpirationSystem : IMonthlySystem<WorldState>
{
    public string Id => "reputation.favorExpiration";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "favorObligations" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "favorObligations", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();
        var expired = new List<FavorObligation>();

        foreach (var entry in state.FavorObligations.InAscendingOrder())
        {
            var favor = entry.Value;
            if (favor.Status != FavorStatus.Outstanding)
                continue;
            var ageInMonths = context.Date.TotalMonths - favor.GrantedDate.TotalMonths;
            if (ageInMonths >= ReputationCatalog.FavorExpirationAfterMonths)
                expired.Add(favor);
        }

        foreach (var favor in expired)
        {
            state.FavorObligations.Remove(favor.FavorId);
            state.FavorObligations.Add(favor.FavorId, favor with { Status = FavorStatus.Expired, ResolvedDate = context.Date });

            events.Add(new FavorExpiredEvent(
                state.EventIds.Issue(), context.Date, favor.FavorId, favor.GrantorId, favor.BeneficiaryId, CausationId: null));
        }

        return events;
    }
}
