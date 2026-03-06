using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ResourceNodeRuntime))]
public class ResourceNodeRuntimeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var runtime = (ResourceNodeRuntime)target;
        var def = runtime.Definition;

        if (def == null)
        {
            EditorGUILayout.HelpBox("No ResourceNodeDefinition assigned.", MessageType.Info);
            return;
        }

        if (GUILayout.Button("Sync Interaction Points → Definition"))
        {
            SyncResourceNode(runtime, def);
        }
    }

    private void SyncResourceNode(ResourceNodeRuntime runtime, ResourceNodeDefinition def)
    {
        var points = runtime.InteractionPoints;
        if (points == null || points.Count == 0)
        {
            Debug.LogWarning($"[{runtime.name}] No InteractionPoints assigned.");
            return;
        }

        SerializedObject so = new SerializedObject(def);
        SerializedProperty listProp = so.FindProperty("interactionPoints");

        if (listProp == null)
        {
            Debug.LogError($"Definition '{def.name}' has no 'interactionPoints' field.");
            return;
        }

        listProp.ClearArray();

        for (int i = 0; i < points.Count; i++)
        {
            if (points[i] == null)
                continue;

            listProp.InsertArrayElementAtIndex(i);
            Vector3 localOffset = runtime.transform.InverseTransformPoint(points[i].position);
            listProp.GetArrayElementAtIndex(i).vector3Value = localOffset;
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(def);
        AssetDatabase.SaveAssets();

        Debug.Log($"[{runtime.name}] Synced {points.Count} interaction point(s) to definition '{def.name}'.");
    }
}

[CustomEditor(typeof(BuildingRuntime))]
public class BuildingRuntimeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var runtime = (BuildingRuntime)target;
        var def = runtime.Definition;

        if (def == null)
        {
            EditorGUILayout.HelpBox("No BuildingDefinition assigned.", MessageType.Info);
            return;
        }

        if (GUILayout.Button("Sync Interaction Points → Definition"))
        {
            SyncBuilding(runtime, def);
        }
    }

    private void SyncBuilding(BuildingRuntime runtime, BuildingDefinition def)
    {
        var points = runtime.InteractionPoints;
        if (points == null || points.Count == 0)
        {
            Debug.LogWarning($"[{runtime.name}] No InteractionPoints assigned.");
            return;
        }

        SerializedObject so = new SerializedObject(def);
        SerializedProperty listProp = so.FindProperty("interactionPoints");

        if (listProp == null)
        {
            Debug.LogError($"Definition '{def.name}' has no 'interactionPoints' field.");
            return;
        }

        listProp.ClearArray();

        for (int i = 0; i < points.Count; i++)
        {
            if (points[i] == null)
                continue;

            listProp.InsertArrayElementAtIndex(i);
            Vector3 localOffset = runtime.transform.InverseTransformPoint(points[i].position);
            listProp.GetArrayElementAtIndex(i).vector3Value = localOffset;
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(def);
        AssetDatabase.SaveAssets();

        Debug.Log($"[{runtime.name}] Synced {points.Count} interaction point(s) to definition '{def.name}'.");
    }
}
