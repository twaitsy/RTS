using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/TaskSteps/Work")]
public class WorkStep : TaskStepDefinition
{
    [SerializeField] private float duration = 1f;
    [SerializeField] private int gatherAmount = 1;
    [SerializeField] private string resourceTypeId;
    [SerializeField] private string completedEventId;
    [SerializeField] private string failedEventId;

    public override TaskStepResult Execute(TaskContext context)
    {
        TaskDebug.Log(TaskDebug.Work,
            $"[WorkStep] Execute start. Carry={context?.InventoryCount}, Target={context?.ResourceTarget}");

        if (context == null)
        {
            TaskDebug.Error(TaskDebug.Work, "[WorkStep] Context is null.");
            return TaskStepResult.FailTask("WorkStep: Context is null.", failedEventId);
        }

        if (context.RuntimeContext == null)
        {
            TaskDebug.Error(TaskDebug.Work, "[WorkStep] RuntimeContext is null.");
            return TaskStepResult.FailTask("WorkStep: RuntimeContext is null.", failedEventId);
        }

        if (context.ResourceTarget == null)
        {
            TaskDebug.Error(TaskDebug.Work, "[WorkStep] Resource target is missing.");
            return TaskStepResult.FailTask("WorkStep: Resource target is missing.", failedEventId);
        }

        if (context.ResourceTarget.IsDepleted)
        {
            TaskDebug.Log(TaskDebug.Work, "[WorkStep] Node depleted → advancing.");
            return TaskStepResult.AdvanceStep(completedEventId);
        }

        if (string.IsNullOrWhiteSpace(context.CarriedResourceTypeId))
        {
            context.CarriedResourceTypeId = context.ResourceTarget.ResourceTypeId;
            TaskDebug.Log(TaskDebug.Work,
                $"[WorkStep] Setting carried resource type → {context.CarriedResourceTypeId}");
        }

        if (!string.IsNullOrWhiteSpace(resourceTypeId) &&
            !string.Equals(resourceTypeId, context.ResourceTarget.ResourceTypeId, System.StringComparison.Ordinal))
        {
            TaskDebug.Warn(TaskDebug.Work,
                $"[WorkStep] Resource type mismatch. Expected '{resourceTypeId}', got '{context.ResourceTarget.ResourceTypeId}'.");
            return TaskStepResult.FailTask("WorkStep: Resource type mismatch.", failedEventId);
        }

        int carryCap = Mathf.Max(1,
            Mathf.RoundToInt(context.RuntimeContext.ResolveStat(CanonicalStatIds.Production.CarryCapacity, 1f)));

        if (context.InventoryCount >= carryCap)
        {
            TaskDebug.Log(TaskDebug.Work,
                $"[WorkStep] Carry full ({context.InventoryCount}/{carryCap}) → advancing.");
            return TaskStepResult.AdvanceStep(completedEventId);
        }

        float baseDuration = Mathf.Max(0.01f, duration);
        float throughput = ComputeGatherThroughput(context);

        float delta = (Time.deltaTime * throughput) / baseDuration;
        context.GatherProgress += delta;

        TaskDebug.Log(TaskDebug.Work,
            $"[WorkStep] Gathering… progress={context.GatherProgress:F2}, delta={delta:F3}, throughput={throughput:F2}");

        if (context.GatherProgress < 1f)
            return TaskStepResult.StayOnStep();

        // Completed one gather tick
        context.GatherProgress -= 1f;

        int requestAmount = Mathf.Max(1, gatherAmount);
        if (!context.ResourceTarget.TryGather(requestAmount, out int gathered))
        {
            TaskDebug.Log(TaskDebug.Work,
                "[WorkStep] TryGather returned false → advancing.");
            return TaskStepResult.AdvanceStep(completedEventId);
        }

        context.InventoryCount = Mathf.Clamp(context.InventoryCount + gathered, 0, carryCap);

        TaskDebug.Log(TaskDebug.Work,
            $"[WorkStep] Gathered {gathered}. Inventory now {context.InventoryCount}/{carryCap}");

        if (context.InventoryCount >= carryCap || context.ResourceTarget.IsDepleted)
        {
            TaskDebug.Log(TaskDebug.Work,
                "[WorkStep] Inventory full or node depleted → advancing.");
            return TaskStepResult.AdvanceStep(completedEventId);
        }

        return TaskStepResult.StayOnStep();
    }

    private float ComputeGatherThroughput(TaskContext context)
    {
        if (UnitInterpreterRegistry.TryGet(context.RuntimeContext, out var interpreters) &&
            interpreters?.Work != null)
        {
            return Mathf.Max(0.01f, interpreters.Work.ComputeGatherThroughput(context.ResourceTarget));
        }

        float throughput = ProductionWorkSystem.ComputeWorkThroughput(context.RuntimeContext);
        float difficulty = Mathf.Max(0.1f, context.ResourceTarget.GatherDifficulty);
        float nodeMultiplier = Mathf.Max(0.05f, context.ResourceTarget.ThroughputMultiplier);
        return Mathf.Max(0.01f, throughput * nodeMultiplier / difficulty);
    }
}
