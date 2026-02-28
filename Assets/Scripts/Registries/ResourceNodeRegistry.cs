using System.Collections.Generic;
using UnityEngine;

public class ResourceNodeRegistry : DefinitionRegistry<ResourceNodeDefinition>
{
    public static ResourceNodeRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple ResourceNodeRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override void ValidateDefinitions(List<ResourceNodeDefinition> defs)
    {
        if (ResourceRegistry.Instance == null)
        {
            Debug.LogError("ResourceNodeRegistry validation skipped: ResourceRegistry.Instance is null.");
            return;
        }

        DefinitionReferenceValidator.ValidateSingleReference(
            defs,
            definition => definition.name,
            definition => definition.Id,
            definition => definition.ResourceId,
            nameof(ResourceNodeDefinition.ResourceId),
            targetId => ResourceRegistry.Instance.TryGet(targetId, out _),
            Debug.LogError);
    }
}
