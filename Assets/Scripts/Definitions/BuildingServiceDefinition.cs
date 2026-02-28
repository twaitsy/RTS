using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/BuildingService")]
public class BuildingServiceDefinition : ScriptableObject, IIdentifiable, IDefinitionMetadataProvider
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private DefinitionMetadata metadata = DefinitionMetadata.Create(DefinitionCategory.Building);
    public DefinitionMetadata Metadata => metadata;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

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
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.Building);
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);
    }
#endif
}