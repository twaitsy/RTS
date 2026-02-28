using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    public string ResourceId; // e.g. "resource.wood"

    private void OnEnable() => ResourceLocator.Register(this);
    private void OnDisable() => ResourceLocator.Unregister(this);
}