using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Role")]
public class RoleDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private List<StatModifier> statModifiers = new();
    public IReadOnlyList<StatModifier> StatModifiers => statModifiers;

    [SerializeField] private List<string> behaviourIds = new();
    public IReadOnlyList<string> BehaviourIds => behaviourIds;

    [SerializeField] private List<string> jobIds = new();
    public IReadOnlyList<string> JobIds => jobIds;

    [SerializeField] private List<RoleNeedMultiplier> needMultipliers = new();
    public IReadOnlyList<RoleNeedMultiplier> NeedMultipliers => needMultipliers;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;

        statModifiers ??= new();
    }
#endif
}
