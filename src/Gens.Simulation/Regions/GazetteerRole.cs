namespace Gens.Simulation.Regions;

/// <summary>What a <see cref="GazetteerLocationDefinition"/> is *for* — which of a region's
/// cross-referenced systems can transact with that place (<c>gens-starting-regions-design.md</c>
/// §8.3). A single entry can carry more than one role (a Major Port that's also the Provincial Seat is
/// realistic and common, per §8.3's own closing note).</summary>
public enum GazetteerRole
{
    /// <summary>Rome only (§8.3) — the single unique seat of the cursus honorum, the Senate, and the
    /// widest range of political/legal/spectacle actions in the game. Exactly one gazetteer entry in
    /// the entire roster may ever carry this role; enforced by <see cref="RegionProfileCatalog"/>, not
    /// by any single region's own definition.</summary>
    Capital,

    /// <summary>The region's own administrative capital (§8.3) — cursus honorum, provincial
    /// governance, and higher magistrate rulings anchor here. Usually only one per region.</summary>
    ProvincialSeat,

    /// <summary>Trade volume, import/export flow, and the embarkation point for travel leaving the
    /// region entirely (§8.3, §7).</summary>
    MajorPort,

    /// <summary>Recruitment, muster, and (for a frontier region) a standing garrison musters out of
    /// here (§8.3).</summary>
    LegionaryBase,

    /// <summary>Pilgrimage, favor-seeking, and Haruspex-consultation actions anchor here (§8.3).</summary>
    Sanctuary,

    /// <summary>A concrete venue for ordinary market dynamics and lower-stakes local Clientela
    /// dealings, distinct from a Major Port's larger-scale trade (§8.3).</summary>
    MarketHub,

    /// <summary>A neighboring people's territory and treaty-negotiation site; also a natural
    /// espionage/raid-target flavor location (§8.3).</summary>
    FrontierOutpost,
}
