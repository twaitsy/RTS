using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/TaskSteps/Deliver")]
public class DeliverStep : TaskStepDefinition
{
    private const float ARRIVAL_THRESHOLD = 0.1f;

    [SerializeField] private string deliveredEventId;
    [SerializeField] private string failedEventId;

    public override TaskStepResult Execute(TaskContext context)
    {
        if (context == null || context.Actor == null)
            return TaskStepResult.FailTask("DeliverStep: Context or Actor is null.", failedEventId);

        if (context.Target is not DropoffReceiver dropoff)
            return TaskStepResult.FailTask("DeliverStep: Target is not a drop-off receiver.", failedEventId);

        Vector3 actorPos = context.Actor.transform.position;
        Vector3 dropoffPos = dropoff.transform.position;
        float sqrDist = (dropoffPos - actorPos).sqrMagnitude;

        if (sqrDist > ARRIVAL_THRESHOLD * ARRIVAL_THRESHOLD)
            return TaskStepResult.FailTask("DeliverStep: Actor is not at drop-off target.", failedEventId);

        if (context.InventoryCount <= 0)
            return TaskStepResult.FailTask("DeliverStep: Inventory is empty.", failedEventId);

        dropoff.Receive(context.InventoryCount);
        context.InventoryCount = 0;

        return TaskStepResult.AdvanceStep(deliveredEventId);
    }
}
