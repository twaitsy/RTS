using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Event")]
public class EventDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private string description;
    public string Description => description;

    [SerializeField] private float probability;
    public float Probability => probability;

    [SerializeField] private float duration;
    public float Duration => duration;

    [SerializeField] private List<string> statModifierIds = new();
    public IReadOnlyList<string> StatModifierIds => statModifierIds;

    [SerializeField] private List<NeedModifier> needModifiers = new();
    public IReadOnlyList<NeedModifier> NeedModifiers => needModifiers;

    [SerializeField] private string diseaseId;
    public string DiseaseId => diseaseId;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}