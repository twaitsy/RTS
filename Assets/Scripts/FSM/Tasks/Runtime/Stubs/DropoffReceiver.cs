using UnityEngine;

public class DropoffReceiver : MonoBehaviour
{
    [SerializeField] private BuildingDefinition buildingDefinition;
    private BuildingRuntime runtime;

    private void OnEnable()
    {
        EnsureRuntimeAdapter();
    }

    private void OnDisable()
    {
        if (runtime != null)
            DropoffLocator.Unregister(runtime);
    }

    public void Receive(int amount)
    {
        EnsureRuntimeAdapter();
        if (runtime == null)
        {
            Debug.LogWarning($"{name} could not receive resources because BuildingRuntime is unavailable.");
            return;
        }

        if (!runtime.TryReceiveDelivery("resource.wood", amount, out int accepted, out string reason))
        {
            Debug.LogWarning($"{name} failed to receive resources. {reason}");
            return;
        }

        Debug.Log($"{name} received {accepted} items.");
    }

    private void EnsureRuntimeAdapter()
    {
        runtime = GetComponent<BuildingRuntime>();
        if (runtime == null)
            runtime = gameObject.AddComponent<BuildingRuntime>();

        if (runtime.Definition == null && buildingDefinition != null)
            runtime.SetDefinition(buildingDefinition);
    }
}
