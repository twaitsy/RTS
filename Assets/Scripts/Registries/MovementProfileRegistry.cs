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
            .RequireField(nameof(MovementProfileDefinition.Stats), definition => definition.Stats)
            .OptionalField(nameof(MovementProfileDefinition.MoveSpeedMultiplier), definition => definition.MoveSpeedMultiplier)
            .OptionalField(nameof(MovementProfileDefinition.DefenseMultiplier), definition => definition.DefenseMultiplier)
            .AddReference(
                $"{nameof(MovementProfileDefinition.Stats)}.{nameof(SerializedStatContainer.Entries)}",
                definition => RegistrySchema<MovementProfileDefinition>.ReferenceCollection(definition.Stats.Entries, stat => stat.StatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance.TryGet(targetId, out _)));
    }

    protected override void ValidateDefinitions(List<MovementProfileDefinition> defs, System.Action<string> reportError)
    {
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (StatRegistry.Instance == null)
            yield return "Missing dependency: StatRegistry.Instance is null.";
    }
}
