using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/TaskSteps/Deliver")]
public class DeliverStep : TaskStepDefinition
{
    public override TaskStepResult Execute(TaskContext context)
    {
        // Find nearest drop-off point
        DropoffReceiver dropoff = DropoffLocator.FindNearest(context.Actor.transform.position);

        if (dropoff == null)
        {
            Debug.LogWarning($"{context.Actor.name} could not find a drop-off point.");
            return TaskStepResult.Complete();
        }

        // Move the actor toward the drop-off
        MovementSystem.MoveTo(context.Actor, dropoff.transform.position);

        // Transfer items (stub logic)
        dropoff.Receive(context.InventoryCount);
        context.InventoryCount = 0;

        return TaskStepResult.Complete();
    }
}