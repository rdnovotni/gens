namespace Gens.Simulation.Characters;

/// <summary>The five Labor Skills (<c>gens-familia-design.md</c> §2.2), numeric 0–100, governing manual
/// duty assignment. Present on every Character, though mainly relevant to the enslaved/laboring.</summary>
public readonly record struct LaborSkills
{
    public LaborSkills(int fieldwork, int domesticService, int craft, int culinary, int medicine)
    {
        Fieldwork = StatRange.Validate(fieldwork, nameof(fieldwork));
        DomesticService = StatRange.Validate(domesticService, nameof(domesticService));
        Craft = StatRange.Validate(craft, nameof(craft));
        Culinary = StatRange.Validate(culinary, nameof(culinary));
        Medicine = StatRange.Validate(medicine, nameof(medicine));
    }

    public int Fieldwork { get; }
    public int DomesticService { get; }
    public int Craft { get; }
    public int Culinary { get; }
    public int Medicine { get; }
}
