using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/TaskSteps/Query")]
public class QueryStep : TaskStepDefinition
{
    [SerializeField] private string resourceType;
    // Example: "resource.wood" or "storage.wood"

    public override TaskStepResult Execute(TaskContext context)
    {
        if (context == null)
        {
            Debug.LogError("QueryStep: Context is null.");
            return TaskStepResult.FailTask();
        }

        if (context.Actor == null)
        {
            Debug.LogError("QueryStep: Context.Actor is null.");
            return TaskStepResult.FailTask();
        }

        var target = ResourceLocator.FindNearest(resourceType, context.Actor.transform.position);

        if (target == null)
        {
            Debug.LogWarning(
                $"QueryStep: No target found for '{resourceType}' for actor '{context.Actor.name}'."
            );
            return TaskStepResult.FailTask();
        }

        context.Target = target;
        return TaskStepResult.AdvanceStep();
    }
}
