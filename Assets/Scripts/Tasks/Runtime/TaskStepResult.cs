public struct TaskStepResult
{
    public bool IsComplete;
    public int NextStepIndex; // -1 = go to next step automatically

    public static TaskStepResult Continue() => new TaskStepResult { IsComplete = false, NextStepIndex = -1 };
    public static TaskStepResult Jump(int index) => new TaskStepResult { IsComplete = false, NextStepIndex = index };
    public static TaskStepResult Complete() => new TaskStepResult { IsComplete = true };
}