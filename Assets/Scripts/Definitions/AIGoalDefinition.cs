using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/AIGoal")]
public class AIGoalDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

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
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}