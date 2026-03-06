using UnityEngine;

public class TaskDebugBootstrap : MonoBehaviour
{
    [Header("Assign your TaskDebugSettings asset here")]
    [SerializeField] private TaskDebugSettings settings;

    private void Awake()
    {
        if (settings == null)
        {
            Debug.LogWarning("[TaskDebugBootstrap] No TaskDebugSettings assigned. Debug toggles will be disabled.");
            return;
        }

        TaskDebug.Load(settings);
        Debug.Log("[TaskDebugBootstrap] Task debug settings loaded.");
    }
}
