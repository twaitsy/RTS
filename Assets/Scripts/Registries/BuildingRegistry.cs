using System.Collections.Generic;
using UnityEngine;

public class BuildingRegistry : DefinitionRegistry<BuildingDefinition>
{
    public static BuildingRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple BuildingRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override void ValidateDefinitions(List<BuildingDefinition> defs)
    {
        if (StatRegistry.Instance == null)
        {
            Debug.LogError("BuildingRegistry validation skipped: StatRegistry.Instance is null.");
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
        }
    }
}
