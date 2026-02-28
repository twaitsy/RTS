using System;
using System.Collections.Generic;
using UnityEngine;

public interface IDefinitionRegistryValidator
{
    string RegistryName { get; }
    void ValidateAll(DefinitionValidationReport report);
}

public abstract class DefinitionRegistry<T> : MonoBehaviour, IDefinitionRegistryValidator
    where T : ScriptableObject, IIdentifiable
{
    [SerializeField] protected List<T> definitions = new();

    protected Dictionary<string, T> lookup = new();

    public string RegistryName => GetType().Name;

    protected virtual void Awake()
    {
        BuildLookup();
    }

    private void BuildLookup()
    {
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
        if (lookup.TryGetValue(id, out var result))
            return result;

        Debug.LogError($"{GetType().Name} could not find definition with ID '{id}'.");
        return null;
    }

    public bool TryGet(string id, out T definition)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            definition = null;
            return false;
        }

        return lookup.TryGetValue(id, out definition);
    }
}
