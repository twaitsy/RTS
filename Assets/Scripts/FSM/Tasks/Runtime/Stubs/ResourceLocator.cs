using System.Collections.Generic;
using UnityEngine;

public static class ResourceLocator
{
    // Registry of all active resource nodes in the world.
    // Key = canonical resource ID (e.g., "resource.wood")
    // Value = list of nodes of that type
    private static readonly Dictionary<string, List<ResourceNode>> registry
        = new Dictionary<string, List<ResourceNode>>();

    public static void Register(ResourceNode node)
    {
        if (node == null || string.IsNullOrEmpty(node.ResourceId))
            return;

        if (!registry.TryGetValue(node.ResourceId, out var list))
        {
            list = new List<ResourceNode>();
            registry[node.ResourceId] = list;
        }

        if (!list.Contains(node))
            list.Add(node);
    }

    public static void Unregister(ResourceNode node)
    {
        if (node == null || string.IsNullOrEmpty(node.ResourceId))
            return;

        if (registry.TryGetValue(node.ResourceId, out var list))
            list.Remove(node);
    }

    public static ResourceNode FindNearest(string resourceId, Vector3 position)
    {
        if (string.IsNullOrEmpty(resourceId))
            return null;

        if (!registry.TryGetValue(resourceId, out var list) || list.Count == 0)
            return null;

        ResourceNode best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < list.Count; i++)
        {
            var node = list[i];
            if (node == null)
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