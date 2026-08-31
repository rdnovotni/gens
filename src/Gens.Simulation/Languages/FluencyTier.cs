namespace Gens.Simulation.Languages;

/// <summary>§4's four-step Language Proficiency scale.</summary>
public enum FluencyTier
{
    /// <summary>No meaningful comprehension.</summary>
    None,

    /// <summary>Enough for simple trade and basic exchange (§4).</summary>
    Basic,

    /// <summary>Genuine, comfortable daily communication, without formal/technical polish (§4) — the
    /// floor §6's hard gate and §7's informal Interpres both check for.</summary>
    Conversational,

    /// <summary>Full command, including the formal register diplomacy or a legal document requires (§4).</summary>
    FluentNative,
}
