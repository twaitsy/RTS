using System.Collections.Generic;
using UnityEngine;

public class ModifierGroupRegistry : DefinitionRegistry<ModifierGroupDefinition>
{
    public static ModifierGroupRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple ModifierGroupRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override void ValidateDefinitions(List<ModifierGroupDefinition> defs, System.Action<string> reportError)
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
