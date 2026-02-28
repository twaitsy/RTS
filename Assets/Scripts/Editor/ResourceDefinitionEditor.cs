#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ResourceDefinition))]
public class ResourceDefinitionEditor : Editor
{
    private SerializedProperty idProp;
    private SerializedProperty displayNameProp;
    private SerializedProperty iconProp;
    private SerializedProperty statsProp;
    private SerializedProperty modifiersProp;

    private void OnEnable()
    {
        idProp = serializedObject.FindProperty("id");
        displayNameProp = serializedObject.FindProperty("displayName");
        iconProp = serializedObject.FindProperty("icon");
        statsProp = serializedObject.FindProperty("stats");
        modifiersProp = serializedObject.FindProperty("statModifiers");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Identity", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(idProp);
        EditorGUILayout.PropertyField(displayNameProp);
        EditorGUILayout.PropertyField(iconProp);

        EditorGUILayout.Space(8);
        StatContainerInspectorUtility.DrawStatsSection(
            serializedObject,
            "stats",
            "ResourceDefinition_Stats",
            "Stats"
        );

        EditorGUILayout.Space(8);
        StatModifierInspectorUtility.DrawModifiersSection(
            serializedObject,
            "statModifiers",
            "Modifiers"
        );

        serializedObject.ApplyModifiedProperties();
    }
}
#endif