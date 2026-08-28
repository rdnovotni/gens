using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Funerary;

/// <summary>Emitted whenever a <see cref="FuneralRecord"/> moves from <see cref="FuneralStatus.Pending"/>
/// to <see cref="FuneralStatus.Held"/> — either a player's own <see cref="ChooseFuneralTierCommand"/>
/// (<see cref="CausationId"/> set to that command's ID) or <see cref="FuneralAutoResolutionSystem"/>'s
/// own unattended default (<see cref="CausationId"/> <c>null</c>, matching <see
/// cref="Succession.SuccessionHandoffSystem"/>'s identical system-originated-event convention).</summary>
public sealed record FuneralHeldEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<FuneralRecord> FuneralId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> DeceasedCharacterId,
    FuneralTier Tier,
    BurialMethod BurialMethodUsed,
    Money Cost,
    int MemoriaGained,
    bool ImaginesDisplayed,
    bool AutoResolved,
    string? CausationId) : IDomainEvent
{
    public string Type => "funerary.funeralHeld";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), DeceasedCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The one real mechanical heart of §2.2's <c>pompa funebris</c>, shared between <see
/// cref="ChooseFuneralTierCommand"/> (a player's own choice) and <see
/// cref="FuneralAutoResolutionSystem"/> (an unattended default after <see
/// cref="FuneraryCatalog.FuneralAutoResolutionAfterMonths"/>) so the actual cost/Memoria math lives in
/// exactly one place, matching <see cref="Succession.PlayerControlResolver"/>'s identical
/// "shared-between-command-and-system" convention.</summary>
internal static class FuneralResolution
{
    public static IReadOnlyList<IDomainEvent> Hold(
        WorldState state, FuneralRecord funeral, FuneralTier tier, GameDate date, bool autoResolved, string? causationId)
    {
        var cost = FuneraryCatalog.TreasuryCost(tier);
        var posted = LedgerService.Post(
            state, date, LedgerTransactionCategory.Gifts,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(funeral.HouseholdId), -cost),
                new LedgerPosting(FuneralSink, cost),
            },
            reference: $"funerary:funeral:{funeral.FuneralId.ToTaggedString()}");

        var ancestralBonus = FuneraryCatalog.AncestralAchievementBonus(state, funeral.HouseholdId);
        var memoriaGained = FuneraryCatalog.BaseMemoriaGain(tier) + (tier == FuneralTier.Grand ? ancestralBonus : 0);
        var imaginesDisplayed = FuneraryCatalog.ImaginesDisplayed(tier, ancestralBonus);

        MemoriaResolver.Apply(state, funeral.HouseholdId, memoriaGained);

        state.FuneralRecords.Remove(funeral.FuneralId);
        state.FuneralRecords.Add(
            funeral.FuneralId,
            funeral with
            {
                Status = FuneralStatus.Held,
                Tier = tier,
                BurialMethod = BurialMethod.Cremation,
                InterredAt = IntermentDestination.FamilyTomb,
                HeldDate = date,
                Cost = cost,
                MemoriaGained = memoriaGained,
                ImaginesDisplayed = imaginesDisplayed,
            });

        var held = new FuneralHeldEvent(
            state.EventIds.Issue(), date, funeral.FuneralId, funeral.HouseholdId, funeral.DeceasedCharacterId,
            tier, BurialMethod.Cremation, cost, memoriaGained, imaginesDisplayed, autoResolved, causationId);

        return new IDomainEvent[] { posted, held };
    }

    /// <summary>The named system sink a held funeral's cost drains into (§2.2's Treasury cost genuinely
    /// consumed by the funeral itself), matching <see cref="Policies.FundFestivalCommands"/>'s identical
    /// "one named account, not an untracked leak" discipline.</summary>
    private static readonly LedgerAccountKey FuneralSink = new(LedgerAccountKind.System, "funerary:funeral");
}
