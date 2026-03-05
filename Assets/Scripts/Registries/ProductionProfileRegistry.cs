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
            .RequireField(nameof(ProductionProfileDefinition.Metadata), definition => definition.Metadata)
            .OptionalField(nameof(ProductionProfileDefinition.DisplayName), definition => definition.DisplayName)
            .RequireField(nameof(ProductionProfileDefinition.Stats), definition => definition.Stats)
            .OptionalField(nameof(ProductionProfileDefinition.StatModifiers), definition => definition.StatModifiers)
            .OptionalField(nameof(ProductionProfileDefinition.BuildingId), definition => definition.BuildingId)
            .OptionalField(nameof(ProductionProfileDefinition.UnitId), definition => definition.UnitId)
            .OptionalField(nameof(ProductionProfileDefinition.Costs), definition => definition.Costs)
            .OptionalField(nameof(ProductionProfileDefinition.UnlockTechIds), definition => definition.UnlockTechIds)
            .OptionalField(nameof(ProductionProfileDefinition.UnlockUnitIds), definition => definition.UnlockUnitIds)
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
                nameof(ProductionProfileDefinition.UnlockTechIds),
                definition => RegistrySchema<ProductionProfileDefinition>.ReferenceCollection(definition.UnlockTechIds, id => id),
                false,
                new ReferenceTargetRule(nameof(TechRegistry), targetId => TechRegistry.Instance != null && TechRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(ProductionProfileDefinition.UnlockUnitIds),
                definition => RegistrySchema<ProductionProfileDefinition>.ReferenceCollection(definition.UnlockUnitIds, id => id),
                false,
                new ReferenceTargetRule(nameof(UnitRegistry), targetId => UnitRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                $"{nameof(ProductionProfileDefinition.Stats)}.{nameof(SerializedStatContainer.Entries)}",
                definition => RegistrySchema<ProductionProfileDefinition>.ReferenceCollection(definition.Stats.Entries, stat => stat.StatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(ProductionProfileDefinition.StatModifiers),
                definition => RegistrySchema<ProductionProfileDefinition>.ReferenceCollection(definition.StatModifiers, modifier => modifier.targetStatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance.TryGet(targetId, out _)))
            .AddConstraint("ProductionProfileConstraints", ValidateConstraints);
    }

    protected override void ValidateDefinitions(List<ProductionProfileDefinition> defs, System.Action<string> reportError)
    {
    }

    private static IEnumerable<string> ValidateConstraints(ProductionProfileDefinition definition)
    {
        if (definition.ProductionTime < 0f)
            yield return $"{nameof(ProductionProfileDefinition.ProductionTime)} must be greater than or equal to 0.";
        if (definition.MaxQueueSize <= 0)
            yield return $"{nameof(ProductionProfileDefinition.MaxQueueSize)} must be greater than 0.";
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
        if (TechRegistry.Instance == null)
            yield return "Missing dependency: TechRegistry.Instance is null.";
    }
}
