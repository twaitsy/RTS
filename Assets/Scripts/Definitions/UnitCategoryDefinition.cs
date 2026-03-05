using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/UnitCategory")]
public class UnitCategoryDefinition : ScriptableObject, IIdentifiable, IDefinitionMetadataProvider
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private DefinitionMetadata metadata = DefinitionMetadata.Create(DefinitionCategory.Unit);
    public DefinitionMetadata Metadata => metadata;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private Sprite icon;
    public Sprite Icon => icon;

    [SerializeField] private Color color = Color.white;
    public Color Color => color;

    [SerializeField] private int sortOrder;
    public int SortOrder => sortOrder;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.Unit);
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);
        sortOrder = Mathf.Max(0, sortOrder);
    }
#endif
}
