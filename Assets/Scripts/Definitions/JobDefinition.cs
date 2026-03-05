using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Job")]
public class JobDefinition : ScriptableObject, IIdentifiable, IDefinitionMetadataProvider
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

    [SerializeField] private float baseWorkTime;
    public float BaseWorkTime => baseWorkTime;

    [SerializeField] private int workPriority = 1;
    public int WorkPriority => workPriority;

    [SerializeField] private List<string> allowedActionIds = new();
    public IReadOnlyList<string> AllowedActionIds => allowedActionIds;

    [SerializeField] private List<string> preferredNeedIds = new();
    public IReadOnlyList<string> PreferredNeedIds => preferredNeedIds;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.Social);
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);

        stats ??= new();
        statModifiers ??= new();
        allowedActionIds ??= new();
        preferredNeedIds ??= new();
        baseWorkTime = Mathf.Max(0f, baseWorkTime);
        workPriority = Mathf.Max(0, workPriority);

        foreach (var duplicateStatId in stats.FindDuplicateStatIds())
        {
            Debug.LogError($"[Validation] Asset '{name}' (id: '{id}') has duplicate stat '{duplicateStatId}' in its base stat container.");
        }
    }
#endif
}
