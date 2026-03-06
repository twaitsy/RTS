#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ValidationPlayModeHook
{
    static ValidationPlayModeHook()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode &&
            ValidationSettings.ValidateOnPlay)
        {
            // Quiet mode: no spam, just one summary line
            var report = DefinitionValidationOrchestrator.RunValidationAndLog(quiet: true);
            Debug.Log($"[Validation] Play Mode check: {report.Issues.Count} issues, {report.ErrorCount} errors.");
        }
    }
}
#endif