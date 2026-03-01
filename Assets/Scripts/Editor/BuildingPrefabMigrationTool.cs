#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public sealed class BuildingPrefabMigrationTool : EditorWindow
{
    private const string DefaultPrefabDefinitionFolder = PrefabRegistry.PrefabRegistryAssetFolder;
    private Vector2 scroll;
    private readonly List<string> unresolvedBuildingIds = new();

    [MenuItem("Tools/Data/Building Prefab Migration")]
    public static void Open()
    {
        GetWindow<BuildingPrefabMigrationTool>("Building Prefab Migration").Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("Migrates BuildingDefinition assets to prefabId/primaryCategoryId fields and scans unresolved prefab references.", MessageType.Info);

        if (GUILayout.Button("Run migration defaults"))
            RunMigrationDefaults();

        if (GUILayout.Button("Scan unresolved prefabId values"))
            ScanUnresolvedPrefabIds();

        using (new EditorGUI.DisabledScope(unresolvedBuildingIds.Count == 0))
        {
            if (GUILayout.Button("Create PrefabDefinition stubs for unresolved ids"))
                CreatePrefabDefinitionStubs();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Unresolved building count: {unresolvedBuildingIds.Count}", EditorStyles.boldLabel);
        using var scope = new EditorGUILayout.ScrollViewScope(scroll, GUILayout.Height(180f));
        scroll = scope.scrollPosition;

        foreach (var entry in unresolvedBuildingIds)
            EditorGUILayout.LabelField(entry, EditorStyles.wordWrappedMiniLabel);
    }

    private static void RunMigrationDefaults()
    {
        var changes = 0;

        foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(BuildingDefinition)}"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<BuildingDefinition>(path);
            if (asset == null)
                continue;

            var serialized = new SerializedObject(asset);
            var id = serialized.FindProperty("id")?.stringValue?.Trim();
            var prefabId = serialized.FindProperty("prefabId");
            var primaryCategory = serialized.FindProperty("primaryCategoryId");

            if (prefabId != null && string.IsNullOrWhiteSpace(prefabId.stringValue) && !string.IsNullOrWhiteSpace(id))
            {
                prefabId.stringValue = id;
                changes++;
            }

            if (primaryCategory != null && string.IsNullOrWhiteSpace(primaryCategory.stringValue))
            {
                var legacyCategory = serialized.FindProperty("categoryId") ?? serialized.FindProperty("buildingCategoryId");
                if (legacyCategory != null && legacyCategory.propertyType == SerializedPropertyType.String)
                {
                    primaryCategory.stringValue = legacyCategory.stringValue?.Trim() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(primaryCategory.stringValue))
                        changes++;
                }
            }

            if (serialized.ApplyModifiedPropertiesWithoutUndo())
                EditorUtility.SetDirty(asset);
        }

        if (changes > 0)
            AssetDatabase.SaveAssets();

        Debug.Log($"[BuildingPrefabMigrationTool] Migration complete. Changed field values: {changes}.");
    }

    private void ScanUnresolvedPrefabIds()
    {
        unresolvedBuildingIds.Clear();
        PrefabRegistry.Initialize();

        foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(BuildingDefinition)}"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var definition = AssetDatabase.LoadAssetAtPath<BuildingDefinition>(path);
            if (definition == null)
                continue;

            var prefabId = definition.PrefabId?.Trim();
            if (string.IsNullOrWhiteSpace(prefabId) || PrefabRegistry.TryGetDefinition(prefabId, out _))
                continue;

            unresolvedBuildingIds.Add($"{definition.Id} -> '{prefabId}' ({path})");
        }

        Debug.Log($"[BuildingPrefabMigrationTool] Scan complete. Unresolved entries: {unresolvedBuildingIds.Count}.");
    }

    private void CreatePrefabDefinitionStubs()
    {
        EnsureFolder(DefaultPrefabDefinitionFolder);

        var created = 0;
        foreach (var unresolved in unresolvedBuildingIds)
        {
            var separatorIndex = unresolved.IndexOf("->", System.StringComparison.Ordinal);
            if (separatorIndex <= 0)
                continue;

            var prefabId = unresolved.Substring(separatorIndex + 2).Trim();
            if (prefabId.StartsWith("'"))
                prefabId = prefabId.Substring(1, prefabId.IndexOf('\'', 1) - 1);

            if (PrefabRegistry.TryGetDefinition(prefabId, out _))
                continue;

            var existingPath = $"{DefaultPrefabDefinitionFolder}/{prefabId}.asset";
            if (AssetDatabase.LoadAssetAtPath<PrefabDefinition>(existingPath) != null)
                continue;

            var definition = CreateInstance<PrefabDefinition>();
            var so = new SerializedObject(definition);
            so.FindProperty("id").stringValue = prefabId;
            var prefabProperty = so.FindProperty("prefab");
            prefabProperty.objectReferenceValue = FindPrefabById(prefabId);
            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(definition, existingPath);
            created++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        PrefabRegistry.Initialize();
        ScanUnresolvedPrefabIds();

        Debug.Log($"[BuildingPrefabMigrationTool] Created {created} prefab definition stub(s). Unresolved remaining: {unresolvedBuildingIds.Count}.");
    }

    private static GameObject FindPrefabById(string prefabId)
    {
        var guids = AssetDatabase.FindAssets($"{prefabId} t:Prefab", new[] { "Assets/Prefabs" });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) == prefabId)
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        return null;
    }

    private static void EnsureFolder(string path)
    {
        var parts = path.Split('/');
        var current = parts[0];

        for (var i = 1; i < parts.Length; i++)
        {
            var next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
#endif
