using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Activities;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Feasts;

/// <summary>§6's real, meaningful tag for why a Feast is happening. Tag-only for this pass — it shapes
/// narrative flavor and <see cref="FeastCatalog"/>'s informational default guest-count/Scale guidance,
/// but does not require or create a link into Politics &amp; Patronage, Ancestor Veneration &amp;
/// Funerary Customs, Religion, Military &amp; Combat, or Public Works &amp; Euergetism — none of those
/// five systems expose a real, hostable-Feast hook today (confirmed by direct search: <c>Clientela</c>
/// has only the automatic monthly Salutatio, no hostable dinner; <c>FuneralRecord</c> has no feast
/// sub-component; no Military Triumph concept exists anywhere; <c>CompetitiveEuergetismEvent</c> has no
/// feast tie-in). Deeper integration for each is a disclosed, deliberate fast-follow, matching this
/// roadmap's own repeated precedent for a genuinely absent dependency elsewhere in Phase 17.</summary>
public enum FeastPurpose
{
    PatronageDinner,
    FuneralFeast,
    WeddingFeast,
    ReligiousFestival,
    TriumphalBanquet,
    CompetitiveEuergetismFeast,
    OrdinarySocial,
}

/// <summary>
/// The Feast-owned facts a generic <see cref="HostedActivity"/> has no field for (Phase 17 item 5;
/// <c>gens-feasts-design.md</c> §6, §7, §10's <c>Feast extends Activity</c>). A 1:1 sidecar kept on the
/// exact same <see cref="Activity"/> runtime ID the Activity Engine already issues (<see
/// cref="WorldState.ActivityIds"/>) — never a new ID kind, matching how <see
/// cref="Gens.Simulation.Companions.AppointSecondSettlementProcuratorCommand"/> already creates its own
/// linked <see cref="Gens.Simulation.Stewardship.StewardshipAssignment"/> atomically alongside its
/// primary effect. Present only for an Activity whose <c>TypeKey</c> is <see
/// cref="FeastCatalog.FeastType"/>'s own key.
/// </summary>
/// <param name="ArbiterBibendiId">§3's Comissatio-phase "master of the drinking" — nullable, since a
/// lighter, ad hoc evening may have none. Grants no separate Quality bonus: when this Character
/// currently holds <see cref="Gens.Simulation.Companions.SeniorPositionTitle.Symposiarch"/> in the host's own household,
/// the Activity Engine's existing <see cref="ActivityCatalog.HostOperatorTitles"/> bonus already rewards
/// that generically — this field is purely descriptive/recorded, the cleanest possible "wiring, not
/// reinventing" instance in the whole item.</param>
/// <param name="EntertainmentDescription">§7's optional Entertainment slot — free-text, since neither
/// Masterworks nor a Wandering-Populations-as-Activity-Entertainment link exists anywhere in this
/// codebase to attach a real object to (confirmed by direct search). No mechanics read this field; it is
/// flavor only, exactly like <see cref="ArbiterBibendiId"/>'s own "Symposiarch already covers the real
/// bonus" reasoning.</param>
public sealed record FeastRecord(
    RuntimeId<Activity> ActivityId,
    FeastPurpose Purpose,
    RuntimeId<Character>? ArbiterBibendiId,
    string? EntertainmentDescription);

/// <summary>Read-side helpers over <see cref="WorldState.FeastRecords"/>.</summary>
public static class FeastRecordResolver
{
    public static FeastRecord? Get(WorldState state, RuntimeId<Activity> activityId) =>
        state.FeastRecords.TryGet(activityId, out var record) ? record : null;
}
