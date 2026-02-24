using System.Collections.Generic;
using UnityEngine;

public abstract class DefinitionRegistry<T> : MonoBehaviour
    where T : ScriptableObject, IIdentifiable
{
    [SerializeField] protected List<T> definitions = new();

    protected Dictionary<string, T> lookup = new();

    protected virtual void Awake()
    {
        BuildLookup();
        ValidateDefinitions(definitions);
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

    public T Get(string id)
    {
        if (lookup.TryGetValue(id, out var result))
            return result;

        Debug.LogError($"{GetType().Name} could not find definition with ID '{id}'.");
        return null;
    }

    protected virtual void ValidateDefinitions(List<T> defs)
    {
        // Overridden in child registries
    }
}