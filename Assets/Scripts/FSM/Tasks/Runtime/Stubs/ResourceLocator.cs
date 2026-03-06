using System.Collections.Generic;
using UnityEngine;

public static class ResourceLocator
{
    private static readonly Dictionary<string, List<ResourceNodeRuntime>> registry
        = new Dictionary<string, List<ResourceNodeRuntime>>();

    public static void Register(ResourceNodeRuntime node)
    {
        if (node == null || string.IsNullOrEmpty(node.ResourceTypeId))
            return;

        if (!registry.TryGetValue(node.ResourceTypeId, out var list))
        {
            list = new List<ResourceNodeRuntime>();
            registry[node.ResourceTypeId] = list;
        }

        if (!list.Contains(node))
            list.Add(node);
    }

    public static void Unregister(ResourceNodeRuntime node)
    {
        if (node == null || string.IsNullOrEmpty(node.ResourceTypeId))
            return;

        if (registry.TryGetValue(node.ResourceTypeId, out var list))
            list.Remove(node);
    }

    public static ResourceNodeRuntime FindNearest(string resourceId, Vector3 position)
    {
        if (string.IsNullOrEmpty(resourceId))
            return null;

        if (!registry.TryGetValue(resourceId, out var list) || list.Count == 0)
            return null;

        ResourceNodeRuntime best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < list.Count; i++)
        {
            var node = list[i];
            if (node == null || node.IsDepleted)
                continue;

            float dist = (node.transform.position - position).sqrMagnitude;
            if (dist < bestDist)
            {
                bestDist = dist;
                best = node;
            }
        }

        return best;
    }
}
