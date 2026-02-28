using System.Collections.Generic;
using UnityEngine;

public class RecipeRegistry : DefinitionRegistry<RecipeDefinition>
{
    private static RegistrySchema<RecipeDefinition> schema;

    public static RecipeRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple RecipeRegistry instances detected.");
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

    protected override RegistrySchema<RecipeDefinition> GetSchema()
    {
        return GetOrCreateSchema();
    }

    private static RegistrySchema<RecipeDefinition> GetOrCreateSchema()
    {
        if (schema != null)
            return schema;

        schema = new RegistrySchema<RecipeDefinition>()
            .RequireField(nameof(RecipeDefinition.Id), definition => definition.Id)
            .RequireField(nameof(RecipeDefinition.DisplayName), definition => definition.DisplayName)
            .RequireField(nameof(RecipeDefinition.Inputs), definition => definition.Inputs)
            .RequireField(nameof(RecipeDefinition.Outputs), definition => definition.Outputs)
            .OptionalField(nameof(RecipeDefinition.BuildingId), definition => definition.BuildingId)
            .OptionalField(nameof(RecipeDefinition.JobId), definition => definition.JobId)
            .AddReference(nameof(RecipeDefinition.BuildingId), definition => RegistrySchema<RecipeDefinition>.SingleReference(definition.BuildingId), false, new ReferenceTargetRule(nameof(BuildingRegistry), targetId => BuildingRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(RecipeDefinition.JobId), definition => RegistrySchema<RecipeDefinition>.SingleReference(definition.JobId), false, new ReferenceTargetRule(nameof(JobRegistry), targetId => JobRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(RecipeDefinition.Inputs), definition => RegistrySchema<RecipeDefinition>.ReferenceCollection(definition.Inputs, amount => amount.itemId), true, new ReferenceTargetRule(nameof(ItemRegistry), targetId => ItemRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(RecipeDefinition.Outputs), definition => RegistrySchema<RecipeDefinition>.ReferenceCollection(definition.Outputs, amount => amount.itemId), true, new ReferenceTargetRule(nameof(ItemRegistry), targetId => ItemRegistry.Instance.TryGet(targetId, out _)));

        return schema;
    }

    protected override void ValidateDefinitions(List<RecipeDefinition> defs, System.Action<string> reportError)
    {
        // Intentionally reserved for bespoke Recipe validation rules.
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (BuildingRegistry.Instance == null)
            yield return "Missing dependency: BuildingRegistry.Instance is null.";
        if (JobRegistry.Instance == null)
            yield return "Missing dependency: JobRegistry.Instance is null.";
        if (ItemRegistry.Instance == null)
            yield return "Missing dependency: ItemRegistry.Instance is null.";
    }
}
