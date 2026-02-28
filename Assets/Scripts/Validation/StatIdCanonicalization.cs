using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class StatIdCanonicalization
{
    private static readonly Regex CanonicalIdRegex = new(
        @"^[a-z][a-z0-9]*(?:\.[a-z][a-zA-Z0-9]*)+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly HashSet<string> CanonicalCatalog = new(CanonicalStatIds.Catalog, System.StringComparer.Ordinal);

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

        canonicalId = statId;
        return false;
    }

    public static bool ExistsInCanonicalCatalog(string statId)
    {
        return !string.IsNullOrWhiteSpace(statId) && CanonicalCatalog.Contains(statId);
    }
}
