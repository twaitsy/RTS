using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/TaskSteps/QueryDropoff")]
public class QueryDropoffStep : TaskStepDefinition
{
    [SerializeField] private string querySucceededEventId;
    [SerializeField] private string queryFailedEventId;

    public override TaskStepResult Execute(TaskContext context)
    {
        if (context == null)
            return TaskStepResult.FailTask("QueryDropoffStep: Context is null.", queryFailedEventId);

        if (context.Actor == null)
            return TaskStepResult.FailTask("QueryDropoffStep: Context.Actor is null.", queryFailedEventId);

        var dropoffLocator = context.Services?.DropoffLocator;
        if (dropoffLocator == null)
            return TaskStepResult.FailTask("QueryDropoffStep: Dropoff locator service is unavailable.", queryFailedEventId);

        var dropoff = dropoffLocator.FindNearest(context.Actor.transform.position, context.CarriedResourceTypeId);
        if (dropoff == null)
            return TaskStepResult.FailTask($"QueryDropoffStep: No drop-off found for actor '{context.Actor.name}'.", queryFailedEventId);

        context.DropoffTarget = dropoff;
        return TaskStepResult.AdvanceStep(querySucceededEventId);
    }
}
