using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public sealed class PrefabIdPickerWindow : EditorWindow
{
    private sealed class Row
    {
        public string Id;
        public string AssetName;
        public PrefabDefinition Definition;
        public GameObject PrefabObject;
    }

    private readonly List<Row> rows = new();
    private readonly List<Row> filteredRows = new();

    private ScriptableObject target;
    private string propertyPath;
    private string currentValue;
    private string selectedPrefabId;
    private string searchText = string.Empty;
    private Vector2 scroll;

    public static void OpenForTarget(ScriptableObject target, string propertyPath, string currentValue)
    {
        var window = GetWindow<PrefabIdPickerWindow>("Prefab ID Picker");
        window.minSize = new Vector2(760f, 360f);
        window.target = target;
        window.propertyPath = propertyPath;
        window.currentValue = currentValue?.Trim() ?? string.Empty;
        window.selectedPrefabId = window.currentValue;
        window.searchText = window.currentValue;
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
        EditorGUILayout.LabelField("Prefab ID Picker", EditorStyles.boldLabel);

        if (target == null || string.IsNullOrWhiteSpace(propertyPath))
        {
            EditorGUILayout.HelpBox("Target/property path are required. Open from a fix button or tool context.", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField($"Target: {target.name}", EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.LabelField($"Property: {propertyPath}", EditorStyles.wordWrappedMiniLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            var newSearch = EditorGUILayout.TextField("Search", searchText ?? string.Empty);
            if (!string.Equals(newSearch, searchText, StringComparison.Ordinal))
            {
                searchText = newSearch;
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
                if (GUILayout.Button("Ping PrefabDefinition", GUILayout.Width(152f)))
                    PingSelectedDefinition();

                if (GUILayout.Button("Ping Prefab", GUILayout.Width(112f)))
                    PingSelectedPrefab();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Cancel", GUILayout.Width(100f)))
                Close();

            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(selectedPrefabId)))
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
            GUILayout.Label("Prefab ID", EditorStyles.miniBoldLabel, GUILayout.Width(280f));
            GUILayout.Label("Asset Name", EditorStyles.miniBoldLabel, GUILayout.Width(200f));
            GUILayout.Label("Assigned Prefab", EditorStyles.miniBoldLabel);
        }

        using var scrollScope = new EditorGUILayout.ScrollViewScope(scroll, GUILayout.Height(220f));
        scroll = scrollScope.scrollPosition;

        foreach (var row in filteredRows)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var isSelected = string.Equals(selectedPrefabId, row.Id, StringComparison.Ordinal);
                if (GUILayout.Toggle(isSelected, GUIContent.none, GUILayout.Width(22f)) && !isSelected)
                    selectedPrefabId = row.Id;

                EditorGUILayout.SelectableLabel(row.Id ?? string.Empty, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(280f));
                EditorGUILayout.SelectableLabel(row.AssetName ?? string.Empty, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(200f));

                using (new EditorGUI.DisabledScope(true))
                    EditorGUILayout.ObjectField(row.PrefabObject, typeof(GameObject), false);
            }
        }

        if (filteredRows.Count == 0)
            EditorGUILayout.HelpBox("No prefab definitions match the current search.", MessageType.Info);
    }

    private void RebuildRows()
    {
        PrefabRegistry.Initialize();

        rows.Clear();
        foreach (var definition in PrefabRegistry.All())
        {
            if (definition == null)
                continue;

            rows.Add(new Row
            {
                Id = definition.Id?.Trim() ?? string.Empty,
                AssetName = definition.name,
                Definition = definition,
                PrefabObject = definition.Prefab
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
                || ContainsIgnoreCase(row.AssetName, filter)
                || ContainsIgnoreCase(row.PrefabObject != null ? row.PrefabObject.name : string.Empty, filter))
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
        return rows.FirstOrDefault(row => string.Equals(row.Id, selectedPrefabId, StringComparison.Ordinal));
    }

    private void PingSelectedDefinition()
    {
        var row = GetSelectedRow();
        if (row?.Definition == null)
            return;

        EditorGUIUtility.PingObject(row.Definition);
        Selection.activeObject = row.Definition;
    }

    private void PingSelectedPrefab()
    {
        var row = GetSelectedRow();
        if (row?.PrefabObject == null)
            return;

        EditorGUIUtility.PingObject(row.PrefabObject);
        Selection.activeObject = row.PrefabObject;
    }

    private void ApplySelection()
    {
        if (target == null)
            return;

        var serializedObject = new SerializedObject(target);
        var property = serializedObject.FindProperty(propertyPath);
        if (property == null || property.propertyType != SerializedPropertyType.String)
        {
            EditorUtility.DisplayDialog("Prefab ID Picker", $"Property '{propertyPath}' was not found or is not a string.", "OK");
            return;
        }

        property.stringValue = selectedPrefabId?.Trim() ?? string.Empty;
        if (serializedObject.ApplyModifiedPropertiesWithoutUndo())
        {
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Close();
    }
}
