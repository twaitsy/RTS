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
            return TaskStepResult.Complete();
        }

        if (context.Actor == null)
        {
            Debug.LogError("MoveToStep: Context.Actor is null.");
            return TaskStepResult.Complete();
        }

        Debug.Log($"MoveToStep: START for actor '{context.Actor.name}'.");

        if (context.Target == null)
        {
            Debug.LogWarning(
                $"MoveToStep: NO TARGET set for actor '{context.Actor.name}'."
            );
            return TaskStepResult.Complete();
        }

        var comp = context.Target as Component;
        if (comp == null)
        {
            Debug.LogWarning(
                $"MoveToStep: TARGET is not a Component for actor '{context.Actor.name}'. " +
                $"Target type = {context.Target.GetType().Name}"
            );
            return TaskStepResult.Complete();
        }

        Vector3 targetPos = comp.transform.position;
        Vector3 currentPos = context.Actor.transform.position;

        float sqrDist = (targetPos - currentPos).sqrMagnitude;
        Debug.Log(
            $"MoveToStep: actor '{context.Actor.name}' at {currentPos}, " +
            $"target at {targetPos}, sqrDist={sqrDist}."
        );

        if (sqrDist < ARRIVAL_THRESHOLD * ARRIVAL_THRESHOLD)
        {
            Debug.Log(
                $"MoveToStep: ARRIVED for actor '{context.Actor.name}'. " +
                "Completing step."
            );
            return TaskStepResult.Complete();
        }

        MovementSystem.MoveTo(context.Actor, targetPos);

        Debug.Log(
            $"MoveToStep: MOVING actor '{context.Actor.name}' towards {targetPos}. " +
            "Continuing step."
        );

        return TaskStepResult.Continue();
    }
}