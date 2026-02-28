using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/ResourceNode")]
public class ResourceNodeDefinition : ScriptableObject, IIdentifiable, IDefinitionMetadataProvider
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private DefinitionMetadata metadata = DefinitionMetadata.Create(DefinitionCategory.Resource);
    public DefinitionMetadata Metadata => metadata;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private string resourceId;
    public string ResourceId => resourceId;

    [SerializeField] private int amount;
    public int Amount => amount;

    [SerializeField] private float respawnTime;
    public float RespawnTime => respawnTime;

    [SerializeField] private float harvestTime;
    public float HarvestTime => harvestTime;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.Resource);
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);
    }
#endif
}