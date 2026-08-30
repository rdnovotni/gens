using Gens.Simulation.Commands;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Legal;

/// <summary>Emitted when <see cref="LegalCaseAdvancementSystem"/> moves a Major case from <see
/// cref="LegalCaseStage.EvidenceGathering"/> into <see cref="LegalCaseStage.Hearing"/> — §5.3's "a real,
/// singular event, not a silent tick." This system holds a case at <see cref="LegalCaseStage.Hearing"/>
/// for exactly one monthly tick before rolling its verdict on the next, giving the Hearing itself real
/// presence between the evidence window closing and the Ruling landing, rather than collapsing both into
/// the same tick.</summary>
public sealed record LegalCaseHearingHeldEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<LegalCase> CaseId,
    string? CausationId) : IDomainEvent
{
    public string Type => "legal.hearingHeld";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => Array.Empty<string>();
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// The monthly tick that carries a Major <see cref="LegalCase"/> through §5's remaining stages once <see
/// cref="FileLawsuitCommand"/> has opened it: <see cref="LegalCaseStage.EvidenceGathering"/> runs for
/// <see cref="LegalCatalog.MajorCaseEvidenceGatheringMonths"/> months, then <see
/// cref="LegalCaseStage.Hearing"/> holds for one month (see <see cref="LegalCaseHearingHeldEvent"/>'s own
/// doc comment), then the next tick rolls the verdict through the same <see
/// cref="LegalCaseResolver.RollVerdict"/>/<see cref="LegalCaseRuling.Apply"/> pair <see
/// cref="FileLawsuitCommand"/>'s own Quick Resolution already uses — one shared resolution path for both
/// depths (rule 2), not two. Runs in <see cref="TickPhase.RelationshipsActors"/>, the same phase every
/// other Politics &amp; Patronage/Religion monthly system in this codebase runs in.
/// </summary>
public sealed class LegalCaseAdvancementSystem : IMonthlySystem<WorldState>
{
    /// <summary>The named random stream (rule 8) reserved for a Major case's own Hearing verdict roll —
    /// kept distinct from <see cref="FileLawsuitCommands.QuickResolutionStreamName"/> for the same rule-8
    /// reason every other pair of same-shaped-but-different-caller rolls in this codebase is kept
    /// distinct. Registered in <see cref="Campaign.CampaignBootstrapper"/>.</summary>
    public const string VerdictOutcomeStreamName = "legal.majorCaseVerdictOutcome";

    public string Id => "legal.caseAdvancement";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "legalCases", "characters", "householdReputations", "magistracyRecords" };
    public IReadOnlyCollection<string> Writes { get; } = new[]
    {
        "legalCases", "eventIds", "householdReputations", "relationships", "magistracyRecords",
        "characters", "ledgerAccounts", "ledgerTransactions",
    };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var entry in state.LegalCases.InAscendingOrder().Where(e => e.Value.Depth == LegalCaseDepth.Major).ToArray())
        {
            var legalCase = entry.Value;

            switch (legalCase.Stage)
            {
                case LegalCaseStage.Filed:
                case LegalCaseStage.EvidenceGathering:
                    var monthsFiled = context.Date.TotalMonths - legalCase.FiledDate.TotalMonths;
                    if (monthsFiled >= LegalCatalog.MajorCaseEvidenceGatheringMonths)
                    {
                        state.LegalCases.Remove(legalCase.CaseId);
                        state.LegalCases.Add(legalCase.CaseId, legalCase with { Stage = LegalCaseStage.Hearing });
                        events.Add(new LegalCaseHearingHeldEvent(state.EventIds.Issue(), context.Date, legalCase.CaseId, CausationId: null));
                    }
                    break;

                case LegalCaseStage.Hearing:
                    var (verdict, sentence) = LegalCaseResolver.RollVerdict(state, legalCase, context.RandomStreams, VerdictOutcomeStreamName);
                    events.AddRange(LegalCaseRuling.Apply(state, legalCase, verdict, sentence, context.Date, causationId: null));
                    break;
            }
        }

        return events;
    }
}
