using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public interface IDefinitionRegistryValidator
{
    string RegistryName { get; }
    void ValidateAll(DefinitionValidationReport report);
    void CollectReferenceMap(DefinitionReferenceMap map);
}

public abstract class DefinitionRegistry<T> : MonoBehaviour, IDefinitionRegistryValidator
    where T : ScriptableObject, IIdentifiable
{
    [SerializeField] protected List<T> definitions = new();

    protected Dictionary<string, T> lookup = new();
    private bool lookupDirty = true;

    public string RegistryName => GetType().Name;

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
            {
                Debug.LogError($"Duplicate ID detected in {GetType().Name}: {def.Id}");
            }
        }

        lookupDirty = false;
    }

    public void ValidateAll(DefinitionValidationReport report)
    {
        foreach (var dependencyError in GetValidationDependencyErrors())
            report.AddError(RegistryName, dependencyError);

        if (report.HasErrorsForRegistry(RegistryName))
            return;

        RegistrySchemaValidator.Validate(
            definitions,
            GetSchema(),
            definition => definition.name,
            definition => definition.Id,
            message => report.AddError(RegistryName, message));

        ValidateDefinitions(definitions, message => report.AddError(RegistryName, message));
    }

    public void CollectReferenceMap(DefinitionReferenceMap map)
    {
        if (map == null)
            return;

        foreach (var definition in definitions)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                continue;

            map.AddDefinition(RegistryName, definition.Id);
        }

        var schema = GetSchema();
        if (schema != null)
            CollectSchemaReferences(definitions, schema, map);

        CollectCustomReferences(definitions, map);
    }

    protected virtual RegistrySchema<T> GetSchema()
    {
        return null;
    }

    protected virtual void CollectCustomReferences(List<T> defs, DefinitionReferenceMap map)
    {
        // Overridden in child registries for complex/custom reference extraction.
    }

    private void CollectSchemaReferences(List<T> defs, RegistrySchema<T> schema, DefinitionReferenceMap map)
    {
        foreach (var definition in defs)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                continue;

            foreach (var rule in schema.ReferenceRules)
            {
                var ids = rule.GetReferenceIds(definition);
                if (ids == null)
                    continue;

                foreach (var targetId in ids)
                {
                    if (string.IsNullOrWhiteSpace(targetId))
                        continue;

                    var matchedTargetCount = 0;

                    foreach (var target in rule.AllowedTargets)
                    {
                        if (target.TargetExists != null && target.TargetExists(targetId))
                        {
                            map.AddReference(RegistryName, definition.Id, rule.FieldName, target.TargetName, targetId);
                            matchedTargetCount++;
                        }
                    }

                    if (matchedTargetCount > 0 || rule.AllowedTargets.Count == 0)
                        continue;

                    foreach (var target in rule.AllowedTargets)
                        map.AddReference(RegistryName, definition.Id, rule.FieldName, target.TargetName, targetId);
                }
            }
        }
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

#if UNITY_EDITOR
    private void LogEditorRegistryDrift()
    {
        if (!RegistrySyncUtilitySyncState.AllowDriftCheck)
            return;

        var assetGuids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        if (assetGuids.Length == definitions.Count)
            return;

        Debug.LogWarning($"{GetType().Name} scene list appears stale. Scene count={definitions.Count}, asset database count={assetGuids.Length}.");
    }

    private static class RegistrySyncUtilitySyncState
    {
        public static bool AllowDriftCheck => !EditorApplication.isCompiling && !EditorApplication.isUpdating;
    }
#endif
}
