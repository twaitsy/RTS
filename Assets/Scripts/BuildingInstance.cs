using UnityEngine;

public class BuildingInstance : MonoBehaviour
{
    [SerializeField] private BuildingDefinition definition;
    public BuildingDefinition Definition => definition;

    private void Awake()
    {
        if (definition == null)
        {
            Debug.LogError($"BuildingInstance on '{name}' has no BuildingDefinition assigned.");
        }
    }
}