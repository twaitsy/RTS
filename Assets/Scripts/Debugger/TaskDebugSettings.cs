using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Debug/Task Debug Settings")]
public class TaskDebugSettings : ScriptableObject
{
    public bool QueryStepLogs = false;
    public bool MoveToStepLogs = false;
    public bool WorkStepLogs = false;
    public bool DeliverStepLogs = false;
}
