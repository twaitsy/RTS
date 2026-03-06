using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ValidationEditorAsmdefFixer
{
    private const string ValidationEditorAsmdefPath =
        "Assets/Scripts/Editor/Validation/Validation.Editor.asmdef";

    // Runtime types that editor validation scripts may depend on (bridge loading is direct and no longer asmdef-name based).
    private static readonly string[] RequiredRuntimeTypes =
    {
        "BuildingCategoryDefinition",
        "PrefabDefinition",
        "TaskDefinition",
        "TaskStepDefinition",
        "StateMachineDefinition",
        "StatDefinition",
        "StatModifierDefinition",
        "StatDomain",
        "IIdentifiable",
        "IStateMachineConditionContext",
        "ITaskEventSink"
    };

    [MenuItem("Tools/Validation/Sync Validation.Editor.asmdef References")]
    private static void FixAsmdef()
    {
        if (!File.Exists(ValidationEditorAsmdefPath))
        {
            Debug.LogError("Validation.Editor.asmdef not found.");
            return;
        }

        // Load the asmdef JSON
        var json = File.ReadAllText(ValidationEditorAsmdefPath);
        var asmdef = JsonUtility.FromJson<AsmdefData>(json);

        var allAsmdefs = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset")
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .ToList();

        var requiredAsmdefs = new HashSet<string>(asmdef.references);

        foreach (var typeName in RequiredRuntimeTypes)
        {
            var scriptPath = AssetDatabase.FindAssets(typeName)
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(path => path.EndsWith(".cs"));

            if (scriptPath == null)
                continue;

            var asmdefPath = FindOwningAsmdef(scriptPath, allAsmdefs);
            if (asmdefPath == null)
                continue;

            var asmdefName = Path.GetFileNameWithoutExtension(asmdefPath);

            if (!requiredAsmdefs.Contains(asmdefName))
            {
                requiredAsmdefs.Add(asmdefName);
                Debug.Log($"Added reference: {asmdefName}");
            }
        }

        asmdef.references = requiredAsmdefs.ToArray();

        // Save updated asmdef
        File.WriteAllText(ValidationEditorAsmdefPath, JsonUtility.ToJson(asmdef, true));
        AssetDatabase.Refresh();

        Debug.Log("Validation.Editor.asmdef references synced. This is optional maintenance for compile-time dependencies.");
    }

    private static string FindOwningAsmdef(string scriptPath, List<string> allAsmdefs)
    {
        var dir = Path.GetDirectoryName(scriptPath);

        while (!string.IsNullOrEmpty(dir))
        {
            var asmdef = allAsmdefs.FirstOrDefault(a => Path.GetDirectoryName(a) == dir);
            if (asmdef != null)
                return asmdef;

            dir = Path.GetDirectoryName(dir);
        }

        return null;
    }

    [System.Serializable]
    private class AsmdefData
    {
        public string name;
        public string[] references;
    }
}