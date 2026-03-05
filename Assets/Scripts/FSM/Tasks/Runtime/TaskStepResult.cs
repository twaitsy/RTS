public struct TaskStepResult
{
    public enum Flow
    {
        StayOnStep,
        AdvanceStep,
        JumpToStep,
        FailTask
    }

    public Flow StepFlow;
    public int NextStepIndex;
    public string EventId;
    public string EventPayload;
    public string FailureReason;

    /// <summary>
    /// Instructs <see cref="TaskRunner"/> to keep executing the current step index on the next tick.
    /// </summary>
    public static TaskStepResult StayOnStep(string eventId = null, string eventPayload = null)
    {
        return new TaskStepResult
        {
            StepFlow = Flow.StayOnStep,
            NextStepIndex = -1,
            EventId = eventId,
            EventPayload = eventPayload,
            FailureReason = null,
        };
    }

    /// <summary>
    /// Instructs <see cref="TaskRunner"/> to increment the current step index by one.
    /// If the new index is out of range, the runner marks the task complete.
    /// </summary>
    public static TaskStepResult AdvanceStep(string eventId = null, string eventPayload = null)
    {
        return new TaskStepResult
        {
            StepFlow = Flow.AdvanceStep,
            NextStepIndex = -1,
            EventId = eventId,
            EventPayload = eventPayload,
            FailureReason = null,
        };
    }

    /// <summary>
    /// Instructs <see cref="TaskRunner"/> to jump directly to <paramref name="index"/>.
    /// If the index is invalid, the runner logs a warning and completes the task.
    /// </summary>
    public static TaskStepResult Jump(int index, string eventId = null, string eventPayload = null)
    {
        return new TaskStepResult
        {
            StepFlow = Flow.JumpToStep,
            NextStepIndex = index,
            EventId = eventId,
            EventPayload = eventPayload,
            FailureReason = null,
        };
    }

    /// <summary>
    /// Instructs <see cref="TaskRunner"/> to end the task immediately on this tick.
    /// </summary>
    public static TaskStepResult FailTask(string failureReason = null, string eventId = null, string eventPayload = null)
    {
        return new TaskStepResult
        {
            StepFlow = Flow.FailTask,
            NextStepIndex = -1,
            EventId = eventId,
            EventPayload = eventPayload,
            FailureReason = failureReason,
        };
    }
}
