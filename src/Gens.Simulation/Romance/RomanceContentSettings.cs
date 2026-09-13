using System.Linq;
#nullable enable
namespace Gens.Simulation.Romance;

/// <summary>
/// A single, campaign-level content-intensity toggle for <see cref="ChildbirthResolutionSystem"/>
/// (<c>gens-romance-sexuality-lineage-design.md</c> §9, §17). Deliberately not threaded through <see
/// cref="Campaign.CampaignConfig"/>: that type's own <c>ContentToggles</c> list is a bootstrap-time-only
/// concept never carried into <see cref="State.WorldState"/>, and plumbing it there for this one flag
/// would touch every system's constructor for no benefit this item needs. Instead this is the simplest
/// possible campaign-level setting: one small record, one plain property on <see
/// cref="State.WorldState"/> itself, mirroring <see cref="State.WorldState.Date"/>'s own "private
/// setter, defaulted at construction" shape.
///
/// <b>Scope note:</b> nothing yet SETS this away from its default — no campaign-bootstrap UI or option
/// exists this pass to let a player actually choose it. The toggle exists and <see
/// cref="ChildbirthResolutionSystem"/> already reads it correctly; wiring a real settings surface that
/// changes it is out of this item's scope, left for a future pass.
/// </summary>
/// <param name="FertilityRiskAbstracted">When <c>true</c>, <see cref="ChildbirthResolutionSystem"/>
/// skips both the maternal-death and infant-death risk rolls entirely and always resolves a due
/// pregnancy as both mother and infant surviving — a lower-intensity content mode for a table that
/// would rather not have childbirth read as a real-name death source. Defaults to <c>false</c> (full
/// §9 risk modeling) so every pre-existing save and test that knows nothing about this setting keeps
/// behaving exactly as before.</param>
public sealed record RomanceContentSettings(bool FertilityRiskAbstracted);
