using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/MapObject")]
public class MapObjectDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private bool interactable;
    public bool Interactable => interactable;

    [SerializeField] private string resourceNodeId;
    public string ResourceNodeId => resourceNodeId;

    [SerializeField] private bool blocking;
    public bool Blocking => blocking;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}