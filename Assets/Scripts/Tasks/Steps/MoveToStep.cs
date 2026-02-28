using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/TaskSteps/MoveTo")]
public class MoveToStep : TaskStepDefinition
{
    private const float ARRIVAL_THRESHOLD = 0.1f;

    public override TaskStepResult Execute(TaskContext context)
    {
        if (context == null)
        {
            Debug.LogError("MoveToStep: Context is null.");
            return TaskStepResult.FailTask();
        }

        if (context.Actor == null)
        {
            Debug.LogError("MoveToStep: Context.Actor is null.");
            return TaskStepResult.FailTask();
        }

        if (context.Target == null)
        {
            Debug.LogWarning($"MoveToStep: No target set for actor '{context.Actor.name}'.");
            return TaskStepResult.FailTask();
        }

        var comp = context.Target as Component;
        if (comp == null)
        {
            Debug.LogWarning(
                $"MoveToStep: Target is not a Component for actor '{context.Actor.name}'. " +
                $"Target type = {context.Target.GetType().Name}"
            );
            return TaskStepResult.FailTask();
        }

        Vector3 targetPos = comp.transform.position;
        Vector3 currentPos = context.Actor.transform.position;

        float sqrDist = (targetPos - currentPos).sqrMagnitude;
        if (sqrDist < ARRIVAL_THRESHOLD * ARRIVAL_THRESHOLD)
            return TaskStepResult.AdvanceStep();

        MovementSystem.MoveTo(context.Actor, targetPos);
        return TaskStepResult.StayOnStep();
    }
}
