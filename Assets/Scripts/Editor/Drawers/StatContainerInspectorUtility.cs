using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public static class StatContainerInspectorUtility
{
    public static void DrawStatsSection(SerializedObject obj, string propertyPath, string keyPrefix)
    {
        if (obj == null || string.IsNullOrEmpty(propertyPath))
            return;

        bool statsChanged = false;
        bool requiresDirty = false;

        SerializedProperty rootProp = obj.FindProperty(propertyPath);
        if (rootProp == null)
        {
            EditorGUILayout.HelpBox($"Property '{propertyPath}' not found.", MessageType.Warning);
            return;
        }

        SerializedProperty arrayProp = rootProp.isArray ? rootProp : FindFirstChildArray(rootProp);
        if (arrayProp == null)
        {
            EditorGUILayout.HelpBox($"No array found at '{propertyPath}' or inside it.", MessageType.Warning);
            return;
        }

        string[] statIds = GetAllStatIdsForEditor();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);

        var targetObject = obj.targetObject;

        for (int i = 0; i < arrayProp.arraySize; i++)
        {
            SerializedProperty entry = arrayProp.GetArrayElementAtIndex(i);
            SerializedProperty idProp = entry.FindPropertyRelative("StatId") ??
                                        entry.FindPropertyRelative("id") ??
                                        entry.FindPropertyRelative("statId");

            SerializedProperty valueProp = entry.FindPropertyRelative("Value") ??
                                           entry.FindPropertyRelative("value");

            EditorGUILayout.BeginVertical("box");

            if (idProp == null)
            {
                EditorGUILayout.HelpBox("Stat entry missing an 'id' field (StatId / id / statId).", MessageType.Warning);
            }
            else if (statIds.Length == 0)
            {
                EditorGUILayout.HelpBox("No stat definitions found in the project or registry.", MessageType.Warning);
            }
            else
            {
                int currentIndex = Array.IndexOf(statIds, idProp.stringValue);
                if (currentIndex < 0) currentIndex = 0;

                EditorGUI.BeginChangeCheck();
                int newIndex = EditorGUILayout.Popup("Stat", currentIndex, statIds);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(targetObject, "Change Stat Id");
                    idProp.stringValue = statIds[newIndex];
                    statsChanged = true;
                    requiresDirty = true;
                }
            }

            if (valueProp != null)
                EditorGUILayout.PropertyField(valueProp, new GUIContent("Value"));
            else
                EditorGUILayout.HelpBox("Stat entry missing a 'value' field.", MessageType.Warning);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Remove"))
            {
                Undo.RecordObject(targetObject, "Remove Stat Entry");
                arrayProp.DeleteArrayElementAtIndex(i);
                statsChanged = true;
                requiresDirty = true;
                break;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Add Stat"))
        {
            Undo.RecordObject(targetObject, "Add Stat Entry");
            arrayProp.InsertArrayElementAtIndex(arrayProp.arraySize);

            SerializedProperty newEntry = arrayProp.GetArrayElementAtIndex(arrayProp.arraySize - 1);
            SerializedProperty idField = newEntry.FindPropertyRelative("StatId") ??
                                         newEntry.FindPropertyRelative("id") ??
                                         newEntry.FindPropertyRelative("statId");

            SerializedProperty valueField = newEntry.FindPropertyRelative("Value") ??
                                            newEntry.FindPropertyRelative("value");

            if (idField != null)
                idField.stringValue = statIds.Length > 0 ? statIds[0] : "";

            if (valueField != null)
            {
                switch (valueField.propertyType)
                {
                    case SerializedPropertyType.Float: valueField.floatValue = 0f; break;
                    case SerializedPropertyType.Integer: valueField.intValue = 0; break;
                }
            }

            statsChanged = true;
            requiresDirty = true;
        }

        if (statsChanged)
        {
            obj.ApplyModifiedProperties();

            if (requiresDirty)
                EditorUtility.SetDirty(targetObject);
        }
    }

    private static SerializedProperty FindFirstChildArray(SerializedProperty root)
    {
        var iter = root.Copy();
        var end = iter.GetEndProperty();

        if (!iter.NextVisible(true))
            return null;

        while (!SerializedProperty.EqualContents(iter, end))
        {
            if (iter.isArray)
                return iter;

            if (!iter.NextVisible(false))
                break;
        }

        return null;
    }

    private static string[] GetAllStatIdsForEditor()
    {
#if UNITY_EDITOR
        try
        {
            // 1. Try registry first
            var registry = StatRegistry.Instance;
            if (registry != null)
            {
                var defs = registry.GetDefinitions();
                if (defs != null && defs.Count > 0)
                {
                    var ids = defs
                        .Where(d => d != null && !string.IsNullOrWhiteSpace(d.Id))
                        .Select(d => d.Id)
                        .ToList();

                    if (ids.Count > 0)
                        return DeduplicateAndSort(ids);
                }
            }

            // 2. Fallback to asset database
            var guids = AssetDatabase.FindAssets("t:StatDefinition");
            if (guids != null && guids.Length > 0)
            {
                var ids = new List<string>(guids.Length);

                foreach (var g in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(g);
                    if (string.IsNullOrEmpty(path))
                        continue;

                    var def = AssetDatabase.LoadAssetAtPath<StatDefinition>(path);
                    if (def == null)
                        continue;

                    if (!string.IsNullOrWhiteSpace(def.Id))
                        ids.Add(def.Id);
                }

                if (ids.Count > 0)
                    return DeduplicateAndSort(ids);
            }

            return Array.Empty<string>();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error resolving stat IDs for editor: {ex}");
            return Array.Empty<string>();
        }
#else
        return Array.Empty<string>();
#endif
    }

    private static string[] DeduplicateAndSort(IEnumerable<string> ids)
    {
        var set = new HashSet<string>(ids.Where(s => !string.IsNullOrWhiteSpace(s)), StringComparer.Ordinal);
        var arr = set.ToArray();
        Array.Sort(arr, StringComparer.Ordinal);
        return arr;
    }
}
