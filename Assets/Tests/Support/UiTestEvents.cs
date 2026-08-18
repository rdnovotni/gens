#nullable enable

using UnityEngine.UIElements;

namespace Gens.Presentation.Tests.Support;

/// <summary>Simulated UI Toolkit input shared by every adapter/binding and PlayMode test that needs to
/// "click" a control.</summary>
public static class UiTestEvents
{
    /// <summary>Sends a pointer-down/pointer-up pair — what <c>Clickable</c> (the manipulator every UI
    /// Toolkit <see cref="Button"/>, and this codebase's clickable roster rows, are built on) actually
    /// listens for. Use this for any <see cref="Button"/> or other Clickable-backed control.</summary>
    public static void SimulateClick(VisualElement target)
    {
        using (var down = PointerDownEvent.GetPooled())
        {
            down.target = target;
            target.SendEvent(down);
        }

        using (var up = PointerUpEvent.GetPooled())
        {
            up.target = target;
            target.SendEvent(up);
        }
    }

    /// <summary>Sends a bare <see cref="ClickEvent"/> directly. <c>GensUIController</c> wires the
    /// ink-bar advance control and the confirmation dialog's wax seal with a raw
    /// <c>RegisterCallback&lt;ClickEvent&gt;</c> rather than a <see cref="Button"/>'s Clickable
    /// manipulator (<c>gens-core-design.md</c> §7.6's wax seal is a plain <see cref="VisualElement"/>,
    /// not a <see cref="Button"/>) — a pointer-down/pointer-up pair has no manipulator on those elements
    /// to translate it into a click, so this is the simulation that actually reaches them.</summary>
    public static void SimulateClickEvent(VisualElement target)
    {
        using var evt = ClickEvent.GetPooled();
        evt.target = target;
        target.SendEvent(evt);
    }
}
