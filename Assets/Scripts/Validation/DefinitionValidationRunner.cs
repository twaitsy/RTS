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
        return DefinitionValidationOrchestrator.RunValidation();
    }

    public static DefinitionValidationReport RunValidationAndLog()
    {
        return DefinitionValidationOrchestrator.RunValidationAndLog();
    }
}
