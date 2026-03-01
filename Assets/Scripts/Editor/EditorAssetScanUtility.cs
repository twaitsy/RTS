using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Shared editor-only asset scan helpers for ScriptableObject-heavy validation/migration tools.
/// </summary>
public static class EditorAssetScanUtility
{
    /// <summary>
    /// Enumerates ScriptableObject asset paths matching an optional type/folder filter.
    /// Scale/perf: one AssetDatabase.FindAssets query plus one GUID-to-path conversion per match.
    /// </summary>
    public static IEnumerable<string> EnumerateScriptableObjectAssetPaths(string typeName = null, IReadOnlyList<string> searchInFolders = null)
    {
        var filter = string.IsNullOrWhiteSpace(typeName)
            ? "t:ScriptableObject"
            : $"t:{typeName}";

        var guids = searchInFolders != null && searchInFolders.Count > 0
            ? AssetDatabase.FindAssets(filter, ToFolderArray(searchInFolders))
            : AssetDatabase.FindAssets(filter);

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (ShouldSkipAssetPath(path))
                continue;

            yield return path;
        }
    }

    /// <summary>
    /// Attempts to load a typed asset and returns false for null/empty paths or missing assets.
    /// Scale/perf: O(1) per call aside from AssetDatabase load cost.
    /// </summary>
    public static bool TryLoadAssetAtPath<T>(string path, out T asset) where T : UnityEngine.Object
    {
        asset = null;
        if (ShouldSkipAssetPath(path))
            return false;

        asset = AssetDatabase.LoadAssetAtPath<T>(path);
        return asset != null;
    }

    /// <summary>
    /// Loads a typed asset or returns null when skip rules apply.
    /// Scale/perf: identical to TryLoadAssetAtPath with a single AssetDatabase load attempt.
    /// </summary>
    public static T LoadAssetAtPathOrNull<T>(string path) where T : UnityEngine.Object
    {
        return TryLoadAssetAtPath(path, out T asset) ? asset : null;
    }

    /// <summary>
    /// Shared path skip rule for scan/load callers.
    /// Scale/perf: O(1) string check; intended for hot loop use.
    /// </summary>
    public static bool ShouldSkipAssetPath(string path)
    {
        return string.IsNullOrWhiteSpace(path);
    }

    private static string[] ToFolderArray(IReadOnlyList<string> folders)
    {
        var filtered = new List<string>(folders.Count);
        for (var i = 0; i < folders.Count; i++)
        {
            if (ShouldSkipAssetPath(folders[i]))
                continue;

            filtered.Add(folders[i]);
        }

        return filtered.ToArray();
    }
}
