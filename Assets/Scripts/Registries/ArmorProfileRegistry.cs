using System.Collections.Generic;
using UnityEngine;

public class ArmorProfileRegistry : DefinitionRegistry<ArmorProfileDefinition>
{
    private static RegistrySchema<ArmorProfileDefinition> schema;

    public static ArmorProfileRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple ArmorProfileRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override RegistrySchema<ArmorProfileDefinition> GetSchema()
    {
        return schema ??= new RegistrySchema<ArmorProfileDefinition>()
            .RequireField(nameof(ArmorProfileDefinition.Id), definition => definition.Id)
            .OptionalField(nameof(ArmorProfileDefinition.DisplayName), definition => definition.DisplayName)
            .OptionalField(nameof(ArmorProfileDefinition.Stats), definition => definition.Stats)
            .OptionalField(nameof(ArmorProfileDefinition.StatModifiers), definition => definition.StatModifiers)
            .AddReference(
                $"{nameof(ArmorProfileDefinition.Stats)}.{nameof(SerializedStatContainer.Entries)}",
                definition => RegistrySchema<ArmorProfileDefinition>.ReferenceCollection(definition.Stats.Entries, stat => stat.StatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(ArmorProfileDefinition.StatModifiers),
                definition => RegistrySchema<ArmorProfileDefinition>.ReferenceCollection(definition.StatModifiers, modifier => modifier.targetStatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance.TryGet(targetId, out _)));
    }

    protected override void ValidateDefinitions(List<ArmorProfileDefinition> defs, System.Action<string> reportError)
    {
        // Schema validation covers required fields and references.
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (StatRegistry.Instance == null)
            yield return "Missing dependency: StatRegistry.Instance is null.";
    }
}
