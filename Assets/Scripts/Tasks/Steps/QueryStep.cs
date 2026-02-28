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
            return TaskStepResult.Complete();
        }

        if (context.Actor == null)
        {
            Debug.LogError("QueryStep: Context.Actor is null.");
            return TaskStepResult.Complete();
        }

        Debug.Log(
            $"QueryStep: START for actor '{context.Actor.name}' " +
            $"requesting resourceType='{resourceType}'."
        );

        var target = ResourceLocator.FindNearest(resourceType, context.Actor.transform.position);

        if (target == null)
        {
            Debug.LogWarning(
                $"QueryStep: NO TARGET found for '{resourceType}' " +
                $"for actor '{context.Actor.name}'."
            );
            return TaskStepResult.Complete(); // Stop task safely
        }

        Debug.Log(
            $"QueryStep: TARGET FOUND for '{resourceType}' " +
            $"for actor '{context.Actor.name}'. Target='{target.name}'."
        );

        context.Target = target;

        Debug.Log(
            $"QueryStep: CONTINUE for actor '{context.Actor.name}'. " +
            $"Target stored in context."
        );

        return TaskStepResult.Continue();
    }
}