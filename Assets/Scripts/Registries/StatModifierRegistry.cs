using System.Collections.Generic;
using UnityEngine;

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

        StatModifierLinkValidator.ValidateStatModifierDefinitions(
            defs,
            statId => StatRegistry.Instance.TryGet(statId, out _),
            Debug.LogError);
    }
}
