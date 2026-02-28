#if UNITY_EDITOR
using UnityEditor;

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
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, "m_Script", "stats");
        StatContainerInspectorUtility.DrawStatsSection(serializedObject, "stats", target.GetInstanceID() + ".building");
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
