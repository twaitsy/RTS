using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/TaskSteps/Loop")]
public class LoopStep : TaskStepDefinition
{
    public override TaskStepResult Execute(TaskContext context)
    {
        Debug.Log("LoopStep: Restarting task at step index 0.");
        return TaskStepResult.JumpTo(0);
    }
}
