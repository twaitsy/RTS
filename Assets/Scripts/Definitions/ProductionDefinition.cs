using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Production")]
public class ProductionDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private string buildingId;
    public string BuildingId => buildingId;

    [SerializeField] private string unitId;
    public string UnitId => unitId;

    [SerializeField] private float productionTime;
    public float ProductionTime => productionTime;

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