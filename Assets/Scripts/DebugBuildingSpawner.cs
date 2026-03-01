using UnityEngine;
using UnityEngine.InputSystem; // IMPORTANT

public class DebugBuildingSpawner : MonoBehaviour
{
    [SerializeField] private string buildingId = "building.house";

    private void Update()
    {
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            SpawnHouse();
        }
    }

    private void SpawnHouse()
    {
        if (BuildingRegistry.Instance == null)
        {
            Debug.LogError("Building placement aborted: BuildingRegistry.Instance is null.");
            return;
        }

        PrefabRegistry.Initialize();

        var definition = BuildingRegistry.Instance.Get(buildingId);
        if (definition == null)
        {
            Debug.LogError($"Building placement aborted: missing BuildingDefinition '{buildingId}'.");
            return;
        }

        if (!PrefabRegistry.TryGet(definition.PrefabId, out var prefab))
        {
            Debug.LogError($"Building placement aborted: BuildingDefinition '{definition.Id}' references unresolved prefabId '{definition.PrefabId}'.");
            return;
        }

        Vector3 spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 5f;
        Instantiate(prefab, spawnPos, Quaternion.identity);

        Debug.Log($"Spawned house: {definition.Id}");
    }
}
