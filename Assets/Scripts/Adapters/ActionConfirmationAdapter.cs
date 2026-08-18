#nullable enable

using System;
using System.Linq;
using Gens.Simulation.Actions;

namespace Gens.Presentation.Adapters;

/// <summary>Display-ready fields for the wax-seal/ordinary confirmation dialog (Phase 9 item 7):
/// whichever <see cref="ActionDefinition"/> the player is about to commit to, translated from its
/// content keys and non-mutating <see cref="ActionResultProjection"/> preview into dialog text, plus
/// the severity that decides which of the two dialog styles §7.6 calls for.</summary>
public readonly record struct ActionConfirmationViewModel(string TitleLabel, string BodyLabel, bool IsWaxSeal);

/// <summary>The only code allowed to turn an <see cref="ActionDefinition"/> and its <see
/// cref="ActionResultProjection"/> preview into confirmation-dialog text, mirroring <see
/// cref="CampaignClockAdapter"/>'s "pure translation, no UnityEngine" role. No localization system
/// exists yet (<c>NameKey</c>/<c>DescriptionKey</c> are bare content-key strings), so the title is
/// humanized directly from the key rather than looked up — §7.10's "reuse the action's own label
/// verbatim" still holds once localized text lands, since this only touches presentation, not the
/// key itself.</summary>
public static class ActionConfirmationAdapter
{
    public static ActionConfirmationViewModel Adapt(ActionDefinition definition, ActionResultProjection projection)
    {
        if (definition is null)
            throw new ArgumentNullException(nameof(definition));

        return new ActionConfirmationViewModel(
            TitleLabel: HumanizeNameKey(definition.NameKey),
            BodyLabel: projection.Summary,
            IsWaxSeal: definition.Confirmation == ActionConfirmationSeverity.WaxSeal);
    }

    /// <summary>Turns a dotted content key like <c>"actions.fund-festival.name"</c> into "Fund
    /// Festival" — takes the second-to-last dot segment (the action's own slug) rather than the last
    /// (a field discriminator like <c>"name"</c>), and title-cases each hyphen-separated word.</summary>
    private static string HumanizeNameKey(string nameKey)
    {
        var segments = nameKey.Split('.');
        var slug = segments.Length >= 2 ? segments[^2] : segments[^1];
        var words = slug.Split('-', StringSplitOptions.RemoveEmptyEntries)
            .Select(word => char.ToUpperInvariant(word[0]) + word[1..]);
        return string.Join(' ', words);
    }
}
