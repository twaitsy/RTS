using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public sealed class BuildingCategoryIdPickerWindow : EditorWindow
{
    private sealed class Row
    {
        public string Id;
        public string DisplayName;
        public string AssetName;
        public BuildingCategoryDefinition Definition;
    }

    private readonly List<Row> rows = new();
    private readonly List<Row> filteredRows = new();

    private ScriptableObject target;
    private string propertyPath;
    private string selectedCategoryId;
    private string searchText = string.Empty;
    private Vector2 scroll;

    public static void OpenForTarget(ScriptableObject target, string propertyPath, string currentValue)
    {
        var window = GetWindow<BuildingCategoryIdPickerWindow>("Building Category Picker");
        window.minSize = new Vector2(760f, 360f);
        window.target = target;
        window.propertyPath = propertyPath;
        window.selectedCategoryId = currentValue?.Trim() ?? string.Empty;
        window.searchText = window.selectedCategoryId;
        window.RebuildRows();
        window.Show();
        window.Focus();

        if (target != null)
            EditorGUIUtility.PingObject(target);
    }

    private void OnEnable()
    {
        RebuildRows();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Building Category Picker", EditorStyles.boldLabel);

        if (target == null || string.IsNullOrWhiteSpace(propertyPath))
        {
            EditorGUILayout.HelpBox("Target/property path are required. Open from a fix button or tool context.", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField($"Target: {target.name}", EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.LabelField($"Property: {propertyPath}", EditorStyles.wordWrappedMiniLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            var nextSearch = EditorGUILayout.TextField("Search", searchText ?? string.Empty);
            if (!string.Equals(nextSearch, searchText, StringComparison.Ordinal))
            {
                searchText = nextSearch;
                ApplyFilter();
            }

            if (GUILayout.Button("Refresh", GUILayout.Width(80f)))
                RebuildRows();
        }

        EditorGUILayout.Space(4f);
        DrawTable();

        EditorGUILayout.Space(8f);
        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope(GetSelectedRow() == null))
            {
                if (GUILayout.Button("Ping Definition", GUILayout.Width(120f)))
                    PingSelectedDefinition();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Cancel", GUILayout.Width(100f)))
                Close();

            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(selectedCategoryId)))
            {
                if (GUILayout.Button("Apply", GUILayout.Width(100f)))
                    ApplySelection();
            }
        }
    }

    private void DrawTable()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            GUILayout.Label("", GUILayout.Width(22f));
            GUILayout.Label("Category ID", EditorStyles.miniBoldLabel, GUILayout.Width(260f));
            GUILayout.Label("Display Name", EditorStyles.miniBoldLabel, GUILayout.Width(240f));
            GUILayout.Label("Asset Name", EditorStyles.miniBoldLabel);
        }

        using var scrollScope = new EditorGUILayout.ScrollViewScope(scroll, GUILayout.Height(220f));
        scroll = scrollScope.scrollPosition;

        foreach (var row in filteredRows)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var isSelected = string.Equals(selectedCategoryId, row.Id, StringComparison.Ordinal);
                if (GUILayout.Toggle(isSelected, GUIContent.none, GUILayout.Width(22f)) && !isSelected)
                    selectedCategoryId = row.Id;

                EditorGUILayout.SelectableLabel(row.Id ?? string.Empty, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(260f));
                EditorGUILayout.SelectableLabel(row.DisplayName ?? string.Empty, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(240f));
                EditorGUILayout.SelectableLabel(row.AssetName ?? string.Empty, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
        }

        if (filteredRows.Count == 0)
            EditorGUILayout.HelpBox("No building categories match the current search.", MessageType.Info);
    }

    private void RebuildRows()
    {
        rows.Clear();

        foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(BuildingCategoryDefinition)}"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var definition = AssetDatabase.LoadAssetAtPath<BuildingCategoryDefinition>(path);
            if (definition == null)
                continue;

            rows.Add(new Row
            {
                Id = definition.Id?.Trim() ?? string.Empty,
                DisplayName = definition.DisplayName?.Trim() ?? string.Empty,
                AssetName = definition.name,
                Definition = definition
            });
        }

        rows.Sort((a, b) => string.Compare(a.Id, b.Id, StringComparison.Ordinal));
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        filteredRows.Clear();
        var filter = searchText?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(filter))
        {
            filteredRows.AddRange(rows);
            return;
        }

        foreach (var row in rows)
        {
            if (ContainsIgnoreCase(row.Id, filter)
                || ContainsIgnoreCase(row.DisplayName, filter)
                || ContainsIgnoreCase(row.AssetName, filter))
            {
                filteredRows.Add(row);
            }
        }
    }

    private static bool ContainsIgnoreCase(string value, string token)
    {
        return !string.IsNullOrEmpty(value)
               && !string.IsNullOrEmpty(token)
               && value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private Row GetSelectedRow()
    {
        return rows.FirstOrDefault(row => string.Equals(row.Id, selectedCategoryId, StringComparison.Ordinal));
    }

    private void PingSelectedDefinition()
    {
        var row = GetSelectedRow();
        if (row?.Definition == null)
            return;

        EditorGUIUtility.PingObject(row.Definition);
        Selection.activeObject = row.Definition;
    }

    private void ApplySelection()
    {
        if (target == null)
            return;

        var serializedObject = new SerializedObject(target);
        var property = serializedObject.FindProperty(propertyPath);
        if (property == null || property.propertyType != SerializedPropertyType.String)
        {
            EditorUtility.DisplayDialog("Building Category Picker", $"Property '{propertyPath}' was not found or is not a string.", "OK");
            return;
        }

        property.stringValue = selectedCategoryId?.Trim() ?? string.Empty;
        if (serializedObject.ApplyModifiedPropertiesWithoutUndo())
        {
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Close();
    }
}
