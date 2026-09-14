using System.Linq;
#nullable enable
using Gens.Simulation.Identity;
using Gens.Simulation.Legal;

namespace Gens.Simulation.Romance;

/// <summary>Which <see cref="AffairRecord"/> a given §12 Adultery <see cref="LegalCase.CaseId"/> is
/// actually about — a new, sparse <see cref="State.WorldState"/> partition keyed by that already-issued
/// case ID, matching <see cref="PublicContracts.ContractFraudLegalLink"/>'s and <see
/// cref="Societates.ActioProSocioLink"/>'s identical "wrap the existing record in a parallel partition
/// rather than edit its schema" convention — this one is simpler than either, needing no new ID
/// issuer/counter of its own since both sides of the pair (<see cref="RuntimeId{LegalCase}"/>, <see
/// cref="RuntimeId{AffairRecord}"/>) are already-issued IDs by the time <see
/// cref="FileAdulteryCaseCommand"/> creates a link.</summary>
public sealed record AdulteryCaseLink(RuntimeId<LegalCase> CaseId, RuntimeId<AffairRecord> AffairId);
