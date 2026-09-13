using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Travel;

namespace Gens.Simulation.Companions;

/// <summary>One <see cref="Travel.TravelParty"/> retinue member's own vacated position back home while
/// they travel — surfaced by <see cref="TravelRetinueQuery"/> so a future encounter screen can show the
/// player what staying away is costing them (§7).</summary>
public readonly record struct VacatedRetinuePosition(RuntimeId<Character> HolderId, string PositionName);

/// <summary>§7's Travel Retinue effects, resolved from whatever Position/Duty each retinue member
/// currently holds — a pure derived read, no state of its own. <see cref="BestAttributes"/> takes the
/// per-<see cref="CoreAttributeAxis"/> maximum across every retinue member's own <see
/// cref="Character.GetEffectiveAttributes"/>, the retinue's best available asset for an encounter roll
/// (§7.2's "the retinue's own Core Attributes become usable during an Encounter").</summary>
public readonly record struct TravelRetinueContribution(
    bool HasAmbushMitigation,
    bool HasCorrespondenceUnlocked,
    bool HasDiseaseMitigation,
    CoreAttributes BestAttributes,
    IReadOnlyList<VacatedRetinuePosition> VacatedPositions);

/// <summary>
/// Resolves a <see cref="TravelParty"/>'s Travel Retinue contribution (Phase 17 item 1; §7) by reading
/// each retinue member's currently-held <see cref="OverseerAssignment"/>, <see
/// cref="SeniorPositionAssignment"/>, or <see cref="DutyAssignment"/> — never a value stored redundantly
/// on <see cref="TravelParty"/> itself (that record's own doc comment: "what each member contributes...
/// is that document's own... territory," this item's own territory now). Only the party's own <see
/// cref="TravelParty.RetinueIds"/> are read, not the traveler themself — §7 frames the Retinue as who
/// accompanies the traveler, not the traveler's own standing.
/// </summary>
public static class TravelRetinueQuery
{
    public static TravelRetinueContribution Resolve(WorldState state, TravelParty party)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (party is null)
            throw new ArgumentNullException(nameof(party));

        var hasBodyguard = false;
        var hasSecretary = false;
        var hasPhysician = false;
        var best = new CoreAttributes(0, 0, 0, 0, 0);
        var vacated = new List<VacatedRetinuePosition>();

        foreach (var memberId in party.RetinueIds)
        {
            if (!state.Characters.TryGet(memberId, out var member))
                continue;

            best = Max(best, member.GetEffectiveAttributes());

            if (member.Duty is { Slot: DutySlot.Physician })
                hasPhysician = true;

            if (OverseerResolver.ActiveRecordForCharacter(state, memberId) is { } overseer)
            {
                if (overseer.Role == OverseerRole.Valetudinarius)
                    hasPhysician = true;
                if (overseer.OnLeaveSince is not null)
                    vacated.Add(new VacatedRetinuePosition(memberId, overseer.Role.ToString()));
            }

            if (SeniorPositionResolver.ActiveRecordForCharacter(state, memberId) is { } senior)
            {
                if (senior.Title is SeniorPositionTitle.Bodyguard or SeniorPositionTitle.GuardCaptain)
                    hasBodyguard = true;
                if (senior.Title == SeniorPositionTitle.Secretary)
                    hasSecretary = true;
                if (senior.Title == SeniorPositionTitle.CourtPhysician)
                    hasPhysician = true;
                if (senior.OnLeaveSince is not null)
                    vacated.Add(new VacatedRetinuePosition(memberId, senior.Title.ToString()));
            }
        }

        return new TravelRetinueContribution(hasBodyguard, hasSecretary, hasPhysician, best, vacated);
    }

    private static CoreAttributes Max(CoreAttributes a, CoreAttributes b) => new(
        Math.Max(a.Diplomacy, b.Diplomacy),
        Math.Max(a.Martial, b.Martial),
        Math.Max(a.Stewardship, b.Stewardship),
        Math.Max(a.Intrigue, b.Intrigue),
        Math.Max(a.Learning, b.Learning));
}
