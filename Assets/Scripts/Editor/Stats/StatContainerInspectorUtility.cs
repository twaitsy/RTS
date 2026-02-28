#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class StatContainerInspectorUtility
{
    private sealed class StatDefinitionLookup
    {
        public readonly Dictionary<string, StatDefinition> ById = new(StringComparer.Ordinal);
        public readonly Dictionary<StatDomain, List<StatDefinition>> ByDomain = new();
    }

    private static readonly Dictionary<string, string> SearchByKey = new(StringComparer.Ordinal);

    public static void DrawStatsSection(SerializedObject serializedObject, string propertyName, string stateKey, string title = "Stats")
    {
        var statsContainer = serializedObject.FindProperty(propertyName);
        if (statsContainer == null)
            return;

        var entries = statsContainer.FindPropertyRelative("entries");
        if (entries == null)
            return;

        var lookup = BuildLookup();

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        DrawSearchField(stateKey);
        DrawGroupedEntries(entries, lookup, GetSearch(stateKey));

        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add Stat", GUILayout.Width(110f)))
            {
                var index = entries.arraySize;
                entries.InsertArrayElementAtIndex(index);
                var entry = entries.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("statId").stringValue = string.Empty;
                entry.FindPropertyRelative("value").floatValue = 0f;
            }
        }
    }

    private static StatDefinitionLookup BuildLookup()
    {
        var lookup = new StatDefinitionLookup();
        var guids = AssetDatabase.FindAssets("t:StatDefinition");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var stat = AssetDatabase.LoadAssetAtPath<StatDefinition>(path);
            if (stat == null || string.IsNullOrWhiteSpace(stat.Id))
                continue;

            lookup.ById[stat.Id] = stat;
            if (!lookup.ByDomain.TryGetValue(stat.Domain, out var list))
            {
                list = new List<StatDefinition>();
                lookup.ByDomain[stat.Domain] = list;
            }

            list.Add(stat);
        }

        return lookup;
    }

    private static void DrawSearchField(string stateKey)
    {
        using var scope = new EditorGUILayout.HorizontalScope();
        EditorGUILayout.LabelField("Search", GUILayout.Width(50f));
        var next = EditorGUILayout.TextField(GetSearch(stateKey));
        if (next != GetSearch(stateKey))
            SearchByKey[stateKey] = next;
    }

    private static string GetSearch(string key)
    {
        return SearchByKey.TryGetValue(key, out var value) ? value : string.Empty;
    }

    private static void DrawGroupedEntries(SerializedProperty entries, StatDefinitionLookup lookup, string search)
    {
        var groups = new Dictionary<string, List<int>>(StringComparer.Ordinal);

        for (int i = 0; i < entries.arraySize; i++)
        {
            var entry = entries.GetArrayElementAtIndex(i);
            var statId = entry.FindPropertyRelative("statId").stringValue;
            if (!MatchesSearch(statId, lookup, search))
                continue;

            var key = ResolveGroupKey(statId, lookup);
            if (!groups.TryGetValue(key, out var list))
            {
                list = new List<int>();
                groups[key] = list;
            }

            list.Add(i);
        }

        if (groups.Count == 0)
        {
            EditorGUILayout.HelpBox("No stats match the current search.", MessageType.Info);
            return;
        }

        foreach (var pair in groups)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField(pair.Key, EditorStyles.miniBoldLabel);
            foreach (var index in pair.Value)
                DrawEntry(entries, index, lookup);
        }
    }

    private static void DrawEntry(SerializedProperty entries, int index, StatDefinitionLookup lookup)
    {
        var entry = entries.GetArrayElementAtIndex(index);
        var statId = entry.FindPropertyRelative("statId");
        var value = entry.FindPropertyRelative("value");

        lookup.ById.TryGetValue(statId.stringValue, out var stat);

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(statId, GUIContent.none);
                EditorGUILayout.PropertyField(value, GUIContent.none, GUILayout.MinWidth(80f));

                if (stat != null && !string.IsNullOrWhiteSpace(stat.Unit))
                    GUILayout.Label(stat.Unit, GUILayout.Width(55f));
                else
                    GUILayout.Space(55f);

                if (GUILayout.Button("X", GUILayout.Width(24f)))
                    entries.DeleteArrayElementAtIndex(index);
            }

            if (stat != null)
            {
                EditorGUILayout.LabelField($"{stat.DisplayName} ({stat.Id})", EditorStyles.miniLabel);
            }
            else if (!string.IsNullOrWhiteSpace(statId.stringValue))
            {
                EditorGUILayout.HelpBox($"Unknown stat ID '{statId.stringValue}'.", MessageType.Warning);
            }
        }
    }

    private static string ResolveGroupKey(string statId, StatDefinitionLookup lookup)
    {
        if (lookup.ById.TryGetValue(statId, out var stat))
            return stat.Domain.ToString();

        return "Unknown Domain";
    }

    private static bool MatchesSearch(string statId, StatDefinitionLookup lookup, string search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return true;

        if (ContainsIgnoreCase(statId, search))
            return true;

        if (!lookup.ById.TryGetValue(statId, out var stat))
            return false;

        return ContainsIgnoreCase(stat.DisplayName, search);
    }

    private static bool ContainsIgnoreCase(string value, string term)
    {
        return !string.IsNullOrWhiteSpace(value)
               && value.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
#endif
