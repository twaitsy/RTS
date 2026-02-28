using UnityEngine;

public class TaskRunner
{
    private TaskDefinition task;
    private TaskContext context;
    private int stepIndex;

    public bool IsComplete { get; private set; }

    public TaskRunner(TaskDefinition task, GameObject actor)
    {
        this.task = task;
        this.context = new TaskContext { Actor = actor };
        this.stepIndex = 0;
        this.IsComplete = false;
    }

    public void Tick()
    {
        if (IsComplete || task.Steps.Count == 0)
            return;

        if (stepIndex < 0 || stepIndex >= task.Steps.Count)
        {
            Debug.LogWarning($"TaskRunner: Invalid step index {stepIndex}. Terminating task '{task.name}'.");
            IsComplete = true;
            return;
        }

        var step = task.Steps[stepIndex];
        var result = step.Execute(context);

        switch (result.StepFlow)
        {
            case TaskStepResult.Flow.StayOnStep:
                return;

            case TaskStepResult.Flow.AdvanceStep:
                stepIndex++;
                if (stepIndex >= task.Steps.Count)
                    IsComplete = true;
                return;

            case TaskStepResult.Flow.JumpToStep:
                if (result.NextStepIndex < 0 || result.NextStepIndex >= task.Steps.Count)
                {
                    Debug.LogWarning($"TaskRunner: Jump target {result.NextStepIndex} is out of range for task '{task.name}'. Terminating task.");
                    IsComplete = true;
                    return;
                }

                stepIndex = result.NextStepIndex;
                return;

            case TaskStepResult.Flow.FailTask:
            default:
                IsComplete = true;
                return;
        }
    }
}
