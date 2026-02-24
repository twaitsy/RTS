using System.Collections.Generic;
using UnityEngine;

public class RequirementRegistry : DefinitionRegistry<RequirementDefinition>
{
    public static RequirementRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple RequirementRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override void ValidateDefinitions(List<RequirementDefinition> defs)
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
