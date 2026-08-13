namespace Gens.Simulation.Characters;

/// <summary>Why a <see cref="MarriageRecord"/> closed (<c>gens-familia-design.md</c> §5.1). <see
/// cref="Death"/> is only ever produced internally by <see cref="CharacterLifecycleSystem"/> when a
/// married Character's mortality roll succeeds — a player-submitted <see cref="EndMarriageCommand"/>
/// may only close a marriage as <see cref="Divorce"/> or <see cref="Annulment"/>.</summary>
public enum MarriageEndReason
{
    Death,
    Divorce,
    Annulment,
}
