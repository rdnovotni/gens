using Gens.Simulation.Identity;
using Gens.Simulation.Numerics;
using Gens.Simulation.RealEstate;
using Gens.Simulation.Time;

namespace Gens.Simulation.Societates;

/// <summary>§2's three real Roman partnership scopes (Phase 15 item 2; <c>gens-societates-business-
/// partnerships-design.md</c> §2). Every real category the design doc names is represented, matching
/// <see cref="RealEstate.PropertyOwnerRef"/>'s and <see cref="Legal.LegalCase.CaseType"/>'s own
/// identical "every real category represented" precedent — see <see cref="FormSocietasCommands"/>'s
/// own doc comment for which of the three this item actually ties to a further real mechanic (only
/// <see cref="UnusRei"/> and <see cref="OmniumBonorum"/>; <see cref="Publicani"/> is schema-complete
/// but structurally inert, since no Publicanus Contract entity exists anywhere in this codebase for it
/// to link to).</summary>
public enum PartnershipType
{
    /// <summary>§2's "partnership for one thing" — a narrow, single-venture partnership. This item's
    /// own default, lightest-commitment type.</summary>
    UnusRei,

    /// <summary>§2's "partnership of all goods" — partners pool essentially their entire property, the
    /// highest-stakes application of §3's unlimited liability.</summary>
    OmniumBonorum,

    /// <summary>§2's tax-farming syndicate type. Kept for schema completeness (every real category
    /// represented), never tied to a real Publicanus Contract record by this item — see <see
    /// cref="FormSocietasCommands"/>'s own doc comment for why.</summary>
    Publicani,
}

/// <summary>§4's three real, recurring governance patterns (Phase 15 item 2). <see
/// cref="Societas.DesignatedPartner"/> names the one partner <see cref="DominantPartner"/> or <see
/// cref="SilentPartner"/> singles out — dominant for the former, silent for the latter — and is
/// required to be null for <see cref="EqualPartners"/>, where no single partner is singled out by
/// definition.</summary>
public enum SocietasGovernanceModel
{
    EqualPartners,
    DominantPartner,
    SilentPartner,
}

/// <summary>§6's dissolution vocabulary, extending Land Ownership &amp; Real Estate §7's own
/// <c>SocietasRecord.dissolutionTrigger</c> sketch (<c>"mutualAgreement" | "partnerDeath" |
/// "partnerInsolvency" | "fraud"</c>) with the one further case that sketch's own free-form list left
/// implicit: a Societas Unius Rei's own natural completion once its single venture concludes (§2's
/// "dissolving automatically the instant that purpose is complete").</summary>
public enum SocietasDissolutionTrigger
{
    MutualAgreement,
    PartnerDeath,
    PartnerInsolvency,

    /// <summary>§6's contested path: a confirmed <see cref="PartnerDisputeType.SuspectedFraud"/> <see
    /// cref="ActioProSocioLink"/> ruled for the plaintiff. The only trigger this item never applies
    /// through <see cref="DissolveSocietasCommand"/> directly — <see
    /// cref="ActioProSocioResolutionHook"/> applies it once a real Legal &amp; Court verdict actually
    /// confirms the fraud.</summary>
    Fraud,

    /// <summary>§2's Societas Unius Rei completing its single named venture — the one dissolution §6
    /// never needed Legal &amp; Court for even conceptually ("dissolving automatically the instant that
    /// purpose is complete").</summary>
    VentureComplete,
}

/// <summary>§7's three real, distinct partner dispute types, each the ground an <see
/// cref="ActioProSocioLink"/> stands on. <see cref="EarlyExitDispute"/> and <see
/// cref="ProfitDistributionDisagreement"/> have no prior "ground truth" detection step — they are real
/// disagreements a filing partner brings directly, matching §7's own "a dispute over interpretation...
/// real and human" framing; only <see cref="SuspectedFraud"/> has a resolved ground truth to catch,
/// set monthly by <see cref="PartnerSkimmingRiskSystem"/> and revealed by <see
/// cref="AuditPartnerCommand"/>, mirroring §7's own explicit "the direct partner-to-partner parallel to
/// Land Ownership &amp; Real Estate's own Operator-skimming risk... detectable the same way."</summary>
public enum PartnerDisputeType
{
    SuspectedFraud,
    EarlyExitDispute,
    ProfitDistributionDisagreement,
}

/// <summary>§10's data model, one partner's own stake and, for <see
/// cref="PartnerDisputeType.SuspectedFraud"/> only, the ground truth <see
/// cref="PartnerSkimmingRiskSystem"/> resolves monthly. §3's unlimited liability is deliberately not a
/// per-partner field here — the design doc treats it as absolute and uniform ("each partner's own
/// personal fortune... in principle without limit," §3), not a value that varies partner to partner,
/// so this record carries no <c>liabilityExposure</c> field the way §10's own sketch shows one: every
/// partner in every Societas this item can create is unlimited, a documented, code-enforced constant
/// rather than a redundant always-<c>"unlimited"</c> string on every partner.</summary>
public sealed record SocietasPartner(PropertyOwnerRef Owner, Fixed64 ShareFraction, bool IsSuspectedSkimming = false);

/// <summary>
/// §10's <c>Societas</c> record — a real, distinct Roman legal entity (Phase 15 item 2), extending
/// Land Ownership &amp; Real Estate §7's own <c>SocietasRecord</c> sketch with §2's real partnership
/// type, §4's real governance model, and §5's negotiated <c>lex societatis</c> terms that sketch left
/// entirely unbuilt. <see cref="LinkedPropertySubject"/> reuses <see
/// cref="RealEstate.PropertySubjectRef"/> directly rather than inventing a parallel
/// <c>linkedPropertyOrVentureId</c> reference type — the exact asset/venture a Societas Unius Rei was
/// formed around is already representable as a Plot or <see cref="RealEstate.PropertyRecord"/>.
/// <c>linkedPublicanusContractId</c> from §10's own data model sketch is deliberately not a field here
/// at all: no Publicanus Contract entity exists anywhere in this codebase (Land Ownership &amp; Real
/// Estate §8, still unbuilt, confirmed by direct search) for it to reference, and an always-null field
/// with nothing to ever set it would only restate that gap redundantly rather than name it once, here.
/// </summary>
public sealed record Societas
{
    private Societas()
    {
    }

    public required RuntimeId<Societas> Id { get; init; }
    public required PartnershipType PartnershipType { get; init; }
    public required SocietasGovernanceModel GovernanceModel { get; init; }

    /// <summary>§5's "the venture's own real duration or defining purpose (open-ended for a Societas
    /// Omnium Bonorum, bounded to a single voyage or contract for a Societas Unius Rei)" — a free-form
    /// negotiated term, not a structured schedule; this item does not attempt to parse or enforce a
    /// duration from it mechanically.</summary>
    public required string DurationOrPurpose { get; init; }

    /// <summary>§4's dominant or silent partner, depending on <see cref="GovernanceModel"/> — null for
    /// <see cref="SocietasGovernanceModel.EqualPartners"/>, required and a member of <see
    /// cref="Partners"/> otherwise.</summary>
    public PropertyOwnerRef? DesignatedPartner { get; init; }

    /// <summary>§5's negotiated profit-and-loss split, one entry per partner. Each entry's own <see
    /// cref="SocietasPartner.ShareFraction"/> already encodes §10's own separate <c>lexSocietatis.
    /// profitSplit</c> field — this item folds that field directly into the partner roster rather than
    /// duplicating the same fractions in two places.</summary>
    public required IReadOnlyList<SocietasPartner> Partners { get; init; }

    /// <summary>§8/§10's "the actual venture or Property Record this Societas was formed around" —
    /// null for a Societas with no single linked asset (e.g. a general Omnium Bonorum pooling, or a
    /// Publicani syndicate with no linked Publicanus Contract, per this item's own scope note above).</summary>
    public PropertySubjectRef? LinkedPropertySubject { get; init; }

    public required bool IsActive { get; init; }
    public SocietasDissolutionTrigger? DissolutionTrigger { get; init; }
    public GameDate? DissolvedDate { get; init; }

    public static Societas Create(
        RuntimeId<Societas> id,
        PartnershipType partnershipType,
        SocietasGovernanceModel governanceModel,
        string durationOrPurpose,
        IReadOnlyList<SocietasPartner> partners,
        PropertyOwnerRef? designatedPartner,
        PropertySubjectRef? linkedPropertySubject) =>
        new()
        {
            Id = id,
            PartnershipType = partnershipType,
            GovernanceModel = governanceModel,
            DurationOrPurpose = durationOrPurpose,
            DesignatedPartner = designatedPartner,
            Partners = partners,
            LinkedPropertySubject = linkedPropertySubject,
            IsActive = true,
            DissolutionTrigger = null,
            DissolvedDate = null,
        };

    /// <summary>Reconstructs a <see cref="Societas"/> from persisted save data (ADR 0010).</summary>
    public static Societas Restore(
        RuntimeId<Societas> id,
        PartnershipType partnershipType,
        SocietasGovernanceModel governanceModel,
        string durationOrPurpose,
        PropertyOwnerRef? designatedPartner,
        IReadOnlyList<SocietasPartner> partners,
        PropertySubjectRef? linkedPropertySubject,
        bool isActive,
        SocietasDissolutionTrigger? dissolutionTrigger,
        GameDate? dissolvedDate) =>
        new()
        {
            Id = id,
            PartnershipType = partnershipType,
            GovernanceModel = governanceModel,
            DurationOrPurpose = durationOrPurpose,
            DesignatedPartner = designatedPartner,
            Partners = partners,
            LinkedPropertySubject = linkedPropertySubject,
            IsActive = isActive,
            DissolutionTrigger = dissolutionTrigger,
            DissolvedDate = dissolvedDate,
        };

    /// <summary>Replaces the one <see cref="SocietasPartner"/> whose <see
    /// cref="SocietasPartner.Owner"/> matches <paramref name="updated"/>'s own owner, leaving every
    /// other partner untouched — the one place this item needs an immutable "update one list entry"
    /// operation (§7's skim-state ground truth).</summary>
    public Societas WithPartner(SocietasPartner updated) =>
        this with
        {
            Partners = Partners.Select(p => p.Owner == updated.Owner ? updated : p).ToArray(),
        };
}

/// <summary>Read-side helpers over a <see cref="Societas"/>'s own partner roster, matching <see
/// cref="Legal.LegalCaseResolver"/>'s and <see cref="RealEstate.PropertyResolver"/>'s own identical
/// "one shared resolver, not each caller re-scanning the list" convention.</summary>
public static class SocietasResolver
{
    public static bool TryGetPartner(Societas societas, PropertyOwnerRef owner, out SocietasPartner partner)
    {
        foreach (var candidate in societas.Partners)
        {
            if (candidate.Owner != owner)
                continue;
            partner = candidate;
            return true;
        }

        partner = default!;
        return false;
    }

    public static bool IsPartner(Societas societas, PropertyOwnerRef owner) => TryGetPartner(societas, owner, out _);
}
