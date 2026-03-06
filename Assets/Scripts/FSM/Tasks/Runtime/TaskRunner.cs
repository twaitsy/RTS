using UnityEngine;
using Unity.Profiling;

public class TaskRunner
{
    private static readonly ProfilerMarker TickMarker = new("Simulation.TaskRunner.Tick");

    private readonly TaskDefinition task;
    private readonly TaskContext context;
    private int stepIndex;

    public bool IsComplete { get; private set; }
    public string TaskId => task?.Id;

    public TaskRunner(
        TaskDefinition task,
        GameObject actor,
        UnitRuntimeContext runtimeContext = null,
        TaskSimulationServices services = null,
        ITaskEventSink eventSink = null,
        TaskBlackboard blackboard = null)
    {
        this.task = task;
        context = new TaskContext
        {
            Actor = actor,
            RuntimeContext = runtimeContext,
            Services = services ?? TaskSimulationServices.Defaults,
            EventSink = eventSink,
            Blackboard = blackboard ?? new TaskBlackboard(),
        };
        stepIndex = 0;
        IsComplete = false;
    }

    public void SetRuntimeContext(UnitRuntimeContext runtimeContext)
    {
        context.RuntimeContext = runtimeContext;
    }

    public void Stop()
    {
        IsComplete = true;
    }

    public void Tick()
    {
        using var scope = TickMarker.Auto();

        if (IsComplete || task == null || task.Steps.Count == 0)
            return;

        if (stepIndex < 0 || stepIndex >= task.Steps.Count)
        {
            Debug.LogWarning($"TaskRunner: Invalid step index {stepIndex}. Terminating task '{task.name}'.");
            IsComplete = true;
            return;
        }

        var step = task.Steps[stepIndex];
        var result = step.Execute(context);
        context.ApplyResult(result);

        switch (result.StepFlow)
        {
            case TaskStepResult.Flow.StayOnStep:
                context.FlushQueuedEvents();
                return;

            case TaskStepResult.Flow.AdvanceStep:
                stepIndex++;
                if (stepIndex >= task.Steps.Count)
                    IsComplete = true;
                context.FlushQueuedEvents();
                return;

            case TaskStepResult.Flow.JumpToStep:
                if (result.NextStepIndex < 0 || result.NextStepIndex >= task.Steps.Count)
                {
                    Debug.LogWarning($"TaskRunner: Jump target {result.NextStepIndex} is out of range for task '{task.name}'. Terminating task.");
                    IsComplete = true;
                    context.FlushQueuedEvents();
                    return;
                }

                stepIndex = result.NextStepIndex;
                context.FlushQueuedEvents();
                return;

            case TaskStepResult.Flow.FailTask:
            default:
                IsComplete = true;
                context.FlushQueuedEvents();
                return;
        }
    }
}
