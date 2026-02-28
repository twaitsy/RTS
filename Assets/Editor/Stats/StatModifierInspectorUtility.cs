#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class StatModifierInspectorUtility
{
    public static void DrawModifiersSection(SerializedObject serializedObject, string propertyName, string title = "Modifiers")
    {
        var modifiers = serializedObject.FindProperty(propertyName);
        if (modifiers == null)
            return;

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

        for (int i = 0; i < modifiers.arraySize; i++)
        {
            var element = modifiers.GetArrayElementAtIndex(i);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(element, GUIContent.none, true);

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Remove", GUILayout.Width(80)))
                    {
                        modifiers.DeleteArrayElementAtIndex(i);
                        break;
                    }
                }
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add Modifier", GUILayout.Width(120)))
            {
                modifiers.InsertArrayElementAtIndex(modifiers.arraySize);
            }
        }
    }
}
#endif