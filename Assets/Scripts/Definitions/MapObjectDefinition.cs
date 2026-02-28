using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/MapObject")]
public class MapObjectDefinition : ScriptableObject, IIdentifiable, IDefinitionMetadataProvider
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private DefinitionMetadata metadata = DefinitionMetadata.Create(DefinitionCategory.World);
    public DefinitionMetadata Metadata => metadata;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private bool interactable;
    public bool Interactable => interactable;

    [SerializeField] private string resourceNodeId;
    public string ResourceNodeId => resourceNodeId;

    [SerializeField] private bool blocking;
    public bool Blocking => blocking;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.World);
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);
    }
#endif
}