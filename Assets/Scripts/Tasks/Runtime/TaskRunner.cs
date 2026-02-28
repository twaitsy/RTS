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

        var step = task.Steps[stepIndex];
        var result = step.Execute(context);

        if (result.IsComplete)
        {
            IsComplete = true;
            return;
        }

        if (result.NextStepIndex >= 0)
        {
            stepIndex = result.NextStepIndex;
        }
        else
        {
            stepIndex++;
            if (stepIndex >= task.Steps.Count)
                IsComplete = true;
        }
    }
}