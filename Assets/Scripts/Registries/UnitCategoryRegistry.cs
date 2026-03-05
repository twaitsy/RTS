using System.Collections.Generic;
using UnityEngine;

public class UnitCategoryRegistry : DefinitionRegistry<UnitCategoryDefinition>
{
    private static RegistrySchema<UnitCategoryDefinition> schema;

    public static UnitCategoryRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple UnitCategoryRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override RegistrySchema<UnitCategoryDefinition> GetSchema()
    {
        return schema ??= new RegistrySchema<UnitCategoryDefinition>()
            .RequireField(nameof(UnitCategoryDefinition.Id), definition => definition.Id)
            .RequireField(nameof(UnitCategoryDefinition.Metadata), definition => definition.Metadata)
            .RequireField(nameof(UnitCategoryDefinition.DisplayName), definition => definition.DisplayName)
            .OptionalField(nameof(UnitCategoryDefinition.Icon), definition => definition.Icon)
            .OptionalField(nameof(UnitCategoryDefinition.Color), definition => definition.Color)
            .OptionalField(nameof(UnitCategoryDefinition.SortOrder), definition => definition.SortOrder)
            .OptionalField(nameof(UnitCategoryDefinition.RequiredStatIds), definition => definition.RequiredStatIds)
            .AddReference(
                nameof(UnitCategoryDefinition.RequiredStatIds),
                definition => RegistrySchema<UnitCategoryDefinition>.ReferenceCollection(definition.RequiredStatIds, id => id),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance != null && StatRegistry.Instance.TryGet(targetId, out _)))
            .AddConstraint("UnitCategoryConstraints", ValidateConstraints);
    }

    protected override void ValidateDefinitions(List<UnitCategoryDefinition> defs, System.Action<string> reportError)
    {
    }

    private static IEnumerable<string> ValidateConstraints(UnitCategoryDefinition definition)
    {
        if (definition.SortOrder < 0)
            yield return $"{nameof(UnitCategoryDefinition.SortOrder)} must be greater than or equal to 0.";

        var seen = new HashSet<string>(System.StringComparer.Ordinal);
        var required = definition.RequiredStatIds;
        if (required == null)
            yield break;

        for (var index = 0; index < required.Count; index++)
        {
            var statId = required[index];
            if (string.IsNullOrWhiteSpace(statId))
            {
                yield return $"{nameof(UnitCategoryDefinition.RequiredStatIds)}[{index}] must not be empty.";
                continue;
            }

            if (!seen.Add(statId))
                yield return $"{nameof(UnitCategoryDefinition.RequiredStatIds)} contains duplicate stat id '{statId}'.";
        }
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (StatRegistry.Instance == null)
            yield return "Missing dependency: StatRegistry.Instance is null.";
    }
}
