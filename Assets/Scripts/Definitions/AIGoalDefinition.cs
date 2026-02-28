using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/AIGoal")]
public class AIGoalDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private SerializedStatContainer stats = new();
    public SerializedStatContainer Stats => stats;

    [SerializeField] private int priority;
    public int Priority => priority;

    [SerializeField] private List<string> requiredNeedIds = new();
    public IReadOnlyList<string> RequiredNeedIds => requiredNeedIds;

    [SerializeField] private AIGoalTargetType targetType;
    public AIGoalTargetType TargetType => targetType;

    [SerializeField] private string commandId;
    public string CommandId => commandId;

    [SerializeField] private string conditionsDescription;
    public string ConditionsDescription => conditionsDescription;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);

        stats ??= new();

        foreach (var duplicateStatId in stats.FindDuplicateStatIds())
        {
            Debug.LogError($"[Validation] Asset '{name}' (id: '{id}') has duplicate stat '{duplicateStatId}' in its base stat container.");
        }
    }
#endif
}
