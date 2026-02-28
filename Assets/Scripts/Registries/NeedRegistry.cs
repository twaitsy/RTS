using System.Collections.Generic;
using UnityEngine;

public class NeedRegistry : DefinitionRegistry<NeedDefinition>
{
    private static readonly HashSet<StatDomain> AllowedNeedDomains = new()
    {
        StatDomain.Needs,
        StatDomain.Mood
    };

    public static NeedRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple NeedRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override void ValidateDefinitions(List<NeedDefinition> defs, System.Action<string> reportError)
    {
        StatModifierLinkValidator.ValidateHostStatModifierLinks(
            defs,
            definition => definition.Id,
            definition => definition.StatModifierIds,
            definition => definition.name,
            definition => definition.AllowAnyModifierDomain,
            modifierId => StatModifierRegistry.Instance.TryGet(modifierId, out _),
            modifierId => StatModifierRegistry.Instance.Get(modifierId),
            statId => StatRegistry.Instance.TryGet(statId, out _),
            statId => StatRegistry.Instance.Get(statId),
            AllowedNeedDomains,
            "Needs or Mood",
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
