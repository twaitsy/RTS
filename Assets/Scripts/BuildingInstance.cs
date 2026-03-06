using UnityEngine;

public class BuildingInstance : MonoBehaviour
{
    [SerializeField] private BuildingDefinition definition;
    public BuildingDefinition Definition => definition;
    private BuildingRuntime runtime;

    private void Awake()
    {
        if (definition == null)
        {
            Debug.LogError($"BuildingInstance on '{name}' has no BuildingDefinition assigned.");
            return;
        }

        runtime = GetComponent<BuildingRuntime>();
        if (runtime == null)
            runtime = gameObject.AddComponent<BuildingRuntime>();

        runtime.SetDefinition(definition);
    }
}
