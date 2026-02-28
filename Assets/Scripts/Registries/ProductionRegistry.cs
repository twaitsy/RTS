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

    protected override void ValidateDefinitions(List<ProductionDefinition> defs, System.Action<string> reportError)
    {
        DefinitionReferenceValidator.ValidateSingleReference(
            defs,
            definition => definition.name,
            definition => definition.Id,
            definition => definition.BuildingId,
            nameof(ProductionDefinition.BuildingId),
            targetId => BuildingRegistry.Instance.TryGet(targetId, out _),
            reportError);

        DefinitionReferenceValidator.ValidateSingleReference(
            defs,
            definition => definition.name,
            definition => definition.Id,
            definition => definition.UnitId,
            nameof(ProductionDefinition.UnitId),
            targetId => UnitRegistry.Instance.TryGet(targetId, out _),
            reportError);

        DefinitionReferenceValidator.ValidateReferenceCollection(
            defs,
            definition => definition.name,
            definition => definition.Id,
            definition => definition.Costs,
            amount => amount.ResourceId,
            nameof(ProductionDefinition.Costs),
            targetId => ResourceRegistry.Instance.TryGet(targetId, out _),
            reportError);

        DefinitionReferenceValidator.ValidateReferenceCollection(
            defs,
            definition => definition.name,
            definition => definition.Id,
            definition => definition.Stats.Entries,
            stat => stat.StatId,
            $"{nameof(ProductionDefinition.Stats)}.{nameof(SerializedStatContainer.Entries)}",
            targetId => StatRegistry.Instance.TryGet(targetId, out _),
            reportError);
    }


    protected override void CollectCustomReferences(List<ProductionDefinition> defs, DefinitionReferenceMap map)
    {
        foreach (var definition in defs)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                continue;

            if (!string.IsNullOrWhiteSpace(definition.BuildingId))
                map.AddReference(RegistryName, definition.Id, nameof(ProductionDefinition.BuildingId), nameof(BuildingRegistry), definition.BuildingId);

            if (!string.IsNullOrWhiteSpace(definition.UnitId))
                map.AddReference(RegistryName, definition.Id, nameof(ProductionDefinition.UnitId), nameof(UnitRegistry), definition.UnitId);

            if (definition.Costs != null)
            {
                foreach (var cost in definition.Costs)
                {
                    if (!string.IsNullOrWhiteSpace(cost.ResourceId))
                        map.AddReference(RegistryName, definition.Id, nameof(ProductionDefinition.Costs), nameof(ResourceRegistry), cost.ResourceId);
                }
            }

            if (definition.Stats?.Entries == null)
                continue;

            foreach (var stat in definition.Stats.Entries)
            {
                if (!string.IsNullOrWhiteSpace(stat.StatId))
                    map.AddReference(RegistryName, definition.Id, $"{nameof(ProductionDefinition.Stats)}.{nameof(SerializedStatContainer.Entries)}", nameof(StatRegistry), stat.StatId);
            }
        }
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (BuildingRegistry.Instance == null)
            yield return "Missing dependency: BuildingRegistry.Instance is null.";
        if (UnitRegistry.Instance == null)
            yield return "Missing dependency: UnitRegistry.Instance is null.";
        if (ResourceRegistry.Instance == null)
            yield return "Missing dependency: ResourceRegistry.Instance is null.";
        if (StatRegistry.Instance == null)
            yield return "Missing dependency: StatRegistry.Instance is null.";
    }
}
