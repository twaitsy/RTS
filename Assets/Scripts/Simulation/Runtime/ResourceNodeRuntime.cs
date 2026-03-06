using System.Collections.Generic;
using UnityEngine;

public class ResourceNodeRuntime : MonoBehaviour
{
    [SerializeField] private ResourceNodeDefinition definition;
    [SerializeField] private string fallbackResourceTypeId;
    [SerializeField, Min(1)] private int fallbackAmount = 9999;
    [SerializeField, Min(0.1f)] private float fallbackGatherDifficulty = 1f;
    [SerializeField, Min(0.1f)] private float fallbackInteractionRadius = 1.25f;
    [SerializeField, Min(0.05f)] private float fallbackThroughputMultiplier = 1f;

    [Header("Scene Interaction Points")]
    [SerializeField] private List<Transform> interactionPoints = new();
    public IReadOnlyList<Transform> InteractionPoints => interactionPoints;

    private float respawnTimer;
    private bool isDepleted;

    public ResourceNodeDefinition Definition => definition;
    public string ResourceTypeId => definition != null ? definition.ResourceTypeId : fallbackResourceTypeId;
    public int CurrentAmount { get; private set; }
    public bool IsDepleted => isDepleted;

    private void Awake() => ResetNode();
    private void OnEnable() => ResourceLocator.Register(this);
    private void OnDisable() => ResourceLocator.Unregister(this);

    public void SetDefinition(ResourceNodeDefinition nextDefinition, bool reset = true)
    {
        if (isActiveAndEnabled)
            ResourceLocator.Unregister(this);

        definition = nextDefinition;
        if (reset)
            ResetNode();

        if (isActiveAndEnabled)
            ResourceLocator.Register(this);
    }

    public void SetFallbackResourceType(string resourceTypeId)
    {
        fallbackResourceTypeId = resourceTypeId;
        if (isActiveAndEnabled)
        {
            ResourceLocator.Unregister(this);
            ResourceLocator.Register(this);
        }
    }

    private void Update()
    {
        if (!isDepleted || definition == null || definition.DepletionMode != ResourceNodeDepletionMode.FiniteRespawn)
            return;

        respawnTimer -= Time.deltaTime;
        if (respawnTimer <= 0f)
            ResetNode();
    }

    public bool TryGather(int requestedAmount, out int gatheredAmount)
    {
        gatheredAmount = 0;

        if ((definition == null && string.IsNullOrWhiteSpace(fallbackResourceTypeId)) || requestedAmount <= 0)
            return false;

        if (definition == null || definition.DepletionMode == ResourceNodeDepletionMode.Infinite)
        {
            gatheredAmount = requestedAmount;
            return true;
        }

        if (isDepleted || CurrentAmount <= 0)
            return false;

        gatheredAmount = Mathf.Min(requestedAmount, CurrentAmount);
        CurrentAmount -= gatheredAmount;

        if (CurrentAmount <= 0)
        {
            CurrentAmount = 0;
            isDepleted = true;
            respawnTimer = Mathf.Max(0f, definition.RespawnTime);
        }

        return gatheredAmount > 0;
    }

    public void ResetNode()
    {
        CurrentAmount = definition != null ? Mathf.Max(1, definition.Amount) : Mathf.Max(1, fallbackAmount);
        isDepleted = false;
        respawnTimer = 0f;
    }

    public float GatherDifficulty => definition != null ? definition.GatherDifficulty : fallbackGatherDifficulty;
    public float ThroughputMultiplier => definition != null ? definition.ThroughputMultiplier : fallbackThroughputMultiplier;
    public float InteractionRadius => definition != null ? definition.InteractionRadius : fallbackInteractionRadius;

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
