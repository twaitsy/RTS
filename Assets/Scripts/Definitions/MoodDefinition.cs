using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Mood")]
public class MoodDefinition : ScriptableObject, IIdentifiable, IDefinitionMetadataProvider
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private DefinitionMetadata metadata = DefinitionMetadata.Create(DefinitionCategory.Social);
    public DefinitionMetadata Metadata => metadata;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private SerializedStatContainer stats = new();
    public SerializedStatContainer Stats => stats;

    [SerializeField] private List<StatModifier> statModifiers = new();
    public IReadOnlyList<StatModifier> StatModifiers => statModifiers;

    [SerializeField] private float moraleStability = 1f;
    public float MoraleStability => moraleStability;

    [SerializeField] private float stressRecoveryRate = 0.25f;
    public float StressRecoveryRate => stressRecoveryRate;

    [SerializeField] private float panicThreshold = 0.2f;
    public float PanicThreshold => panicThreshold;

    [SerializeField] private List<string> personalityTraits = new();
    public IReadOnlyList<string> PersonalityTraits => personalityTraits;

    [SerializeField] private Sprite icon;
    public Sprite Icon => icon;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.Social);
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);

        stats ??= new();
        statModifiers ??= new();
        personalityTraits ??= new();
        moraleStability = Mathf.Max(0f, moraleStability);
        stressRecoveryRate = Mathf.Max(0f, stressRecoveryRate);
        panicThreshold = Mathf.Clamp01(panicThreshold);

        foreach (var duplicateStatId in stats.FindDuplicateStatIds())
        {
            Debug.LogError($"[Validation] Asset '{name}' (id: '{id}') has duplicate stat '{duplicateStatId}' in its base stat container.");
        }
    }
#endif
}
