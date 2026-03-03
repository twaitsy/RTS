#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public sealed class BuildingPrefabMigrationTool : EditorWindow
{
    private sealed class PreviewEntry
    {
        public string assetPath;
        public string buildingId;
        public string currentPrefabId;
        public string proposedPrefabId;
        public bool selected;

        public bool updatePrefabId;
        public string proposedPrimaryCategoryId;
        public bool updatePrimaryCategory;
    }

    private sealed class UnresolvedEntry
    {
        public string buildingId;
        public string prefabId;
        public string assetPath;
    }

    private const string DefaultPrefabDefinitionFolder = PrefabRegistry.PrefabRegistryAssetFolder;
    private Vector2 scroll;
    private Vector2 previewScroll;
    private readonly List<UnresolvedEntry> unresolvedEntries = new();
    private readonly List<PreviewEntry> previewEntries = new();
    private BuildingDefinition selectedBuildingDefinition;
    private bool hasPreview;
    private int selectedPreviewCount;
    private int skippedPreviewCount;

    [MenuItem("Tools/Data/Building Prefab Migration")]
    public static void Open()
    {
        GetWindow<BuildingPrefabMigrationTool>("Building Prefab Migration").Show();
    }

    public static void OpenForBuilding(BuildingDefinition building, string fieldPath, string suggestedValue)
    {
        var window = GetWindow<BuildingPrefabMigrationTool>("Building Prefab Migration");
        window.selectedBuildingDefinition = building;
        window.Show();
        window.Focus();

        if (building != null)
            EditorGUIUtility.PingObject(building);

        if (building == null)
            return;

        var serialized = new SerializedObject(building);
        var property = string.IsNullOrWhiteSpace(fieldPath) ? null : serialized.FindProperty(fieldPath);
        if (property != null && property.propertyType == SerializedPropertyType.String)
        {
            var suggested = ExtractSuggestion(suggestedValue);
            if (!string.IsNullOrWhiteSpace(suggested))
            {
                property.stringValue = suggested;
                if (serialized.ApplyModifiedPropertiesWithoutUndo())
                    EditorUtility.SetDirty(building);
            }
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("Previews and migrates BuildingDefinition defaults for prefabId/primaryCategoryId, and scans unresolved prefab references.", MessageType.Info);

        if (GUILayout.Button("Build migration preview"))
            BuildMigrationPreview();

        EditorGUILayout.Space();
        DrawPreviewSection();

        EditorGUILayout.Space();
        DrawDirectEditSection();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Unresolved Prefab Scan", EditorStyles.boldLabel);

        if (GUILayout.Button("Scan unresolved prefabId values"))
            ScanUnresolvedPrefabIds();

        using (new EditorGUI.DisabledScope(unresolvedEntries.Count == 0))
        {
            if (GUILayout.Button("Create PrefabDefinition stubs for unresolved ids"))
                CreatePrefabDefinitionStubs();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Unresolved building count: {unresolvedEntries.Count}", EditorStyles.boldLabel);
        using var scope = new EditorGUILayout.ScrollViewScope(scroll, GUILayout.Height(180f));
        scroll = scope.scrollPosition;

        foreach (var entry in unresolvedEntries)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"{entry.buildingId} -> '{entry.prefabId}' ({entry.assetPath})", EditorStyles.wordWrappedMiniLabel);
                if (GUILayout.Button("Select", GUILayout.Width(64f)))
                {
                    selectedBuildingDefinition = AssetDatabase.LoadAssetAtPath<BuildingDefinition>(entry.assetPath);
                    if (selectedBuildingDefinition != null)
                        EditorGUIUtility.PingObject(selectedBuildingDefinition);
                }
            }
        }
    }

    private void DrawDirectEditSection()
    {
        EditorGUILayout.LabelField("Direct Building Prefab ID Edit", EditorStyles.boldLabel);
        selectedBuildingDefinition = (BuildingDefinition)EditorGUILayout.ObjectField(
            "Building Definition",
            selectedBuildingDefinition,
            typeof(BuildingDefinition),
            false);

        if (selectedBuildingDefinition == null)
            return;

        var serialized = new SerializedObject(selectedBuildingDefinition);
        serialized.Update();

        var idProperty = serialized.FindProperty("id");
        var prefabIdProperty = serialized.FindProperty("prefabId");

        if (idProperty == null || prefabIdProperty == null)
        {
            EditorGUILayout.HelpBox("Selected BuildingDefinition is missing id/prefabId serialized fields.", MessageType.Warning);
            return;
        }

        var id = idProperty.stringValue?.Trim() ?? string.Empty;
        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.TextField("Current ID", id);

        var validationMessage = string.Empty;
        var validationType = MessageType.None;

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.PropertyField(prefabIdProperty, new GUIContent("Prefab ID"));

            var trimmedPreview = prefabIdProperty.stringValue?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(trimmedPreview))
            {
                validationMessage = "Empty";
                validationType = MessageType.Warning;
            }
            else
            {
                PrefabRegistry.Initialize();
                if (!PrefabRegistry.TryGetDefinition(trimmedPreview, out _))
                {
                    validationMessage = "Unknown prefab";
                    validationType = MessageType.Warning;
                }
                else
                {
                    validationMessage = "OK";
                }
            }

            var color = GUI.color;
            if (validationType == MessageType.Warning)
                GUI.color = Color.yellow;
            EditorGUILayout.LabelField(validationMessage, GUILayout.Width(96f));
            GUI.color = color;
        }

        var trimmedPrefabId = prefabIdProperty.stringValue?.Trim() ?? string.Empty;
        if (validationType == MessageType.Warning)
            EditorGUILayout.HelpBox(
                string.IsNullOrWhiteSpace(trimmedPrefabId)
                    ? "Prefab ID is empty after trimming."
                    : $"No PrefabDefinition found for '{trimmedPrefabId}'.",
                MessageType.Warning);

        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(id)))
            {
                if (GUILayout.Button("Use building id as prefab id"))
                    prefabIdProperty.stringValue = id;
            }

            if (GUILayout.Button("Apply Prefab ID"))
            {
                prefabIdProperty.stringValue = trimmedPrefabId;
                if (serialized.ApplyModifiedPropertiesWithoutUndo())
                {
                    EditorUtility.SetDirty(selectedBuildingDefinition);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }
    }

    private void DrawPreviewSection()
    {
        EditorGUILayout.LabelField("Migration Preview", EditorStyles.boldLabel);

        if (!hasPreview)
        {
            EditorGUILayout.HelpBox("No preview generated yet. Click 'Build migration preview' to scan BuildingDefinition assets.", MessageType.None);
            return;
        }

        EditorGUILayout.LabelField($"Preview entries: {previewEntries.Count} | Selected: {selectedPreviewCount} | Skipped: {skippedPreviewCount}", EditorStyles.miniBoldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Select All"))
                SetAllPreviewSelections(true);

            if (GUILayout.Button("Select None"))
                SetAllPreviewSelections(false);
        }

        using (var scope = new EditorGUILayout.ScrollViewScope(previewScroll, GUILayout.Height(220f)))
        {
            previewScroll = scope.scrollPosition;
            foreach (var entry in previewEntries)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    entry.selected = EditorGUILayout.ToggleLeft(
                        $"{entry.buildingId} ({entry.assetPath})",
                        entry.selected);

                    EditorGUILayout.LabelField($"PrefabId: '{entry.currentPrefabId}' -> '{entry.proposedPrefabId}'", EditorStyles.wordWrappedMiniLabel);
                    if (entry.updatePrimaryCategory)
                        EditorGUILayout.LabelField($"PrimaryCategoryId: set to '{entry.proposedPrimaryCategoryId}'", EditorStyles.wordWrappedMiniLabel);
                }
            }
        }

        RefreshPreviewCounts();
        using (new EditorGUI.DisabledScope(selectedPreviewCount == 0))
        {
            if (GUILayout.Button("Apply Selected Migration"))
                ApplySelectedMigration();
        }
    }

    private void BuildMigrationPreview()
    {
        previewEntries.Clear();
        hasPreview = true;

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
            var currentPrefabId = prefabId?.stringValue?.Trim() ?? string.Empty;

            var proposedPrefabId = currentPrefabId;
            var updatePrefabId = false;
            if (prefabId != null && string.IsNullOrWhiteSpace(currentPrefabId) && !string.IsNullOrWhiteSpace(id))
            {
                proposedPrefabId = id;
                updatePrefabId = true;
            }

            var proposedPrimaryCategoryId = string.Empty;
            var updatePrimaryCategory = false;
            if (primaryCategory != null && string.IsNullOrWhiteSpace(primaryCategory.stringValue))
            {
                var legacyCategory = serialized.FindProperty("categoryId") ?? serialized.FindProperty("buildingCategoryId");
                if (legacyCategory != null && legacyCategory.propertyType == SerializedPropertyType.String)
                {
                    proposedPrimaryCategoryId = legacyCategory.stringValue?.Trim() ?? string.Empty;
                    updatePrimaryCategory = !string.IsNullOrWhiteSpace(proposedPrimaryCategoryId);
                }
            }

            if (!updatePrefabId && !updatePrimaryCategory)
                continue;

            previewEntries.Add(new PreviewEntry
            {
                assetPath = path,
                buildingId = id,
                currentPrefabId = currentPrefabId,
                proposedPrefabId = proposedPrefabId,
                selected = true,
                updatePrefabId = updatePrefabId,
                proposedPrimaryCategoryId = proposedPrimaryCategoryId,
                updatePrimaryCategory = updatePrimaryCategory
            });
        }

        RefreshPreviewCounts();
        Debug.Log($"[BuildingPrefabMigrationTool] Preview generated. Entries: {previewEntries.Count}, selected: {selectedPreviewCount}, skipped: {skippedPreviewCount}.");
    }

    private void SetAllPreviewSelections(bool selected)
    {
        foreach (var entry in previewEntries)
            entry.selected = selected;

        RefreshPreviewCounts();
    }

    private void RefreshPreviewCounts()
    {
        selectedPreviewCount = 0;
        foreach (var entry in previewEntries)
        {
            if (entry.selected)
                selectedPreviewCount++;
        }

        skippedPreviewCount = previewEntries.Count - selectedPreviewCount;
    }

    private void ApplySelectedMigration()
    {
        RefreshPreviewCounts();
        if (selectedPreviewCount == 0)
            return;

        if (!EditorUtility.DisplayDialog(
                "Apply Selected Migration",
                $"Apply migration updates to {selectedPreviewCount} selected BuildingDefinition asset(s)?",
                "Apply",
                "Cancel"))
            return;

        var appliedEntries = 0;
        var changedFields = 0;

        foreach (var entry in previewEntries)
        {
            if (!entry.selected)
                continue;

            var asset = AssetDatabase.LoadAssetAtPath<BuildingDefinition>(entry.assetPath);
            if (asset == null)
                continue;

            var serialized = new SerializedObject(asset);
            var prefabId = serialized.FindProperty("prefabId");
            var primaryCategory = serialized.FindProperty("primaryCategoryId");
            var wroteAny = false;

            if (entry.updatePrefabId && prefabId != null)
            {
                prefabId.stringValue = entry.proposedPrefabId;
                wroteAny = true;
                changedFields++;
            }

            if (entry.updatePrimaryCategory && primaryCategory != null)
            {
                primaryCategory.stringValue = entry.proposedPrimaryCategoryId;
                wroteAny = true;
                changedFields++;
            }

            if (!wroteAny)
                continue;

            if (serialized.ApplyModifiedPropertiesWithoutUndo())
            {
                EditorUtility.SetDirty(asset);
                appliedEntries++;
            }
        }

        if (appliedEntries > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        BuildMigrationPreview();

        Debug.Log($"[BuildingPrefabMigrationTool] Selected changes applied. Assets updated: {appliedEntries}, field writes: {changedFields}, preview remaining: {previewEntries.Count}.");
    }

    private void ScanUnresolvedPrefabIds()
    {
        unresolvedEntries.Clear();
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

            unresolvedEntries.Add(new UnresolvedEntry
            {
                buildingId = definition.Id,
                prefabId = prefabId,
                assetPath = path
            });
        }

        Debug.Log($"[BuildingPrefabMigrationTool] Unresolved scan results: {unresolvedEntries.Count} unresolved prefabId entr{(unresolvedEntries.Count == 1 ? "y" : "ies")}.");
    }

    private void CreatePrefabDefinitionStubs()
    {
        EnsureFolder(DefaultPrefabDefinitionFolder);

        var created = 0;
        foreach (var unresolved in unresolvedEntries)
        {
            var prefabId = unresolved.prefabId;

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

        Debug.Log($"[BuildingPrefabMigrationTool] Created {created} prefab definition stub(s). Unresolved remaining: {unresolvedEntries.Count}.");
    }

    private static string ExtractSuggestion(string suggestedValue)
    {
        if (string.IsNullOrWhiteSpace(suggestedValue))
            return string.Empty;

        const string marker = "'";
        var firstQuote = suggestedValue.IndexOf(marker, System.StringComparison.Ordinal);
        var secondQuote = firstQuote >= 0 ? suggestedValue.IndexOf(marker, firstQuote + 1, System.StringComparison.Ordinal) : -1;
        if (firstQuote >= 0 && secondQuote > firstQuote)
            return suggestedValue.Substring(firstQuote + 1, secondQuote - firstQuote - 1).Trim();

        return suggestedValue.Trim();
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
