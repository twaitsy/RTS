using UnityEngine;

[DefaultExecutionOrder(10000)]
public class DefinitionValidationRunner : MonoBehaviour
{
    private static bool hasRun;
    private static bool bootstrapCreated;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (!Application.isPlaying || bootstrapCreated)
            return;

        bootstrapCreated = true;
        var runnerObject = new GameObject(nameof(DefinitionValidationRunner));
        DontDestroyOnLoad(runnerObject);
        runnerObject.hideFlags = HideFlags.HideAndDontSave;
        runnerObject.AddComponent<DefinitionValidationRunner>();
    }

    private void Start()
    {
        if (hasRun)
            return;

        hasRun = true;
        RunValidationAndLog();
    }

    public static DefinitionValidationReport RunValidation()
    {
        var report = new DefinitionValidationReport();
        var registries = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var registry in registries)
        {
            if (registry is IDefinitionRegistryValidator validator)
                validator.ValidateAll(report);
        }

        return report;
    }

    public static DefinitionValidationReport RunValidationAndLog()
    {
        var report = RunValidation();
        var summary = report.BuildSummary();

        if (report.HasErrors)
            Debug.LogError(summary);
        else
            Debug.Log(summary);

        return report;
    }
}
