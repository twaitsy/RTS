using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public sealed class PrefabAssetPickerWindow : EditorWindow
{
    private const string PrefabRoot = "Assets/Prefabs";

    private readonly List<GameObject> allPrefabs = new();
    private readonly List<GameObject> filteredPrefabs = new();

    private ScriptableObject target;
    private string propertyPath;
    private string searchText = string.Empty;
    private GameObject selectedPrefab;
    private Vector2 scroll;

    public static void OpenForTarget(ScriptableObject target, string propertyPath)
    {
        var window = GetWindow<PrefabAssetPickerWindow>("Prefab Asset Picker");
        window.minSize = new Vector2(720f, 360f);
        window.target = target;
        window.propertyPath = propertyPath;
        window.searchText = string.Empty;
        window.selectedPrefab = null;
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
        EditorGUILayout.LabelField("Prefab Asset Picker", EditorStyles.boldLabel);

        if (target == null || string.IsNullOrWhiteSpace(propertyPath))
        {
            EditorGUILayout.HelpBox("Target/property path are required. Open this picker from a validation fix action.", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField($"Target: {target.name}", EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.LabelField($"Property: {propertyPath}", EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.LabelField($"Source Folder: {PrefabRoot}", EditorStyles.wordWrappedMiniLabel);

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
            using (new EditorGUI.DisabledScope(selectedPrefab == null))
            {
                if (GUILayout.Button("Ping Selected Prefab", GUILayout.Width(152f)))
                    EditorGUIUtility.PingObject(selectedPrefab);
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Cancel", GUILayout.Width(100f)))
                Close();

            using (new EditorGUI.DisabledScope(selectedPrefab == null))
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
            GUILayout.Label(string.Empty, GUILayout.Width(22f));
            GUILayout.Label("Prefab Name", EditorStyles.miniBoldLabel, GUILayout.Width(240f));
            GUILayout.Label("Asset Path", EditorStyles.miniBoldLabel);
        }

        using var scrollScope = new EditorGUILayout.ScrollViewScope(scroll, GUILayout.Height(220f));
        scroll = scrollScope.scrollPosition;

        foreach (var prefab in filteredPrefabs)
        {
            var path = AssetDatabase.GetAssetPath(prefab);
            using (new EditorGUILayout.HorizontalScope())
            {
                var isSelected = ReferenceEquals(selectedPrefab, prefab);
                if (GUILayout.Toggle(isSelected, GUIContent.none, GUILayout.Width(22f)) && !isSelected)
                    selectedPrefab = prefab;

                EditorGUILayout.SelectableLabel(prefab != null ? prefab.name : string.Empty, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(240f));
                EditorGUILayout.SelectableLabel(path ?? string.Empty, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
        }

        if (filteredPrefabs.Count == 0)
            EditorGUILayout.HelpBox($"No prefab assets found under '{PrefabRoot}' for the current filter.", MessageType.Info);
    }

    private void RebuildRows()
    {
        allPrefabs.Clear();
        selectedPrefab = null;

        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabRoot });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
                allPrefabs.Add(prefab);
        }

        allPrefabs.Sort((left, right) =>
        {
            var leftPath = AssetDatabase.GetAssetPath(left);
            var rightPath = AssetDatabase.GetAssetPath(right);
            return string.Compare(leftPath, rightPath, StringComparison.OrdinalIgnoreCase);
        });

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        filteredPrefabs.Clear();
        var filter = searchText?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(filter))
        {
            filteredPrefabs.AddRange(allPrefabs);
            return;
        }

        foreach (var prefab in allPrefabs)
        {
            if (prefab == null)
                continue;

            var path = AssetDatabase.GetAssetPath(prefab);
            var fileName = Path.GetFileNameWithoutExtension(path);
            if (ContainsIgnoreCase(prefab.name, filter)
                || ContainsIgnoreCase(fileName, filter)
                || ContainsIgnoreCase(path, filter))
            {
                filteredPrefabs.Add(prefab);
            }
        }
    }

    private static bool ContainsIgnoreCase(string value, string token)
    {
        return !string.IsNullOrEmpty(value)
               && !string.IsNullOrEmpty(token)
               && value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void ApplySelection()
    {
        if (target == null || selectedPrefab == null)
            return;

        var serializedObject = new SerializedObject(target);
        var property = serializedObject.FindProperty(propertyPath);
        if (property == null || property.propertyType != SerializedPropertyType.ObjectReference)
        {
            EditorUtility.DisplayDialog("Prefab Asset Picker", $"Property '{propertyPath}' was not found or is not an object reference.", "OK");
            return;
        }

        property.objectReferenceValue = selectedPrefab;
        if (serializedObject.ApplyModifiedPropertiesWithoutUndo())
        {
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Close();
    }
}
