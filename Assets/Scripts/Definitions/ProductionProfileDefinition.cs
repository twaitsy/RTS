using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/ProductionProfile")]
public class ProductionProfileDefinition : ScriptableObject, IIdentifiable, IDefinitionMetadataProvider
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private DefinitionMetadata metadata = DefinitionMetadata.Create(DefinitionCategory.Production);
    public DefinitionMetadata Metadata => metadata;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private SerializedStatContainer stats = new();
    public SerializedStatContainer Stats => stats;

    [SerializeField] private List<StatModifier> statModifiers = new();
    public IReadOnlyList<StatModifier> StatModifiers => statModifiers;

    [SerializeField] private string buildingId;
    public string BuildingId => buildingId;

    [SerializeField] private string unitId;
    public string UnitId => unitId;

    [SerializeField] private float productionTime;
    public float ProductionTime => productionTime;

    [SerializeField] private int maxQueueSize = 1;
    public int MaxQueueSize => maxQueueSize;

    [SerializeField] private bool allowParallelQueue;
    public bool AllowParallelQueue => allowParallelQueue;

    [SerializeField] private List<ResourceAmount> costs = new();
    public IReadOnlyList<ResourceAmount> Costs => costs;

    [SerializeField] private List<string> unlockTechIds = new();
    public IReadOnlyList<string> UnlockTechIds => unlockTechIds;

    [SerializeField] private List<string> unlockUnitIds = new();
    public IReadOnlyList<string> UnlockUnitIds => unlockUnitIds;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.Production);
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);

        stats ??= new();
        statModifiers ??= new();
        costs ??= new();
        unlockTechIds ??= new();
        unlockUnitIds ??= new();
        productionTime = Mathf.Max(0f, productionTime);
        maxQueueSize = Mathf.Max(1, maxQueueSize);
        UnitRuntimeContextResolver.ClearCache();

        foreach (var duplicateStatId in stats.FindDuplicateStatIds())
            Debug.LogError($"[Validation] Asset '{name}' (id: '{id}') has duplicate stat '{duplicateStatId}' in its base stat container.");
    }
#endif
}
