using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum ResourceNodeDepletionMode
{
    FiniteExhausted = 0,
    FiniteRespawn = 1,
    Infinite = 2,
}

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/ResourceNode")]
public class ResourceNodeDefinition : ScriptableObject, IIdentifiable, IDefinitionMetadataProvider
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private DefinitionMetadata metadata = DefinitionMetadata.Create(DefinitionCategory.Resource);
    public DefinitionMetadata Metadata => metadata;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [FormerlySerializedAs("resourceId")]
    [SerializeField] private string resourceTypeId;
    public string ResourceTypeId => resourceTypeId;
    public string ResourceId => resourceTypeId;

    [SerializeField, Min(1)] private int amount = 10;
    public int Amount => amount;

    [SerializeField, Min(0f)] private float respawnTime;
    public float RespawnTime => respawnTime;

    [SerializeField, Min(0.01f)] private float harvestTime = 1f;
    public float HarvestTime => harvestTime;

    [SerializeField, Min(0.1f)] private float gatherDifficulty = 1f;
    public float GatherDifficulty => gatherDifficulty;

    [SerializeField] private ResourceNodeDepletionMode depletionMode = ResourceNodeDepletionMode.FiniteRespawn;
    public ResourceNodeDepletionMode DepletionMode => depletionMode;

    [SerializeField, Min(0.1f)] private float interactionRadius = 1.25f;
    public float InteractionRadius => interactionRadius;

    [SerializeField, Min(0.05f)] private float throughputMultiplier = 1f;
    public float ThroughputMultiplier => throughputMultiplier;

    [SerializeField] private List<Vector3> interactionPoints = new();
    public IReadOnlyList<Vector3> InteractionPoints => interactionPoints;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.Resource);
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);

        amount = Mathf.Max(1, amount);
        harvestTime = Mathf.Max(0.01f, harvestTime);
        gatherDifficulty = Mathf.Max(0.1f, gatherDifficulty);
        interactionRadius = Mathf.Max(0.1f, interactionRadius);
        throughputMultiplier = Mathf.Max(0.05f, throughputMultiplier);

        interactionPoints ??= new List<Vector3>();
    }
#endif
}
