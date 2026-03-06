using System;
using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    public string ResourceId; // e.g. "resource.wood"
    [SerializeField] private ResourceNodeDefinition definition;
    private ResourceNodeRuntime runtime;

    private void OnEnable()
    {
        EnsureRuntimeAdapter();
    }

    private void OnDisable()
    {
        if (runtime != null)
            ResourceLocator.Unregister(runtime);
    }

    private void EnsureRuntimeAdapter()
    {
        runtime = GetComponent<ResourceNodeRuntime>();
        if (runtime != null)
            return;

        runtime = gameObject.AddComponent<ResourceNodeRuntime>();
        if (definition == null && ResourceNodeRegistry.Instance != null)
        {
            foreach (var candidate in ResourceNodeRegistry.Instance.GetDefinitions())
            {
                if (candidate == null)
                    continue;

                if (!string.Equals(candidate.ResourceTypeId, ResourceId, StringComparison.Ordinal))
                    continue;

                definition = candidate;
                break;
            }
        }

        if (definition != null)
            runtime.SetDefinition(definition);
        else if (!string.IsNullOrWhiteSpace(ResourceId))
            runtime.SetFallbackResourceType(ResourceId);
    }
}
