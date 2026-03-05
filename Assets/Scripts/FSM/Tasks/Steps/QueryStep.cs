using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/TaskSteps/Query")]
public class QueryStep : TaskStepDefinition
{
    [SerializeField] private string resourceType;
    [SerializeField] private string querySucceededEventId;
    [SerializeField] private string queryFailedEventId;

    public override TaskStepResult Execute(TaskContext context)
    {
        if (context == null)
            return TaskStepResult.FailTask("QueryStep: Context is null.", queryFailedEventId);

        if (context.Actor == null)
            return TaskStepResult.FailTask("QueryStep: Context.Actor is null.", queryFailedEventId);

        var resourceLocator = context.Services?.ResourceLocator;
        if (resourceLocator == null)
            return TaskStepResult.FailTask("QueryStep: Resource locator service is unavailable.", queryFailedEventId);

        var target = resourceLocator.FindNearest(resourceType, context.Actor.transform.position);

        if (target == null)
        {
            return TaskStepResult.FailTask(
                $"QueryStep: No target found for '{resourceType}' for actor '{context.Actor.name}'.",
                queryFailedEventId);
        }

        context.Target = target;
        return TaskStepResult.AdvanceStep(querySucceededEventId);
    }
}
