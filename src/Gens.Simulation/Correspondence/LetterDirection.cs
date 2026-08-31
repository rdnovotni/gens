namespace Gens.Simulation.Correspondence;

/// <summary>§11's <c>direction</c> field — which way a <see cref="Letter"/> is traveling. <see
/// cref="Outbound"/> letters are sent by <see cref="SendLetterCommand"/>; <see cref="Inbound"/> ones
/// originate from another Living World Actor (§6's Inbox) via <see
/// cref="OriginateInboundLetterCommand"/> and are the only kind <see cref="RespondToLetterCommand"/>
/// can ever target.</summary>
public enum LetterDirection
{
    Outbound,
    Inbound,
}
