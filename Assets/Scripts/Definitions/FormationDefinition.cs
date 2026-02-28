using UnityEngine;

public enum FormationShape
{
    Line,
    Column,
    Square,
    Wedge
}

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Formation")]
public class FormationDefinition : ScriptableObject, IIdentifiable, IDefinitionMetadataProvider
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private DefinitionMetadata metadata = DefinitionMetadata.Create(DefinitionCategory.Combat);
    public DefinitionMetadata Metadata => metadata;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private FormationShape shape;
    public FormationShape Shape => shape;

    [SerializeField] private float spacing = 1.5f;
    public float Spacing => spacing;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.Combat);
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);
    }
#endif
}