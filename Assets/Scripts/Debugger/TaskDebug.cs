using UnityEngine;

public static class TaskDebug
{
    private static TaskDebugSettings settings;

    public static void Load(TaskDebugSettings s)
    {
        settings = s;
    }

    public static bool Query => settings != null && settings.QueryStepLogs;
    public static bool MoveTo => settings != null && settings.MoveToStepLogs;
    public static bool Work => settings != null && settings.WorkStepLogs;
    public static bool Deliver => settings != null && settings.DeliverStepLogs;

    public static void Log(bool enabled, string msg)
    {
        if (enabled)
            Debug.Log(msg);
    }

    public static void Warn(bool enabled, string msg)
    {
        if (enabled)
            Debug.LogWarning(msg);
    }

    public static void Error(bool enabled, string msg)
    {
        if (enabled)
            Debug.LogError(msg);
    }
}
