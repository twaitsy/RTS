using UnityEngine;

public class BuildingSpawner : MonoBehaviour
{
    private void Awake()
    {
        PrefabRegistry.Initialize();
    }

    public bool TryPlace(string buildingId, Vector3 position, Quaternion rotation)
    {
        if (BuildingRegistry.Instance == null)
        {
            Debug.LogError("Building placement aborted: BuildingRegistry.Instance is null.");
            return false;
        }

        var definition = BuildingRegistry.Instance.Get(buildingId);
        if (definition == null)
        {
            Debug.LogError($"Building placement aborted: no building definition found for id '{buildingId}'.");
            return false;
        }

        if (!PrefabRegistry.TryGet(definition.PrefabId, out var prefab))
        {
            Debug.LogError($"Building placement aborted: prefabId '{definition.PrefabId}' on building '{definition.Id}' is unresolved.");
            return false;
        }

        Instantiate(prefab, position, rotation);
        return true;
    }
}
