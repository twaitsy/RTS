using System.Collections.Generic;
using UnityEngine;

public class ConditionRegistry : DefinitionRegistry<ConditionDefinition>
{
    public static ConditionRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple ConditionRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override void ValidateDefinitions(List<ConditionDefinition> defs)
    {
        var ids = new HashSet<string>();

        foreach (var def in defs)
        {
            if (string.IsNullOrWhiteSpace(def.Id))
                Debug.LogError($"{def.name} has empty ID.");

            if (!ids.Add(def.Id))
                Debug.LogError($"Duplicate ID detected: {def.Id}");
        }
    }
}
