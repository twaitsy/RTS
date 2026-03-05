using System.Collections.Generic;
using UnityEngine;

public static class DropoffLocator
{
    private static readonly List<DropoffReceiver> receivers = new();

    public static void Register(DropoffReceiver receiver)
    {
        if (receiver == null || receivers.Contains(receiver))
            return;

        receivers.Add(receiver);
    }

    public static void Unregister(DropoffReceiver receiver)
    {
        if (receiver == null)
            return;

        receivers.Remove(receiver);
    }

    public static DropoffReceiver FindNearest(Vector3 position)
    {
        DropoffReceiver best = null;
        var bestDist = float.MaxValue;

        for (int i = 0; i < receivers.Count; i++)
        {
            var receiver = receivers[i];
            if (receiver == null)
                continue;

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
