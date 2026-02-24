using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Tech")]
public class TechDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private List<string> requiredTechIds = new();
    public IReadOnlyList<string> RequiredTechIds => requiredTechIds;

    [SerializeField] private List<string> statModifierIds = new();
    public IReadOnlyList<string> StatModifierIds => statModifierIds;

    [SerializeField] private List<ResourceAmount> costs = new();
    public IReadOnlyList<ResourceAmount> Costs => costs;

    [SerializeField] private float researchTime;
    public float ResearchTime => researchTime;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}