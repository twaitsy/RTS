using System;
using System.Collections.Generic;
using UnityEngine;

public static class DropoffLocator
{
    private static readonly List<BuildingRuntime> receivers = new();

    public static void Register(BuildingRuntime receiver)
    {
        if (receiver == null || receivers.Contains(receiver))
            return;

        receivers.Add(receiver);
    }

    public static void Unregister(BuildingRuntime receiver)
    {
        if (receiver == null)
            return;

        receivers.Remove(receiver);
    }

    public static BuildingRuntime FindNearest(Vector3 position, string resourceTypeId = null)
    {
        BuildingRuntime best = null;
        var bestDist = float.MaxValue;

        for (int i = 0; i < receivers.Count; i++)
        {
            var receiver = receivers[i];
            if (receiver == null || !receiver.SupportsDropoff)
                continue;

            if (!string.IsNullOrWhiteSpace(resourceTypeId) &&
                !receiver.AcceptsResource(resourceTypeId))
            {
                continue;
            }

            var dist = (receiver.transform.position - position).sqrMagnitude;
            if (dist < bestDist)
            {
                bestDist = dist;
                best = receiver;
            }
        }

        return best;
    }
}
