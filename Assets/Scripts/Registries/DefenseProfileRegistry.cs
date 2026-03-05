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
            .RequireField(nameof(DefenseProfileDefinition.Metadata), definition => definition.Metadata)
            .OptionalField(nameof(DefenseProfileDefinition.DisplayName), definition => definition.DisplayName)
            .RequireField(nameof(DefenseProfileDefinition.Stats), definition => definition.Stats)
            .OptionalField(nameof(DefenseProfileDefinition.StatModifiers), definition => definition.StatModifiers)
            .OptionalField(nameof(DefenseProfileDefinition.Modifiers), definition => definition.Modifiers)
            .AddReference(
                $"{nameof(DefenseProfileDefinition.Stats)}.{nameof(SerializedStatContainer.Entries)}",
                definition => RegistrySchema<DefenseProfileDefinition>.ReferenceCollection(definition.Stats.Entries, stat => stat.StatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance != null && StatRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(DefenseProfileDefinition.StatModifiers),
                definition => RegistrySchema<DefenseProfileDefinition>.ReferenceCollection(definition.StatModifiers, modifier => modifier.targetStatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance != null && StatRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(DefenseProfileDefinition.Modifiers) + ".weaponTypeId",
                definition => RegistrySchema<DefenseProfileDefinition>.ReferenceCollection(definition.Modifiers, modifier => modifier.weaponTypeId),
                false,
                new ReferenceTargetRule(nameof(WeaponTypeRegistry), targetId => WeaponTypeRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(DefenseProfileDefinition.Modifiers) + ".armorTypeId",
                definition => RegistrySchema<DefenseProfileDefinition>.ReferenceCollection(definition.Modifiers, modifier => modifier.armorTypeId),
                false,
                new ReferenceTargetRule(nameof(ArmorTypeRegistry), targetId => ArmorTypeRegistry.Instance.TryGet(targetId, out _)))
            .AddConstraint("DefenseProfileConstraints", ValidateConstraints);
    }

    protected override void ValidateDefinitions(List<DefenseProfileDefinition> defs, System.Action<string> reportError)
    {
        // Schema handles validation.
    }

    private static IEnumerable<string> ValidateConstraints(DefenseProfileDefinition definition)
    {
        var seenPairs = new HashSet<string>(System.StringComparer.Ordinal);
        var modifiers = definition.Modifiers;
        if (modifiers == null)
            yield break;

        for (var index = 0; index < modifiers.Count; index++)
        {
            var modifier = modifiers[index];
            if (modifier.multiplier < 0f)
                yield return $"{nameof(DefenseProfileDefinition.Modifiers)}[{index}].{nameof(DamageModifier.multiplier)} must be greater than or equal to 0.";

            var pairKey = $"{modifier.weaponTypeId}|{modifier.armorTypeId}";
            if (!seenPairs.Add(pairKey))
                yield return $"{nameof(DefenseProfileDefinition.Modifiers)} contains duplicate weapon/armor mapping '{pairKey}'.";
        }
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (StatRegistry.Instance == null)
            yield return "Missing dependency: StatRegistry.Instance is null.";
        if (WeaponTypeRegistry.Instance == null)
            yield return "Missing dependency: WeaponTypeRegistry.Instance is null.";

        if (ArmorTypeRegistry.Instance == null)
            yield return "Missing dependency: ArmorTypeRegistry.Instance is null.";
    }
}
