namespace Gens.Simulation.Characters;

/// <summary>The five lifecycle stages <c>gens-familia-design.md</c> §3 defines, each gating different
/// available actions and events. Never stored on <see cref="Character"/> directly — always derived
/// from <see cref="Character.BirthDate"/> via <see cref="Character.GetLifecycleStage"/>, matching the
/// project's existing convention that calendar-derived values are never stored redundantly (<see
/// cref="Time.GameDate"/>'s own doc comment).</summary>
public enum LifecycleStage
{
    /// <summary>Age 0–3. No stats beyond Health; primary gameplay surface is disease/mortality risk.</summary>
    Infant,

    /// <summary>Age 4–12. Formative traits begin forming; no labor, no marriage.</summary>
    Child,

    /// <summary>Age 13–17. Betrothal/marriage negotiations and Labor Skill duty assignment open up.</summary>
    Adolescent,

    /// <summary>Age 18–59. Full participation: duties, Court Positions, military service, political office.</summary>
    Adult,

    /// <summary>Age 60+. Core Attributes and Health gradually decline; succession pressure and death risk rise.</summary>
    Elderly,
}
