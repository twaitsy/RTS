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

    protected override void ValidateDefinitions(List<StatModifierDefinition> defs, System.Action<string> reportError)
    {
        StatModifierLinkValidator.ValidateStatModifierDefinitions(
            defs,
            statId => StatRegistry.Instance.TryGet(statId, out _),
            reportError);
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (StatRegistry.Instance == null)
            yield return "Missing dependency: StatRegistry.Instance is null.";
    }
}
