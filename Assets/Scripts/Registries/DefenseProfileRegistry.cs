using System.Collections.Generic;
using UnityEngine;

public class DefenseProfileRegistry : DefinitionRegistry<DefenseProfileDefinition>
{
    private static RegistrySchema<DefenseProfileDefinition> schema;

    public static DefenseProfileRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple DefenseProfileRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override RegistrySchema<DefenseProfileDefinition> GetSchema()
    {
        return schema ??= new RegistrySchema<DefenseProfileDefinition>()
            .RequireField(nameof(DefenseProfileDefinition.Id), definition => definition.Id)
            .OptionalField(nameof(DefenseProfileDefinition.Modifiers), definition => definition.Modifiers)
            .AddReference(
                nameof(DefenseProfileDefinition.Modifiers) + ".weaponTypeId",
                definition => RegistrySchema<DefenseProfileDefinition>.ReferenceCollection(definition.Modifiers, modifier => modifier.weaponTypeId),
                false,
                new ReferenceTargetRule(nameof(WeaponTypeRegistry), targetId => WeaponTypeRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(DefenseProfileDefinition.Modifiers) + ".armorTypeId",
                definition => RegistrySchema<DefenseProfileDefinition>.ReferenceCollection(definition.Modifiers, modifier => modifier.armorTypeId),
                false,
                new ReferenceTargetRule(nameof(ArmorTypeRegistry), targetId => ArmorTypeRegistry.Instance.TryGet(targetId, out _)));
    }

    protected override void ValidateDefinitions(List<DefenseProfileDefinition> defs, System.Action<string> reportError)
    {
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (WeaponTypeRegistry.Instance == null)
            yield return "Missing dependency: WeaponTypeRegistry.Instance is null.";

        if (ArmorTypeRegistry.Instance == null)
            yield return "Missing dependency: ArmorTypeRegistry.Instance is null.";
    }
}
