using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public static class StatIdCanonicalization
{
    private static readonly Regex CanonicalIdRegex = new(
        @"^[a-z][a-z0-9]*(?:\.[a-z][a-zA-Z0-9]*)+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly HashSet<string> CanonicalCatalog = new(CanonicalStatIds.Catalog, System.StringComparer.Ordinal);
    private static readonly Dictionary<string, string> CanonicalBySimplifiedId = BuildCanonicalBySimplifiedId();

    public static bool IsCanonicalFormat(string statId)
    {
        return !string.IsNullOrWhiteSpace(statId) && CanonicalIdRegex.IsMatch(statId);
    }

    public static bool TryGetCanonical(string statId, out string canonicalId)
    {
        if (string.IsNullOrWhiteSpace(statId))
        {
            canonicalId = statId;
            return false;
        }

        if (StatIdCompatibilityMap.LegacyToCanonical.TryGetValue(statId, out canonicalId))
            return true;

        var normalized = SimplifyStatId(statId);
        if (!string.IsNullOrEmpty(normalized) && CanonicalBySimplifiedId.TryGetValue(normalized, out canonicalId))
            return true;

        canonicalId = statId;
        return false;
    }

    public static bool ExistsInCanonicalCatalog(string statId)
    {
        return !string.IsNullOrWhiteSpace(statId) && CanonicalCatalog.Contains(statId);
    }

    private static Dictionary<string, string> BuildCanonicalBySimplifiedId()
    {
        var map = new Dictionary<string, string>(System.StringComparer.Ordinal);
        foreach (var id in CanonicalStatIds.Catalog)
        {
            var simplified = SimplifyStatId(id);
            if (!string.IsNullOrEmpty(simplified))
                map[simplified] = id;
        }

        return map;
    }

    private static string SimplifyStatId(string statId)
    {
        if (string.IsNullOrWhiteSpace(statId))
            return string.Empty;

        var builder = new StringBuilder(statId.Length);
        foreach (var c in statId)
        {
            if (char.IsLetterOrDigit(c))
            {
                builder.Append(char.ToLowerInvariant(c));
                continue;
            }

            if (c is '.' or '_' or '-' || char.IsWhiteSpace(c))
                builder.Append('.');
        }

        return builder.ToString().Trim('.');
    }
}
