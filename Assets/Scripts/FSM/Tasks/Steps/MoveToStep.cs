using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/TaskSteps/MoveTo")]
public class MoveToStep : TaskStepDefinition
{
    private const float ARRIVAL_THRESHOLD = 0.1f;

    [SerializeField] private TaskTargetType targetType = TaskTargetType.None;
    [SerializeField] private string arrivedEventId;
    [SerializeField] private string failedEventId;

    public override TaskStepResult Execute(TaskContext context)
    {
        if (context == null)
            return TaskStepResult.FailTask("MoveToStep: Context is null.", failedEventId);

        if (context.Actor == null)
            return TaskStepResult.FailTask("MoveToStep: Context.Actor is null.", failedEventId);

        var comp = ResolveTarget(context);
        if (comp == null)
            return TaskStepResult.FailTask($"MoveToStep: No target set for actor '{context.Actor.name}'.", failedEventId);

        var movement = context.Services?.Movement;
        if (movement == null)
            return TaskStepResult.FailTask("MoveToStep: Movement service is unavailable.", failedEventId);

        Vector3 targetPos = comp.transform.position;
        if (HasArrived(context, targetPos))
            return TaskStepResult.AdvanceStep(arrivedEventId);

        if (!movement.MoveTo(context.Actor, targetPos, context.RuntimeContext, out string failureReason))
            return TaskStepResult.FailTask($"MoveToStep: {failureReason}", failedEventId);

        return TaskStepResult.StayOnStep();
    }

    private Component ResolveTarget(TaskContext context)
    {
        return targetType switch
        {
            TaskTargetType.ResourceNode => context.ResourceTarget,
            TaskTargetType.Dropoff => context.DropoffTarget,
            _ => context.DropoffTarget != null ? context.DropoffTarget : context.ResourceTarget,
        };
    }

    private bool HasArrived(TaskContext context, Vector3 targetPos)
    {
        if (UnitInterpreterRegistry.TryGet(context.RuntimeContext, out var interpreters) &&
            interpreters?.Movement != null &&
            interpreters.Movement.TryGetRemainingDistance(context.Actor, out float remainingDistance))
        {
            return remainingDistance <= ARRIVAL_THRESHOLD;
        }

        Vector3 currentPos = context.Actor.transform.position;
        float sqrDist = (targetPos - currentPos).sqrMagnitude;
        return sqrDist < ARRIVAL_THRESHOLD * ARRIVAL_THRESHOLD;
    }
}
