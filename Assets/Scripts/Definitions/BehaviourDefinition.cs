using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Behaviour")]
public class BehaviourDefinition : ScriptableObject, IIdentifiable, IDefinitionMetadataProvider
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private DefinitionMetadata metadata = DefinitionMetadata.Create(DefinitionCategory.AI);
    public DefinitionMetadata Metadata => metadata;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private int priority;
    public int Priority => priority;

    [SerializeField] private SerializedStatContainer stats = new();
    public SerializedStatContainer Stats => stats;

    [SerializeField] private List<StatModifier> statModifiers = new();
    public IReadOnlyList<StatModifier> StatModifiers => statModifiers;

    [SerializeField] private List<string> jobIds = new();
    public IReadOnlyList<string> JobIds => jobIds;

    [SerializeField] private float decisionInterval = 0.25f;
    public float DecisionInterval => decisionInterval;

    [SerializeField] private float reactionTime = 0.1f;
    public float ReactionTime => reactionTime;

    [SerializeField] private string preferredMoodId;
    public string PreferredMoodId => preferredMoodId;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.AI);
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);

        stats ??= new();
        statModifiers ??= new();
        jobIds ??= new();
        decisionInterval = Mathf.Max(0f, decisionInterval);
        reactionTime = Mathf.Max(0f, reactionTime);
        UnitRuntimeContextResolver.ClearCache();

        foreach (var duplicateStatId in stats.FindDuplicateStatIds())
            Debug.LogError($"[Validation] Asset '{name}' (id: '{id}') has duplicate stat '{duplicateStatId}' in its base stat container.");
    }
#endif
}
