using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/BuildingUpgrade")]
public class BuildingUpgradeDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private List<StatModifier> statModifiers = new();
    public IReadOnlyList<StatModifier> StatModifiers => statModifiers;

    [SerializeField] private string fromBuildingId;
    public string FromBuildingId => fromBuildingId;

    [SerializeField] private string toBuildingId;
    public string ToBuildingId => toBuildingId;

    [SerializeField] private float upgradeTime;
    public float UpgradeTime => upgradeTime;

    [SerializeField] private List<ResourceAmount> costs = new();
    public IReadOnlyList<ResourceAmount> Costs => costs;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;

        statModifiers ??= new();
    }
#endif
}
