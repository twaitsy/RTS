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

    protected override void ValidateDefinitions(List<BuildingDefinition> defs, System.Action<string> reportError)
    {
        DefinitionReferenceValidator.ValidateReferenceCollection(
            defs,
            definition => definition.name,
            definition => definition.Id,
            definition => definition.Stats.Entries,
            stat => stat.StatId,
            $"{nameof(BuildingDefinition.Stats)}.{nameof(SerializedStatContainer.Entries)}",
            targetId => StatRegistry.Instance.TryGet(targetId, out _),
            reportError);

        DefinitionReferenceValidator.ValidateReferenceCollection(
            defs,
            definition => definition.name,
            definition => definition.Id,
            definition => definition.BuildCosts,
            amount => amount.ResourceId,
            nameof(BuildingDefinition.BuildCosts),
            targetId => ResourceRegistry.Instance.TryGet(targetId, out _),
            reportError);
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (StatRegistry.Instance == null)
            yield return "Missing dependency: StatRegistry.Instance is null.";
        if (ResourceRegistry.Instance == null)
            yield return "Missing dependency: ResourceRegistry.Instance is null.";
    }
}
