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

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (ResourceRegistry.Instance == null)
            yield return "Missing dependency: ResourceRegistry.Instance is null.";
    }
}
