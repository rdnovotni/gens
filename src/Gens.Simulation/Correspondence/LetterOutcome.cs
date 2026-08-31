namespace Gens.Simulation.Correspondence;

/// <summary>§9's message-content risk resolution — what a <see cref="Letter"/>'s content actually was
/// once transit finished, independent of §9's own separate Redirection delay (<see
/// cref="Letter.Redirected"/>), which is about the recipient's whereabouts, not the message's
/// integrity.</summary>
public enum LetterOutcome
{
    /// <summary>Still <see cref="LetterStatus.InTransit"/> — not yet resolved.</summary>
    Pending,

    /// <summary>Reached the recipient exactly as sent.</summary>
    DeliveredIntact,

    /// <summary>Intercepted and never passed on — the recipient never actually sees this letter's
    /// content. §9's "interception" outcome without §9's own further "forgery" complication.</summary>
    Intercepted,

    /// <summary>Intercepted, altered, and passed on anyway — the recipient receives content the
    /// original sender never wrote. §9's own "forgery" outcome; forgery detection mechanics are
    /// explicitly unresolved (§12), so this item only records that a forgery happened, not whether
    /// anyone in-fiction ever detects it.</summary>
    Forged,
}
