using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Recipe")]
public class RecipeDefinition : ScriptableObject, IIdentifiable, IDefinitionMetadataProvider
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private DefinitionMetadata metadata = DefinitionMetadata.Create(DefinitionCategory.Production);
    public DefinitionMetadata Metadata => metadata;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private List<ItemAmount> inputs = new();
    public IReadOnlyList<ItemAmount> Inputs => inputs;

    [SerializeField] private List<ItemAmount> outputs = new();
    public IReadOnlyList<ItemAmount> Outputs => outputs;

    [SerializeField] private string buildingId;
    public string BuildingId => buildingId;

    [SerializeField] private string jobId;
    public string JobId => jobId;

    [SerializeField] private float craftTime;
    public float CraftTime => craftTime;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.Production);
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);
    }
#endif
}