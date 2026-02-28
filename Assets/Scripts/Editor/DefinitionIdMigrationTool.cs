using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public sealed class DefinitionIdMigrationTool : EditorWindow
{
    private ScriptableObject targetDefinition;
    private string newId;

    [MenuItem("Tools/Data/Definition ID Migration")]
    public static void Open()
    {
        var window = GetWindow<DefinitionIdMigrationTool>("Definition ID Migration");
        window.minSize = new Vector2(520f, 260f);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("Use this tool for deliberate ID renames. It updates the definition ID plus all matching serialized string references in ScriptableObject assets.", MessageType.Info);

        targetDefinition = (ScriptableObject)EditorGUILayout.ObjectField("Target Definition", targetDefinition, typeof(ScriptableObject), false);

        using (new EditorGUI.DisabledScope(targetDefinition == null || !TryGetIdProperty(targetDefinition, out _, out _)))
        {
            if (targetDefinition != null && TryGetIdProperty(targetDefinition, out _, out var idProperty))
            {
                EditorGUILayout.LabelField("Current ID", idProperty.stringValue);
                if (string.IsNullOrWhiteSpace(newId))
                    newId = idProperty.stringValue;
            }

            newId = EditorGUILayout.TextField("New ID", newId ?? string.Empty);

            if (GUILayout.Button("Validate + Migrate"))
                ValidateAndMigrate();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Migration support path: normalize all existing definition IDs in bulk. This safely skips conflicts where multiple assets converge to the same normalized ID.", MessageType.None);

        if (GUILayout.Button("Normalize Existing IDs (Bulk)"))
            NormalizeExistingIdsInBulk();
    }

    private void ValidateAndMigrate()
    {
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
        {
            Debug.LogWarning($"[Definition IDs] Normalized migration ID from '{rawNewId}' to '{newId}'.");
        }

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

        if (!EditorUtility.DisplayDialog("Confirm Migration", $"Rename '{oldId}' to '{newId}' and update all ScriptableObject references?", "Migrate", "Cancel"))
            return;

        var changedAssets = 0;
        var touchedDefinitions = 0;

        AssetDatabase.StartAssetEditing();
        try
        {
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset == null)
                    continue;

                var serializedObject = new SerializedObject(asset);
                bool changed = ReplaceMatchingStringIds(serializedObject, oldId, newId);
                if (!changed)
                    continue;

                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(asset);
                touchedDefinitions++;

                if (path != AssetDatabase.GetAssetPath(targetDefinition))
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
        {
            Debug.LogWarning($"[Definition IDs] Skipping normalization for '{invalid.Path}'. Raw ID '{invalid.RawId}' normalizes to invalid value '{invalid.NormalizedId}'.", invalid.Asset);
        }

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

    private static bool ReplaceMatchingStringIds(SerializedObject serializedObject, string oldId, string newId)
    {
        bool changed = false;
        var iterator = serializedObject.GetIterator();
        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = true;

            if (iterator.propertyType != SerializedPropertyType.String)
                continue;

            string propertyName = iterator.name;
            if (!propertyName.EndsWith("id", StringComparison.OrdinalIgnoreCase))
                continue;

            if (!string.Equals(iterator.stringValue, oldId, StringComparison.Ordinal))
                continue;

            iterator.stringValue = newId;
            changed = true;
        }

        return changed;
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
        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
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
}
