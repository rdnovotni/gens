using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Numerics;
using Gens.Simulation.RealEstate;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Societates;

/// <summary>§7/§9's real mechanical consequence of a successful <see
/// cref="PartnerDisputeType.EarlyExitDispute"/> ruling (Phase 15 item 2) — "particularly relevant for a
/// Societas Omnium Bonorum, where unwinding one partner's own stake from a genuinely comprehensive
/// pooled arrangement is real, complicated work rather than a clean, instant withdrawal": the
/// withdrawing partner's own <see cref="SocietasPartner.ShareFraction"/> is redistributed
/// proportionally across the remaining partners (so their own shares still sum to <see
/// cref="Fixed64.One"/>), matching Land Ownership &amp; Real Estate §7's own "dissolution... reuses...
/// proportional inheritance-division logic" precedent applied to an exit rather than a full
/// dissolution. A withdrawal that would leave fewer than two partners instead dissolves the whole <see
/// cref="Societas"/> outright (a one-partner "partnership" is not a partnership at all) — via <see
/// cref="DissolveSocietasCommands"/> itself, <see cref="SocietasDissolutionTrigger.MutualAgreement"/>,
/// rather than this command inventing a second dissolution path.</summary>
public sealed record WithdrawPartnerCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Societas> SocietasId,
    PropertyOwnerRef WithdrawingPartnerOwner) : ICommand;

public sealed record PartnerWithdrewEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Societas> SocietasId,
    PropertyOwnerRef WithdrawingPartnerOwner,
    bool SocietasDissolved,
    string? CausationId) : IDomainEvent
{
    public string Type => "societates.partnerWithdrew";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { SocietasId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class WithdrawPartnerCommands
{
    public static readonly ValidationErrorCode SocietasNotFound = new("societates.withdraw.societasNotFound");
    public static readonly ValidationErrorCode PartnerNotFound = new("societates.withdraw.partnerNotFound");

    public static readonly CommandPipeline<WorldState, WithdrawPartnerCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, WithdrawPartnerCommand command)
    {
        if (!state.Societates.TryGet(command.SocietasId, out var societas) || !societas!.IsActive)
            return SocietasNotFound;
        if (!SocietasResolver.IsPartner(societas, command.WithdrawingPartnerOwner))
            return PartnerNotFound;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, WithdrawPartnerCommand command)
    {
        state.Societates.TryGet(command.SocietasId, out var societas);
        SocietasResolver.TryGetPartner(societas!, command.WithdrawingPartnerOwner, out var withdrawing);
        var remaining = societas!.Partners.Where(p => p.Owner != command.WithdrawingPartnerOwner).ToArray();

        var events = new List<IDomainEvent>();

        if (remaining.Length < 2)
        {
            events.AddRange(DissolveSocietasCommands.Pipeline.Execute(
                state, new DissolveSocietasCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    command.SocietasId, SocietasDissolutionTrigger.MutualAgreement)).Events);

            events.Add(new PartnerWithdrewEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.SocietasId, command.WithdrawingPartnerOwner,
                SocietasDissolved: true, command.CommandId.ToTaggedString()));
            return events.ToArray();
        }

        var redistributionFactor = Fixed64.Divide(Fixed64.One, Fixed64.One - withdrawing.ShareFraction);
        var redistributed = remaining
            .Select(p => p with { ShareFraction = Fixed64.Multiply(p.ShareFraction, redistributionFactor) })
            .ToArray();

        state.Societates.Remove(command.SocietasId);
        state.Societates.Add(command.SocietasId, societas with { Partners = redistributed });

        events.Add(new PartnerWithdrewEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.SocietasId, command.WithdrawingPartnerOwner,
            SocietasDissolved: false, command.CommandId.ToTaggedString()));
        return events.ToArray();
    }
}
