using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/LocomotionProfile")]
public class LocomotionProfileDefinition : ScriptableObject, IIdentifiable, IDefinitionMetadataProvider
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private DefinitionMetadata metadata = DefinitionMetadata.Create(DefinitionCategory.Unit);
    public DefinitionMetadata Metadata => metadata;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private string clipName;
    public string ClipName => clipName;

    [SerializeField] private float speed = 1f;
    public float Speed => speed;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.Unit);
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);
        speed = Mathf.Max(0f, speed);
    }
#endif
}
