using System.Collections.Generic;
using UnityEngine;

public class ProductionProfileRegistry : DefinitionRegistry<ProductionProfileDefinition>
{
    private static RegistrySchema<ProductionProfileDefinition> schema;

    public static ProductionProfileRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple ProductionProfileRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override RegistrySchema<ProductionProfileDefinition> GetSchema()
    {
        return schema ??= new RegistrySchema<ProductionProfileDefinition>()
            .RequireField(nameof(ProductionProfileDefinition.Id), definition => definition.Id)
            .RequireField(nameof(ProductionProfileDefinition.Stats), definition => definition.Stats)
            .OptionalField(nameof(ProductionProfileDefinition.BuildingId), definition => definition.BuildingId)
            .OptionalField(nameof(ProductionProfileDefinition.UnitId), definition => definition.UnitId)
            .OptionalField(nameof(ProductionProfileDefinition.Costs), definition => definition.Costs)
            .AddReference(
                nameof(ProductionProfileDefinition.BuildingId),
                definition => RegistrySchema<ProductionProfileDefinition>.SingleReference(definition.BuildingId),
                false,
                new ReferenceTargetRule(nameof(BuildingRegistry), targetId => BuildingRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(ProductionProfileDefinition.UnitId),
                definition => RegistrySchema<ProductionProfileDefinition>.SingleReference(definition.UnitId),
                false,
                new ReferenceTargetRule(nameof(UnitRegistry), targetId => UnitRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(ProductionProfileDefinition.Costs),
                definition => RegistrySchema<ProductionProfileDefinition>.ReferenceCollection(definition.Costs, amount => amount.ResourceId),
                false,
                new ReferenceTargetRule(nameof(ResourceRegistry), targetId => ResourceRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                $"{nameof(ProductionProfileDefinition.Stats)}.{nameof(SerializedStatContainer.Entries)}",
                definition => RegistrySchema<ProductionProfileDefinition>.ReferenceCollection(definition.Stats.Entries, stat => stat.StatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance.TryGet(targetId, out _)));
    }

    protected override void ValidateDefinitions(List<ProductionProfileDefinition> defs, System.Action<string> reportError)
    {
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
