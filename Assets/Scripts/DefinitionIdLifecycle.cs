using System;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class DefinitionIdLifecycle
{
#if UNITY_EDITOR
    private static readonly Regex IdFormatRegex = new("^[a-z0-9]+(?:\\.[a-zA-Z0-9]+)*$", RegexOptions.Compiled);

    public static bool IsValidIdFormat(string id)
    {
        return !string.IsNullOrWhiteSpace(id) && IdFormatRegex.IsMatch(id);
    }

    public static string NormalizeId(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        var input = raw.Trim();
        var builder = new StringBuilder(input.Length);
        var segment = new StringBuilder();
        var hasSegment = false;

        void FlushSegment(bool isFirstSegment)
        {
            if (segment.Length == 0)
                return;

            if (builder.Length > 0)
                builder.Append('.');

            if (isFirstSegment)
            {
                builder.Append(segment.ToString().ToLowerInvariant());
            }
            else
            {
                builder.Append(char.ToLowerInvariant(segment[0]));
                if (segment.Length > 1)
                    builder.Append(segment.ToString(1, segment.Length - 1));
            }

            segment.Clear();
        }

        foreach (char c in input)
        {
            if (char.IsLetterOrDigit(c))
            {
                segment.Append(c);
                continue;
            }

            if (c is '.' or '_' or '-' || char.IsWhiteSpace(c))
            {
                FlushSegment(!hasSegment);
                hasSegment |= builder.Length > 0;
            }
        }

        FlushSegment(!hasSegment);
        return builder.ToString();
    }

    public static void ValidateOnValidate(ScriptableObject asset, ref string id, ref bool isIdFinalized, ref string finalizedId)
    {
        if (asset == null)
            return;

        var rawCandidate = id;
        var candidate = NormalizeId(rawCandidate);

        if (!isIdFinalized)
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                candidate = NormalizeId(asset.name);
                id = candidate;
                Debug.LogWarning($"[Definition IDs] Auto-assigned initial ID '{id}' for '{asset.name}'. Future ID changes must use Tools/Data/Definition ID Migration.", asset);
            }

            if (string.IsNullOrWhiteSpace(candidate))
                return;

            if (!string.Equals(rawCandidate, candidate, StringComparison.Ordinal))
            {
                Debug.LogWarning($"[Definition IDs] Normalized non-canonical ID on '{asset.name}' from '{rawCandidate}' to '{candidate}'.", asset);
                id = candidate;
            }

            if (!IsValidIdFormat(candidate))
            {
                Debug.LogWarning($"[Definition IDs] '{asset.name}' produced invalid ID '{candidate}' after normalization. Allowed pattern: lowercase domain + dot-separated alphanumeric segments (example: 'core.maxHealth').", asset);
                return;
            }

            if (TryFindIdCollision(asset, candidate, out var conflictPath))
            {
                Debug.LogWarning($"[Definition IDs] Cannot finalize '{asset.name}' with normalized ID '{candidate}' because it collides with '{conflictPath}'. Rename one asset or run Tools/Data/Definition ID Migration.", asset);
                return;
            }

            finalizedId = candidate;
            id = candidate;
            isIdFinalized = true;
            return;
        }

        if (!IsValidIdFormat(finalizedId))
        {
            var repairedFinalized = NormalizeId(finalizedId);
            if (IsValidIdFormat(repairedFinalized))
            {
                Debug.LogWarning($"[Definition IDs] Repaired legacy finalized ID on '{asset.name}' from '{finalizedId}' to '{repairedFinalized}'.", asset);
                finalizedId = repairedFinalized;
                id = repairedFinalized;
            }
            else
            {
                Debug.LogWarning($"[Definition IDs] '{asset.name}' has invalid finalized ID '{finalizedId}'. Use Tools/Data/Definition ID Migration to assign a valid ID.", asset);
                return;
            }
        }

        if (string.IsNullOrWhiteSpace(candidate))
        {
            Debug.LogWarning($"[Definition IDs] '{asset.name}' has an empty finalized ID. Restore '{finalizedId}' or use Tools/Data/Definition ID Migration for an intentional rename.", asset);
            return;
        }

        if (!string.Equals(rawCandidate, candidate, StringComparison.Ordinal))
        {
            Debug.LogWarning($"[Definition IDs] Normalized edited ID on '{asset.name}' from '{rawCandidate}' to '{candidate}', but finalized IDs can only be changed via Tools/Data/Definition ID Migration.", asset);
            id = candidate;
        }

        if (!string.Equals(candidate, finalizedId, StringComparison.Ordinal))
        {
            Debug.LogWarning($"[Definition IDs] Ignoring implicit ID edit on '{asset.name}': '{candidate}' -> '{finalizedId}'. Use Tools/Data/Definition ID Migration for a deliberate rename.", asset);
            id = finalizedId;
        }
    }

    private static bool TryFindIdCollision(ScriptableObject currentAsset, string candidate, out string conflictPath)
    {
        conflictPath = null;
        var currentPath = AssetDatabase.GetAssetPath(currentAsset);

        foreach (var guid in AssetDatabase.FindAssets("t:ScriptableObject"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.Equals(path, currentPath, StringComparison.Ordinal))
                continue;

            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (asset == null)
                continue;

            var serializedObject = new SerializedObject(asset);
            var idProperty = serializedObject.FindProperty("id");
            if (idProperty == null || idProperty.propertyType != SerializedPropertyType.String)
                continue;

            var existing = NormalizeId(idProperty.stringValue);
            if (!string.Equals(existing, candidate, StringComparison.Ordinal))
                continue;

            conflictPath = path;
            return true;
        }

        return false;
    }
#endif
}
