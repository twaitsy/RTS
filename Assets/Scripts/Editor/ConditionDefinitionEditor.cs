#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ConditionDefinition))]
public class ConditionDefinitionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("id"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Condition Tree", EditorStyles.boldLabel);

        // Draw the root using the PropertyDrawer
        EditorGUILayout.PropertyField(serializedObject.FindProperty("root"), true);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif