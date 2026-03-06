using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "DataDrivenRTS/Buildings/Building")]
public class BuildingDefinition : ScriptableObject, IIdentifiable, IDefinitionMetadataProvider
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private DefinitionMetadata metadata = DefinitionMetadata.Create(DefinitionCategory.Building);
    public DefinitionMetadata Metadata => metadata;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private string prefabId;
    public string PrefabId => prefabId;

    [SerializeField] private string primaryCategoryId;
    public string PrimaryCategoryId => primaryCategoryId;

    [SerializeField] private List<string> secondaryCategoryIds = new();
    public IReadOnlyList<string> SecondaryCategoryIds => secondaryCategoryIds;

    [SerializeField] private List<ResourceAmount> buildCosts = new();
    public IReadOnlyList<ResourceAmount> BuildCosts => buildCosts;

    [FormerlySerializedAs("baseStats")]
    [SerializeField] private SerializedStatContainer stats = new();
    public IReadOnlyList<StatEntry> BaseStats => stats.Entries;
    public SerializedStatContainer Stats => stats;

    [Header("Dropoff/Storage")]
    [SerializeField] private bool supportsDropoff;
    public bool SupportsDropoff => supportsDropoff;

    [SerializeField] private List<string> acceptedResourceTypeIds = new();
    public IReadOnlyList<string> AcceptedResourceTypeIds => acceptedResourceTypeIds;

    [SerializeField, Min(0)] private int storageCapacity;
    public int StorageCapacity => storageCapacity;

    [SerializeField] private bool allowPartialDelivery = true;
    public bool AllowPartialDelivery => allowPartialDelivery;

    [SerializeField, Min(0.1f)] private float interactionRadius = 1.5f;
    public float InteractionRadius => interactionRadius;

    [SerializeField] private List<Vector3> interactionPoints = new();
    public IReadOnlyList<Vector3> InteractionPoints => interactionPoints;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.Building);
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);

        stats ??= new();
        secondaryCategoryIds ??= new();
        acceptedResourceTypeIds ??= new();
        interactionPoints ??= new();
        storageCapacity = Mathf.Max(0, storageCapacity);
        interactionRadius = Mathf.Max(0.1f, interactionRadius);

        foreach (var duplicateStatId in stats.FindDuplicateStatIds())
        {
            Debug.LogError($"[Validation] Asset '{name}' (id: '{id}') has duplicate stat '{duplicateStatId}' in its base stat container.");
        }
    }
#endif
}
