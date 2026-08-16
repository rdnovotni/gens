namespace Gens.Simulation.Ledger;

/// <summary>
/// One household, actor, settlement-treasury, or system account's running <see cref="Money"/> balance
/// (Phase 8 items 1-2). Immutable, like <see cref="Characters.Character"/> — <see cref="LedgerService"/>
/// updates a balance by removing and re-adding the record in <see cref="State.WorldState.LedgerAccounts"/>,
/// the same remove-then-add pattern every other immutable-record partition in this codebase already
/// uses (e.g. <see cref="Characters.CharacterLifecycleSystem"/>). A balance may be negative — the
/// Treasury "can run negative" (<c>gens-economy-finance-design.md</c> §2) — so no non-negative
/// invariant is enforced here.
/// </summary>
public sealed record LedgerAccount(LedgerAccountKey Key, Money Balance);
