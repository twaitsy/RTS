using System.Collections.Generic;
using UnityEngine;

public class DiseaseRegistry : DefinitionRegistry<DiseaseDefinition>
{
    private static readonly HashSet<StatDomain> AnyDomain = new();

    public static DiseaseRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple DiseaseRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override void ValidateDefinitions(List<DiseaseDefinition> defs, System.Action<string> reportError)
    {
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
            reportError);
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (StatModifierRegistry.Instance == null)
            yield return "Missing dependency: StatModifierRegistry.Instance is null.";
        if (StatRegistry.Instance == null)
            yield return "Missing dependency: StatRegistry.Instance is null.";
    }
}
