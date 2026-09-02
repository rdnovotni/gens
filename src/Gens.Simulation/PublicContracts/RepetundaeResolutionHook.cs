using Gens.Simulation.Commands;
using Gens.Simulation.Ledger;
using Gens.Simulation.Legal;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.PublicContracts;

/// <summary>
/// §6.2's real, mechanical consequence of a <see cref="ContractFraudLegalLink"/> actually being ruled —
/// called from <see cref="LegalCaseRuling.Apply"/> exactly when <see cref="LegalCase.CaseType"/> is <see
/// cref="LegalCaseType.Repetundae"/>, matching that method's own established "an additive, gated call for
/// one specific case flavor" precedent (<see cref="Societates.ActioProSocioResolutionHook"/>'s identical
/// shape). <see cref="LegalCaseRuling.Apply"/>'s own ordinary consequences (Dignitas swing, relationship
/// scar, and — since this case type is capital-shaped — a <see
/// cref="Crime.PunishableOffenseSource.LegalConviction"/> offense) are untouched and already applied
/// before this runs; this hook adds §6.2's own further, contract-specific weight: "restitution... and,
/// concretely, permanent or long-term disqualification from future contract bidding."
/// </summary>
internal static class RepetundaeResolutionHook
{
    private static readonly LedgerAccountKey RestitutionSink = PublicContractsCatalog.ConvictionRestitutionSink;

    public static IDomainEvent[] Apply(WorldState state, LegalCase legalCase, LegalCaseVerdict verdict, GameDate date, string? causationId)
    {
        if (!state.ContractFraudLegalLinks.TryGet(legalCase.CaseId, out var link))
            return Array.Empty<IDomainEvent>();
        if (!state.PublicContractFraudRecords.TryGet(link!.FraudRecordId, out var record))
            return Array.Empty<IDomainEvent>();

        var events = new List<IDomainEvent>();
        var outcome = verdict == LegalCaseVerdict.Convicted ? LegalCaseVerdict.Convicted : LegalCaseVerdict.Acquitted;

        if (verdict != LegalCaseVerdict.Convicted)
        {
            state.PublicContractFraudRecords.Remove(link.FraudRecordId);
            state.PublicContractFraudRecords.Add(link.FraudRecordId, record! with { LegalOutcome = outcome });
            return events.ToArray();
        }

        if (state.PublicContracts.TryGet(record!.ContractId, out var contract) && contract!.ContractValue > Money.Zero)
        {
            var restitution = contract.ContractValue.Scale(PublicContractsCatalog.RestitutionFraction);
            if (restitution > Money.Zero)
            {
                events.Add(LedgerService.Post(
                    state, date, LedgerTransactionCategory.Gifts,
                    new[]
                    {
                        new LedgerPosting(LedgerAccountKey.ForHousehold(legalCase.DefendantId), -restitution),
                        new LedgerPosting(RestitutionSink, restitution),
                    },
                    reference: $"publicContracts:restitution:{legalCase.CaseId.ToTaggedString()}"));
            }

            // §6.2's own disqualification is sharper than an ordinary Reputation recovery — the state
            // reclaims the contract itself rather than leaving a convicted holder in place.
            if (contract.CurrentHolder == record.Holder)
            {
                state.PublicContracts.Remove(record.ContractId);
                state.PublicContracts.Add(
                    record.ContractId,
                    contract with { Status = PublicContractStatus.OpenForBidding, CurrentHolder = null, ContractValue = Money.Zero, AwardedDate = null, IsCuttingCorners = false });
            }
        }

        var disqualifiedUntil = new GameDate(date.TotalMonths + PublicContractsCatalog.DisqualificationMonths);
        state.PublicContractFraudRecords.Remove(link.FraudRecordId);
        state.PublicContractFraudRecords.Add(
            link.FraudRecordId,
            record with { LegalOutcome = outcome, DisqualifiedFromBidding = true, DisqualifiedUntilDate = disqualifiedUntil });

        return events.ToArray();
    }
}
