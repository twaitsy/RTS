using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Building")]
public class BuildingDefinition : ScriptableObject, IIdentifiable, IDefinitionMetadataProvider
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private DefinitionMetadata metadata = DefinitionMetadata.Create(DefinitionCategory.Building);
    public DefinitionMetadata Metadata => metadata;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private string prefabId;
    public string PrefabId => prefabId;

    [SerializeField] private string primaryCategoryId;
    public string PrimaryCategoryId => primaryCategoryId;

    [SerializeField] private List<string> secondaryCategoryIds = new();
    public IReadOnlyList<string> SecondaryCategoryIds => secondaryCategoryIds;

    [SerializeField] private List<ResourceAmount> buildCosts = new();
    public IReadOnlyList<ResourceAmount> BuildCosts => buildCosts;

    [FormerlySerializedAs("baseStats")]
    [SerializeField] private SerializedStatContainer stats = new();
    public IReadOnlyList<StatEntry> BaseStats => stats.Entries;
    public SerializedStatContainer Stats => stats;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.Building);
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);

        stats ??= new();
        secondaryCategoryIds ??= new();

        foreach (var duplicateStatId in stats.FindDuplicateStatIds())
        {
            Debug.LogError($"[Validation] Asset '{name}' (id: '{id}') has duplicate stat '{duplicateStatId}' in its base stat container.");
        }
    }
#endif
}
