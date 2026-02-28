using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/TaskSteps/Work")]
public class WorkStep : TaskStepDefinition
{
    [SerializeField] private float duration = 1f;

    public override TaskStepResult Execute(TaskContext context)
    {
        // Initialize timer if needed
        if (context.WorkTimer <= 0f)
            context.WorkTimer = duration;

        // Count down
        context.WorkTimer -= Time.deltaTime;

        // Still working
        if (context.WorkTimer > 0f)
            return TaskStepResult.Continue();

        // Work finished
        context.WorkTimer = 0f;
        context.InventoryCount++;

        return TaskStepResult.Complete();
    }
}