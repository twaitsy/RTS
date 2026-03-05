using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/AIPerception")]
public class AIPerceptionDefinition : ScriptableObject, IIdentifiable, IDefinitionMetadataProvider
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private DefinitionMetadata metadata = DefinitionMetadata.Create(DefinitionCategory.AI);
    public DefinitionMetadata Metadata => metadata;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private SerializedStatContainer stats = new();
    public SerializedStatContainer Stats => stats;

    [SerializeField] private float visionArc = 120f;
    public float VisionArc => visionArc;

    [SerializeField] private float hearingRadius = 8f;
    public float HearingRadius => hearingRadius;

    [SerializeField] private float stealthDetection = 0.25f;
    public float StealthDetection => stealthDetection;

    [SerializeField] private float alertnessDecay = 0.5f;
    public float AlertnessDecay => alertnessDecay;

    [SerializeField] private float memoryDuration = 6f;
    public float MemoryDuration => memoryDuration;

    [SerializeField] private Sprite icon;
    public Sprite Icon => icon;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.AI);
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);

        stats ??= new();

        foreach (var duplicateStatId in stats.FindDuplicateStatIds())
        {
            Debug.LogError($"[Validation] Asset '{name}' (id: '{id}') has duplicate stat '{duplicateStatId}' in its base stat container.");
        }
    }
#endif
}
