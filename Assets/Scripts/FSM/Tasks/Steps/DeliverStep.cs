using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/TaskSteps/Deliver")]
public class DeliverStep : TaskStepDefinition
{
    private const float ARRIVAL_THRESHOLD = 0.1f;

    [SerializeField] private string requiredResourceTypeId;
    [SerializeField] private string deliveredEventId;
    [SerializeField] private string failedEventId;

    public override TaskStepResult Execute(TaskContext context)
    {
        if (context == null || context.Actor == null)
            return TaskStepResult.FailTask("DeliverStep: Context or Actor is null.", failedEventId);

        var dropoff = context.DropoffTarget;
        if (dropoff == null)
            return TaskStepResult.FailTask("DeliverStep: Target is not a drop-off runtime.", failedEventId);

        Vector3 actorPos = context.Actor.transform.position;
        Vector3 dropoffPos = dropoff.transform.position;
        float sqrDist = (dropoffPos - actorPos).sqrMagnitude;

        if (sqrDist > ARRIVAL_THRESHOLD * ARRIVAL_THRESHOLD)
            return TaskStepResult.FailTask("DeliverStep: Actor is not at drop-off target.", failedEventId);

        if (context.InventoryCount <= 0)
            return TaskStepResult.FailTask("DeliverStep: Inventory is empty.", failedEventId);

        var carriedType = context.CarriedResourceTypeId;
        if (string.IsNullOrWhiteSpace(carriedType))
            return TaskStepResult.FailTask("DeliverStep: Carried resource type is missing.", failedEventId);

        if (!string.IsNullOrWhiteSpace(requiredResourceTypeId) &&
            !string.Equals(requiredResourceTypeId, carriedType, System.StringComparison.Ordinal))
        {
            return TaskStepResult.FailTask($"DeliverStep: Required resource '{requiredResourceTypeId}' but carrying '{carriedType}'.", failedEventId);
        }

        int deliveredAmount = 0;
        if (UnitInterpreterRegistry.TryGet(context.RuntimeContext, out var interpreters) &&
            interpreters?.Dropoff != null)
        {
            if (!interpreters.Dropoff.TryDeliver(dropoff, carriedType, context.InventoryCount, out deliveredAmount, out string reason))
                return TaskStepResult.FailTask($"DeliverStep: {reason}", failedEventId);
        }
        else if (!dropoff.TryReceiveDelivery(carriedType, context.InventoryCount, out deliveredAmount, out string fallbackReason))
        {
            return TaskStepResult.FailTask($"DeliverStep: {fallbackReason}", failedEventId);
        }

        context.InventoryCount = Mathf.Max(0, context.InventoryCount - deliveredAmount);
        if (context.InventoryCount > 0)
            return TaskStepResult.FailTask("DeliverStep: Partial delivery left remaining inventory.", failedEventId);

        context.CarriedResourceTypeId = null;
        context.ResourceTarget = null;
        context.DropoffTarget = null;
        context.GatherProgress = 0f;

        return TaskStepResult.AdvanceStep(deliveredEventId);
    }
}
