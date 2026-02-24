using System.Collections.Generic;
using UnityEngine;

public class WeaponTypeRegistry : DefinitionRegistry<WeaponTypeDefinition>
{
    public static WeaponTypeRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple WeaponTypeRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override void ValidateDefinitions(List<WeaponTypeDefinition> defs)
    {
        if (StatRegistry.Instance == null)
        {
            Debug.LogError("WeaponTypeRegistry validation skipped: StatRegistry.Instance is null.");
            return;
        }

        foreach (var definition in defs)
        {
            if (definition == null)
                continue;

            foreach (var stat in definition.Stats.Entries)
            {
                if (!StatRegistry.Instance.TryGet(stat.StatId, out _))
                    Debug.LogError($"{definition.Id} references unknown base stat '{stat.StatId}'.");
            }

            foreach (var modifier in definition.StatModifiers)
            {
                if (!StatRegistry.Instance.TryGet(modifier.targetStatId, out _))
                    Debug.LogError($"{definition.Id} references unknown targetStatId '{modifier.targetStatId}'.");
            }
        }
    }
}
