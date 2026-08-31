using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Correspondence;

/// <summary>§6's Inbox read model: which delivered, unanswered inbound letters a given recipient
/// actually still needs to act on. Mirrors <see cref="Travel.TravelTripQueries"/>'s identical
/// "small, stateless read helper over an <see cref="OrderedRegistry{TKey,TValue}"/>" shape.</summary>
public static class LetterQueries
{
    /// <summary>A letter belongs in the Inbox once it has actually arrived with real content — <see
    /// cref="LetterOutcome.Intercepted"/> deliveries never reach the recipient at all (§9), so they
    /// never surface here even though their own <see cref="LetterStatus"/> is <see
    /// cref="LetterStatus.Delivered"/> like every other resolved letter.</summary>
    public static IEnumerable<KeyValuePair<RuntimeId<Letter>, Letter>> PendingInbox(WorldState state, string recipientCharacterOrActorId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (string.IsNullOrWhiteSpace(recipientCharacterOrActorId))
            throw new ArgumentException("A recipient ID is required.", nameof(recipientCharacterOrActorId));

        foreach (var entry in state.Letters.InAscendingOrder())
        {
            var letter = entry.Value;
            if (letter.Direction != LetterDirection.Inbound)
                continue;
            if (letter.RecipientCharacterOrActorId != recipientCharacterOrActorId)
                continue;
            if (letter.Status != LetterStatus.Delivered)
                continue;
            if (!letter.RequiresResponse || letter.Responded)
                continue;
            if (letter.Outcome == LetterOutcome.Intercepted)
                continue;

            yield return entry;
        }
    }
}
