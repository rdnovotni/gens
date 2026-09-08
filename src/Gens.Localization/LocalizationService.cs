using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Gens.Localization;

public sealed class LocalizationService
{
    private static readonly Regex Placeholder = new("\\{([A-Za-z][A-Za-z0-9_]*)\\}", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private readonly Dictionary<string, IReadOnlyDictionary<string, string>> catalogs = new(StringComparer.OrdinalIgnoreCase);
    public LocalizationService(string fallbackLocale = "en", bool developmentMode = false) { FallbackLocale = fallbackLocale; CurrentLocale = fallbackLocale; DevelopmentMode = developmentMode; }
    public string FallbackLocale { get; }
    public string CurrentLocale { get; private set; }
    public bool DevelopmentMode { get; }
    public event Action<string>? LocaleChanged;
    public event Action<string, string>? MissingKey;
    public IReadOnlyCollection<string> AvailableLocales => catalogs.Keys;

    public void AddJson(string locale, Stream json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale); ArgumentNullException.ThrowIfNull(json);
        catalogs[locale] = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? throw new InvalidDataException($"Localization catalog '{locale}' is empty.");
    }
    public void SetLocale(string locale)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        if (!locale.Equals("qps-ploc", StringComparison.OrdinalIgnoreCase) && !catalogs.ContainsKey(locale)) throw new KeyNotFoundException($"Locale '{locale}' is not loaded.");
        if (CurrentLocale.Equals(locale, StringComparison.OrdinalIgnoreCase)) return; CurrentLocale = locale; LocaleChanged?.Invoke(locale);
    }
    public string Get(string key, IReadOnlyDictionary<string, object?>? arguments = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        bool pseudo = CurrentLocale.Equals("qps-ploc", StringComparison.OrdinalIgnoreCase);
        string lookupLocale = pseudo ? FallbackLocale : CurrentLocale;
        if (!TryLookup(lookupLocale, key, out string? value) && !TryLookup(FallbackLocale, key, out value))
        { MissingKey?.Invoke(CurrentLocale, key); return DevelopmentMode ? $"⟦missing:{key}⟧" : key; }
        string formatted = Format(key, value!, arguments);
        return pseudo ? PseudoLocalizer.Expand(formatted) : formatted;
    }
    private bool TryLookup(string locale, string key, out string? value) { value = null; return catalogs.TryGetValue(locale, out IReadOnlyDictionary<string, string>? catalog) && catalog.TryGetValue(key, out value); }
    private static string Format(string key, string value, IReadOnlyDictionary<string, object?>? arguments)
    {
        MatchCollection required = Placeholder.Matches(value);
        if (required.Count == 0) return value;
        if (arguments is null) throw new FormatException($"Localization key '{key}' requires parameters: {string.Join(", ", required.Select(static match => match.Groups[1].Value).Distinct(StringComparer.Ordinal))}.");
        return Placeholder.Replace(value, match => arguments.TryGetValue(match.Groups[1].Value, out object? argument) ? Convert.ToString(argument, CultureInfo.CurrentCulture) ?? string.Empty : throw new FormatException($"Localization key '{key}' is missing parameter '{match.Groups[1].Value}'."));
    }
}

public static class PseudoLocalizer
{
    private static readonly Dictionary<char, char> Accents = new() { ['a'] = 'à', ['A'] = 'À', ['e'] = 'ë', ['E'] = 'Ë', ['i'] = 'ï', ['I'] = 'Ï', ['o'] = 'ô', ['O'] = 'Ô', ['u'] = 'ü', ['U'] = 'Ü', ['c'] = 'ç', ['C'] = 'Ç', ['n'] = 'ñ', ['N'] = 'Ń', ['w'] = 'ŵ', ['W'] = 'Ŵ', ['g'] = 'ğ', ['G'] = 'Ġ', ['y'] = 'ÿ', ['Y'] = 'Ÿ' };
    public static string Expand(string text)
    {
        var result = new StringBuilder(text.Length * 2).Append('[');
        foreach (char character in text) result.Append(Accents.GetValueOrDefault(character, character));
        result.Append('~', Math.Max(3, (int)Math.Ceiling(text.Length * .4))).Append(']'); return result.ToString();
    }
}
