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

    protected override void ValidateDefinitions(List<ConditionDefinition> defs, System.Action<string> reportError)
    {
        var ids = new HashSet<string>();

        foreach (var def in defs)
        {
            if (string.IsNullOrWhiteSpace(def.Id))
                reportError($"{def.name} has empty ID.");

            if (!ids.Add(def.Id))
                reportError($"Duplicate ID detected: {def.Id}");
        }
    }
}
