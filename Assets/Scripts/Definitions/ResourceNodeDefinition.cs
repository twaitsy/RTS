using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/ResourceNode")]
public class ResourceNodeDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private string resourceId;
    public string ResourceId => resourceId;

    [SerializeField] private int amount;
    public int Amount => amount;

    [SerializeField] private float respawnTime;
    public float RespawnTime => respawnTime;

    [SerializeField] private float harvestTime;
    public float HarvestTime => harvestTime;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}