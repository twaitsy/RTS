using System.Collections.Generic;
using UnityEngine;

public class MovementProfileRegistry : DefinitionRegistry<MovementProfileDefinition>
{
    private static RegistrySchema<MovementProfileDefinition> schema;

    public static MovementProfileRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple MovementProfileRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override RegistrySchema<MovementProfileDefinition> GetSchema()
    {
        return schema ??= new RegistrySchema<MovementProfileDefinition>()
            .RequireField(nameof(MovementProfileDefinition.Id), definition => definition.Id)
            .RequireField(nameof(MovementProfileDefinition.Metadata), definition => definition.Metadata)
            .OptionalField(nameof(MovementProfileDefinition.DisplayName), definition => definition.DisplayName)
            .RequireField(nameof(MovementProfileDefinition.Stats), definition => definition.Stats)
            .OptionalField(nameof(MovementProfileDefinition.StatModifiers), definition => definition.StatModifiers)
            .OptionalField(nameof(MovementProfileDefinition.LocomotionProfileId), definition => definition.LocomotionProfileId)
            .OptionalField(nameof(MovementProfileDefinition.MoveSpeedMultiplier), definition => definition.MoveSpeedMultiplier)
            .OptionalField(nameof(MovementProfileDefinition.DefenseMultiplier), definition => definition.DefenseMultiplier)
            .AddReference(
                $"{nameof(MovementProfileDefinition.Stats)}.{nameof(SerializedStatContainer.Entries)}",
                definition => RegistrySchema<MovementProfileDefinition>.ReferenceCollection(definition.Stats.Entries, stat => stat.StatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(MovementProfileDefinition.StatModifiers),
                definition => RegistrySchema<MovementProfileDefinition>.ReferenceCollection(definition.StatModifiers, modifier => modifier.targetStatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance != null && StatRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(MovementProfileDefinition.LocomotionProfileId),
                definition => RegistrySchema<MovementProfileDefinition>.SingleReference(definition.LocomotionProfileId),
                false,
                new ReferenceTargetRule(nameof(LocomotionProfileRegistry), targetId => LocomotionProfileRegistry.Instance != null && LocomotionProfileRegistry.Instance.TryGet(targetId, out _)))
            .AddConstraint("MovementProfileConstraints", ValidateConstraints);
    }

    protected override void ValidateDefinitions(List<MovementProfileDefinition> defs, System.Action<string> reportError)
    {
    }

    private static IEnumerable<string> ValidateConstraints(MovementProfileDefinition definition)
    {
        if (definition.MoveSpeedMultiplier < 0f)
            yield return $"{nameof(MovementProfileDefinition.MoveSpeedMultiplier)} must be greater than or equal to 0.";
        if (definition.DefenseMultiplier < 0f)
            yield return $"{nameof(MovementProfileDefinition.DefenseMultiplier)} must be greater than or equal to 0.";
        if (definition.Acceleration < 0f)
            yield return $"{nameof(MovementProfileDefinition.Acceleration)} must be greater than or equal to 0.";
        if (definition.TurnRate < 0f)
            yield return $"{nameof(MovementProfileDefinition.TurnRate)} must be greater than or equal to 0.";
        if (definition.StoppingDistance < 0f)
            yield return $"{nameof(MovementProfileDefinition.StoppingDistance)} must be greater than or equal to 0.";
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (StatRegistry.Instance == null)
            yield return "Missing dependency: StatRegistry.Instance is null.";
        if (LocomotionProfileRegistry.Instance == null)
            yield return "Missing dependency: LocomotionProfileRegistry.Instance is null.";
    }
}
