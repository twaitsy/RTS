using System.Collections.Generic;
using UnityEngine;

public class TechRegistry : DefinitionRegistry<TechDefinition>
{
    private static readonly HashSet<StatDomain> AnyDomain = new();

    public static TechRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple TechRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override void ValidateDefinitions(List<TechDefinition> defs)
    {
        if (StatModifierRegistry.Instance == null || StatRegistry.Instance == null)
        {
            Debug.LogError("TechRegistry validation skipped: StatModifierRegistry or StatRegistry instance is null.");
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
