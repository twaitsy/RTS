using System.Collections.Generic;
using UnityEngine;

public class EffectRegistry : DefinitionRegistry<EffectDefinition>
{
    private static readonly HashSet<StatDomain> AnyDomain = new();

    public static EffectRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple EffectRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override void ValidateDefinitions(List<EffectDefinition> defs)
    {
        if (StatModifierRegistry.Instance == null || StatRegistry.Instance == null)
        {
            Debug.LogError("EffectRegistry validation skipped: StatModifierRegistry or StatRegistry instance is null.");
            return;
        }

        StatModifierLinkValidator.ValidateHostStatModifierLinks(
            defs,
            definition => definition.Id,
            definition => definition.StatModifierIds,
            definition => definition.name,
            _ => true,
            modifierId => StatModifierRegistry.Instance.TryGet(modifierId, out _),
            modifierId => StatModifierRegistry.Instance.Get(modifierId),
            statId => StatRegistry.Instance.TryGet(statId, out _),
            statId => StatRegistry.Instance.Get(statId),
            AnyDomain,
            "any domain",
            Debug.LogError);
    }
}
