using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.RealEstate;
using Gens.Simulation.Reputation;
using Gens.Simulation.Societates;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.PublicWorks;

public sealed record PublicWorkFundedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<PublicWork> PublicWorkId,
    RuntimeId<Settlement> SettlementId,
    PublicWorkType WorkType,
    PublicWorkFundingSource FundingSource,
    string? CausationId) : IDomainEvent
{
    public string Type => "publicWorks.funded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { PublicWorkId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// §2/§3/§4/§7's real construction-and-funding mutation (Phase 15 item 9) — the one command path every
/// new <see cref="PublicWork"/> is created through, covering both real funding sources (§7): a named
/// private patron or Societas (§2's euergetism proper, real Dignitas and inscription credit, §4) and the
/// settlement's own Treasury (§7's impersonal state alternative, no patron credit at all). Exactly one of
/// <see cref="FundingPatronId"/>/<see cref="FundingSocietasId"/> is required for <see
/// cref="PublicWorkFundingSource.PrivateEuergetism"/>, and neither is accepted for <see
/// cref="PublicWorkFundingSource.StateTaxRevenue"/>.
/// </summary>
public sealed record FundPublicWorkCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Settlement> SettlementId,
    PublicWorkType WorkType,
    PublicWorkFundingSource FundingSource,
    RuntimeId<District>? DistrictId = null,
    PropertyOwnerRef? FundingPatronId = null,
    RuntimeId<Societas>? FundingSocietasId = null) : ICommand;

public static class FundPublicWorkCommands
{
    public static readonly ValidationErrorCode SettlementNotFound = new("publicWorks.fund.settlementNotFound");
    public static readonly ValidationErrorCode DistrictNotFound = new("publicWorks.fund.districtNotFound");
    public static readonly ValidationErrorCode PatronRequired = new("publicWorks.fund.patronRequired");
    public static readonly ValidationErrorCode PatronNotSupportedForStateFunding = new("publicWorks.fund.patronNotSupportedForStateFunding");
    public static readonly ValidationErrorCode BothPatronAndSocietasSupplied = new("publicWorks.fund.bothPatronAndSocietasSupplied");
    public static readonly ValidationErrorCode UnsupportedPatronKind = new("publicWorks.fund.unsupportedPatronKind");
    public static readonly ValidationErrorCode SocietasNotFound = new("publicWorks.fund.societasNotFound");
    public static readonly ValidationErrorCode SocietasNotActive = new("publicWorks.fund.societasNotActive");
    public static readonly ValidationErrorCode HarborRequiresCoastalSettlement = new("publicWorks.fund.harborRequiresCoastalSettlement");
    public static readonly ValidationErrorCode InsufficientFunds = new("publicWorks.fund.insufficientFunds");

    public static readonly CommandPipeline<WorldState, FundPublicWorkCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, FundPublicWorkCommand command)
    {
        if (!state.Settlements.TryGet(command.SettlementId, out _))
            return SettlementNotFound;
        if (command.DistrictId is { } districtId && !state.Districts.TryGet(districtId, out _))
            return DistrictNotFound;

        if (command.WorkType == PublicWorkType.Harbor)
        {
            // §3's own "a coastal settlement's own trade capacity" — matching Shipping's identical
            // "at least one Coast/River-terrain Plot" maritime-access gate (Phase 15 item 8;
            // CommissionShipCommands.SettlementNotMaritime) applied here to the one work type §3 frames
            // as inherently coastal.
            var isCoastal = state.Plots.InAscendingOrder()
                .Any(entry => entry.Value.SettlementId == command.SettlementId && entry.Value.Terrain == TerrainType.Coast);
            if (!isCoastal)
                return HarborRequiresCoastalSettlement;
        }

        if (command.FundingSource == PublicWorkFundingSource.StateTaxRevenue)
        {
            if (command.FundingPatronId is not null || command.FundingSocietasId is not null)
                return PatronNotSupportedForStateFunding;
            return null;
        }

        // PrivateEuergetism.
        if (command.FundingPatronId is not null && command.FundingSocietasId is not null)
            return BothPatronAndSocietasSupplied;

        if (command.FundingSocietasId is { } societasId)
        {
            if (!state.Societates.TryGet(societasId, out var societas))
                return SocietasNotFound;
            if (!societas!.IsActive)
                return SocietasNotActive;
            return null;
        }

        if (command.FundingPatronId is not { } patron)
            return PatronRequired;
        if (patron.Kind is not (PropertyOwnerKind.PlayerHousehold or PropertyOwnerKind.RivalGens))
            return UnsupportedPatronKind;

        if (patron.Kind == PropertyOwnerKind.PlayerHousehold)
        {
            var householdId = RuntimeId<Household>.Parse(patron.OwnerId!);
            var cost = PublicWorksCatalog.ConstructionCost(command.WorkType);
            var balance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var account)
                ? account!.Balance
                : Money.Zero;
            if (balance.RawValue < cost.RawValue)
                return InsufficientFunds;
        }

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, FundPublicWorkCommand command)
    {
        var events = new List<IDomainEvent>();
        var cost = PublicWorksCatalog.ConstructionCost(command.WorkType);

        // §7's real funding settlement: state funding draws from the Treasury (SanitationInvestment's
        // own established LedgerAccountKey.ForSettlementTreasury account); a PlayerHousehold patron pays
        // from their own ledger account (already fund-checked above); a RivalGens or Societas patron has
        // no real tracked balance this codebase can debit — routed through the Mint with no real charge,
        // matching TransferPropertyCommand's own "route an owner kind this item cannot yet track a real
        // balance for through the Mint" precedent.
        if (command.FundingSource == PublicWorkFundingSource.StateTaxRevenue)
        {
            events.Add(LedgerService.Post(
                state, command.SubmittedDate, LedgerTransactionCategory.Treasury,
                new[]
                {
                    new LedgerPosting(LedgerAccountKey.ForSettlementTreasury(command.SettlementId), -cost),
                    new LedgerPosting(LedgerAccountKey.Mint, cost),
                },
                reference: $"publicWorks.fund.state:{command.SettlementId.ToTaggedString()}"));
        }
        else if (command.FundingPatronId is { Kind: PropertyOwnerKind.PlayerHousehold } patron)
        {
            var householdId = RuntimeId<Household>.Parse(patron.OwnerId!);
            events.Add(LedgerService.Post(
                state, command.SubmittedDate, LedgerTransactionCategory.Treasury,
                new[]
                {
                    new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), -cost),
                    new LedgerPosting(LedgerAccountKey.Mint, cost),
                },
                reference: $"publicWorks.fund.private:{householdId.ToTaggedString()}"));
        }

        var publicWorkId = state.PublicWorkIds.Issue();
        var work = PublicWork.Create(
            publicWorkId, command.SettlementId, command.DistrictId, command.WorkType, command.FundingSource,
            command.FundingPatronId, command.FundingSocietasId, command.SubmittedDate);
        PublicWorkResolver.Set(state, work);

        events.Add(new PublicWorkFundedEvent(
            state.EventIds.Issue(), command.SubmittedDate, publicWorkId, command.SettlementId, command.WorkType,
            command.FundingSource, command.CommandId.ToTaggedString()));

        // §4's real Dignitas/inscription credit — only for a genuinely private patron.
        if (command.FundingSource == PublicWorkFundingSource.PrivateEuergetism)
        {
            if (command.FundingPatronId is { Kind: PropertyOwnerKind.PlayerHousehold } playerPatron)
            {
                var householdId = RuntimeId<Household>.Parse(playerPatron.OwnerId!);
                events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
                    state, new AdjustDignitasCommand(
                        state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                        householdId, PublicWorksCatalog.PrivateFundingDignitasAward, $"public works: funded a {command.WorkType}")).Events);
                EuergetismObligationResolver.RecordFunded(state, householdId);
            }
            else if (command.FundingSocietasId is { } societasId && state.Societates.TryGet(societasId, out var societas))
            {
                // §7's joint Societas venture — each partner who resolves to a real PlayerHousehold
                // shares in the credit, the same "only some owner kinds resolve against a real,
                // checkable figure" narrowing Societates.PartnerSkimmingRiskSystem already established;
                // a RivalGens/other-kind partner cannot receive Dignitas through this item's own path
                // (Reputation.AdjustDignitasCommand is household-scoped) and is honestly skipped.
                foreach (var partner in societas!.Partners)
                {
                    if (partner.Owner.Kind != PropertyOwnerKind.PlayerHousehold)
                        continue;
                    var partnerHouseholdId = RuntimeId<Household>.Parse(partner.Owner.OwnerId!);
                    events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
                        state, new AdjustDignitasCommand(
                            state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                            partnerHouseholdId, PublicWorksCatalog.PrivateFundingDignitasAward,
                            $"public works: jointly funded a {command.WorkType}")).Events);
                    EuergetismObligationResolver.RecordFunded(state, partnerHouseholdId);
                }
            }
        }

        // §3's Bridge — a one-time bump to every already-Districted Plot's own tracked Property Value in
        // the linked District, per PublicWorksCatalog.BridgePropertyValueBonusPerPlot's own doc comment.
        if (command.WorkType == PublicWorkType.Bridge && command.DistrictId is { } bridgeDistrictId)
        {
            foreach (var entry in state.PlotPropertyExtensions.InAscendingOrder().ToArray())
            {
                if (entry.Value.DistrictId != bridgeDistrictId)
                    continue;
                PlotPropertyResolver.Set(state, entry.Value with { Value = entry.Value.Value + PublicWorksCatalog.BridgePropertyValueBonusPerPlot });
            }
        }

        return events.ToArray();
    }
}
