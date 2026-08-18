namespace Gens.Presentation.Adapters;

/// <summary>
/// The sole translation point ADR 0013 permits between a simulation projection DTO (or a command
/// outcome) and a UI Toolkit-bound view model. Implementations never touch <c>WorldState</c> or any
/// other simulation-owned mutable object directly — they take exactly the immutable value a query or
/// command submission already produced and reshape it into display-ready fields, with no simulation
/// shape leaking into the returned view model.
/// </summary>
public interface IProjectionAdapter<in TProjection, out TViewModel>
{
    TViewModel Adapt(TProjection projection);
}
