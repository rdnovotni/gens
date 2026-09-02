using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.RealEstate;
using Gens.Simulation.State;

namespace Gens.Simulation.MerchantFamilies;

/// <summary>
/// §2's "a real, formally recognized Roman social and legal class" (Phase 15 item 3;
/// <c>gens-merchant-families-design.md</c> §2) and §9's <c>EquestrianStatus</c> data model — <b>computed,
/// not stored</b>, per §10's own explicit open question ("this document treats it as computed rather
/// than separately stored"): every field here is a direct read of an existing figure this codebase
/// already tracks, so there is nothing this item needs to persist and no save-migration risk from ever
/// getting that computation wrong. §2 names four real privileges once a household clears the threshold —
/// the angusticlavus, reserved public seating, equestrian-exclusive offices (Egypt's own Prefecture), and
/// Publicani eligibility — but specifies no further distinguishing criterion for any one of the four
/// beyond clearing the same wealth gate, so this item does not invent one: every field here reads
/// directly off <see cref="QualifiesByNetWorth"/> rather than four independently-tracked flags with
/// nothing yet to differentiate them, the same "not a redundant always-same field" judgment call <see
/// cref="Societates.SocietasPartner"/>'s own doc comment already made for unlimited liability.
/// </summary>
public readonly record struct EquestrianStatus(
    bool QualifiesByNetWorth,
    bool HoldsAngusticlavus,
    bool EligibleForEquestrianOffices,
    bool PublicaniEligible)
{
    public static readonly EquestrianStatus NotQualified = new(false, false, false, false);

    private static EquestrianStatus Qualified => new(true, true, true, true);

    internal static EquestrianStatus For(bool qualifiesByNetWorth) => qualifiesByNetWorth ? Qualified : NotQualified;
}

/// <summary>Resolves §2's Equestrian Order threshold against whichever of §2's real owner kinds this
/// codebase can actually read a Net Worth figure for (Phase 15 item 3) — a <see
/// cref="PropertyOwnerKind.PlayerHousehold"/>'s own <see cref="Economy.NetWorth.Total"/>, or a
/// <see cref="PropertyOwnerKind.RivalGens"/>'s own <see cref="Actors.LivingWorldActorNetWorth.Figure"/>
/// when that Rival House has actually been promoted to <see
/// cref="Actors.LivingWorldActorTier.Noteworthy"/> (a Background-tier actor's own Net Worth is only ever
/// a <see cref="Characters.HouseholdWealthBand"/>, per that record's own doc comment — no exact figure
/// to compare against a Denarii threshold). Every other owner kind (Individual Character, Temple,
/// Collegium, Municipal, Roman State, Societas placeholder, Imperial Patrimonium) has no tracked Net
/// Worth this codebase can read at all, and reads as <see cref="EquestrianStatus.NotQualified"/> — the
/// same honest "only some owner kinds resolve against a real, checkable figure" narrowing <see
/// cref="Societates.PartnerSkimmingRiskSystem"/> already established for a partner's own Character.
/// </summary>
public static class EquestrianStatusQuery
{
    public static EquestrianStatus Current(WorldState state, PropertyOwnerRef owner) =>
        EquestrianStatus.For(TryGetNetWorth(state, owner, out var netWorth) && netWorth >= MerchantFamiliesCatalog.EquestrianNetWorthThreshold);

    private static bool TryGetNetWorth(WorldState state, PropertyOwnerRef owner, out Money netWorth)
    {
        switch (owner.Kind)
        {
            case PropertyOwnerKind.PlayerHousehold:
                if (state.NetWorthAssessments.TryGet(RuntimeId<Household>.Parse(owner.OwnerId!), out var assessment))
                {
                    netWorth = assessment!.Total;
                    return true;
                }

                netWorth = Money.Zero;
                return false;

            case PropertyOwnerKind.RivalGens:
                if (state.Actors.TryGet(RuntimeId<Actor>.Parse(owner.OwnerId!), out var actor) && actor!.NetWorth.Figure is { } figure)
                {
                    netWorth = figure;
                    return true;
                }

                netWorth = Money.Zero;
                return false;

            default:
                netWorth = Money.Zero;
                return false;
        }
    }
}
