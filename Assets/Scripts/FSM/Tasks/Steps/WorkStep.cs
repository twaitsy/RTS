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
        if (context == null)
            return TaskStepResult.FailTask("WorkStep: Context is null.", failedEventId);

        if (context.RuntimeContext == null)
            return TaskStepResult.FailTask("WorkStep: RuntimeContext is null.", failedEventId);

        if (context.ResourceTarget == null)
            return TaskStepResult.FailTask("WorkStep: Resource target is missing.", failedEventId);

        if (context.ResourceTarget.IsDepleted)
            return TaskStepResult.AdvanceStep(completedEventId);

        if (string.IsNullOrWhiteSpace(context.CarriedResourceTypeId))
            context.CarriedResourceTypeId = context.ResourceTarget.ResourceTypeId;

        if (!string.IsNullOrWhiteSpace(resourceTypeId) &&
            !string.Equals(resourceTypeId, context.ResourceTarget.ResourceTypeId, System.StringComparison.Ordinal))
        {
            return TaskStepResult.FailTask($"WorkStep: Resource type mismatch. Expected '{resourceTypeId}' but found '{context.ResourceTarget.ResourceTypeId}'.", failedEventId);
        }

        int carryCap = Mathf.Max(1, Mathf.RoundToInt(context.RuntimeContext.ResolveStat(CanonicalStatIds.Production.CarryCapacity, 1f)));
        if (context.InventoryCount >= carryCap)
            return TaskStepResult.AdvanceStep(completedEventId);

        float baseDuration = Mathf.Max(0.01f, duration);
        float throughput = ComputeGatherThroughput(context);
        context.GatherProgress += (Time.deltaTime * throughput) / baseDuration;

        if (context.GatherProgress < 1f)
            return TaskStepResult.StayOnStep();

        context.GatherProgress -= 1f;
        int requestAmount = Mathf.Max(1, gatherAmount);
        if (!context.ResourceTarget.TryGather(requestAmount, out int gathered))
            return TaskStepResult.AdvanceStep(completedEventId);

        context.InventoryCount = Mathf.Clamp(context.InventoryCount + gathered, 0, carryCap);

        if (context.InventoryCount >= carryCap || context.ResourceTarget.IsDepleted)
            return TaskStepResult.AdvanceStep(completedEventId);

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
