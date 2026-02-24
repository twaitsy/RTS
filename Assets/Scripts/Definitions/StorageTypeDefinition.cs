using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/StorageType")]
public class StorageTypeDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private int capacity;
    public int Capacity => capacity;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}