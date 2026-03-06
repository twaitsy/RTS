using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class DefinitionRegistry<T> : MonoBehaviour
    where T : ScriptableObject, IIdentifiable
{
    [SerializeField] protected List<T> definitions = new();

    protected Dictionary<string, T> lookup = new();
    private bool lookupDirty = true;


    protected virtual void Awake()
    {
        BuildLookup();
    }

    protected virtual void OnValidate()
    {
        lookupDirty = true;
    }

    protected void BuildLookup()
    {
#if UNITY_EDITOR
        LogEditorRegistryDrift();
#endif

        lookup.Clear();

        foreach (var def in definitions)
        {
            if (def == null)
            {
                Debug.LogError($"{GetType().Name} contains a null definition.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(def.Id))
            {
                Debug.LogError($"{def.name} has empty ID.");
                continue;
            }

            if (!lookup.TryAdd(def.Id, def))
                Debug.LogError($"Duplicate ID detected in {GetType().Name}: {def.Id}");
        }

        lookupDirty = false;
    }


    protected virtual RegistrySchema<T> GetSchema()
    {
        return null;
    }

    protected virtual IEnumerable<string> GetValidationDependencyErrors()
    {
        yield break;
    }

    protected virtual void ValidateDefinitions(List<T> defs, Action<string> reportError)
    {
        // Overridden in child registries
    }

    public T Get(string id)
    {
        if (lookupDirty)
            BuildLookup();

        if (lookup.TryGetValue(id, out var result))
            return result;

        Debug.LogError($"{GetType().Name} could not find definition with ID '{id}'.");
        return null;
    }

    public bool TryGet(string id, out T definition)
    {
        if (lookupDirty)
            BuildLookup();

        if (string.IsNullOrWhiteSpace(id))
        {
            definition = null;
            return false;
        }

        return lookup.TryGetValue(id, out definition);
    }

    public IReadOnlyList<T> GetDefinitions()
    {
        return definitions;
    }

#if UNITY_EDITOR
    private void LogEditorRegistryDrift()
    {
        if (!RegistrySyncUtilitySyncState.AllowDriftCheck)
            return;

        var assetGuids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        if (assetGuids.Length == definitions.Count)
            return;

        Debug.LogWarning(
            $"{GetType().Name} scene list appears stale. Scene count={definitions.Count}, asset database count={assetGuids.Length}.");
    }

    private static class RegistrySyncUtilitySyncState
    {
        public static bool AllowDriftCheck => !EditorApplication.isCompiling && !EditorApplication.isUpdating;
    }
#endif
}