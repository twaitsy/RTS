using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public sealed class DefinitionIdMigrationTool : EditorWindow
{
    private ScriptableObject targetDefinition;
    private string newId;
    private bool dryRun = true;
    private Vector2 previewScroll;
    private readonly List<string> plannedChangeLog = new();

    [MenuItem("Tools/Data/Definition ID Migration")]
    public static void Open()
    {
        var window = GetWindow<DefinitionIdMigrationTool>("Definition ID Migration");
        window.minSize = new Vector2(640f, 360f);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("Safely migrate a definition ID. This tool now updates only the target's own ID/finalizedId and known definition reference fields from schema metadata + explicit custom reference paths.", MessageType.Info);

        targetDefinition = (ScriptableObject)EditorGUILayout.ObjectField("Target Definition", targetDefinition, typeof(ScriptableObject), false);
        dryRun = EditorGUILayout.ToggleLeft("Dry Run (preview planned edits without saving)", dryRun);

        using (new EditorGUI.DisabledScope(targetDefinition == null || !TryGetIdProperty(targetDefinition, out _, out _)))
        {
            if (targetDefinition != null && TryGetIdProperty(targetDefinition, out _, out var idProperty))
            {
                EditorGUILayout.LabelField("Current ID", idProperty.stringValue);
                if (string.IsNullOrWhiteSpace(newId))
                    newId = idProperty.stringValue;
            }

            newId = EditorGUILayout.TextField("New ID", newId ?? string.Empty);

            if (GUILayout.Button(dryRun ? "Preview Migration" : "Validate + Apply Migration"))
                ValidateAndMigrate();
        }

        if (plannedChangeLog.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Planned / Last Migration Changes", EditorStyles.boldLabel);
            using var scroll = new EditorGUILayout.ScrollViewScope(previewScroll, GUILayout.Height(180f));
            previewScroll = scroll.scrollPosition;
            foreach (var line in plannedChangeLog)
                EditorGUILayout.LabelField(line, EditorStyles.wordWrappedMiniLabel);
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Bulk normalization updates only each definition's own id/finalizedId and skips collisions/invalid outputs.", MessageType.None);

        if (GUILayout.Button("Normalize Existing IDs (Bulk)"))
            NormalizeExistingIdsInBulk();
    }

    private void ValidateAndMigrate()
    {
        plannedChangeLog.Clear();

        if (targetDefinition == null || !TryGetIdProperty(targetDefinition, out var targetSerializedObject, out var targetIdProperty))
        {
            EditorUtility.DisplayDialog("Definition ID Migration", "Pick a ScriptableObject definition with a serialized 'id' field.", "OK");
            return;
        }

        var oldId = targetIdProperty.stringValue;
        if (string.IsNullOrWhiteSpace(oldId))
        {
            EditorUtility.DisplayDialog("Definition ID Migration", "Target definition has an empty ID. Set an initial ID first.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(newId))
        {
            EditorUtility.DisplayDialog("Definition ID Migration", "New ID cannot be empty.", "OK");
            return;
        }

        var rawNewId = newId;
        newId = DefinitionIdLifecycle.NormalizeId(newId);

        if (!string.Equals(rawNewId, newId, StringComparison.Ordinal))
            Debug.LogWarning($"[Definition IDs] Normalized migration ID from '{rawNewId}' to '{newId}'.");

        if (!DefinitionIdLifecycle.IsValidIdFormat(newId))
        {
            EditorUtility.DisplayDialog("Definition ID Migration", "Invalid ID format after normalization. Allowed pattern: lowercase domain + dot-separated alphanumeric segments (example: 'core.maxHealth').", "OK");
            return;
        }

        if (string.Equals(oldId, newId, StringComparison.Ordinal))
        {
            EditorUtility.DisplayDialog("Definition ID Migration", "Old and new IDs match. Nothing to migrate.", "OK");
            return;
        }

        if (HasDuplicateId(targetDefinition, newId))
        {
            EditorUtility.DisplayDialog("Definition ID Migration", $"Duplicate ID detected: '{newId}'. Migration canceled.", "OK");
            return;
        }

        var targetPath = AssetDatabase.GetAssetPath(targetDefinition);
        var referenceFieldMap = BuildReferenceFieldMap();
        var operations = CollectMigrationOperations(oldId, newId, targetPath, referenceFieldMap);

        if (operations.Count == 0)
        {
            plannedChangeLog.Add("No matching safe reference fields found for migration.");
            EditorUtility.DisplayDialog("Definition ID Migration", "No matching safe reference fields found.", "OK");
            return;
        }

        foreach (var op in operations.Take(200))
            plannedChangeLog.Add($"- {op.AssetPath} :: {op.PropertyPath} ({op.OldValue} -> {op.NewValue})");
        if (operations.Count > 200)
            plannedChangeLog.Add($"... plus {operations.Count - 200} additional planned changes.");

        var targetEdits = operations.Count(op => string.Equals(op.AssetPath, targetPath, StringComparison.Ordinal));
        var referenceEdits = operations.Count - targetEdits;
        var summary = $"Planned changes: {operations.Count} field edit(s) across {operations.Select(op => op.AssetPath).Distinct(StringComparer.Ordinal).Count()} asset(s).\n" +
                      $"Target fields: {targetEdits}\nReference fields: {referenceEdits}";

        if (dryRun)
        {
            Debug.Log($"[Definition IDs] Dry-run preview for '{oldId}' -> '{newId}'.\n{summary}\n{string.Join("\n", plannedChangeLog.Take(100))}");
            EditorUtility.DisplayDialog("Definition ID Migration (Dry Run)", summary + "\n\nReview the preview list in the window/Console. No assets were modified.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Confirm Migration", summary + "\n\nApply migration now?", "Migrate", "Cancel"))
            return;

        var changedAssets = 0;
        var touchedDefinitions = 0;

        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var group in operations.GroupBy(op => op.AssetPath, StringComparer.Ordinal))
            {
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(group.Key);
                if (asset == null)
                    continue;

                var serializedObject = new SerializedObject(asset);
                var appliedAny = false;

                foreach (var op in group)
                {
                    var property = serializedObject.FindProperty(op.PropertyPath);
                    if (property == null || property.propertyType != SerializedPropertyType.String)
                        continue;
                    if (!string.Equals(property.stringValue, oldId, StringComparison.Ordinal))
                        continue;

                    property.stringValue = newId;
                    appliedAny = true;
                }

                if (!appliedAny)
                    continue;

                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(asset);
                touchedDefinitions++;

                if (!string.Equals(group.Key, targetPath, StringComparison.Ordinal))
                    changedAssets++;
            }

            // Ensure lifecycle metadata is synchronized for the renamed definition.
            targetSerializedObject.Update();
            targetIdProperty.stringValue = newId;
            SetIfPresent(targetSerializedObject.FindProperty("isIdFinalized"), true);
            SetIfPresent(targetSerializedObject.FindProperty("finalizedId"), newId);
            targetSerializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(targetDefinition);

            AssetDatabase.SaveAssets();
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }

        Debug.Log($"[Definition IDs] Migration complete: '{oldId}' -> '{newId}'. Updated {changedAssets} referencing asset(s), touched {touchedDefinitions} definition asset(s).");
        EditorUtility.DisplayDialog("Definition ID Migration", $"Migration complete. Updated {changedAssets} referencing asset(s).", "OK");
    }

    private static List<MigrationOperation> CollectMigrationOperations(
        string oldId,
        string newId,
        string targetAssetPath,
        IReadOnlyDictionary<Type, HashSet<string>> referenceFieldMap)
    {
        var operations = new List<MigrationOperation>();

        foreach (var guid in AssetDatabase.FindAssets("t:ScriptableObject"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (asset == null)
                continue;

            var serializedObject = new SerializedObject(asset);
            var isTargetAsset = string.Equals(path, targetAssetPath, StringComparison.Ordinal);
            ReplaceMatchingStringIds(serializedObject, oldId, newId, isTargetAsset, referenceFieldMap, path, operations);
        }

        return operations;
    }

    private static bool ReplaceMatchingStringIds(
        SerializedObject serializedObject,
        string oldId,
        string newId,
        bool isTargetAsset,
        IReadOnlyDictionary<Type, HashSet<string>> referenceFieldMap,
        string assetPath,
        List<MigrationOperation> operations)
    {
        var changed = false;
        var iterator = serializedObject.GetIterator();
        var enterChildren = true;
        referenceFieldMap.TryGetValue(serializedObject.targetObject.GetType(), out var knownReferencePaths);

        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = true;

            if (iterator.propertyType != SerializedPropertyType.String)
                continue;

            if (!string.Equals(iterator.stringValue, oldId, StringComparison.Ordinal))
                continue;

            if (ShouldSkipProperty(iterator.propertyPath))
                continue;

            if (!IsAllowedPropertyPath(iterator.propertyPath, isTargetAsset, knownReferencePaths))
                continue;

            operations.Add(new MigrationOperation(assetPath, iterator.propertyPath, oldId, newId));
            changed = true;
        }

        return changed;
    }

    private static bool ShouldSkipProperty(string propertyPath)
    {
        return propertyPath.StartsWith("m_", StringComparison.Ordinal)
               || propertyPath.StartsWith("metadata", StringComparison.Ordinal)
               || propertyPath.Contains("managedReference", StringComparison.Ordinal);
    }

    private static bool IsAllowedPropertyPath(string propertyPath, bool isTargetAsset, HashSet<string> knownReferencePaths)
    {
        var normalized = NormalizeArrayPath(propertyPath);

        if (isTargetAsset && (string.Equals(normalized, "id", StringComparison.Ordinal) || string.Equals(normalized, "finalizedId", StringComparison.Ordinal)))
            return true;

        return knownReferencePaths != null && knownReferencePaths.Contains(normalized);
    }

    private static IReadOnlyDictionary<Type, HashSet<string>> BuildReferenceFieldMap()
    {
        var map = new Dictionary<Type, HashSet<string>>
        {
            [typeof(UnitDefinition)] = BuildSchemaReferencePatterns(typeof(UnitDefinition), UnitRegistry.GetReferenceFieldPaths()),
            [typeof(BuildingDefinition)] = BuildSchemaReferencePatterns(typeof(BuildingDefinition), BuildingRegistry.GetReferenceFieldPaths()),
            [typeof(RecipeDefinition)] = BuildSchemaReferencePatterns(typeof(RecipeDefinition), RecipeRegistry.GetReferenceFieldPaths()),
            [typeof(TechDefinition)] = BuildSchemaReferencePatterns(typeof(TechDefinition), TechRegistry.GetReferenceFieldPaths()),

            // Non-schema custom reference extraction.
            [typeof(ProductionDefinition)] = new HashSet<string>(StringComparer.Ordinal)
            {
                "buildingId",
                "unitId",
                "costs.Array.data[].resourceId",
                "stats.entries.Array.data[].statId"
            },
            [typeof(ResourceNodeDefinition)] = new HashSet<string>(StringComparer.Ordinal)
            {
                "resourceId"
            }
        };

        return map;
    }

    private static HashSet<string> BuildSchemaReferencePatterns(Type definitionType, IReadOnlyCollection<string> schemaFieldPaths)
    {
        var patterns = new HashSet<string>(StringComparer.Ordinal);
        if (schemaFieldPaths == null)
            return patterns;

        foreach (var schemaFieldPath in schemaFieldPaths)
        {
            var serializedRoot = string.Join('.', schemaFieldPath.Split('.').Select(ToSerializedMemberName));
            AddReferencePattern(definitionType, serializedRoot, patterns);
        }

        return patterns;
    }

    private static void AddReferencePattern(Type definitionType, string serializedRoot, HashSet<string> patterns)
    {
        var normalizedRoot = serializedRoot.Replace(" ", string.Empty);
        var explicitElementIdFields = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["stats.entries"] = "statId",
            ["statModifiers"] = "targetStatId",
            ["costs"] = "resourceId",
            ["upkeepCosts"] = "resourceId",
            ["buildCosts"] = "resourceId",
            ["inputs"] = "itemId",
            ["outputs"] = "itemId"
        };

        if (explicitElementIdFields.TryGetValue(normalizedRoot, out var idField))
        {
            patterns.Add($"{normalizedRoot}.Array.data[].{idField}");
            return;
        }

        var memberType = ResolveMemberType(definitionType, normalizedRoot.Split('.'));
        if (memberType == typeof(string))
        {
            patterns.Add(normalizedRoot);
            return;
        }

        if (TryGetEnumerableElementType(memberType, out var elementType) && elementType == typeof(string))
            patterns.Add($"{normalizedRoot}.Array.data[]");
    }

    private static Type ResolveMemberType(Type rootType, IEnumerable<string> path)
    {
        var current = rootType;
        foreach (var segment in path)
        {
            var field = current.GetField(segment, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.IgnoreCase);
            if (field == null)
                return null;

            current = field.FieldType;
        }

        return current;
    }

    private static bool TryGetEnumerableElementType(Type type, out Type elementType)
    {
        elementType = null;
        if (type == null)
            return false;

        if (type.IsArray)
        {
            elementType = type.GetElementType();
            return true;
        }

        if (!typeof(IEnumerable).IsAssignableFrom(type) || type == typeof(string))
            return false;

        if (type.IsGenericType)
        {
            elementType = type.GetGenericArguments()[0];
            return true;
        }

        return false;
    }

    private static string ToSerializedMemberName(string schemaName)
    {
        if (string.IsNullOrWhiteSpace(schemaName))
            return schemaName;

        if (schemaName.Length == 1)
            return schemaName.ToLowerInvariant();

        return char.ToLowerInvariant(schemaName[0]) + schemaName.Substring(1);
    }

    private static string NormalizeArrayPath(string propertyPath)
    {
        if (string.IsNullOrWhiteSpace(propertyPath))
            return propertyPath;

        return System.Text.RegularExpressions.Regex.Replace(propertyPath, @"\.Array\.data\[\d+\]", ".Array.data[]");
    }

    private static void NormalizeExistingIdsInBulk()
    {
        var entries = CollectDefinitionEntries();
        if (entries.Count == 0)
        {
            EditorUtility.DisplayDialog("Definition ID Migration", "No ScriptableObject assets with serialized 'id' fields were found.", "OK");
            return;
        }

        var invalidEntries = new List<DefinitionIdEntry>();
        var toNormalize = new List<DefinitionIdEntry>();
        var groupedByNormalized = new Dictionary<string, List<DefinitionIdEntry>>(StringComparer.Ordinal);

        foreach (var entry in entries)
        {
            if (!DefinitionIdLifecycle.IsValidIdFormat(entry.NormalizedId))
            {
                invalidEntries.Add(entry);
                continue;
            }

            if (!groupedByNormalized.TryGetValue(entry.NormalizedId, out var list))
            {
                list = new List<DefinitionIdEntry>();
                groupedByNormalized.Add(entry.NormalizedId, list);
            }

            list.Add(entry);
        }

        var collisions = groupedByNormalized
            .Where(kvp => kvp.Value.Count > 1)
            .ToList();

        var collidingAssets = new HashSet<string>(StringComparer.Ordinal);
        foreach (var collision in collisions)
        {
            foreach (var entry in collision.Value)
                collidingAssets.Add(entry.Path);
        }

        foreach (var entry in entries)
        {
            if (invalidEntries.Contains(entry) || collidingAssets.Contains(entry.Path))
                continue;

            if (!string.Equals(entry.RawId, entry.NormalizedId, StringComparison.Ordinal))
                toNormalize.Add(entry);
        }

        foreach (var invalid in invalidEntries)
            Debug.LogWarning($"[Definition IDs] Skipping normalization for '{invalid.Path}'. Raw ID '{invalid.RawId}' normalizes to invalid value '{invalid.NormalizedId}'.", invalid.Asset);

        foreach (var collision in collisions)
        {
            var details = string.Join("\n", collision.Value.Select(e => $"- {e.Path} (raw: '{e.RawId}')"));
            Debug.LogError($"[Definition IDs] Normalization collision for '{collision.Key}'. Multiple assets converge to the same normalized ID. Resolve IDs manually before bulk normalization:\n{details}");
        }

        var summary = $"Scanned {entries.Count} definition asset(s).\n" +
                      $"Will normalize {toNormalize.Count} asset(s).\n" +
                      $"Skipped {invalidEntries.Count} invalid normalization(s).\n" +
                      $"Detected {collisions.Count} collision group(s).";

        if (!EditorUtility.DisplayDialog("Normalize Existing IDs", summary + "\n\nProceed with safe normalization?", "Normalize", "Cancel"))
            return;

        var updated = 0;

        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var entry in toNormalize)
            {
                entry.SerializedObject.Update();
                entry.IdProperty.stringValue = entry.NormalizedId;
                SetIfPresent(entry.SerializedObject.FindProperty("isIdFinalized"), true);
                SetIfPresent(entry.SerializedObject.FindProperty("finalizedId"), entry.NormalizedId);
                entry.SerializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(entry.Asset);
                updated++;

                Debug.LogWarning($"[Definition IDs] Normalized ID for '{entry.Path}' from '{entry.RawId}' to '{entry.NormalizedId}'.", entry.Asset);
            }

            AssetDatabase.SaveAssets();
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }

        EditorUtility.DisplayDialog("Normalize Existing IDs", $"Normalization complete. Updated {updated} asset(s). Conflicts and invalid entries were skipped; see Console for details.", "OK");
    }

    private static List<DefinitionIdEntry> CollectDefinitionEntries()
    {
        var entries = new List<DefinitionIdEntry>();

        foreach (var guid in AssetDatabase.FindAssets("t:ScriptableObject"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (asset == null)
                continue;

            if (!TryGetIdProperty(asset, out var serializedObject, out var idProperty))
                continue;

            var rawId = idProperty.stringValue;
            var normalizedId = DefinitionIdLifecycle.NormalizeId(rawId);
            entries.Add(new DefinitionIdEntry(asset, path, serializedObject, idProperty, rawId, normalizedId));
        }

        return entries;
    }

    private static void SetIfPresent(SerializedProperty property, bool value)
    {
        if (property != null && property.propertyType == SerializedPropertyType.Boolean)
            property.boolValue = value;
    }

    private static void SetIfPresent(SerializedProperty property, string value)
    {
        if (property != null && property.propertyType == SerializedPropertyType.String)
            property.stringValue = value;
    }

    private static bool TryGetIdProperty(ScriptableObject asset, out SerializedObject serializedObject, out SerializedProperty idProperty)
    {
        serializedObject = null;
        idProperty = null;

        if (asset == null)
            return false;

        serializedObject = new SerializedObject(asset);
        idProperty = serializedObject.FindProperty("id");
        return idProperty != null && idProperty.propertyType == SerializedPropertyType.String;
    }

    private static bool HasDuplicateId(ScriptableObject target, string candidate)
    {
        foreach (var guid in AssetDatabase.FindAssets("t:ScriptableObject"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (asset == null || asset == target || asset is not IIdentifiable identifiable)
                continue;

            if (string.Equals(DefinitionIdLifecycle.NormalizeId(identifiable.Id), candidate, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private sealed class DefinitionIdEntry
    {
        public ScriptableObject Asset { get; }
        public string Path { get; }
        public SerializedObject SerializedObject { get; }
        public SerializedProperty IdProperty { get; }
        public string RawId { get; }
        public string NormalizedId { get; }

        public DefinitionIdEntry(
            ScriptableObject asset,
            string path,
            SerializedObject serializedObject,
            SerializedProperty idProperty,
            string rawId,
            string normalizedId)
        {
            Asset = asset;
            Path = path;
            SerializedObject = serializedObject;
            IdProperty = idProperty;
            RawId = rawId;
            NormalizedId = normalizedId;
        }
    }

    private sealed class MigrationOperation
    {
        public string AssetPath { get; }
        public string PropertyPath { get; }
        public string OldValue { get; }
        public string NewValue { get; }

        public MigrationOperation(string assetPath, string propertyPath, string oldValue, string newValue)
        {
            AssetPath = assetPath;
            PropertyPath = propertyPath;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
