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

    protected override void ValidateDefinitions(List<ResourceNodeDefinition> defs, System.Action<string> reportError)
    {
        DefinitionReferenceValidator.ValidateSingleReference(
            defs,
            definition => definition.name,
            definition => definition.Id,
            definition => definition.ResourceId,
            nameof(ResourceNodeDefinition.ResourceId),
            targetId => ResourceRegistry.Instance.TryGet(targetId, out _),
            reportError);
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (ResourceRegistry.Instance == null)
            yield return "Missing dependency: ResourceRegistry.Instance is null.";
    }
}
