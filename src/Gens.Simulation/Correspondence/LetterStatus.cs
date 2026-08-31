namespace Gens.Simulation.Correspondence;

/// <summary>A <see cref="Letter"/>'s own transit/response lifecycle — distinct from <see
/// cref="LetterOutcome"/>, which is what actually reached the recipient once <see
/// cref="Delivered"/>. Mirrors <see cref="Travel.TravelTripStatus"/>'s identical "terminal state kept
/// forever once recorded" shape.</summary>
public enum LetterStatus
{
    /// <summary>Being carried; <see cref="Letter.MonthsElapsed"/> counts up toward <see
    /// cref="Letter.TransitTimeMonths"/> (plus any §9 Redirection delay).</summary>
    InTransit,

    /// <summary>Transit is over and <see cref="LetterOutcome"/> is resolved. Terminal for every letter
    /// that either doesn't require a response or was <see cref="LetterOutcome.Intercepted"/> outright
    /// (nothing left to respond to); an inbound, undelivered-content-free letter still moves through
    /// this state, matching <see cref="Travel.TravelTripStatus.Arrived"/>'s own "a resting state, not
    /// itself terminal" shape for the one case that can still advance further.</summary>
    Delivered,

    /// <summary>An inbound <see cref="Letter"/> that required a response and got one, via <see
    /// cref="RespondToLetterCommand"/>. Terminal.</summary>
    Answered,
}
