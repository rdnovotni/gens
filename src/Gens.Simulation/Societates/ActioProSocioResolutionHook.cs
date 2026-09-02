using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Legal;
using Gens.Simulation.RealEstate;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Societates;

/// <summary>
/// §6's real, mechanical consequence of an <see cref="ActioProSocioLink"/> actually being ruled —
/// called from <see cref="LegalCaseRuling.Apply"/> exactly when <see
/// cref="LegalCase.CaseType"/> is <see cref="LegalCaseType.PartnershipDispute"/>, matching that
/// method's own already-established "an additive, gated call for one specific case flavor" precedent
/// (<c>RecordWeaponizedLegalCaseScandal</c>'s identical shape). <see
/// cref="LegalCaseRuling.Apply"/>'s own ordinary consequences (Dignitas swing, relationship scar) are
/// untouched and already applied before this runs — this hook only adds the Societas-specific
/// mechanical follow-through §7 names for each dispute type:
///
/// <list type="bullet">
/// <item><description><see cref="PartnerDisputeType.SuspectedFraud"/>, ruled <see
/// cref="LegalCaseVerdict.Plaintiff"/> (fraud confirmed) — dissolves the <see cref="Societas"/> (<see
/// cref="SocietasDissolutionTrigger.Fraud"/>) and calls in §3's unlimited liability against the
/// confirmed-fraudulent respondent partner.</description></item>
/// <item><description><see cref="PartnerDisputeType.EarlyExitDispute"/>, ruled <see
/// cref="LegalCaseVerdict.Plaintiff"/> — the filing partner is granted the exit (<see
/// cref="WithdrawPartnerCommands"/>).</description></item>
/// <item><description><see cref="PartnerDisputeType.ProfitDistributionDisagreement"/> — no further
/// mechanical effect beyond the ordinary ruling consequences already applied: no per-partner cash-flow
/// or profit-remittance engine exists for a <see cref="Societas"/> itself (only a linked, already-
/// leased <see cref="RealEstate.PropertyRecord"/>/Plot has one, §6's own Operator remittance) for this
/// item to actually rebalance, a real, investigated scope cut rather than an invented redistribution
/// formula §5's own negotiated <c>lex societatis</c> never specified numerically.</description></item>
/// <item><description>Any other verdict (Dismissed, Defendant, SplitCompromise, Acquitted/Convicted —
/// the latter two never actually reachable here, since <see cref="LegalCaseType.PartnershipDispute"/>
/// is not one of <see cref="LegalCaseResolver"/>'s capital-shaped types) — no further mechanical
/// effect.</description></item>
/// </list>
/// </summary>
internal static class ActioProSocioResolutionHook
{
    public static IDomainEvent[] Apply(WorldState state, LegalCase legalCase, LegalCaseVerdict verdict, GameDate date, string? causationId)
    {
        if (!state.ActioProSocioLinks.TryGet(legalCase.CaseId, out var link))
            return Array.Empty<IDomainEvent>();
        if (!state.Societates.TryGet(link!.SocietasId, out var societas) || !societas!.IsActive)
            return Array.Empty<IDomainEvent>();
        if (verdict != LegalCaseVerdict.Plaintiff)
            return Array.Empty<IDomainEvent>();

        return link.DisputeType switch
        {
            PartnerDisputeType.SuspectedFraud => ApplyFraudConfirmed(state, legalCase, link, societas, date, causationId),
            PartnerDisputeType.EarlyExitDispute => ApplyEarlyExitGranted(state, legalCase, date, causationId),
            _ => Array.Empty<IDomainEvent>(),
        };
    }

    private static IDomainEvent[] ApplyFraudConfirmed(
        WorldState state, LegalCase legalCase, ActioProSocioLink link, Societas societas, GameDate date, string? causationId)
    {
        if (!TryFindPartnerHouseholdOwner(societas, legalCase.DefendantId, out var respondentOwner))
            return Array.Empty<IDomainEvent>();

        var events = new List<IDomainEvent>();
        events.AddRange(DissolveSocietasCommands.Pipeline.Execute(
            state, new DissolveSocietasCommand(
                state.CommandIds.Issue(), "system", date, causationId, link.SocietasId, SocietasDissolutionTrigger.Fraud)).Events);
        events.AddRange(TriggerUnlimitedLiabilityCommands.Pipeline.Execute(
            state, new TriggerUnlimitedLiabilityCommand(
                state.CommandIds.Issue(), "system", date, causationId, link.SocietasId, respondentOwner,
                TriggeringPartnerFailure: true)).Events);
        return events.ToArray();
    }

    private static IDomainEvent[] ApplyEarlyExitGranted(WorldState state, LegalCase legalCase, GameDate date, string? causationId)
    {
        if (!state.ActioProSocioLinks.TryGet(legalCase.CaseId, out var link) ||
            !state.Societates.TryGet(link!.SocietasId, out var societas) ||
            !TryFindPartnerHouseholdOwner(societas!, legalCase.PlaintiffId, out var plaintiffOwner))
            return Array.Empty<IDomainEvent>();

        return WithdrawPartnerCommands.Pipeline.Execute(
            state, new WithdrawPartnerCommand(
                state.CommandIds.Issue(), "system", date, causationId, link.SocietasId, plaintiffOwner)).Events.ToArray();
    }

    private static bool TryFindPartnerHouseholdOwner(Societas societas, RuntimeId<Household> householdId, out PropertyOwnerRef owner)
    {
        foreach (var partner in societas.Partners)
        {
            if (partner.Owner.Kind != PropertyOwnerKind.PlayerHousehold)
                continue;
            if (RuntimeId<Household>.Parse(partner.Owner.OwnerId!) != householdId)
                continue;
            owner = partner.Owner;
            return true;
        }

        owner = default;
        return false;
    }
}
