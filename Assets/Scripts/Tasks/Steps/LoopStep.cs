using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/TaskSteps/Loop")]
public class LoopStep : TaskStepDefinition
{
    public override TaskStepResult Execute(TaskContext context)
    {
        Debug.Log("LoopStep: Restarting task.");
        return TaskStepResult.Jump(0); // restart at QueryStep
    }
}