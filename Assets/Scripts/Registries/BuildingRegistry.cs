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
        if (StatRegistry.Instance == null || ResourceRegistry.Instance == null)
        {
            Debug.LogError("BuildingRegistry validation skipped: one or more dependent registries are null (StatRegistry, ResourceRegistry).");
            return;
        }

        DefinitionReferenceValidator.ValidateReferenceCollection(
            defs,
            definition => definition.name,
            definition => definition.Id,
            definition => definition.Stats.Entries,
            stat => stat.StatId,
            $"{nameof(BuildingDefinition.Stats)}.{nameof(SerializedStatContainer.Entries)}",
            targetId => StatRegistry.Instance.TryGet(targetId, out _),
            Debug.LogError);

        DefinitionReferenceValidator.ValidateReferenceCollection(
            defs,
            definition => definition.name,
            definition => definition.Id,
            definition => definition.BuildCosts,
            amount => amount.ResourceId,
            nameof(BuildingDefinition.BuildCosts),
            targetId => ResourceRegistry.Instance.TryGet(targetId, out _),
            Debug.LogError);
    }
}
