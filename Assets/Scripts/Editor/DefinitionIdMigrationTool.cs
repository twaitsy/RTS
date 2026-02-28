using System;
using System.Collections.Generic;
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
        window.minSize = new Vector2(520f, 200f);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("Use this tool for deliberate ID renames. It updates the definition ID plus all matching serialized string references in ScriptableObject assets.", MessageType.Info);

        targetDefinition = (ScriptableObject)EditorGUILayout.ObjectField("Target Definition", targetDefinition, typeof(ScriptableObject), false);

        using (new EditorGUI.DisabledScope(targetDefinition == null || !TryGetIdProperty(targetDefinition, out _, out _)))
        {
            if (targetDefinition != null && TryGetIdProperty(targetDefinition, out var so, out var idProperty))
            {
                EditorGUILayout.LabelField("Current ID", idProperty.stringValue);
                if (string.IsNullOrWhiteSpace(newId))
                    newId = idProperty.stringValue;
            }

            newId = EditorGUILayout.TextField("New ID", newId ?? string.Empty);

            if (GUILayout.Button("Validate + Migrate"))
                ValidateAndMigrate();
        }
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

        newId = newId.Trim();

        if (!DefinitionIdLifecycle.IsValidIdFormat(newId))
        {
            EditorUtility.DisplayDialog("Definition ID Migration", "Invalid ID format. Allowed pattern: lowercase alphanumeric segments separated by '.', '_' or '-'.", "OK");
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

            if (string.Equals(identifiable.Id, candidate, StringComparison.Ordinal))
                return true;
        }

        return false;
    }
}
