using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Clientela;

/// <summary>
/// The monthly Salutatio (Phase 12 item 2; §4.3): "clients calling on their patron for greetings,
/// favors, and small business... a well-attended Salutatio... generates a small Dignitas and Influence
/// trickle just from being seen to hold court; a neglected one... costs Dignitas instead." Writes the
/// Dignitas half through <see cref="AdjustDignitasCommand"/> — "the one command path every future
/// Dignitas-moving system routes through," per that command's own doc comment naming this exact system
/// as the deferred caller — and the Influence half directly via <see cref="InfluenceResolver.Apply"/>,
/// which (unlike Dignitas) has no existing shared command to route through yet.
///
/// A patron holding zero clients has nothing to greet and pays nothing either way — the neglected cost
/// is for holding court badly (a small, low-opinion roster), not for not holding it at all. Runs in
/// <see cref="TickPhase.RelationshipsActors"/>, alongside <see cref="InfluenceCycleSystem"/>.
/// </summary>
public sealed class SalutatioSystem : IMonthlySystem<WorldState>
{
    public string Id => "clientela.salutatio";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "clientelaEntries", "relationships", "householdHeadships" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "householdReputations", "householdInfluences", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var patronHouseholdId in ClientelaResolver.PatronsWithClients(state))
        {
            var clients = ClientelaResolver.ClientsOf(state, patronHouseholdId);
            if (clients.Count == 0 || !state.HouseholdHeadships.TryGet(patronHouseholdId, out var headship))
                continue;

            var averageOpinion = 0.0;
            var total = 0;
            foreach (var client in clients)
                total += state.Relationships.TryGet(new RelationshipKey(client.ClientId, headship!.HeadCharacterId), out var relationship)
                    ? relationship.Opinion
                    : 0;
            averageOpinion = (double)total / clients.Count;

            var wellAttended = clients.Count >= ClientelaCatalog.SalutatioWellAttendedMinClients &&
                averageOpinion >= ClientelaCatalog.SalutatioWellAttendedMinAvgOpinion;

            var dignitasDelta = wellAttended ? ClientelaCatalog.SalutatioWellAttendedDignitasGain : -ClientelaCatalog.SalutatioNeglectedDignitasCost;
            var reason = wellAttended ? "a well-attended Salutatio" : "a neglected Salutatio";

            var command = new AdjustDignitasCommand(
                state.CommandIds.Issue(), "system", context.Date, CausationId: null, patronHouseholdId, dignitasDelta, reason);
            var result = AdjustDignitasCommands.Pipeline.Execute(state, command);
            if (result.Accepted)
                events.AddRange(result.Events);

            if (wellAttended)
                InfluenceResolver.Apply(state, patronHouseholdId, ClientelaCatalog.SalutatioWellAttendedInfluenceGain);
        }

        return events;
    }
}
