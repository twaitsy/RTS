using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Ability")]
public class AbilityDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private float cooldown;
    public float Cooldown => cooldown;

    [SerializeField] private float range;
    public float Range => range;

    [SerializeField] private string effectId;
    public string EffectId => effectId;

    [SerializeField] private string animationId;
    public string AnimationId => animationId;

    [SerializeField] private List<ResourceAmount> costs = new();
    public IReadOnlyList<ResourceAmount> Costs => costs;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}