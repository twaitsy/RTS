using System.Collections.Generic;
using UnityEngine;

public class CivilianRegistry : DefinitionRegistry<CivilianDefinition>
{
    public static CivilianRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple CivilianRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override void ValidateDefinitions(List<CivilianDefinition> defs)
    {
        if (StatRegistry.Instance == null)
        {
            Debug.LogError("CivilianRegistry validation skipped: StatRegistry.Instance is null.");
            return;
        }

        foreach (var definition in defs)
        {
            if (definition == null)
                continue;

            foreach (var stat in definition.BaseStats)
            {
                if (!StatRegistry.Instance.TryGet(stat.StatId, out _))
                    Debug.LogError($"{definition.Id} references unknown base stat '{stat.StatId}'.");
            }
        }
    }
}
