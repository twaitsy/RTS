using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Sound")]
public class SoundDefinition : ScriptableObject, IIdentifiable, IDefinitionMetadataProvider
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private DefinitionMetadata metadata = DefinitionMetadata.Create(DefinitionCategory.UI);
    public DefinitionMetadata Metadata => metadata;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private AudioClip clip;
    public AudioClip Clip => clip;

    [SerializeField] private float volume = 1f;
    public float Volume => volume;

    [SerializeField] private float pitch = 1f;
    public float Pitch => pitch;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.UI);
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);
    }
#endif
}