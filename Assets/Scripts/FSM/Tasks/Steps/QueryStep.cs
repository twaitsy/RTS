using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/TaskSteps/Query")]
public class QueryStep : TaskStepDefinition
{
    [SerializeField] private string resourceType;
    [SerializeField] private string querySucceededEventId;
    [SerializeField] private string queryFailedEventId;

    public override TaskStepResult Execute(TaskContext context)
    {
        // --- BASIC CONTEXT CHECKS ---
        if (context == null)
        {
            Debug.LogError("QueryStep: Context is null.");
            return TaskStepResult.FailTask("QueryStep: Context is null.", queryFailedEventId);
        }

        if (context.Actor == null)
        {
            Debug.LogError("QueryStep: Context.Actor is null.");
            return TaskStepResult.FailTask("QueryStep: Context.Actor is null.", queryFailedEventId);
        }

        // --- LOG ACTOR + RESOURCE TYPE ---
        Debug.Log($"[QueryStep] Actor='{context.Actor.name}', resourceType='{resourceType}'");

        // --- LOCATOR CHECK ---
        var resourceLocator = context.Services?.ResourceLocator;
        if (resourceLocator == null)
        {
            Debug.LogError("[QueryStep] Resource locator service is NULL.");
            return TaskStepResult.FailTask("QueryStep: Resource locator service is unavailable.", queryFailedEventId);
        }

        // --- DEBUG: ACTOR POSITION ---
        Vector3 actorPos = context.Actor.transform.position;
        Debug.Log($"[QueryStep] Actor position = {actorPos}");

        // --- QUERY THE LOCATOR ---
        Debug.Log("[QueryStep] Calling ResourceLocator.FindNearest...");
        var target = resourceLocator.FindNearest(resourceType, actorPos);

        // --- DEBUG: RESULT OF QUERY ---
        if (target != null)
        {
            Debug.Log($"[QueryStep] Target FOUND: '{target.name}' at {target.transform.position}");
        }
        else
        {
            Debug.LogWarning(
                $"[QueryStep] No target found for resourceType='{resourceType}' near actor='{context.Actor.name}'.");
        }

        // --- FAILURE CASE ---
        if (target == null)
        {
            return TaskStepResult.FailTask(
                $"QueryStep: No target found for '{resourceType}' for actor '{context.Actor.name}'.",
                queryFailedEventId);
        }

        // --- SUCCESS: ASSIGN TARGET ---
        context.ResourceTarget = target;
        context.DropoffTarget = null;
        context.CarriedResourceTypeId = target.ResourceTypeId;

        Debug.Log($"[QueryStep] SUCCESS. Assigned ResourceTarget='{target.name}', ResourceTypeId='{target.ResourceTypeId}'");

        return TaskStepResult.AdvanceStep(querySucceededEventId);
    }
}
