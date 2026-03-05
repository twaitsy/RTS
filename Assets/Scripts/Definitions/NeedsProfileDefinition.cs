using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/NeedsProfile")]
public class NeedsProfileDefinition : ScriptableObject, IIdentifiable, IDefinitionMetadataProvider
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

    [SerializeField] private string civilianDefinitionId;
    public string CivilianDefinitionId => civilianDefinitionId;

    [SerializeField] private List<CivilianNeedEntry> needs = new();
    public IReadOnlyList<CivilianNeedEntry> Needs => needs;

    [SerializeField] private float hungerCurve = 1f;
    public float HungerCurve => hungerCurve;

    [SerializeField] private float thirstCurve = 1f;
    public float ThirstCurve => thirstCurve;

    [SerializeField] private float fatigueCurve = 1f;
    public float FatigueCurve => fatigueCurve;

    [SerializeField] private float moraleCurve = 1f;
    public float MoraleCurve => moraleCurve;

    [SerializeField] private float stressCurve = 1f;
    public float StressCurve => stressCurve;

    [SerializeField] private float socialNeedCurve = 1f;
    public float SocialNeedCurve => socialNeedCurve;

    [SerializeField] private float criticalNeedThreshold = 0.2f;
    public float CriticalNeedThreshold => criticalNeedThreshold;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.Social);
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);
        stats ??= new();
        statModifiers ??= new();
        needs ??= new();
        hungerCurve = Mathf.Max(0f, hungerCurve);
        thirstCurve = Mathf.Max(0f, thirstCurve);
        fatigueCurve = Mathf.Max(0f, fatigueCurve);
        moraleCurve = Mathf.Max(0f, moraleCurve);
        stressCurve = Mathf.Max(0f, stressCurve);
        socialNeedCurve = Mathf.Max(0f, socialNeedCurve);
        criticalNeedThreshold = Mathf.Clamp01(criticalNeedThreshold);

        foreach (var duplicateStatId in stats.FindDuplicateStatIds())
            Debug.LogError($"[Validation] Asset '{name}' (id: '{id}') has duplicate stat '{duplicateStatId}' in its base stat container.");
    }
#endif
}
