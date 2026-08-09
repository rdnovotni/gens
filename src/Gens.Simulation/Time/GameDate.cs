namespace Gens.Simulation.Time;

public readonly record struct GameDate(int TotalMonths)
{
    public GameDate NextMonth() => new(checked(TotalMonths + 1));
}

