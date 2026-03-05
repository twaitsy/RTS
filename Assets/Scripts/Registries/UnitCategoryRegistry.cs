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
            .RequireField(nameof(UnitCategoryDefinition.DisplayName), definition => definition.DisplayName)
            .OptionalField(nameof(UnitCategoryDefinition.Icon), definition => definition.Icon)
            .OptionalField(nameof(UnitCategoryDefinition.Color), definition => definition.Color)
            .OptionalField(nameof(UnitCategoryDefinition.SortOrder), definition => definition.SortOrder);
    }

    protected override void ValidateDefinitions(List<UnitCategoryDefinition> defs, System.Action<string> reportError)
    {
    }
}
