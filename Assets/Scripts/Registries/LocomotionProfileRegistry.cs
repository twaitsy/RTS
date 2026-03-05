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
            .RequireField(nameof(LocomotionProfileDefinition.ClipName), definition => definition.ClipName)
            .OptionalField(nameof(LocomotionProfileDefinition.Speed), definition => definition.Speed);
    }

    protected override void ValidateDefinitions(List<LocomotionProfileDefinition> defs, System.Action<string> reportError)
    {
    }
}
