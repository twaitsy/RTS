using System.Collections.Generic;
using UnityEngine;

public class LocomotionProfileRegistry : DefinitionRegistry<LocomotionProfileDefinition>
{
    private static RegistrySchema<LocomotionProfileDefinition> schema;

    public static LocomotionProfileRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple LocomotionProfileRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override RegistrySchema<LocomotionProfileDefinition> GetSchema()
    {
        return schema ??= new RegistrySchema<LocomotionProfileDefinition>()
            .RequireField(nameof(LocomotionProfileDefinition.Id), definition => definition.Id)
            .RequireField(nameof(LocomotionProfileDefinition.Metadata), definition => definition.Metadata)
            .OptionalField(nameof(LocomotionProfileDefinition.DisplayName), definition => definition.DisplayName)
            .RequireField(nameof(LocomotionProfileDefinition.Stats), definition => definition.Stats)
            .OptionalField(nameof(LocomotionProfileDefinition.StatModifiers), definition => definition.StatModifiers)
            .RequireField(nameof(LocomotionProfileDefinition.ClipName), definition => definition.ClipName)
            .OptionalField(nameof(LocomotionProfileDefinition.Speed), definition => definition.Speed)
            .AddReference(
                $"{nameof(LocomotionProfileDefinition.Stats)}.{nameof(SerializedStatContainer.Entries)}",
                definition => RegistrySchema<LocomotionProfileDefinition>.ReferenceCollection(definition.Stats.Entries, stat => stat.StatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance != null && StatRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(LocomotionProfileDefinition.StatModifiers),
                definition => RegistrySchema<LocomotionProfileDefinition>.ReferenceCollection(definition.StatModifiers, modifier => modifier.targetStatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance != null && StatRegistry.Instance.TryGet(targetId, out _)))
            .AddConstraint("LocomotionProfileConstraints", ValidateConstraints);
    }

    protected override void ValidateDefinitions(List<LocomotionProfileDefinition> defs, System.Action<string> reportError)
    {
    }

    private static IEnumerable<string> ValidateConstraints(LocomotionProfileDefinition definition)
    {
        if (definition.Speed < 0f)
            yield return $"{nameof(LocomotionProfileDefinition.Speed)} must be greater than or equal to 0.";
        if (!definition.CanTraverseGround && !definition.CanTraverseWater && !definition.CanTraverseAir)
            yield return "At least one traversal mode must be enabled.";
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (StatRegistry.Instance == null)
            yield return "Missing dependency: StatRegistry.Instance is null.";
    }
}
