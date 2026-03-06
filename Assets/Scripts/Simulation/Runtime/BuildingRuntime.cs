using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildingRuntime : MonoBehaviour
{
    [SerializeField] private BuildingDefinition definition;

    [Header("Scene Interaction Points")]
    [SerializeField] private List<Transform> interactionPoints = new();
    public IReadOnlyList<Transform> InteractionPoints => interactionPoints;

    private readonly Dictionary<string, int> storedByResourceId = new(StringComparer.Ordinal);

    public BuildingDefinition Definition => definition;
    public bool SupportsDropoff => definition == null || definition.SupportsDropoff;
    public int StorageCapacity => definition != null ? Mathf.Max(0, definition.StorageCapacity) : int.MaxValue;
    public int CurrentStored { get; private set; }

    private void OnEnable() => DropoffLocator.Register(this);
    private void OnDisable() => DropoffLocator.Unregister(this);

    public void SetDefinition(BuildingDefinition nextDefinition) => definition = nextDefinition;

    public bool TryReceiveDelivery(string resourceTypeId, int amount, out int acceptedAmount, out string failureReason)
    {
        acceptedAmount = 0;
        failureReason = null;

        if (!SupportsDropoff)
        {
            failureReason = $"Building '{name}' does not support dropoff.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(resourceTypeId))
        {
            failureReason = "Resource type is required.";
            return false;
        }

        if (amount <= 0)
        {
            failureReason = "Delivered amount must be greater than zero.";
            return false;
        }

        if (!AcceptsResource(resourceTypeId))
        {
            failureReason = $"Building '{name}' does not accept resource '{resourceTypeId}'.";
            return false;
        }

        int freeSpace = Mathf.Max(0, StorageCapacity - CurrentStored);
        if (freeSpace <= 0)
        {
            failureReason = $"Building '{name}' is full.";
            return false;
        }

        bool allowPartial = definition == null || definition.AllowPartialDelivery;
        if (!allowPartial && amount > freeSpace)
        {
            failureReason = $"Building '{name}' cannot partially accept delivery.";
            return false;
        }

        acceptedAmount = allowPartial ? Mathf.Min(amount, freeSpace) : amount;
        CurrentStored += acceptedAmount;

        storedByResourceId.TryGetValue(resourceTypeId, out int currentAmount);
        storedByResourceId[resourceTypeId] = currentAmount + acceptedAmount;

        return true;
    }

    public bool AcceptsResource(string resourceTypeId)
    {
        if (!SupportsDropoff || string.IsNullOrWhiteSpace(resourceTypeId))
            return false;

        if (definition == null)
            return true;

        var accepted = definition.AcceptedResourceTypeIds;
        if (accepted == null || accepted.Count == 0)
            return false;

        for (int i = 0; i < accepted.Count; i++)
        {
            if (string.Equals(accepted[i], resourceTypeId, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    public float InteractionRadius => definition != null ? definition.InteractionRadius : 1.5f;

    public Vector3 GetBestInteractionWorldPosition(Vector3 actorPos)
    {
        // 1. Prefer definition points
        if (definition != null && definition.InteractionPoints != null && definition.InteractionPoints.Count > 0)
        {
            Vector3 best = transform.TransformPoint(definition.InteractionPoints[0]);
            float bestDist = (best - actorPos).sqrMagnitude;

            for (int i = 1; i < definition.InteractionPoints.Count; i++)
            {
                Vector3 wp = transform.TransformPoint(definition.InteractionPoints[i]);
                float d = (wp - actorPos).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    best = wp;
                }
            }

            return best;
        }

        // 2. Fall back to scene points
        if (interactionPoints != null && interactionPoints.Count > 0)
        {
            Transform best = interactionPoints[0];
            float bestDist = (best.position - actorPos).sqrMagnitude;

            for (int i = 1; i < interactionPoints.Count; i++)
            {
                float d = (interactionPoints[i].position - actorPos).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    best = interactionPoints[i];
                }
            }

            return best.position;
        }

        // 3. Fallback
        return transform.position;
    }
}
