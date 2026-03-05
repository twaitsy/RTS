using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "DataDrivenRTS/TaskSteps/Loop")]
public class LoopStep : TaskStepDefinition
{
    [FormerlySerializedAs("loopToIndex")]
    [SerializeField] private int targetStepIndex;

    public override TaskStepResult Execute(TaskContext context)
    {
        return TaskStepResult.Jump(targetStepIndex);
    }
}
