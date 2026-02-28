using System;
using System.Text.RegularExpressions;
using UnityEngine;

public static class DefinitionIdLifecycle
{
#if UNITY_EDITOR
    private static readonly Regex IdFormatRegex = new("^[a-z0-9]+([._-][a-z0-9]+)*$", RegexOptions.Compiled);

    public static bool IsValidIdFormat(string id)
    {
        return !string.IsNullOrWhiteSpace(id) && IdFormatRegex.IsMatch(id);
    }

    public static void ValidateOnValidate(ScriptableObject asset, ref string id, ref bool isIdFinalized, ref string finalizedId)
    {
        if (asset == null)
            return;

        var candidate = id?.Trim();

        if (!isIdFinalized)
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                candidate = asset.name;
                id = candidate;
                Debug.LogWarning($"[Definition IDs] Auto-assigned initial ID '{id}' for '{asset.name}'. Future ID changes must use Tools/Data/Definition ID Migration.", asset);
            }

            if (string.IsNullOrWhiteSpace(candidate))
                return;

            finalizedId = candidate;
            isIdFinalized = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(candidate))
        {
            Debug.LogWarning($"[Definition IDs] '{asset.name}' has an empty finalized ID. Restore '{finalizedId}' or use Tools/Data/Definition ID Migration for an intentional rename.", asset);
            return;
        }

        if (!string.Equals(candidate, finalizedId, StringComparison.Ordinal))
        {
            Debug.LogWarning($"[Definition IDs] Ignoring implicit ID edit on '{asset.name}': '{candidate}' -> '{finalizedId}'. Use Tools/Data/Definition ID Migration for a deliberate rename.", asset);
            id = finalizedId;
        }
    }
#endif
}
