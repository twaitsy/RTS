using UnityEngine;
using System.Collections.Generic;

public class StatModifierRegistry : DefinitionRegistry<StatModifierDefinition>
{
    public static StatModifierRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple StatModifierRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override void ValidateDefinitions(List<StatModifierDefinition> defs)
    {
        if (StatRegistry.Instance == null)
        {
            Debug.LogError("StatModifierRegistry validation skipped: StatRegistry.Instance is null.");
            return;
        }

        foreach (var definition in defs)
        {
            if (definition == null)
                continue;

            foreach (var modifier in definition.Modifiers)
            {
                if (string.IsNullOrWhiteSpace(modifier.targetStatId))
                {
                    Debug.LogError($"{definition.Id} contains a stat modifier with an empty targetStatId.");
                    continue;
                }

                if (!StatRegistry.Instance.TryGet(modifier.targetStatId, out _))
                    Debug.LogError($"{definition.Id} references unknown targetStatId '{modifier.targetStatId}'.");
            }
        }
    }
}
