using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Numerics;
using Gens.Simulation.RealEstate;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Societates;

/// <summary>
/// §5's <c>lex societatis</c> negotiation (Phase 15 item 2) — "a real, meaningful Interaction rather
/// than an instant checkbox," resolving §2's partnership type, §4's governance model, the negotiated
/// profit-and-loss split (folded directly into each <see cref="SocietasPartner.ShareFraction"/>, per
/// <see cref="Societas.Partners"/>'s own doc comment), and the venture's own duration or purpose. This
/// item treats every real category §2 names as formable — <see cref="PartnershipType.Publicani"/>
/// included — but only <see cref="PartnershipType.UnusRei"/> and <see
/// cref="PartnershipType.OmniumBonorum"/> ever reach a further real mechanic downstream (§3's
/// unlimited-liability sizing, <see cref="SocietatesCatalog.OmniumBonorumLiabilityMultiplier"/>):
/// forming a Publicani Societas is real and persists, but this item does not tie it to a Publicanus
/// Contract, since none exists anywhere in this codebase yet (Land Ownership &amp; Real Estate §8,
/// confirmed unbuilt by direct search) — matching <see cref="RealEstateCatalog.MaxDistrictsForStage"/>'s
/// own "Publicani tax farming... explicitly untouched" scope note from item 1.
/// </summary>
public sealed record FormSocietasCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    PartnershipType PartnershipType,
    SocietasGovernanceModel GovernanceModel,
    string DurationOrPurpose,
    IReadOnlyList<SocietasPartner> Partners,
    PropertyOwnerRef? DesignatedPartner = null,
    PropertySubjectRef? LinkedPropertySubject = null) : ICommand;

public sealed record SocietasFormedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Societas> SocietasId,
    PartnershipType PartnershipType,
    SocietasGovernanceModel GovernanceModel,
    string? CausationId) : IDomainEvent
{
    public string Type => "societates.formed";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { SocietasId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class FormSocietasCommands
{
    public static readonly ValidationErrorCode EmptyDurationOrPurpose = new("societates.form.emptyDurationOrPurpose");
    public static readonly ValidationErrorCode TooFewPartners = new("societates.form.tooFewPartners");
    public static readonly ValidationErrorCode DuplicatePartner = new("societates.form.duplicatePartner");
    public static readonly ValidationErrorCode NonPositiveShareFraction = new("societates.form.nonPositiveShareFraction");
    public static readonly ValidationErrorCode ShareFractionsDoNotSumToOne = new("societates.form.shareFractionsDoNotSumToOne");
    public static readonly ValidationErrorCode DesignatedPartnerRequired = new("societates.form.designatedPartnerRequired");
    public static readonly ValidationErrorCode DesignatedPartnerNotAllowed = new("societates.form.designatedPartnerNotAllowed");
    public static readonly ValidationErrorCode DesignatedPartnerNotAMember = new("societates.form.designatedPartnerNotAMember");
    public static readonly ValidationErrorCode LinkedSubjectNotFound = new("societates.form.linkedSubjectNotFound");

    public static readonly CommandPipeline<WorldState, FormSocietasCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, FormSocietasCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.DurationOrPurpose))
            return EmptyDurationOrPurpose;
        if (command.Partners is null || command.Partners.Count < 2)
            return TooFewPartners;
        if (command.Partners.Select(p => p.Owner).Distinct().Count() != command.Partners.Count)
            return DuplicatePartner;
        if (command.Partners.Any(p => p.ShareFraction <= Fixed64.Zero))
            return NonPositiveShareFraction;

        var total = Fixed64.Zero;
        foreach (var partner in command.Partners)
            total += partner.ShareFraction;
        if (total != Fixed64.One)
            return ShareFractionsDoNotSumToOne;

        if (command.GovernanceModel == SocietasGovernanceModel.EqualPartners)
        {
            if (command.DesignatedPartner is not null)
                return DesignatedPartnerNotAllowed;
        }
        else
        {
            if (command.DesignatedPartner is not { } designated)
                return DesignatedPartnerRequired;
            if (!command.Partners.Any(p => p.Owner == designated))
                return DesignatedPartnerNotAMember;
        }

        if (command.LinkedPropertySubject is { } subject && !PropertyResolver.TryResolve(state, subject, out _))
            return LinkedSubjectNotFound;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, FormSocietasCommand command)
    {
        var id = state.SocietasIds.Issue();
        var societas = Societas.Create(
            id, command.PartnershipType, command.GovernanceModel, command.DurationOrPurpose,
            command.Partners, command.DesignatedPartner, command.LinkedPropertySubject);
        state.Societates.Add(id, societas);

        return new IDomainEvent[]
        {
            new SocietasFormedEvent(
                state.EventIds.Issue(), command.SubmittedDate, id, command.PartnershipType, command.GovernanceModel,
                command.CommandId.ToTaggedString()),
        };
    }
}
