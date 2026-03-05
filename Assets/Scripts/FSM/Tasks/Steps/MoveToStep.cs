using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/TaskSteps/MoveTo")]
public class MoveToStep : TaskStepDefinition
{
    private const float ARRIVAL_THRESHOLD = 0.1f;

    [SerializeField] private string arrivedEventId;
    [SerializeField] private string failedEventId;

    public override TaskStepResult Execute(TaskContext context)
    {
        if (context == null)
            return TaskStepResult.FailTask("MoveToStep: Context is null.", failedEventId);

        if (context.Actor == null)
            return TaskStepResult.FailTask("MoveToStep: Context.Actor is null.", failedEventId);

        if (context.Target == null)
            return TaskStepResult.FailTask($"MoveToStep: No target set for actor '{context.Actor.name}'.", failedEventId);

        var comp = context.Target as Component;
        if (comp == null)
        {
            return TaskStepResult.FailTask(
                $"MoveToStep: Target is not a Component for actor '{context.Actor.name}'. Target type = {context.Target.GetType().Name}",
                failedEventId);
        }

        var movement = context.Services?.Movement;
        if (movement == null)
            return TaskStepResult.FailTask("MoveToStep: Movement service is unavailable.", failedEventId);

        Vector3 targetPos = comp.transform.position;
        Vector3 currentPos = context.Actor.transform.position;

        float sqrDist = (targetPos - currentPos).sqrMagnitude;
        if (sqrDist < ARRIVAL_THRESHOLD * ARRIVAL_THRESHOLD)
            return TaskStepResult.AdvanceStep(arrivedEventId);

        movement.MoveTo(context.Actor, targetPos, context.RuntimeContext);
        return TaskStepResult.StayOnStep();
    }
}
