#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

internal static class StatIdValidationSettings
{
    private const string StrictModeSuffix = "Validation.StrictCanonicalStatIds";
    private static string ProjectKey => $"{Application.productName}.{Application.dataPath}.{StrictModeSuffix}";

    public static bool StrictCanonicalStatIds
    {
        get => EditorPrefs.GetBool(ProjectKey, false);
        set => EditorPrefs.SetBool(ProjectKey, value);
    }
}
#endif
