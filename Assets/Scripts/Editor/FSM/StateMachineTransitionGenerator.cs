#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class StateMachineTransitionGenerator
{
    private const string MenuPath = "Tools/FSM/Sync Runtime Transitions from Definitions";

    private static bool generationQueued;

    static StateMachineTransitionGenerator()
    {
        QueueGenerateAll("domain reload");
    }

    [MenuItem(MenuPath)]
    public static void GenerateAllFromMenu()
    {
        GenerateAll("manual menu");
    }

    public static void QueueGenerateAll(string reason)
    {
        if (generationQueued)
            return;

        generationQueued = true;
        EditorApplication.delayCall += () =>
        {
            generationQueued = false;
            GenerateAll(reason);
        };
    }

    public static void GenerateAll(string reason)
    {
        var definitionsById = CollectDefinitionsById();
        var machineIdsWithTransitions = CollectMachineIdsWithRuntimeTransitions();

        var touchedAssets = SyncUnitBrainsWithDefinitions(definitionsById, machineIdsWithTransitions);

        if (touchedAssets > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[FSM Runtime Transition Sync] {touchedAssets} UnitBrain asset changes ({reason}).");
        }
    }

    private static Dictionary<string, StateMachineDefinition> CollectDefinitionsById()
    {
        var machineGuids = AssetDatabase.FindAssets("t:StateMachineDefinition");
        var definitionsById = new Dictionary<string, StateMachineDefinition>(StringComparer.Ordinal);

        foreach (var guid in machineGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var definition = AssetDatabase.LoadAssetAtPath<StateMachineDefinition>(path);
            if (definition == null)
                continue;

            var machineId = ResolveMachineId(definition);
            if (string.IsNullOrWhiteSpace(machineId) || definitionsById.ContainsKey(machineId))
                continue;

            definitionsById[machineId] = definition;
        }

        return definitionsById;
    }

    private static HashSet<string> CollectMachineIdsWithRuntimeTransitions()
    {
        var machineGuids = AssetDatabase.FindAssets("t:StateMachineDefinition");
        var machineIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var guid in machineGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var definition = AssetDatabase.LoadAssetAtPath<StateMachineDefinition>(path);
            if (definition == null)
                continue;

            var machineId = ResolveMachineId(definition);
            if (string.IsNullOrWhiteSpace(machineId))
                continue;

            foreach (var entry in definition.Transitions)
            {
                if (string.IsNullOrWhiteSpace(entry.fromStateId) || string.IsNullOrWhiteSpace(entry.toStateId))
                    continue;

                machineIds.Add(machineId);
                break;
            }
        }

        return machineIds;
    }

    private static int SyncUnitBrainsWithDefinitions(
        IReadOnlyDictionary<string, StateMachineDefinition> definitionsById,
        HashSet<string> machineIdsWithRuntimeTransitions)
    {
        var changed = 0;
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab");

        foreach (var guid in prefabGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefabRoot == null)
                continue;

            var unitBrains = prefabRoot.GetComponentsInChildren<UnitBrain>(true);
            var prefabDirty = false;

            foreach (var unitBrain in unitBrains)
            {
                if (unitBrain == null)
                    continue;

                var resolvedDefinition = unitBrain.MachineDefinition;
                if (resolvedDefinition == null && !string.IsNullOrWhiteSpace(unitBrain.MachineDefinitionId))
                    definitionsById.TryGetValue(unitBrain.MachineDefinitionId, out resolvedDefinition);

                if (resolvedDefinition == null)
                    continue;

                var resolvedMachineId = ResolveMachineId(resolvedDefinition);
                if (string.IsNullOrWhiteSpace(resolvedMachineId) || !machineIdsWithRuntimeTransitions.Contains(resolvedMachineId))
                    continue;

                if (string.Equals(unitBrain.MachineDefinitionId, resolvedMachineId, StringComparison.Ordinal))
                    continue;

                unitBrain.MachineDefinitionId = resolvedMachineId;
                EditorUtility.SetDirty(unitBrain);
                prefabDirty = true;
                changed++;
            }

            if (prefabDirty)
                EditorUtility.SetDirty(prefabRoot);
        }

        return changed;
    }

    private static string ResolveMachineId(StateMachineDefinition definition)
    {
        if (definition == null)
            return null;

        return string.IsNullOrWhiteSpace(definition.Id) ? definition.name : definition.Id;
    }
}

public sealed class StateMachineTransitionGeneratorPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (!ContainsStateMachineDefinitionChange(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths))
            return;

        StateMachineTransitionGenerator.QueueGenerateAll("state machine definition asset change");
    }

    private static bool ContainsStateMachineDefinitionChange(params string[][] changeSets)
    {
        foreach (var changeSet in changeSets)
        {
            if (changeSet == null)
                continue;

            foreach (var path in changeSet)
            {
                if (string.IsNullOrWhiteSpace(path) || !path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                    continue;

                var mainType = AssetDatabase.GetMainAssetTypeAtPath(path);
                if (mainType == typeof(StateMachineDefinition))
                    return true;

                if (mainType == null && path.Contains("/Definitions/", StringComparison.Ordinal) && path.Contains("StateMachine", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }

        return false;
    }
}
#endif
