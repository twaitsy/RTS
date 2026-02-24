using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Item")]
public class ItemDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private SerializedStatContainer stats = new();
    public SerializedStatContainer Stats => stats;

    [SerializeField] private ItemCategory category;
    public ItemCategory Category => category;

    [SerializeField] private int maxStackSize = 1;
    public int MaxStackSize => maxStackSize;

    [SerializeField] private float weight;
    public float Weight => weight;

    [SerializeField] private int value;
    public int Value => value;

    [SerializeField] private string consumableId;
    public string ConsumableId => consumableId;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;

        stats ??= new();

        foreach (var duplicateStatId in stats.FindDuplicateStatIds())
        {
            Debug.LogError($"[Validation] Asset '{name}' (id: '{id}') has duplicate stat '{duplicateStatId}' in its base stat container.");
        }
    }
#endif
}
