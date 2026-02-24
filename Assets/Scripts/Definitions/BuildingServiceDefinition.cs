using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/BuildingService")]
public class BuildingServiceDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private string buildingId;
    public string BuildingId => buildingId;

    [SerializeField] private string needId;
    public string NeedId => needId;

    [SerializeField] private float satisfactionPerSecond = 10f;
    public float SatisfactionPerSecond => satisfactionPerSecond;

    [SerializeField] private int capacity = 1;
    public int Capacity => capacity;

    [SerializeField] private List<ResourceAmount> operatingCosts = new();
    public IReadOnlyList<ResourceAmount> OperatingCosts => operatingCosts;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}