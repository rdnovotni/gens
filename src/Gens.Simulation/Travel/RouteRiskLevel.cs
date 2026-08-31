namespace Gens.Simulation.Travel;

/// <summary>§4's route-danger classification — "real-stakes events... weighted up sharply on genuinely
/// dangerous routes (Frontier, Campaign, sea lanes) and down on secure ones." This item only builds the
/// classification itself, the "trigger conditions" §4 says existing retinue bonuses (Companions &amp;
/// Court Positions §7.2, not yet built) apply against; the actual weighted event pool, dice rolls, and
/// retinue mitigation are that document's own future job, not this one's.</summary>
public enum RouteRiskLevel
{
    Secure,
    Guarded,
    Dangerous,
}
