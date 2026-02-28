public struct TaskStepResult
{
    public enum Flow
    {
        Stay,
        Advance,
        Jump,
        Terminate
    }

    public Flow StepFlow;
    public int NextStepIndex;

    public static TaskStepResult Stay() => new TaskStepResult { StepFlow = Flow.Stay, NextStepIndex = -1 };
    public static TaskStepResult Advance() => new TaskStepResult { StepFlow = Flow.Advance, NextStepIndex = -1 };
    public static TaskStepResult JumpTo(int index) => new TaskStepResult { StepFlow = Flow.Jump, NextStepIndex = index };
    public static TaskStepResult Terminate() => new TaskStepResult { StepFlow = Flow.Terminate, NextStepIndex = -1 };
}
