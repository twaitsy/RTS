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

    protected override void ValidateDefinitions(List<CivilianDefinition> defs, System.Action<string> reportError)
    {
        foreach (var definition in defs)
        {
            if (definition == null)
                continue;

            foreach (var stat in definition.Stats.Entries)
            {
                if (!StatRegistry.Instance.TryGet(stat.StatId, out _))
                    reportError($"{definition.Id} references unknown base stat '{stat.StatId}'.");
            }
        }
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (StatRegistry.Instance == null)
            yield return "Missing dependency: StatRegistry.Instance is null.";
    }
}
