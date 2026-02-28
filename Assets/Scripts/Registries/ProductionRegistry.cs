using System.Collections.Generic;
using UnityEngine;

public class ProductionRegistry : DefinitionRegistry<ProductionDefinition>
{
    public static ProductionRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple ProductionRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override void ValidateDefinitions(List<ProductionDefinition> defs)
    {
        if (BuildingRegistry.Instance == null || UnitRegistry.Instance == null || ResourceRegistry.Instance == null || StatRegistry.Instance == null)
        {
            Debug.LogError("ProductionRegistry validation skipped: one or more dependent registries are null (BuildingRegistry, UnitRegistry, ResourceRegistry, StatRegistry).");
            return;
        }

        DefinitionReferenceValidator.ValidateSingleReference(
            defs,
            definition => definition.name,
            definition => definition.Id,
            definition => definition.BuildingId,
            nameof(ProductionDefinition.BuildingId),
            targetId => BuildingRegistry.Instance.TryGet(targetId, out _),
            Debug.LogError);

        DefinitionReferenceValidator.ValidateSingleReference(
            defs,
            definition => definition.name,
            definition => definition.Id,
            definition => definition.UnitId,
            nameof(ProductionDefinition.UnitId),
            targetId => UnitRegistry.Instance.TryGet(targetId, out _),
            Debug.LogError);

        DefinitionReferenceValidator.ValidateReferenceCollection(
            defs,
            definition => definition.name,
            definition => definition.Id,
            definition => definition.Costs,
            amount => amount.ResourceId,
            nameof(ProductionDefinition.Costs),
            targetId => ResourceRegistry.Instance.TryGet(targetId, out _),
            Debug.LogError);

        DefinitionReferenceValidator.ValidateReferenceCollection(
            defs,
            definition => definition.name,
            definition => definition.Id,
            definition => definition.Stats.Entries,
            stat => stat.StatId,
            $"{nameof(ProductionDefinition.Stats)}.{nameof(SerializedStatContainer.Entries)}",
            targetId => StatRegistry.Instance.TryGet(targetId, out _),
            Debug.LogError);
    }
}
