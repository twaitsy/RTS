#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class StatIdValidationMenu
{
    private static readonly Regex ArrayElementRegex = new(@"^(.*)\.Array\.data\[(\d+)\]$", RegexOptions.Compiled);

    [MenuItem("Tools/Validation/Migrate Legacy Stat IDs")]
    public static void MigrateLegacyStatIds()
    {
        int updatedAssets = 0;
        foreach (var path in EnumerateScriptableObjectAssetPaths())
        {
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (asset == null)
                continue;

            var serializedObject = new SerializedObject(asset);
            if (!TryMigrateSerializedStatIds(serializedObject, path, out bool changed, out _))
                continue;

            if (!changed)
                continue;

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
            updatedAssets++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[Validation] Legacy stat ID migration complete. Updated {updatedAssets} asset(s).");
    }

    [MenuItem("Tools/Validation/Validate Canonical Stat IDs")]
    public static void ValidateCanonicalStatIdsMenu()
    {
        var errors = ValidateCanonicalStatIdsInternal();
        if (errors.Count == 0)
        {
            Debug.Log("[Validation] Canonical stat ID validation passed.");
            EditorUtility.DisplayDialog("Validation", "Canonical stat ID validation passed.", "OK");
            return;
        }

        foreach (var error in errors)
            Debug.LogError(error);

        Debug.LogError($"[Validation] Canonical stat ID validation failed with {errors.Count} issue(s).");
        EditorUtility.DisplayDialog("Validation", $"Validation found {errors.Count} issue(s). Check Console for details.", "OK");
    }

    // CI/batch-mode entry point.
    public static void ValidateCanonicalStatIdsForCI()
    {
        var errors = ValidateCanonicalStatIdsInternal();
        if (errors.Count == 0)
        {
            Debug.Log("[Validation] Canonical stat ID CI validation passed.");
            return;
        }

        foreach (var error in errors)
            Debug.LogError(error);

        throw new Exception($"Canonical stat ID validation failed with {errors.Count} issue(s).");
    }

    private static List<string> ValidateCanonicalStatIdsInternal()
    {
        var errors = new List<string>();
        var statDefinitions = LoadStatDefinitionIds();

        foreach (var path in EnumerateScriptableObjectAssetPaths())
        {
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (asset == null)
                continue;

            if (asset is StatDefinition statDefinition)
                ValidateStatDefinition(statDefinition, path, errors);

            ValidateSerializedStatIdFields(asset, path, statDefinitions, errors);
        }

        return errors;
    }

    private static HashSet<string> LoadStatDefinitionIds()
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var guid in AssetDatabase.FindAssets("t:StatDefinition"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<StatDefinition>(path);
            if (asset != null && !string.IsNullOrWhiteSpace(asset.Id))
                ids.Add(asset.Id);
        }

        return ids;
    }

    private static void ValidateStatDefinition(StatDefinition definition, string path, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(definition.Id))
        {
            errors.Add($"[Validation] StatDefinition '{path}' has an empty id.");
            return;
        }

        if (StatIdCanonicalization.TryGetCanonical(definition.Id, out var canonical) && !string.Equals(canonical, definition.Id, StringComparison.Ordinal))
        {
            errors.Add($"[Validation] StatDefinition '{path}' uses legacy id '{definition.Id}'. Expected '{canonical}'.");
        }

        if (!StatIdCanonicalization.IsCanonicalFormat(definition.Id))
            errors.Add($"[Validation] StatDefinition '{path}' has non-canonical id format '{definition.Id}'.");

        if (!StatIdCanonicalization.ExistsInCanonicalCatalog(definition.Id))
            errors.Add($"[Validation] StatDefinition '{path}' id '{definition.Id}' is not present in CanonicalStatIds.Catalog.");
    }

    private static void ValidateSerializedStatIdFields(ScriptableObject asset, string path, HashSet<string> statDefinitionIds, List<string> errors)
    {
        var serializedObject = new SerializedObject(asset);
        var iterator = serializedObject.GetIterator();

        if (!iterator.NextVisible(true))
            return;

        do
        {
            if (iterator.propertyType != SerializedPropertyType.String)
                continue;

            if (iterator.name != "statId" && iterator.name != "targetStatId")
                continue;

            var value = iterator.stringValue;
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add($"[Validation] Asset '{path}' has empty {iterator.name} at '{iterator.propertyPath}'.");
                continue;
            }

            if (StatIdCanonicalization.TryGetCanonical(value, out var canonical) && !string.Equals(value, canonical, StringComparison.Ordinal))
            {
                errors.Add($"[Validation] Asset '{path}' uses legacy {iterator.name} '{value}' at '{iterator.propertyPath}'. Expected '{canonical}'.");
            }

            if (!StatIdCanonicalization.IsCanonicalFormat(value))
                errors.Add($"[Validation] Asset '{path}' has non-canonical {iterator.name} '{value}' at '{iterator.propertyPath}'.");

            bool resolvable = statDefinitionIds.Contains(value) || StatIdCanonicalization.ExistsInCanonicalCatalog(value);
            if (!resolvable)
                errors.Add($"[Validation] Asset '{path}' references unresolved {iterator.name} '{value}' at '{iterator.propertyPath}'.");

        } while (iterator.NextVisible(false));
    }

    private static bool TryMigrateSerializedStatIds(SerializedObject serializedObject, string path, out bool changed, out int removed)
    {
        changed = false;
        removed = 0;

        var removals = new List<(string arrayPath, int index)>();
        var iterator = serializedObject.GetIterator();
        if (!iterator.NextVisible(true))
            return false;

        do
        {
            if (iterator.propertyType != SerializedPropertyType.String)
                continue;

            if (iterator.name != "statId" && iterator.name != "targetStatId")
                continue;

            if (string.IsNullOrWhiteSpace(iterator.stringValue))
            {
                if (TryGetArrayElementInfo(iterator.propertyPath, out var arrayPath, out var index))
                    removals.Add((arrayPath, index));

                Debug.LogWarning($"[Validation] Removing empty {iterator.name} from '{path}' at '{iterator.propertyPath}'.");
                continue;
            }

            if (!StatIdCanonicalization.TryGetCanonical(iterator.stringValue, out var canonicalId))
                continue;

            if (string.Equals(iterator.stringValue, canonicalId, StringComparison.Ordinal))
                continue;

            iterator.stringValue = canonicalId;
            changed = true;
        } while (iterator.NextVisible(false));

        if (removals.Count > 0)
        {
            removals.Sort((a, b) =>
            {
                int compare = string.CompareOrdinal(a.arrayPath, b.arrayPath);
                return compare != 0 ? compare : b.index.CompareTo(a.index);
            });

            foreach (var (arrayPath, index) in removals)
            {
                var array = serializedObject.FindProperty(arrayPath);
                if (array == null || !array.isArray || index < 0 || index >= array.arraySize)
                    continue;

                array.DeleteArrayElementAtIndex(index);
                changed = true;
                removed++;
            }
        }

        return true;
    }

    private static bool TryGetArrayElementInfo(string propertyPath, out string arrayPath, out int index)
    {
        arrayPath = null;
        index = -1;

        int separator = propertyPath.LastIndexOf('.');
        if (separator <= 0)
            return false;

        var parentPath = propertyPath.Substring(0, separator);
        var match = ArrayElementRegex.Match(parentPath);
        if (!match.Success)
            return false;

        arrayPath = match.Groups[1].Value;
        return int.TryParse(match.Groups[2].Value, out index);
    }

    private static IEnumerable<string> EnumerateScriptableObjectAssetPaths()
    {
        foreach (var guid in AssetDatabase.FindAssets("t:ScriptableObject"))
            yield return AssetDatabase.GUIDToAssetPath(guid);
    }
}
#endif
