using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/TaskSteps/Work")]
public class WorkStep : TaskStepDefinition
{
    [SerializeField] private float duration = 1f;
    [SerializeField] private int gatherAmount = 1;
    [SerializeField] private string completedEventId;
    [SerializeField] private string failedEventId;

    public override TaskStepResult Execute(TaskContext context)
    {
        if (context == null)
            return TaskStepResult.FailTask("WorkStep: Context is null.", failedEventId);

        if (context.RuntimeContext == null)
            return TaskStepResult.FailTask("WorkStep: RuntimeContext is null.", failedEventId);

        var configuredDuration = Mathf.Max(0.01f, duration);
        var throughput = ProductionWorkSystem.ComputeWorkThroughput(context.RuntimeContext);
        var speedFactor = Mathf.Max(0.1f, throughput <= 0f ? 1f : throughput);
        var effectiveDuration = configuredDuration / speedFactor;

        if (context.WorkTimer <= 0f)
            context.WorkTimer = effectiveDuration;

        context.WorkTimer -= Time.deltaTime;

        if (context.WorkTimer > 0f)
            return TaskStepResult.StayOnStep();

        context.WorkTimer = 0f;

        int carryCap = Mathf.Max(1, Mathf.RoundToInt(context.RuntimeContext.ResolveStat(CanonicalStatIds.Production.CarryCapacity, 1f)));
        context.InventoryCount = Mathf.Clamp(context.InventoryCount + Mathf.Max(1, gatherAmount), 0, carryCap);

        return TaskStepResult.AdvanceStep(completedEventId);
    }
}
