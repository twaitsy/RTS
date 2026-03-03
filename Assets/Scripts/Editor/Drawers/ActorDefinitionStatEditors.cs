#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnitDefinition))]
public class UnitDefinitionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, "m_Script", "stats");
        StatContainerInspectorUtility.DrawStatsSection(serializedObject, "stats", target.GetInstanceID() + ".unit");
        serializedObject.ApplyModifiedProperties();
    }
}

[CustomEditor(typeof(CivilianDefinition))]
public class CivilianDefinitionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, "m_Script", "stats");
        StatContainerInspectorUtility.DrawStatsSection(serializedObject, "stats", target.GetInstanceID() + ".civilian");
        serializedObject.ApplyModifiedProperties();
    }
}

[CustomEditor(typeof(BuildingDefinition))]
public class BuildingDefinitionEditor : Editor
{
    private static readonly GUIContent SecondaryCategoriesLabel = new("Secondary Categories");

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "m_Script", "stats", "primaryCategoryId", "secondaryCategoryIds");
        DrawCategorySection(serializedObject);

        StatContainerInspectorUtility.DrawStatsSection(serializedObject, "stats", target.GetInstanceID() + ".building");
        serializedObject.ApplyModifiedProperties();
    }

    private static void DrawCategorySection(SerializedObject serializedObject)
    {
        var primaryCategoryId = serializedObject.FindProperty("primaryCategoryId");
        var secondaryCategoryIds = serializedObject.FindProperty("secondaryCategoryIds");
        if (primaryCategoryId == null || secondaryCategoryIds == null)
            return;

        SanitizeSerializedStringArray(secondaryCategoryIds);
        var categories = LoadCategories();
        DrawPrimaryCategoryField(primaryCategoryId, categories);
        DrawSecondaryCategoryField(secondaryCategoryIds, categories);
    }

    private static void DrawPrimaryCategoryField(SerializedProperty primaryCategoryId, IReadOnlyList<BuildingCategoryDefinition> categories)
    {
        var categoryOptions = new GUIContent[categories.Count + 1];
        categoryOptions[0] = new GUIContent("<None>");

        var selectedIndex = 0;
        for (var index = 0; index < categories.Count; index++)
        {
            var optionIndex = index + 1;
            var category = categories[index];
            categoryOptions[optionIndex] = BuildCategoryContent(category);

            if (string.Equals(primaryCategoryId.stringValue, category.Id, StringComparison.Ordinal))
                selectedIndex = optionIndex;
        }

        var nextIndex = EditorGUILayout.Popup(new GUIContent("Primary Category"), selectedIndex, categoryOptions);
        primaryCategoryId.stringValue = nextIndex <= 0 ? string.Empty : categories[nextIndex - 1].Id;
    }

    private static void DrawSecondaryCategoryField(SerializedProperty secondaryCategoryIds, IReadOnlyList<BuildingCategoryDefinition> categories)
    {
        EditorGUILayout.LabelField(SecondaryCategoriesLabel);
        EditorGUI.indentLevel++;

        for (var index = 0; index < categories.Count; index++)
        {
            var category = categories[index];
            var isSelected = SerializedArrayContains(secondaryCategoryIds, category.Id);
            var nextSelected = EditorGUILayout.ToggleLeft(BuildCategoryContent(category), isSelected);
            if (nextSelected == isSelected)
                continue;

            if (nextSelected)
                SerializedArrayAddUnique(secondaryCategoryIds, category.Id);
            else
                SerializedArrayRemoveAll(secondaryCategoryIds, category.Id);
        }

        EditorGUI.indentLevel--;
    }

    private static List<BuildingCategoryDefinition> LoadCategories()
    {
        var categories = LoadCategoriesFromRegistryOrAssets();
        return categories
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.DisplayName, StringComparer.Ordinal)
            .ToList();
    }

    private static IEnumerable<BuildingCategoryDefinition> LoadCategoriesFromRegistryOrAssets()
    {
        var registry = BuildingCategoryRegistry.Instance;
        if (registry != null)
            return registry.GetDefinitions().Where(category => category != null);

        return AssetDatabase
            .FindAssets($"t:{nameof(BuildingCategoryDefinition)}")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(path => AssetDatabase.LoadAssetAtPath<BuildingCategoryDefinition>(path))
            .Where(category => category != null);
    }

    private static GUIContent BuildCategoryContent(BuildingCategoryDefinition category)
    {
        var displayName = string.IsNullOrWhiteSpace(category.DisplayName) ? category.Id : category.DisplayName;
        var orderSuffix = $" ({category.SortOrder})";
        return new GUIContent($"{ColorUtility.ToHtmlStringRGB(category.Color)} • {displayName}{orderSuffix}", category.Icon);
    }

    private static bool SerializedArrayContains(SerializedProperty arrayProperty, string value)
    {
        for (var index = 0; index < arrayProperty.arraySize; index++)
        {
            if (string.Equals(arrayProperty.GetArrayElementAtIndex(index).stringValue, value, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private static void SerializedArrayAddUnique(SerializedProperty arrayProperty, string value)
    {
        if (SerializedArrayContains(arrayProperty, value))
            return;

        var index = arrayProperty.arraySize;
        arrayProperty.InsertArrayElementAtIndex(index);
        arrayProperty.GetArrayElementAtIndex(index).stringValue = value;
    }

    private static void SerializedArrayRemoveAll(SerializedProperty arrayProperty, string value)
    {
        for (var index = arrayProperty.arraySize - 1; index >= 0; index--)
        {
            if (!string.Equals(arrayProperty.GetArrayElementAtIndex(index).stringValue, value, StringComparison.Ordinal))
                continue;

            arrayProperty.DeleteArrayElementAtIndex(index);
        }
    }

    private static void SanitizeSerializedStringArray(SerializedProperty arrayProperty)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);

        for (var index = arrayProperty.arraySize - 1; index >= 0; index--)
        {
            var value = arrayProperty.GetArrayElementAtIndex(index).stringValue;
            if (string.IsNullOrWhiteSpace(value) || !seen.Add(value))
                arrayProperty.DeleteArrayElementAtIndex(index);
        }
    }
}
#endif
