using System.Collections.Generic;
using UnityEngine;

public class BuildingRegistry : DefinitionRegistry<BuildingDefinition>
{
    private static RegistrySchema<BuildingDefinition> schema;

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

    public static IReadOnlyCollection<string> GetReferenceFieldPaths()
    {
        return GetOrCreateSchema().GetReferenceFieldNames();
    }

    protected override RegistrySchema<BuildingDefinition> GetSchema()
    {
        return GetOrCreateSchema();
    }

    private static RegistrySchema<BuildingDefinition> GetOrCreateSchema()
    {
        if (schema != null)
            return schema;

        schema = new RegistrySchema<BuildingDefinition>()
            .RequireField(nameof(BuildingDefinition.Id), definition => definition.Id)
            .RequireField(nameof(BuildingDefinition.DisplayName), definition => definition.DisplayName)
            .RequireField(nameof(BuildingDefinition.PrefabId), definition => definition.PrefabId)
            .RequireField(nameof(BuildingDefinition.PrimaryCategoryId), definition => definition.PrimaryCategoryId)
            .RequireField(nameof(BuildingDefinition.Stats), definition => definition.Stats)
            .OptionalField(nameof(BuildingDefinition.SecondaryCategoryIds), definition => definition.SecondaryCategoryIds)
            .OptionalField(nameof(BuildingDefinition.BuildCosts), definition => definition.BuildCosts)
            .AddReference(
                $"{nameof(BuildingDefinition.Stats)}.{nameof(SerializedStatContainer.Entries)}",
                definition => RegistrySchema<BuildingDefinition>.ReferenceCollection(definition.Stats.Entries, stat => stat.StatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance != null && StatRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(BuildingDefinition.PrefabId),
                definition => RegistrySchema<BuildingDefinition>.SingleReference(definition.PrefabId),
                true,
                new ReferenceTargetRule(nameof(PrefabRegistry), targetId => PrefabRegistry.TryGetDefinition(targetId, out _)))
            .AddReference(
                nameof(BuildingDefinition.PrimaryCategoryId),
                definition => RegistrySchema<BuildingDefinition>.SingleReference(definition.PrimaryCategoryId),
                true,
                new ReferenceTargetRule(nameof(BuildingCategoryRegistry), targetId => BuildingCategoryRegistry.Instance != null && BuildingCategoryRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(BuildingDefinition.SecondaryCategoryIds),
                definition => RegistrySchema<BuildingDefinition>.ReferenceCollection(definition.SecondaryCategoryIds, categoryId => categoryId),
                false,
                new ReferenceTargetRule(nameof(BuildingCategoryRegistry), targetId => BuildingCategoryRegistry.Instance != null && BuildingCategoryRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(BuildingDefinition.BuildCosts),
                definition => RegistrySchema<BuildingDefinition>.ReferenceCollection(definition.BuildCosts, amount => amount.ResourceId),
                false,
                new ReferenceTargetRule(nameof(ResourceRegistry), targetId => ResourceRegistry.Instance != null && ResourceRegistry.Instance.TryGet(targetId, out _)))
            .AddConstraint(
                nameof(BuildingDefinition.SecondaryCategoryIds),
                definition =>
                {
                    var errors = new List<string>();
                    var ids = definition.SecondaryCategoryIds;
                    if (ids == null)
                        return errors;

                    for (var index = 0; index < ids.Count; index++)
                    {
                        if (string.IsNullOrWhiteSpace(ids[index]))
                            errors.Add($"{nameof(BuildingDefinition.SecondaryCategoryIds)}[{index}] must not be empty.");
                    }

                    return errors;
                });

        return schema;
    }

    protected override void ValidateDefinitions(List<BuildingDefinition> defs, System.Action<string> reportError)
    {
        // Intentionally reserved for bespoke Building validation rules.
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (StatRegistry.Instance == null)
            yield return "Missing dependency: StatRegistry.Instance is null.";
        if (BuildingCategoryRegistry.Instance == null)
            yield return "Missing dependency: BuildingCategoryRegistry.Instance is null.";
        if (ResourceRegistry.Instance == null)
            yield return "Missing dependency: ResourceRegistry.Instance is null.";

        PrefabRegistry.Initialize();
    }
}
