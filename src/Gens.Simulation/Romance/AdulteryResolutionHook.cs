using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Ledger;
using Gens.Simulation.Legal;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Romance;

/// <summary>
/// §12's real, mechanical consequence of an <see cref="AdulteryCaseLink"/> actually being ruled — called
/// from <see cref="LegalCaseRuling.Apply"/> exactly when <see cref="LegalCase.CaseType"/> is <see
/// cref="LegalCaseType.Adultery"/>, matching that method's own established "an additive, gated call for
/// one specific case flavor" precedent (<see cref="PublicContracts.RepetundaeResolutionHook"/>'s and <see
/// cref="Societates.ActioProSocioResolutionHook"/>'s identical shape). <see cref="LegalCaseRuling.Apply"/>'s
/// own ordinary consequences (the <see cref="LegalSentence.Relegatio"/> sentence itself — rolled by <see
/// cref="LegalCaseResolver.RollVerdict"/>, not this hook — Dignitas swing, relationship scar, and a
/// capital-severity <see cref="Crime.PunishableOffenseSource.LegalConviction"/> offense) are untouched and
/// already applied before this runs; this hook adds §12/§13's own further, adultery-specific weight:
/// partial property confiscation, the Status/Role Dignitas modifier, and a real <see cref="InfamiaStatus"/>
/// mark on the convicted party.
///
/// Only a <see cref="LegalCaseVerdict.Convicted"/> verdict triggers any of this — an acquittal or
/// dismissal leaves the linked <see cref="AffairRecord"/> exactly as <see
/// cref="FileAdulteryCaseCommand"/> already set it (already-<see cref="AffairResolution.ProsecutedAdultery"/>);
/// this hook does not need to touch that record again either way, unlike <see
/// cref="PublicContracts.RepetundaeResolutionHook"/>'s own comparable record, since <see
/// cref="AffairRecord"/> has no separate "legal outcome" field of its own to update.
/// </summary>
internal static class AdulteryResolutionHook
{
    private static readonly LedgerAccountKey ConfiscationSink = new(LedgerAccountKind.System, "legal:adulteryConfiscation");

    public static IDomainEvent[] Apply(WorldState state, LegalCase legalCase, LegalCaseVerdict verdict, GameDate date, string? causationId)
    {
        if (!state.AdulteryCaseLinks.TryGet(legalCase.CaseId, out var link))
            return Array.Empty<IDomainEvent>();
        if (!state.AffairRecords.TryGet(link!.AffairId, out var affair))
            return Array.Empty<IDomainEvent>();
        if (verdict != LegalCaseVerdict.Convicted)
            return Array.Empty<IDomainEvent>();

        var events = new List<IDomainEvent>();

        // §12's "partial property confiscation," on top of LegalCaseRuling.Apply's own already-applied
        // ordinary conviction consequences — mirrors RepetundaeResolutionHook's own restitution-posting
        // pattern exactly, scaled by this module's own, deliberately smaller, confiscation fraction.
        if (state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(legalCase.DefendantId), out var account)
            && account!.Balance > Money.Zero)
        {
            var confiscation = account.Balance.Scale(RomanceCatalog.AdulteryConfiscationFraction);
            if (confiscation > Money.Zero)
            {
                events.Add(LedgerService.Post(
                    state, date, LedgerTransactionCategory.Gifts,
                    new[]
                    {
                        new LedgerPosting(LedgerAccountKey.ForHousehold(legalCase.DefendantId), -confiscation),
                        new LedgerPosting(ConfiscationSink, confiscation),
                    },
                    reference: $"romance:adulteryConfiscation:{legalCase.CaseId.ToTaggedString()}"));
            }
        }

        // §13's Status/Role Dignitas modifier — an ADDITIVE further AdjustDignitasCommand call, stacked
        // on top of LegalCaseRuling.Apply's own already-applied ordinary conviction Dignitas penalty, not
        // a replacement for it (matching RepetundaeResolutionHook's own "ordinary consequences already
        // applied, this hook adds further weight" precedent exactly).
        var modifier = StatusRoleDignitasModifier.Calculate(state, affair!.OffenderCharacterId, affair.ThirdPartyCharacterId);
        var higherRankedPartyId = StatusRoleDignitasModifier.DetermineHigherRankedParty(
            state, affair.OffenderCharacterId, affair.ThirdPartyCharacterId);
        if (modifier != 0 && higherRankedPartyId is { } higherRankedId
            && state.Characters.TryGet(higherRankedId, out var higherRankedCharacter)
            && higherRankedCharacter!.Household is { } higherRankedHouseholdId)
        {
            events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
                state, new AdjustDignitasCommand(
                    state.CommandIds.Issue(), "system", date, causationId, higherRankedHouseholdId, modifier,
                    $"adultery conviction {legalCase.CaseId.ToTaggedString()} status/role modifier")).Events);
        }

        // §13: a real Infamia mark on the convicted party (the affair's own offender — the defendant
        // side of this case, per FileAdulteryCaseCommand's own household resolution).
        events.AddRange(GrantInfamiaCommands.Pipeline.Execute(
            state, new GrantInfamiaCommand(
                state.CommandIds.Issue(), "system", date, causationId, affair.OffenderCharacterId,
                InfamiaSource.ConvictedAdultery)).Events);

        return events.ToArray();
    }
}
