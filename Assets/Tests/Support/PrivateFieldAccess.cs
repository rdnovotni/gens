#nullable enable

using System;
using System.Reflection;

namespace Gens.Presentation.Tests.Support;

/// <summary>Sets a private/[SerializeField]-backed instance field by name — Unity's own Inspector is
/// the only other way to fill in <c>CampaignShellBehaviour</c>/<c>GensUIController</c>'s serialized
/// fields, which is unavailable from a scriptable test. Test-only reflection, mirroring how a scene
/// author would otherwise drag references in the Inspector.</summary>
public static class PrivateFieldAccess
{
    public static void Set<T>(object target, string fieldName, T value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"No instance field '{fieldName}' found on '{target.GetType()}'.");
        field.SetValue(target, value);
    }
}
