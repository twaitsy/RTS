#if UNITY_EDITOR
using UnityEditor;

public static class ValidationSettings
{
    private const string MenuPath = "Tools/Validation/Validate On Play";
    private const string PrefKey = "Validation.ValidateOnPlay";

    public static bool ValidateOnPlay
    {
        get => EditorPrefs.GetBool(PrefKey, false);
        set => EditorPrefs.SetBool(PrefKey, value);
    }

    [MenuItem(MenuPath)]
    private static void Toggle()
    {
        ValidateOnPlay = !ValidateOnPlay;
        Menu.SetChecked(MenuPath, ValidateOnPlay);
    }

    [MenuItem(MenuPath, true)]
    private static bool ToggleValidate()
    {
        Menu.SetChecked(MenuPath, ValidateOnPlay);
        return true;
    }
}
#endif