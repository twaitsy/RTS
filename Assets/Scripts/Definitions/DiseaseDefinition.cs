using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Disease")]
public class DiseaseDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private float incubationTime;
    public float IncubationTime => incubationTime;

    [SerializeField] private float duration;
    public float Duration => duration;

    [SerializeField] private float infectionChance;
    public float InfectionChance => infectionChance;

    [SerializeField] private List<NeedModifier> needEffects = new();
    public IReadOnlyList<NeedModifier> NeedEffects => needEffects;

    [SerializeField] private List<string> statModifierIds = new();
    public IReadOnlyList<string> StatModifierIds => statModifierIds;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}