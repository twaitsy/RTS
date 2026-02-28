using UnityEngine;
using UnityEngine.InputSystem; // IMPORTANT

public class DebugBuildingSpawner : MonoBehaviour
{
    [SerializeField] private BuildingDefinition houseDefinition;
    [SerializeField] private GameObject housePrefab;

    private void Update()
    {
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            SpawnHouse();
        }
    }

    private void SpawnHouse()
    {
        if (housePrefab == null)
        {
            Debug.LogError("No house prefab assigned.");
            return;
        }

        Vector3 spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 5f;
        Instantiate(housePrefab, spawnPos, Quaternion.identity);

        Debug.Log($"Spawned house: {houseDefinition.Id}");
    }
}