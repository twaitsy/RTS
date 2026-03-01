using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/TaskSteps/Deliver")]
public class DeliverStep : TaskStepDefinition
{
    private const float ARRIVAL_THRESHOLD = 0.1f;

    public override TaskStepResult Execute(TaskContext context)
    {
        if (context == null || context.Actor == null)
        {
            Debug.LogError("DeliverStep: Context or Actor is null.");
            return TaskStepResult.FailTask();
        }

        // Find nearest drop-off point
        DropoffReceiver dropoff = DropoffLocator.FindNearest(context.Actor.transform.position);

        if (dropoff == null)
        {
            Debug.LogWarning($"{context.Actor.name} could not find a drop-off point.");
            return TaskStepResult.FailTask();
        }

        Vector3 actorPos = context.Actor.transform.position;
        Vector3 dropoffPos = dropoff.transform.position;
        float sqrDist = (dropoffPos - actorPos).sqrMagnitude;

        if (sqrDist > ARRIVAL_THRESHOLD * ARRIVAL_THRESHOLD)
        {
            MovementSystem.MoveTo(context.Actor, dropoffPos);
            return TaskStepResult.StayOnStep();
        }

        // Transfer items once in range
        dropoff.Receive(context.InventoryCount);
        context.InventoryCount = 0;

        return TaskStepResult.AdvanceStep();
    }
}
