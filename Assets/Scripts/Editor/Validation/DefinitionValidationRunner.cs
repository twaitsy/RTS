using UnityEngine;

[DefaultExecutionOrder(10000)]
public class DefinitionValidationRunner : MonoBehaviour
{
    private static bool hasRun;
    private static bool bootstrapCreated;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying || bootstrapCreated)
            return;

        bootstrapCreated = true;

        var runnerObject = new GameObject(nameof(DefinitionValidationRunner));
        DontDestroyOnLoad(runnerObject);
        runnerObject.hideFlags = HideFlags.HideAndDontSave;
      //  runnerObject.AddComponent<DefinitionValidationRunner>();
#endif
    }

    private void Start()
    {
#if UNITY_EDITOR
        if (hasRun)
            return;

        hasRun = true;

        // Only run validation if the toggle is enabled
        if (ValidationSettings.ValidateOnPlay)
        {
            var report = DefinitionValidationOrchestrator.RunValidationAndLog(quiet: true);
            Debug.Log($"[Validation] Play Mode check: {report.Issues.Count} issues, {report.ErrorCount} errors.");
        }
#endif
    }

    public static DefinitionValidationReport RunValidation()
    {
        return DefinitionValidationOrchestrator.RunValidation();
    }

    public static DefinitionValidationReport RunValidationAndLog()
    {
        return DefinitionValidationOrchestrator.RunValidationAndLog();
    }
}
