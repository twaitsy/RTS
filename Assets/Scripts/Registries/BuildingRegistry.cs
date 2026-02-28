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
            .RequireField(nameof(BuildingDefinition.Stats), definition => definition.Stats)
            .OptionalField(nameof(BuildingDefinition.BuildCosts), definition => definition.BuildCosts)
            .AddReference(
                $"{nameof(BuildingDefinition.Stats)}.{nameof(SerializedStatContainer.Entries)}",
                definition => RegistrySchema<BuildingDefinition>.ReferenceCollection(definition.Stats.Entries, stat => stat.StatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(BuildingDefinition.BuildCosts),
                definition => RegistrySchema<BuildingDefinition>.ReferenceCollection(definition.BuildCosts, amount => amount.ResourceId),
                false,
                new ReferenceTargetRule(nameof(ResourceRegistry), targetId => ResourceRegistry.Instance.TryGet(targetId, out _)));

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
        if (ResourceRegistry.Instance == null)
            yield return "Missing dependency: ResourceRegistry.Instance is null.";
    }
}
