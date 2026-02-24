using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Consumable")]
public class ConsumableDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private List<NeedAmount> needRestorations = new();
    public IReadOnlyList<NeedAmount> NeedRestorations => needRestorations;

    [SerializeField] private List<ResourceAmount> resourceCosts = new();
    public IReadOnlyList<ResourceAmount> ResourceCosts => resourceCosts;

    [SerializeField] private string itemId;
    public string ItemId => itemId;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}